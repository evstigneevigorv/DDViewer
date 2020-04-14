using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using System.Windows.Controls;

namespace DDViewer
{
    public class TransformMatrix
    {
        private Matrix3D viewMatrix = new Matrix3D();
        private Matrix3D projMatrix = new Matrix3D();
        public Matrix3D totalMatrix = new Matrix3D();

        public double scaleFactor = 1.2; // sensativity for zoom
        private bool mouseDown = false;  // mouse down
        private Point movePoint;         // previous mouse location

        public void ResetView()
        {
            viewMatrix.SetIdentity();
        }

        public void OnLBtnDown(Point pt)
        {
            mouseDown = true;
            movePoint = pt;
        }

        public void OnMouseMove(Point pt, Viewport3D viewPort, bool middleButtonPressed)
        {
            if (!mouseDown) return;

            double width = viewPort.ActualWidth;
            double height = viewPort.ActualHeight;

            //OrthographicCamera camera = viewPort.Camera as System.Windows.Media.Media3D.OrthographicCamera;
            //Matrix3D cameraMatrix = camera.Transform.Value;

            if (Keyboard.IsKeyDown(Key.LeftCtrl) ||
                Keyboard.IsKeyDown(Key.RightCtrl) ||
                middleButtonPressed)
            {
                double shiftX = - Math.Sqrt(2) * (pt.X - movePoint.X) / (width);
                double shiftY = - shiftX;
                double shiftZ = - Math.Sqrt(2) * (pt.Y - movePoint.Y) / (height);
                viewMatrix.Translate(new Vector3D(shiftX, shiftY, shiftZ));
            }
            else
            {
                double aZ = 180.0 * (pt.X - movePoint.X) / width;
                double aXY = 180.0 * (pt.Y - movePoint.Y) / height;

                if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                {
                    if (Math.Abs(pt.Y - movePoint.Y) > Math.Abs(pt.X - movePoint.X))
                        viewMatrix.Rotate(new Quaternion(new Vector3D(-1.0, 1.0, 0.0), aXY));
                    else
                        viewMatrix.Rotate(new Quaternion(new Vector3D(0.0, 0.0, 1.0), aZ));
                }
                else
                {
                    viewMatrix.Rotate(new Quaternion(new Vector3D(0.0, 0.0, 1.0), aZ));
                    viewMatrix.Rotate(new Quaternion(new Vector3D(-1.0, 1.0, 0.0), aXY));
                }
            }
            movePoint = pt;
            totalMatrix = Matrix3D.Multiply(projMatrix, viewMatrix);
        }

        public void OnLBtnUp()
        {
            mouseDown = false;
        }

        public void OnKeyDown(System.Windows.Input.KeyEventArgs args)
        {
            switch (args.Key)
            {
                case Key.Home:
                    viewMatrix.SetIdentity();
                    break;
                case Key.OemPlus:
                    ViewScale(+1.0);
                    break;
                case Key.OemMinus:
                    ViewScale(-1.0);
                    break;
                default:
                    return;
            }
            totalMatrix = Matrix3D.Multiply(projMatrix, viewMatrix);
        }

        public void ViewScale(double delta)
        {
            double scale = Math.Pow(scaleFactor, delta);
            viewMatrix.Scale(new Vector3D(scale, scale, scale));
            totalMatrix = Matrix3D.Multiply(projMatrix, viewMatrix);
        }

        // transform input point pt1, (rotate "aX, aZ" and move to "center")
        public static Point3D Transform(Point3D pt1, Point3D center, double aX, double aZ)
        {
            double angleX = 3.1415926f * aX / 180;
            double angleZ = 3.1415926f * aZ / 180;

            // rotate from z-axis
            double x2 = pt1.X * Math.Cos(angleZ) + pt1.Z * Math.Sin(angleZ);
            double y2 = pt1.Y;
            double z2 = -pt1.X * Math.Sin(angleZ) + pt1.Z * Math.Cos(angleZ);

            double x3 = center.X + x2 * Math.Cos(angleX) - y2 * Math.Sin(angleX);
            double y3 = center.Y + x2 * Math.Sin(angleX) + y2 * Math.Cos(angleX);
            double z3 = center.Z + z2;

            return new Point3D(x3, y3, z3);
        }

        // transform input point pt1, (rotate "aX, aZ" and move to "center")
        public static void Transform(Mesh3D model, Point3D center, double aX, double aZ)
        {
            double angleX = 3.1415926f * aX / 180;
            double angleZ = 3.1415926f * aZ / 180;

            int vertNo = model.GetVertexNo();
            for (int i = 0; i < vertNo; i++)
            {
                Point3D pt1 = model.GetPoint(i);
                // rotate from z-axis
                double x2 = pt1.X * Math.Cos(angleZ) + pt1.Z * Math.Sin(angleZ);
                double y2 = pt1.Y;
                double z2 = -pt1.X * Math.Sin(angleZ) + pt1.Z * Math.Cos(angleZ);

                double x3 = center.X + x2 * Math.Cos(angleX) - y2 * Math.Sin(angleX);
                double y3 = center.Y + x2 * Math.Sin(angleX) + y2 * Math.Cos(angleX);
                double z3 = center.Z + z2;

                model.SetPoint(i, x3, y3, z3);
            }
        }

