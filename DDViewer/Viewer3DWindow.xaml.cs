using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace DDViewer
{
    /// <summary>
    /// Interaction logic for Viewer3DWindow.xaml
    /// </summary>
    public partial class Viewer3DWindow : DockingLibrary.DockableContent
    {
        private const double xC = 0.0; // chart3D.XCenter();
        private const double yC = 0.0; // chart3D.YCenter();
        private const double zC = 0.0; // chart3D.ZCenter();

        private const double xMin = -1.0; // chart3D.XMin();
        private const double xMax = 1.0;  // chart3D.XMax();
        private const double yMin = -1.0; // chart3D.YMin();
        private const double yMax = 1.0;  // chart3D.YMax();
        private const double zMin = -1.0; // chart3D.ZMin();
        private const double zMax = 1.0;  // chart3D.ZMax();

        private const double WheelSensitivity = 120.0;

        public ArrayList chart3DList = new ArrayList();
        public int chartModelIndex = -1;         // model index in the Viewport3d
        public int chart3DInd = -1;
        // transform class object for rotate the 3d model
        public TransformMatrix transformMatrix = new TransformMatrix();
        private ChartParameters chartPrm = new ChartParameters(CoordinateSystem.Polar,
                                                              CoordinateScale.Log,
                                                              - 60.0);

        public Viewer3DWindow()
        {
            InitializeComponent();
        }

        public void CreateChart3D(ArrayList ary, CSType csType, string capt)
        {
            Chart3D chart3D = new Chart3D();
            chart3D.caption = capt;
            int vertNum = ary.Count;
            int trigNum = (int)(vertNum / 3);
            chart3D.SetDataNo(vertNum);
            double phi, theta;
            double alpha, beta, r;
            for (int i = 0; i < vertNum; i++)
            {
                switch (csType)
                {
                    case CSType.DEC: { } break;
                    case CSType.CALC:
                        {
                            phi = ((Point3D)ary[i]).X;
                            theta = ((Point3D)ary[i]).Y;
                            r = ((Point3D)ary[i]).Z;
                            chart3D.vertices[i] = new Vertex3D();
                            chart3D.vertices[i].x = r * Math.Cos(phi) * Math.Sin(theta);
                            chart3D.vertices[i].y = r * Math.Sin(phi) * Math.Sin(theta);
                            chart3D.vertices[i].z = r * Math.Cos(theta);
                        } break;
                    case CSType.TOPO:
                        {
                            alpha = ((Point3D)ary[i]).X;
                            beta = ((Point3D)ary[i]).Y;
                            r = ((Point3D)ary[i]).Z;
                            chart3D.vertices[i] = new Vertex3D();
                            chart3D.vertices[i].x = r * Math.Sin(alpha) * Math.Cos(beta);
                            chart3D.vertices[i].y = r * Math.Sin(beta);
                            chart3D.vertices[i].z = r * Math.Cos(alpha) * Math.Cos(beta);
                        } break;
                }
            }
            chart3D.GetDataRange();

            for (int i = 0; i < vertNum; i++)
            {
                Vertex3D vert = chart3D[i];
                r = Math.Pow((vert.x - xC) / (xMax - xMin), 2);
                r += Math.Pow((vert.y - yC) / (yMax - yMin), 2);
                r += Math.Pow((vert.z - zC) / (zMax - zMin), 2);
                r = 1.8 * Math.Sqrt(r);

                Color color = TextureMapping.PseudoColor(r);
                chart3D[i].color = color;
            }
            chart3D.SetAxes(0.0, 0.0, 0.0, 1.2f, 1.2f, 1.2f);
            chart3DList.Add(chart3D);
        }

        public void ShowChart3D(int ind)
        {
            if (chart3DList.Count == 0) return;
            // 4. Get the Mesh3D array from surface chart
            chart3DInd = ind;
            Chart3D chart3D = ((Chart3D)chart3DList[ind]);
            this.Title = chart3D.caption;
            int vertNum = chart3D.GetDataNo();
            int trigNum = (int)(vertNum / 3);
            ArrayList meshs = new ArrayList();
            ColorMesh3D surfaceMesh = new ColorMesh3D();
            surfaceMesh.SetSize(vertNum, (int)(vertNum / 3));

            double x, y, z, r;
            double rMinLog = 0.0;
            double rMaxLog = 1.0;
            double rMin = Math.Pow(10.0, chartPrm.logMin / 10.0);
            double rMax = 1.0;
            double logR;
            for (int i = 0; i < vertNum; i++)
            {
                x = chart3D.vertices[i].x;
                y = chart3D.vertices[i].y;
                z = chart3D.vertices[i].z;
                if (chartPrm.coordinateScale == CoordinateScale.Log)
                {
                    r = Math.Sqrt(x * x + y * y + z * z);
                    if (r > rMin)
                    {
                        logR = rMinLog + (rMaxLog - rMinLog) * (Math.Log10(r / rMin) / Math.Log10(rMax / rMin));
                        x = x * logR / r;
                        y = y * logR / r;
                        z = z * logR / r;
                    }
                    else
                    {
                        z = y = x = 0.0;
                    }
                }
                surfaceMesh.SetPoint(i, new Point3D(x, y, z));
                surfaceMesh.SetColor(i, chart3D.vertices[i].color);
            }
            for (int i = 0; i < trigNum; i++)
            {
                surfaceMesh.SetTriangle(i, 3*i, 3*i + 1, 3*i + 2);
            }
            meshs.Add(surfaceMesh);
            chart3D.AddAxesMeshes(meshs);

            // 5. display vertex no and triangle no of this surface chart
            UpdateModelSizeInfo(meshs);

            // 6. Set the model display of surface chart
            Model3D model3d = new Model3D();
            Material backMaterial = new DiffuseMaterial(new SolidColorBrush(Colors.Gray));
            chartModelIndex = model3d.UpdateModel(meshs, backMaterial, chartModelIndex, this.mainViewport);

            // 7. set projection matrix, so the data is in the display region
            //float xMin = chart3D.XMin();
            //float xMax = chart3D.XMax();
            transformMatrix.CalculateProjectionMatrix(xMin, xMax, xMin, xMax, zMin, zMax, 0.5);
            TransformChart();
        }

        // Получение диаграммы в полярной системе координат
        //public void TestPolarSurfacePlot(int alphaNo, int betaNo)
        //{
        //    // 1. Получение сетки
        //    chart3D = new PolarSurfaceChart3D();
        //    ((PolarSurfaceChart3D)chart3D).SetDataNo(alphaNo, betaNo);
        //    double dA = 2 * Math.PI / (alphaNo - 1);
        //    double dB = Math.PI / (betaNo - 1);
        //    double alpha = 0.0;
        //    double beta = 0.0;
        //    double ka, kb;
        //    if (alphaSlider == null) ka = 1.0;
        //    else ka = alphaSlider.Value;
        //    if (betaSlider == null) kb = 1.0;
        //    else kb = betaSlider.Value;
        //    double rMinLog = 0.0;
        //    double rMaxLog = 4.0;
        //    double rMin = 0.001;
        //    double rMax = 5.0;
        //    for (int j = 0; j < betaNo; j++)
        //    {
        //        beta = j * dB - Math.PI / 2;
        //        for (int i = 0; i < alphaNo; i++)
        //        {
        //            alpha = i * dA - Math.PI;
        //            chart3D.vertices[j * alphaNo + i] = new Vertex3D();
        //            double r = Math.Abs(2.0 * Math.Sin(ka * Math.PI * alpha) * Math.Sin(kb * Math.PI * beta) / (Math.PI * Math.PI * ka * alpha * kb * beta));
        //            r = rMinLog + (rMaxLog - rMinLog) * (Math.Log10(r / rMin) / Math.Log10(rMax / rMin));
        //            double x = r * Math.Sin(alpha) * Math.Cos(beta);
        //            double y = r * Math.Sin(beta);
        //            double z = r * Math.Cos(alpha) * Math.Cos(beta);
        //            ((PolarSurfaceChart3D)chart3D).SetPoint(i, j, (float)x, (float)y, (float)z);
        //        }
        //    }
        //    chart3D.GetDataRange();

        //    int vertNo = chart3D.GetDataNo();
        //    double xC = chart3D.XCenter();
        //    double yC = chart3D.YCenter();
        //    double zC = chart3D.ZCenter();

        //    double xMin = chart3D.XMin();
        //    double xMax = chart3D.XMax();
        //    double yMin = chart3D.YMin();
        //    double yMax = chart3D.YMax();
        //    double zMin = chart3D.ZMin();
        //    double zMax = chart3D.ZMax();

        //    for (int i = 0; i < vertNo; i++)
        //    {
        //        Vertex3D vert = chart3D[i];
        //        double r = Math.Pow((vert.x - xC) / (xMax - xMin), 2);
        //        r += Math.Pow((vert.y - yC) / (yMax - yMin), 2);
        //        r += Math.Pow((vert.z - zC) / (zMax - zMin), 2);
        //        if (colorSlider == null) r = 1.7 * Math.Sqrt(r);
        //        else r = colorSlider.Value * Math.Sqrt(r);

        //        Color color = TextureMapping.PseudoColor(r);
        //        chart3D[i].color = color;
        //    }
        //    chart3D.SetAxes(0, 0, 0, 2.2f, 2.2f, 2.2f);

        //    // 4. Get the Mesh3D array from surface chart
        //    ArrayList meshs = ((PolarSurfaceChart3D)chart3D).GetMeshes();

        //    // 5. display vertex no and triangle no of this surface chart
        //    UpdateModelSizeInfo(meshs);

        //    // 6. Set the model display of surface chart
        //    Model3D model3d = new Model3D();
        //    Material backMaterial = new DiffuseMaterial(new SolidColorBrush(Colors.Gray));
        //    chartModelIndex = model3d.UpdateModel(meshs, backMaterial, chartModelIndex, this.mainViewport);

        //    // 7. set projection matrix, so the data is in the display region
        //    //float xMin = chart3D.XMin();
        //    //float xMax = chart3D.XMax();
        //    transformMatrix.CalculateProjectionMatrix(xMin, xMax, xMin, xMax, zMin, zMax, 0.2);
        //    TransformChart();
        //}

        // function for testing surface chart
        //public void TestSurfacePlot(int gridNo)
        //{
        //    int xNo = gridNo;
        //    int yNo = gridNo;
        //    // 1. set the surface grid
        //    chart3D = new UniformSurfaceChart3D();
        //    ((UniformSurfaceChart3D)chart3D).SetGrid(xNo, yNo, -100, 100, -100, 100);

        //    // 2. set surface chart z value
        //    double xC = chart3D.XCenter();
        //    double yC = chart3D.YCenter();
        //    int vertNo = chart3D.GetDataNo();
        //    double zV;
        //    for (int i = 0; i < vertNo; i++)
        //    {
        //        Vertex3D vert = chart3D[i];

        //        double r = 0.15 * Math.Sqrt((vert.x - xC) * (vert.x - xC) + (vert.y - yC) * (vert.y - yC));
        //        if (r < 1e-10) zV = 1;
        //        else zV = Math.Sin(r) / r;

        //        chart3D[i].z = (float)zV;
        //    }
        //    chart3D.GetDataRange();

        //    // 3. set the surface chart color according to z value
        //    double xMin = chart3D.XMin(); xC = chart3D.XCenter();
        //    double xMax = chart3D.XMax(); yC = chart3D.YCenter();
        //    double yMin = chart3D.YMin(); double zC = (chart3D.ZMax() + chart3D.ZMin()) / 2;
        //    double yMax = chart3D.YMax();
        //    double zMin = chart3D.ZMin();
        //    double zMax = chart3D.ZMax();
        //    for (int i = 0; i < vertNo; i++)
        //    {
        //        Vertex3D vert = chart3D[i];
        //        double h = (vert.z - zMin) / (zMax - zMin);
        //        double r = Math.Pow((vert.x - xC) / (xMax - xMin), 2);
        //        r += Math.Pow((vert.y - yC) / (yMax - yMin), 2);
        //        r += Math.Pow((vert.z - zC) / (zMax - zMin), 2);
        //        r = Math.Sqrt(r);
        //        //double r = Math.Sqrt(Math.Pow((vert.x - xC) / (xMax - xC), 2) +
        //        //                     Math.Pow((vert.y - yC) / (yMax - yC), 2) +
        //        //                     Math.Pow((vert.z - zC) / (zMax - zC), 2));

        //        Color color = TextureMapping.PseudoColor(r);
        //        chart3D[i].color = color;
        //    }

        //    // 4. Get the Mesh3D array from surface chart
        //    ArrayList meshs = ((UniformSurfaceChart3D)chart3D).GetMeshes();

        //    // 5. display vertex no and triangle no of this surface chart
        //    UpdateModelSizeInfo(meshs);

        //    // 6. Set the model display of surface chart
        //    Model3D model3d = new Model3D();
        //    Material backMaterial = new DiffuseMaterial(new SolidColorBrush(Colors.Gray));
        //    chartModelIndex = model3d.UpdateModel(meshs, backMaterial, chartModelIndex, this.mainViewport);

        //    // 7. set projection matrix, so the data is in the display region
        //    //float xMin = chart3D.XMin();
        //    //float xMax = chart3D.XMax();
        //    transformMatrix.CalculateProjectionMatrix(xMin, xMax, xMin, xMax, zMin, zMax, 0.5);
        //    TransformChart();
        //}

        private void UpdateModelSizeInfo(ArrayList meshs)
        {
            int meshNo = meshs.Count;
            int chartVertNo = 0;
            int chartTriangelNo = 0;
            for (int i = 0; i < meshNo; i++)
            {
                chartVertNo += ((Mesh3D)meshs[i]).GetVertexNo();
                chartTriangelNo += ((Mesh3D)meshs[i]).GetTriangleNo();
            }
            // labelVertNo.Content = String.Format("Vertex No: {0:d}", chartVertNo);
            // labelTriNo.Content = String.Format("Triangle No: {0:d}", chartTriangelNo);
        }

        // this function is used to rotate, drag and zoom the 3d chart
        public void TransformChart()
        {
            if (chartModelIndex == -1) return;
            ModelVisual3D visual3d = (ModelVisual3D)(this.mainViewport.Children[chartModelIndex]);
            if (visual3d.Content == null) return;
            Transform3DGroup group1 = visual3d.Content.Transform as Transform3DGroup;
            group1.Children.Clear();
            group1.Children.Add(new MatrixTransform3D(transformMatrix.totalMatrix));
        }

        #region Обработка мыши и клавиатуры
        
        public void OnViewportMouseDown(object sender, MouseButtonEventArgs args)
        {
            Point pt = args.GetPosition(mainViewport);
            if ((args.ChangedButton == MouseButton.Left) ||
                (args.ChangedButton == MouseButton.Middle)) // rotate or drag 3d model
            {
                transformMatrix.OnLBtnDown(pt);
            }
            else if (args.ChangedButton == MouseButton.Right)   // select rect
            {
                // selectRect.OnMouseDown(pt, mainViewport, rectModelIndex);
            }
        }

        public void OnViewportMouseMove(object sender, System.Windows.Input.MouseEventArgs args)
        {
            Point pt = args.GetPosition(mainViewport);

            if ((args.LeftButton == MouseButtonState.Pressed) ||
                (args.MiddleButton == MouseButtonState.Pressed)) // rotate or drag 3d model
            {
                transformMatrix.OnMouseMove(pt, mainViewport, (args.MiddleButton == MouseButtonState.Pressed));
                TransformChart();
            }
            else if (args.RightButton == MouseButtonState.Pressed)          // select rect
            {
                // selectRect.OnMouseMove(pt, mainViewport, m_nRectModelIndex);
            }
            else
            {
                /*
                String s1;
                Point pt2 = m_transformMatrix.VertexToScreenPt(new Point3D(0.5, 0.5, 0.3), mainViewport);
                s1 = string.Format("Screen:({0:d},{1:d}), Predicated: ({2:d}, H:{3:d})", 
                    (int)pt.X, (int)pt.Y, (int)pt2.X, (int)pt2.Y);
                this.statusPane.Text = s1;
                */
            }
        }

        public void OnViewportMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs args)
        {
            Point pt = args.GetPosition(mainViewport);
            if (args.ChangedButton == MouseButton.Left)
            {
                transformMatrix.OnLBtnUp();
            }
            else if (args.ChangedButton == MouseButton.Right)
            {
                if (chartModelIndex == -1) return;
                // 1. get the mesh structure related to the selection rect
                MeshGeometry3D meshGeometry = Model3D.GetGeometry(mainViewport, chartModelIndex);
                if (meshGeometry == null) return;

                // 2. set selection in 3d chart
                // chart3D.Select(selectRect, transformMatrix, mainViewport);

                // 3. update selection display
                // 3dChart.HighlightSelection(meshGeometry, Color.FromRgb(200, 200, 200));
            }
        }

        // zoom in 3d display
        public void OnKeyDown(object sender, KeyEventArgs args)
        {
            transformMatrix.OnKeyDown(args);
            TransformChart();
        }

        private void OnViewportMouseWheel(object sender, MouseWheelEventArgs args)
        {
            transformMatrix.ViewScale(args.Delta / WheelSensitivity);
            TransformChart();
        }
        
        #endregion Обработки мыши и клавиатуры

        #region Обработки элементов управления
        
        public void viewXYZButton_Click(object sender, RoutedEventArgs e)
        {
            transformMatrix.ViewXYZ();
            TransformChart();
        }

        public void viewZYXButton_Click(object sender, RoutedEventArgs e)
        {
            transformMatrix.ViewZYX();
            TransformChart();
        }

        public void viewXYButton_Click(object sender, RoutedEventArgs e)
        {
            transformMatrix.ViewXY();
            TransformChart();
        }

        public void viewXZButton_Click(object sender, RoutedEventArgs e)
        {
            transformMatrix.ViewXZ();
            TransformChart();
        }

        public void viewYZButton_Click(object sender, RoutedEventArgs e)
        {
            transformMatrix.ViewYZ();
            TransformChart();
        }

        public void controlCSysPolarRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            chartPrm.coordinateSystem = CoordinateSystem.Polar;
            ShowChart3D(chart3DInd);
        }

        public void controlCSysRectRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            chartPrm.coordinateSystem = CoordinateSystem.Rect;
            ShowChart3D(chart3DInd);
        }

        public void controlCSclLinRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            chartPrm.coordinateScale = CoordinateScale.Lin;
            ShowChart3D(chart3DInd);
        }

        public void controlCSclLogRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            chartPrm.coordinateScale = CoordinateScale.Log;
            ShowChart3D(chart3DInd);
        }

        public void SetLogMin(double lm)
        {
            this.chartPrm.logMin = lm;
        }

        public void gridRectXYCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            chartPrm.gridRect.XY = true;
            ShowChart3D(chart3DInd);
        }

        public void gridRectXYCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            chartPrm.gridRect.XY = false;
            ShowChart3D(chart3DInd);
        }

        public void gridRectXZCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            chartPrm.gridRect.XZ = true;
            ShowChart3D(chart3DInd);
        }

        public void gridRectXZCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            chartPrm.gridRect.XZ = false;
            ShowChart3D(chart3DInd);
        }

        public void gridRectYZCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            chartPrm.gridRect.YZ = true;
            ShowChart3D(chart3DInd);
        }

        public void gridRectYZCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            chartPrm.gridRect.YZ = false;
            ShowChart3D(chart3DInd);
        }

        public void gridPolarXYCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            chartPrm.gridPolar.XY = true;
            ShowChart3D(chart3DInd);
        }

        public void gridPolarXYCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            chartPrm.gridPolar.XY = false;
            ShowChart3D(chart3DInd);
        }

        public void gridPolarXZCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            chartPrm.gridPolar.XZ = true;
            ShowChart3D(chart3DInd);
        }

        public void gridPolarXZCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            chartPrm.gridPolar.XZ = false;
            ShowChart3D(chart3DInd);
        }

        public void gridPolarYZCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            chartPrm.gridPolar.YZ = true;
            ShowChart3D(chart3DInd);
        }

        public void gridPolarYZCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            chartPrm.gridPolar.YZ = false;
            ShowChart3D(chart3DInd);
        }

        #endregion Обработки элементов управления

    }
}
