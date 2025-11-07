using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace PasiveRadar
{
    public unsafe class RadioRX888 : IDisposable
    {
        public int BufferSize;
        internal const int Radio_buffer_size = 1024 * 32;

        public uint frequency;
        public int FreqCorrection;
        public uint rate;
        public uint decymation = 1;
        uint decymation_old = 1;
        public int bandwith;
        public int gain = 10;
        public int attenuation = 22;
        public bool VHF_HF = true;
        public bool dither;
        public bool Pga;
        public bool Rand;
        public bool HfBias;
        public bool VhfBias;
        public int item;
        public uint dev_number;
        public bool status = false;
        public string manufact;
        public string product;
        public string serial;
        public int dongle_type;
        public uint lost = 0;
        public int[] tuner_gain_list;
        public int Number_of_gains = 16;
        public int[] tuner_attenuation_list;
        public int Number_of_attenuations = 24;

        private readonly DLL_RX888 dll;
        RadioPostProcessing postProcessing;

        private Thread thread = null;
        public volatile bool exited = true;

        // Public stuff
        public float[] dataIQ;
        public float[] dataIQ_radio;

        private IntPtr _rawDeviceBuffer;


        // Reusable working buffers
        // private short[] _newSamples; // size Radio_buffer_size/2
        private MathNet.Numerics.Complex32[] ConvertedRawData; // for FFT domain (Complex32)
        private MathNet.Numerics.Complex32[] _demodulated;    // time-domain analytic signal (I + jQ)
        private MathNet.Numerics.Complex32[] _firHistory;     // circular history buffer for FIR
        private float[] _hilbertMul;                          // real multipliers for Hilbert filter
        private readonly float _scaleFactor = 1f / 32768f;

        private int _firHistoryPos = 0;

        MathNet.Numerics.Complex32 Complex32Null;
        public RadioRX888()
        {
            dll = new DLL_RX888();
            postProcessing = new RadioPostProcessing();
            status = false;
        }

        public void InitBuffers(Flags flags)
        {
            if (!exited)
                Stop();

            BufferSize = (int)flags.BufferSize;

            if (_rawDeviceBuffer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(_rawDeviceBuffer);
            }
            _rawDeviceBuffer = Marshal.AllocHGlobal(Radio_buffer_size);

            dataIQ = new float[BufferSize];
            dataIQ_radio = new float[Radio_buffer_size];

        }

        private void DisposeBuffers()
        {
            if (_rawDeviceBuffer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(_rawDeviceBuffer);
                _rawDeviceBuffer = IntPtr.Zero;
            }

        }

        private void ProcessDataLoop()
        {
            SetSampleRate(rate);
            SetAttenuation(attenuation);
            SetGain(gain);
            SetSampleRate(rate);


            if (decymation < 1) decymation = 1;

            dataIQ_radio = new float[(BufferSize / 2) / decymation];

            postProcessing.Init(BufferSize, Radio_buffer_size / 2, rate, decymation);
            short[] datadongle = new short[Radio_buffer_size];

            try
            {
                bool success = dll.SimpleRead(ref _rawDeviceBuffer, Radio_buffer_size, 0);

                while (!exited)
                {
                    if (dll.SimpleRead(ref _rawDeviceBuffer, Radio_buffer_size, 1))
                    {

                        short* src = (short*)_rawDeviceBuffer.ToPointer();


                        for (int i = 0; i < Radio_buffer_size; i++)
                        {
                            datadongle[i] = (short)-src[i];
                        }


                        if (decymation != decymation_old)
                        {
                            postProcessing.Init(BufferSize, Radio_buffer_size / 2, rate, decymation);
                            decymation_old = decymation;
                        }
                        postProcessing.PostProc(datadongle, ref dataIQ, ref dataIQ_radio, true);

                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Wystąpił błąd w wątku przetwarzania: {ex.Message}");
            }
            finally
            {
                dll.SimpleRead(ref _rawDeviceBuffer, Radio_buffer_size, 2);
                exited = true;
            }
        }


        public int Open()
        {
            status = true;
            bool r = dll.Open();
            SetSampleRate(rate);
            SetCentralFreq(frequency);
            SetVHF_HF(VHF_HF);
            SetGain(gain);
            SetAttenuation(attenuation);
            SetDither(dither);
            SetAdcPga(Pga);
            return 0;
        }
        public void Close() { DLL_RX888.Close(); status = false; }
        public string GetName() => dll.get_name();
        public bool SetVHF_HF(bool VHF_HF_) { VHF_HF = VHF_HF_; return VHF_HF ? dll.set_RF_mode(2) : dll.set_RF_mode(1); }
        public bool SetDither(bool dither_) { dither = dither_; return dll.set_dither(dither); }
        public int SetCentralFreq(uint frequency_)
        {

            ulong CorrectedFrequency = frequency_ + (uint)bandwith / 4;// + (ulong)correction;
            frequency = (uint)CorrectedFrequency;

            if (dll.set_frequency(CorrectedFrequency)) return 0;
            else
                return -1;
        }
        public int SetGain(int gain_) { gain = gain_; if (dll.set_gain(gain_)) return 0; else return -1; }
        public int SetAttenuation(int att_) { attenuation = att_; if (dll.set_atenuation(att_)) return 0; else return -1; }
        public uint GetCentralFreq()
        {
            frequency = (uint)dll.get_frequency(); return 0;
        }
        public int SetFreqCorrection(int freq) { if (dll.SetFreqCorrection(freq)) return 0; else return -1; }
        public int SetSampleRate(uint _rate) { rate = _rate; if (dll.set_sample_rate(rate)) return 0; else return -1; }

        public bool SetAdcPga(bool _pga) { Pga = _pga; return dll.SetAdcPga(Pga); }
        public bool SetUptRand(bool _rand) { Rand = _rand; return dll.SetUptRand(Pga); }
        public bool SetVhfBias(bool _bias) { VhfBias = _bias; return dll.SetVhfBias(Pga); }
        public bool SetHfBias(bool _bias) { HfBias = _bias; return dll.SetHfBias(Pga); }

        public void Start()
        {
            Open();

            exited = false;
            if (thread == null || exited)
            {
                exited = false;
                thread = new Thread(new ThreadStart(ProcessDataLoop));
                thread.Priority = System.Threading.ThreadPriority.AboveNormal;
                thread.IsBackground = true;
                thread.Start();
            }
        }

        public void Stop()
        {
            if (thread != null && !exited)
            {
                //status = false;
                exited = true;

                thread.Join(2000);
                if (thread.IsAlive)
                {
                    thread.Abort();
                }
                thread = null;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Stop();
                DisposeBuffers();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
