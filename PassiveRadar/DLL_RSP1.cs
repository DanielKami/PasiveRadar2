using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
//using static System.Windows.Forms.VisualStyles.VisualStyleElement;
//using System.Windows.Forms;

namespace PasiveRadar
{


    public unsafe class DLL_RSP1
    {
        //pointer to the device. It can be changed to table of pointers for multiple devices
        public IntPtr dev = IntPtr.Zero;
        IntPtr pnt = IntPtr.Zero;  //data pointer
        public enum mirisdr_band_t
        {
            MIRISDR_BAND_AM1,
            MIRISDR_BAND_AM2,
            MIRISDR_BAND_VHF,
            MIRISDR_BAND_3,
            MIRISDR_BAND_45,
            MIRISDR_BAND_L,
        }

        public enum mirisdr_hw_flavour_t
        {
            MIRISDR_HW_DEFAULT,
            MIRISDR_HW_SDRPLAY,
        }

        public enum transfer_t
        {
            MIRISDR_TRANSFER_BULK = 0,
            MIRISDR_TRANSFER_ISOC
        }

        public enum format_t
        {
            MIRISDR_FORMAT_252_S16,
            MIRISDR_FORMAT_336_S16,
            MIRISDR_FORMAT_384_S16,
            MIRISDR_FORMAT_504_S16,
            MIRISDR_FORMAT_504_S8,
            MIRISDR_FORMAT_AUTO_ON
        }

        public enum if_freq_t
        {
            MIRISDR_IF_ZERO = 0,
            MIRISDR_IF_450KHZ,
            MIRISDR_IF_1620KHZ,
            MIRISDR_IF_2048KHZ
        }



        //[System.Runtime.InteropServices.DllImport(@"SDRplay.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern Win32.HRESULT Sdrplay_libUSBInit(HINSTANCE hDll);

