using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;


namespace PasiveRadar
{
    public unsafe partial class Form1 : Form
    {
        public int cumulation, cumulation_max;
        private readonly Object LockMem = new Object();


        Flags flags;
        Flags save_flags1;
        Flags save_flags2;

        FindRtlSdr findRtlSdr;
        FindRSP1 findRSP1;
        RadioRtlSdr[] radioRtlSdr;   //Control window for RTL-SDR devices
        RadioRSP1[] radioRSP1 = null; //Control window for Miri devices
        Calculate[] calculate;
        RadarCumulate[] radar_cumulate;
        RadarCumulate[] map_cumulate;


        Ambiguity ambiguity;
        ClassRegresion[] Regresion;
        ClassRegresion[] RegresionMap;

        SettingsRtlSdr[] setRtlSdr;
        Settings_RSP1[] setSDRplays;
        WindowsRadio[] radio_window;
        Window[] windowRadar;

        Map mMap;

        short[][] dataOutRadio;

        bool FormsReady = false;
        bool runing = false;

        //Sending flags info to all clases
        public delegate void MyDelegate(Flags flags);
        public static event MyDelegate FlagsDelegate;

        public Form1()
        {
            InitializeComponent();
            //           UsbNotification.RegisterUsbDeviceNotification(this.Handle);
            // SDR1_init();
            //Initialize all flags and values for controls
            flags = new Flags();
            save_flags1 = new Flags();
            save_flags2 = new Flags();
            ambiguity = new Ambiguity();

            flags.Read();
            flags.AMDdriver = FindAMD();//Detect AMD video driver

            save_flags1.Copy(flags);
            save_flags2.Copy(flags);

            //Mouse position parameters
            MouseDownPanel = new bool[Flags.ALL_DONGLES];
            oldX_position = new int[Flags.ALL_DONGLES];
            MHz_perPixel = new double[Flags.ALL_DONGLES];

            string[] str = new string[16];
            findRtlSdr = new FindRtlSdr();
            findRSP1 = new FindRSP1();

            findRSP1.Device();
            findRtlSdr.Device();

            radioRtlSdr = new RadioRtlSdr[Flags.MAX_DONGLES_RTLSDR];
            for (int i = 0; i < Flags.MAX_DONGLES_RTLSDR; i++)
                radioRtlSdr[i] = new RadioRtlSdr();

            //Last window 
            radioRSP1 = new RadioRSP1[Flags.MAX_DONGLES_RSP1];
            for (int i = 0; i < Flags.MAX_DONGLES_RSP1; i++)
                radioRSP1[i] = new RadioRSP1();





            #region Windows
            windowRadar = new Window[Flags.ALL_DONGLES];
            windowRadar[0] = new Window(panelViewport1, 0);
            windowRadar[1] = new Window(panelViewport2, 0);
            windowRadar[2] = new Window(panelViewport3, 0);
            windowRadar[3] = new Window(panelViewport4, 0);

            window_wave = new WindowWave[Flags.ALL_DONGLES];
            window_flow = new WindowFlow[Flags.ALL_DONGLES];

            radio_window = new WindowsRadio[Flags.ALL_DONGLES];

            for (int i = 0; i < Flags.MAX_DONGLES_RTLSDR; i++)
            {
                radio_window[i] = new WindowsRadio(i, "RTLSDR");
                radio_window[i].Show();
                radio_window[i].tuningNumber.frequency = (int)flags.frequency[i];
                radio_window[i].tuningNumber.Update_(false);
                radio_window[i].Initialize(flags);

                window_wave[i] = new WindowWave(radio_window[i].panelRadioWave, i);
                window_flow[i] = new WindowFlow(radio_window[i].panelRadioFlow, i);
            }

            for (int i = (int)Flags.MAX_DONGLES_RTLSDR; i < Flags.ALL_DONGLES; i++)
            {
                radio_window[i] = new WindowsRadio(i, "SDRPLAY");
                radio_window[i].Show();
                radio_window[i].tuningNumber.frequency = (int)flags.frequency[i];
                radio_window[i].tuningNumber.Update_(false);
                radio_window[i].Initialize(flags);

                window_wave[i] = new WindowWave(radio_window[i].panelRadioWave, i);
                window_flow[i] = new WindowFlow(radio_window[i].panelRadioFlow, i);
            }
            #endregion

            DrawRadio();

            radar_cumulate = new RadarCumulate[Flags.ALL_DONGLES];
            Regresion = new ClassRegresion[Flags.ALL_DONGLES];
            calculate = new Calculate[Flags.ALL_DONGLES];
            map_cumulate = new RadarCumulate[Flags.ALL_DONGLES];
            RegresionMap = new ClassRegresion[Flags.ALL_DONGLES];

            for (int i = 0; i < Flags.ALL_DONGLES; i++)
            {
                map_cumulate[i] = new RadarCumulate();
                calculate[i] = new Calculate();
                Regresion[i] = new ClassRegresion();
                RegresionMap[i] = new ClassRegresion();
                radar_cumulate[i] = new RadarCumulate();
                LockMainDataStream[i] = new Object();
                LockRadarScene[i] = new Object();
                LockMap[i] = new Object();
            }

            InitBuffers();

            #region Setting windows
            setRtlSdr = new SettingsRtlSdr[Flags.MAX_DONGLES_RTLSDR];
            for (int i = 0; i < Flags.MAX_DONGLES_RTLSDR; i++)
                setRtlSdr[i] = new SettingsRtlSdr(i);

            setSDRplays = new Settings_RSP1[Flags.MAX_DONGLES_RSP1];
            for (int i = 0; i < Flags.MAX_DONGLES_RSP1; i++)
                setSDRplays[i] = new Settings_RSP1(i);

            for (int i = 0; i < Flags.MAX_DONGLES_RTLSDR; i++)
            {
                if (radioRtlSdr[i] != null)
                {
                    radioRtlSdr[i].rate = (uint)flags.rate[i];
                    setRtlSdr[i].SetSettings(radioRtlSdr[i]);
                }
            }

            for (int i = 0; i < Flags.MAX_DONGLES_RSP1; i++)
            {
                if (setSDRplays[i] != null)
                {
                    radioRSP1[i].rate = (uint)flags.rate[i + Flags.MAX_DONGLES_RTLSDR];
                    radioRSP1[i].bandwith = (uint)flags.bandwitch[i + Flags.MAX_DONGLES_RTLSDR];
                    setSDRplays[i].SetSettings(radioRSP1[i]);
                }
            }

            #endregion

            RestoreState();

            #region Events

            //User control settings
            SettingsRtlSdr.EventGain += new SettingsRtlSdr.MyDelegateSettings(ReturnRadioSettingsRtlSdr);
            Settings_RSP1.EventGain += new Settings_RSP1.MyDelegateSettings(ReturnRadioSettingsRSP1);
            DisplayControl.EventSettings += new DisplayControl.MyDelegate(DisplaySettings);
            RadarControl.RadarSettings += new RadarControl.MyDelegate(RadarSettings);
            TranslateControl.EventSettings += new TranslateControl.MyDelegate(TranslationSettings);



            ///Radio rtlsdr data ready
            SettingsRtlSdr.EventRadio += new SettingsRtlSdr.MyDelegate(AddRadio);
            ///Radio sdrplay data ready
            Settings_RSP1.EventRadio += new Settings_RSP1.MyDelegate(AddRadioSDR1);
            ///Radio sdrplay data ready
           // Settings_SDRplay.EventRadio += new Settings_SDRplay.MyDelegate(AddRadioSDRplay);

            //WindowsRadio size changed
            WindowsRadio.SizeChangedx += new WindowsRadio.DelegateEvents(WindowsSizeCorection);

            //Radio Set pressed
            WindowsRadio.RadioSet += new WindowsRadio.DelegateEventsSettings(WindowsSetPressed);

            //Windows radio mouse event
            WindowsRadio.RadioMouseDown += new WindowsRadio.DelegateMouse(CalculateMouseDown);
            WindowsRadio.RadioMouseMove += new WindowsRadio.DelegateMouse(CalculateMouseMove);
            WindowsRadio.RadioMouseUp += new WindowsRadio.DelegateMouse(CalculateMouseUp);

            //Window radio frequency changed
            WindowsRadio.RadioFrequencyChanged += new WindowsRadio.DelegateEventsSettings(WindowRadioFrequencyChanged);

            //Window radio settings
            WindowsRadio.EventSettings += new WindowsRadio.MyDelegate(DisplaySettings2);

            //Color changes
            ColorForm.EventColor += new ColorForm.MyDelegate(SetCustomColor);

            #endregion


            //Update all user controls
            UpdateFrequencies(flags.LastActiveWindowRadio);
            if (FlagsDelegate != null)
                FlagsDelegate(flags);

            InitSiteMenu();
        }

