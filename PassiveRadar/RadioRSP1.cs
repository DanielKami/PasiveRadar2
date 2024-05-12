using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;

namespace PasiveRadar
{
    public class RadioRSP1 : IDisposable
    {
        //    private readonly AutoResetEvent _signal = new AutoResetEvent();
        public int BufferSize;
        public int frequency;
        public int FreqCorrection;
        public uint rate;
        public uint bandwith;
        public int gain = 80; //Max 102
        public DLL_RSP1.mirisdr_band_t band = 0;
        public uint IF_frequency = 0;
        public uint ctx_frequency = 0;
        public int transfer;

        public int gainLNA = 0;
        public int gainMixer = 0;
        public int gainBaseBand = 0;
        public bool gain_mode = false;


        public int item;

        //USB dongle data
        public uint dev_number;//in usb port
        public bool status = false;//it is open or close false - close
        public string manufact;
        public string product;
        public string serial;

        public int dongle_type;
        public uint lost = 0;  //number of lost bytes per buffer size/ can be recalculated per [s]

        // Size of dongle buffer, it is smaller than buffer. Data are cumulated to generate full buffer. 
        UInt16 Radio_buffer_size = 16384; //DEFAULT_BUF_LENGTH; MAXIMAL_BUF_LENGTH		(256 * 16384); MINIMAL_BUF_LENGTH		512
        public DLL_RSP1.transfer_t tt = DLL_RSP1.transfer_t.MIRISDR_TRANSFER_BULK;
        public DLL_RSP1.format_t format = DLL_RSP1.format_t.MIRISDR_FORMAT_AUTO_ON;

        //Gains of amlplifier
        public int[] tuner_gain_list;
        public int Number_of_gains = 102;

        DLL_RSP1 dll;

        public Int16[] dataIQ = null;
        uint RadioInternalBufferSize = 16 * 1024;// 16K IS RECOMENDET

        Thread thread = null;
        private readonly Object _Lock = new Object();
        volatile bool exit = false;
        public volatile bool exited = true; //Flag is true when the thread is exited

        public AutoResetEvent autoVisualEvent;
        //int gap = 0;

        public RadioRSP1()
        {
            dll = new DLL_RSP1();
            status = false;
        }

        public void InitBuffers(Flags flags)
        {
            Stop();
            //gap = (int)flags.Radio_buffer_size * 2;
            BufferSize = (int)flags.BufferSize;
            RadioInternalBufferSize = (uint)Math.Pow(2, 5) * 1024;//flags.Radio_buffer_size
            dataIQ = new Int16[(int)flags.BufferSize];
        }

        public void Start()
        {
            exit = false;


            if (thread == null && exited == true)
            {
                exited = false;
                thread = new Thread(new ThreadStart(Read));
                thread.Priority = System.Threading.ThreadPriority.AboveNormal;
                thread.Start();
            }
        }

        //SDRplay
      //  [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
        public void Stop()
        {

            exit = true;
            // Thread.Sleep(300);//Like that works without hanging
            if (thread != null)
            {
                thread.Join(2000);
                thread = null;
            }

            //if (thread != null)
            //{

            //    while (!exited) ;
            //    thread = null;
            //    //Array.Clear(dataIQ, 0, BufferSize);
            //}

        }

