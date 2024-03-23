using PasiveRadar;
using System;
using System.Collections.Generic;

namespace SDRdue
{
    //Find blops from the radar scene
    public static class Finder
    {

        static int SizeX, SizeY;

        static float[] space;
        static bool[,] tested;  //Tested neighbouring points
        static bool[,] found;  //Scaned points (the center point around which are scaned points)

        //scan_neighbours relative positions
        static MapPoints[] scan_neighbours = new MapPoints[]
        {
                new MapPoints(-1,0), new MapPoints(1,0), new MapPoints(0,1), new MapPoints(0,-1),
              //  new MapPoints(-1,1), new MapPoints(1,1), new MapPoints(-1,-1), new MapPoints(1,-1)
        };

        public struct MapPoints
        {
            public int row;//index columns
            public int col;//index rows (reduced)
            public int size; //number of points inblop
            public int w;

            public MapPoints(int _X = 0, int _Y = 0, int _w = 0, int _size = 0)
            {
                row = _X;
                col = _Y;
                w = _w;
                size = _size;
            }

            public static MapPoints operator +(MapPoints f1, MapPoints f2)
            {
                var f3 = new MapPoints(f1.row + f2.row, f1.col + f2.col);
                return f3;
            }

            public static MapPoints operator &(MapPoints f1, MapPoints f2)
            {
                f1.row += f2.row * f2.w;
                f1.col += f2.col * f2.w;
                f1.w += f2.w;
                return f1;
            }
            public static MapPoints operator /(MapPoints f1, int f2)
            {
                f1.row /= f2;
                f1.col /= f2;
                return f1;
            }
            public static MapPoints Norm(ref MapPoints f1)
            {
                if (f1.w != 0)
                {
                    f1.row = (int)Math.Round(1.0 * f1.row / f1.w);
                    f1.col = (int)Math.Round(1.0 * f1.col / f1.w);
                }
                return f1;
            }
        };


        //Inputs: data
        //flags
        //output: MapPoints
        public static List<MapPoints> FindObject(float[] data, Flags flags)
        {
            List<MapPoints> ListMap = new List<MapPoints>();

            if (data.Length == flags.Columns * flags.Rows)
                ListMap = Finder.FindAllBlops(data, flags);

            return ListMap;
        }




        public static List<MapPoints> FindAllBlops(float[] _space, Flags flags)
        {

            SizeX = (int)flags.Columns;
            SizeY = (int)(flags.Rows / flags.NrReduceRows);

            tested = new bool[SizeX, SizeY];
            found = new bool[SizeX, SizeY];
            space = _space;

            var found_points = new List<MapPoints>();

            for (int i = 0; i < SizeX; ++i)
                for (int j = 0; j < SizeY; ++j)
                {
                    var n = new MapPoints(i, j);
                    if (TestPoint(ref n, flags))
                    {
                        MapPoints m = FindBlop(n, flags);
                        if (m.size > flags.NrPointsOfObject) //accept only found blops with minimal size of NrPointsOfObject 
                            found_points.Add(m);
                    }
                }

            found_points.Sort((x, y) => x.size.CompareTo(y.size));


            return found_points; //Found points in a single blop
        }

        static MapPoints FindBlop(MapPoints point, Flags flags)
        {
            var blop_points = new List<MapPoints>(500)
            {
                point //The first found point
            };

            //find the rest of points in the blop (the heart of the algorithm)
            for (int i = 0; i < blop_points.Count; ++i)
                if (found[blop_points[i].row, blop_points[i].col] == false)
                    blop_points.AddRange(ScanNeighburst(blop_points[i], flags));

            return Average(blop_points, flags);
        }


        static List<MapPoints> ScanNeighburst(MapPoints point, Flags flags)
        {
            MapPoints neighbour;
            //Set flags for the investigated point
            tested[point.row, point.col] = found[point.row, point.col] = true;

            List<MapPoints> surrounded_points = new List<MapPoints>(scan_neighbours.Length);

            //Test all surrounded points
            for (int n = 0; n < scan_neighbours.Length; ++n)
            {
                neighbour = point + scan_neighbours[n];
                if (TestPointFull(ref neighbour, flags))
                    surrounded_points.Add(neighbour);
            }

            return surrounded_points;
        }


        static bool TestPointFull(ref MapPoints point, Flags flags)
        {
            if (point.row < SizeX / 2 - flags.DistanceFrom0line || point.row > SizeX / 2 + flags.DistanceFrom0line)
                if (point.row > 0 && point.col > 0 && point.row < SizeX && point.col < SizeY)
                    return TestPoint(ref point, flags);
            return false;
        }

        static bool TestPoint(ref MapPoints point, Flags flags)
        {
            if (!tested[point.row, point.col])
            {
                float amplitude;
                tested[point.row, point.col] = true;
                if ((amplitude = Position(point.row, point.col)) >= flags.AmplitudeOfAccepterPoints)
                {
                    point.w = (int)amplitude;
                    return true;
                }
            }
            return false;

        }

        static MapPoints Average(List<MapPoints> blop_points, Flags flags)
        {
            MapPoints ave = new MapPoints();

            if (blop_points.Count > 0)
            {
                if (flags.Integration) //center of mass
                {
                    for (int i = 0; i < blop_points.Count; ++i)
                        ave &= blop_points[i];

                    MapPoints.Norm(ref ave);
                }
                else
                {
                    for (int i = 0; i < blop_points.Count; i++)
                        ave += blop_points[i];

                    ave /= blop_points.Count;
                }
            }

            ave.size = blop_points.Count;

            if (ave.row < 0 || ave.col < 0)
                return new MapPoints();

            return ave;
        }

        static float Position(int x, int y)
        {
            return (space[x * SizeY + y]);
        }

    }
}

