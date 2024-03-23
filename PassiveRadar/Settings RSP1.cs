using System;
using System.Windows.Forms;

namespace PasiveRadar
{
    public partial class Settings_RSP1 : Form
    {
        int TunerNr; //Inform for which radio the window is assigned
        public int itm;


        public static object Default { get; internal set; }

        public delegate void MyDelegate(int Radio, int item);
        public delegate void MyDelegateSettings(int index, int gain_index, uint rate, uint bandwith, uint IF_freq, bool AGC, bool MGC, bool OffsetTuning, int FrequencyCorrection, int sampling, int gain_LNA, int gain_Mixer, int gain_Baseband, int transfer);
        public static event MyDelegate EventRadio;


        public static event MyDelegateSettings EventGain;

        public Settings_RSP1(int _TunerNr)
        {
            InitializeComponent();
            TunerNr = _TunerNr;

            comboBox2.SelectedIndex = 0;
            comboBox3.SelectedIndex = 0;

        }

        public void SetSettings(RadioRSP1 radio)
        {
            //Rate
            if (radio == null) return;
            uint rate = radio.rate;
            uint bandwith = radio.bandwith;
            DLL_RSP1.format_t format = radio.format;


            //xtl
            label15.Text = "xtal: " + radio.ctx_frequency / 1000000 + " MHz";

            //RF gain
            trackBar1.Value = radio.gain;
            label1.Text = "" + radio.gain;

            //Gain stages
            if (radio.gainLNA > 0)
            {
                trackBar2.Value = 1;
                label2.Text = "24 dB";
            }
            else
            {
                trackBar2.Value = 0;
                label2.Text = "0 dB";
            }

            if (radio.gainMixer > 0)
            {
                trackBar3.Value = 1;
                label3.Text = "19 dB";
            }
            else
            {
                trackBar3.Value = 0;
                label3.Text = "0 dB";
            }

            trackBar4.Value = radio.gainBaseBand;
            label4.Text = "" + trackBar4.Value + " dB";

            int itm = 0;
            if (comboBox2.SelectedIndex != rate)
            {
                switch (rate)
                {
                    case 2000000:
                        itm = 0;
                        break;
                    case 3000000:
                        itm = 1;
                        break;
                    case 4000000:
                        itm = 2;
                        break;
                    case 5000000:
                        itm = 3;
                        break;
                    case 6000000:
                        itm = 4;
                        break;
                    case 7000000:
                        itm = 5;
                        break;
                    case 8000000:
                        itm = 6;
                        break;
                    case 9000000:
                        itm = 7;
                        break;
                    case 10000000:
                        itm = 8;
                        break;
                }
                comboBox2.SelectedIndex = itm;
            }
            //if (comboBox3.SelectedIndex!=format)
            switch (format)
            {
                case DLL_RSP1.format_t.MIRISDR_FORMAT_252_S16:
                    itm = 0;
                    break;
                case DLL_RSP1.format_t.MIRISDR_FORMAT_336_S16:
                    itm = 1;
                    break;
                case DLL_RSP1.format_t.MIRISDR_FORMAT_384_S16:
                    itm = 2;
                    break;
                case DLL_RSP1.format_t.MIRISDR_FORMAT_504_S16:
                    itm = 3;
                    break;
                case DLL_RSP1.format_t.MIRISDR_FORMAT_504_S8:
                    itm = 4;
                    break;
                case DLL_RSP1.format_t.MIRISDR_FORMAT_AUTO_ON:
                    itm = 5;
                    break;
            }
            comboBox3.SelectedIndex = itm;

            if (comboBox4.SelectedIndex != bandwith)
            {
                switch (bandwith)
                {
                    case 200000:
                        itm = 0;
                        break;
                    case 300000:
                        itm = 1;
                        break;
                    case 600000:
                        itm = 2;
                        break;
                    case 1536000:
                        itm = 3;
                        break;
                    case 5000000:
                        itm = 4;
                        break;
                    case 6000000:
                        itm = 5;
                        break;
                    case 7000000:
                        itm = 6;
                        break;
                    case 8000000:
                        itm = 7;
                        break;
                }
                comboBox4.SelectedIndex = itm;
            }

            switch (radio.band)
            {
                case DLL_RSP1.mirisdr_band_t.MIRISDR_BAND_AM1:
                    label5.Text = "AM1";
                    break;
                case DLL_RSP1.mirisdr_band_t.MIRISDR_BAND_AM2:
                    label5.Text = "AM2";
                    break;
                case DLL_RSP1.mirisdr_band_t.MIRISDR_BAND_VHF:
                    label5.Text = "VHF";
                    break;
                case DLL_RSP1.mirisdr_band_t.MIRISDR_BAND_3:
                    label5.Text = "BAND 3";
                    break;
                case DLL_RSP1.mirisdr_band_t.MIRISDR_BAND_45:
                    label5.Text = "BAND 45";
                    break;
                case DLL_RSP1.mirisdr_band_t.MIRISDR_BAND_L:
                    label5.Text = "Band L";
                    break;
            }
            comboBox4.SelectedIndex = itm;

            itm = 0;
            uint IF_freq = radio.IF_frequency;
            //Rate
            switch (IF_freq)
            {
                case 0:
                    itm = 0;
                    break;
                case 450000:
                    itm = 1;
                    break;
                case 1620000:
                    itm = 2;
                    break;
                case 2048000:
                    itm = 3;
                    break;
            }
            comboBox5.SelectedIndex = itm;

            label10.Text = "" + radio.GetName();

            if (radio.tt == DLL_RSP1.transfer_t.MIRISDR_TRANSFER_BULK)
            {
                checkBox2.Checked = true; //Bulk mode
            }
            else { checkBox2.Checked = false; }//isoc mode

        }


