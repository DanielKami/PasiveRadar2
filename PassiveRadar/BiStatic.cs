using Microsoft.Xna.Framework;
using System;

namespace SDRdue
{
    internal class BiStatic
    {

        readonly static float MinDistance = 1;//Minimum distance between points from pairs ABD, BCD to be accepted

        public struct Data
        {
            //Input
            public double X, Y, Z;

            public double a;
            public double b;      //distane A-P
            public double c;      //Distance P-O
            public double d;      //Distance P-B
            public double h;      //P height over ground

            public double alpha;  //angle AOP
            public double beta;   //angle BOP
            public double gamm;   //angle AOB
            public double teta;   //teta = alpha + beta, it is known
            public double phi;    //phi = alpha + gamma, it is known

            public Vector3 v1;
        }

        public static double CalculateAngle(Vector3 v1, Vector3 v2, Vector3 v3)
        {
            double distance1 = Vector3.Distance(v1, v2);
            double distance2 = Vector3.Distance(v2, v3);
            double distance3 = Vector3.Distance(v1, v3);

            //ABC = arc cos (AB*AB + BC*BC - AC*AC) / (2*AB*AC)

            double temp_calc = Sqr(distance1) + Sqr(distance2) - Sqr(distance3);
            temp_calc /= 2.0 * distance1 * distance2;
            return Math.Acos(temp_calc);
        }

        static double CalculateDistance(double a, double delta_a, double alpha)
        {
            double c1, c2;

            c1 = Sqr(delta_a) + 2.0 * a * delta_a;
            c2 = 2.0 * (delta_a + a - a * Math.Cos(alpha));
            if (c2 == 0) return 0;
            return c1 / c2;
        }

        //Precise
        static void CalculateDistance2(double a, double e, double delta_a, double delta_e, ref Data dat)
        {
            //All geometry is in surface of plane AOB


            double A, B, C, D;

            if (delta_e != 0)
            {
                A = (Sqr(delta_a) + 2.0 * a * delta_a) / (Sqr(delta_e) + 2.0 * e * delta_e);

                if (A != 0)
                {
                    B = a / A - e * Math.Cos(dat.gamm);
                    C = Sqr(e * Math.Sin(dat.gamm));
                    D = (delta_a + a) / A - delta_e - e;

                    double a_d, b_d, c_d;

                    a_d = 2.0 * (Sqr(B) + C);
                    b_d = -2.0 * B * D;
                    c_d = Sqr(D) - C;

                    double delta = Sqr(b_d) - 2.0 * a_d * c_d;

                    if (delta >= 0)
                    {
                        double sqrt_del = Math.Sqrt(delta);

                        //dat.x1 = (-b_d - sqrt_del) / a_d;
                        //dat.x2 = (-b_d + sqrt_del) / a_d;

                        //in anle means it is in the part of space limited by the sharp nagle AOB (between its arms)
                        //If point S is inside AOB angle than is x1
                        //if point s is outsaid of the angle AOB than is x2


                        //dat.alpha1 = Math.Acos(dat.x1);
                        //dat.beta1 = dat.gamm - dat.alpha1;
                        //dat.c1 = CalculateDistance(a, delta_a, dat.alpha1);
                        //dat.b1 = delta_a + a - dat.c1;
                        //dat.d1 = delta_e + e - dat.c1;


                        //dat.alpha2 = Math.Acos(dat.x2);
                        //dat.beta2 = dat.gamm - dat.alpha2;
                        //dat.c2 = CalculateDistance(a, delta_a, dat.alpha2);
                        //dat.b2 = delta_a + a - dat.c2;
                        //dat.d2 = delta_e + e - dat.c2;

                    }
                }
            }
        }


