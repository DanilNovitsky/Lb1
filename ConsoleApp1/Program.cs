using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Виберiть опцiю:");
            Console.WriteLine("1. Використовувати дефолтну матрицю");
            Console.WriteLine("2. Ввести свою матрицю");
            int choice;

            MatrixProcessor matrixProcessor;
            
            if (!int.TryParse(Console.ReadLine(), out choice) || (choice != 1 && choice != 2)) {
                Console.WriteLine("Неправильний вибiр");
                return;
            }

            if (choice == 1)
            {
                matrixProcessor = new MatrixProcessor(DefaultMatrix());
            }
            else
            {
                matrixProcessor = new MatrixProcessor(ReadMatrixFromUser());
            }

            int[] threadCountsToTest = {1, 2, 3, 4}; // Масив кількостей потоків для тестування
            foreach (int threadCount in threadCountsToTest)
            {
                double totalMultiThreadTime = 0.0;

                int repeatCount = 5; // Кількість повторень
                for (int i = 0; i < repeatCount; i++)
                {
                    matrixProcessor.CalculateMaxRowValuesMultiThread(threadCount);
                    double multiThreadTimeInSeconds = matrixProcessor.GetMultiThreadTimeInSeconds();
                    totalMultiThreadTime += multiThreadTimeInSeconds;
                }

                double averageMultiThreadTimeInSeconds = totalMultiThreadTime / repeatCount;
                Console.WriteLine($"Тестування з {threadCount} потоками:");
                matrixProcessor.PrintResult();
                Console.WriteLine($"Середнiй час багатопотокового розрахунку ({threadCount} потоки): {averageMultiThreadTimeInSeconds:F6} секунд");
            }
        }

        static int[][] DefaultMatrix()
        {
            return new int[][] {
            new int[] { 1, 2, 3 },
            new int[] { 4, 5, 6 },
            new int[] { 7, 8, 9 }
        };
        }

        static int[][] ReadMatrixFromUser()
        {
            int rowCount, colCount;
            do
            {
                Console.WriteLine("Введiть кiлькiсть рядкiв матрицi:");
            } while (!int.TryParse(Console.ReadLine(), out rowCount) || rowCount <= 0);

            do
            {
                Console.WriteLine("Введiть кiлькiсть стовпцiв матрицi:");
            } while (!int.TryParse(Console.ReadLine(), out colCount) || colCount <= 0);

            int[][] inputMatrix = new int[rowCount][];
            for (int i = 0; i < rowCount; i++)
            {
                inputMatrix[i] = new int[colCount];
                Console.WriteLine($"Введiть значення для рядка {i + 1} (роздiлiть значення пробілами):");
                string[] values = Console.ReadLine().Split(' ');
                if (values.Length != colCount)
                {
                    Console.WriteLine("Некоректна кiлькiсть значень. Спробуйте ще раз.");
                    i--; // Повторяем ввод для этой строки
                    continue;
                }
                for (int j = 0; j < colCount; j++)
                {
                    if (!int.TryParse(values[j], out inputMatrix[i][j]))
                    {
                        Console.WriteLine("Некоректне значення. Спробуйте ще раз.");
                        i--; // Повторяем ввод для этой строки
                        break;
                    }
                }
            }
            return inputMatrix;
        }
    }

    internal class MatrixProcessor
    {
        private readonly int[][] matrix;
        private int[] maxRowValues;
        private long singleThreadTime;
        private long multiThreadTime;

        public MatrixProcessor(int[][] matrix)
        {
            this.matrix = matrix;
        }

        public void CalculateMaxRowValuesMultiThread(int threadCount)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            
            int rowCount = matrix.Length;
            maxRowValues = new int[rowCount];

            Thread[] threads = new Thread[threadCount];

            for (int i = 0; i < threadCount; i++)
            {
                int startRow = i * (rowCount / threadCount);
                int endRow = (i == threadCount - 1) ? rowCount : (i + 1) * (rowCount / threadCount);

                threads[i] = new Thread(() =>
                {
                    for (int j = startRow; j < endRow; j++)
                    {
                        int max = int.MinValue;
                        for (int k = 0; k < matrix[j].Length; k++)
                        {
                            if (matrix[j][k] > max)
                            {
                                max = matrix[j][k];
                            }
                        }
                        maxRowValues[j] = max;
                    }
                });
            }

            // Запускаємо потоки
            foreach (Thread thread in threads)
            {
                thread.Start();
            }

            // Очікуємо завершення усіх потоків
            foreach (Thread thread in threads)
            {
                thread.Join();
            }

            stopwatch.Stop();
            multiThreadTime = stopwatch.ElapsedMilliseconds;
        }

        public void PrintResult()
        {
            Console.WriteLine("Результат:");
            foreach (int max in maxRowValues)
            {
                Console.Write(max + " ");
            }
            Console.WriteLine();
        }

        public double GetMultiThreadTimeInSeconds()
        {
            return multiThreadTime; 
        }
    }
}