        // set the projection matrix
        public void CalculateProjectionMatrix(Mesh3D mesh, double scaleFactor)
        {
            CalculateProjectionMatrix(mesh.xMin, mesh.xMax, mesh.yMin, mesh.yMax, mesh.zMin, mesh.zMax, scaleFactor);
        }

        public void CalculateProjectionMatrix(double min, double max, double scaleFactor)
        {
            CalculateProjectionMatrix(min, max, min, max, min, max, scaleFactor);
        }

        public void CalculateProjectionMatrix(double xMin, double xMax, double yMin, double yMax, double zMin, double zMax, double scaleFactor)
        {
            double xC = (xMin + xMax) / 2;
            double yC = (yMin + yMax) / 2;
            double zC = (zMin + zMax) / 2;

            double xRange = (xMax - xMin) / 2;
            double yRange = (yMax - yMin) / 2;
            double zRange = (zMax - zMin) / 2;

            projMatrix.SetIdentity();
            projMatrix.Translate(new Vector3D(-xC, -yC, -zC));

            if (xRange < 1e-10) return;

            double sX = scaleFactor / xRange;
            double sY = scaleFactor / yRange;
            double sZ = scaleFactor / zRange;
            projMatrix.Scale(new Vector3D(sX, sY, sZ));

            totalMatrix = Matrix3D.Multiply(projMatrix, viewMatrix);
        }

        // get the screen position from original vertex
        public Point VertexToScreenPt(Point3D point, Viewport3D viewPort)
        {
            Point3D pt2 = totalMatrix.Transform(point);

            double width = viewPort.ActualWidth;
            double height = viewPort.ActualHeight;

            double x3 = width / 2 + (pt2.X) * width / 2;
            double y3 = height / 2 - (pt2.Y) * width / 2;

            return new Point(x3, y3);
        }

        public static Point ScreenPtToViewportPt(Point point, Viewport3D viewPort)
        {
            double width = viewPort.ActualWidth;
            double height = viewPort.ActualHeight;

            double x3 = (double)point.X;
            double y3 = (double)point.Y;
            double x2 = (x3 - width / 2) * 2 / width;
            double y2 = (height / 2 - y3) * 2 / width;

            return new Point(x2, y2);
        }

        public Point VertexToViewportPt(Point3D point, Viewport3D viewPort)
        {
            Point3D pt2 = totalMatrix.Transform(point);
            return new Point(pt2.X, pt2.Y);
        }

        public void ViewXYZ()
        {
            viewMatrix = new Matrix3D(  1.0,    0.0,    0.0,    0.0,
                                        0.0,    1.0,    0.0,    0.0,
                                        0.0,    0.0,    1.0,    0.0,
                                        0.0,    0.0,    0.0,    1.0 );
            totalMatrix = Matrix3D.Multiply(projMatrix, viewMatrix);
        }

        public void ViewZYX()
        {
            viewMatrix = new Matrix3D( -1.0,    0.0,    0.0,    0.0,
                                        0.0,   -1.0,    0.0,    0.0,
                                        0.0,    0.0,    1.0,    0.0,
                                        0.0,    0.0,    0.0,    1.0 );
            totalMatrix = Matrix3D.Multiply(projMatrix, viewMatrix);
        }

        public void ViewXY()
        {
            viewMatrix = new Matrix3D(-0.70711,  0.70711,  0.0,      0.0,
                                      -0.40825, -0.40825,  0.81650,  0.0,
                                       0.57735,  0.57735,  0.57735,  0.0,
                                       0.0,      0.0,      0.0,      1.0 );
            totalMatrix = Matrix3D.Multiply(projMatrix, viewMatrix);
        }

        public void ViewXZ()
        {
            viewMatrix = new Matrix3D( 0.70711, -0.70711,  0.0,      0.0,
                                       0.57735,  0.57735,  0.57735,  0.0,
                                      -0.40825, -0.40825,  0.81650,  0.0,
                                       0.0,      0.0,      0.0,      1.0 );
            totalMatrix = Matrix3D.Multiply(projMatrix, viewMatrix);
        }

        public void ViewYZ()
        {
            viewMatrix = new Matrix3D( 0.57735,  0.57735,  0.57735,  0.0,
                                      -0.70711,  0.70711,  0.0,      0.0,
                                      -0.40825, -0.40825,  0.81650,  0.0,
                                       0.0,      0.0,      0.0,      1.0 );
            totalMatrix = Matrix3D.Multiply(projMatrix, viewMatrix);
        }
    }
}