        private void WindowRadioFrequencyChanged(int Nr, string type) //type:RTLSDR or SDRPLAY
        {
            flags.frequency[Nr] = radio_window[Nr].tuningNumber.frequency;

            if (type == "RTLSDR")
            {
                if (radioRtlSdr == null) return;

                for (int i = 0; i < Flags.MAX_DONGLES_RTLSDR; i++)
                {
                    if (radioRtlSdr[i] != null)
                    {
                        radioRtlSdr[i].frequency = (int)flags.frequency[i];
                        radioRtlSdr[i].SetCentralFreq();
                    }
                }
            }
            if (type == "SDRPLAY")
                if (radioRSP1 == null) return;

            for (int i = 0; i < Flags.MAX_DONGLES_RSP1; i++)
            {
                if (radioRSP1[i] != null)
                {
                    radioRSP1[i].frequency = (int)flags.frequency[i + Flags.MAX_DONGLES_RTLSDR];
                    radioRSP1[i].SetCentralFreq();
                }
            }

            WindowsUpdate();
        }

        private void SetCustomColor(System.Drawing.Color[] col)
        {
            for (int i = 0; i < Flags.ColorTableSize; i++)
            {
                //Convert colors from system to XNA
                flags.ColorThemeTable[i].R = col[i].R;
                flags.ColorThemeTable[i].G = col[i].G;
                flags.ColorThemeTable[i].B = col[i].B;
                flags.ColorThemeTable[i].A = 255;
            }
            //WindowsUpdate();

            for (int i = 0; i < Flags.ALL_DONGLES; i++)
                windowRadar[i].Update(flags);

            foreach (WindowFlow x in window_flow)
                if (x != null) x.Update(flags);
        }

        //Find AMD driver, if yes we have OpenCL support 
        bool FindAMD()
        {
            System.Management.SelectQuery query = new System.Management.SelectQuery("Win32_SystemDriver");
            query.Condition = "Name = 'amdkmafd'";//Name of AMD driver
            System.Management.ManagementObjectSearcher searcher = new System.Management.ManagementObjectSearcher(query);
            var drivers = searcher.Get();

            if (drivers.Count > 0) return true;
            else return false;
        }

        //USB support needs more selectivity a simple way
        //protected override void WndProc(ref Message m)
        //{
        //    int devType;

        //    base.WndProc(ref m);
        //    if (m.Msg == UsbNotification.WmDevicechange)
        //    {
        //        switch ((int)m.WParam)
        //        {
        //            case UsbNotification.DbtDeviceremovecomplete:

        //                devType = Marshal.ReadInt32(m.LParam, 4);
        //                //MessageBox.Show("MEDIArem");
        //                //No radio 0 and 1
        //                StopDraw();

        //                for (int i = 0; i < Flags.ALL_DONGLES; i++)
        //                {
        //                    if (radioRtlSdr[i] != null)
        //                    {
        //                        radioRtlSdr[i].Stop();
        //                        radioRtlSdr[i].Close();
        //                    }
        //                    setRtlSdr[i].ComboBoxRadio_Update(ref findRtlSdr);
        //                }

        //                findRtlSdr.NrOfDevices = 1;


        //                break;

        //            case UsbNotification.DbtDevicearrival:
        //                {
        //                    devType = Marshal.ReadInt32(m.LParam, 4);
        //                    if (devType == 5)
        //                    {
        //                        String str = "New usb device found";
        //                        MessageBox.Show(str);
        //                    }
        //                    //No radio 0 and 1
        //                    StopDraw();

        //                    for (int i = 0; i < Flags.ALL_DONGLES; i++)
        //                    {
        //                        if (radioRtlSdr[i] != null)
        //                        {
        //                            radioRtlSdr[i].Stop();
        //                            radioRtlSdr[i].Close();
        //                        }
        //                    }

        //                    if (!radioRtlSdr[0].status || !radioRtlSdr[1].status)
        //                    {
        //                        findRtlSdr.Device();
        //                        for (int i = 0; i < Flags.ALL_DONGLES; i++)
        //                            setRtlSdr[i].ComboBoxRadio_Update(ref findRtlSdr);
        //                    }

        //                }
        //                break;
        //        }
        //    }
        //}


