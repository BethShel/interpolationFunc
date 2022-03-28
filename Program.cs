using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using org.mariuszgromada.math.mxparser;

namespace _1._1laba
{
    internal class Program
    {
        private struct ToSave
        {
            public double[] X { get; set; }
            public double[] Y { get; set; }
            public double[] InterpolationValues { get; set; }
            public double[] XForDrawing { get; set; }
        };

        static void Main()
        {
            int n;
            double a, b;
            Console.Write("Введите количество сегментов n: ");
            n = int.Parse(Console.ReadLine());
            Console.Write("Введите левую границу a: "); 
            a = double.Parse(Console.ReadLine());
            Console.Write("Введите правую границу b: ");
            b = double.Parse(Console.ReadLine());

            double[,] matrix = generate(n + 1, a, b);

            Console.WriteLine("Исходная табличная функция");
            print(matrix);

            double[] interpolationValues = new double[n + 1];
            for (int i = 0; i < n + 1; ++i)
            {
                interpolationValues[i] = Calculate(matrix, matrix[0, i], n + 1);
            }

            double maxError = countError(matrix, interpolationValues, n + 1, a, b);

            Console.WriteLine($"Погрешность = {maxError}");

            double[] interpolationValuesForDrawing = createInterpolationValuesForDrawing(matrix, n + 1, a, b, out double[] xForDrawing);
            //for (int i = 0; i < 3; ++i)
            //{
            //    double[,] save = merge(interpolationValuesForDrawing, xForDrawing);
            //    interpolationValuesForDrawing = createInterpolationValuesForDrawing(save, interpolationValuesForDrawing.Length, a, b, out double[] temp);
            //    xForDrawing = temp;
            //}
            

            drawingGraph(matrix, interpolationValuesForDrawing, xForDrawing, n + 1);

            Console.Read();
        }

        private static double[,] merge(double[] y, double[] x)
        {
            double[,] res = new double[2, y.Length];
            for (int i = 0; i < y.Length; ++i)
            {
                res[0, i] = x[i];
                res[1, i] = y[i];
            }
            return res;
        }

        //создание массива х и у для построения графика интерполяции
        static double[] createInterpolationValuesForDrawing(double[,] matrix, int size, double a, double b, out double[] xForDrawing)
        {
            double[] res = new double[size * 2 - 1];
            xForDrawing = new double[size * 2 - 1];
            double[,] temp = new double[2, size * 2 - 1];
            double[,] tempMatrix = /*generate(size * 3, a, b);*/new double[2, size * 2 - 1];
            for (int i = 0, j = 0; i < size * 2 - 1; i += 2, ++j)
            {
                tempMatrix[0, i] = matrix[0, j];
                tempMatrix[1, i] = matrix[1, j];
                temp[0, i] = matrix[0, j];
                temp[1, i] = matrix[1, j];
            }
            for (int i = 1; i < size * 2 - 1; i += 2)
            {
                temp[0, i] = (tempMatrix[0, i - 1] + tempMatrix[0, i + 1]) / 2;
                temp[1, i] = f(temp[0, i]);
            }
            for (int i = 1; i < size * 2 - 1; i += 2)
            {
                tempMatrix[0, i] = (tempMatrix[0, i - 1] + tempMatrix[0, i + 1]) / 2;
                tempMatrix[1, i] = Calculate(temp, tempMatrix[0, i], size * 2 - 1);
            }
            //for (int i = 0; i < size * 2 - 1; ++i)
            //{
            //    xForDrawing[i] = tempMatrix[0, i];
            //    res[i] = Calculate(tempMatrix, xForDrawing[i], size * 3);
            //}
            for (int i = 0; i < size * 2 - 1; ++i)
            {
                res[i] = tempMatrix[1, i];
                xForDrawing[i] = tempMatrix[0, i];
            }
            return res;
        }

        static double[] createErrorValues(int size, double a, double b, out double[] xForDrawing)
        {
            double[] res = new double[size * 3];
            xForDrawing = new double[size * 3];
            double step = (b - a) / (size * 3 - 1);
            double[,] tempMatrix = generate(size * 3, a, b);
            for (int i = 0; i < size * 3; ++i)
            {
                xForDrawing[i] = a + i * step;
                res[i] = Math.Abs(Calculate(tempMatrix, xForDrawing[i], size * 3) - f(xForDrawing[i]));
            }
            return res;
        }

