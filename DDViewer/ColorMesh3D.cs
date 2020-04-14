using System;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace DDViewer
{
    public class ColorMesh3D : Mesh3D
    {
        private Color[] colors;             // color information of each vertex

        // override the set vertex number, since we include the color information for each vertex
        public override void SetVertexNo(int size)
        {
            points = new Point3D[size];
            colors = new Color[size];
        }

        // get color information of each vertex
        public override Color GetColor(int v)
        {
            return colors[v];
        }

        // set color information of each vertex
        public void SetColor(int v, Byte r, Byte g, Byte b)
        {
            colors[v] = Color.FromRgb(r, g, b);
        }

        public void SetColor(int v, Color color)
        {
            colors[v] = color;
        }
    }
}
