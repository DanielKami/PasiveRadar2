using Microsoft.Maps.MapControl.WPF;
using Microsoft.Xna.Framework;
using SDRdue;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using static SDRdue.BiStatic;

namespace PasiveRadar
{
    public partial class Map : Form
    {
        const double f = 3.35281066474748E-3;		    // Flattening factor							*/
        const double twopi = 6.28318530717958623;       // 2*Pi	
        const double J2000 = 2451545.0;					// 2000 January 1.5TD  
        const double xkmper = 6.378137E3;				// WGS 84 Earth radius km
        const double pio2 = 1.57079632679489656;		// Pi/2	
        const double e2f = f * (2.0 - f);
        const double secday = 8.6400E4;                 // Seconds per day	
        const double omega_E = 1.00273790934;           // Earth rotations/siderial day	

        Flags flags;
        public List<Finder.MapPoints>[] pointFromRadar = new List<Finder.MapPoints>[Flags.ALL_DONGLES];

        public Map()
        {
            InitializeComponent();

            for (int i = 0; i < Flags.ALL_DONGLES; i++)
                pointFromRadar[i] = new List<Finder.MapPoints>();


            //location = new StringBuilder("https://www.bing.com/maps/?cp=51.250312%7E22.527466");


            //mapControll.MyMap.ViewChangeOnFrame += new EventHandler<MapEventArgs>(MyMap_ViewChangeOnFrame);
            mapControll.MyMap.ZoomLevel = 9;

            // Create a timer with a one second interval.
            System.Windows.Forms.Timer tm = new System.Windows.Forms.Timer();
            tm.Tick += Tm_Tick;
            tm.Interval = 1000;
            tm.Start();
        }

        //  https://learn.microsoft.com/en-us/previous-versions/bing/wpf-control/hh709044(v=msdn.10)
        private void Tm_Tick(object sender, EventArgs e)
        {
            if (flags != null)
                Operate(flags);
        }

        public void CopyFlags(Flags flags_)
        {
            flags = flags_;
        }
        void MyMap_ViewChangeOnFrame(object sender, MapEventArgs e)
        {
            //Gets the map that raised this event
        }

        /*
         * 
         3	490	MUX-3	Lublin "Raabego"	1	~2.37	
        12	226.5	MUX-8	Lublin "Piaski"	35	~25.361	
        21	474	MUX-4T2	Lublin "Piaski"	100	~25.361	
        23	490	MUX-3	Lublin "Piaski"	100	~25.361	
        25	506	MUX-1T2	Lublin "Piaski"	100	~25.361	
        39	618	MUX-2T2	Lublin "Piaski"	100	~25.361	
        23	490	MUX-3	Lublin "Boży Dar"	20	~28.917	
        23	490	MUX-3	Kazimierz Dolny "Góry I"	2.5	~42.473	
        22	482	MUX-1T2	Dęblin "Ryki"	20	~58.145	
        32	562	MUX-4T2	Dęblin "Ryki"	20	~58.145	
        33	570	MUX-3	Dęblin "Ryki"	25	~58.145	
        44	658	MUX-2T2	Dęblin "Ryki"	20	~58.145	
         */