        //For async mode
        public int ResetBuffer()
        {
            return dll.reset_buffer();
        }
        public int Open()
        {
            int r = dll.open(dev_number);

            if (r == 0)
            {
                int r1 = dll.get_usb_strings(ref manufact, ref product, ref serial);

                Set_tuner_gain_mode(gain_mode);

                // band = GetBand();

                r = SetCentralFreq();
                if (r < 0) return r;
                r = dll.set_transfer(tt);
                if (r < 0) return r;
                r = dll.set_sample_format(format);
                if (r < 0) return r;
                r = dll.set_hw_flavour(DLL_RSP1.mirisdr_hw_flavour_t.MIRISDR_HW_DEFAULT);
                if (r < 0) return r;
                r = dll.set_sample_rate(rate);
                if (r < 0) return r;
                r = dll.set_bandwidth(bandwith);
                if (r < 0) return r;
                r = dll.set_if_freq(IF_frequency);
                if (r < 0) return r;

                ctx_frequency = dll.get_xtal_freq();
                // r = dll.cancel_async();


                r = dll.set_tuner_gain(gain);
                if (r < 0) return r;
                gainLNA = dll.get_lna_gain();
                gainMixer = dll.get_mixer_gain();
                gainBaseBand = dll.get_baseband_gain();

                status = true;
            }
            else
                status = false;
            return r;
        }
        public void Set_flawor(int setting)
        {
            switch (setting)
            {
                case 0:
                    dll.set_hw_flavour(DLL_RSP1.mirisdr_hw_flavour_t.MIRISDR_HW_DEFAULT);
                    break;
                case 1:
                    dll.set_hw_flavour(DLL_RSP1.mirisdr_hw_flavour_t.MIRISDR_HW_SDRPLAY);
                    break;
                deafoult:
                    dll.set_hw_flavour(DLL_RSP1.mirisdr_hw_flavour_t.MIRISDR_HW_DEFAULT);
                    break;
            }
        }

        public void Close()
        {
            dll.close();

            status = false;
        }

        public void Set_usb_transfer(int _transfer)
        {
            transfer = _transfer;

            DLL_RSP1.transfer_t _tt = DLL_RSP1.transfer_t.MIRISDR_TRANSFER_BULK;
            if (transfer == 1) _tt = DLL_RSP1.transfer_t.MIRISDR_TRANSFER_BULK;
            if (transfer == 2) _tt = DLL_RSP1.transfer_t.MIRISDR_TRANSFER_ISOC;

            if (tt != _tt)
                /* Set USB transfer type */
                switch (transfer)
                {
                    case 1:
                        dll.set_transfer(DLL_RSP1.transfer_t.MIRISDR_TRANSFER_BULK);
                        break;
                    case 2:
                        dll.set_transfer(DLL_RSP1.transfer_t.MIRISDR_TRANSFER_ISOC);
                        break;
                }
        }

        //It is memory sensitive transfer has to be stopped
        public int Set_sample_format(int sample_format)
        {
            int r = -1;
            /* Set sample format */
            switch (sample_format)
            {
                case 0:
                    r = dll.set_sample_format(DLL_RSP1.format_t.MIRISDR_FORMAT_252_S16);
                    format = DLL_RSP1.format_t.MIRISDR_FORMAT_252_S16;
                    break;

                case 1:
                    r = dll.set_sample_format(DLL_RSP1.format_t.MIRISDR_FORMAT_336_S16);
                    format = DLL_RSP1.format_t.MIRISDR_FORMAT_336_S16;
                    break;

                case 2:
                    r = dll.set_sample_format(DLL_RSP1.format_t.MIRISDR_FORMAT_384_S16);
                    format = DLL_RSP1.format_t.MIRISDR_FORMAT_384_S16;
                    break;

                case 3:
                    r = dll.set_sample_format(DLL_RSP1.format_t.MIRISDR_FORMAT_504_S16);
                    format = DLL_RSP1.format_t.MIRISDR_FORMAT_504_S16;
                    break;
                case 4:
                    r = dll.set_sample_format(DLL_RSP1.format_t.MIRISDR_FORMAT_504_S8);
                    format = DLL_RSP1.format_t.MIRISDR_FORMAT_504_S8;
                    break;
                case 5:
                    r = dll.set_sample_format(DLL_RSP1.format_t.MIRISDR_FORMAT_AUTO_ON);
                    format = DLL_RSP1.format_t.MIRISDR_FORMAT_AUTO_ON;
                    break;
            }
            return r;
        }

        public static DLL_RSP1.format_t Sample_format(int sample_format)
        {
            /* Set sample format */
            switch (sample_format)
            {
                case 0:
                    return DLL_RSP1.format_t.MIRISDR_FORMAT_252_S16;
                case 1:
                    return DLL_RSP1.format_t.MIRISDR_FORMAT_336_S16;
                case 2:
                    return DLL_RSP1.format_t.MIRISDR_FORMAT_384_S16;
                case 3:
                    return DLL_RSP1.format_t.MIRISDR_FORMAT_504_S16;
                case 4:
                    return DLL_RSP1.format_t.MIRISDR_FORMAT_504_S8;
                case 5:
                    return DLL_RSP1.format_t.MIRISDR_FORMAT_AUTO_ON;
            }
            return DLL_RSP1.format_t.MIRISDR_FORMAT_AUTO_ON;
        }

