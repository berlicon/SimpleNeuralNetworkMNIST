using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

namespace SimpleNeuralNetworkMNIST
{
    class Program
    {
        const int IMAGE_SIZE = 28;              //each image 28*28 pixels
        const int SAMPLE_COUNT = 10;            //analyse 10 images - numbers 0..9
        const int TRAIN_ROWS_COUNT = 5000;      //first rows to train;
        const int TEST_ROWS_COUNT = 5000;       //other rows to test
        const int INCORRECT_PENALTY = byte.MaxValue * TRAIN_ROWS_COUNT;  //penalty for incorrect overlap
        //const string FILE_PATH = @"C:\Users\3208080\Downloads\mnist-in-csv\mnist_test_200_rows.csv";//43% 100+100
        //const string FILE_PATH = @"C:\Users\3208080\Downloads\mnist-in-csv\mnist_test_2000_rows.csv";//53% 1000+1000
        //const string FILE_PATH = @"C:\Users\3208080\Downloads\mnist-in-csv\mnist_test_2000_rows.csv";//56% 1900+100
        //const string FILE_PATH = @"C:\Users\3208080\Downloads\mnist-in-csv\mnist_test.csv";//50% 9900+100
        //const string FILE_PATH = @"C:\Users\3208080\Downloads\mnist-in-csv\mnist_test.csv";//56% 9000+1000
        const string FILE_PATH = @"C:\Users\3208080\Downloads\mnist-in-csv\mnist_test.csv";//57% 5000+5000
        //const string FILE_PATH = @"C:\Users\3208080\Downloads\mnist-in-csv\mnist_test.csv";//49% 1000+9000
        //const string FILE_PATH = @"C:\Users\3208080\Downloads\mnist-in-csv\mnist_test.csv";//41% 100+9900

        //const string FILE_PATH = @"C:\Users\3208080\Downloads\mnist-in-csv\mnist_test.csv";//55% 5000+5000 black/white

        private static long[, ,] layerAssotiations = new long[SAMPLE_COUNT, IMAGE_SIZE, IMAGE_SIZE];
        private static Dictionary<long, long> layerResult = new Dictionary<long, long>();
        private static long correctResults = 0;
        
        static void Main(string[] args)
        {
            train();
            test();

            Console.WriteLine("Правильно распознано {0}% вариантов",
                100 * correctResults / TEST_ROWS_COUNT);
        }

        private static void train()
        {
            Console.WriteLine("Начало тренировки нейросети");
            var index = 1;
            var rows = File.ReadAllLines(FILE_PATH).Skip(1).Take(TRAIN_ROWS_COUNT).ToList();

            foreach (var row in rows)
            {
                Console.WriteLine("Итерация {0} из {1}", index++, TRAIN_ROWS_COUNT);
                var values = row.Split(',');
                for (int i = 1; i < values.Length; i++)
                {
                    var value = byte.Parse(values[i]);  //var value = (values[i] == "0") ? 0 : 1;
                    layerAssotiations[
                        byte.Parse(values[0]), 
                        (i - 1) / IMAGE_SIZE, 
                        (i - 1) % IMAGE_SIZE]
                        += value;
                }
            }
        }

        private static void test()
        {
            Console.WriteLine("Начало тестирования нейросети");
            var index = 1;
            var rows = File.ReadAllLines(FILE_PATH).Skip(1 + TRAIN_ROWS_COUNT).Take(TEST_ROWS_COUNT).ToList();

            foreach (var row in rows)
            {
                Console.WriteLine("Итерация {0} из {1}", index++, TEST_ROWS_COUNT);
                clearResultLayer();

                var values = row.Split(',');
                for (int i = 1; i < values.Length; i++)
                {
                    var value = byte.Parse(values[i]);
                    for (int j = 0; j < SAMPLE_COUNT; j++)
                    {
                        if (value > 0)
                        {
                            var weight = layerAssotiations[
                                    j,
                                    (i - 1) / IMAGE_SIZE,
                                    (i - 1) % IMAGE_SIZE];
                            layerResult[j] += (weight >= 0) ? weight : -INCORRECT_PENALTY;
                        }
                    }
                }

                calculateStatistics(byte.Parse(values[0]));
            }
        }

        private static void clearResultLayer()
        {
            layerResult = new Dictionary<long, long>();
            for (int i = 0; i < SAMPLE_COUNT; i++) layerResult[i] = 0;
        }

        private static void calculateStatistics(byte correctNumber)
        {
            var proposalNumber = layerResult.OrderByDescending(p => p.Value).First().Key;
            Console.WriteLine("Число {0} определено как {1} {2}", correctNumber, proposalNumber,
                proposalNumber == correctNumber ? "УСПЕХ" : "НЕУДАЧА");
            if (proposalNumber == correctNumber) correctResults++;
        }
    }
}