        public void Operate(Flags flags)
        {
            BiStatic.Data BiTable = new BiStatic.Data();
            List<BiStatic.Data> ResultsABC = new List<BiStatic.Data>();
            List<BiStatic.Data> ResultsABD = new List<BiStatic.Data>();
            List<Vector3> Results = new List<Vector3>();

            float[] pixel_km = new float[Flags.ALL_DONGLES];
            for (int i = 0; i < Flags.ALL_DONGLES; i++)
                pixel_km[i] = 299792.458f / flags.rate[i]; //distance related to one pixel 

            mapControll.MyMap.Children.Clear();


            //BTV1 "Piaski"
            Location locationA = new Location(51.13388, 22.869722);

            //BTV2  "Rabego"
            Location locationB = new Location(51.244041, 22.544852);

            //BTV2  "Ryki"
            Location locationC = new Location(51.623889, 21.968333);

            //BTV2  "any"
            Location locationD = new Location(51.623889, 21.868333);

            //Observer  "Observer"
            Location locationO = new Location(51.2340383, 22.54236);

            //ECI system ad JD for rotation earth in case we want real eci in time
            var A = Calculate_Eci(locationA, 0);
            var B = Calculate_Eci(locationB, 0);
            var C = Calculate_Eci(locationC, 0);
            var D = Calculate_Eci(locationD, 0);
            var O = Calculate_Eci(locationO, 0);

            double AO = Vector3.Distance(A, O);
            double BO = Vector3.Distance(B, O);
            double CO = Vector3.Distance(C, O);
            double DO = Vector3.Distance(D, O);

            BiTable.teta = CalculateAngle(A, O, B);
            var phi1 = CalculateAngle(A, O, C);
            var phi2 = CalculateAngle(A, O, D);

            if (pointFromRadar[0].Count > 0 && pointFromRadar[1].Count > 0 && pointFromRadar[2].Count > 0)
            {
                //A
                for (int i = 0; i < pointFromRadar[0].Count; i++)
                {
                    //B
                    BiTable.X = pixel_km[0] * pointFromRadar[0][i].col;// AP + PO - AO;//test

                    for (int j = 0; j < pointFromRadar[1].Count; j++)
                    {
                        //C
                        BiTable.phi = phi1;
                        BiTable.Y = pixel_km[1] * pointFromRadar[1][j].col;// BP + PO - BO;//test
                        for (int k = 0; k < pointFromRadar[2].Count; k++)
                        {
                            BiTable.Z = pixel_km[2] * pointFromRadar[2][k].col;// CP + PO - CO;//test
                            BiStatic.CalculateDistance3(AO, BO, CO, BiTable.X, BiTable.Y, BiTable.Z, ref BiTable);
                            ResultsABC.Add(BiTable);
                        }

                        //D
                        BiTable.phi = phi2;
                        for (int k = 0; k < pointFromRadar[3].Count; k++)
                        {
                            BiTable.Z = pixel_km[3] * pointFromRadar[3][k].col;// CP + PO - CO;//test
                            BiStatic.CalculateDistance3(AO, BO, DO, BiTable.X, BiTable.Y, BiTable.Z, ref BiTable);
                            ResultsABD.Add(BiTable);
                        }
                    }
                }

                float adist = 0.01f * flags.AcceptedDistance;
                for (int i = 0; i < ResultsABC.Count; i++)
                {
                    for (int j = 0; j < ResultsABD.Count; j++)
                    {
                        //Compare results for two data sets
                        if (ResultsABD[j].d < ResultsABC[i].d + adist && ResultsABD[j].d > ResultsABC[i].d - adist)
                            if (ResultsABD[j].a < ResultsABC[i].a + adist && ResultsABD[j].a > ResultsABC[i].a - adist)

                            {
                                //both arte the same so one ResultsABC[i] is enough
                                var v = BiStatic.Intersect3spheres(A, B, O, ResultsABC[i].a, ResultsABC[i].b, ResultsABC[i].d);
                                Results.Add(v);
                            }
                    }
                }
                label4.Text = "Points=" + Results.Count;

                // The pushpin (represents objects) create.
                Pushpin[] pin = new Pushpin[Results.Count];
                for (int i = 0; i < Results.Count; ++i)
                    pin[i] = new Pushpin();

                //Add puspins to the map
                for (int i = 0; i < Results.Count; i++)
                {
                    pin[i].Location = Calculate_LatLonAlt(Results[i], 0);
                    pin[i].Content = i;
                }

                // Adds the pushpin to the map.
                for (int i = 0; i < Results.Count; ++i)
                    mapControll.MyMap.Children.Add(pin[i]);

                //mapControll.MyMap.Children.Add(pin4);
                mapControll.MyMap.Center = locationO;

                //Add circles
                AddCircle(locationA, (float)BiTable.a, System.Windows.Media.Colors.Blue);
                AddCircle(locationB, (float)BiTable.b, System.Windows.Media.Colors.Red);
                AddCircle(locationC, (float)BiTable.c, System.Windows.Media.Colors.Orange);
                AddCircle(locationO, (float)BiTable.d, System.Windows.Media.Colors.Green);
            }

        }
        static float Error(Vector3 v1, Vector3 v2)
        {
            Vector3 average = (v1 + v2) / 2;
            float n = 2;

            float err_x1 = (float)Sqr(average.X - v1.X);
            float err_y1 = (float)Sqr(average.Y - v1.Y);
            float err_z1 = (float)Sqr(average.Z - v1.Z);

            float err_x2 = (float)Sqr(average.X - v2.X);
            float err_y2 = (float)Sqr(average.Y - v2.Y);
            float err_z2 = (float)Sqr(average.Z - v2.Z);

            float res_x = (float)(Math.Sqrt(err_x1 + err_x2) / (n - 1));
            float res_y = (float)(Math.Sqrt(err_y1 + err_y2) / (n - 1));
            float res_z = (float)(Math.Sqrt(err_z1 + err_z2) / (n - 1));

            return (float)(Math.Sqrt(Sqr(res_x) + Sqr(res_y + res_z)));

        }

