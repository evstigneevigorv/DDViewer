using System;
using System.Collections;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Controls;

namespace DDViewer
{
    class Chart3D
    {
        public Vertex3D[] vertices; // 3d plot data
        public string caption = "";
        protected double xMin, xMax, yMin, yMax, zMin, zMax; // data range
        public static int SHAPE_NO = 5;
        public enum SHAPE { BAR, ELLIPSE, CYLINDER, CONE, PYRAMID };    // shape of the 3d dot in the plot

        private double axisLengthWidthRatio = 200.0;                  // axis length / width ratio
        private double xAxisLength, yAxisLength, zAxisLength;       // axis length
        private double xAxisCenter, yAxisCenter, zAxisCenter;       // axis start point
        private bool useAxes = false;                              // use axis
        public Color axisColor = Color.FromRgb(0, 0, 196);         // axis color

        public Chart3D() { }

        public Vertex3D this[int n]
        {
            get
            {
                return (Vertex3D)vertices[n];
            }
            set
            {
                vertices[n] = value;
            }
        }

        public double XCenter()
        {
            return (xMin + xMax) / 2;
        }

        public double YCenter()
        {
            return (yMin + yMax) / 2;
        }

        public double ZCenter()
        {
            return (zMin + zMax) / 2;
        }

        public double XRange()
        {
            return xMax - xMin;
        }
        public double YRange()
        {
            return yMax - yMin;
        }
        public double ZRange()
        {
            return zMax - zMin;
        }
        public double XMin()
        {
            return xMin;
        }

        public double XMax()
        {
            return xMax;
        }
        public double YMin()
        {
            return yMin;
        }

        public double YMax()
        {
            return yMax;
        }
        public double ZMin()
        {
            return zMin;
        }

        public double ZMax()
        {
            return zMax;
        }

        public int GetDataNo()
        {
            return vertices.Length;
        }

        public void SetDataNo(int nSize)
        {
            vertices = new Vertex3D[nSize];
        }

        public void GetDataRange()
        {
            int dataNo = GetDataNo();
            if (dataNo == 0) return;
            xMin = Single.MaxValue;
            yMin = Single.MaxValue;
            zMin = Single.MaxValue;
            xMax = Single.MinValue;
            yMax = Single.MinValue;
            zMax = Single.MinValue;
            for (int i = 0; i < dataNo; i++)
            {
                double xV = this[i].x;
                double yV = this[i].y;
                double zV = this[i].z;
                if (xMin > xV) xMin = xV;
                if (yMin > yV) yMin = yV;
                if (zMin > zV) zMin = zV;
                if (xMax < xV) xMax = xV;
                if (yMax < yV) yMax = yV;
                if (zMax < zV) zMax = zV;
            }
        }

        public void SetAxes(double x0, double y0, double z0, double xL, double yL, double zL)
        {
            xAxisLength = xL;
            yAxisLength = yL;
            zAxisLength = zL;
            xAxisCenter = x0;
            yAxisCenter = y0;
            zAxisCenter = z0;
            useAxes = true;
        }

        public void SetAxes()
        {
            SetAxes(0.05f);
        }

        public void SetAxes(double margin)
        {
            double xRange = xMax - xMin;
            double yRange = yMax - yMin;
            double zRange = zMax - zMin;

            double xC = xMin - margin * xRange;
            double yC = yMin - margin * yRange;
            double zC = zMin - margin * zRange;
            double xL = (1 + 2 * margin) * xRange;
            double yL = (1 + 2 * margin) * yRange;
            double zL = (1 + 2 * margin) * zRange;

            SetAxes(xC, yC, zC, xL, yL, zL);
        }

