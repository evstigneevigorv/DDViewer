using System;
using System.IO;
using System.Numerics;

namespace DDDrawerConsole
{
    class Program
    {
        // Параметры ФАР
        private const double lambda = 0.12; // Длина волны
        private const int trmXNum = 8;      // Количество излучателей по оси X
        private const int trmYNum = 8;      // Количество излучателей по оси Y
        private const double xStep = 0.06;  // Шаг решетки по оси X
        private const double yStep = 0.06;  // Шаг решетки по оси Y
        private enum CSType { DEC, CALC, TOPO };
        private const CSType csType = CSType.CALC; // Система координат

        // Параметры ДН
        private const int dotNumA = 100;    // Количество точек по азимуту
        private const int dotNumB = 100;    // Количество точек по углу места
        private const int itrtNum = 21;     // Количество ДН

        // Параметры файла для сохранения ДН
        private const string fileName = "5 Ошибка по фазе.tdd";    // Имя выходного файла

        static void Main(string[] args)
        {
            StreamWriter sw = File.CreateText(fileName);

            // Итерации вычисления ДН
            for (int itrt = 0; itrt < itrtNum; itrt++)
            {
                // 1). Расчет координат и параметров излучателей
                Transmitter[] trms = new Transmitter[trmXNum * trmYNum];
                Random rnd = new Random();
                for (int j = 0; j < trmYNum; j++)
                    for (int i = 0; i < trmXNum; i++)
                    {
                        double phaseError = rnd.NextDouble() * (Math.PI / 2) * itrt / itrtNum;
                        trms[j * trmXNum + i] = new Transmitter(i * xStep - (trmXNum - 1) * xStep / 2,
                                                                j * yStep - (trmYNum - 1) * yStep / 2,
                                                                0.0,
                                                                1.0,
                                                                0.0 + phaseError, // i * Math.PI * (itrt - 10.0) / 10.0,
                                                                true);
                    }
                
                // 2). Расчет ДН
                double[,] dotArray = new double[dotNumA * dotNumB, 3];
                double x, y, z;
                double phi, theta;
                double alpha, beta;
                Complex r;
                double xDeltaPhase, yDeltaPhase; // Набег фазы сигнала в отдельном излучателе

                for (int j = 0; j < dotNumB; j++)
                    for (int i = 0; i < dotNumA; i++)
                    {
                        r = Complex.Zero;
                        switch (csType)
                        {
                            case CSType.DEC :
                                {
                                    
                                } break;
                            case CSType.CALC :
                                {
                                    phi = i * 2 * Math.PI / (dotNumA - 1) - Math.PI;
                                    theta = Math.PI - j * Math.PI / (dotNumB - 1);

                                    for (int k = 0; k < trmXNum * trmYNum; k++)
                                        if (trms[k].IsEnabled)
                                        {
                                            xDeltaPhase = 2 * Math.PI * trms[k].X * Math.Sin(theta) * Math.Cos(phi) / lambda;
                                            yDeltaPhase = 2 * Math.PI * trms[k].Y * Math.Sin(theta) * Math.Sin(phi) / lambda;
                                            r += trms[k].A *
                                                 Complex.Exp(Complex.ImaginaryOne * (trms[k].Phase +
                                                                                     xDeltaPhase +
                                                                                     yDeltaPhase));
                                        }

                                    dotArray[j * dotNumA + i, 0] = phi;
                                    dotArray[j * dotNumA + i, 1] = theta;
                                    dotArray[j * dotNumA + i, 2] = r.Magnitude / (trmXNum * trmYNum);
                                } break;
                            case CSType.TOPO :
                                {
                                    alpha = i * 2 * Math.PI / (dotNumA - 1) - Math.PI;
                                    beta = j * Math.PI / (dotNumB - 1) - Math.PI / 2;
                                } break;
                        }
                    }

                // 3. Запись результатов в файл
                sw.WriteLine("# " + csType.ToString() + " " + "Решетка: " + trmXNum.ToString() + " x " + trmYNum.ToString() +
                             " - Ошибка по фазе до " + 90.0 * itrt / itrtNum + " градусов");
                string str = "";
                for (int j = 0; j < dotNumB - 1; j++)
                    for (int i = 0; i < dotNumA - 1; i++)
                    {
                        // Первый треугольник
                        str = // Первая вершина
                              dotArray[j * dotNumA + i, 0].ToString("0.00000000") + "\t" +
                              dotArray[j * dotNumA + i, 1].ToString("0.00000000") + "\t" +
                              dotArray[j * dotNumA + i, 2].ToString("0.00000000") + "\t" +
                              // Вторая вершина
                              dotArray[j * dotNumA + i + 1, 0].ToString("0.00000000") + "\t" +
                              dotArray[j * dotNumA + i + 1, 1].ToString("0.00000000") + "\t" +
                              dotArray[j * dotNumA + i + 1, 2].ToString("0.00000000") + "\t" +
                              // Третья вершина
                              dotArray[(j + 1) * dotNumA + i, 0].ToString("0.00000000") + "\t" +
                              dotArray[(j + 1) * dotNumA + i, 1].ToString("0.00000000") + "\t" +
                              dotArray[(j + 1) * dotNumA + i, 2].ToString("0.00000000");
                        sw.WriteLine(str);

                        // Второй треугольник
                        str = // Первая вершина
                              dotArray[(j + 1) * dotNumA + i, 0].ToString("0.00000000") + "\t" +
                              dotArray[(j + 1) * dotNumA + i, 1].ToString("0.00000000") + "\t" +
                              dotArray[(j + 1) * dotNumA + i, 2].ToString("0.00000000") + "\t" +
                              // Вторая вершина
                              dotArray[j * dotNumA + i + 1, 0].ToString("0.00000000") + "\t" +
                              dotArray[j * dotNumA + i + 1, 1].ToString("0.00000000") + "\t" +
                              dotArray[j * dotNumA + i + 1, 2].ToString("0.00000000") + "\t" +
                              // Третья вершина
                              dotArray[(j + 1) * dotNumA + i + 1, 0].ToString("0.00000000") + "\t" +
                              dotArray[(j + 1) * dotNumA + i + 1, 1].ToString("0.00000000") + "\t" +
                              dotArray[(j + 1) * dotNumA + i + 1, 2].ToString("0.00000000");
                        sw.WriteLine(str);
                    }
            }

            sw.Close();
        }
    }
}
