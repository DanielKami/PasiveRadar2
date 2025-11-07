using System;
using System.Windows.Forms;

namespace PasiveRadar
{
    public partial class SettingsAirSpy : Form
    {
        int TunerNr; //Inform for which radio the window is assigned
        public int itm;

        public delegate void MyDelegate(int Radio, int item);
        public delegate void MyDelegateSettings(int index, int gainLNA, int MixerGain, int gainBaseBand, uint rate, uint decymation, int FrequencyCorrection, bool bias_tee, bool packing, bool agc);


        public static event MyDelegate EventRadio;
        public static event MyDelegateSettings EventGain;


        public SettingsAirSpy(int _TunerNr)
        {
            InitializeComponent();
            TunerNr = _TunerNr;

            comboBox2.SelectedIndex = 0;
        }

        public void SetSettings(RadioAirSpy radio)
        {
            //Rate
            if (radio == null) return;
            uint rate = radio.rate;

            uint decymation = radio.decymation;

            //xtl
            //label15.Text = "xtal: " + radio.FreqCorrection / 1000000 + " MHz";
            label12.Text = "SN: " + radio.SN;


            //LNA gain
            int LNAgains = radio.Number_of_LNAgains;
            if (LNAgains < 1) LNAgains = 1;
            trackBar_amplification.Maximum = LNAgains;

            if (radio.gainLNA <= LNAgains)
                trackBar_amplification.Value = radio.gainLNA;
            label1.Text = "" + radio.gainLNA;

            //Gain Mixer
            int Mixergains = radio.Number_of_Mixergains;
            if (Mixergains < 1) Mixergains = 1;
            trackBarAttenuation.Maximum = Mixergains;

            if (radio.gainMixer < Mixergains)
                trackBarAttenuation.Value = radio.gainMixer;
            label9.Text = "" + radio.gainMixer;

            //RFgain
            int IFgains = radio.Number_of_RFgains;
            if (IFgains < 1) IFgains = 1;
            trackBar2.Maximum = IFgains;

            if (radio.gainRF < IFgains)
                trackBar2.Value = radio.gainRF;
            label3.Text = "" + radio.gainRF;


            bool agc= radio.agc;
            checkBox2.Checked = agc;

            bool BiasTee = radio.BiasTee;
            checkBox1.Checked = BiasTee;

            bool packing = radio.packing;
            checkBox6.Checked = packing;

            int itm = 0;
            if (comboBox2.SelectedIndex != rate)
            {
                switch (rate)
                {
                    case 10000000:
                        itm = 0;
                        break;
                    case 2500000:
                        itm = 1;
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


        }


        public void ComboBoxRadio_Update(ref FindAirSpy find)
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


        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex != itm)
            {
                itm = comboBox1.SelectedIndex;
                if (EventRadio != null)
                    EventRadio(TunerNr, itm);
            }
        }

        void SendSettings()
        {
            uint rate = 1;
            int gainLNA = trackBar_amplification.Value;
            label1.Text = "" + trackBar_amplification.Value;

            int MixerGain = trackBarAttenuation.Value;
            label9.Text = "" + trackBarAttenuation.Value;

            int gainBaseBand = trackBar2.Value;
            label3.Text = "" + gainBaseBand;

            //Rate
            switch (comboBox2.SelectedIndex)
            {
                case 0:
                    rate = 10000000;
                    break;
                case 1:
                    rate = 2500000;
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





            int FrequencyCorrection = 0;

            if (checkBox3.Checked)
            {
                FrequencyCorrection = (int)numericUpDown1.Value;
                numericUpDown1.Enabled = true;
            }
            else
                numericUpDown1.Enabled = false;


            bool bias_tee;
            if (checkBox1.Checked) bias_tee = true;
            else bias_tee = false;

            bool packing;
            if (checkBox6.Checked) packing = true;
            else packing = false;

            bool agc;
            if (checkBox2.Checked) agc = true;
            else agc = false;

            if (EventGain != null)
                EventGain(TunerNr, gainLNA, MixerGain, gainBaseBand, rate, decymation, FrequencyCorrection, bias_tee, packing, agc);


        }




        private void label7_Click(object sender, EventArgs e)
        {

        }



        private void trackBar_amplification_Scroll(object sender, EventArgs e)
        {
            SendSettings();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            SendSettings();
        }

        private void checkBox6_CheckedChanged(object sender, EventArgs e)
        {
            SendSettings();
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            SendSettings();
        }

        private void trackBarAttenuation_Scroll(object sender, EventArgs e)
        {
            SendSettings();
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            SendSettings();
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            SendSettings();
        }

        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            SendSettings();
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            SendSettings();
        }

        private void label12_Click(object sender, EventArgs e)
        {

        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            SendSettings();
        }

        private void SettingsAirSpy_FormClosed(object sender, FormClosedEventArgs e)
        {
        
        }

        private void SettingsAirSpy_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Visible = false;
        }
    }
}