        //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        #region device
        [System.Runtime.InteropServices.DllImport(@"SDRplay.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Sdrplay_get_device_count();
        [System.Runtime.InteropServices.DllImport(@"SDRplay.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Sdrplay_get_device_name(uint index, StringBuilder product);
        [System.Runtime.InteropServices.DllImport(@"SDRplay.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Sdrplay_get_device_usb_strings(uint index, StringBuilder manufact, StringBuilder product, StringBuilder serial);
        #endregion

        #region main
        [System.Runtime.InteropServices.DllImport(@"SDRplay.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Sdrplay_open(ref IntPtr dev, uint index);
        [System.Runtime.InteropServices.DllImport(@"SDRplay.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Sdrplay_close(IntPtr dev);
        [System.Runtime.InteropServices.DllImport(@"SDRplay.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Sdrplay_reset(IntPtr dev);                       /* extra */
        [System.Runtime.InteropServices.DllImport(@"SDRplay.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Sdrplay_reset_buffer(IntPtr dev);
        [System.Runtime.InteropServices.DllImport(@"SDRplay.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Sdrplay_get_usb_strings(IntPtr dev, StringBuilder manufact, StringBuilder product, StringBuilder serial);
        [System.Runtime.InteropServices.DllImport(@"SDRplay.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Sdrplay_set_hw_flavour(IntPtr dev, mirisdr_hw_flavour_t hw_flavour);
        #endregion

        #region sync
        [System.Runtime.InteropServices.DllImport(@"SDRplay.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Sdrplay_read_sync(IntPtr dev, IntPtr buf, UInt16 len, ref UInt16 readed, ref UInt32 lost);

        [System.Runtime.InteropServices.DllImport(@"SDRplay.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern UInt16 Sdrplay_BufforSize(IntPtr dev);
        #endregion



        #region async, we dont need it we are async anyway in thread
        [System.Runtime.InteropServices.DllImport(@"SDRplay.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern int Sdrplay_read_async(IntPtr dev, mirisdr_read_async_cb_t cb, void* ctx, uint num, uint_t len);
        //[System.Runtime.InteropServices.DllImport(@"SDRplay.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Sdrplay_cancel_async(IntPtr dev);
        [System.Runtime.InteropServices.DllImport(@"SDRplay.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Sdrplay_cancel_async_now(IntPtr dev);            /* extra */
        [System.Runtime.InteropServices.DllImport(@"SDRplay.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Sdrplay_start_async(IntPtr dev);                 /* extra */
        [System.Runtime.InteropServices.DllImport(@"SDRplay.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Sdrplay_stop_async(IntPtr dev);                  /* extra */
        #endregion

        #region adc
        [System.Runtime.InteropServices.DllImport(@"SDRplay.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Sdrplay_adc_init(IntPtr dev);                    /* extra */
        [System.Runtime.InteropServices.DllImport(@"SDRplay.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Sdrplay_adc_stop(IntPtr dev);
        #endregion

        #region rate control 
        [System.Runtime.InteropServices.DllImport(@"SDRplay.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Sdrplay_set_sample_rate(IntPtr dev, uint rate);
        [System.Runtime.InteropServices.DllImport(@"SDRplay.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint Sdrplay_get_sample_rate(IntPtr dev);
        #endregion

        #region sample format control
        [System.Runtime.InteropServices.DllImport(@"SDRplay.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Sdrplay_set_sample_format(IntPtr dev, format_t tt);  /* extra */
        [System.Runtime.InteropServices.DllImport(@"SDRplay.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern format_t Sdrplay_get_sample_format(IntPtr dev);   /* extra */
        #endregion

        #region streaming control 
        [System.Runtime.InteropServices.DllImport(@"SDRplay.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Sdrplay_streaming_start(IntPtr dev);             /* extra */
        [System.Runtime.InteropServices.DllImport(@"SDRplay.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Sdrplay_streaming_stop(IntPtr dev);              /* extra */
        #endregion

        #region frequency
        [System.Runtime.InteropServices.DllImport(@"SDRplay.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Sdrplay_set_center_freq(IntPtr dev, uint freq);
        [System.Runtime.InteropServices.DllImport(@"SDRplay.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint Sdrplay_get_center_freq(IntPtr dev);
        [System.Runtime.InteropServices.DllImport(@"SDRplay.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Sdrplay_set_if_freq(IntPtr dev, uint freq);  /* extra */
        [System.Runtime.InteropServices.DllImport(@"SDRplay.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint Sdrplay_get_if_freq(IntPtr dev);            /* extra */
        [System.Runtime.InteropServices.DllImport(@"SDRplay.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Sdrplay_set_xtal_freq(IntPtr dev, uint freq);/* extra */
        [System.Runtime.InteropServices.DllImport(@"SDRplay.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint Sdrplay_get_xtal_freq(IntPtr dev);          /* extra */
        [System.Runtime.InteropServices.DllImport(@"SDRplay.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Sdrplay_set_bandwidth(IntPtr dev, uint bw);  /* extra */
        [System.Runtime.InteropServices.DllImport(@"SDRplay.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint Sdrplay_get_bandwidth(IntPtr dev);          /* extra */
        [System.Runtime.InteropServices.DllImport(@"SDRplay.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Sdrplay_set_offset_tuning(IntPtr dev, int on);   /* extra */
        [System.Runtime.InteropServices.DllImport(@"SDRplay.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern mirisdr_band_t Sdrplay_get_band(IntPtr dev);         /* extra */
        [System.Runtime.InteropServices.DllImport(@"SDRplay.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Sdrplay_set_freq_correction(IntPtr dev, int ppm);         /* extra */

        #endregion

        #region not implemented yet  
        [System.Runtime.InteropServices.DllImport(@"SDRplay.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Sdrplay_set_direct_sampling(IntPtr dev, int on);
        #endregion

        #region transfer 
        [System.Runtime.InteropServices.DllImport(@"SDRplay.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Sdrplay_set_transfer(IntPtr dev, transfer_t tt);       /* extra */
        [System.Runtime.InteropServices.DllImport(@"SDRplay.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern transfer_t Sdrplay_get_transfer(IntPtr dev);        /* extra */
        #endregion

        #region  gain
        [System.Runtime.InteropServices.DllImport(@"SDRplay.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Sdrplay_set_gain(IntPtr dev);                    /* extra */
        [System.Runtime.InteropServices.DllImport(@"SDRplay.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Sdrplay_tuner_gains(IntPtr dev, int* gains);
        [System.Runtime.InteropServices.DllImport(@"SDRplay.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Sdrplay_tuner_gain(IntPtr dev, int gain);
        [System.Runtime.InteropServices.DllImport(@"SDRplay.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Sdrplay_get_tuner_gain(IntPtr dev);              /* extra */
        [System.Runtime.InteropServices.DllImport(@"SDRplay.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Sdrplay_set_tuner_gain_mode(IntPtr dev, int mode);
        [System.Runtime.InteropServices.DllImport(@"SDRplay.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Sdrplay_get_tuner_gain_mode(IntPtr dev);         /* extra */
        [System.Runtime.InteropServices.DllImport(@"SDRplay.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Sdrplay_set_mixer_gain(IntPtr dev, int gain);    /* extra */
        [System.Runtime.InteropServices.DllImport(@"SDRplay.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Sdrplay_set_mixbuffer_gain(IntPtr dev, int gain);/* extra */
        [System.Runtime.InteropServices.DllImport(@"SDRplay.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Sdrplay_set_lna_gain(IntPtr dev, int gain);      /* extra */
        [System.Runtime.InteropServices.DllImport(@"SDRplay.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Sdrplay_set_baseband_gain(IntPtr dev, int gain); /* extra */
        [System.Runtime.InteropServices.DllImport(@"SDRplay.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Sdrplay_get_mixer_gain(IntPtr dev);              /* extra */
        [System.Runtime.InteropServices.DllImport(@"SDRplay.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Sdrplay_get_mixbuffer_gain(IntPtr dev);          /* extra */
        [System.Runtime.InteropServices.DllImport(@"SDRplay.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Sdrplay_get_lna_gain(IntPtr dev);                /* extra */
        [System.Runtime.InteropServices.DllImport(@"SDRplay.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Sdrplay_get_baseband_gain(IntPtr dev);           /* extra */
        [System.Runtime.InteropServices.DllImport(@"SDRplay.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Sdrplay_set_bias(IntPtr dev, int bias);           /* extra */
        [System.Runtime.InteropServices.DllImport(@"SDRplay.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Sdrplay_get_bias(IntPtr dev);                    /* extra */
        #endregion





        //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

        //c# accesed functions

        //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx



        public void InitBuffer(int lenght)
        {
            //The main data buffer is in byte format so no worry about the size
            pnt = Marshal.AllocHGlobal(lenght);

        }
        public int get_device_count()
        {
            int count = Sdrplay_get_device_count();
            return count;
        }
        public int get_device_usb_strings(uint index, ref string manufact, ref string product, ref string serial)
        {
            int r = 0;
            StringBuilder str1 = new StringBuilder(new String(' ', 256));
            StringBuilder str2 = new StringBuilder(new String(' ', 256));
            StringBuilder str3 = new StringBuilder(new String(' ', 256));
            try
            {
                r = Sdrplay_get_device_usb_strings(index, str1, str2, str3);
            }
            catch (Exception e) { }

            manufact = str1.ToString();
            product = str2.ToString();
            serial = str3.ToString();

            return r;
        }
        public string get_device_name(uint devIndex)
        {
            int r;
            StringBuilder str1 = new StringBuilder(new String(' ', 256));
            try
            {
                r = Sdrplay_get_device_name(devIndex, str1);
            }
            catch (Exception e)
            {
                r = -1;
            }

            string product = "Not found";
            if (r == -1)
                return product;
            else
                return str1.ToString();

        }

        public int open(uint devIndex)
        {
            int r;

            if (dev != null)
            {
                close();
                dev = IntPtr.Zero;
            }

            try
            {
                r = Sdrplay_open(ref dev, devIndex);
            }
            catch (Exception ex)
            {
                String str = "Can't open SDRplay dongle. " + ex.ToString();
                MessageBox.Show(str);
                return -1;
            }
            return r;
        }
        public int close()
        {
            return Sdrplay_close(dev);
        }
        public int reset()
        {
            return Sdrplay_reset(dev);
        }
        public int reset_buffer()
        {
            return Sdrplay_reset_buffer(dev);
        }
        public int get_usb_strings(ref string manufact, ref string product, ref string serial)
        {
            int r;
            StringBuilder str1 = new StringBuilder(new String(' ', 256));
            StringBuilder str2 = new StringBuilder(new String(' ', 256));
            StringBuilder str3 = new StringBuilder(new String(' ', 256));
            try
            {
                r = Sdrplay_get_usb_strings(dev, str1, str2, str3);
            }
            catch (Exception e) { }

            manufact = str1.ToString();
            product = str2.ToString();
            serial = str3.ToString();

            return r = -1;
        }
        public int set_hw_flavour(mirisdr_hw_flavour_t hw_flavour)
        {
            return Sdrplay_set_hw_flavour(dev, hw_flavour);
        }

        //Errors
        //returns 0 on success(and populates <tt>transferred</tt>)
        //returns -1 LIBUSB_ERROR_TIMEOUT if the transfer timed out
        //returns -2 LIBUSB_ERROR_PIPE if the endpoint halted
        //returns -3 LIBUSB_ERROR_OVERFLOW if the device offered more data, see ref libusb_packetoverflow
        //returns -4 LIBUSB_ERROR_NO_DEVICE if the device has been disconnected
        //returns -5 LIBUSB_ERROR_BUSY if called from event handling context
        //returns -6 LIBUSB_ERROR_INVALID_PARAM if the transfer size is larger than the operating system and/or hardware can support(see \ref asynclimits)
        //returns another LIBUSB_ERROR code on other error
        public int read_sync(ref Int16[] buf, UInt16 len, ref UInt16 n_read, ref UInt32 lost, bool bit8)
        {
            if (dev == IntPtr.Zero)
                return -1;

            int r = Sdrplay_read_sync(dev, pnt, len, ref n_read, ref lost);

            byte[] tmp = new byte[len];
            if (bit8) //8bit data
            {

                Marshal.Copy(pnt, tmp, 0, len);

                rotate_180_s8(tmp);

                //convert to int
                for (int i = 0; i < len; i++)
                {
                    buf[i] = tmp[i];
                    buf[i] -= 127;
                    buf[i] <<= 4;//increase the signal to be on the same level as 16bits
                }
            }
            else //16 bit data
            {
                Marshal.Copy(pnt, buf, 0, (int)len);
                rotate_180_s16(ref buf);
            }

            return r;
        }

        //function define internal buffer size 
        public UInt16 get_BufforSize()
        {
            if (dev != IntPtr.Zero)
                return Sdrplay_BufforSize(dev);
            else
                return 1024 * 5;
        }   
        public int adc_init()
        {
            if (dev == IntPtr.Zero) return -1;
            return Sdrplay_adc_init(dev);
        }
        public int adc_stop()
        {
            if (dev == IntPtr.Zero) return -1;
            return Sdrplay_adc_stop(dev);
        }

        public int set_sample_rate(uint rate)
        {
            if (dev == IntPtr.Zero) return -1;
            return Sdrplay_set_sample_rate(dev, rate);
        }
        public uint get_sample_rate()
        {
            if (dev != IntPtr.Zero)
                return Sdrplay_get_sample_rate(dev);
            return 0;
        }
        public int set_sample_format(format_t tt)
        {
            if (dev == IntPtr.Zero) return -1;
            return Sdrplay_set_sample_format(dev, tt);
        }
        public format_t get_sample_formate()
        {
            if (dev != IntPtr.Zero)
                return Sdrplay_get_sample_format(dev);
            return 0;
        }
        public int streaming_start()
        {
            if (dev == IntPtr.Zero) return -1;
            return Sdrplay_streaming_start(dev);
        }
        public int streaming_stop()
        {
            if (dev == IntPtr.Zero) return -1;
            return Sdrplay_streaming_stop(dev);
            
        }
        public int set_center_freq(uint freq)
        {
            if (dev == IntPtr.Zero) return -1;
            return Sdrplay_set_center_freq(dev, freq);
        }
        public uint get_center_freq()
        {
            if (dev != IntPtr.Zero)
                return Sdrplay_get_center_freq(dev);
            return 0;
        }


        //alowed frequencies: 0, 450000, 1620000, 2048000
        public int set_if_freq(uint freq)
        {
            if (dev == IntPtr.Zero) return -1;
            return Sdrplay_set_if_freq(dev, freq);
        }
        public uint get_if_freq()
        {
            if (dev == IntPtr.Zero)
                return Sdrplay_get_if_freq(dev);
            return 0;
        }
        public int set_xtal_freq(uint freq)
        {
            if (dev == IntPtr.Zero)
                return Sdrplay_set_xtal_freq(dev, freq);
            return 0;
        }
        public uint get_xtal_freq()
        {
            if (dev != IntPtr.Zero)
                return Sdrplay_get_xtal_freq(dev);
            return 0;
        }
        public int set_bandwidth(uint bw)
        {
            if (dev == IntPtr.Zero) return -1;
            return Sdrplay_set_bandwidth(dev, bw);
        }
        public uint get_bandwidth()
        {
            if (dev != IntPtr.Zero)
                return Sdrplay_get_bandwidth(dev);
            return 0;
        }
        public int set_offset_tuning(int on)
        {
            if (dev == IntPtr.Zero) return -1;
            return Sdrplay_set_offset_tuning(dev, on);
        }
        public int set_freq_correction(int ppm)
        {
            if (dev == IntPtr.Zero) return -1;
            return Sdrplay_set_freq_correction(dev, ppm);
        }


        public int set_transfer(transfer_t tt)
        {
            if (dev == IntPtr.Zero) return -1;
            return Sdrplay_set_transfer(dev, tt);
        }
        public transfer_t get_transfer()
        {
            return Sdrplay_get_transfer(dev);
        }
        public int set_gain()
        {
            if (dev == IntPtr.Zero) return -1;
            return Sdrplay_set_gain(dev);
        }
        public int tuner_gains(int* gains)//usless max is 103
        {
            if (dev == IntPtr.Zero) return -1;
            return Sdrplay_tuner_gains(dev, gains);
        }

        //For VHF mode LNA is turned on to + 24 db, mixer to + 19 dB and baseband
        //can be adjusted continuously from 0 to 59 db, of which the maximum gain of 102 db
        public int set_tuner_gain(int gain)//main tuner gain max is 102
        {
            if (dev == IntPtr.Zero) return -1;
            return Sdrplay_tuner_gain(dev, gain);
        }

        //get the main gein
        public int get_tuner_gain()
        {
            if (dev == IntPtr.Zero) return -1;
            return Sdrplay_get_tuner_gain(dev);
        }
        public int set_tuner_gain_mode(int mode)
        {
            if (dev == IntPtr.Zero) return -1;
            return Sdrplay_set_tuner_gain_mode(dev, mode);
        }
        public int get_tuner_gain_mode()
        {
            if (dev == IntPtr.Zero) return -1;
            return Sdrplay_get_tuner_gain_mode(dev);
        }//main gein

        //Mixer gain
        public int get_mixer_gain()
        {
            if (dev == IntPtr.Zero) return -1;
            return Sdrplay_get_mixer_gain(dev);
        }
        public int set_mixer_gain(int gain)
        {
            if (dev == IntPtr.Zero) return -1;
            return Sdrplay_set_mixer_gain(dev, gain);
        }


        //LNA gain
        public int get_lna_gain()
        {
            if (dev == IntPtr.Zero) return -1;
            return Sdrplay_get_lna_gain(dev);
        }
        public int set_lna_gain(int gain)
        {
            if (dev == IntPtr.Zero) return -1;
            return Sdrplay_set_lna_gain(dev, gain);
        }

        //baseband gain
        public int get_baseband_gain()
        {
            if (dev == IntPtr.Zero) return -1;
            return Sdrplay_get_baseband_gain(dev);
        }
        public int set_baseband_gain(int gain)
        {
            if (dev == IntPtr.Zero) return -1;
            return Sdrplay_set_baseband_gain(dev, gain);
        }

        //mixbuffer gain
        public int get_mixbuffer_gain()
        {
            if (dev == IntPtr.Zero) return -1;
            return Sdrplay_get_mixbuffer_gain(dev);
        }
        public int set_mixbuffer_gain(int gain)
        {
            if (dev == IntPtr.Zero) return -1;
            return Sdrplay_set_mixbuffer_gain(dev, gain);
        }

        public int set_bias(int bias)
        {
            if (dev == IntPtr.Zero) return -1;
            return Sdrplay_set_bias(dev, bias);
        }
        public int get_bias()
        {
            if (dev == IntPtr.Zero) return -1;
            return Sdrplay_get_bias(dev);
        }
        public mirisdr_band_t get_band()
        {
            if (dev == IntPtr.Zero)
                return Sdrplay_get_band(dev);
            return 0;
        }

        public int cancel_async()
        {
            if (dev == IntPtr.Zero) return -1;
            return Sdrplay_cancel_async(dev);
        }

        //Rotations
        void rotate_180_s8(byte[] buf)
        /* 180 rotation is 1+0j, 
               0  1   2   3   4  5   6   7
           or [0, 1; -2, -3;  4, 5; -6, -7] */

        {
            uint a;
            for (uint i = 0; i < buf.Length - 7; i += 8)
            {
                /* uint8_t negation = 255 - x */
                buf[a = i + 2] = (byte)(255 - buf[a]);
                buf[a = i + 3] = (byte)(255 - buf[a]);

                buf[a = i + 6] = (byte)(255 - buf[a]);
                buf[a = i + 7] = (byte)(255 - buf[a]);
            }
        }

        void rotate_180_s16(ref Int16[] buf)
        /* 180 rotation is 1+0j, 
               0  1   2   3   4  5   6   7
           or [0, 1; -2, -3;  4, 5; -6, -7] */

        {
            uint a;
            for (uint i = 0; i < buf.Length; i += 8)
            {
                /* uint16_t negation =  - x */
                a = i + 2;
                buf[a] = (short)-buf[a];
                a++;
                buf[a] = (short)-buf[a];
                a = i + 6;
                buf[a] = (short)-buf[a];
                a++;
                buf[a] = (short)-buf[a];
            }
        }

        void rotate_90_s16(ref Int16[] buf, int len)
        /* 90 rotation is 1+0j, 0+1j, -1+0j, 0-1j
           or [0, 1, -3, 2, -4, -5, 7, -6] */
        {
            int i;
            Int16 tmp;
            for (i = 0; i < len; i += 8)
            {
                /* uint8_t negation = 255 - x */
                tmp = buf[i + 3];
                tmp *= -1;
                buf[i + 3] = buf[i + 2];
                buf[i + 2] = tmp;

                tmp = buf[i + 4]; tmp *= -1;
                buf[i + 4] = tmp;

                tmp = buf[i + 5]; tmp *= -1;
                buf[i + 5] = tmp;

                tmp = buf[i + 6]; tmp *= -1;
                buf[i + 6] = buf[i + 7];
                buf[i + 7] = tmp;
            }
        }
    }

    public class HINSTANCE
    {
    }
}