        void AddBTSposition(Location location, System.Windows.Media.Color color)
        {
            MapPolygon polygon = new MapPolygon();
            polygon.Fill = new System.Windows.Media.SolidColorBrush(color);
            polygon.Stroke = new System.Windows.Media.SolidColorBrush(color);
            polygon.StrokeThickness = 3;
            polygon.Opacity = 0.7;
            polygon.Locations = new LocationCollection() {
               new Location(-0.001+location.Latitude, 0.001+location.Longitude),
               new Location(0+location.Latitude, 0+location.Longitude),
               new Location(-0.001+location.Latitude, -0.001+location.Longitude)
            };

            mapControll.MyMap.Children.Add(polygon);
        }

        void AddCircle(Location location, float radius, System.Windows.Media.Color color)
        {
            MapPolygon polygon = new MapPolygon();
            polygon.Fill = new System.Windows.Media.SolidColorBrush(color);
            polygon.Stroke = new System.Windows.Media.SolidColorBrush(color);
            polygon.StrokeThickness = 3;
            polygon.Opacity = 0.2;
            radius /= 111f;
            polygon.Locations = new LocationCollection();

            for (double i = 0; i < 2 * Math.PI; i += 0.1f)
            {
                double lat1 = location.Latitude;
                double lon1 = location.Longitude;

                Location l = new Location();
                l.Latitude = lat1 + Math.Sin(i) * radius;
                l.Longitude = lon1 + Math.Cos(i) * radius * 1.6;
                polygon.Locations.Add(l);
            }

            mapControll.MyMap.Children.Add(polygon);
        }


        /////////////////////////////////////////////////////////////////////////////////
        ///
        /// 
        ///Module:       Julian_Date_of_Year
        ///description:  calculae Julian day from year
        ///Created by: Daniel
        ///Modyfied by: 
        ///Data: 2007
        public double Julian_Date_of_Year(double year)
        {
            /* The function Julian_Date_of_Year calculates the Julian Date  */
            /* of Day 0.0 of {year}. This function is used to calculate the */
            /* Julian Date of any date by using Julian_Date_of_Year, DOY,   */
            /* and Fraction_of_Day. */

            /* Astronomical Formulae for Calculators, Jean Meeus, */
            /* pages 23-25. Calculate Julian Date of 0.0 Jan year */

            int A, yr;
            double Julian_Data;

            if ((year <= 1582) || (year > 3000)) return -1;

            //year
            yr = (int)(year - 1);
            A = Trunk(yr * 0.01);
            //	Julian_Data=Trunk(365.25*yr)+Trunk(30.6001*14)+1720994.5+2.0-A+Trunk(A*0.25);
            Julian_Data = 1720994.5 + (int)(365.25 * yr) + (int)(30.6001 * 14) + 2.0 - A + (int)(A * 0.25);
            return Julian_Data;
        }

        ///Module:       JD
        ///description:  calculae Julian day from time
        ///Created by: Daniel
        ///Modyfied by: 
        ///Data: 2007
        double JD(int year, int mo, int dy, int hr, int mn, int sc, int msec)
        {
            double Julian_Data, seconds;
            int[] days = { 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };
            int i, day;

            //year
            Julian_Data = Julian_Date_of_Year(year);

            // 1582 A.D.: 10 days removed from calendar
            // 3000 A.D.: Arbitrary error checking limit
            if (dy < 0.0 || dy > 31 || Julian_Data == -1) return -1;

            //day
            day = 0;
            for (i = 0; i < mo - 1; ++i)
            {
                day += days[i];
            }
            day += dy;
            if (((year % 4) == 0 && (year % 100) != 0 || (year % 400) == 0) && mo > 1)
                day += 1;

            //second of the day
            seconds = hr * 3600.0 + mn * 60.0 + sc + 0.001 * msec;
            seconds /= secday; //return in J days

            return (Julian_Data + day + seconds);
        }

