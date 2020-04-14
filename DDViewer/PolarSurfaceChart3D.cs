using System;
using System.Collections;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace DDViewer
{
    class PolarSurfaceChart3D : SurfaceChart3D
    {
        private int gridANo, gridBNo;

        public void SetDataNo(int alphaNo, int betaNo)
        {
            gridANo = alphaNo;
            gridBNo = betaNo;
            SetDataNo(alphaNo * betaNo);
        }

        public void SetPoint(int i, int j, float x, float y, float z)
        {
            int k = j * gridANo + i;
            vertices[k].x = x;
            vertices[k].y = y;
            vertices[k].z = z;
        }

        public ArrayList GetMeshes()
        {
            ArrayList meshs = new ArrayList();
            ColorMesh3D surfaceMesh = new ColorMesh3D();

            surfaceMesh.SetSize(gridANo * gridBNo, 2 * (gridANo - 1) * (gridBNo - 1));

            for (int i = 0; i < gridANo; i++)
            {
                for (int j = 0; j < gridBNo; j++)
                {
                    int k = j * gridANo + i;
                    Vertex3D vert = vertices[k];
                    vertices[k].minI = k;
                    surfaceMesh.SetPoint(k, new Point3D(vert.x, vert.y, vert.z));
                    surfaceMesh.SetColor(k, vert.color);
                }
            }
            // set triangle
            int nT = 0;
            for (int i = 0; i < gridANo - 1; i++)
            {
                for (int j = 0; j < gridBNo - 1; j++)
                {
                    int n00 = j * gridANo + i;
                    int n10 = j * gridANo + i + 1;
                    int n01 = (j + 1) * gridANo + i;
                    int n11 = (j + 1) * gridANo + i + 1;

                    surfaceMesh.SetTriangle(nT, n00, n10, n01);
                    nT++;
                    surfaceMesh.SetTriangle(nT, n01, n10, n11);
                    nT++;
                }
            }
            meshs.Add(surfaceMesh);
            AddAxesMeshes(meshs);

            return meshs;
        }
    }
}