        private void TrackBar1_Scroll(object sender, EventArgs e)
        {
            SendSettings();
        }

        private void Settings1_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Visible = false;
        }

        public void ComboBoxRadio_Update(ref FindRSP1 find)
        {
            comboBox1.Items.Clear();

            string str = "";

            //Add devices to the list
            for (int i = 0; i < find.NrOfDevices; i++)
            {
                if (find.StatusList[i] == 1)//is in use?
                    str = "*" + find.NameList[i];
                else
                    str = find.NameList[i];
                comboBox1.Items.Add(str);
            }
            comboBox1.SelectedIndex = itm;
        }


        void SendSettings()
        {
            int gain_LNA, gain_Mixer, gain_Baseband;

            uint rate = 1;
            int gain = trackBar1.Value;
            label1.Text = "" + trackBar1.Value;

            if (trackBar2.Value > 0)
                gain_LNA = 24;
            else
                gain_LNA = 0;

            if (trackBar3.Value > 0)
                gain_Mixer = 19;
            else
                gain_Mixer = 0;

            gain_Baseband = trackBar4.Value;

            if (gain_LNA > 0)
                label2.Text = "24 dB";
            else
                label2.Text = "0 dB";

            if (gain_Mixer > 0)
                label3.Text = "19 dB";
            else
                label3.Text = "0 dB";

            label4.Text = "" + gain_Baseband + " dB";

            //Rate
            switch (comboBox2.SelectedIndex)
            {
                case 0:
                    rate = 2000000;
                    break;
                case 1:
                    rate = 3000000;
                    break;
                case 2:
                    rate = 4000000;
                    break;
                case 3:
                    rate = 5000000;
                    break;
                case 4:
                    rate = 6000000;
                    break;
                case 5:
                    rate = 7000000;
                    break;
                case 6:
                    rate = 8000000;
                    break;
                case 7:
                    rate = 9000000;
                    break;
                case 8:
                    rate = 10000000;
                    break;
            }


            uint bandwith = 0;
            switch (comboBox4.SelectedIndex)
            {
                case 0:
                    bandwith = 200000;
                    break;
                case 1:
                    bandwith = 300000;
                    break;
                case 2:
                    bandwith = 600000;
                    break;
                case 3:
                    bandwith = 1536000;
                    break;
                case 4:
                    bandwith = 5000000;
                    break;
                case 5:
                    bandwith = 6000000;
                    break;
                case 6:
                    bandwith = 7000000;
                    break;
                case 7:
                    bandwith = 8000000;
                    break;
            }


            //Sampling mode
            int Format = comboBox3.SelectedIndex;


            if (Format == 5)//FORMAT_AUTO_ON
            {
                if (rate <= 6048000)
                {
                    label13.Text = "Format 252_S16 14bit";
                }
                else if (rate <= 8064000)
                {
                    label13.Text = "Format 336_S16 12bit";
                }
                else if (rate <= 9216000)
                {
                    label13.Text = "Format 384_S16 10+2bit";
                }
                else
                {
                    label13.Text = "Format 504_S16 14bit";
                }
            }

            uint IF_freq = 0;
            //Rate
            switch (comboBox5.SelectedIndex)
            {
                case 0:
                    IF_freq = 0;
                    break;
                case 1:
                    IF_freq = 450000;
                    break;
                case 2:
                    IF_freq = 1620000;
                    break;
                case 3:
                    IF_freq = 2408000;
                    break;
            }


            bool AGC = checkBox1.Checked;
            bool ShiftOn = checkBox3.Checked;

            int shift = (int)numericUpDown1.Value;

            if (!AGC)
            {
                trackBar1.Enabled = true;
                trackBar2.Enabled = true;
                trackBar3.Enabled = true;
                trackBar4.Enabled = true;
            }
            else
            {
                trackBar1.Enabled = false;
                trackBar2.Enabled = false;
                trackBar3.Enabled = false;
                trackBar4.Enabled = false;
            }

            if (ShiftOn)
                numericUpDown1.Enabled = true;
            else
                numericUpDown1.Enabled = false;

            int transfer = 1;

            if (checkBox3.Checked) transfer = 1;
            else if (checkBox3.Checked) transfer = 2;

            if (EventGain != null)
                EventGain(TunerNr, gain, rate, bandwith, IF_freq, AGC, false, ShiftOn, shift, Format, gain_LNA, gain_Mixer, gain_Baseband, transfer);

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            SendSettings();
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            SendSettings();
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            SendSettings();
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            SendSettings();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex != itm)
            {
                itm = comboBox1.SelectedIndex;
                if (EventRadio != null)
                    EventRadio(TunerNr, itm);
            }
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            SendSettings();
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            SendSettings();
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
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

