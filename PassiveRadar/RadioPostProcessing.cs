using MathNet.Numerics;
using MathNet.Numerics.IntegralTransforms;
using System;
using System.Threading;

namespace PasiveRadar
{
    internal class RadioPostProcessing
    {
        const int bufferMultiplier = 3; // e.g., 2x the frame size
        // --- ORIGINAL FIELDS ---
        int BufferSize;
        uint rate;
        uint decymation;
        short[] tempBuffer; // Used in Decimate

        private float[] dataIQa; // Buffer 1 for processed data
        private float[] dataIQb; // Buffer 2 for processed data
        private bool _useBufferAForNextWrite = true;

        private float[] dataIQ_radioa = null;
        private float[] dataIQ_radiob = null;
        private bool _useBufferAForNextWriteRadio = true;

        private readonly object _dataIQLock = new object();
        private readonly object _dataIQ_radioLock = new object();

        /// decimation
        /// //////////////////////////////////////////////////////////////////////////////////////////////
        private readonly ThreadLocal<Complex32[]> _localBuffer = new ThreadLocal<Complex32[]>(() => null);
        private Complex32[] decimateBuffer;
        private Complex32[] _windowMask;
        const int FFT_BLOCK_SIZE = 1024 * 2;

        //  int current_fill_index = 0; // OLD: Used for initial fill of the shifting buffer

        // --- NEW CIRCULAR BUFFER FIELDS ---
        private Int16[] temp_circular_buffer;
        // private readonly object _tempBufferLock = new object();
        private int _totalCircularBufferSize;
        private int _circularWritePtr = 0;   // Pointer for new data coming from dongle
        private int _circularBlockPtr = 0;   // Pointer for the start of the FrameSize block to read


        public void Init(int _BufferSize, int _radioBufferSize, uint _rate, uint _decymation)
        {
            BufferSize = _BufferSize;
            rate = _rate;
            decymation = _decymation;

            if (decymation == 0) decymation = 1;
            uint decimatedLength = (uint)((_radioBufferSize) / decymation);
            tempBuffer = new short[decimatedLength * 4];

            // --- NEW CIRCULAR BUFFER INITIALIZATION ---

            _totalCircularBufferSize = BufferSize * bufferMultiplier;
            temp_circular_buffer = new Int16[_totalCircularBufferSize];

            // Set block pointer to read position after the first full frame is theoretically written.
            _circularBlockPtr = _totalCircularBufferSize - BufferSize;


            dataIQa = new float[BufferSize];
            dataIQb = new float[BufferSize];

            dataIQ_radioa = new float[BufferSize];
            dataIQ_radiob = new float[BufferSize];

            //init decimation
            PrecomputeWindowMask(_radioBufferSize, "hamming");
        }

        // ----------------------------------------------------------------------
        // 1. MAIN ENTRY POINT
        // ----------------------------------------------------------------------

        public void PostProc(Int16[] data_dongle, ref float[] _dataIQ, ref float[] _dataIQ_radio, bool symmetric = false)
        {
            if (data_dongle != null)
            {
                if (decymation > 1 || symmetric == true)
                    data_dongle = Decimate(data_dongle, tempBuffer, symmetric);


                if (!symmetric)
                    Rotate_180_s16(ref data_dongle);
                else
                    Rotate_90_s16(ref data_dongle);

                // CALLING THE OPTIMIZED CIRCULAR BUFFER IMPLEMENTATION
                SwitchBuffers(data_dongle, ref _dataIQ, ref _dataIQ_radio);
            }
        }

        // ----------------------------------------------------------------------
        // 2. OPTIMIZED CIRCULAR BUFFER IMPLEMENTATION
        // ----------------------------------------------------------------------

