﻿using System;
using System.Windows.Forms;

namespace PasiveRadar
{
    public partial class WindowsRadio : Form
    {
        //Sending flags info to all clases
        public delegate void DelegateEvents(int Nr);
        public delegate void DelegateEventsSettings(int Nr, string type);
        public static event DelegateEvents SizeChangedx;
        public static event DelegateEventsSettings RadioSet;
        public static event DelegateEventsSettings RadioFrequencyChanged;

        public delegate void DelegateMouse(int Nr, int x);
        public static event DelegateMouse RadioMouseDown;
        public static event DelegateMouse RadioMouseMove;
        public static event DelegateMouse RadioMouseUp;

        public delegate void MyDelegate(int Nr, Flags LocalFlags);
        public static event MyDelegate EventSettings;

        private bool SET;
        private int Nr;
        public uint BufferSizeRadio;
        public string type = "";  //Type of radio RTLSDR or SDRplay
        private bool listen_enabled = false;
        public bool radio_resize;

        public WindowsRadio(int _Nr, string _type)
        {
            InitializeComponent();
            type = _type;
            Nr = (int)_Nr;

            if (Nr > Flags.MAX_DONGLES_RTLSDR - 1)
                this.Text = "Radio  " + Nr + "  RSP1 ";
            else
                this.Text = "Radio  " + Nr + "  RtlSdr ";


            //Get info from TuningNumber
            tuningNumber.DigitalNumberChange += new TuningNumber.MyDelegate(TuningNumberChanged);
        }

        private void TuningNumberChanged()
        {
            if (RadioFrequencyChanged != null)
                RadioFrequencyChanged(Nr, type);
        }

        private void splitContainer_SplitterMoved(object sender, SplitterEventArgs e)
        {
            radio_resize = false;
            //if (SizeChangedx != null)
            //    SizeChangedx(Nr);
        }

        private void WindowsRadio_ResizeEnd(object sender, EventArgs e)
        {
            radio_resize = false;
            if (SizeChangedx != null)
                SizeChangedx(Nr);
        }

        private void panelRadioWave_MouseDown(object sender, MouseEventArgs e)
        {
            if (RadioMouseDown != null)
                RadioMouseDown(Nr, e.Location.X);
        }

        private void panelRadioWave_MouseMove(object sender, MouseEventArgs e)
        {
            if (RadioMouseMove != null)
                RadioMouseMove(Nr, e.Location.X);
        }

        private void panelRadioWave_MouseUp(object sender, MouseEventArgs e)
        {
            if (RadioMouseUp != null)
                RadioMouseUp(Nr, e.Location.X);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (RadioSet != null)
                RadioSet(Nr, type);
        }

        private void WindowsRadio_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Visible = false;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            SendSettings();
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            SendSettings();
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            SendSettings();
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            SendSettings();
        }

        private void trackBar3_Scroll(object sender, EventArgs e)
        {
            SendSettings();
        }
        void SendSettings()
        {
            if (!SET) return;
            Flags LocalFlags = new Flags();

            LocalFlags.BufferSizeRadio[Nr] = BufferSizeRadio;
            LocalFlags.showRadioWave[Nr] = checkBox1.Checked;
            LocalFlags.showRadioFlow[Nr] = checkBox2.Checked;

            LocalFlags.Amplification[Nr] = trackBar1.Value;
            LocalFlags.Level[Nr] = trackBar2.Value - 1000;
            LocalFlags.Cumulation[Nr] = (uint)trackBar3.Value;

            label2.Text = "" + LocalFlags.Amplification[Nr];
            label5.Text = "" + LocalFlags.Level[Nr];
            label6.Text = "" + (int)LocalFlags.Cumulation[Nr];

            if (EventSettings != null)
                EventSettings(Nr, LocalFlags);
        }

        public void UpdateButtonListen(int ActiveRadioLeasyn)
        {
            if (ActiveRadioLeasyn == Nr)
            {
                listen_enabled = true;
                button1.ImageIndex = 0;
            }
            else
            {
                listen_enabled = false;
                button1.ImageIndex = 1;
            }
        }

