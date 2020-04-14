using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace DDViewer
{
    public class Mesh3D
    {
        protected Point3D[] points;
        protected int[] vertIndices;
        protected Color color;
        protected Triangle3D[] tris;

        public double xMin, xMax, yMin, yMax, zMin, zMax;

        public int GetVertexNo()
        {
            if (points == null) return 0;
            return points.Length;
        }

        public virtual void SetVertexNo(int size)
        {
            points = new Point3D[size];
            vertIndices = new int[size];
        }

        public int GetTriangleNo()
        {
            if (tris == null) return 0;
            return tris.Length;
        }

        public void SetTriangleNo(int size)
        {
            tris = new Triangle3D[size];
        }

        public virtual void SetSize(int vertexNo, int triangleNo)
        {
            SetVertexNo(vertexNo);
            SetTriangleNo(triangleNo);
        }

        public Point3D GetPoint(int n)
        {
            return points[n];
        }

        public void SetPoint(int n, Point3D pt)
        {
            points[n] = pt;
        }

        public void SetPoint(int n, double x, double y, double z)
        {
            points[n] = new Point3D(x, y, z);
        }

        public Triangle3D GetTriangle(int n)
        {
            return tris[n];
        }

        public void SetTriangle(int n, Triangle3D triangle)
        {
            tris[n] = triangle;
        }

        public void SetTriangle(int i, int m0, int m1, int m2)
        {
            tris[i] = new Triangle3D(m0, m1, m2);
        }

        public Vector3D GetTriangleNormal(int n)
        {
            Triangle3D tri = GetTriangle(n);
            Point3D pt0 = GetPoint(tri.n0);
            Point3D pt1 = GetPoint(tri.n1);
            Point3D pt2 = GetPoint(tri.n2);

            double dx1 = pt1.X - pt0.X;
            double dy1 = pt1.Y - pt0.Y;
            double dz1 = pt1.Z - pt0.Z;

            double dx2 = pt2.X - pt0.X;
            double dy2 = pt2.Y - pt0.Y;
            double dz2 = pt2.Z - pt0.Z;

            double vx = dy1 * dz2 - dz1 * dy2;
            double vy = dz1 * dx2 - dx1 * dz2;
            double vz = dx1 * dy2 - dy1 * dx2;

            double length = Math.Sqrt(vx * vx + vy * vy + vz * vz);

            return new Vector3D(vx / length, vy / length, vz / length);
        }

        // get the color of a vertex (all the same for this class, but different in child class)
        public virtual Color GetColor(int nV)
        {
            return color;
        }

        // set the color of this mesh
        public void SetColor(Byte r, Byte g, Byte b)
        {
            color = Color.FromRgb(r, g, b);
        }

        public void SetColor(Color clr)
        {
            color = clr;
        }

        // after we change the location of the mesh, use this function to update the display
        public void UpdatePositions(MeshGeometry3D meshGeometry)
        {
            int vertNo = GetVertexNo();
            for (int i = 0; i < vertNo; i++)
            {
                meshGeometry.Positions[i] = points[i];
            }
        }

        // Set the test model
        public virtual void SetTestModel()
        {
            double size = 10;
            SetSize(3, 1);
            SetPoint(0, -0.5, 0, 0);
            SetPoint(1, 0.5, 0.5, 0.3);
            SetPoint(2, 0, 0.5, 0);
            SetTriangle(0, 0, 2, 1);
            xMin = 0; xMax = 2 * size;
            yMin = 0; yMax = size;
            zMin = -size; zMax = size;
        }
    }
}