        private void InitBuffers()
        {
            lock (LockMem)
            {
                for (int i = 0; i < Flags.MAX_DONGLES_RTLSDR; i++)
                {
                    if (radioRtlSdr[i] != null)
                    {
                        radioRtlSdr[i].InitBuffers(flags);
                    }
                }

                for (int i = 0; i < Flags.MAX_DONGLES_RSP1; i++)
                {
                    if (radioRSP1[i] != null)
                    {
                        radioRSP1[i].InitBuffers(flags);
                    }
                }

                dataRadio = new double[Flags.ALL_DONGLES][];
                for (int i = 0; i < Flags.ALL_DONGLES; i++)
                    dataRadio[i] = new double[flags.BufferSize];

                dataRadar = new float[Flags.ALL_DONGLES][];
                for (int i = 0; i < Flags.ALL_DONGLES; i++)
                    dataRadar[i] = new float[flags.Columns * flags.Rows + flags.Columns];


                for (int i = 0; i < Flags.ALL_DONGLES; i++)
                {
                    calculate[i].Init(flags.BufferSizeRadio[i], flags.Cumulation[i]);
                    radar_cumulate[i].Init(flags);
                    map_cumulate[i].Init(flags);
                }

                if (mMap != null)
                    mMap.CopyFlags(flags);
                //uint BufferNegPos = flags.BufferSize + (flags.Negative + flags.Positive) * 2;
                uint BufferNegPos = flags.BufferSize;


                dataOutRadio = new short[Flags.MAX_DONGLES_RSP1 + Flags.MAX_DONGLES_RTLSDR][];//
                for (int i = 0; i < Flags.MAX_DONGLES_RSP1 + Flags.MAX_DONGLES_RTLSDR; i++)
                    dataOutRadio[i] = new short[BufferNegPos];

                PostProc = new float[Flags.MAX_DONGLES_RSP1 + Flags.MAX_DONGLES_RTLSDR][];
                for (int i = 0; i < Flags.MAX_DONGLES_RSP1 + Flags.MAX_DONGLES_RTLSDR; i++)
                    PostProc[i] = new float[flags.Columns * flags.Rows];

                if (ambiguity != null)
                {
                    ambiguity.Release(flags);
                    ambiguity.Prepare(flags);
                }

                for (int i = 0; i < Flags.MAX_DONGLES_RSP1 + Flags.MAX_DONGLES_RTLSDR; i++)
                {
                    if (Regresion[i] != null)
                        Regresion[i].Initiate(flags);
                    if (RegresionMap[i] != null)
                        RegresionMap[i].Initiate(flags);
                }
            }
        }

        private void Resynchronise()
        {
            flags.Resynchronisation = true;
            StopAllRadios();
            // System.Threading.Thread.Sleep(10);
            StartAllRadios();
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            if (findRtlSdr.NrOfDevices == 0) return;
            if (runing == false)
            {

                //init buffers just in case
                InitBuffers();

                // if (CheckTheCorrelationRadarRate())
                {
                    runing = true;
                    WindowsUpdate();
                    StartAllRadios();
                    StartDraw();
                    button1.ImageIndex = 1;
                }
            }
            else
            {
                if (findRtlSdr.NrOfDevices == 0) return;
                // radarControl1.ActiveDeactivateColumnsControll(true);
                StopAllThreads();
                StopAllRadios();
                button1.ImageIndex = 0;
                runing = false;
            }
        }

        void StopAllRadios()
        {
            if (radioRtlSdr != null)
                for (int i = 0; i < Flags.MAX_DONGLES_RTLSDR; i++)
                {
                    if (radioRtlSdr[i] != null & radioRtlSdr[i].status)
                    {
                        radioRtlSdr[i].Stop();
                    }
                }

            if (radioRSP1 != null)
                for (int i = 0; i < Flags.MAX_DONGLES_RSP1; i++)
                {
                    if (radioRSP1[i] != null & radioRSP1[i].status)
                    {
                        radioRSP1[i].Stop();
                    }
                }
        }

        void StartAllRadios()
        {
            if (radioRtlSdr != null)
                for (int i = 0; i < Flags.MAX_DONGLES_RTLSDR; i++)
                {
                    if (radioRtlSdr[i] != null & radioRtlSdr[i].status)
                    {
                        radioRtlSdr[i].Start();
                    }
                }

            if (radioRSP1 != null)
                for (int i = 0; i < Flags.MAX_DONGLES_RSP1; i++)
                {
                    if (radioRSP1[i] != null & radioRSP1[i].status)
                    {
                        if (radioRSP1[i].exited == false)
                            radioRSP1[i].Stop();
                        radioRSP1[i].Start();
                    }
                }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            //Windwos position
            SaveState();

            StopDraw();
            ambiguity.Release(flags); //OpenCL AMD clear and close

            if (findRtlSdr.NrOfDevices == 0) return;
            if (radioRtlSdr != null)
                for (int i = 0; i < Flags.MAX_DONGLES_RTLSDR; i++)
                {
                    if (radioRtlSdr[i] != null)
                    {
                        radioRtlSdr[i].Stop();
                        radioRtlSdr[i].Close();
                    }
                }

            if (findRSP1.NrOfDevices == 0) return;
            if (findRSP1 != null)
                for (int i = 0; i < Flags.MAX_DONGLES_RSP1; i++)
                {
                    if (radioRSP1[i] != null)
                    {
                        radioRSP1[i].Stop();
                        radioRSP1[i].Close();
                    }
                }

            for (int i = 0; i < Flags.ALL_DONGLES; i++)
                windowRadar[i].service.ResetingDevice();

            foreach (WindowWave x in window_wave)
                x.service.ResetingDevice();
            foreach (WindowFlow x in window_flow)
                x.service.ResetingDevice();

            flags.Save();
        }

        #region Windwos position
        private void SaveState()
        {
            if (WindowState == FormWindowState.Normal)
            {
                Properties.Settings.Default.MainFormLocation = Location;
                Properties.Settings.Default.MainFormSize = Size;
            }
            else
            {
                Properties.Settings.Default.MainFormLocation = RestoreBounds.Location;
                Properties.Settings.Default.MainFormSize = RestoreBounds.Size;
            }
            Properties.Settings.Default.MainFormState = WindowState;
            Properties.Settings.Default.Save();

            Properties.Settings.Default.Spliter1 = splitContainer1.SplitterDistance;
            Properties.Settings.Default.Spliter2 = splitContainer2.SplitterDistance;
            Properties.Settings.Default.Spliter3 = splitContainer3.SplitterDistance;

            //Radio windows
            Properties.Settings.Default.ListFormLocation = "";
            Properties.Settings.Default.ListFormSize = "";
            Properties.Settings.Default.ListFormState = "";
            Properties.Settings.Default.ListFormShow = "";

            for (int i = 0; i < Flags.ALL_DONGLES; i++)
            {
                if (radio_window[i].WindowState == FormWindowState.Normal)
                {
                    Properties.Settings.Default.ListFormLocation += radio_window[i].Location.X + "," + radio_window[i].Location.Y + ",";
                    Properties.Settings.Default.ListFormSize += radio_window[i].Size.Width + "," + radio_window[i].Size.Height + ",";
                }
                else
                {
                    Properties.Settings.Default.ListFormLocation += radio_window[i].RestoreBounds.X + "," + radio_window[i].RestoreBounds.Y + ",";
                    Properties.Settings.Default.ListFormSize += radio_window[i].RestoreBounds.Size.Width + "," + radio_window[i].RestoreBounds.Size.Height + ",";
                }
                Properties.Settings.Default.ListFormState += radio_window[i].WindowState + ",";
                Properties.Settings.Default.ListFormShow += radio_window[i].Visible + ",";
            }
            Properties.Settings.Default.Save();
        }