        static double countError(double[,] matrix, double[] interpolarValues, int size, double a, double b)
        {
            double res = int.MinValue;
            double[] xForDrawing = new double[size * 3];
            double step = (b - a) / (size * 3 - 1);
            double[,] tempMatrix = generate(size * 3, a, b);
            for (int i = 0; i < size * 3; ++i)
            {
                xForDrawing[i] = a + i * step;
                res = Math.Max(Math.Abs(Calculate(tempMatrix, xForDrawing[i], size * 3) - f(xForDrawing[i])), res);
            }
            return res;
            //double max = 0;
            //for (int i = 0; i < size; ++i)
            //{
            //    double temp = Math.Abs(interpolarValues[i] - matrix[1, i]);
            //    if (temp > max)
            //    {
            //        max = temp;
            //    }
            //}
            //return max;
        }

        static void drawingGraph(double[,] matrix, double[] interpolationValues, double[] xForDrawing, int size)
        {
            try
            {
                ToSave data = new ToSave();
                double[] x = new double[size];
                double[] y = new double[size];
                for (int i = 0; i < size; ++i)
                {
                    x[i] = matrix[0, i];
                    y[i] = matrix[1, i];
                }
                data.X = x;
                data.Y = y;
                data.InterpolationValues = interpolationValues;
                data.XForDrawing = xForDrawing;

                string json = JsonSerializer.Serialize(data);
                File.WriteAllText(@"D:\Лиза\ВУЗ\ЧМ\InterpolationV\InterpolationFunctionsByAlgebraicPolynomials\Save\temp.json", json);

                Process p = Process.Start(@"D:\Лиза\ВУЗ\ЧМ\InterpolationV\InterpolationFunctionsByAlgebraicPolynomials\visualizationGraphs\visualizationGraphs.py");
                p.WaitForExit();
            }
            catch (Win32Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static double[,] generate(int n, double a, double b)
        {
            double[,] result = new double[2, n];

            uniformPartitioning(result, a, b, n);
            //ChebyshevPartition(result, a, b, n);

            return result;
        }

        private static void ChebyshevPartition(double[,] matrix, double a, double b, int size)
        {
            for (int i = 0; i < size; ++i)
            {
                matrix[0, i] = (a + b) / 2 - (b - a) * Math.Cos((2 * i + 1) * Math.PI / (2 * (size - 1) + 2)) / 2;
                matrix[1, i] = f(matrix[0, i]);
            }
        }

        private static void uniformPartitioning(double[,] matrix, double a, double b, int size)
        {
            for (int i = 0; i < size; ++i)
            {
                matrix[0, i] = a + (b - a) * i / (size - 1);
                matrix[1, i] = f(matrix[0, i]);
            }
        }

        private static double f(double x)
        {
            return x * x * x;//Math.Cos(x);//Math.Abs(x);//Math.Cos(x);//x * x * x - 1;//Math.Log(x);
        }

        private static void print(double[,] matrix)
        {
            int n = matrix.GetLength(0);
            int m = matrix.GetLength(1);
            for (int i = 0; i < n; ++i)
            {
                for (int j = 0; j < m; ++j)
                {
                    Console.Write(Math.Round(matrix[i, j], 3) + " ");
                }
                Console.WriteLine();
            }
        }
        public static double Calculate(double[,] matrix, double value, int size)
        {
            double result = matrix[1, 0];

            for (var i = 1; i < size; ++i)
            {
                double f = 0;
                for (var j = 0; j <= i; ++j)
                {
                    double den = 1;

                    for (var k = 0; k <= i; ++k)
                    {
                        if (k != j)
                        {
                            den *= (matrix[0, j] - matrix[0, k]);
                        }
                    }

                    f += matrix[1, j] / den;
                    //f += den / matrix[1, j];
                }

                for (var k = 0; k < i; ++k)
                {
                    f *= (value - matrix[0, k]);
                }

                result += f;
            }

            return result;
        }

        public static double determinant(double[,] matrix, int size)
        {
            double res = 1;
            double[,] gausMatrix = gaus(matrix, size);
            for (int i = 0; i < size; ++i)
            {
                res *= gausMatrix[i, i];
            }
            return res;
        }
        //метод гаусса
        public static double[,] gaus(double[,] matrix, int size)
        {
            double[,] res = new double[size, size];

            for (int i = 0; i < size; ++i)
            {
                for (int j = 0; j < size; ++j)
                {
                    res[i,j] = matrix[i,j];
                }
            }

            Boolean flag = false;
            for (int i = 0; i < size - 1 && !flag; ++i)
            {
                for (int j = i + 1; j < size && !flag; ++j)
                {
                    if (res[i,i] != 0)
                    {
                        double value = -res[j,i] / res[i,i];
                        res[j,i] = 0;
                        for (int k = i + 1; k < size; ++k)
                        {
                            res[j,k] += res[i,k] * value;
                        }
                    }
                    else
                    {
                        flag = true;
                    }
                }
            }
            return res;
        }
    }
}

