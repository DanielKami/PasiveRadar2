using System;
using System.Threading;
using System.Windows;


namespace PasiveRadar
{
    public class RadioAirSpy
    {
        public int BufferSize;
        public int frequency;
        public int FreqCorrection;

        public uint rate;
        public uint decymation = 1;
        public uint decymation_old = 1;
        public int transfer;

        public int gainLNA = 0;
        public int gainMixer = 0;
        public int gainRF = 0;
        public bool agc = false;

        public int Number_of_LNAgains = 22;
        public int Number_of_Mixergains = 22;
        public int Number_of_RFgains = 22;

        public bool BiasTee = false;
        public bool packing = false;

        public int item;

        //USB dongle data
        public uint dev_number;//in usb port
        public bool status = false;//it is open or close false - close
        public string manufact;
        public string product;
        public UInt64 SN;

        public int dongle_type;
        public uint lost = 0;  //number of lost bytes per buffer size/ can be recalculated per [s]

        // Size of dongle buffer, it is smaller than buffer. Data are cumulated to generate full buffer. 
        UInt16 Radio_buffer_size = 1024 * 16; //DEFAULT_BUF_LENGTH; MAXIMAL_BUF_LENGTH		(256 * 16384); MINIMAL_BUF_LENGTH		512



        DLL_AirSpy dll;
        RadioPostProcessing postProcessing;


        public float[] dataIQ = null;                //Dato for radar
        public float[] dataIQ_radio = null;         //szhort data for radio display

        ushort RadioInternalBufferSize = 16;// in kB


        Thread thread = null;

        volatile bool exit = false;
        public volatile bool exited = true; //Flag is true when the thread is exited

        public AutoResetEvent autoVisualEvent;

        public RadioAirSpy()
        {
            dll = new DLL_AirSpy();
            postProcessing = new RadioPostProcessing();
            status = false;
        }

        public void InitBuffers(Flags flags)
        {
            Stop();

            BufferSize = (int)flags.BufferSize;
            RadioInternalBufferSize = (ushort)flags.Radio_buffer_size;

        }

        public void Start()
        {
            Open();
            exit = false;


            if (thread == null && exited == true)
            {
                exited = false;
                thread = new Thread(new ThreadStart(Read));
                thread.Priority = System.Threading.ThreadPriority.AboveNormal;
                thread.Start();
            }
        }

        public void Stop()
        {

            exit = true;
            // Thread.Sleep(300);//Like that works without hanging
            if (thread != null)
            {
                thread.Join(2000);
                thread = null;
            }

        }

        public bool Open()
        {
            bool r = dll.Open();

            if (r == true)
            {
                status = true;

                SN = dll.GetSN();

                r = dll.CentralFrequency((uint)frequency);
                if (r == false) return r;

                r = dll.SampleRate(rate);
                if (r == false) return r;

                dll.setLNAGain((byte)gainLNA);
                dll.SetMixerGain((byte)gainMixer);
                dll.SetVgaGain((byte)gainRF);

                status = true;
            }
            else
                status = false;
            return status;
        }

        public void Close()
        {
            dll.Close();

            status = false;
        }


        public bool SetCentralFreq(int freq)
        {

            if (dll.CentralFrequency((uint)(frequency + FreqCorrection)))
            {
                frequency = freq;
                return true;
            }
            return false;
        }


        public bool SetFreqCorrection(int freq)
        {
            FreqCorrection = freq;

            return SetCentralFreq(frequency);
        }

        public bool SetSampleRate(uint _rate)
        {

            if (dll.SampleRate(_rate))
            {
                rate = _rate;
                return true;
            }
            return false;
        }


        public bool SeBiasTee(bool s)
        {
            BiasTee = s;
            if (s == false)
                return dll.SetBiasTee(0);
            else
                return dll.SetBiasTee(1);
        }

        public bool SetPacking(bool s)
        {
            packing = s;
            if (s == false)
                return dll.SetPacking(0);
            else
                return dll.SetPacking(1);
        }

        public void Read()
        {

            if (dll.airspyDevice == IntPtr.Zero)
            {
                exited = true;
                return;
            }


            Int16[] data_dongle = new Int16[Radio_buffer_size];
            Array.Clear(data_dongle, 0, Radio_buffer_size);
            lost = 0;


            postProcessing.Init(BufferSize, Radio_buffer_size, rate, decymation);

            //Start striming
            bool r = dll.StartStreaming();
            if (r == false)
            {
                String str = "Error streaming imposible ";
                MessageBox.Show(str);
                return;
            }

            //******************************************************************************************

            while (!exit)
            {
                try
                {
                    if (status)
                        if (!DLL_AirSpy.DataQueue.TryDequeue(out data_dongle))
                        {
                            exited = true;
                        }
                }
                catch (Exception ex)
                {
                    String str = "Error open device.   " + ex.ToString();
                    MessageBox.Show(str);
                    // r = dll.streaming_stop();
                    exited = true;
                    Close();
                    break;
                }

                if (decymation != decymation_old)
                {
                    postProcessing.Init(BufferSize, Radio_buffer_size, rate, decymation);
                    decymation_old = decymation;
                }
                postProcessing.PostProc(data_dongle, ref dataIQ, ref dataIQ_radio);

            }

            r = dll.StopStreaming();
            if (r == false)
            {
                String str = "Error streaming stop. " + r;
                MessageBox.Show(str);
            }

            exited = true;
        }


        public string GetName()
        {
            // return dll.get_device_name(dev_number);
            return "AirSpy";
        }

        public bool SetgainLNA(int gain)
        {

            if (dll.setLNAGain((byte)gain))
            {
                gainLNA = gain;
                return true;
            }
            return false;
        }

        public bool SetgainMixer(int gain)
        {
            if (dll.SetMixerGain((byte)gain))
            {
                gainMixer = gain;
                return true;
            }
            return false;
        }

        public bool SetGainRF(int gain)
        {
            if (dll.SetVgaGain((byte)gain))
            {
                gainRF = gain;
                return true;
            }
            return false;
        }


        public bool SetAgc(bool s)
        {
            agc = s;
            if (!s)
                return dll.SetAgc(0);
            else
                return dll.SetAgc(1);
        }


    }
}
