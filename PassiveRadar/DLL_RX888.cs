using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace PasiveRadar
{
    public class DLL_RX888
    {
        public int AttenuationStepsCount;
        public static float[] attenuation_table;
        public int GainStepsCount;
        public static float[] if_gain_table;
        private int FreqCorrection = 0;

        private static int currentFilledBytes = 0; // <-- Tutaj jest zdefiniowane
        const int MAX_ERROR_MESSAGE_SIZE = 256;
        public static StringBuilder openErrorMessage = new StringBuilder(MAX_ERROR_MESSAGE_SIZE);

        ~DLL_RX888()
        {
            _newDataEvent.Close();
            _newDataEvent.Dispose();
        }

        // Nowa definicja delegata, zgodna z SddcWrapper.h po modyfikacji
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void SddcStreamCallback(uint data_size, IntPtr data_ptr, IntPtr user_context);

        // Wszystkie te pola również muszą być publiczne lub mieć publiczne właściwości/metody dostępowe
        private static IntPtr _sddcDeviceInstance = IntPtr.Zero;
        private static SddcStreamCallback _dataCallbackInstance;
        public static ConcurrentQueue<byte[]> _dataQueue = new ConcurrentQueue<byte[]>();
        public static AutoResetEvent _newDataEvent = new AutoResetEvent(false);

        private static volatile bool _isRunning = false;

        private static long _totalBytesReceived = 0;
        private static long _totalCallbacksReceived = 0;
        //private static Stopwatch _stopwatch = new Stopwatch(); // Używamy System.Diagnostics.Stopwatch

        // --- P/Invoke Declarations (muszą być publiczne) ---

        [DllImport("RX_888MK2.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr Sddc_CreateDevice();//v

        [DllImport("RX_888MK2.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void Sddc_DestroyDevice(IntPtr device);//v

        [DllImport("RX_888MK2.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool Sddc_Open(IntPtr device);//v

        [DllImport("RX_888MK2.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void Sddc_Close(IntPtr device);//v

        [DllImport("RX_888MK2.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool Sddc_IsOpen(IntPtr device);//v

        [DllImport("RX_888MK2.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool Sddc_SetSampleRate(IntPtr device, uint sample_rate);//v

        [DllImport("RX_888MK2.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern double Sddc_GetSampleRate(IntPtr device);//v


        [DllImport("RX_888MK2.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool Sddc_SetRfMode(IntPtr device, int mode);//v

        [DllImport("RX_888MK2.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool Sddc_SetTunerFrequency(IntPtr device, ulong frequency_hz);//v

        [DllImport("RX_888MK2.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern double Sddc_GetTunerFrequency(IntPtr device);//v

        [DllImport("RX_888MK2.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool Sddc_SetTunerRfAttenuation(IntPtr device, int attenuation_index);//v

        [DllImport("RX_888MK2.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Sddc_GetRfAttenuationSteps(IntPtr device, out IntPtr steps);//v

        [DllImport("RX_888MK2.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool Sddc_SetTunerIfGain(IntPtr device, int gain_index);//v

        [DllImport("RX_888MK2.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int Sddc_GetIfGainSteps(IntPtr device, out IntPtr steps);//v

        [DllImport("RX_888MK2.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int Sddc_GetTunerIfGain(IntPtr device);//v

        [DllImport("RX_888MK2.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool Sddc_StartStreaming(IntPtr device, SddcStreamCallback callback, IntPtr user_context);//v

        [DllImport("RX_888MK2.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool Sddc_StopStreaming(IntPtr device);//v

        [DllImport("RX_888MK2.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool Sddc_HandleEvents(IntPtr device);//v


        [DllImport("RX_888MK2.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr Sddc_GetName(IntPtr device);//v

        [DllImport("RX_888MK2.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool Sddc_SetAdcDither(IntPtr device, bool dither);//v

        [DllImport("RX_888MK2.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool Sddc_StartSimpleStreaming(IntPtr device, IntPtr buffer, int size, int action);

        [DllImport("RX_888MK2.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool Sddc_SetAdcPga(IntPtr device, bool pga);

        [DllImport("RX_888MK2.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool Sddc_SetUptRand(IntPtr device, bool rand);


        //bias
        [DllImport("RX_888MK2.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool Sddc_SetVhfBias(IntPtr device, bool bias);

        [DllImport("RX_888MK2.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool Sddc_SetHfBias(IntPtr device, bool bias);


        [DllImport("RX_888MK2.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool Sddc_GetVhfBias(IntPtr device);


        [DllImport("RX_888MK2.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool Sddc_GetHfBias(IntPtr device);


        ///
        [DllImport("RX_888MK2.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void Sddc_GetLastError(
                IntPtr device,
                int buffer_size, // Odpowiada size_t buffer_size w C++
                [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder buffer // Bufor na komunikat błędu
            );//v




        // --- Callback Implementation (musi być publiczna statyczna) ---
        //tu tylko kopiujemy dane
        private static void MyDataCallback(uint data_size, IntPtr data_ptr, IntPtr user_context)
        {
            _totalCallbacksReceived++;
            // _totalBytesReceived += data_size;

            if (data_size > 0 && data_ptr != IntPtr.Zero)
            {
                byte[] data = new byte[data_size];
                Marshal.Copy(data_ptr, data, 0, (int)data_size);// dane 8 bitowe

                _dataQueue.Enqueue(data);
                _newDataEvent.Set();
            }
        }

        private static IntPtr GetSddcDeviceInstance()
        {
            return _sddcDeviceInstance;
        }


        private static SddcStreamCallback GetDataCallbackInstance()
        {
            return _dataCallbackInstance;
        }
        // --- Data Processing Thread (musi być publiczna statyczna) ---
        public static void ProcessDataLoop(ref byte[] temp_buffer)
        {

            if (_isRunning || _dataQueue.Count > 0) // Kontynuuj, dopóki jest uruchomiony LUB są dane do przetworzenia
            {

                if (_dataQueue.TryDequeue(out byte[] inputBufferFromQueue))
                {
                    // Przetwórz dane

                    int newChunkLength = inputBufferFromQueue.Length;

                    // --- Faza 1: Początkowe wypełnianie bufora ---
                    // Ta faza trwa, dopóki 'temp_buffer' nie zostanie całkowicie wypełniony po raz pierwszy.
                    if (currentFilledBytes < temp_buffer.Length)
                    {
                        // Oblicz, ile bajtów z nowego chunka zmieści się w pozostałej, pustej części bufora.
                        int remainingSpaceInBuffer = temp_buffer.Length - currentFilledBytes;
                        int bytesToCopyFromNewChunk = Math.Min(newChunkLength, remainingSpaceInBuffer);

                        // Kopiuj część nowego chunka do aktualnej pozycji w buforze.
                        Array.Copy(inputBufferFromQueue, 0, temp_buffer, currentFilledBytes, bytesToCopyFromNewChunk);
                        currentFilledBytes += bytesToCopyFromNewChunk;

                        //Console.WriteLine($"  Faza początkowego wypełniania: skopiowano {bytesToCopyFromNewChunk} bajtów. Aktualnie wypełniono: {currentFilledBytes}/{temp_buffer.Length} bajtów.");

                        // Jeśli nowy chunk był większy niż pozostałe miejsce w buforze,
                        // oznacza to, że bufor jest teraz pełny, a część nowego chunka pozostała.
                        // Tę pozostałą część musimy przetworzyć w fazie "przesuwania".
                        if (newChunkLength > bytesToCopyFromNewChunk)
                        {
                            //Console.WriteLine($"  Bufor pełny po początkowym wypełnianiu. Pozostało {newChunkLength - bytesToCopyFromNewChunk} bajtów z nowego chunka do przetworzenia przez przesuwanie.");
                            // Tworzymy nową tablicę z pozostałych danych, aby przekazać ją do funkcji przesuwającej.
                            byte[] remainingNewData = new byte[newChunkLength - bytesToCopyFromNewChunk];
                            Array.Copy(inputBufferFromQueue, bytesToCopyFromNewChunk, remainingNewData, 0, remainingNewData.Length);

                            // Przejście do fazy przesuwania z pozostałymi danymi.
                            ProcessNewChunkWithShifting(ref temp_buffer, remainingNewData);
                        }
                    }
                    // --- Faza 2: Ciągłe przesuwanie (bufor jest już pełny) ---
                    else
                    {
                        //Console.WriteLine("  Faza ciągłego przesuwania (bufor jest już pełny).");
                        // Bufor jest już pełny. Nowe dane będą wypychać najstarsze.
                        ProcessNewChunkWithShifting(ref temp_buffer, inputBufferFromQueue);
                    }
                }
                else
                {
                    // Jeśli kolejka jest pusta i _isRunning jest true, poczekaj na nowe dane
                    if (_isRunning)
                    {
                        // Użyj timeoutu, aby wątek mógł regularnie sprawdzać _isRunning
                        _newDataEvent.WaitOne(5000); // Czekaj na sygnał, max 100 ms
                    }
                    //Console.WriteLine("Kolejka jest pusta. Brak danych do przetworzenia.");
                }
            }
        }

        //Console.WriteLine("ProcessDataThread stopped.");


        private static void ProcessNewChunkWithShifting(ref byte[] temp_buffer, byte[] chunkToAppend)
        {
            int chunkLength = chunkToAppend.Length;

            // Ilość danych do przesunięcia (czyli ile najstarszych bajtów zostanie wypchniętych)
            // jest równa długości nowego, przychodzącego chunka.
            int shiftAmount = chunkLength;

            // Oblicz, ile danych pozostanie w buforze po przesunięciu.
            int remainingDataInOldBuffer = temp_buffer.Length - shiftAmount;

            if (remainingDataInOldBuffer <= 0)
            {
                // Nowy chunk jest większy lub równy pojemności bufora.
                // Oznacza to, że nowy chunk całkowicie nadpisze stary bufor.
                // Kopiujemy tylko ostatnie 'temp_buffer.Length' bajtów z nowego chunka,
                // aby wypełnić cały bufor. Starsze części *nowego chunka* zostaną odrzucone, jeśli jest za duży.
                //Console.WriteLine($"    Nowy chunk ({chunkLength} bajtów) całkowicie nadpisuje bufor ({temp_buffer.Length} bajtów).");
                Array.Copy(chunkToAppend, chunkLength - temp_buffer.Length, temp_buffer, 0, temp_buffer.Length);
                currentFilledBytes = temp_buffer.Length; // Bufor jest pełny
            }
            else
            {
                // Nowy chunk jest mniejszy niż pojemność bufora.
                // Przesuwamy istniejące dane w buforze w lewo o 'shiftAmount' bajtów.
                // Kopiujemy dane z pozycji 'shiftAmount' do pozycji '0' na długość 'remainingDataInOldBuffer'.
                //Console.WriteLine($"    Przesuwanie danych: {remainingDataInOldBuffer} bajtów w lewo o {shiftAmount} pozycji.");
                Array.Copy(temp_buffer, shiftAmount, temp_buffer, 0, remainingDataInOldBuffer);

                // Dodajemy nowy chunk danych na koniec bufora.
                // Kopiujemy cały 'chunkToAppend' od jego początku (0) do końca 'temp_buffer'
                // (czyli na pozycję 'remainingDataInOldBuffer').
                //Console.WriteLine($"    Dodawanie nowego chunka ({chunkLength} bajtów) na koniec bufora na pozycji {remainingDataInOldBuffer}.");
                Array.Copy(chunkToAppend, 0, temp_buffer, remainingDataInOldBuffer, chunkLength);

                currentFilledBytes = temp_buffer.Length; // Bufor pozostaje pełny
            }
        }



        //Rozpoczyna thread zapisu do buforu oraz startuje RX888 i jego buforowania asynchronicznie (wszystko)
        public bool Open()
        {
            if (_sddcDeviceInstance != IntPtr.Zero)
                if (Sddc_IsOpen(_sddcDeviceInstance))
                {
                    Sddc_Close(_sddcDeviceInstance);
                    Sddc_DestroyDevice(_sddcDeviceInstance);
                    _sddcDeviceInstance = IntPtr.Zero;
                }
            // Console.WriteLine($"Próba otwarcia urządzenia  ");
            //if (_sddcDeviceInstance == IntPtr.Zero)
            _sddcDeviceInstance = Sddc_CreateDevice();

            if (_sddcDeviceInstance == IntPtr.Zero)
            {
                //Sddc_GetLastError(_sddcDeviceInstance, openErrorMessage.Capacity, openErrorMessage);
                MessageBox.Show($"BŁĄD: Nie udało się utworzyć instancji SddcDevice. {openErrorMessage.ToString()}", "Błąd Operacji Init", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //Environment.Exit(1);
                return false;
            }

            if (_sddcDeviceInstance == IntPtr.Zero)
            {
                //Sddc_GetLastError(_sddcDeviceInstance, openErrorMessage.Capacity, openErrorMessage);
                //MessageBox.Show($"BŁĄD: Nie udało się utworzyć instancji SddcDevice. {openErrorMessage.ToString()}", "Błąd Operacji Start", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //Environment.Exit(1);
                return false;
            }

            if (!Sddc_Open(_sddcDeviceInstance))
                return false;

            if (_dataCallbackInstance == null)
                _dataCallbackInstance = new SddcStreamCallback(MyDataCallback);

            return true;
        }
        public static bool Start()
        {
            if (_sddcDeviceInstance == IntPtr.Zero)
                return false;

            if (!Sddc_StartStreaming(GetSddcDeviceInstance(), GetDataCallbackInstance(), GetSddcDeviceInstance())) // Używamy instancji urządzenia jako kontekstu
            {
                Sddc_GetLastError(_sddcDeviceInstance, openErrorMessage.Capacity, openErrorMessage);
                MessageBox.Show($"BŁĄD STARTU STRUMIENIOWANIA:  {openErrorMessage.ToString()}", "Błąd Operacji Start", MessageBoxButtons.OK, MessageBoxIcon.Error);

                //Close();
                return false;
            }
            //Console.WriteLine("Strumieniowanie rozpoczęte.");
            _isRunning = true;

            return true;
        }


        public static bool Stop()
        {
            _isRunning = false;
            _newDataEvent.Set();

            if (_sddcDeviceInstance != IntPtr.Zero)
            {
                if (Sddc_IsOpen(_sddcDeviceInstance))
                {

                    if (!Sddc_StopStreaming(_sddcDeviceInstance))
                    {
                        //Sddc_GetLastError(_sddcDeviceInstance, openErrorMessage.Capacity, openErrorMessage);
                        MessageBox.Show($"Nie udało się otworzyć urządzenia SDDC. {openErrorMessage.ToString()}", "Błąd Operacji stop", MessageBoxButtons.OK, MessageBoxIcon.Error);

                        // Close();
                        return false;
                    }
                    else
                        return true;
                }

            }
            return false;
        }
        public static void Close()
        {
            if (_sddcDeviceInstance != IntPtr.Zero)
            {
                Console.WriteLine("Zamykanie urządzenia...");
                Sddc_Close(_sddcDeviceInstance);
                //Console.WriteLine("Niszczenie instancji urządzenia...");
                Sddc_DestroyDevice(_sddcDeviceInstance);
                _sddcDeviceInstance = IntPtr.Zero;

                Thread.Sleep(100);


            }
            //Console.WriteLine("Czyszczenie zakończone.");
        }

        public static long GetTotalBytesReceived()
        {
            return _totalBytesReceived;
        }

        public static long GetTotalCallbacksReceived()
        {
            return _totalCallbacksReceived;
        }


        public bool SimpleRead(ref IntPtr buffer, int len, int action)
        {
            if (GetSddcDeviceInstance() == IntPtr.Zero)
                return false;
 

            return Sddc_StartSimpleStreaming(GetSddcDeviceInstance(), buffer, len, action);
      
        }



        public bool set_sample_rate(uint sampleRate)
        {
            if (GetSddcDeviceInstance() == IntPtr.Zero) return false;
            // Console.WriteLine($"Ustawianie częstotliwości próbkowania: {sampleRate} Hz");
            if (!Sddc_SetSampleRate(GetSddcDeviceInstance(), sampleRate))
            {
                // Sddc_GetLastError(_sddcDeviceInstance, openErrorMessage.Capacity, openErrorMessage);
               // MessageBox.Show($"BŁĄD USTAWIENIA CZĘSTOTLIWOŚCI PRÓBKOWANIA: {openErrorMessage.ToString()}", "Błąd Operacji ustawienia czestotliwosci", MessageBoxButtons.OK, MessageBoxIcon.Error);

                return false;
            }
            return true;
        }

        public double get_sample_rate()
        {
            if (GetSddcDeviceInstance() == IntPtr.Zero) return -1;
            return Sddc_GetSampleRate(GetSddcDeviceInstance());

        }


        // HFMODE - 1;
        // VHFMODE - 2;
        public bool set_RF_mode(int mode)
        {
            if (GetSddcDeviceInstance() == IntPtr.Zero) return false;
            IntPtr pt = GetSddcDeviceInstance();

            if (pt != IntPtr.Zero)
            {
                // Console.WriteLine($"Ustawianie trybu RF: {mode}");
                if (!Sddc_SetRfMode(pt, mode))
                {
                    //  Sddc_GetLastError(_sddcDeviceInstance, openErrorMessage.Capacity, openErrorMessage);
                   // MessageBox.Show($"BŁĄD USTAWIENIA TRYBU RF: {openErrorMessage.ToString()}", "Błąd Operacji ustawienia czestotliwosci", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    //Close();
                    return false;
                }
                return true;
            }
            return false;
        }


        public string get_name()
        {
            if (GetSddcDeviceInstance() == IntPtr.Zero)
                return "";
            IntPtr namePtr = Sddc_GetName(GetSddcDeviceInstance());
            if (namePtr != IntPtr.Zero)
            {
                // Skopiuj ciąg znaków z niezarządzanej pamięci do zarządzanego ciągu C#
                string name = Marshal.PtrToStringAnsi(namePtr); // Lub PtrToStringUTF8, jeśli DLL używa UTF-8
                return name;
            }
            return "No name";
        }

        public bool set_frequency(ulong frequencyHz)
        {

            long Freq = (long)frequencyHz;
            //jeśli FreqCorrection
            if ((long)frequencyHz > (long)FreqCorrection)
                Freq = (long)frequencyHz + (long)FreqCorrection;
            // Console.WriteLine($"Ustawianie częstotliwości tunera: {Freq} Hz");

            if (GetSddcDeviceInstance() != IntPtr.Zero)
                if (!Sddc_SetTunerFrequency(GetSddcDeviceInstance(), (ulong)Freq))
                {
                    //Sddc_GetLastError(_sddcDeviceInstance, openErrorMessage.Capacity, openErrorMessage);
                   // MessageBox.Show($"BŁĄD USTAWIENIA CZĘSTOTLIWOŚCI TUNERA: {openErrorMessage.ToString()}", "Błąd Operacji ustawienia czestotliwosci", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    //Close();
                    return false;
                }
            return true;
        }

        public double get_frequency()
        {
            long Freq = 0;
            if (GetSddcDeviceInstance() != IntPtr.Zero)
            {
                double frequencyHz = Sddc_GetTunerFrequency(GetSddcDeviceInstance());
                Freq = (long)frequencyHz;
                //jeśli FreqCorrection
                if ((long)frequencyHz > (long)FreqCorrection)
                    Freq = -(long)FreqCorrection;
                // Console.WriteLine($"Odczytana częstotliwości tunera: {Freq} Hz");
            }
            return Freq;

        }

        public int get_attenuation_steps()
        {
            if (GetSddcDeviceInstance() != IntPtr.Zero)
            {
                IntPtr rfStepsPtr;
                AttenuationStepsCount = Sddc_GetRfAttenuationSteps(GetSddcDeviceInstance(), out rfStepsPtr);
                if (AttenuationStepsCount > 0 && rfStepsPtr != IntPtr.Zero)
                {
                    attenuation_table = new float[AttenuationStepsCount];
                    Marshal.Copy(rfStepsPtr, attenuation_table, 0, AttenuationStepsCount);// dane 8 bitowe
                                                                                          //Console.WriteLine($"Dostępne kroki tłumienia RF: {AttenuationStepsCount}");
                    return AttenuationStepsCount;
                }
            }
            return -1;
        }

        public bool set_atenuation(int rfAttenuationIndex)
        {
            if (GetSddcDeviceInstance() == IntPtr.Zero) return false;
            // Console.WriteLine($"Ustawianie tłumienia RF (index): {rfAttenuationIndex}");
            if (!Sddc_SetTunerRfAttenuation(GetSddcDeviceInstance(), rfAttenuationIndex))
            {
               // Console.WriteLine($"BŁĄD USTAWIENIA TŁUMIENIA RF:   {openErrorMessage.ToString()}");

                //Close();
                return false;
            }
            return true;
        }



        public bool set_dither(bool dither)
        {
            if (GetSddcDeviceInstance() == IntPtr.Zero) return false;
            if (!Sddc_SetAdcDither(GetSddcDeviceInstance(), dither))
            {
                // Close();
                return false;
            }
            return true;
        }
        public bool set_gain(int ifGainIndex)
        {
            if (GetSddcDeviceInstance() == IntPtr.Zero)
                return false;
            //Console.WriteLine($"Ustawianie wzmocnienia IF (index): {ifGainIndex}");
            if (!Sddc_SetTunerIfGain(GetSddcDeviceInstance(), ifGainIndex))
            {
                //Sddc_GetLastError(_sddcDeviceInstance, openErrorMessage.Capacity, openErrorMessage);
               // MessageBox.Show($"BŁĄD USTAWIENIA WZMOCNIENIA IF:  {openErrorMessage.ToString()}", "Błąd Operacji ustawienia wzmocnienia", MessageBoxButtons.OK, MessageBoxIcon.Error);

                //Close();
                return false;
            }
            return true;

        }

        public int get_gain()
        {
            return Sddc_GetTunerIfGain(GetSddcDeviceInstance());
        }

        public int get_if_gain_steps()
        {
            IntPtr rfStepsPtr;
            GainStepsCount = Sddc_GetIfGainSteps(GetSddcDeviceInstance(), out rfStepsPtr);
            if (GainStepsCount > 0 && rfStepsPtr != IntPtr.Zero)
            {
                if_gain_table = new float[GainStepsCount];
                Marshal.Copy(rfStepsPtr, if_gain_table, 0, GainStepsCount);// dane 8 bitowe
                                                                           // Console.WriteLine($"Dostępne kroki tłumienia RF: {GainStepsCount}");
                return GainStepsCount;
            }
            return -1;
        }

        public bool SetFreqCorrection(int correction)
        {
            FreqCorrection = correction;

            return true;
        }

        public bool SetAdcPga(bool pga)
        {
            if (GetSddcDeviceInstance() == IntPtr.Zero)
                return false;
            //Console.WriteLine($"Ustawianie wzmocnienia IF (index): {ifGainIndex}");
            if (!Sddc_SetAdcPga(GetSddcDeviceInstance(), pga))
            {
                //Sddc_GetLastError(_sddcDeviceInstance, openErrorMessage.Capacity, openErrorMessage);
               // MessageBox.Show($"BŁĄD USTAWIENIA AUTOMATYCZNEGO WZMOCNIENIA IF:  {openErrorMessage.ToString()}", "Błąd Operacji ustawienia wzmocnienia", MessageBoxButtons.OK, MessageBoxIcon.Error);

                return false;
            }
            return true;
        }

        public bool SetUptRand(bool rand)
        {
            if (GetSddcDeviceInstance() == IntPtr.Zero)
                return false;
            //Console.WriteLine($"Ustawianie wzmocnienia IF (index): {ifGainIndex}");
            if (!Sddc_SetUptRand(GetSddcDeviceInstance(), rand))
            {
                //Sddc_GetLastError(_sddcDeviceInstance, openErrorMessage.Capacity, openErrorMessage);
             //   MessageBox.Show($"BŁĄD USTAWIENIA RAND ADC:  {openErrorMessage.ToString()}", "Błąd Operacji ustawienia wzmocnienia", MessageBoxButtons.OK, MessageBoxIcon.Error);

                return false;
            }
            return true;
        }

        public bool SetVhfBias(bool bias)
        {
            if (GetSddcDeviceInstance() == IntPtr.Zero)
                return false;
            //Console.WriteLine($"Ustawianie wzmocnienia IF (index): {ifGainIndex}");
            if (!Sddc_SetVhfBias(GetSddcDeviceInstance(), bias))
            {
                //Sddc_GetLastError(_sddcDeviceInstance, openErrorMessage.Capacity, openErrorMessage);
                MessageBox.Show($"BŁĄD USTAWIENIA Automatycznego WZMOCNIENIA IF:  {openErrorMessage.ToString()}", "Błąd Operacji ustawienia wzmocnienia", MessageBoxButtons.OK, MessageBoxIcon.Error);

                //Close();
                return false;
            }
            return true;
        }

        public bool SetHfBias(bool bias)
        {
            if (GetSddcDeviceInstance() == IntPtr.Zero)
                return false;
            //Console.WriteLine($"Ustawianie wzmocnienia IF (index): {ifGainIndex}");
            if (!Sddc_SetHfBias(GetSddcDeviceInstance(), bias))
            {
                //Sddc_GetLastError(_sddcDeviceInstance, openErrorMessage.Capacity, openErrorMessage);
                MessageBox.Show($"BŁĄD USTAWIENIA Automatycznego WZMOCNIENIA IF:  {openErrorMessage.ToString()}", "Błąd Operacji ustawienia wzmocnienia", MessageBoxButtons.OK, MessageBoxIcon.Error);

                //Close();
                return false;
            }
            return true;
        }


        public bool GetHfBias(bool bias)
        {
            if (GetSddcDeviceInstance() == IntPtr.Zero)
                return false;
            //Console.WriteLine($"Ustawianie wzmocnienia IF (index): {ifGainIndex}");
            return Sddc_GetHfBias(GetSddcDeviceInstance());
        }

        public bool GetVhfBias(bool bias)
        {
            if (GetSddcDeviceInstance() == IntPtr.Zero)
                return false;
            //Console.WriteLine($"Ustawianie wzmocnienia IF (index): {ifGainIndex}");
            return Sddc_GetVhfBias(GetSddcDeviceInstance());
        }
    }
}