        /// <summary>
        /// Optimized SwitchBuffers using a circular buffer logic instead of a shifting buffer.
        /// Eliminates the costly mass 'shift left' operation.
        /// </summary>
        /// <summary>
        /// Optimized SwitchBuffers using a circular buffer logic instead of a shifting buffer.
        /// Eliminates the costly mass 'shift left' operation.
        /// UWAGA: Wymaga pól: temp_circular_buffer, _circularWritePtr, _circularBlockPtr, _totalCircularBufferSize, 
        /// oraz funkcji pomocniczych: CopyDataToBuffer i ExtractBlockAndConvert.
        /// </summary>
        public void SwitchBuffers(Int16[] data_dongle, ref float[] _dataIQ_ref, ref float[] _dataIQ_radio_ref)
        {
            int new_data_length = data_dongle.Length;

            // Używamy bloku lock tylko dla operacji na wskaźnikach bufora kołowego
            //lock (_tempBufferLock)
            //{
            // A. ZAPISZ NOWE DANE (D_new) do bufora kołowego (obsługa zawijania)
            CopyDataToBuffer(data_dongle, temp_circular_buffer, _circularWritePtr, new_data_length, _totalCircularBufferSize);

            // 1. Zaktualizuj Wskaźnik Zapisu
            _circularWritePtr = (_circularWritePtr + new_data_length) % _totalCircularBufferSize;

            // B. PRZESUŃ WSKAŹNIK BLOKU DO ODCZYTU (Eliminuje masowe Memory Copy!)
            // Wystarczy przesunąć wskaźnik startowy ramki o długość nowo wpisanych danych.
            _circularBlockPtr = (_circularBlockPtr + new_data_length) % _totalCircularBufferSize;
            //}

            // ********************************************************************
            // C. RADAR DATA (I/Q) - Odczyt i Konwersja (logika double-buffering)
            // ********************************************************************

            float[] dataIQtarget = _useBufferAForNextWrite ? dataIQa : dataIQb;

            // Wydobądź CAŁĄ RAMKĘ (BufferSize) z bufora kołowego 
            // i przekonwertuj short na float (obsługa zawijania wewnątrz funkcji ExtractBlockAndConvert).
            ExtractBlockAndConvert(temp_circular_buffer, _circularBlockPtr, _totalCircularBufferSize, BufferSize, dataIQtarget);

            // Zamień aktywny bufor dla wątku przetwarzającego (lock chroni zmienne dataIQa/dataIQb)
            lock (_dataIQLock)
            {
                _dataIQ_ref = dataIQtarget;
                _useBufferAForNextWrite = !_useBufferAForNextWrite;
            }

            // ********************************************************************
            // D. RADIO DATA (RAW) - Bezpośrednie kopiowanie z D_new (oryginalna logika)
            // ********************************************************************

            float[] dataIQradiotarget = _useBufferAForNextWriteRadio ? dataIQ_radioa : dataIQ_radiob;

            int lenght = Math.Min(data_dongle.Length, dataIQ_radioa.Length);

            // Pętla unsafe dla szybkiego kopiowania/konwersji short do float
            unsafe
            {
                fixed (Int16* pSource = data_dongle)
                fixed (float* pDest = dataIQradiotarget)
                {
                    short* pS = pSource;
                    float* pD = pDest;
                    for (int i = 0; i < lenght; i++)
                    {
                        *pD++ = *pS++;
                    }
                }
            }

            // Zamień aktywny bufor dla wątku radiowego
            lock (_dataIQ_radioLock)
            {
                _dataIQ_radio_ref = dataIQradiotarget;
                _useBufferAForNextWriteRadio = !_useBufferAForNextWriteRadio;
            }
        }        // ----------------------------------------------------------------------
        // 3. HELPER FUNCTIONS FOR CIRCULAR BUFFER (UNSAFE)
        // ----------------------------------------------------------------------

        private void CopyDataToBuffer(short[] source, short[] destination, int startPtr, int dataLength, int bufferLength)
        {
            // Copies data (D_new) INTO the circular buffer, handling wrap-around.
            int first_part_len = Math.Min(dataLength, bufferLength - startPtr);

            unsafe
            {
                fixed (short* pSource = source)
                fixed (short* pDest = destination)
                {
                    // Part 1: Copy from startPtr to the physical end of the array
                    Buffer.MemoryCopy(pSource, pDest + startPtr,
                                      (bufferLength - startPtr) * sizeof(short), first_part_len * sizeof(short));

                    // Part 2: Wrap-around copy (if needed)
                    int second_part_len = dataLength - first_part_len;
                    if (second_part_len > 0)
                    {
                        Buffer.MemoryCopy(pSource + first_part_len, pDest,
                                          bufferLength * sizeof(short), second_part_len * sizeof(short));
                    }
                }
            }
        }