        public static void CalculateDistance3(double OA, double OB, double OC, double X, double Y, double Z, ref Data dat)
        {
            //All geometry is in ECI (km)

            double S1, S2, S3, S4, S5, S6, S7, S8, S9, S10, S11, S12;

            S1 = 2.0 * OA * X + Sqr(X);
            S2 = OA + X;
            S3 = 2.0 * OB * Y + Sqr(Y);
            S4 = OB + Y;
            S5 = 2.0 * OC * Z + Sqr(Z);
            S6 = OC + Z;

            //Can be calculated once for given BTV positions
            S7 = Math.Cos(dat.teta);
            S8 = Math.Sin(dat.teta);
            S9 = Math.Cos(dat.phi);
            S10 = Math.Sin(dat.phi);

            S11 = OA / OB;
            S12 = OA / OC;

            double P1, P2, P3, P4;

            P1 = S11 / S8;
            P2 = S7 / S8;
            P3 = S12 / S10;
            P4 = S9 / S10;

            double P = P1 * S3 - P2 * S1 - P3 * S5 + P4 * S1;
            double Q = 2.0 * (P4 * S2 - P2 * S2 + P1 * S4 - P3 * S6);

            // d is the  PO distance (Plane - Observer)
            if (Q == 0) return; //just in case

            dat.d = P / Q;
            dat.a = S2 - dat.d;
            dat.b = S4 - dat.d;
            dat.c = S6 - dat.d;

            double S1dS2 = S2 * dat.d + S1;

            //calculate alpha and dependent angles
            double N1 = S11 * (dat.d * S4 + S3) - S7 * S1dS2;
            double N2 = S8 * S1dS2;

            dat.alpha = Math.Atan2(N1, N2) + Math.PI;
            dat.beta = dat.teta - dat.alpha;
            dat.gamm = dat.phi - dat.alpha;

            double w = S1dS2 / OA / Math.Cos(dat.alpha);

            // only positive solution becouse over ground
            dat.h = Math.Sqrt(Sqr(w / 2.0) + Sqr(dat.d));

            //ground vectors
            //a1=sqrt(a*a-h*h)
            //b1=sqrt(b*b-h*h)
            //c1=sqrt(c*c-h*h)
            //d1=sqrt(d*d-h*h)
        }



        static double Sqr(double x) { return (x * x); }

        //	
        //Find the intersection(s) (x,y,z) and (x_,y_,z_) of three spheres centered
        //at (x1,y1,z1), (x2,y2,z2) and (x3,y3,z3) with corresponding radii of r1, r2, and r3
        //Adapted from http://mathforum.org/library/drmath/view/64311.html