        private void RestoreState()
        {
            if (Properties.Settings.Default.MainFormSize == new Size(0, 0))
            {
                return; // state has never been saved
            }
            StartPosition = FormStartPosition.Manual;
            Location = Properties.Settings.Default.MainFormLocation;
            Size = Properties.Settings.Default.MainFormSize;
            // I don't like an app to be restored minimized, even if I closed it that way
            WindowState = Properties.Settings.Default.MainFormState ==
              FormWindowState.Minimized ? FormWindowState.Normal : Properties.Settings.Default.MainFormState;

            //Spliters
            splitContainer1.SplitterDistance = Properties.Settings.Default.Spliter1;
            splitContainer2.SplitterDistance = Properties.Settings.Default.Spliter2;
            splitContainer3.SplitterDistance = Properties.Settings.Default.Spliter3;
            //Radio Windows
            //Position
            List<Point> PositionList = ConvertStringToPoint(Properties.Settings.Default.ListFormLocation);
            if (PositionList.Count > 0)
                for (int i = 0; i < Flags.ALL_DONGLES; i++)
                {
                    if (i < PositionList.Count)
                    {
                        radio_window[i].StartPosition = FormStartPosition.Manual;
                        radio_window[i].Location = PositionList[i];
                    }
                }

            //Size
            List<Size> SizeList = ConvertStringToSize(Properties.Settings.Default.ListFormSize);
            if (SizeList.Count > 0)
                for (int i = 0; i < Flags.ALL_DONGLES; i++)
                {
                    if (i < SizeList.Count)
                    {
                        radio_window[i].Size = SizeList[i];
                    }
                }

            //State
            List<FormWindowState> StateList = ConvertStringToFormWindowState(Properties.Settings.Default.ListFormState);
            if (StateList.Count > 0)
                for (int i = 0; i < Flags.ALL_DONGLES; i++)
                {
                    if (i < StateList.Count)
                    {
                        radio_window[i].WindowState = StateList[i];
                    }
                }

            List<bool> ShowList = ConvertStringToBolean(Properties.Settings.Default.ListFormShow);
            if (ShowList.Count > 0)
                for (int i = 0; i < Flags.ALL_DONGLES; i++)
                {
                    if (i < ShowList.Count)
                    {
                        radio_window[i].Visible = ShowList[i];
                    }
                }
        }

        List<bool> ConvertStringToBolean(string Str)
        {
            List<bool> S = new List<bool>();

            if (Str != null && Str.Length > 0)
            {
                foreach (string txt in Str.Split(','))
                    if (txt != "") S.Add(bool.Parse(txt));
            }
            return S;

        }

        List<Point> ConvertStringToPoint(string Str)
        {
            List<Point> P = new List<Point>();
            List<int> arr = new List<int>();

            if (Str != null && Str.Length > 1)
            {
                foreach (string txt in Str.Split(','))
                    if (txt != "") arr.Add(int.Parse(txt));

                for (int i = 0; i < Flags.ALL_DONGLES; i++)
                    if (i * 2 < arr.Count - 1)
                        P.Add(new Point(arr[i * 2], arr[i * 2 + 1]));
            }
            return P;
        }

        List<Size> ConvertStringToSize(string Str)
        {
            List<Size> S = new List<Size>();
            List<int> arr = new List<int>();

            if (Str != null && Str.Length > 1)
            {
                foreach (string txt in Str.Split(','))
                    if (txt != "") arr.Add(int.Parse(txt));
                //Now use ints to create List of Sizes
                for (int i = 0; i < Flags.ALL_DONGLES; i++)
                    if (i * 2 < arr.Count - 1)
                        S.Add(new Size(arr[i * 2], arr[i * 2 + 1]));
            }
            return S;
        }

        List<FormWindowState> ConvertStringToFormWindowState(string Str)
        {
            List<FormWindowState> S = new List<FormWindowState>();

            if (Str != null && Str.Length > 0)
            {
                foreach (string txt in Str.Split(','))
                    if (txt != "") S.Add((FormWindowState)Enum.Parse(typeof(FormWindowState), txt));
            }
            return S;
        }
        #endregion

        private void panelViewport_MouseLeave(object sender, EventArgs e)
        {
            for (int i = 0; i < Flags.ALL_DONGLES; i++)
                windowRadar[i].Location(-1);

            foreach (WindowWave x in window_wave)
                x.Location(-1, -1, -1);
            foreach (WindowFlow x in window_flow)
                x.Location(-1);
        }

        public void DisplaySettings2(int Nr, Flags LocalFlags)
        {
            flags.showRadioWave[Nr] = LocalFlags.showRadioWave[Nr];
            flags.showRadioFlow[Nr] = LocalFlags.showRadioFlow[Nr];
            flags.Amplification[Nr] = LocalFlags.Amplification[Nr];
            flags.Cumulation[Nr] = LocalFlags.Cumulation[Nr];
            flags.Level[Nr] = LocalFlags.Level[Nr];

            if (flags.BufferSizeRadio[Nr] != LocalFlags.BufferSizeRadio[Nr])
            {
                StopDraw();
                StopAllRadios();
                flags.BufferSizeRadio = LocalFlags.BufferSizeRadio;
                InitBuffers();
                if (runing)
                {
                    StartAllRadios();
                    StartDraw();
                }
            }

            WindowsUpdate();
        }

        public void DisplaySettings(Flags LocalFlags)
        {
            for (int ix = 0; ix < Flags.ALL_DONGLES; ix++)
                flags.showRadar[ix] = LocalFlags.showRadar[ix];

            flags.showBackground = LocalFlags.showBackground;

            flags.ColorTheme = LocalFlags.ColorTheme;
            flags.refresh_delay = LocalFlags.refresh_delay;
            flags.Radio_buffer_size = LocalFlags.Radio_buffer_size;
            displayControl1.UpdateTable(flags);

            if (flags.BufferSize != LocalFlags.BufferSize)
            {
                StopDraw();
                StopAllRadios();
                flags.BufferSizeRadio = LocalFlags.BufferSizeRadio;
                InitBuffers();
                if (runing)
                {
                    StartAllRadios();
                    StartDraw();
                }
            }
            WindowsUpdate();
        }

        //bool CheckTheCorrelationRadarRate()
        //{
        //    bool res = true;
        //    if (flags.showRadar && flags.rate[0] != flags.rate[1])
        //    {
        //        flags.showRadar = false;