        private void ExtractBlockAndConvert(short[] source, int readStart, int bufferLength, int frameSize, float[] destination)
        {
            int first_part_len = Math.Min(frameSize, bufferLength - readStart);
            int second_part_len = frameSize - first_part_len;

            unsafe
            {
                fixed (short* pSource = source)
                fixed (float* pDest = destination)
                {
                    // --- CZĘŚĆ 1: Kopiowanie i Konwersja (od readStart do końca tablicy) ---
                    short* pS1 = pSource + readStart;
                    float* pD = pDest;

                    for (int i = 0; i < first_part_len; i++)
                    {
                        *pD++ = (float)*pS1++;
                    }

                    // --- CZĘŚĆ 2: Zawinięcie i Konwersja (od początku tablicy, jeśli jest) ---
                    if (second_part_len > 0)
                    {
                        short* pS2 = pSource; // Źródło od pSource[0]
                        for (int i = 0; i < second_part_len; i++)
                        {
                            *pD++ = (float)*pS2++;
                        }
                    }
                }
            }
        }
        // ----------------------------------------------------------------------
        // 4. ORIGINAL DEPRECATED/REFERENCE FUNCTION (SWITCHBUFFERS)
        // ----------------------------------------------------------------------


        // ----------------------------------------------------------------------
        // 5. ORIGINAL RADIO PROCESSING/HELPER FUNCTIONS
        // ----------------------------------------------------------------------


        public short[] Decimate(short[] inputBuffer, short[] outputBuffer, bool symmetric)
        {
            Complex32[] workingBuffer;
            // requiredLength to długość danych I/Q (połowa długości short[]), używana do alokacji Complex32
            int requiredLength = inputBuffer.Length / 2;

            // 1. Inicjalizacja bufora roboczego (workingBuffer)
            if (decimateBuffer == null || decimateBuffer.Length < requiredLength)
            {
                decimateBuffer = new Complex32[requiredLength];
            }
            workingBuffer = decimateBuffer;

            // 2. KROK 2 (PARALLEL I/Q CONVERSION) - USUNIĘTY. Przeniesiony do pętli Parallel.For.
            // workingBuffer jest używany tylko jako bufor docelowy dla finalnego wyniku IFFT.

            // 3. Sprawdzenie długości FFT
            int totalSamples = workingBuffer.Length;
            if (totalSamples % FFT_BLOCK_SIZE != 0)
            {
                throw new InvalidOperationException("Całkowita długość próbek musi być podzielna przez rozmiar bloku FFT.");
            }
            int numChunks = totalSamples / FFT_BLOCK_SIZE;

            // 4. PRZETWARZANIE RÓWNOLEGŁE
            System.Threading.Tasks.Parallel.For(0, numChunks, chunkIndex =>
            {
                int start = chunkIndex * FFT_BLOCK_SIZE;
                const int chunkSize = FFT_BLOCK_SIZE;

                // Krok 4a: Lokalne buforowanie (Recycling)
                Complex32[] localSamples = _localBuffer.Value;
                if (localSamples == null || localSamples.Length < chunkSize)
                {
                    localSamples = new Complex32[chunkSize];
                    _localBuffer.Value = localSamples;
                }

                // Krok 4b: I/Q Conversion i Load do localSamples (POPRAWIONA ZASADA WSKAŹNIKÓW)
                unsafe
                {
                    fixed (short* pInStart = inputBuffer)
                    fixed (Complex32* pLocal = localSamples)
                    {
                        // POPRAWKA: Wskaźnik w tablicy short[] musi być podwójnie przesunięty
                        short* pCurrentIn = pInStart + start * 2;

                        // pLocal[n] to Complex32, pCurrentIn++ to short
                        for (int n = 0; n < chunkSize; n++)
                        {
                            short I = *pCurrentIn++;
                            short Q = *pCurrentIn++;
                            pLocal[n] = new Complex32(I, Q);
                        }
                    }
                }

                // Krok 4c: FORWARD FFT
                Fourier.Forward(localSamples, FourierOptions.Matlab);

                // Krok 4d: MASKOWANIE I FILTROWANIE (ZOPTYMALIZOWANE - usunięto warunek "if(symmetric)" z pętli)
                unsafe
                {
                    fixed (Complex32* pLocal = localSamples)
                    fixed (Complex32* pMask = _windowMask)
                    {
                        // Maska startuje od pozycji start (Complex32 index)
                        Complex32* pCurrentMask = pMask + start;
                        if (start + localSamples.Length > _windowMask.Length)
                        {
                            return;  
                        }

                        if (symmetric)
                        {
                            int halfLength = localSamples.Length / 2;
                            // Pętla zoptymalizowana dla trybu SYMETRYCZNEGO
                            for (int i = 0; i < localSamples.Length; i++)
                            {
                                // Mnożenie przez maskę
                                pLocal[i] *= pCurrentMask[i];

                                if (i > halfLength)
                                {
                                    pLocal[i] = Complex32.Zero; // Zerowanie górnej połowy
                                }
                                else if (i > 0)
                                {
                                    pLocal[i] *= 2f; // Skalowanie dolnej połowy (z pominięciem DC)
                                }
                            }
                        }
                        else
                        {
                            // Pętla zoptymalizowana dla trybu ASYMETRYCZNEGO
                            for (int i = 0; i < localSamples.Length; i++)
                            {
                                pLocal[i] *= pCurrentMask[i];
                            }
                        }
                    }
                }

                // Krok 4e: INVERSE FFT
                Fourier.Inverse(localSamples, FourierOptions.Matlab);

                // Krok 4f: Kopiowanie wyniku z powrotem do workingBuffer
                Array.Copy(localSamples, 0, workingBuffer, start, localSamples.Length);
            });

            // 5. DECIMATION (UNSAFE) - Bez zmian
            uint decimatedLength = (uint)(workingBuffer.Length / decymation);

            // Sprawdzamy i ewentualnie alokujemy nowy bufor wyjściowy
            if (outputBuffer == null || outputBuffer.Length != decimatedLength * 2)
                outputBuffer = new short[decimatedLength * 2];

            unsafe
            {
                fixed (Complex32* pSamples = workingBuffer)
                fixed (short* pOutput = outputBuffer)
                {
                    for (int i = 0; i < decimatedLength; i++)
                    {
                        long df = i * decymation;
                        pOutput[i * 2] = (short)pSamples[df].Real;
                        pOutput[i * 2 + 1] = (short)pSamples[df].Imaginary;
                    }
                }
            }

            return outputBuffer;
        }

