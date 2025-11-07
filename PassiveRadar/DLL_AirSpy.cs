using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;

namespace PasiveRadar
{


    // --- 1. Definicje struktur i wyliczeń (mapowanie z C na C#) ---
    // Atrybut LayoutKind.Sequential zapewnia, że pola w C# mają taką samą kolejność jak w C.
    [StructLayout(LayoutKind.Sequential)]
    public struct airspy_read_partid_serialno_t
    {
        // uint32_t part_id[2];
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public UInt32[] part_id;

        // uint32_t serial_no[4];
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public UInt32[] serial_no;
    }



    [StructLayout(LayoutKind.Sequential)]
    public struct airspy_lib_version_t
    {
        public uint major_version;
        public uint minor_version;
        public uint revision;
    }


    [StructLayout(LayoutKind.Sequential)]
    public struct airspy_transfer_t
    {
        public IntPtr device;
        public IntPtr ctx;
        public IntPtr samples;
        public int sample_count;
        public ulong dropped_samples;
        public airspy_sample_type sample_type;
    }

    public enum airspy_sample_type
    {
        AIRSPY_SAMPLE_FLOAT32_IQ = 0,
        AIRSPY_SAMPLE_INT16_IQ = 2,
    }

    public enum airspy_error
    {
        AIRSPY_SUCCESS = 0,
        AIRSPY_TRUE = 1,
        AIRSPY_ERROR_INVALID_PARAM = -2,
        AIRSPY_ERROR_NOT_FOUND = -5,
        AIRSPY_ERROR_BUSY = -6,
        AIRSPY_ERROR_NO_MEM = -11,
        AIRSPY_ERROR_UNSUPPORTED = -12,
        AIRSPY_ERROR_LIBUSB = -1000,
        AIRSPY_ERROR_THREAD = -1001,
        AIRSPY_ERROR_STREAMING_THREAD_ERR = -1002,
        AIRSPY_ERROR_STREAMING_STOPPED = -1003,
        AIRSPY_ERROR_OTHER = -9999,
    }




    internal class DLL_AirSpy
    {
        public IntPtr airspyDevice;

        const int MAX_PACKETS = 100;

        public DLL_AirSpy()
        {
            airspyDevice = IntPtr.Zero;
        }

        // Zmienna dostępna zarówno dla kodu C# wywołującego callback, jak i dla wątku głównego
        public static ConcurrentQueue<short[]> DataQueue = new ConcurrentQueue<short[]>();


        private const string DllName = "airspy.dll";

        // Funkcja zwrotna (callback) do przetwarzania próbek
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int airspy_sample_block_cb_fn(ref airspy_transfer_t transfer);



        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int airspy_board_partid_serialno_read(IntPtr device, ref airspy_read_partid_serialno_t read_partid_serialno);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int airspy_board_id_read(IntPtr device, byte[] value);


        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int airspy_set_rf_bias(IntPtr device, byte value); /* Parameter value shall be 0=Disable BiasT or 1=Enable BiasT */


        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int airspy_set_packing(IntPtr device, byte value); /* Parameter value shall be 0=Disable Packing or 1=Enable Packing */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int airspy_get_samplerates(IntPtr device, ref uint[] buffer, uint len);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void airspy_lib_version(out airspy_lib_version_t lib_version);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int airspy_open(out IntPtr device);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int airspy_close(IntPtr device);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int airspy_set_samplerate(IntPtr device, uint samplerate);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int airspy_set_freq(IntPtr device, uint freq_hz);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int airspy_set_lna_gain(IntPtr device, byte value);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int airspy_set_mixer_gain(IntPtr device, byte value);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int airspy_set_vga_gain(IntPtr device, byte value);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int airspy_set_lna_agc(IntPtr device, byte value);
        /* Parameter value:
            0=Disable MIXER Automatic Gain Control
            1=Enable MIXER Automatic Gain Control
        */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int airspy_start_rx(IntPtr device, airspy_sample_block_cb_fn callback, IntPtr rx_ctx);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int airspy_stop_rx(IntPtr device);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int airspy_set_sample_type(IntPtr device, airspy_sample_type sample_type);

        // Pole do przechowywania referencji do delegata, aby Garbage Collector go nie usunął
        private static airspy_sample_block_cb_fn _callback;

