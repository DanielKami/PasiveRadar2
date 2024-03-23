using System;
using System.Windows.Forms;

namespace PasiveRadar
{
    public partial class TranslateControl : UserControl
    {
        public delegate void MyDelegate(Flags LocalFlags);
        public static event MyDelegate EventSettings;
        bool SET = false;  //Avoid sending settings to the main program
        public TranslateControl()
        {
            InitializeComponent();
            Form1.FlagsDelegate += new Form1.MyDelegate(Initialize);
        }

        void Initialize(Flags flags)
        {
            SET = false;
            Flags LocalFlags = new Flags();

            LocalFlags.AmplitudeOfAccepterPoints = flags.AmplitudeOfAccepterPoints;
            LocalFlags.NrPointsOfObject = flags.NrPointsOfObject;
            LocalFlags.DistanceFrom0line = flags.DistanceFrom0line;
            LocalFlags.AcceptedDistance = flags.AcceptedDistance;
            LocalFlags.Integration = flags.Integration;


            //Execute
            trackBar1.Value = LocalFlags.AmplitudeOfAccepterPoints;
            trackBar2.Value = LocalFlags.NrPointsOfObject;
            trackBar3.Value = LocalFlags.DistanceFrom0line;
            trackBar4.Value = LocalFlags.AcceptedDistance;
            checkBox1.Checked = LocalFlags.Integration;

            label3.Text = "" + trackBar1.Value;
            label4.Text = "" + trackBar2.Value;
            label6.Text = "" + trackBar3.Value;
            label9.Text = "" + trackBar4.Value;

            SET = true;
        }


        public void UpdateAllControls()
        {
            trackBar1.Update();
            trackBar2.Update();

            trackBar3.Update();
            trackBar4.Update();
            trackBar5.Update();
            checkBox1.Update();
            label3.Update();
            label4.Update();
            label6.Update();
            label9.Update();
            label10.Update();

        }
        void SendSettings()
        {
            if (!SET) return;
            Flags LocalFlags = new Flags();

            LocalFlags.AmplitudeOfAccepterPoints = trackBar1.Value;
            LocalFlags.NrPointsOfObject = trackBar2.Value;
            LocalFlags.DistanceFrom0line = trackBar3.Value;
            LocalFlags.AcceptedDistance = trackBar4.Value;
            LocalFlags.Integration = checkBox1.Checked;

            label3.Text = "" + trackBar1.Value;
            label4.Text = "" + trackBar2.Value;
            label6.Text = "" + trackBar3.Value;
            label9.Text = "" + trackBar4.Value;

            if (EventSettings != null)
                EventSettings(LocalFlags);
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            SendSettings();
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            SendSettings();
        }

        private void label5_Click(object sender, EventArgs e)
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

        private void trackBar5_Scroll(object sender, EventArgs e)
        {
            SendSettings();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            SendSettings();
        }
    }
}