        private void trackBar4_Scroll(object sender, EventArgs e)
        {
            SendSettings();
        }



        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {
            SendSettings();
        }

        private void label10_Click(object sender, EventArgs e)
        {

        }

        private void label9_Click(object sender, EventArgs e)
        {

        }

        private void label8_Click(object sender, EventArgs e)
        {

        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void Settings_SDRplay_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Visible = false;
        }



        private void checkBox2_CheckedChanged_1(object sender, EventArgs e)
        {
            SendSettings();
        }

        private void checkBox1_CheckedChanged_1(object sender, EventArgs e)
        {
            SendSettings();
        }

        private void trackBar1_Scroll_1(object sender, EventArgs e)
        {
            SendSettings();
        }

        private void label7_Click_1(object sender, EventArgs e)
        {

        }

        private void comboBox2_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            SendSettings();
        }

        private void trackBar2_Scroll_1(object sender, EventArgs e)
        {
            SendSettings();
        }

        private void trackBar3_Scroll_1(object sender, EventArgs e)
        {
            SendSettings();
        }

        private void trackBar4_Scroll_1(object sender, EventArgs e)
        {
            SendSettings();
        }

        private void comboBox3_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            SendSettings();
        }

        private void label11_Click(object sender, EventArgs e)
        {

        }

        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            SendSettings();
        }

        private void checkBox3_CheckedChanged_1(object sender, EventArgs e)
        {
            SendSettings();
        }

        private void numericUpDown1_ValueChanged_1(object sender, EventArgs e)
        {
            SendSettings();
        }

        private void checkBox2_CheckedChanged_2(object sender, EventArgs e)
        {
            SendSettings();
        }

        private void comboBox5_SelectedIndexChanged(object sender, EventArgs e)
        {
            SendSettings();
        }

    }
}