        public static Vector3 Intersect3spheres(Vector3 v1, Vector3 v2, Vector3 v3, double r1, double r2, double r3)
        {
            double x1, x2, x3, y1, y2, y3, z1, z2, z3;

            x1 = v1.X; x2 = v2.X; x3 = v3.X;
            y1 = v1.Y; y2 = v2.Y; y3 = v3.Y;
            z1 = v1.Z; z2 = v2.Z; z3 = v3.Z;

            double a1, b1, c1, k1, a3, b3, c3, k3, a31, b31,
                e, f, g, h, A, B, C, x, y, z, x_, y_, z_, rootD, temp;
            /*
                Three spheres:
                EQ1: (x1 - x)^2 + (y1 - y)^2 + (z1 - z)^2 = r1^2
                EQ2: (x2 - x)^2 + (y2 - y)^2 + (z2 - z)^2 = r2^2
                EQ3: (x3 - x)^2 + (y3 - y)^2 + (z3 - z)^2 = r3^2	

                1. Pick one of the equations (EQ2) and subtract it from the other two (EQ1, EQ3).
                That will make those other two equations into linear equations in the three unknowns.  
            */

            // Subtract EQ2 from EQ1, move all constants to right side
            // Call the right side constant k1
            k1 = r1 * r1 - r2 * r2 - x1 * x1 + x2 * x2 - y1 * y1 + y2 * y2 - z1 * z1 + z2 * z2;

            // Left side of EQ1 is of the form a1x + b1y + c1z
            // where a1, b1, and c1 are the coefficients	
            a1 = 2.0 * (x2 - x1);
            b1 = 2.0 * (y2 - y1);
            c1 = 2.0 * (z2 - z1);

            // Subtract EQ2 from EQ3, move all constants to right side
            // Call the right side constant k3
            k3 = r3 * r3 - r2 * r2 - x3 * x3 + x2 * x2 - y3 * y3 + y2 * y2 - z3 * z3 + z2 * z2;

            // Left side of EQ3 is of the form a3x + b3y + c3z
            // where a3, b3, and c3 are the coefficients		
            a3 = 2.0 * (x2 - x3);
            b3 = 2.0 * (y2 - y3);
            c3 = 2.0 * (z2 - z3);

            // The two equations (EQ1, EQ3) are now linear equations in the three unknowns:
            // EQ1: a1x + b1y + c1z = k1
            // EQ3: a3x + b3y + c3z = k3	


            /*	
                2. Use them to find two of the variables (x, y) as linear expressions in the
                third (z).  These two equations are those of a line in 3-space, which
                passes through the two points of intersection of the three spheres. 
            */

            // Find y as a linear expression of z.
            // y = ez + f

            if (a1 == 0)
            {
                // y = -(c1/b1)z + k1/b1
                e = -c1 / b1;
                f = k1 / b1;
            }
            else if (a3 == 0)
            {
                // y = -(c3/b3)z + k3/b3
                e = -c3 / b3;
                f = k3 / b3;
            }
            else
            {
                // If a1 and a3 are non-zero:
                // Multiply EQ1 by a3 / a1, then subtract EQ3 from it.
                // This gives a new equation with coefficients 

                a31 = a3 / a1;
                temp = a31 * b1 - b3;
                // Subtract equations, x term cancels out. Left with:
                // (a31 * b1 - b3)y + (a31 * c1 - c3)z = a31 * k1 - k3

                e = -((a31 * c1 - c3) / temp);
                f = (a31 * k1 - k3) / temp;
            }

            // Find x as a linear expression of z.
            // x = gz + h		

            if (b1 == 0)
            {
                g = -c1 / a1;
                h = k1 / a1;
            }
            else if (b3 == 0)
            {
                g = -c3 / a3;
                h = k3 / a3;
            }
            else
            {
                // If b1 and b3 are non-zero:
                // Multiply EQ1 by b3 / b1, then subtract EQ3 from it.
                // This gives a new equation with coefficients 

                b31 = b3 / b1;

                // Subtract equations, y term cancels out. Left with:
                // (b31 * a1 - a3)x + (b31 * c1 - c3)z = b31 * k1 - k3
                temp = (b31 * a1 - a3);
                g = -((b31 * c1 - c3) / temp);
                h = (b31 * k1 - k3) / temp;
            }

            /*	
                3. Then substitute these into the equation of any of the original
                spheres (EQ1).  This will give you a quadratic equation in one variable,
                which you can solve to find the two roots.  
                EQ1: (x1 - x)^2 + (y1 - y)^2 + (z1 - z)^2 = r1^2
                EQ1: (x1 - gz - h)^2 + (y1 - ez - f)^2 + (z1 - z)^2 = r1^2

                x1^2 - x1gz - x1h - x1gz + g^2 * z^2 + ghz - x1h + ghz + h^2 +
                y1^2 - y1ez - y1f - y1ez + e^2 * z^2 + efz - y1f + efz + f^2 +
                z1^2 - 2z1z + z^2
                = r1^2
                Simplify and put in quadratic form of Az^2 + Bz + C = 0

                x1^2 + y1^2 + z1^2 - x1h - y1f - x1h - y1f + h^2 + f^2
                - x1gz - y1ez - 2z1z - x1gz - y1ez + ghz + efz + ghz + efz
                + g^2 * z^2 + e^2 * z^2 + z^2
                = r1^2

                x1^2 + y1^2 + z1^2 - x1h - y1f - x1h - y1f + h^2 + f^2 - r1^2
                + (- x1g- y1e - 2z1 - x1g - y1e + gh + ef + gh + ef) * z
                + (g^2 + e^2 + 1) * z^2
                = 0
            */


            A = g * g + e * e + 1.0;
            B = -x1 * g - y1 * e - 2.0 * z1 - x1 * g - y1 * e + 2.0 * g * h + 2.0 * e * f;
            C = x1 * x1 + y1 * y1 + z1 * z1 - 2.0 * x1 * h - 2.0 * y1 * f + h * h + f * f - r1 * r1;

            // Quadratic formula: z = (-B +- sqrt(B^2 - 4AC)) / 2A
            // Use the quadratic formula to solve to find the two roots.
            double D = B * B - 4.0 * A * C;
            if (D < 0)
            {
                D = 0;//we are very close to 0 in this particular problem
            }
            rootD = Math.Sqrt(D);


            temp = 2.0 * A;
            z = (-B + rootD) / temp;
            z_ = (-B - rootD) / temp;

            /*	
                4. These values will allow you to determine the corresponding values of
                the other two variables, giving you the coordinates of the two
                intersection points.	
            */

            x = g * z + h;
            x_ = g * z_ + h;

            y = e * z + f;
            y_ = e * z_ + f;

            Vector3 res;

            res.X = (float)((x + x_) / 2); res.Y = (float)((y + y_) / 2); res.Z = (float)((z + z_) / 2);

            return res;
        }
    }
}