        public int get_device_usb_strings(ref string str)
        {
            string str1 = "";
            string str2 = "";
            string str3 = "";

            return dll.get_device_usb_strings(dev_number, ref str1, ref str2, ref str3);

        }

        public int SetCentralFreq()
        {
            return dll.set_center_freq((uint)frequency);
        }

        public int GetCentralFreq()
        {
            frequency = (int)dll.get_center_freq();

            return 0;
        }

        public int SetFreqCorrection(int freq)
        {
            dll.set_freq_correction(freq);
            return dll.set_center_freq((uint)frequency);
        }

        //public int GetFreqCorrection()
        //{
        //    if (dev != IntPtr.Zero)
        //        FreqCorrection = dll.get_freq_correction(dev);

        //    return -1;
        //}

        public int SetSampleRate(uint _rate)
        {
            rate = _rate;
            int r; //0- succes

            r = dll.set_sample_rate(rate);

            return r;
        }

        public int GetSampleRate()
        {
            rate = dll.get_sample_rate();
            return 0;
        }

        public int Reset()
        {
            return dll.reset();
        }

        //Set the memory after format change
        public void Read()
        {
            if (dll.dev == IntPtr.Zero)
            {
                exited = true;
                return;
            }

            int r;
            bool bit8;
            UInt16 readed = 0;
            int count = 0;
            Int16[] temp_buffer = new Int16[BufferSize];
            //Int16[] temp_buffer = new Int16[BufferSize + gap];
            // Int16[] buf_zero = new Int16[gap];

            //Array.Clear(buf_zero, 0, buf_zero.Length);

            //8/16bit format
            if (format == DLL_RSP1.format_t.MIRISDR_FORMAT_504_S8)
            {
                bit8 = true;
                Radio_buffer_size = dll.get_BufforSize();
            }
            else
            {
                bit8 = false;

                int temp = dll.get_BufforSize() / 2;
                Radio_buffer_size = (ushort)temp;
            }
            dll.InitBuffer(Radio_buffer_size * 2);
            Int16[] data_dongle = new Int16[Radio_buffer_size];
            Array.Clear(data_dongle, 0, Radio_buffer_size);
            int reduced_buffer_size = BufferSize - Radio_buffer_size;
            int buffer_multiplication = BufferSize / Radio_buffer_size - 1;//-1 because copy offset and the radio buffer must be smaller than BufferSize
            lost = 0;

            r = dll.streaming_start();
            if (r < 0)
            {
                return;
            }

            while (!exit)
            {
                try
                {
                    if (status)
                        r = dll.read_sync(ref data_dongle, Radio_buffer_size, ref readed, ref lost, bit8);//direct data from dongle
                }
                catch (Exception ex)
                {
                    String str = "Error open device. " + ex.ToString() + "Error " + r;
                    MessageBox.Show(str);
                    r = dll.streaming_stop();
                    exited = true;
                    break;
                }

                //For all configurations we stay with 8bit format later this can be converted to int (this thread is already working hard.
                //first fill t he temp buffer
                if (count < buffer_multiplication)
                {
                    Array.Copy(data_dongle, 0, temp_buffer, data_dongle.Length * count, data_dongle.Length);
                    count++;
                }

                //////Make a place for new data
                Array.Copy(temp_buffer, data_dongle.Length, temp_buffer, 0, reduced_buffer_size);

                ////Add to the end of temp_buffer a new data (fast)
                Array.Copy(data_dongle, 0, temp_buffer, reduced_buffer_size, data_dongle.Length);


                //Array.Copy(temp_buffer, data_dongle.Length + gap, temp_buffer, 0, reduced_buffer_size - gap);//make a place for new data
                //Array.Copy(data_dongle, 0, temp_buffer, reduced_buffer_size, data_dongle.Length);// add new data dongle
                //Array.Copy(buf_zero, 0, temp_buffer, reduced_buffer_size - gap, gap); //    set gap with 0


                //Protct reading data
                lock (_Lock)
                {
                    //copy temp to dataIQ must be protected, acces by other threads
                    Array.Copy(temp_buffer, 0, dataIQ, 0, dataIQ.Length);
                }
            }

            r = dll.streaming_stop();
            if (r < 0)
            {
                String str = "Error streaming stop. " + r;
                MessageBox.Show(str);
            }

            exited = true;
        }

