using System;

namespace DDViewer
{
    public class Cone3D : Mesh3D
    {
        private int res; // resolution of the cone

        // first 3 parameter are cone size, last parameter is cone resolution (smoothness)
        public Cone3D(double a, double b, double h, int res)
        {
            SetMesh(res);
            SetData(a, b, h);
        }

        // set mesh structure, (triangle connection)
        void SetMesh(int res)
        {
            int vertNo = res + 2;
            int triNo = 2 * res;
            SetSize(vertNo, triNo);
            for (int i = 0; i < res - 1; i++)
            {
                SetTriangle(i, i, i + 1, res + 1);
                SetTriangle(res + i, i + 1, i, res);
            }
            SetTriangle(res - 1, res - 1, 0, res + 1);
            SetTriangle(2 * res - 1, 0, res - 1, res);

            this.res = res;
        }

        // set cone vertices  
        // a: cone bottom ellipse long axis
        // b: cone bottom ellipse short axis
        // h: cone height
        void SetData(double a, double b, double h)
        {
            double xyStep = 2.0f * 3.1415926f / ((double)res);
            for (int i = 0; i < res; i++)
            {
                double xy = ((double)i) * xyStep;
                SetPoint(i, a * Math.Cos(xy), b * Math.Sin(xy), 0);
            }
            SetPoint(res, 0, 0, 0);
            SetPoint(res + 1, 0, 0, h);

            xMin = -a;
            xMax = a;
            yMin = -b;
            yMax = b;
            zMin = 0;
            zMax = h;
        }
    }
}
