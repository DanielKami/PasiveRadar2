using System;
using System.Windows.Forms;

namespace PasiveRadar
{
    public partial class SettingsRX888 : Form
    {
        int TunerNr; //Inform for which radio the window is assigned
        public int itm;

        public delegate void MyDelegate(int Radio, int item);
        public delegate void MyDelegateSettings(int index, int gain_index, int attenuation, uint rate, uint decymation, int FrequencyCorrection, bool VHF_HF, bool dither, bool Pga, bool Rand, bool HfBias, bool VhfBias);



        public static event MyDelegate EventRadio;
        public static event MyDelegateSettings EventGain;

        public SettingsRX888(int _TunerNr)
        {
            InitializeComponent();
            TunerNr = _TunerNr;

            comboBox2.SelectedIndex = 0;
        }

        public void SetSettings(RadioRX888 radio)
        {
            //Rate
            if (radio == null) return;
            uint rate = radio.rate;
            uint decymation = radio.decymation;


            //xtl
            label15.Text = "xtal: " + radio.FreqCorrection / 1000000 + " MHz";

            //RF gain
            int gains = radio.Number_of_gains;
            if (gains < 1) gains = 1;
            trackBar_amplification.Maximum = gains - 1;
            if (radio.gain < gains)
                trackBar_amplification.Value = radio.gain;
            label1.Text = "" + radio.gain;

            //Gain stages
            int attenuations = radio.Number_of_attenuations;
            if (attenuations < 1) attenuations = 1;


            trackBarAttenuation.Maximum = attenuations;
            if (radio.attenuation < attenuations)
                trackBarAttenuation.Value = radio.attenuation;
            label9.Text = "" + radio.attenuation;

            bool VHF_HF = radio.VHF_HF;
            checkBox1.Checked = VHF_HF;

            bool dither = radio.dither;
            checkBox2.Checked = dither;

            bool Pga = radio.Pga;
            checkBox4.Checked = Pga;

            bool Rand = radio.Rand;
            checkBox5.Checked = Rand;

            bool HfBias = radio.HfBias;
            checkBox6.Checked = HfBias;

            bool VhfBias = radio.VhfBias;
            checkBox7.Checked = VhfBias;

            int itm = 0;
            if (comboBox2.SelectedIndex != rate)
            {
                switch (rate)
                {
                    case 64000000:
                        itm = 0;
                        break;
                    case 32000000:
                        itm = 1;
                        break;
                    case 24000000:
                        itm = 2;
                        break;
                    case 20000000:
                        itm = 3;
                        break;
                    case 8000000:
                        itm = 4;
                        break;
                    case 4000000:
                        itm = 5;
                        break;
                    case 2000000:
                        itm = 6;
                        break;
                    case 1000000:
                        itm = 7;
                        break;
                }
                comboBox2.SelectedIndex = itm;
            }

            if (comboBox4.SelectedIndex != decymation)
            {
                switch (decymation)
                {
                    case 1:
                        itm = 0;
                        break;
                    case 2:
                        itm = 1;
                        break;
                    case 4:
                        itm = 2;
                        break;
                    case 8:
                        itm = 3;
                        break;
                }
                comboBox4.SelectedIndex = itm;
            }


            //  label10.Text = "" + radio.;

        }

        public void ComboBoxRadio_Update(ref FindRX888 find)
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
            uint rate = 1;
            int gain = trackBar_amplification.Value;
            label1.Text = "" + trackBar_amplification.Value;

            int attenuation = trackBarAttenuation.Value;
            label9.Text = "" + trackBarAttenuation.Value;

            //Rate
            switch (comboBox2.SelectedIndex)
            {
                case 0:
                    rate = 64000000;
                    break;
                case 1:
                    rate = 32000000;
                    break;
                case 2:
                    rate = 24000000;
                    break;
                case 3:
                    rate = 20000000;
                    break;
                case 4:
                    rate = 8000000;
                    break;
                case 5:
                    rate = 4000000;
                    break;
                case 6:
                    rate = 2000000;
                    break;
                case 7:
                    rate = 1000000;
                    break;
            }


            uint decymation = 0;
            switch (comboBox4.SelectedIndex)
            {
                case 0:
                    decymation = 1;
                    break;
                case 1:
                    decymation = 2;
                    break;
                case 2:
                    decymation = 4;
                    break;
                case 3:
                    decymation = 8;
                    break;

            }



            bool ShiftOn = checkBox3.Checked;

            int frequency_coorection = (int)numericUpDown1.Value;



            if (ShiftOn)
                numericUpDown1.Enabled = true;
            else
                numericUpDown1.Enabled = false;

            bool VHF_HF;

            if (checkBox1.Checked) VHF_HF = true;
            else VHF_HF = false;

            bool dither;
            if (checkBox2.Checked) dither = true;
            else dither = false;

            bool Pga;
            if (checkBox4.Checked) Pga = true;
            else Pga = false;

            bool Rand;
            if (checkBox5.Checked) Rand = true;
            else Rand = false;

            bool HfBias;
            if (checkBox6.Checked) HfBias = true;
            else HfBias = false;

            bool VhfBias;
            if (checkBox7.Checked) VhfBias = true;
            else VhfBias = false;

            if (EventGain != null)
                EventGain(TunerNr, gain, attenuation, rate, decymation, frequency_coorection, VHF_HF, dither, Pga, Rand, HfBias, VhfBias);


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



        private void SettingsRX888_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Visible = false;
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            SendSettings();
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
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

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void trackBar4_Scroll(object sender, EventArgs e)
        {
            SendSettings();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            SendSettings();
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            SendSettings();
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            SendSettings();
        }

        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {
            SendSettings();
        }

        private void checkBox6_CheckedChanged(object sender, EventArgs e)
        {
            SendSettings();
        }

        private void checkBox7_CheckedChanged(object sender, EventArgs e)
        {
            SendSettings();
        }

        private void toolTip1_Popup(object sender, PopupEventArgs e)
        {

        }

        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            SendSettings();
        }
    }
}
