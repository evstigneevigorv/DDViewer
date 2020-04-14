using System;

namespace DDViewer
{
    public class Cylinder3D : Mesh3D
    {
        private int res;

        //  first 3 parameter is the cylinder size, last parameter is the cylinder smoothness
        public Cylinder3D(double a, double b, double h, int res)
        {
            SetMesh(res);
            SetData(a, b, h);
        }

        // set mesh structure, (triangle connection)
        void SetMesh(int res)
        {
            int vertNo = 2 * res + 2;
            int triNo = 4 * res;
            SetSize(vertNo, triNo);
            int n1, n2;
            for (int i = 0; i < res; i++)
            {
                n1 = i;
                if (i == (res - 1)) n2 = 0;
                else n2 = i + 1;
                SetTriangle(i * 4 + 0, n1, n2, res + n1);                 // side
                SetTriangle(i * 4 + 1, res + n1, n2, res + n2);           // side
                SetTriangle(i * 4 + 2, n2, n1, 2 * res);                  // bottom
                SetTriangle(i * 4 + 3, res + n1, res + n2, 2 * res + 1);  // top
            }

            this.res = res;
        }

        // set mesh vertex location
        void SetData(double a, double b, double h)
        {
            double xyStep = 2.0f * 3.1415926f / ((double)res);
            for (int i = 0; i < res; i++)
            {
                double xy = ((double)i) * xyStep;
                SetPoint(i, a * Math.Cos(xy), b * Math.Sin(xy), -h / 2);
            }

            for (int i = 0; i < res; i++)
            {
                double xy = ((double)i) * xyStep;
                SetPoint(res + i, a * Math.Cos(xy), b * Math.Sin(xy), h / 2);
            }

            SetPoint(2 * res, 0, 0, -h / 2);
            SetPoint(2 * res + 1, 0, 0, h / 2);

            xMin = -a;
            xMax = a;
            yMin = -b;
            yMax = b;
            zMin = -h / 2;
            zMax = h / 2;
        }
    }
}
