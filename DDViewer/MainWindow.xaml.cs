using Microsoft.Win32;
using System.Collections;
using System.IO;
using System.Threading;
using System.Windows.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Media3D;

namespace DDViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public enum CSType { DEC, CALC, TOPO }

    public partial class MainWindow : Window
    {
        public ControlWindow controlWindow = new ControlWindow();
        private PlayerWindow playerWindow = new PlayerWindow();
        public Viewer3DWindow viewer3DWindow = new Viewer3DWindow();
        private ProgressWindow progressWindow;
        private double taskProgress = 0.0;
        private SectionWindow azimuthSectionWindow = new SectionWindow();
        private SectionWindow elevationSectionWindow = new SectionWindow();

        private Dispatcher mainUIDispatcher { set; get; }
        private string fileName = "";
        private delegate void DispatcherDelegate();

        #region Инициализация
        
        public MainWindow()
        {
            InitializeComponent();

            mainUIDispatcher = Dispatcher.CurrentDispatcher;

            controlWindow.cSysPolarRadioButton.Checked += viewer3DWindow.controlCSysPolarRadioButton_Checked;
            controlWindow.cSysRectRadioButton.Checked += viewer3DWindow.controlCSysRectRadioButton_Checked;
            controlWindow.cSclLinRadioButton.Checked += viewer3DWindow.controlCSclLinRadioButton_Checked;
            controlWindow.cSclLogRadioButton.Checked += viewer3DWindow.controlCSclLogRadioButton_Checked;
            controlWindow.cSclLogMinComboBox.SelectionChanged += controlCSclLogMinComboBox_SelectionChanged;

            controlWindow.gridRectXYCheckBox.Checked += viewer3DWindow.gridRectXYCheckBox_Checked;
            controlWindow.gridRectXYCheckBox.Unchecked += viewer3DWindow.gridRectXYCheckBox_Unchecked;
            controlWindow.gridRectXZCheckBox.Checked += viewer3DWindow.gridRectXZCheckBox_Checked;
            controlWindow.gridRectXZCheckBox.Unchecked += viewer3DWindow.gridRectXZCheckBox_Unchecked;
            controlWindow.gridRectYZCheckBox.Checked += viewer3DWindow.gridRectYZCheckBox_Checked;
            controlWindow.gridRectYZCheckBox.Unchecked += viewer3DWindow.gridRectYZCheckBox_Unchecked;

            controlWindow.gridPolarXYCheckBox.Checked += viewer3DWindow.gridPolarXYCheckBox_Checked;
            controlWindow.gridPolarXYCheckBox.Unchecked += viewer3DWindow.gridPolarXYCheckBox_Unchecked;
            controlWindow.gridPolarXZCheckBox.Checked += viewer3DWindow.gridPolarXZCheckBox_Checked;
            controlWindow.gridPolarXZCheckBox.Unchecked += viewer3DWindow.gridPolarXZCheckBox_Unchecked;
            controlWindow.gridPolarYZCheckBox.Checked += viewer3DWindow.gridPolarYZCheckBox_Checked;
            controlWindow.gridPolarYZCheckBox.Unchecked += viewer3DWindow.gridPolarYZCheckBox_Unchecked;

            controlWindow.viewXYZButton.Click += viewer3DWindow.viewXYZButton_Click;
            controlWindow.viewZYXButton.Click += viewer3DWindow.viewZYXButton_Click;
            controlWindow.viewXYButton.Click += viewer3DWindow.viewXYButton_Click;
            controlWindow.viewXZButton.Click += viewer3DWindow.viewXZButton_Click;
            controlWindow.viewYZButton.Click += viewer3DWindow.viewYZButton_Click;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {

            #region Инициализация Dock-панелей
            
            dockManager.ParentWindow = this;
            
            azimuthSectionWindow.DockManager = dockManager;
            elevationSectionWindow.DockManager = dockManager;
            viewer3DWindow.DockManager = dockManager;
            controlWindow.DockManager = dockManager;
            playerWindow.DockManager = dockManager;

            azimuthSectionWindow.Title = "Сечение - азимутальная плоскость";
            elevationSectionWindow.Title = "Сечение - угломестная плоскость";

            azimuthSectionWindow.Show();
            azimuthSectionWindow.Hide();
            elevationSectionWindow.Show();
            elevationSectionWindow.Hide();
            viewer3DWindow.ShowAsDocument();
            controlWindow.Show(Dock.Right);
            playerWindow.Show(Dock.Bottom);
            playerWindow.Hide();
            
            #endregion Инициализация Dock-панелей

        }

        #endregion Инициализация

        #region Главное меню
        
        private void MainMenuExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        
        private void MainMenuOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openDlg = new OpenFileDialog();
            openDlg.Title = "Открыть запись ДН";
            openDlg.Filter = "Запись ДН (*.tdd)|*.tdd|Все файлы (*.*)|*.*";
            openDlg.DefaultExt = "tdd";
            openDlg.InitialDirectory = "D:\\Work\\MSVS 11.0\\Projects\\DDViewer\\DDDrawerConsole\\bin\\Debug";
            openDlg.RestoreDirectory = true;
            openDlg.Multiselect = true;

            if (openDlg.ShowDialog().GetValueOrDefault() != true) return;
            fileName = openDlg.FileName;
            Thread loadThread = new Thread(LoadAction);
            progressWindow = new ProgressWindow();
            progressWindow.Title = "Загрузка файла " + fileName;
            loadThread.Start();
            progressWindow.ShowDialog();

            if ((chart3DSlider.Maximum = (viewer3DWindow.chart3DList.Count - 1)) > 1)
            {
                chart3DSlider.Visibility = Visibility.Visible;
                chart3DSlider.Focus();
            }
            else
            {
                chart3DSlider.Visibility = Visibility.Hidden;
            }
            chart3DSlider.Value = 0.0;
            viewer3DWindow.ShowChart3D(0);
        }

        private void LoadAction()
        {
            StreamReader sr = File.OpenText(fileName);
            char[] delimiters = { ' ', '\t', ';' };
            string str = sr.ReadLine();

            int strNo = 1; taskProgress = 0.0;
            viewer3DWindow.chart3DList.Clear();
            while (!sr.EndOfStream)
            {
                string[] words = new string[0];
                string caption = "";
                words = str.Split(delimiters);
                CSType csType = CSType.DEC;
                if ((words[0] == "#") && (words[1] != null))
                    if (words[1] == "DEC") csType = CSType.DEC;
                    else if (words[1] == "CALC") csType = CSType.CALC;
                    else if (words[1] == "TOPO") csType = CSType.TOPO;
                    else
                    {
                        sr.Close();
                        return;
                    }
                for (int i = 2; i < words.Length; i++) caption += words[i] + " ";
                str = " ";
                ArrayList verts = new ArrayList();

                while (!sr.EndOfStream)
                {
                    str = sr.ReadLine(); strNo++;
                    if (str[0] != '#')
                    {

                        words = str.Split(delimiters);
                        if (words.Length != 9)
                        {
                            MessageBox.Show("Неправильный формат файла: в строке №" + strNo.ToString() +
                                            " найдено " + words.Length.ToString() + " координат(ы). Загрузка будет прервана.",
                                            "Ошибка чтения файла",
                                            MessageBoxButton.OK,
                                            MessageBoxImage.Warning);
                            progressWindow.Close();

                            return;
                        }
                        verts.Add(new Point3D(double.Parse(words[0]),
                                              double.Parse(words[1]),
                                              double.Parse(words[2])));
                        verts.Add(new Point3D(double.Parse(words[3]),
                                              double.Parse(words[4]),
                                              double.Parse(words[5])));
                        verts.Add(new Point3D(double.Parse(words[6]),
                                              double.Parse(words[7]),
                                              double.Parse(words[8])));
                    }
                    else break;

                    if (strNo % 1000 == 0)
                    {
                        taskProgress = sr.BaseStream.Position * 100.0 / sr.BaseStream.Length;
                        mainUIDispatcher.Invoke(new DispatcherDelegate(ProgressWindowUpdate));
                    }
                }
                viewer3DWindow.CreateChart3D(verts, csType, caption);
            }

            taskProgress = 100.0;
            mainUIDispatcher.Invoke(new DispatcherDelegate(ProgressWindowUpdate));
            sr.Close();
        }

        private void ProgressWindowUpdate()
        {
            progressWindow.progressBar.Value = taskProgress;
        }

        #endregion Главное меню

        private void mainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            viewer3DWindow.OnKeyDown(sender, e);
        }

        private void chart3DSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            viewer3DWindow.ShowChart3D((int)chart3DSlider.Value);
        }
        
        public void controlCSclLogMinComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            viewer3DWindow.SetLogMin(-10.0 * (controlWindow.cSclLogMinComboBox.SelectedIndex + 1));            
            viewer3DWindow.ShowChart3D(viewer3DWindow.chart3DInd);
        }

    }
}
