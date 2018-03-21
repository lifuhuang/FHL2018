namespace CountQueryPairs
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Microsoft.VisualBasic.FileIO;

    class Program
    {
        static void Main(string[] args)
        {
            var inputs = GetUserInputs();
            var input = inputs.Item1;
            var output = inputs.Item2;
            var k = inputs.Item3;

            var counts = CountQueryPairCounts(input, k);
            var result = counts.Where(item => item.Value > 1).OrderByDescending(item => item.Value);

            OutputResult(result);
            OutputResult(result, new FileStream(output, FileMode.Create));

            Console.WriteLine("\nCompleted!");
            Console.ReadKey();
        }

        private static void OutputResult(IOrderedEnumerable<KeyValuePair<Tuple<string, string>, int>> result, Stream stream = null)
        {
            if (stream == null)
            {
                stream = Console.OpenStandardOutput();
            }

            var list = result.Where(item => item.Value > 1).OrderByDescending(item => item.Value);
            using (var sw = new StreamWriter(stream))
            {
                foreach (var item in list)
                {
                    sw.WriteLine($"{item.Key}\t{item.Value}");
                }
            }
        }

        private static Tuple<string, string, int> GetUserInputs()
        {
            Console.Write("Please input the TSV file path (default: input.tsv):");
            var input = Console.ReadLine().Trim('"');
            if (string.IsNullOrWhiteSpace(input))
            {
                input = "input.tsv";
            }

            Console.Write("Please output file path (default: output.tsv):");
            var output = Console.ReadLine().Trim('"');
            if (string.IsNullOrWhiteSpace(output))
            {
                output = "input.tsv";
            }

            Console.Write("Please input the delta value (default: 3):");
            int k;
            if (!int.TryParse(Console.ReadLine(), out k))
            {
                k = 3;
            }

            return new Tuple<string, string, int>(input, output, k);
        }

        private static IDictionary<Tuple<string, string>, int> CountQueryPairCounts(string inputPath, int k)
        {
            var prevKQueries = new LimitedQueue<string>(k);
            var counts = new Dictionary<Tuple<string, string>, int>();

            using (var parser = new TextFieldParser(inputPath))
            {
                parser.SetDelimiters("\t");

                while (!parser.EndOfData)
                {
                    var curQuery = NormalizeQuery(parser.ReadFields().Single());

                    foreach (var prevQuery in prevKQueries)
                    {
                        var queryPair = new Tuple<string, string>(prevQuery, curQuery);
                        if (!counts.ContainsKey(queryPair))
                        {
                            counts[queryPair] = 1;
                        }
                        else
                        {
                            ++counts[queryPair];
                        }
                    }

                    prevKQueries.Enqueue(curQuery);
                }

                return counts;
            }
        }

        private static string NormalizeQuery(string rawQuery)
        {
            return rawQuery.ToLowerInvariant(); // TODO: Add more normalizations?
        }
    }
}
