using SDRdue;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PasiveRadar
{
    public unsafe partial class Form1
    {
        protected Stopwatch watch = System.Diagnostics.Stopwatch.StartNew();
        int calculation = 0;
        static public int calculations_per_sec = 0;

        Thread thread_calculate_draw_radio_data = null;
        volatile bool calculate_draw_radio_data_exit = false;
        volatile bool calculate_draw_radio_data_exited = false;

        Thread thread_calculate_radar = null;
        volatile bool calculate_radar_exit = false;
        volatile bool calculate_radar_exited = false;

        Thread thread_draw_radar = null;
        volatile bool draw_radar_exit = false;
        volatile bool draw_radar_exited = false;

        Thread thread_draw_map = null;
        volatile bool draw_map_exit = false;
        volatile bool draw_map_exited = false;

        private readonly Object[] LockMainDataStream = new Object[Flags.ALL_DONGLES];
        private readonly Object[] LockRadarScene = new Object[Flags.ALL_DONGLES];
        private readonly Object[] LockMap = new Object[Flags.ALL_DONGLES];
        private readonly Object LockMapBlops = new Object();
        public AutoResetEvent DrawEvent2 = new AutoResetEvent(false);
        public AutoResetEvent DrawEvent = new AutoResetEvent(false);

        private bool resizing;

        WindowWave[] window_wave;
        WindowFlow[] window_flow;
        double[][] dataRadio;
        float[][] dataRadar;
        float[][] PostProc;
        Complex[][] DataIn;
        Complex[][] DataOut;

        bool[] MouseDownPanel;
        int[] oldX_position;
        double[] MHz_perPixel;
        bool FilterMove = false;
        bool RightBandMove = false;
        bool LeftBandMove = false;
        long lost = 0, count = 0;
        uint lost_per_s = 0;
        double[] average;
        int max = 0;
        //Start radar with radio and map
        public void StartDraw()
        {
            //Draw radar only the scale and crosses
            {
                for (int i = 0; i < Flags.ALL_DONGLES; i++)
                    windowRadar[i].RenderRadar(PostProc[i], flags, null, true);
            }

            //////////////////////////////////////////////////////////////
            calculate_draw_radio_data_exit = false;
            calculate_draw_radio_data_exited = false;

            if (thread_calculate_draw_radio_data == null)
            {
                thread_calculate_draw_radio_data = new Thread(new ThreadStart(CalculateDrawrRadioScenes));
                thread_calculate_draw_radio_data.Priority = System.Threading.ThreadPriority.Lowest;
                thread_calculate_draw_radio_data.Start();
            }

            //////////////////////////////////////////////////////////////
            calculate_radar_exit = false;
            calculate_radar_exited = false;

            if (thread_calculate_radar == null)
            {
                thread_calculate_radar = new Thread(new ThreadStart(CalculateRadarScene));
                thread_calculate_radar.Priority = System.Threading.ThreadPriority.AboveNormal;
                thread_calculate_radar.Start();
            }
            ///////////////////////////////////////////////////////////////

            draw_radar_exit = false;
            draw_radar_exited = false;

            if (thread_draw_radar == null)
            {
                thread_draw_radar = new Thread(new ThreadStart(DrawRadarScene));
                thread_draw_radar.Priority = System.Threading.ThreadPriority.BelowNormal;
                thread_draw_radar.Start();
            }

            ///////////////////////////////////////////////////////////////
            draw_map_exit = false;
            draw_map_exited = false;

            if (thread_draw_map == null)
            {
                thread_draw_map = new Thread(new ThreadStart(DrawMap));
                thread_draw_map.Priority = System.Threading.ThreadPriority.Lowest;
                thread_draw_map.Start();
            }
        }

        //Stop radar, radio and map
        public void StopDraw()
        {
            //Stop thread calculate data for scenes
            if (thread_calculate_draw_radio_data != null)
            {
                calculate_draw_radio_data_exit = true;
                while (!calculate_draw_radio_data_exited) ;
                thread_calculate_draw_radio_data = null;
            }

            ////Stop thread calculate data for radar
            if (thread_calculate_radar != null)
            {
                calculate_radar_exit = true;
                while (!calculate_radar_exited) ;
                thread_calculate_radar = null;
            }

            ////Stop thread draw data for radar
            if (thread_draw_radar != null)
            {
                draw_radar_exit = true;
                while (!draw_radar_exited) ;
                thread_draw_radar = null;
            }

            if (thread_draw_map != null)
            {
                draw_map_exit = true;
                while (!draw_map_exited) ;
                thread_draw_map = null;
            }
        }

        private void CalculateDrawrRadioScenes()
        {
            average = new double[Flags.ALL_DONGLES];
            for (int k = 0; k < Flags.ALL_DONGLES; k++)
                average[k] = 0;

            DataIn = new Complex[Flags.ALL_DONGLES][];
            DataOut = new Complex[Flags.ALL_DONGLES][];

            for (int i = 0; i < Flags.ALL_DONGLES; ++i)
            {
                DataIn[i] = new Complex[flags.BufferSizeRadio[i]];
                DataOut[i] = new Complex[flags.BufferSizeRadio[i]];
            }

            while (!calculate_draw_radio_data_exit)
            {
                if (!runing)
                {
                    Thread.Sleep(100);
                }
                else
                    DrawRadio();
            }
            calculate_draw_radio_data_exited = true;
        }

        //Calculate the scenes and draw them for radios It is part of CalculateDrawrRadioScenes()
        void DrawRadio()
        {
            if (flags == null) return;
            //Slow it down a bit 
            if (flags.refresh_delay > 0)
                System.Threading.Thread.Sleep(flags.refresh_delay);

            //Start FFT calculations
            Parallel.For(0, Flags.MAX_DONGLES_RTLSDR, i =>
            {
                if (radioRtlSdr[i].status & radio_window[i].Visible & (flags.showRadioWave[i] | flags.showRadioFlow[i]))
                {
                    lock (LockMainDataStream[i])
                    {
                        calculate[i].CopyToComplex(radioRtlSdr[i].dataIQ, ref DataIn[i]);
                    }
                    calculate[i].FFT(DataIn[i], DataOut[i]);

                    //Now the trick to calculate  FFT in the same time 
                    //Wait until completed FFT calculations
                    calculate[i].FFTWaitToComplete();
                    //Start calculations  FFT data stream RTLSDR
                    calculate[i].Start(DataOut[i], dataRadio[i], flags.Cumulation[i], flags.Amplification[i]);
                    //Wait until completed calculations for streams
                    calculate[i].WaitToFinischCalc();

                    double ave = 0;
                    for (int k = 0; k < DataIn.Length; k++)
                        ave += Math.Sqrt(DataIn[i][k].Im * DataIn[i][k].Im + DataIn[i][k].Re * DataIn[i][k].Re); ;
                    ave /= DataIn.Length;

                    average[i] += ave * 0.05;
                    average[i] /= 1.05;

                    //Draw scenes waves
                    if (flags.showRadioWave[i])
                        window_wave[i].RenderWave(dataRadio[i], average[i], 0);

                    //Draw scenes flows
                    if (flags.showRadioFlow[i])
                        window_flow[i].RenderFlow(dataRadio[i]);
                }
            }
            );

            Parallel.For(Flags.MAX_DONGLES_RTLSDR, Flags.ALL_DONGLES, i =>
            {
                if (radioRSP1[i - Flags.MAX_DONGLES_RTLSDR].status & radio_window[i].Visible & (flags.showRadioWave[i] | flags.showRadioFlow[i]))
                {
                    lock (LockMainDataStream[i])
                    {
                        calculate[i].CopyToComplex(radioRSP1[i - Flags.MAX_DONGLES_RTLSDR].dataIQ, ref DataIn[i]);
                        for (int l = 0; l < DataIn.Length; l++)
                            if (DataIn[i][l].Re > max)
                                max = (int)DataIn[i][l].Re;
                    }
                    calculate[i].FFT(DataIn[i], DataOut[i]);

                    calculate[i].FFTWaitToComplete();
                    //Start calculations  FFT data stream RTLSDR
                    calculate[i].Start(DataOut[i], dataRadio[i], flags.Cumulation[i], flags.Amplification[i]);
                    //Wait until completed calculations for streams
                    calculate[i].WaitToFinischCalc();


                    double ave = 0;
                    for (int k = 0; k < DataIn.Length; k++)
                    {
                        ave += Math.Sqrt(DataIn[i][k].Im * DataIn[i][k].Im + DataIn[i][k].Re * DataIn[i][k].Re); ;

                    }
                    ave /= DataIn.Length;

                    average[i] += ave * 0.05;
                    average[i] /= 1.05;
                    if (flags.showRadioWave[i])
                    {
                        lost += radioRSP1[i - Flags.MAX_DONGLES_RTLSDR].lost;
                        count++;
                        if (count == 10)
                        {
                            lost_per_s = (uint)(lost / count);
                            count = lost = 0;
                        }
                        window_wave[i].RenderWave(dataRadio[i], average[i], lost_per_s);
                    }
                    if (flags.showRadioFlow[i])
                        window_flow[i].RenderFlow(dataRadio[i]);
                }
            }
            );
        }

        private void CalculateRadarScene()
        {
            while (!calculate_radar_exit)
            {
                if (!runing)
                {
                    Thread.Sleep(100);
                }
                else
                {
                    //RtlSdr
                    for (int i = 0; i < Flags.MAX_DONGLES_RTLSDR; i++)
                        if (radioRtlSdr[i].status & radioRtlSdr[i] != null && flags.showRadar[i] == true)
                            lock (LockMainDataStream[i])
                                Array.Copy(radioRtlSdr[i].dataIQ, 0, dataOutRadio[i], 0, radioRtlSdr[i].dataIQ.Length);


                    //SDRplay
                    for (int i = 0; i < Flags.MAX_DONGLES_RSP1; i++)
                        if (radioRSP1[i].status & radioRSP1[i] != null && flags.showRadar[i + Flags.MAX_DONGLES_RTLSDR] == true)
                            lock (LockMainDataStream[i + Flags.MAX_DONGLES_RTLSDR])
                                Array.Copy(radioRSP1[i].dataIQ, 0, dataOutRadio[i + Flags.MAX_DONGLES_RTLSDR], 0, radioRSP1[i].dataIQ.Length);

                    //Calculate ambiguity map 
                    if (runing)
                        try
                        {
                            //Calculate ambiguity for recivers
                            for (int i = 0; i < Flags.ALL_DONGLES; i++)
                                if (flags.showRadar[i] == true)
                                {
                                    ambiguity.ProcessGPU(dataOutRadio[i], null, dataRadar[i], flags);
                                }
                        }
                        catch (Exception ex)
                        {
                            String str = "Error ambiguity function. " + ex.ToString();
                            MessageBox.Show(str);
                            break;
                        }
                    ///////////////////////////////////////////////////////////////////////////////

                    //Calculate calculations per second
                    calculation++;
                    if (watch.ElapsedMilliseconds >= 1000L)
                    {
                        calculations_per_sec = calculation;
                        calculation = 0;
                        watch.Restart();
                    }
                }
            }
            calculate_radar_exited = true; //Thread exit normally
        }

        private void DrawMap()
        {
            //draw_map_exited = true;
            //return;
            float[][] PostProcMap = new float[Flags.MAX_DONGLES_RSP1 + Flags.MAX_DONGLES_RTLSDR][];

            float[][] dataRadar_copy = new float[Flags.MAX_DONGLES_RSP1 + Flags.MAX_DONGLES_RTLSDR][];
            for (int i = 0; i < Flags.MAX_DONGLES_RSP1 + Flags.MAX_DONGLES_RTLSDR; i++)
                dataRadar_copy[i] = new float[dataRadar[i].Length];

            while (!draw_map_exit)
            {
                if (mMap == null || runing == false)
                {
                    Thread.Sleep(100);
                }
                else
                {
                    if (dataRadar[0] != null || dataRadar[1] != null || dataRadar[2] != null || dataRadar[3] != null)
                    {
                        if (flags.ShowMap)
                        {
                            Parallel.For(0, Flags.ALL_DONGLES, i =>
                            {
                                //copy rugh data to local arrays
                                lock (LockMap[i])
                                {
                                    Array.Copy(dataRadar[i], 0, dataRadar_copy[i], 0, dataRadar[i].Length);
                                }

                                uint colrow = flags.Columns * flags.Rows;

                                PostProcMap[i] = new float[colrow];
                                map_cumulate[i].Run(dataRadar_copy[i], PostProcMap[i], flags.average);

                                if (flags.CorrectBackground)
                                {
                                    if (!flags.FreezeBackground)
                                        RegresionMap[i].Add(PostProcMap[i], flags.ColectEvery); //Add frames for regresion fit to find the best background correction, the second parameter describe how othen to add the element
                                     
                                    RegresionMap[i].CorrectBackground(PostProcMap[i], flags.CorectionWeight);
                                }

                                //Reduce rows
                                ReduceRows(ref PostProcMap[i]);

                                //Translated blops to values are in map after this
                                lock (LockMapBlops)
                                {
                                    mMap.pointFromRadar[i] = Finder.FindObject(PostProcMap[i], flags);
                                }
                                 
                            }
                        );
                        }
                    }
                    else
                        Thread.Sleep(100);
                }
            }

            draw_map_exited = true; //Thread exit normally
        }

        private void DrawRadarScene()
        {
            //draw_radar_exited = true;
            //return;

            float[][] dataRadar_copy = new float[Flags.ALL_DONGLES][];
            for (int i = 0; i < Flags.ALL_DONGLES; i++)
                dataRadar_copy[i] = new float[dataRadar[i].Length];

            while (!draw_radar_exit)
            {
                if (!runing)
                {
                    Thread.Sleep(100);
                }
                else
                {
                    //Average maps; ups it was slow
                    Parallel.For(0, Flags.ALL_DONGLES, i =>
                    {
                        if (flags.showRadar[i])
                        {
                            lock (LockRadarScene[i])
                            {
                                Array.Copy(dataRadar[i], 0, dataRadar_copy[i], 0, dataRadar[i].Length);

                            }

                            //average
                            radar_cumulate[i].Run(dataRadar_copy[i], PostProc[i], flags.average);

                            if (flags.CorrectBackground)
                            {
                                if (!flags.FreezeBackground)
                                    Regresion[i].Add(PostProc[i], flags.ColectEvery); //Add frames for regresion fit to find the best background correction, the second parameter describe how othen to add the element
                                Regresion[i].CorrectBackground(PostProc[i], flags.CorectionWeight);
                            }

                            ReduceRows(ref PostProc[i]);

                            if (flags.ShowMap == false)
                                windowRadar[i].RenderRadar(PostProc[i], flags, null, true);//expensive
                            else   //Draw radar with map data, must be separated with this configuration. To corectly calculate the position all 4 radar data are necessary  
                                windowRadar[i].RenderRadar(PostProc[i], flags, mMap.pointFromRadar[i], true);
                        }
                    }
                   );
                }
            }
            draw_radar_exited = true; //Thread exit normally
        }

        private void ReduceRows(ref float[] PostProc)
        {
            //Reduce rows
            if (flags != null)
            {
                uint ReducedRows = (flags.Rows / flags.NrReduceRows);

                if (PostProc.Length >= flags.Columns * flags.Rows)
                    for (uint i = 0; i < flags.Columns; i++)
                    {
                        uint index_i = i * flags.Rows;
                        uint index_r = i * ReducedRows;
                        float temp;
                        for (uint j = 0; j < ReducedRows; j++)
                        {
                            uint l = index_i + j * flags.NrReduceRows;
                            float tmp;
                            //Reduce points from the row, find the max in the group
                            temp = 0;
                            for (uint k = 0; k < flags.NrReduceRows; k++)
                            {
                                if ((tmp = PostProc[l + k]) > temp) //take max from the group
                                    temp = tmp;
                            }
                            PostProc[index_r + j] = temp;//only one?
                        }
                    }
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            foreach (WindowWave x in window_wave)
                x.OnLoadWindow();
            foreach (WindowFlow x in window_flow)
                x.OnLoadWindow();

            for (int i = 0; i < Flags.ALL_DONGLES; i++)
                windowRadar[i].OnLoadWindow();

            resizing = false;
            UpdateAllScenesWhenRunning();
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            resizing = true;
        }

        private void splitContainer_SplitterMoved(object sender, SplitterEventArgs e)
        {
            UpdateAllScenesWhenRunning();
        }

        //Size correction only for radio windows
        private void WindowsSizeCorection(int Nr)
        {
            StopDraw();//Thread stop
            if (radio_window[Nr].radio_resize == false)
            {
                window_wave[Nr].SizeChange();
                window_flow[Nr].SizeChange();
                UpdateAllScenes();
            }
            if (runing)
                StartDraw();//Thread start
        }

        //Size correction for all
        public void UpdateAllScenesWhenRunning()
        {
            StopDraw();//Thread stop
            if (resizing == false)
            {
                for (int i = 0; i < Flags.ALL_DONGLES; i++)
                    if (windowRadar != null && windowRadar[i] != null) windowRadar[i].SizeChange();

                if (window_wave != null)
                    foreach (WindowWave x in window_wave)
                        if (x != null) x.SizeChange();

                if (window_flow != null)
                    foreach (WindowFlow x in window_flow)
                        if (x != null) x.SizeChange();

                DrawRadio();
            }
            if (runing)
                StartDraw();//Thread start
        }

        //Scenes update when is not running
        void UpdateAllScenes()
        {
            if (resizing == false)
                if (FormsReady)
                {
                    StartDraw();
                    System.Threading.Thread.Sleep(20);
                    StopDraw();
                }
        }

        private void panelViewport3_MouseMove(object sender, MouseEventArgs e)
        {
            CalculateMouseMove(0, e.Location.X);
        }

        #region Mouse

        private void CalculateMouseUp(int Nr, int x)
        {
            MouseDownPanel[Nr] = false;
            UpdateFrequencies(Nr);
            FilterMove = false;
            LeftBandMove = false;
            RightBandMove = false;
            window_wave[Nr].panelViewport.Cursor = Cursors.Cross;
            UpdateFiltersAndScale(Nr);
        }

        private void CalculateMouseDown(int Nr, int x)
        {
            MouseDownPanel[Nr] = true;
            oldX_position[Nr] = x;
            //Find how many MHz is one pixel
            MHz_perPixel[Nr] = 1.0 / ((radio_window[Nr].panelRadioWave.Width - DrawWave.LeftMargin - DrawWave.RightMargin) / flags.rate[Nr]);

            float pos = (float)((x - DrawWave.LeftMargin) * MHz_perPixel[Nr] + flags.rate[Nr] / 2);
            float leftBand = (float)(flags.rate[Nr] - flags.FilterCentralFreq[Nr] - flags.RadioHalfBandwith);
            float rightBand = (float)(flags.rate[Nr] - flags.FilterCentralFreq[Nr] + flags.RadioHalfBandwith);
            if (pos > leftBand && pos < rightBand)
                FilterMove = true;
            if (pos > leftBand - 20000 && pos < leftBand)
                LeftBandMove = true;
            if (pos < rightBand + 20000 && pos > rightBand)
                RightBandMove = true;
        }

        private void CalculateMouseMove(int Nr, int x)
        {
            float half_rate = (float)flags.rate[Nr] / 2;
            MHz_perPixel[Nr] = 1.0 / ((radio_window[Nr].panelRadioWave.Width - DrawWave.LeftMargin - DrawWave.RightMargin) / flags.rate[Nr]);
            double PointedFrequency = (flags.frequency[Nr] - half_rate);
            float FrequencyAtCursorPosition = (float)(PointedFrequency + (x - DrawWave.LeftMargin) * MHz_perPixel[Nr]) / 1000000;

            if (MouseDownPanel[Nr] == false)
            {
                WindowsLocation(FrequencyAtCursorPosition);

                float pos = (float)((x - DrawWave.LeftMargin) * MHz_perPixel[Nr] + half_rate);
                float leftBand = (float)(flags.rate[Nr] - flags.FilterCentralFreq[Nr] - flags.RadioHalfBandwith);
                float rightBand = (float)(flags.rate[Nr] - flags.FilterCentralFreq[Nr] + flags.RadioHalfBandwith);

                if (pos > leftBand - 20000 && pos < leftBand)
                    window_wave[Nr].panelViewport.Cursor = Cursors.VSplit;
                else
                if (pos < rightBand + 20000 && pos > rightBand)
                    window_wave[Nr].panelViewport.Cursor = Cursors.VSplit;
                else
                    window_wave[Nr].panelViewport.Cursor = Cursors.Cross;
            }
            else
            {
                int delta = x - oldX_position[Nr];
                double s = delta * MHz_perPixel[Nr];

                if (FilterMove)
                {
                    flags.FilterCentralFreq[Nr] -= (float)s;
                    float band = (float)(flags.FilterCentralFreq[Nr] + half_rate);
                    if (band < 1) flags.FilterCentralFreq[Nr] = 1 - half_rate;
                    else if (band > flags.rate[Nr]) flags.FilterCentralFreq[Nr] = half_rate;
                    if (band >= 1 && band <= flags.rate[Nr])
                        oldX_position[Nr] = x;
                    UpdateFiltersAndScale(Nr);
                }
                if (LeftBandMove)
                {
                    flags.RadioHalfBandwith -= (float)s;
                    if (flags.RadioHalfBandwith < 10) flags.RadioHalfBandwith = 10;
                    else if (flags.RadioHalfBandwith > 250000 / 2) flags.RadioHalfBandwith = 250000 / 2;
                    if (flags.RadioHalfBandwith >= 10 && flags.RadioHalfBandwith < 250000 / 2)
                    {
                        oldX_position[Nr] = x;
                        UpdateFiltersAndScale(Nr);
                        //Cursor
                        window_wave[Nr].panelViewport.Cursor = Cursors.VSplit;
                    }
                }
                if (RightBandMove)
                {
                    flags.RadioHalfBandwith += (float)s;
                    if (flags.RadioHalfBandwith < 10) flags.RadioHalfBandwith = 10;
                    else if (flags.RadioHalfBandwith > 250000 / 2) flags.RadioHalfBandwith = 250000 / 2;
                    if (flags.RadioHalfBandwith >= 10 / 2 && flags.RadioHalfBandwith < 250000 / 2)
                    {
                        oldX_position[Nr] = x;
                        UpdateFiltersAndScale(Nr);
                        //Cursor
                        window_wave[Nr].panelViewport.Cursor = Cursors.VSplit;
                    }
                }
                if (!FilterMove & !LeftBandMove & !RightBandMove)
                {
                    flags.frequency[Nr] -= s;

                    UpdateFrequencies(Nr);
                    oldX_position[Nr] = x;
                }
            }
        }

        void UpdateFiltersAndScale(int Nr)//Nr-active radio at the moment
        {
            for (int i = 0; i < Flags.ALL_DONGLES; i++)
            {
                window_wave[i].Location(-1, (float)(flags.rate[i] / 2 - (flags.FilterCentralFreq[i] + flags.RadioHalfBandwith)) / 1000000, (float)(flags.rate[i] / 2 - (flags.FilterCentralFreq[i] - flags.RadioHalfBandwith)) / 1000000);
                window_flow[i].Location(-1);
            }
        }

        void WindowsLocation(float FrequencyAtCursorPosition)
        {
            // windowBackground.Location(FrequencyAtCursorPosition);

            for (int i = 0; i < Flags.ALL_DONGLES; i++)
            {
                window_wave[i].Location(FrequencyAtCursorPosition, (float)(flags.rate[i] / 2 - (flags.FilterCentralFreq[i] + flags.RadioHalfBandwith)) / 1000000, (float)(flags.rate[i] / 2 - (flags.FilterCentralFreq[i] - flags.RadioHalfBandwith)) / 1000000);
            }
            foreach (WindowFlow x in window_flow)
                x.Location(FrequencyAtCursorPosition);
        }
        #endregion


    }
}