        //        displayControl1.FalseCheckBoxes();
        //        res = false;
        //        StopAllThreads();
        //        StopAllRadios();
        //        MessageBox.Show("The radio 0 rate is different than the radio 1. Make them equal to start correlation or /and radar. ", "Info", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        //        button1.ImageIndex = 0;
        //        runing = false;
        //    }
        //    return res;
        //}


        //private void RadarMode()
        //{
        //    if (flags.showRadar)
        //    {
        //        //Reduce bandwith to the the highest working one
        //        if (flags.rate[0] > 2560000) flags.rate[0] = 2560000;
        //        //Set 1 and 2 radios to the same bandwith and buffer size
        //        if (flags.rate[1] != flags.rate[0] || flags.frequency[1] != flags.frequency[0])
        //        {
        //            //  StopAllThreads();
        //            flags.rate[1] = flags.rate[0];
        //            flags.frequency[1] = flags.frequency[0];
        //        }
        //    }
        //}

        /// <summary>
        /// Apply the radar settings from Radar settings panel
        /// </summary>
        /// <param name="LocalFlags"></param>
        private void RadarSettings(Flags LocalFlags)
        {
            // lock (LockMem)
            {
                flags.PasiveGain = LocalFlags.PasiveGain;
                flags.remove_symetrics = LocalFlags.remove_symetrics;
                flags.average = LocalFlags.average;
                flags.CorrectBackground = LocalFlags.CorrectBackground;
                flags.ColectEvery = LocalFlags.ColectEvery;
                flags.CorectionWeight = LocalFlags.CorectionWeight;
                flags.DistanceShift = LocalFlags.DistanceShift;
                flags.FreezeBackground = LocalFlags.FreezeBackground;
                flags.DopplerZoom = LocalFlags.DopplerZoom;
                flags.NrCorrectionPoints = LocalFlags.NrCorrectionPoints;
                flags.scale_type = LocalFlags.scale_type;
                flags.alpha = LocalFlags.alpha;
                flags.NrReduceRows = LocalFlags.NrReduceRows;

                if (runing == true)  //on running
                {
                    if (flags.BufferSize != LocalFlags.BufferSize ||
                        flags.Columns != LocalFlags.Columns ||
                        flags.Rows != LocalFlags.Rows ||
                        flags.NrCorrectionPoints != LocalFlags.NrCorrectionPoints
                        )
                    {
                        StopDraw();
                        for (int i = 0; i < Flags.MAX_DONGLES_RTLSDR; i++)
                            if (radioRtlSdr[i] != null)
                                radioRtlSdr[i].Stop();

                        for (int i = 0; i < Flags.MAX_DONGLES_RSP1; i++)
                            if (radioRSP1[i] != null)
                                radioRSP1[i].Stop();

                        flags.Columns = LocalFlags.Columns;
                        flags.Rows = LocalFlags.Rows;
                        flags.NrCorrectionPoints = LocalFlags.NrCorrectionPoints;
                        flags.BufferSize = LocalFlags.BufferSize;

                        InitBuffers();
                        for (int i = 0; i < Flags.MAX_DONGLES_RTLSDR; i++)
                            radioRtlSdr[i].Start();
                        for (int i = 0; i < Flags.MAX_DONGLES_RSP1; i++)
                            radioRSP1[i].Start();

                        StartDraw();
                    }
                }
                else
                {
                    if (flags.BufferSize != LocalFlags.BufferSize ||
                        flags.Columns != LocalFlags.Columns ||
                        flags.Rows != LocalFlags.Rows ||
                        flags.DopplerZoom != LocalFlags.DopplerZoom ||
                        flags.OpenCL != LocalFlags.OpenCL ||
                        flags.NrCorrectionPoints != LocalFlags.NrCorrectionPoints)
                    {
                        StopAllThreads();
                        radioRtlSdr[0].Stop();
                        radioRtlSdr[1].Stop();

                        flags.Columns = LocalFlags.Columns;
                        flags.Rows = LocalFlags.Rows;
                        flags.DopplerZoom = LocalFlags.DopplerZoom;
                        flags.NrCorrectionPoints = LocalFlags.NrCorrectionPoints;
                        flags.scale_type = LocalFlags.scale_type;
                        flags.BufferSize = LocalFlags.BufferSize;
                        InitBuffers();
                    }
                }
                WindowsUpdate();//To correct the scale and so on
            }
        }

        private int RadioCount()
        {
            int count = 0;
            for (int i = 0; i < Flags.MAX_DONGLES_RTLSDR; i++)
            {
                if (radioRtlSdr[i].status)
                    count++;
            }

            label3.Text = "Nr. recivers: " + count;

            if (count == 0)
                label3.Text = "Select reciver! ";

            flags.Nr_active_radio = count;//must  be before update all windows
            if (FlagsDelegate != null)
                FlagsDelegate(flags);

            return count;
        }

        private void TranslationSettings(Flags LocalFlags)
        {
            flags.AmplitudeOfAccepterPoints = LocalFlags.AmplitudeOfAccepterPoints;
            flags.NrPointsOfObject = LocalFlags.NrPointsOfObject;
            flags.DistanceFrom0line = LocalFlags.DistanceFrom0line;
            flags.AcceptedDistance = LocalFlags.AcceptedDistance;
            flags.Integration = LocalFlags.Integration;
        }

        void WindowsSetPressed(int Nr, string type)
        {
            UpdateSet(Nr, type);
            //  flags.Nr_active_radio=RadioCount();

        }

        private void StopAllThreads()
        {
            StopDraw();
        }



        private void AddRadioSDRplay(int WindowRadio, int Reciver)
        {
            string str = "None";
            lock (LockMem)
            {
                runing = false;
                button1.ImageIndex = 0;
                StopAllThreads();
                if (radioRSP1[WindowRadio] != null)
                {
                    radioRSP1[WindowRadio].Stop();
                    radioRSP1[WindowRadio].Close();
                    //First check if other Radio posses the dongle (item) and free it
                    for (int i = 0; i < Flags.MAX_DONGLES_RSP1; i++)

                        if (radioRSP1[i].item == Reciver)
                        {
                            radioRSP1[i].Stop();
                            radioRSP1[i].Close();
                            findRSP1.StatusList[radioRSP1[i].item] = 0;
                            //radioSDRplays[i].itm = 0;
                            radioRSP1[i].item = 0;
                            radio_window[i].label_Radio.Text = "None";
                        }

                    findRSP1.StatusList[radioRtlSdr[WindowRadio].item] = 0;//free the previously used dongle
                    radioRSP1[WindowRadio].item = Reciver;// change entry in radio to the currrent
                    radioRSP1[WindowRadio].dev_number = findRSP1.List[Reciver];
                }
                findRSP1.StatusList[Reciver] = 1;

                if (Reciver > 0)
                {
                    if (radioRSP1[WindowRadio] != null)
                    {
                        radioRSP1[WindowRadio].BufferSize = (int)flags.BufferSize;
                        radioRSP1[WindowRadio].frequency = (int)flags.frequency[WindowRadio];
                        radioRSP1[WindowRadio].rate = (uint)flags.rate[WindowRadio];
                        radioRSP1[WindowRadio].Open();

                        if (radioRSP1[WindowRadio].status)
                            str = "(" + Reciver + ") " + radioRSP1[WindowRadio].GetName();
                    }
                }
                //RadioCount();
            }
            //Update lists in combo boxes
            for (int i = 0; i < Flags.MAX_DONGLES_RSP1; i++)
            {
                setSDRplays[i].ComboBoxRadio_Update(ref findRSP1);
                //Update parameters
                setSDRplays[i].SetSettings(radioRSP1[i]);
            }
            //Set text in windowRadio
            radio_window[WindowRadio].label_Radio.Text = str;
        }

