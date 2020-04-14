using System.Windows.Media;

namespace DDViewer
{
    class Vertex3D
    {
        public Color color;           // color of the dot
        public double x, y, z;         // location of the dot
        public int minI, maxI;        // link to the viewport positions array index
        public bool selected = false; // is this dot selected by user
    }
}