        private void PrecomputeWindowMask(int fftLength, string windowType = "hamming")
        {
       
            _windowMask = new Complex32[fftLength];
            
            // Prawidłowa częstotliwość próbkowania: Fs = 2 * rate
            double fs = 2.0 * rate;

            // Prawidłowa częstotliwość odcięcia dla decymacji D: f_cut = rate / D
            double desired_cutoff_freq = rate / decymation;

            // Szerokość przejścia (np. 5% nowej częstotliwości Nyquista)
            double transition_width = 0.05 * desired_cutoff_freq;

            // Obliczenie indeksów granicznych (od 0 do fftLength)
            int passband_index = (int)(desired_cutoff_freq / fs * fftLength); // Indeks cięcia (f_cut)
            int stopband_start_index = (int)((desired_cutoff_freq + transition_width) / fs * fftLength); // Początek stopband

            // Ustawienie wartości maski dla pierwszej połowy FFT (częstotliwości dodatnie)
            for (int i = 0; i < fftLength; i++)
            {
                double window_value = 0f; // Domyślnie zerowe

                // --- 1. PASMO PRZEPUSTOWE (Passband: 0 do f_cut) ---
                if (i < passband_index)
                {
                    window_value = 1f;
                }
                // --- 2. PASMO PRZEJŚCIA (Transition Band: f_cut do f_stop) ---
                else if (i >= passband_index && i < stopband_start_index)
                {
                    // Normalizacja n od 0 (przy f_cut) do 1 (przy f_stop)
                    double n = (double)(i - passband_index) / (stopband_start_index - passband_index);

                    // Okna przechodzą od 1 (n=0) do 0 (n=1)
                    switch (windowType.ToLower())
                    {
                        case "hamming":
                            // Cosinus przechodzi od 1 (n=0) do -1 (n=1). Skalujemy do 1 do 0.
                            window_value = 0.5 * (1 + Math.Cos(Math.PI * n));
                            break;
                        case "hanning":
                            window_value = 0.5 * (1 + Math.Cos(Math.PI * n));
                            break;
                        case "blackman":
                            window_value = 0.42 + 0.5 * Math.Cos(Math.PI * n) + 0.08 * Math.Cos(2 * Math.PI * n); // Blackman odwrócony
                            break;
                        default:
                            window_value = 1.0 - n; // Prosta interpolacja liniowa, gdy okno nieznane
                            break;
                    }
                }
                // --- 3. PASMO ZAPOROWE (Stopband: powyżej f_stop) ---
                else if (i >= stopband_start_index)
                {
                    window_value = 0f;
                }

                _windowMask[i] = new Complex32((float)window_value, 0f);
            }

            // --- KOREKTA LUSTRZANA (DLA CZĘSTOTLIWOŚCI UJEMNYCH) ---

            // W FFT (0 do N-1), dolnoprzepustowy musi być lustrzany wokół N/2.
            // Pasmo przechodzi od 0 do f_cut i od f_cut_mirror do N-1. Środek musi być zerowy.

            int mirror_stopband_index = fftLength - stopband_start_index;
            int mirror_passband_index = fftLength - passband_index;

            // Zerujemy środek tablicy (aliasy)
            for (int i = stopband_start_index; i < mirror_stopband_index; i++)
            {
                _windowMask[i] = Complex32.Zero; // Zapewniamy zero dla pasma zapory
            }

            // Kopiujemy lustrzaną kopię (Pasmo Przepustowe i Przejścia) na drugą stronę
            for (int i = mirror_stopband_index; i < fftLength; i++)
            {
                int j = fftLength - i; // Indeks lustrzany
                _windowMask[i] = _windowMask[j];
            }

            // Sprawdzamy punkt Nyquista (N/2) – musi być symetryczny
            if (fftLength % 2 == 0)
            {
                int nyquist_index = fftLength / 2;
                // W punkcie Nyquista wartość powinna być lustrzana lub zero (dla LPF)
                if (nyquist_index >= stopband_start_index && nyquist_index < mirror_stopband_index)
                {
                    _windowMask[nyquist_index] = Complex32.Zero;
                }
            }
            
        }


