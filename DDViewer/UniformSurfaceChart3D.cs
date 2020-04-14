using System.Collections;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace DDViewer
{
    class UniformSurfaceChart3D : SurfaceChart3D
    {
        // the grid number on each axis
        private int gridXNo, gridYNo;

        public void SetPoint(int i, int j, float x, float y, float z)
        {
            int k = j * gridXNo + i;
            vertices[k].x = x;
            vertices[k].y = y;
            vertices[k].z = z;
        }

        public void SetZ(int i, int j, float z)
        {
            vertices[j * gridXNo + i].z = z;
        }

        public void SetColor(int i, int j, Color color)
        {
            int k = j * gridXNo + i;
            vertices[k].color = color;
        }

        public void SetGrid(int xNo, int yNo, float xMin, float xMax, float yMin, float yMax)
        {
            SetDataNo(xNo * yNo);
            gridXNo = xNo;
            gridYNo = yNo;
            this.xMin = xMin;
            this.xMax = xMax;
            this.yMin = yMin;
            this.yMax = yMax;
            float dx = (xMax - xMin) / ((float)xNo - 1);
            float dy = (yMax - yMin) / ((float)yNo - 1);
            for (int i = 0; i < xNo; i++)
            {
                for (int j = 0; j < yNo; j++)
                {
                    float xV = xMin + dx * ((float)(i));
                    float yV = yMin + dy * ((float)(j));
                    vertices[j * xNo + i] = new Vertex3D();
                    SetPoint(i, j, xV, yV, 0);
                }
            }
        }

        // convert the uniform surface chart to a array of Mesh3D (only one element)
        public ArrayList GetMeshes()
        {
            ArrayList meshs = new ArrayList();
            ColorMesh3D surfaceMesh = new ColorMesh3D();

            surfaceMesh.SetSize(gridXNo * gridYNo, 2 * (gridXNo - 1) * (gridYNo - 1));

            for (int i = 0; i < gridXNo; i++)
            {
                for (int j = 0; j < gridYNo; j++)
                {
                    int k = j * gridXNo + i;
                    Vertex3D vert = vertices[k];
                    vertices[k].minI = k;
                    surfaceMesh.SetPoint(k, new Point3D(vert.x, vert.y, vert.z));
                    surfaceMesh.SetColor(k, vert.color);
                }
            }
            // set triangle
            int nT = 0;
            for (int i = 0; i < gridXNo - 1; i++)
            {
                for (int j = 0; j < gridYNo - 1; j++)
                {
                    int n00 = j * gridXNo + i;
                    int n10 = j * gridXNo + i + 1;
                    int n01 = (j + 1) * gridXNo + i;
                    int n11 = (j + 1) * gridXNo + i + 1;

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
