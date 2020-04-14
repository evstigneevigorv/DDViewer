using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DDDrawerConsole
{
    public class Transmitter
    {
        // Координаты излучателя
        public double X;
        public double Y;
        public double Z;

        public double A;        // Амплитуда излучения
        public double Phase;    // Начальная фаза излучения
        public bool IsEnabled;  // Признак включения

        public Transmitter(double x, double y, double z, double a, double phase, bool en)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;

            this.A = a;
            this.Phase = phase;
            this.IsEnabled = en;
        }
    }
}