        private void AddRadio(int WindowRadio, int Reciver)
        {
            string str = "None";
            lock (LockMem)
            {
                runing = false;
                button1.ImageIndex = 0;
                StopAllThreads();
                if (radioRtlSdr[WindowRadio] != null)
                {
                    radioRtlSdr[WindowRadio].Stop();
                    radioRtlSdr[WindowRadio].Close();
                    //First check if other Radio posses the dongle (item) and free it
                    for (int i = 0; i < Flags.MAX_DONGLES_RTLSDR; i++)

                        if (radioRtlSdr[i].item == Reciver)
                        {
                            radioRtlSdr[i].Stop();
                            radioRtlSdr[i].Close();
                            findRtlSdr.StatusList[radioRtlSdr[i].item] = 0;
                            setRtlSdr[i].itm = 0;
                            radioRtlSdr[i].item = 0;
                            radio_window[i].label_Radio.Text = "None";
                        }

                    findRtlSdr.StatusList[radioRtlSdr[WindowRadio].item] = 0;//free the previously used dongle
                    radioRtlSdr[WindowRadio].item = Reciver;// change entry in radio to the currrent
                    radioRtlSdr[WindowRadio].dev_number = findRtlSdr.List[Reciver];
                }
                findRtlSdr.StatusList[Reciver] = 1;

                if (Reciver > 0)
                {
                    if (radioRtlSdr[WindowRadio] != null)
                    {
                        radioRtlSdr[WindowRadio].BufferSize = (int)flags.BufferSize;
                        radioRtlSdr[WindowRadio].frequency = (int)flags.frequency[WindowRadio];
                        radioRtlSdr[WindowRadio].rate = (uint)flags.rate[WindowRadio];
                        radioRtlSdr[WindowRadio].Open();

                        if (radioRtlSdr[WindowRadio].status)
                            str = "(" + Reciver + ") " + radioRtlSdr[WindowRadio].GetName();
                    }

                }
                RadioCount();
            }
            //Update lists in combo boxes
            for (int i = 0; i < Flags.MAX_DONGLES_RTLSDR; i++)
            {
                setRtlSdr[i].ComboBoxRadio_Update(ref findRtlSdr);
                //Update parameters
                setRtlSdr[i].SetSettings(radioRtlSdr[i]);
            }
            //Set text in windowRadio
            radio_window[WindowRadio].label_Radio.Text = str;
        }

        private void AddRadioSDR1(int WindowRadio, int Reciver)
        {
            string str = "None";
            lock (LockMem)
            {
                runing = false;
                button1.ImageIndex = 0;
                StopAllThreads();
                if (radioRSP1[WindowRadio] != null)
                {
                    radioRSP1[WindowRadio].Stop();
                    radioRSP1[WindowRadio].Close();
                    //First check if other Radio posses the dongle (item) and free it
                    for (int i = 0; i < Flags.MAX_DONGLES_RSP1; i++)

                        if (radioRSP1[i].item == Reciver)
                        {
                            radioRSP1[i].Stop();
                            radioRSP1[i].Close();
                            findRSP1.StatusList[radioRSP1[i].item] = 0;
                            // radioSDRplays[i].itm = 0;
                            radioRSP1[i].item = 0;
                            radio_window[i].label_Radio.Text = "None";
                        }

                    findRtlSdr.StatusList[radioRSP1[WindowRadio].item] = 0;//free the previously used dongle
                    radioRSP1[WindowRadio].item = Reciver;// change entry in radio to the currrent
                    radioRSP1[WindowRadio].dev_number = findRSP1.List[Reciver];
                }
                findRSP1.StatusList[Reciver] = 1;

                if (Reciver > 0)
                {
                    if (radioRtlSdr[WindowRadio] != null)
                    {
                        radioRSP1[WindowRadio].BufferSize = (int)flags.BufferSize;
                        radioRSP1[WindowRadio].frequency = (int)flags.frequency[WindowRadio + Flags.MAX_DONGLES_RTLSDR];
                        radioRSP1[WindowRadio].rate = (uint)flags.rate[WindowRadio + Flags.MAX_DONGLES_RTLSDR];
                        radioRSP1[WindowRadio].Open();

                        if (radioRSP1[WindowRadio].status)
                            str = "(" + Reciver + ") " + radioRSP1[WindowRadio].GetName();
                    }

                }
                //  RadioCount();
            }
            //Update lists in combo boxes
            for (int i = 0; i < Flags.MAX_DONGLES_RSP1; i++)
            {
                setSDRplays[i].ComboBoxRadio_Update(ref findRSP1);
                //Update parameters
                setSDRplays[i].SetSettings(radioRSP1[i]);
            }
            //Set text in windowRadio
            radio_window[WindowRadio].label_Radio.Text = str;
        }

        void UpdateSet(int n, string type)
        {
            if (type == "RTLSDR")
            {
                if (setRtlSdr[n].Visible == false)
                {
                    setRtlSdr[n].Show();
                    setRtlSdr[n].ComboBoxRadio_Update(ref findRtlSdr);
                }
                else
                    setRtlSdr[n].Visible = false;
            }
            else if (type == "SDRPLAY")
            {
                n -= (int)Flags.MAX_DONGLES_RTLSDR;
                if (setSDRplays[n].Visible == false)
                {
                    setSDRplays[n].Show();
                    setSDRplays[n].ComboBoxRadio_Update(ref findRSP1);
                }
                else
                    setSDRplays[n].Visible = false;
            }
        }

