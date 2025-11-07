//#define INTFLAG
using System;
using System.Runtime.InteropServices;
using System.Windows;

namespace PasiveRadar
{
    class Ambiguity
    {

        [System.Runtime.InteropServices.DllImport(@"Ambiguity.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Initialize(uint BufferSize, uint col, uint row, float doopler_shift, short[] Name);

        [System.Runtime.InteropServices.DllImport(@"Ambiguity.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern int Run(IntPtr Data_In0, IntPtr Data_In1, float[] Data_Out, float amplification, float doppler_zoom, int shift, bool mode, short scale_type, bool remove_symetric);
        public static extern int Run(float[] Data_In0, float[] Data_In1, float[] Data_Out, float amplification, float doppler_zoom, int shift, bool mode, short scale_type, bool remove_symetric);

        [System.Runtime.InteropServices.DllImport(@"Ambiguity.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Release();

        //// NOWE: Funkcje alokacji/dealokacji Pinned Memory
        //[DllImport(@"Ambiguity.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern int AllocHostMemory(uint bufferSize, out IntPtr hostPtr0, out IntPtr hostPtr1);

        //[DllImport(@"Ambiguity.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern int FreeHostMemory(ref IntPtr hostPtr0, ref IntPtr hostPtr1);

        private readonly Object LockMem = new Object();
        // private Thread ThreadGPU;

        //Important to protect Run form wrong values of Nth, col, Row
        private uint BufferSize, Columns, Rows;

        // 2. Alokacja Pamięci Przypiętej (Pinned Memory)
        //IntPtr dataIn0Ptr = IntPtr.Zero;
        //IntPtr dataIn1Ptr = IntPtr.Zero;

        public void Prepare(Flags flags)
        {
            short[] name = new short[Flags.MAX_DEVICE_NAME];
            //Name is a return string containing info about NVIDIA card
            int err = Initialize(flags.BufferSize, flags.Columns, flags.Rows, (float)flags.DopplerZoom, name);

            //copy the device name
            flags.DeviceName = "";
            for (int i = 0; i < Flags.MAX_DEVICE_NAME; i++)
                if (name[i] > 32 && name[i] < 126)
                    flags.DeviceName += (char)name[i];

            if (err < 0)
            {
                String str = "CUDA error. " + err;
                MessageBox.Show(str);
            }


            //int allocResult = Ambiguity.AllocHostMemory(flags.BufferSize, out dataIn0Ptr, out dataIn1Ptr);

            //if (allocResult != 0 || dataIn0Ptr == IntPtr.Zero || dataIn1Ptr == IntPtr.Zero)
            //{
            //    Console.ForegroundColor = ConsoleColor.Red;
            //    Console.WriteLine($"[BŁĄD KRYTYCZNY] Nie udało się zaalokować Pinned Memory. Kod błędu: {allocResult}");
            //    Console.ResetColor();
            //    Ambiguity.Release();
            //    return;
            //}


            BufferSize = flags.BufferSize; Columns = flags.Columns; Rows = flags.Rows;
        }

        public int ProcessGPU(float[] In0, float[] In1, float[] Out, Flags flags)
        {
            int err;
            var dataRadar = new float[Out.Length + flags.Rows];

            //Protection
            if (BufferSize != flags.BufferSize || Columns != flags.Columns || Rows != flags.Rows)
            {
                short[] name = new short[Flags.MAX_DEVICE_NAME];
                //Name is a return string containing info about NVIDIA card
                err = Initialize(flags.BufferSize, flags.Columns, flags.Rows, (float)flags.DopplerZoom, name);
                if (err < 0)
                {
                    String str = "CUDA error. " + err;
                    MessageBox.Show(str);
                    return -1;
                }
                BufferSize = flags.BufferSize; Columns = flags.Columns; Rows = flags.Rows;
            }

            try
            {

               // Kopiowanie danych z tablicy C# do Pinned Memory (IntPtr) za pomocą Marshal
                //Marshal.Copy(In0, 0, dataIn0Ptr, In0.Length);
                //if (flags.two_dongles)
                //    Marshal.Copy(In1, 0, dataIn1Ptr, In1.Length);

                //err = Run(dataIn0Ptr, dataIn1Ptr, dataRadar, flags.PasiveGain, flags.DopplerZoom, flags.DistanceShift, flags.two_dongles, flags.scale_type, flags.remove_symetrics);
                 err = Run(In0, In1, dataRadar, flags.PasiveGain, flags.DopplerZoom, flags.DistanceShift, flags.two_dongles, flags.scale_type, flags.remove_symetrics);
                if (err < 0)
                {
                    String str = "CUDA error. " + err;
                    MessageBox.Show(str);
                    return -1;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return -1;
            }

            lock (LockMem)
            {
                Array.Copy(dataRadar, 0, Out, 0, Out.Length);
            }
            return 0;
        }


        public void Release(Flags flags)
        {
            int err = Release();
            if (err < 0)
            {
                String str = "CUDA error. " + err;
                MessageBox.Show(str);
            }
           //  err = Ambiguity.FreeHostMemory(ref dataIn0Ptr, ref dataIn1Ptr);
            if (err < 0)
            {
                String str = "CUDA error. " + err;
                MessageBox.Show(str);
            }
        }


    }
}
