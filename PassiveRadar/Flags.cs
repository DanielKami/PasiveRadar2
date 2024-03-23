using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace PasiveRadar
{
    public class Flags
    {
        public const string version = "v. 2.2";   //version of Radar
        public const uint MAX_DONGLES_RTLSDR = 3;        //Number of suportet dongles (the number can be bigger but for radar it is no really sense)
        public const uint MAX_DONGLES_RSP1 = 1;      //Is a Miri 2500 device present
        public const uint ALL_DONGLES = MAX_DONGLES_RTLSDR + MAX_DONGLES_RSP1;
        public const uint MAX_DEVICE_NAME = 256;  //number of characters dedicated to the name of CUDA device
        public const int ColorTableSize = 1024;   //Size of color table
        public const uint FramePerSec = 39;       //Integrated Graphics card frames/s (radar and radio windows drawing)
        public Flags()
        {
            rate = new float[ALL_DONGLES];
            bandwitch = new uint[ALL_DONGLES];
            frequency = new double[ALL_DONGLES];
            Amplification = new int[ALL_DONGLES];
            Cumulation = new uint[ALL_DONGLES];
            Level = new int[ALL_DONGLES];
            showRadioWave = new bool[ALL_DONGLES];
            showRadioFlow = new bool[ALL_DONGLES];
            FilterCentralFreq = new float[ALL_DONGLES];
            BufferSizeRadio = new uint[ALL_DONGLES];
            showRadar = new bool[ALL_DONGLES];

            //Flags general
            for (int i = 0; i < ALL_DONGLES; i++)
            {
                rate[i] = 3200000;                             //Bit rate of the IF from reciver
                bandwitch[i] = 8000000;                        //Bandwith filter
                frequency[i] = 102800000;                      //Frequency of  dongle 
                Amplification[i] = 37;                         //Amplification of data in view from radio i
                Level[i] = 0;                                  //Level of data for average in view  from radio i
                Cumulation[i] = 4;                             //Cumulation of data for average in view from radio i
                showRadioWave[i] = true;
                showRadioFlow[i] = true;
                FilterCentralFreq[i] = 0;
                BufferSizeRadio[i] = 512;
            }

            ColorThemeTable = new Microsoft.Xna.Framework.Color[ColorTableSize];

        }

        //Map
        public bool ShowMap = false;
        public int NrPointsOfObject = 10;                       //number of object pixels to be accepted
        public int AmplitudeOfAccepterPoints = 1000;            //minimum pixel value to be accepted
        public int AcceptedDistance = 1;
        public int DistanceFrom0line = 1;                       //omit points for signal search which are close to the 0 line (usually intensive)
        public bool Integration = false;                        //integration true with weighs/ false only geometrical
                                                                // public float AcceptedDistance = 0.01f;                   //Accepted distance between calculated points on the map from different towers, in km

        //Flags general
        public int ColorTheme = 0;                              //Color them for flow and radar screens
        public bool AMDdriver = false;                          //State if AMD driver is installed

        //Flag related to radio
        public float[] rate;                                    //Bit rate of the IF from reciver
        public uint[] bandwitch;
        public double[] frequency;                              //Frequency of  dongle
        public int[] Amplification;                             //Amplification of data in view from radio i
        public uint[] Cumulation = new uint[ALL_DONGLES];       //Number of cumulations wave and flow radio window
        public int[] Level = new int[ALL_DONGLES];              //Level of background in flow window
        public bool[] showRadioWave;
        public bool[] showRadioFlow;
        public int LastActiveWindowRadio = 0;                   //Last use window radio numer
        public int Nr_active_radio = 0;                         //Number of active radio recivers
        public uint Radio_buffer_size = 5;                       //The small buffer size to read dongle data is in form 2^buffer_Size * 1024
        public bool format8_16 = false;                         //Data format 8bit/16bit. important for radar   (to divide the signal on float to high amplitude  


        //Flags related to radar
        public uint[] BufferSizeRadio;                          //Buffer for FFT   of radio
        public uint BufferSize = 1024 * 256;                    //Buffer for FFT   of radar                            
        public bool remove_symetrics = false;                   //Flag indicate if the symetric signals in radar has to be removed
        public int average = 5;                                 //Average Radar frames over specified number
        public short scale_type = 0;                            //sygnal scale in radar 0-lin, 1-sqrt, 2-log
        public uint MaxAverage = 40;                            //Maximum frame average for radar
        public uint DopplerZoom = 500;                          //Distance zoom in pasive radar
        public float PasiveGain = 5;                            //Gain of the ambiguity function
        public uint Columns = 100;                              //Screen columns
        public uint Rows = 200;                                 //Screen rows
        public bool OpenCL = false;                             //Turn on/off openCL calculations
        public int DistanceShift = 0;                           //Task divider between GPU and CPU for rows calculations in ambiguity function. CPU task is CPU_GPU_task_ratio and GPU is 1.0-CPU_GPU_task_ratio
        public bool CorrectBackground = false;                  //Correct radar background
        public bool FreezeBackground = false;                   //Freze the recorded background with out updating it
        public uint NrReduceRows = 2;

        public int NrCorrectionPoints = 10;                     //Nr of points used for correction in regresion
        public int ColectEvery = 10;                            //Colect correction points every .... points
        public float CorectionWeight = 0.5f;                    //Corection weight
        public string DeviceName;
        public byte alpha = 255;                                //color alpha of the Radar screen



        //Flags related to sound
        public List<string> SoundDevices;                       //List of available sound devices
        public string SoundDeviceName;                          //Selected device
        public bool demodulate = false;                         //Flag dentte if the data have to be demodulagted
        public int demodulateFromRadio = 0;                     //Flag indicate which data (from which dongle) shold be demodulated
        public int SoundSampleRate = 48000;                     //Rate of the sound samples per second
        public int demodulation = 1;                            //1- demodulation FM
        public int LowCut = 20;                                 //Low sound frequency cut
        public int UpCut = 16000;                               //Up sound frequency cut

        //Site menu
        public bool ButtonRadio = true;                         //Side menu radio settings (true- menu closed, false menu open)
        public bool ButtonDisplay = true;                       //Side menu display settings (true- menu closed, false menu open)
        public bool ButtonAudio = true;                         //Side menu audio settings (true- menu closed, false menu open)
        public bool ButtonRadar = true;                         //Side menu radar settings (true- menu closed, false menu open)
        public bool ButtonCorrelation = true;                   //Side menu correlation settings (true- menu closed, false menu open)

        //Flags related to display
        public bool[] showRadar;                           //Show data in view control 2

        public bool showBackground = false;                     //Show data in view control 3

        public int refresh_delay = 0;                           //Dead time between video frames calculations in ms to save procesor power
        public Color[] ColorThemeTable;                         //Color theme defined by user

        //Related to filters IQ stream
        public float[] FilterCentralFreq;
        public int taps = 56;                                   //Points in fiter window more better but slower
        public int FilterType = 0;                              //0-RECTANGULAR, 1-BARTLETT, 2-HANNING, 3-HAMMING, 4-BLACKMAN
        public uint downsampling = 4;                           //down sampling of demodulated signal 1, 2, 4, 8......
        public float RadioHalfBandwith = 125000;                //Radio Bandwitch
        public float RadioFrequency = 0;                        //Radio frequency it is a difference from main frequency

        //Internal flags for synchronisation 
        public bool Resynchronisation = false;                  //Flags used to send information to correlate draw to show this event
        public int SynchronisationTime = 5;                     //Time between radios stop and start to resynchronise

        public void Copy(Flags NewFlags)
        {
            for (int i = 0; i < ALL_DONGLES; i++)
            {
                rate[i] = NewFlags.rate[i];                      //Bit rate of the IF from reciver
                bandwitch[i] = NewFlags.bandwitch[i];            //filters bandwitch
                frequency[i] = NewFlags.frequency[i];            //Frequency of  dongle 
                Amplification[i] = NewFlags.Amplification[i];    //Amplification of data in view from radio i
                Level[i] = NewFlags.Level[i];                    //Level of data for average in view  from radio i
                Cumulation[i] = NewFlags.Cumulation[i];          //Cumulation of data for average in view from radio i
                showRadioWave[i] = NewFlags.showRadioWave[i];
                showRadioFlow[i] = NewFlags.showRadioFlow[i];
                FilterCentralFreq[i] = NewFlags.FilterCentralFreq[i];
                BufferSizeRadio[i] = NewFlags.BufferSizeRadio[i];
            }
            Radio_buffer_size = NewFlags.Radio_buffer_size;

            //Flags related to radar
            BufferSizeRadio = NewFlags.BufferSizeRadio;           //Buffer for FFT   of radio
            BufferSize = NewFlags.BufferSize;                     //Buffer for FFT   of radar                            
            remove_symetrics = NewFlags.remove_symetrics;         //Flag indicate if the symetric signals in radar has to be removed
            average = NewFlags.average;                           //Average Radar frames over specified number
            scale_type = NewFlags.scale_type;                     //sygnal scale in radar 0-lin, 1-sqrt, 2-log
            MaxAverage = NewFlags.MaxAverage;                     //Maximum frame average for radar
            DopplerZoom = NewFlags.DopplerZoom;                   //Distance zoom in pasive radar
            PasiveGain = NewFlags.PasiveGain;                     //Gain of the ambiguity function
            Columns = NewFlags.Columns;                           //Screen columns
            Rows = NewFlags.Rows;                                 //Screen rows
            OpenCL = NewFlags.OpenCL;                             //Turn on/off openCL calculations
            DistanceShift = NewFlags.DistanceShift;               //Task divider between GPU and CPU for rows calculations in ambiguity function. CPU task is CPU_GPU_task_ratio and GPU is 1.0-CPU_GPU_task_ratio
            CorrectBackground = NewFlags.CorrectBackground;       //Correct radar background
            FreezeBackground = NewFlags.FreezeBackground;         //Freze the recorded background with out updating it
            NrReduceRows = NewFlags.NrReduceRows;

            NrCorrectionPoints = NewFlags.NrCorrectionPoints;     //Nr of points used for correction in regresion
            ColectEvery = NewFlags.ColectEvery;                   //Colect correction points every .... points
            CorectionWeight = NewFlags.CorectionWeight;           //Corection weight
            DeviceName = NewFlags.DeviceName;
            alpha = NewFlags.alpha;



            //Site menu
            ButtonRadio = NewFlags.ButtonRadio;                                   //Side menu radio settings (true- menu closed, false menu open)
            ButtonDisplay = NewFlags.ButtonDisplay;                               //Side menu display settings (true- menu closed, false menu open)
            ButtonAudio = NewFlags.ButtonAudio;                                   //Side menu audio settings (true- menu closed, false menu open)
            ButtonRadar = NewFlags.ButtonRadar;                                   //Side menu radar settings (true- menu closed, false menu open)
            ButtonCorrelation = NewFlags.ButtonCorrelation;                       //Side menu correlation settings (true- menu closed, false menu open)

            //Flags related to display
            for (int i = 0; i < ALL_DONGLES; i++)
                showRadar[i] = NewFlags.showRadar[i];                                       //Show data in view control 2



            showBackground = NewFlags.showBackground;                             //Show data in view control 3

            refresh_delay = NewFlags.refresh_delay;                               //Dead time between video frames calculations in ms to save procesor power


            for (int i = 0; i < ColorThemeTable.Length; i++)
                ColorThemeTable[i] = NewFlags.ColorThemeTable[i];                 //Color theme defined by user

        }

        public void Save()
        {

            String yourText;
            //Flags general
            yourText = "# Flags general" + "\n";
            for (int ix = 0; ix < ALL_DONGLES; ix++)
                yourText += "rate" + ix + " " + rate[ix].ToString(CultureInfo.InvariantCulture) + "\n";
            for (int ix = 0; ix < ALL_DONGLES; ix++)
                yourText += "bandwitch" + ix + " " + bandwitch[ix].ToString(CultureInfo.InvariantCulture) + "\n";
            for (int ix = 0; ix < ALL_DONGLES; ix++)
                yourText += "BufferSizeRadio" + ix + " " + BufferSizeRadio[ix].ToString(CultureInfo.InvariantCulture) + "\n";
            //yourText += "rate1 " + rate1.ToString(CultureInfo.InvariantCulture) + "\n";
            for (int ix = 0; ix < ALL_DONGLES; ix++)
                yourText += "frequency" + ix + " " + frequency[ix].ToString(CultureInfo.InvariantCulture) + "\n";
            //yourText += "frequency1 " + frequency1.ToString(CultureInfo.InvariantCulture) + "\n";

            yourText += "ColorTheme " + ColorTheme + "\n";
            yourText += "LastActiveWindowRadio " + LastActiveWindowRadio + "\n";

            //Flags related to radar
            yourText += "                      # Flags related to radar" + "\n";
            yourText += "BufferSize " + BufferSize + "\n";
            yourText += "symetrices " + remove_symetrics + "\n";
            yourText += "average " + average + "\n";
            yourText += "DopplerZoom " + DopplerZoom + "\n";
            yourText += "PasiveGain " + PasiveGain.ToString(CultureInfo.InvariantCulture) + "\n";
            yourText += "Columns " + Columns + "\n";
            yourText += "Rows " + Rows + "\n";
            yourText += "OpenCL  " + OpenCL + "\n";
            yourText += "DistanceShift " + DistanceShift + "\n";
            yourText += "CorrectBackground " + CorrectBackground + "\n";
            yourText += "NrCorrectionPoints " + NrCorrectionPoints + "\n";
            yourText += "ColectEvery " + ColectEvery + "\n";
            yourText += "CorectionWeight " + CorectionWeight.ToString(CultureInfo.InvariantCulture) + "\n";
            yourText += "ReduceRows " + NrReduceRows + "\n";

            //Flags related to Map digitalisation
            yourText += "   #                  Flags related to translation" + "\n";
            yourText += "DistanceFrom0line " + DistanceFrom0line + "\n";
            yourText += "NrPointsOfObject " + NrPointsOfObject + "\n";
            yourText += "AmplitudeOfAccepterPoints " + AmplitudeOfAccepterPoints + "\n";
            yourText += "ScanPointsPosNegDir " + AcceptedDistance + "\n";
            yourText += "Integration " + Integration + "\n";


            //Flags related to sound
            yourText += "                   # Flags related to sound" + "\n";
            yourText += "SoundDeviceName " + SoundDeviceName + "\n";
            yourText += "demodulate " + demodulate + "\n";
            yourText += "demodulateFromRadio " + demodulateFromRadio + "\n";
            yourText += "SoundSampleRate " + SoundSampleRate + "\n";
            yourText += "demodulation " + demodulation + "\n";
            yourText += "LowCut " + LowCut + "\n";
            yourText += "UpCut " + UpCut + "\n";

            //Related to filters IQ stream
            for (int ix = 0; ix < ALL_DONGLES; ix++)
                yourText += "FilterCentralFreq" + ix + " " + FilterCentralFreq[ix].ToString(CultureInfo.InvariantCulture) + "\n";
            yourText += "taps " + taps + "\n";
            yourText += "FilterType " + FilterType + "\n";
            yourText += "Radio_buffer_size " + Radio_buffer_size + "\n";


            //Site menu
            yourText += "                              # Site menu " + "\n";
            yourText += "ButtonRadio " + !ButtonRadio + "\n";
            yourText += "ButtonDisplay " + !ButtonDisplay + "\n";
            yourText += "ButtonAudio " + !ButtonAudio + "\n";
            yourText += "ButtonRadar " + !ButtonRadar + "\n";
            yourText += "ButtonCorrelation " + !ButtonCorrelation + "\n";

            //Flags related to display
            yourText += "                              # Flags related to display" + "\n";
            for (int ix = 0; ix < ALL_DONGLES; ix++)
                yourText += "showRadioWave" + ix + " " + showRadioWave[ix] + "\n";
            for (int ix = 0; ix < ALL_DONGLES; ix++)
                yourText += "showRadioFlow" + ix + " " + showRadioFlow[ix] + "\n";

            for (int ix = 0; ix < ALL_DONGLES; ix++)
                yourText += "showRadar" + ix + " " + showRadar[ix] + "\n";


            yourText += "showDifference " + showBackground + "\n";

            for (int ix = 0; ix < ALL_DONGLES; ix++)
                yourText += "Amplification" + ix + " " + Amplification[ix] + "\n";

            for (int ix = 0; ix < ALL_DONGLES; ix++)
            {
                if (Cumulation[ix] == 0) Cumulation[ix] = 1;
                yourText += "Cumulation" + ix + " " + Cumulation[ix] + "\n";
            }

            for (int ix = 0; ix < ALL_DONGLES; ix++)
                yourText += "Level" + ix + " " + Level[ix] + "\n";

            yourText += "refresh_delay " + refresh_delay + "\n";

            int i = 0;
            foreach (Color element in ColorThemeTable)
            {
                yourText += "ColorThemeTable " + i + " " + element.R + " " + element.G + " " + element.B + "\n";
                i++;
            }

            File.WriteAllText("settings.ext", yourText); //writing
        }

        public void Read()
        {
            char[] delimiterChars = { ' ', ':', '\t' };
            int counter = 0;
            string line;

            try
            {
                System.IO.StreamReader file = new System.IO.StreamReader("settings.ext");
                while ((line = file.ReadLine()) != null)
                {
                    string[] words = line.Split(delimiterChars);

                    if (words.Length > 1 && words[0] != "#")
                        Translate(words[0], words);

                    counter++;
                }

                file.Close();
            }
            catch
            {

            }
        }

        private void Translate(string parameter, string[] worlds)
        {
            string value = worlds[1];
            string tmp = "";
            //Flags radio
            for (int ix = 0; ix < ALL_DONGLES; ix++)
            {
                if (parameter == ("rate" + ix))
                    float.TryParse(value, out rate[ix]);
                else if (parameter == "frequency" + ix) frequency[ix] = Double.Parse(value, CultureInfo.InvariantCulture); //Double.TryParse(value, out frequency0);
                else if (parameter == "Amplification" + ix) Int32.TryParse(value, out Amplification[ix]);
                else if (parameter == "Cumulation" + ix) UInt32.TryParse(value, out Cumulation[ix]);
                else if (parameter == "Level" + ix) Int32.TryParse(value, out Level[ix]);
                else if (parameter == "showRadioWave" + ix) Boolean.TryParse(value, out showRadioWave[ix]);//Flags related to display
                else if (parameter == "showRadioFlow" + ix) Boolean.TryParse(value, out showRadioFlow[ix]);//Flags related to display
                else if (parameter == "FilterCentralFreq" + ix) float.TryParse(value, out FilterCentralFreq[ix]);//Flags related IQ strem and filters 
                else if (parameter == "BufferSizeRadio" + ix) UInt32.TryParse(value, out BufferSizeRadio[ix]);
                else if (parameter == "bandwitch" + ix) UInt32.TryParse(value, out bandwitch[ix]);
            }


            if (parameter == "ColorTheme") Int32.TryParse(value, out ColorTheme);
            else if (parameter == "LastActiveWindowRadio") Int32.TryParse(value, out LastActiveWindowRadio);

            //Flags related to radar
            else if (parameter == "BufferSize") UInt32.TryParse(value, out BufferSize);
            else if (parameter == "symetrices") Boolean.TryParse(value, out remove_symetrics);
            else if (parameter == "average") Int32.TryParse(value, out average);
            else if (parameter == "DopplerZoom") UInt32.TryParse(value, out DopplerZoom);
            else if (parameter == "PasiveGain") PasiveGain = float.Parse(value, CultureInfo.InvariantCulture); //float.TryParse(value, out PasiveGain);
            else if (parameter == "Columns") UInt32.TryParse(value, out Columns);
            else if (parameter == "Rows") UInt32.TryParse(value, out Rows);
            else if (parameter == "OpenCL") Boolean.TryParse(value, out OpenCL);
            else if (parameter == "DistanceShift") DistanceShift = Int32.Parse(value, CultureInfo.InvariantCulture); //float.TryParse(value, out CPU_GPU_task_ratio);
            else if (parameter == "CorrectBackground") Boolean.TryParse(value, out CorrectBackground);
            else if (parameter == "NrCorrectionPoints") Int32.TryParse(value, out NrCorrectionPoints);
            else if (parameter == "ColectEvery") Int32.TryParse(value, out ColectEvery);
            else if (parameter == "CorectionWeight") CorectionWeight = float.Parse(value, CultureInfo.InvariantCulture); //float.TryParse(value, out CorectionWeight);
            else if (parameter == "ReduceRows") UInt32.TryParse(value, out NrReduceRows);

            //Flags related to Map
            else if (parameter == "NrPointsOfObject") Int32.TryParse(value, out NrPointsOfObject);
            else if (parameter == "AmplitudeOfAccepterPoints") Int32.TryParse(value, out AmplitudeOfAccepterPoints);
            else if (parameter == "ScanPointsPosNegDir") Int32.TryParse(value, out AcceptedDistance);
            else if (parameter == "DistanceFrom0line") Int32.TryParse(value, out DistanceFrom0line);
            else if (parameter == "Integration") Boolean.TryParse(value, out Integration);

            //Flags related to sound
            else if (parameter == "SoundDeviceName") SoundDeviceName = value;
            else if (parameter == "demodulate") Boolean.TryParse(value, out demodulate);
            else if (parameter == "demodulateFromRadio") Int32.TryParse(value, out demodulateFromRadio);
            else if (parameter == "SoundSampleRate") Int32.TryParse(value, out SoundSampleRate);
            else if (parameter == "demodulation") Int32.TryParse(value, out demodulation);
            else if (parameter == "LowCut") Int32.TryParse(value, out LowCut);
            else if (parameter == "UpCut") Int32.TryParse(value, out UpCut);

            //Related to filters IQ stream
            //else if (parameter == "taps") Int32.TryParse(value, out taps);
            else if (parameter == "FilterType") Int32.TryParse(value, out FilterType);
            else if (parameter == "Radio_buffer_size") Int32.TryParse(value, out FilterType);

            //Site menu
            else if (parameter == "ButtonRadio") Boolean.TryParse(value, out ButtonRadio);
            else if (parameter == "ButtonDisplay") Boolean.TryParse(value, out ButtonDisplay);
            else if (parameter == "ButtonAudio") Boolean.TryParse(value, out ButtonAudio);
            else if (parameter == "ButtonRadar") Boolean.TryParse(value, out ButtonRadar);
            else if (parameter == "ButtonCorrelation") Boolean.TryParse(value, out ButtonCorrelation);

            //Flags related to display
            else
                for (int ix = 0; ix < ALL_DONGLES; ix++)
                {
                    tmp = "showRadar" + ix;
                    if (parameter == tmp) Boolean.TryParse(value, out showRadar[ix]);
                }

            if (parameter == "showDifference") Boolean.TryParse(value, out showBackground);

            else if (parameter == "refresh_delay") Int32.TryParse(value, out refresh_delay);

            else if (parameter == "ColorThemeTable")
            {
                int Index;
                Int32.TryParse(value, out Index);
                if (Index >= 0 && Index < ColorThemeTable.Length)
                {
                    int r = 0, g = 0, b = 0;
                    Int32.TryParse(worlds[2], out r);
                    Int32.TryParse(worlds[3], out g);
                    Int32.TryParse(worlds[4], out b);
                    ColorThemeTable[Index] = Color.FromNonPremultiplied(r, g, b, 255);
                }

            }

            //Check bondaries

            if (LastActiveWindowRadio < 0 || LastActiveWindowRadio > ALL_DONGLES) LastActiveWindowRadio = 0;
        }


    }
}