        void ReturnRadioSettingsRtlSdr(int Nr, int gain, uint _rate, bool AGC, bool MGC, bool ShiftOn, int shift, int sampling, bool dithering, bool StagesFlag, int[] StageGain)
        {
            if (radioRtlSdr[Nr] != null)
            {
                if (StagesFlag)
                {
                    for (uint stage = 0; stage < 3; stage++)
                        radioRtlSdr[Nr].SetTunerStageGain(stage, StageGain[stage]);
                }
                else
                    radioRtlSdr[Nr].SetGain(gain);

                radioRtlSdr[Nr].GainMode(MGC);
                radioRtlSdr[Nr].AGCMode(AGC);
                radioRtlSdr[Nr].SetDithering(dithering);
                radioRtlSdr[Nr].SetSampleRate(_rate);

                flags.rate[Nr] = _rate;

                if (ShiftOn)
                    radioRtlSdr[Nr].SetFreqCorrection(shift);
                else
                    radioRtlSdr[Nr].SetFreqCorrection(0);

                radioRtlSdr[Nr].SetDirectSampling(sampling);
            }
            UpdateFrequencies(Nr);

            //     flags.Nr_active_radio = RadioCount();
        }

        void ReturnRadioSettingsRSP1(int Nr, int gain, uint rate, uint bandwith, uint IF_frequency, bool AGC, bool MGC, bool ShiftOn, int shift, int format, int gain_LNA, int gain_Mixer, int gain_Baseband, int transfer)
        {
            if (radioRSP1[Nr] != null)
            {
                int r;
                //gain
                if (!AGC)
                {
                    if (radioRSP1[Nr].gain != gain)
                    {
                        r = radioRSP1[Nr].SetTunerGein(gain);
                        radioRSP1[Nr].GetLNAGain();
                        radioRSP1[Nr].GetMixerGain();
                        radioRSP1[Nr].GetBasebandGain();
                    }
                    else if (radioRSP1[Nr].gainLNA != gain_LNA)
                    {
                        r = radioRSP1[Nr].SetLNAGain(gain_LNA);
                        radioRSP1[Nr].GetGain();
                    }
                    else if (radioRSP1[Nr].gainMixer != gain_Mixer)
                    {
                        r = radioRSP1[Nr].SetMixerGain(gain_Mixer);
                        radioRSP1[Nr].GetGain();
                    }
                    else if (radioRSP1[Nr].gainBaseBand != gain_Baseband)
                    {
                        radioRSP1[Nr].SetBasebandGain(gain_Baseband);
                        radioRSP1[Nr].GetGain();
                    }
                }

                radioRSP1[Nr].SetCentralFreq();
                radioRSP1[Nr].SetFreqCorrection(shift);
                if (radioRSP1[Nr].IF_frequency != IF_frequency)
                    radioRSP1[Nr].Set_if_frequency(IF_frequency);


                //Memory sensitive functions/parameters
                if (runing)
                    if (radioRSP1[Nr].format != RadioRSP1.Sample_format(format) ||
                        radioRSP1[Nr].bandwith != bandwith ||
                        radioRSP1[Nr].rate != rate ||
                        radioRSP1[Nr].transfer != transfer ||
                        radioRSP1[Nr].gain_mode != AGC
                        )
                    {
                        radioRSP1[Nr].Stop();

                        Thread.Sleep(10);
                        radioRSP1[Nr].ResetBuffer();

                        if (radioRSP1[Nr].gain_mode != AGC)
                            radioRSP1[Nr].Set_tuner_gain_mode(AGC);//Automatic manual gain mode


                        if (radioRSP1[Nr].format != RadioRSP1.Sample_format(format))
                            radioRSP1[Nr].Set_sample_format(format);

                        if (radioRSP1[Nr].bandwith != bandwith)
                        {
                            radioRSP1[Nr].Set_bandwidth(bandwith);
                            flags.bandwitch[Nr + Flags.MAX_DONGLES_RTLSDR] = bandwith;
                        }

                        if (radioRSP1[Nr].rate != rate)
                        {
                            radioRSP1[Nr].SetSampleRate(rate);
                            flags.rate[Nr + Flags.MAX_DONGLES_RTLSDR] = rate;
                        }

                        if (radioRSP1[Nr].transfer != transfer)
                            radioRSP1[Nr].Set_usb_transfer(transfer);

                        Thread.Sleep(10);
                        radioRSP1[Nr].Start();
                    }
                    else
                    {
                        radioRSP1[Nr].Set_bandwidth(bandwith);
                        radioRSP1[Nr].Set_sample_format(format);
                        radioRSP1[Nr].SetSampleRate(rate);
                        flags.rate[Nr + Flags.MAX_DONGLES_RTLSDR] = rate;
                        flags.bandwitch[Nr + Flags.MAX_DONGLES_RTLSDR] = bandwith;
                        radioRSP1[Nr].Set_usb_transfer(transfer);
                        radioRSP1[Nr].Set_tuner_gain_mode(AGC);//Automatic manual gain mode
                        radioRSP1[Nr].Set_if_frequency(IF_frequency);
                    }


                if (ShiftOn)
                    radioRSP1[Nr].SetFreqCorrection(shift);
                else
                    radioRSP1[Nr].SetFreqCorrection(0);

                //send back the changes
                setSDRplays[Nr].SetSettings(radioRSP1[Nr]);
            }

            UpdateFrequencies(Nr);
        }


        void OnPowerChange(Object sender, Microsoft.Win32.PowerModeChangedEventArgs e)
        {
            switch (e.Mode)
            {
                case Microsoft.Win32.PowerModes.Suspend:
                    StopAllThreads();
                    break;
                case Microsoft.Win32.PowerModes.Resume:
                    UpdateAllScenesWhenRunning();
                    break;
            }
        }

        void UpdateFrequencies(int Nr)
        {
            if (radioRtlSdr == null) return;

            if (Nr == -1) Nr = flags.LastActiveWindowRadio;
            flags.LastActiveWindowRadio = Nr;

            for (int i = 0; i < Flags.ALL_DONGLES; i++)
            {
                radio_window[Nr].tuningNumber.frequency = (int)flags.frequency[Nr];
                radio_window[Nr].tuningNumber.Update_(false);
            }

            for (int i = 0; i < Flags.MAX_DONGLES_RTLSDR; i++)
            {
                if (radioRtlSdr[i] != null)
                {
                    radioRtlSdr[i].frequency = (int)flags.frequency[i];
                    radioRtlSdr[i].SetCentralFreq();
                }
            }

            if (radioRSP1 == null) return;
            for (int i = 0; i < Flags.MAX_DONGLES_RSP1; i++)
            {
                if (radioRSP1[i] != null)
                {
                    radioRSP1[i].frequency = (int)flags.frequency[i + Flags.MAX_DONGLES_RTLSDR];
                    radioRSP1[i].SetCentralFreq();
                }
            }

            WindowsLocation(-1);
            WindowsUpdate();
        }