        //Rotation 90deg
        public static void Rotate_90_s16(ref short[] samples)
        {
            // Applies a cyclic rotation: 1, j, -1, -j
            unsafe
            {
                fixed (short* pBuf = samples)
                {
                    for (int i = 0; i < samples.Length / 2; i++)
                    {
                        short* pCurrent = pBuf + (i * 2);

                        short I = *pCurrent;
                        short Q = *(pCurrent + 1);

                        switch (i % 4)
                        {
                            case 0:
                                // Multiplier: 1 (I, Q) -> (I, Q)
                                break;
                            case 1:
                                // Multiplier: j (I, Q) -> (-Q, I)
                                *pCurrent = (short)-Q;
                                *(pCurrent + 1) = I;
                                break;
                            case 2:
                                // Multiplier: -1 (I, Q) -> (-I, -Q)
                                *pCurrent = (short)-I;
                                *(pCurrent + 1) = (short)-Q;
                                break;
                            case 3:
                                // Multiplier: -j (I, Q) -> (Q, -I)
                                *pCurrent = Q;
                                *(pCurrent + 1) = (short)-I;
                                break;
                        }
                    }
                }
            }
        }

        static void Rotate_180_s16(ref Int16[] buf)
        {
            // Applies a cyclic negation: 1, 1, -1, -1, 1, 1, -1, -1 for the complex samples (I, Q, I, Q, ...)
            // Resulting pattern is: I, Q, -I, -Q, I, Q, -I, -Q (in 4 complex sample blocks, or 8 shorts)
            unsafe
            {
                fixed (short* pBuf = buf)
                {
                    for (short* pCurrent = pBuf; pCurrent < pBuf + buf.Length; pCurrent += 8)
                    {
                        // Negate samples 2, 3, 6, 7 (indices starting from 0 within the 8-short block)
                        // This corresponds to the 3rd, 4th, 7th, and 8th short, which are 
                        // the second I/Q pair and the fourth I/Q pair in the block.
                        pCurrent[2] = (short)-pCurrent[2];
                        pCurrent[3] = (short)-pCurrent[3];
                        pCurrent[6] = (short)-pCurrent[6];
                        pCurrent[7] = (short)-pCurrent[7];
                    }
                }
            }
        }
    }
}