        public void Initialize(Flags flags)
        {
            SET = false;
            Flags LocalFlags = new Flags();

            LocalFlags.Amplification[Nr] = flags.Amplification[Nr];
            LocalFlags.Cumulation[Nr] = flags.Cumulation[Nr];
            LocalFlags.Level[Nr] = flags.Level[Nr] + 1000;

            LocalFlags.showRadioWave[Nr] = flags.showRadioWave[Nr];
            LocalFlags.showRadioFlow[Nr] = flags.showRadioFlow[Nr];
            LocalFlags.BufferSizeRadio = flags.BufferSizeRadio;

            checkBox1.Checked = LocalFlags.showRadioWave[Nr];
            checkBox2.Checked = LocalFlags.showRadioFlow[Nr];

            trackBar1.Value = LocalFlags.Amplification[Nr];
            trackBar2.Value = LocalFlags.Level[Nr];
            trackBar3.Value = (int)LocalFlags.Cumulation[Nr];

            label2.Text = "" + LocalFlags.Amplification[Nr];
            label5.Text = "" + LocalFlags.Level[Nr];
            label6.Text = "" + (int)LocalFlags.Cumulation[Nr];

            if (LocalFlags.BufferSizeRadio[Nr] == 256) comboBox1.SelectedIndex = 0;
            if (LocalFlags.BufferSizeRadio[Nr] == 512) comboBox1.SelectedIndex = 1;
            if (LocalFlags.BufferSizeRadio[Nr] == 1024) comboBox1.SelectedIndex = 2;
            if (LocalFlags.BufferSizeRadio[Nr] == 2048) comboBox1.SelectedIndex = 3;
            if (LocalFlags.BufferSizeRadio[Nr] == 4096) comboBox1.SelectedIndex = 4;
            if (LocalFlags.BufferSizeRadio[Nr] == 8192) comboBox1.SelectedIndex = 5;

            SET = true;
        }

        public void UpdateAllControls()
        {
            checkBox1.Update();
            checkBox2.Update();
            trackBar1.Update();
            trackBar2.Update();
            trackBar3.Update();
            label2.Update();

            label5.Update();
            label6.Update();
            comboBox1.Update();

            tuningNumber.Update_(false);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (listen_enabled == false)//if no, turn it on 
            {
                listen_enabled = true;
                button1.ImageIndex = 1;
            }
            else
            {
                listen_enabled = false;
                button1.ImageIndex = 0;
            }
            SendSettings();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex == 0) BufferSizeRadio = 256;
            if (comboBox1.SelectedIndex == 1) BufferSizeRadio = 512;
            if (comboBox1.SelectedIndex == 2) BufferSizeRadio = 1024;
            if (comboBox1.SelectedIndex == 3) BufferSizeRadio = 2048;
            if (comboBox1.SelectedIndex == 4) BufferSizeRadio = 4096;
            if (comboBox1.SelectedIndex == 5) BufferSizeRadio = 8192;
            SendSettings();
        }

        private void WindowsRadio_Resize(object sender, EventArgs e)
        {
            radio_resize = true;
        }

        private void WindowsRadio_ResizeBegin(object sender, EventArgs e)
        {
            radio_resize = true;
        }

        private void WindowsRadio_Load(object sender, EventArgs e)
        {
            radio_resize = false;
            if (SizeChangedx != null)
                SizeChangedx(Nr);
        }

        private void splitContainer1_SizeChanged(object sender, EventArgs e)
        {
            radio_resize = false;
            //if (SizeChangedx != null)
            //    SizeChangedx(Nr);
        }

        private void splitContainer2_SizeChanged(object sender, EventArgs e)
        {
            radio_resize = false;
            //if (SizeChangedx != null)
            //    SizeChangedx(Nr);
        }

        private void splitContainer_Resize(object sender, EventArgs e)
        {
            radio_resize = true;
        }

        private void splitContainer_SizeChanged(object sender, EventArgs e)
        {
            radio_resize = false;
        }

        private void splitContainer1_SplitterMoving(object sender, SplitterCancelEventArgs e)
        {
            radio_resize = true;
        }

        private void splitContainer2_SplitterMoving(object sender, SplitterCancelEventArgs e)
        {
            radio_resize = true;
        }

        private void splitContainer1_MouseUp(object sender, MouseEventArgs e)
        {
            radio_resize = false;
            if (SizeChangedx != null)
                SizeChangedx(Nr);
        }

        private void splitContainer2_MouseUp(object sender, MouseEventArgs e)
        {
            radio_resize = false;
            if (SizeChangedx != null)
                SizeChangedx(Nr);
        }
 
    }
}
