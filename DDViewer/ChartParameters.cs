using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DDViewer
{
    enum CoordinateSystem { Polar, Rect }
    enum CoordinateScale { Lin, Log }

    class ChartParameters
    {
        public CoordinateSystem coordinateSystem;
        public CoordinateScale coordinateScale;
        public double logMin;
        public GridParameters gridRect, gridPolar;

        public ChartParameters(CoordinateSystem cSys,
                               CoordinateScale cScl,
                               double lm)
        {
            this.coordinateSystem = cSys;
            this.coordinateScale = cScl;
            this.logMin = lm;

            this.gridRect = new GridParameters();
            this.gridRect.XY = true;
            this.gridRect.XZ = false;
            this.gridRect.YZ = false;

            this.gridPolar = new GridParameters();
            this.gridPolar.XY = false;
            this.gridPolar.XZ = true;
            this.gridPolar.YZ = true;
        }
    }

    class GridParameters
    {
        public bool XY;
        public bool XZ;
        public bool YZ;
    }
}