        private void WindowsUpdate()
        {
            for (int i = 0; i < Flags.ALL_DONGLES; i++)
                if (windowRadar[i] != null)
                    windowRadar[i].Update(flags);

            if (FlagsDelegate != null)
                FlagsDelegate(flags);

            foreach (WindowWave x in window_wave)
                if (x != null) x.Update(flags);
            foreach (WindowFlow x in window_flow)
                if (x != null) x.Update(flags);

            if (!runing)
                UpdateAllScenes();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (flags.ButtonDisplay)
            {
                button5.ImageIndex = 1;
                flags.ButtonDisplay = false;
                displayControl1.Visible = false;
            }
            else
            {
                button5.ImageIndex = 0;
                flags.ButtonDisplay = true;
                displayControl1.Visible = true;
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (flags.ButtonRadar)
            {
                button7.ImageIndex = 1;
                flags.ButtonRadar = false;
                radarControl1.Visible = false;
            }
            else
            {
                button7.ImageIndex = 0;
                flags.ButtonRadar = true;
                radarControl1.Visible = true;
            }
        }

        void InitSiteMenu()
        {
            if (flags.ButtonDisplay)
            {
                button5.ImageIndex = 1;
                flags.ButtonDisplay = false;
                displayControl1.Visible = false;
            }
            else
            {
                button5.ImageIndex = 0;
                flags.ButtonDisplay = true;
                displayControl1.Visible = true;
            }

            if (flags.ButtonRadar)
            {
                button7.ImageIndex = 1;
                flags.ButtonRadar = false;
                radarControl1.Visible = false;
            }
            else
            {
                button7.ImageIndex = 0;
                flags.ButtonRadar = true;
                radarControl1.Visible = true;
            }

            if (flags.ButtonCorrelation)
            {
                button9.ImageIndex = 1;
                flags.ButtonCorrelation = false;
                translateControl1.Visible = false;
            }
            else
            {
                button9.ImageIndex = 0;
                flags.ButtonCorrelation = true;
                translateControl1.Visible = true;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < Flags.ALL_DONGLES; i++)
            {
                radio_window[i].Show();
            }
        }

        private void radarControl1_Load(object sender, EventArgs e)
        {

        }

        private void Form1_ResizeEnd(object sender, EventArgs e)
        {
            resizing = false;
            UpdateAllScenesWhenRunning();
            WindowsUpdate();
        }

        private void Form1_ResizeBegin(object sender, EventArgs e)
        {
            resizing = true;
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            resizing = true;
        }

        private void buttonSetSettings1_Click(object sender, EventArgs e)
        {
            save_flags1.Copy(flags);
        }

        private void buttonSetSettings2_Click(object sender, EventArgs e)
        {
            save_flags2.Copy(flags);
        }

        private void buttonSettings1_Click(object sender, EventArgs e)
        {
            bool tmp_runing = runing;
            StopAllThreads();
            StopAllRadios();
            runing = false;

            //////////////////////////////////////////
            flags.Copy(save_flags1);
            if (FlagsDelegate != null)
                FlagsDelegate(flags);

            System.Threading.Thread.Sleep(100);//wait for changes, not the best solution
            radarControl1.UpdateAllControls();
            translateControl1.UpdateAllControls();
            displayControl1.UpdateAllControls();
            for (int i = 0; i < Flags.ALL_DONGLES; i++)
            {
                radio_window[i].UpdateAllControls();
            }
            InitBuffers();
            WindowsUpdate();
            ////////////////////////////////////////////
            if (tmp_runing)
                runing = true;
            StartAllRadios();
            StartDraw();
        }

        private void buttonSettings2_Click(object sender, EventArgs e)
        {
            bool tmp_runing = runing;
            StopAllThreads();
            StopAllRadios();
            runing = false;

            ////////////////////////////////////////////////
            flags.Copy(save_flags2);
            // UpdateFrequencies(flags.LastActiveWindowRadio);
            if (FlagsDelegate != null)
                FlagsDelegate(flags);

            System.Threading.Thread.Sleep(100);
            radarControl1.UpdateAllControls();
            translateControl1.UpdateAllControls();
            displayControl1.UpdateAllControls();
            for (int i = 0; i < Flags.ALL_DONGLES; i++)
            {
                radio_window[i].UpdateAllControls();
            }
            InitBuffers();
            WindowsUpdate();

            if (tmp_runing)
                runing = true;
            StartAllRadios();
            StartDraw();
        }


        private void splitContainer3_SplitterMoved(object sender, SplitterEventArgs e)
        {
            //   UpdateAllScenesWhenRunning();
        }

        private void splitContainer1_SplitterMoved(object sender, SplitterEventArgs e)
        {
            //  UpdateAllScenesWhenRunning();
        }

        private void splitContainer2_SplitterMoved(object sender, SplitterEventArgs e)
        {
            //  UpdateAllScenesWhenRunning();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (flags.ShowMap == false)
            {
                mMap = new Map();
                mMap.Show();
                if (mMap != null)
                    mMap.CopyFlags(flags);
                flags.ShowMap = true;
            }
            else
            {
                flags.ShowMap = false;
                if (mMap != null)
                    mMap.Close();
            }
        }

        private void Form1_MaximumSizeChanged(object sender, EventArgs e)
        {
            resizing = false;
            UpdateAllScenesWhenRunning();
            WindowsUpdate();
        }

        private void panelViewport1_SizeChanged(object sender, EventArgs e)
        {
            resizing = false;
        }

        private void panelViewport1_ParentChanged(object sender, EventArgs e)
        {
            //resizing = false;
            //UpdateAllScenesWhenRunning();
            //WindowsUpdate();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            label2.Text = Flags.version;
            UpdateFrequencies(flags.LastActiveWindowRadio);
            if (FlagsDelegate != null)
                FlagsDelegate(flags);
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            FormsReady = true;
            resizing = false;
            UpdateAllScenes();
        }

        private void Form1_ClientSizeChanged(object sender, EventArgs e)
        {
            resizing = false;

        }

        private void splitContainer3_MouseUp(object sender, MouseEventArgs e)
        {
            UpdateAllScenesWhenRunning();
        }

        private void splitContainer2_MouseUp(object sender, MouseEventArgs e)
        {
            UpdateAllScenesWhenRunning();
        }

        private void splitContainer1_MouseUp(object sender, MouseEventArgs e)
        {
            UpdateAllScenesWhenRunning();
        }

        private void splitContainer4_MouseUp(object sender, MouseEventArgs e)
        {
            UpdateAllScenesWhenRunning();
        }

        private void button9_Click(object sender, EventArgs e)
        {

            if (flags.ButtonCorrelation)
            {
                button9.ImageIndex = 1;
                flags.ButtonCorrelation = false;
                translateControl1.Visible = false;
            }
            else
            {
                button9.ImageIndex = 0;
                flags.ButtonCorrelation = true;
                translateControl1.Visible = true;
            }
        }
    }
}