        public string GetName()
        {
            return dll.get_device_name(dev_number);
        }


        //possible If frequencies:    0, 450000, 1620000,  2048000
        public int Set_if_frequency(uint frequency)
        {
            int r;

            r = dll.set_if_freq(frequency);
            if (r >= 0)
            {
                IF_frequency = frequency;
            }
            return r;
        }
        public uint Get_if_frequency()
        {
            return dll.get_if_freq();
        }

        public int Set_bandwidth(uint _bandwith)
        {
            bandwith = _bandwith;
            return dll.set_bandwidth(_bandwith);
        }
        public uint Get_bandwidth()
        {
            uint bandwidth = dll.get_bandwidth();
            return bandwidth;
        }



        //********************************************************************************
        //                               Gain
        //********************************************************************************

        //  0- auto gain; 1 - manual gain
        public void Set_tuner_gain_mode(bool mode)
        {
            gain_mode = mode;

            if (mode == false)
                dll.set_tuner_gain_mode(1);//manual
            else
                dll.set_tuner_gain_mode(0);//auto bot doesn't work
        }

        //return 0- auto gain; 1 - manual gain
        public int Get_tuner_gain_mode()
        {
            return dll.get_tuner_gain_mode();
        }

        //Gains
        // Reset to 0xf380 to enable gain control added Dec 5 2014 SM5BSZ
        //    mirisdr_write_reg(p, 0x08, 0xf380);   
        public int SetGein()
        {
            return dll.set_gain();
        }

        public int SetTunerGein(int _gain)
        {
            gain = _gain;
            return dll.set_tuner_gain(gain);
        }

        //get the main gain
        public int GetGain()
        {
            gain = dll.get_tuner_gain();
            return gain;
        }

        //Mixer gain a component of main gain 
        /*
 * Gain reduction is an index that depends on the AM mode (only applies to AM inputs)
 *          AM1     AM2     VHF
 * 0x00    0 dB    0 dB     19 dB? 
 * 0x01    6 dB   24 dB     19 dB?
 * 0x10   12 dB   24 dB     19 dB?
 * 0x11   18 dB   24 dB     19 dB?
 */

        public int GetMixerGain()
        {
            gainMixer = dll.get_mixer_gain();
            return gainMixer;
        }
        public int SetMixerGain(int _gain)
        {
            gainMixer = _gain;
            return dll.set_mixer_gain(_gain);
        }


        //LNA gain For VHF mode LNA is turned on to + 24 db
        public int GetLNAGain()
        {
            gainLNA = dll.get_lna_gain();
            return gainLNA;
        }
        public int SetLNAGain(int _gain)
        {
            gainLNA = _gain;
            return dll.set_lna_gain(_gain);
        }

        //Mixer reduced gain
        public int GetMReducedGain()
        {
            return dll.get_mixbuffer_gain();
        }
        public int SetMReducedGain(int gain)
        {
            return dll.set_mixbuffer_gain(gain);
        }

        //Baseband gain , 0 - 59,
        public int GetBasebandGain()
        {
            gainBaseBand = dll.get_baseband_gain();
            return gainBaseBand;
        }
        public int SetBasebandGain(int _gain)
        {
            gainBaseBand = _gain;
            return dll.set_baseband_gain(_gain);
        }

        public DLL_RSP1.mirisdr_band_t GetBand()
        {
            return dll.get_band();
        }

        //Bias
        public int SetBias(int bias)
        {
            return dll.set_bias(bias);
        }
        public int GetBias()
        {
            return dll.get_bias();
        }

        //ADC
        public int ADC_stop()
        {
            return dll.adc_stop();
        }
        public int ADC_start()
        {
            return dll.adc_init();
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // dispose managed resources
            }
            // free native resources
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