        ///Module:       ThetaG_
        ///description:  Calculate theta G from julian day
        ///Created by:   Daniel
        ///Modyfied by: 
        ///Data: 2007
        ///Additional :  Used partly in the code
        public double ThetaG(double JulianTime)
        {
            double UT, TU, GMST;

            UT = Frac(JulianTime + 0.5);

            TU = ((JulianTime - UT) - J2000) / 36525.0;

            GMST = 24110.54841 + TU * (8640184.812866 + TU * (0.093104 - TU * 0.0000062));

            GMST = Math.IEEERemainder((GMST + secday * omega_E * UT), secday);
            if (GMST < 0) GMST += secday;

            return (twopi * GMST / secday);
        }
        ///Module:        Calculate_Eci
        ///description:   calculate ECI from geodetic position radians
        ///Created by:    Astronomical Almanac
        ///Modyfied by:   Daniel
        ///Data: 		  1992
        ///Additional :   If we are in the sam local place and all point rotated with the same JD than we can ignore this JD=0 <summary>
        /// Module:        Calculate
        /// </summary>
        /// <param name="location"></param>
        /// <param name="JD"></param>
        /// <returns></returns>
        public Vector3 Calculate_Eci(Location location, float JD)
        {
            //Convert to radians
            double lat = location.Latitude * Math.PI / 180.0;
            double lon = location.Longitude * Math.PI / 180.0;

            Vector3 eci;
            // f = Earth oblateness flattening factor, re = equatorial radius:
            //Get Greenwich sidereal time:
            //yd=year*1000L+doy
            double gst, ra, dec, rnx, rny, rnz, rs, cos_dec;
            gst = lon + ThetaG(JD);
            // Calculate  right ascension:
            ra = Math.Atan2(Math.Sin(gst), Math.Cos(gst)); //AcTan

            // Calculate declination:
            dec = Math.Atan(Math.Tan(lat) * Sqr(1.0 - f));
            cos_dec = Math.Cos(dec);
            // Calculate normalized position vector:
            rnx = Math.Cos(ra) * cos_dec;
            rny = Math.Sin(ra) * cos_dec;
            rnz = Math.Sin(dec);

            // Calculate length of position vector:
            rs = location.Altitude + xkmper * (1.0 - f) / (Math.Sqrt(1.0 - f * (2.0 - f) * cos_dec * cos_dec));

            // Calculate position vector:
            eci.X = (float)(rnx * rs);
            eci.Y = (float)(rny * rs);
            eci.Z = (float)(rnz * rs);

            return eci;
        }

        ///Module:       Calculate_LatLonAlt
        ///Input: pos in ECI Vector3
        ///JD -julian time can be 0 for local calculations for all objects
        ///Output: Geodetic position of object - Location
        ///description:   
        /// Procedure Calculate_LatLonAlt will calculate the geodetic  
        ///position of an object given its ECI position pos and time. 
        ///It is intended to be used to determine the ground track of 
        ///a satellite.  The calculations  assume the earth to be an  
        ///oblate spheroid as defined in WGS '72.                     
        ///Reference:  The 1992 Astronomical Almanac, page K12.		  

        ///Created by:    The Astronomical Almanac
        ///Modyfied by:   Daniel
        ///Data: 		1992
        ///Additional :  
        public Location Calculate_LatLonAlt(Vector3 pos, double JD)
        {

            double r, phi, c, sin_phi;
            int loop_counter = 0;
            Location location = new Location();


            double theta = Math.Atan2(pos.Y, pos.X); /* radians */
            location.Longitude = Math.IEEERemainder(theta - ThetaG(JD), Math.PI * 2.0); /* radians */
            r = Math.Sqrt(pos.X * pos.X + pos.Y * pos.Y);
            location.Latitude = Math.Atan2(pos.Z, r); /* radians */

            do
            {
                loop_counter++;
                phi = location.Latitude;
                sin_phi = Math.Sin(phi);
                c = 1.0 / Math.Sqrt(1.0 - e2f * sin_phi * sin_phi);
                location.Latitude = Math.Atan2(pos.Z + xkmper * e2f * c * sin_phi, r);

            } while (loop_counter < 20 && Math.Abs(location.Latitude - phi) >= 1E-7);//1E-8

            double sin_lat = Math.Sin(location.Latitude);
            double cos_lat = Math.Cos(location.Latitude);

            location.Altitude = r / cos_lat - xkmper * c; /* kilometers */

            if (location.Latitude > pio2)
                location.Latitude -= twopi;


            location.Longitude *= 180.0 / Math.PI;
            location.Latitude *= 180.0 / Math.PI;

            return location;
        }

        static double Sqr(double x)
        {
            return (x * x);
        }


        public int Trunk(double value)
        {
            if (value > 0)
                return (int)Math.Floor(value);
            else
                return (int)Math.Ceiling(value);
        }

        ///return the fractional part of the number
        public double Frac(double value1)
        {
            return (value1 - Trunk(value1));
        }



        private void button1_Click(object sender, EventArgs e)
        {

            //Operate();
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }
    }
}
