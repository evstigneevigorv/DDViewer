using System;
using System.Collections;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Controls;

namespace DDViewer
{
    public class Model3D : ModelVisual3D
    {
        private TextureMapping mapping = new TextureMapping();

        public Model3D() { }

        public void SetRGBColor()
        {
            mapping.SetRGBMapping();
        }

        public void SetPseudoColor()
        {
            mapping.SetPseudoMapping();
        }

        private void SetModel(ArrayList meshs, Material backMaterial)
        {
            int meshNo = meshs.Count;
            if (meshNo == 0) return;

            MeshGeometry3D triangleMesh = new MeshGeometry3D();
            int totalVertNo = 0;
            for (int j = 0; j < meshNo; j++)
            {
                Mesh3D mesh = (Mesh3D)meshs[j];
                int vertNo = mesh.GetVertexNo();
                int triNo = mesh.GetTriangleNo();
                if ((vertNo <= 0) || (triNo <= 0)) continue;

                double[] vx = new double[vertNo];
                double[] vy = new double[vertNo];
                double[] vz = new double[vertNo];
                for (int i = 0; i < vertNo; i++)
                {
                    vx[i] = vy[i] = vz[i] = 0;
                }

                // get normal of each vertex
                for (int i = 0; i < triNo; i++)
                {
                    Triangle3D tri = mesh.GetTriangle(i);
                    Vector3D vN = mesh.GetTriangleNormal(i);
                    int n0 = tri.n0;
                    int n1 = tri.n1;
                    int n2 = tri.n2;

                    vx[n0] += vN.X;
                    vy[n0] += vN.Y;
                    vz[n0] += vN.Z;
                    vx[n1] += vN.X;
                    vy[n1] += vN.Y;
                    vz[n1] += vN.Z;
                    vx[n2] += vN.X;
                    vy[n2] += vN.Y;
                    vz[n2] += vN.Z;
                }
                for (int i = 0; i < vertNo; i++)
                {
                    double length = Math.Sqrt(vx[i] * vx[i] + vy[i] * vy[i] + vz[i] * vz[i]);
                    if (length > 1e-20)
                    {
                        vx[i] /= length;
                        vy[i] /= length;
                        vz[i] /= length;
                    }
                    triangleMesh.Positions.Add(mesh.GetPoint(i));
                    Color color = mesh.GetColor(i);
                    Point mapPt = mapping.GetMappingPosition(color);
                    triangleMesh.TextureCoordinates.Add(new Point(mapPt.X, mapPt.Y));
                    triangleMesh.Normals.Add(new Vector3D(vx[i], vy[i], vz[i]));
                }

                for (int i = 0; i < triNo; i++)
                {
                    Triangle3D tri = mesh.GetTriangle(i);
                    int n0 = tri.n0;
                    int n1 = tri.n1;
                    int n2 = tri.n2;

                    triangleMesh.TriangleIndices.Add(totalVertNo + n0);
                    triangleMesh.TriangleIndices.Add(totalVertNo + n1);
                    triangleMesh.TriangleIndices.Add(totalVertNo + n2);
                }
                totalVertNo += vertNo;
            }
            //Material material = new DiffuseMaterial(new SolidColorBrush(Colors.Red));
            Material material = mapping.material;

            GeometryModel3D triangleModel = new GeometryModel3D(triangleMesh, material);
            triangleModel.Transform = new Transform3DGroup();
            if (backMaterial != null) triangleModel.BackMaterial = backMaterial;

            Content = triangleModel;
        }

        // get MeshGeometry3D object from Viewport3D
        public static MeshGeometry3D GetGeometry(Viewport3D viewport3d, int modelIndex)
        {
            if (modelIndex == -1) return null;
            ModelVisual3D visual3d = (ModelVisual3D)(viewport3d.Children[modelIndex]);
            if (visual3d.Content == null) return null;
            GeometryModel3D triangleModel = (GeometryModel3D)(visual3d.Content);
            return (MeshGeometry3D)triangleModel.Geometry;
        }

        // update the ModelVisual3D object in "viewport3d" using Mesh3D array "meshs"
        public int UpdateModel(ArrayList meshs, Material backMaterial, int modelIndex, Viewport3D viewport3d)
        {
            if (modelIndex >= 0)
            {
                ModelVisual3D m = (ModelVisual3D)viewport3d.Children[modelIndex];
                viewport3d.Children.Remove(m);
            }

            if (backMaterial == null)
                SetRGBColor();
            else
                SetPseudoColor();

            SetModel(meshs, backMaterial);

            int nModelNo = viewport3d.Children.Count;
            viewport3d.Children.Add(this);

            return nModelNo;
        }
    }
}