        // add the axes mesh to the Mesh3D array
        // if you are using the projection matrix which is not uniform along all the axess, you need change this function
        public void AddAxesMeshes(ArrayList meshs)
        {
            if (!useAxes) return;

            double radius = (xAxisLength + yAxisLength + zAxisLength) / (3 * axisLengthWidthRatio);

            Mesh3D xAxisCylinder = new Cylinder3D(radius, radius, xAxisLength, 6);
            xAxisCylinder.SetColor(axisColor);
            TransformMatrix.Transform(xAxisCylinder, new Point3D(xAxisCenter + xAxisLength / 2, yAxisCenter, zAxisCenter), 0, 90);
            meshs.Add(xAxisCylinder);

            Mesh3D xAxisCone = new Cone3D(2 * radius, 2 * radius, radius * 5, 6);
            xAxisCone.SetColor(axisColor);
            TransformMatrix.Transform(xAxisCone, new Point3D(xAxisCenter + xAxisLength, yAxisCenter, zAxisCenter), 0, 90);
            meshs.Add(xAxisCone);

            Mesh3D yAxisCylinder = new Cylinder3D(radius, radius, yAxisLength, 6);
            yAxisCylinder.SetColor(axisColor);
            TransformMatrix.Transform(yAxisCylinder, new Point3D(xAxisCenter, yAxisCenter + yAxisLength / 2, zAxisCenter), 90, 90);
            meshs.Add(yAxisCylinder);

            Mesh3D yAxisCone = new Cone3D(2 * radius, 2 * radius, radius * 5, 6);
            yAxisCone.SetColor(axisColor);
            TransformMatrix.Transform(yAxisCone, new Point3D(xAxisCenter, yAxisCenter + yAxisLength, zAxisCenter), 90, 90);
            meshs.Add(yAxisCone);

            Mesh3D zAxisCylinder = new Cylinder3D(radius, radius, zAxisLength, 6);
            zAxisCylinder.SetColor(axisColor);
            TransformMatrix.Transform(zAxisCylinder, new Point3D(xAxisCenter, yAxisCenter, zAxisCenter + zAxisLength / 2), 0, 0);
            meshs.Add(zAxisCylinder);

            Mesh3D zAxisCone = new Cone3D(2 * radius, 2 * radius, radius * 5, 6);
            zAxisCone.SetColor(axisColor);
            TransformMatrix.Transform(zAxisCone, new Point3D(xAxisCenter, yAxisCenter, zAxisCenter + zAxisLength), 0, 0);
            meshs.Add(zAxisCone);

            // Подписи координат
            double l = 7.0 * radius;
            double ofs = 10.0 * radius;

            Mesh3D x1CrdCylinder = new Cylinder3D(radius, radius, 2 * l, 6);
            Mesh3D x2CrdCylinder = new Cylinder3D(radius, radius, 2 * l, 6);
            x1CrdCylinder.SetColor(axisColor);
            x2CrdCylinder.SetColor(axisColor);
            TransformMatrix.Transform(x1CrdCylinder, new Point3D(xAxisCenter + xAxisLength + ofs, yAxisCenter, zAxisCenter), 90.0, 30.0);
            TransformMatrix.Transform(x2CrdCylinder, new Point3D(xAxisCenter + xAxisLength + ofs, yAxisCenter, zAxisCenter), 90.0, -30.0);
            meshs.Add(x1CrdCylinder);
            meshs.Add(x2CrdCylinder);

            Mesh3D y1CrdCylinder = new Cylinder3D(radius, radius, l, 6);
            Mesh3D y2CrdCylinder = new Cylinder3D(radius, radius, l, 6);
            Mesh3D y3CrdCylinder = new Cylinder3D(radius, radius, l, 6);
            y1CrdCylinder.SetColor(axisColor);
            y2CrdCylinder.SetColor(axisColor);
            y3CrdCylinder.SetColor(axisColor);
            TransformMatrix.Transform(y1CrdCylinder, new Point3D(xAxisCenter - l * Math.Sin(30.0) / 4, yAxisCenter + yAxisLength + ofs, zAxisCenter + l * Math.Cos(30.0)), 0.0, 30.0);
            TransformMatrix.Transform(y2CrdCylinder, new Point3D(xAxisCenter + l * Math.Sin(30.0) / 4, yAxisCenter + yAxisLength + ofs, zAxisCenter + l * Math.Cos(30.0)), 0.0, 330.0);
            TransformMatrix.Transform(y3CrdCylinder, new Point3D(xAxisCenter, yAxisCenter + yAxisLength + ofs, zAxisCenter - l / 2), 0.0, 0.0);
            meshs.Add(y1CrdCylinder);
            meshs.Add(y2CrdCylinder);
            meshs.Add(y3CrdCylinder);

            Mesh3D z1CrdCylinder = new Cylinder3D(radius, radius, l, 6);
            Mesh3D z2CrdCylinder = new Cylinder3D(radius, radius, 2 * l, 6);
            Mesh3D z3CrdCylinder = new Cylinder3D(radius, radius, l, 6);
            z1CrdCylinder.SetColor(axisColor);
            z2CrdCylinder.SetColor(axisColor);
            z3CrdCylinder.SetColor(axisColor);
            TransformMatrix.Transform(z1CrdCylinder, new Point3D(xAxisCenter, yAxisCenter + l * Math.Cos(60.0) + radius, zAxisCenter + zAxisLength + ofs), 0.0, 90.0);
            TransformMatrix.Transform(z2CrdCylinder, new Point3D(xAxisCenter, yAxisCenter, zAxisCenter + zAxisLength + ofs), 60.0, 90.0);
            TransformMatrix.Transform(z3CrdCylinder, new Point3D(xAxisCenter, yAxisCenter - l * Math.Cos(60.0) - radius, zAxisCenter + zAxisLength + ofs), 0.0, 90.0);
            meshs.Add(z1CrdCylinder);
            meshs.Add(z2CrdCylinder);
            meshs.Add(z3CrdCylinder);
        }

        // select 
        //public virtual void Select(ViewportRect rect, TransformMatrix matrix, Viewport3D viewport3d) { }

        // highlight selected model
        //public virtual void HighlightSelection(System.Windows.Media.Media3D.MeshGeometry3D meshGeometry, System.Windows.Media.Color selectColor) { }

    }
}
