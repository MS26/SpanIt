using System;
using System.Collections.Generic;
using System.Diagnostics;
using SpanIt.Processors;

namespace SpanIt
{
    public class Program
    {
        public static void Main(string[] args)
        {
            const string words = "The quick brown-fox jumps over the lazy dog The quick brown-fox jumps over the lazy dog The quick brown-fox jumps over the lazy dog The quick brown-fox jumps over the lazy dog The quick brown-fox jumps over the lazy dog";
            const int iterations = 1_000_000;
            
            AppDomain.MonitoringIsEnabled = true;

            var dict = new Dictionary<string, IWordProcessor>
            {
                ["1"] = new ProcessorV1(),
                ["2"] = new ProcessorV2(),
                ["3"] = new ProcessorV3(),
            };

            if (args.Length > 0 && dict.ContainsKey(args[0]))
            {
                var processor = dict[args[0]];

                for (var i = 0; i < iterations; i++)
                {
                    processor.Add(words);
                }

                Console.WriteLine(processor.GetType());
            }
            else
            {
                Console.WriteLine(string.Join(Environment.NewLine, dict.Keys));
                Environment.Exit(1);
            }


            Console.WriteLine($"Iterations: {iterations}");
            Console.WriteLine($"Took: {AppDomain.CurrentDomain.MonitoringTotalProcessorTime.TotalMilliseconds:#,###} ms");
            Console.WriteLine($"Allocated: {AppDomain.CurrentDomain.MonitoringTotalAllocatedMemorySize / 1024:#,#} kb");
            Console.WriteLine($"Peak Working Set: {Process.GetCurrentProcess().PeakWorkingSet64 / 1024:#,#} kb");

            for (var index = 0; index <= GC.MaxGeneration; index++)
            {
                Console.WriteLine($"Gen {index} collections: {GC.CollectionCount(index)}");
            }

            Console.WriteLine(Environment.NewLine);
        }
    }

    public interface IWordProcessor
    {
        void Add(string words);
    }
}