        public UInt64 GetSN(bool sn_pd = false)
        {
            if (airspyDevice == IntPtr.Zero) return 0;

            airspy_read_partid_serialno_t serial_number_t = new airspy_read_partid_serialno_t();

            airspy_board_partid_serialno_read(airspyDevice, ref serial_number_t);

            string sn = "" + serial_number_t.serial_no[0] + serial_number_t.serial_no[1] + serial_number_t.serial_no[2] + serial_number_t.serial_no[3];
            string pd = "" + serial_number_t.part_id[0] + serial_number_t.part_id[1];
            string res = "";

            UInt64 number;

            if (!sn_pd)
            {
                res = "" + sn;
                number = (UInt64)double.Parse(res);
            }
            else
            {
                res = "" + pd;
                number = (UInt64)double.Parse(res);
            }
            return number;

        }


        public string GetIBoardd()
        {
            if (airspyDevice == IntPtr.Zero) return "";

            byte[] val = new byte[256];
            airspy_board_id_read(airspyDevice, val);
            string str = "";

            for (int i = 0; i < val.Length; i++)
                str += "" + val[i];

            return str;
        }
        public string GetVersion()
        {
            airspy_lib_version(out airspy_lib_version_t lib_version);
            string str = lib_version.major_version + " " + lib_version.minor_version + " " + lib_version.revision;
            if (str == "") str = "No version";
            return str;
        }

        public bool SetBiasTee(byte set_bias)
        {
            if (airspyDevice == IntPtr.Zero) return false;
            if (airspy_set_rf_bias(airspyDevice, set_bias) != 0) /* Parameter value shall be 0=Disable BiasT or 1=Enable BiasT */
                return false;
            return true;
        }

        public bool SetPacking(byte set_bias)
        {
            if (airspyDevice == IntPtr.Zero) return false;
            if (airspy_set_packing(airspyDevice, set_bias) != 0) /* Parameter value shall be 0=Disable BiasT or 1=Enable BiasT */
                return false;
            return true;
        }

        public bool SetAgc(byte agc)  //0-disable, 1-enable
        {
            if (airspyDevice == IntPtr.Zero) return false;
            if (airspy_set_lna_agc(airspyDevice, agc) != 0) /* Parameter value shall be 0=Disable BiasT or 1=Enable BiasT */
                return false;
            return true;
        }


        public void getRates()
        {
            if (airspyDevice == IntPtr.Zero) return;

            uint[] buff = new uint[10];

            airspy_get_samplerates(airspyDevice, ref buff, (uint)buff.Length);
        }

        public bool Open()
        {
            IntPtr copyPtr = airspyDevice;

            Close();
            int result = airspy_open(out airspyDevice);
            if (result != (int)airspy_error.AIRSPY_SUCCESS)
            {
                // Console.WriteLine($"Błąd otwierania urządzenia: {result}");
                return false;
            }
            //  Console.WriteLine("Urządzenie Airspy otwarte pomyślnie.");
            return true;
        }

        public bool SampleRate(uint sampleRate)
        {
            if (airspyDevice == IntPtr.Zero) return false;

            getRates();

            //Console.WriteLine($"Ustawiam Sample Rate na {sampleRate} Hz...");
            int result = airspy_set_samplerate(airspyDevice, sampleRate);
            if (result != (int)airspy_error.AIRSPY_SUCCESS)
            {
                //Console.WriteLine($"Błąd ustawienia Sample Rate: {result}");
                return false;
            }
            return true;
        }

        public bool CentralFrequency(uint centerFreq)
        {
            if (airspyDevice == IntPtr.Zero) return false;
            //Console.WriteLine($"Ustawiam częstotliwość na {centerFreq / 1000000.0} MHz...");
            int result = airspy_set_freq(airspyDevice, centerFreq);
            if (result != (int)airspy_error.AIRSPY_SUCCESS)
            {
                Console.WriteLine($"Błąd ustawienia częstotliwości: {result}");
                return false;
            }

            return true;
        }

        public bool SampleType(uint type)
        {
            if (airspyDevice == IntPtr.Zero) return false;

            // Console.WriteLine("Ustawiam typ próbek na INT16_IQ...");
            int result = airspy_set_sample_type(airspyDevice, airspy_sample_type.AIRSPY_SAMPLE_INT16_IQ);
            if (result != (int)airspy_error.AIRSPY_SUCCESS)
            {
                //Console.WriteLine($"Błąd ustawienia typu próbek: {result}");
                return false;
            }
            return true;
        }

        public bool setLNAGain(byte gain)
        {
            if (airspyDevice == IntPtr.Zero) return false;

            int result = airspy_set_lna_gain(airspyDevice, gain);
            if (result != (int)airspy_error.AIRSPY_SUCCESS)
            {
                //Console.WriteLine($"Błąd ustawienia typu próbek: {result}");
                return false;
            }
            return true;
        }

        public bool SetMixerGain(byte gain)
        {
            if (airspyDevice == IntPtr.Zero) return false;

            int result = airspy_set_mixer_gain(airspyDevice, gain);
            if (result != (int)airspy_error.AIRSPY_SUCCESS)
            {
                //Console.WriteLine($"Błąd ustawienia typu próbek: {result}");
                return false;
            }
            return true;
        }

        public bool SetVgaGain(byte gain)
        {
            if (airspyDevice == IntPtr.Zero) return false;

            int result = airspy_set_vga_gain(airspyDevice, gain);
            if (result != (int)airspy_error.AIRSPY_SUCCESS)
            {
                //Console.WriteLine($"Błąd ustawienia typu próbek: {result}");
                return false;
            }
            return true;
        }


        public bool Close()
        {
            if (airspyDevice == IntPtr.Zero) return false;

            int result = airspy_close(airspyDevice);
            if (result != (int)airspy_error.AIRSPY_SUCCESS)
            {
                Console.WriteLine($"Błąd startu strumieniowania: {result}");
                return false;
            }
            return false;
        }

        public bool StartStreaming()
        {
            if (airspyDevice == IntPtr.Zero) return false;

            // --- 5. Rozpoczęcie strumieniowania ---
            _callback = MySampleCallback; // Przypisanie delegata
            airspy_set_sample_type(airspyDevice, airspy_sample_type.AIRSPY_SAMPLE_INT16_IQ);

            //Console.WriteLine("Rozpoczynam strumieniowanie...");
            int result = airspy_start_rx(airspyDevice, _callback, IntPtr.Zero);
            if (result != (int)airspy_error.AIRSPY_SUCCESS)
            {
                //   Console.WriteLine($"Błąd startu strumieniowania: {result}");
                return false;
            }
 
            return true;
        }

        public bool StopStreaming()
        {
            if (airspyDevice == IntPtr.Zero) return false;

            int result = airspy_stop_rx(airspyDevice);
            if (result != (int)airspy_error.AIRSPY_SUCCESS)
            {
                //   Console.WriteLine($"Błąd startu strumieniowania: {result}");
                return false;
            }
            return true;
        }

        private static int MySampleCallback(ref airspy_transfer_t transfer)
        {
            // Sprawdzamy, czy typ to INT16_IQ
            if (transfer.sample_type != airspy_sample_type.AIRSPY_SAMPLE_INT16_IQ)
            {
                // Console.WriteLine("Otrzymano nieobsługiwany typ próbek!");
                return 0;
            }

            // Obliczanie rozmiaru danych w bajtach (ilość próbek * 2 bajty na próbkę I + 2 bajty na próbkę Q)
            long dataSizeBytes = (long)transfer.sample_count * 4;

            // Deklaracja tablicy na próbki I/Q
            short[] samples = new short[transfer.sample_count * 2];

            // Kopiowanie danych z niezarządzanej pamięci (IntPtr) do zarządzanej tablicy C#
            Marshal.Copy(transfer.samples, samples, 0, samples.Length);


           // rotate_180_s16(ref samples);

            // C. Bezpieczne dla wątków dodanie danych do kolejki
            if (DataQueue.Count >= MAX_PACKETS)
            {
                // Usuwamy najstarszy element (FIFO), aby zrobić miejsce dla najnowszego
                DataQueue.TryDequeue(out short[] res);

                // Opcjonalnie: Zwiększamy licznik utraconych pakietów
                // Interlocked.Increment(ref droppedPacketCount); 
            }
            DataQueue.Enqueue(samples);
 
            // Zwracamy 0, aby kontynuować strumieniowanie.
            return 0;
        }


 

        public static implicit operator DLL_AirSpy(DLL_RSP1 v)
        {
            throw new NotImplementedException();
        }
    }
}
