namespace Box2dNet.Samples
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var samples = new ISample[]
            {
                new CircleShapeSample(),
                new PolygonShapeSample(),
                new MultiThreadedSample(),
                new HitEventsSample(),
                new TrackGameObjectWithUserDataSample(),
                new DebugDrawSample(),
            };

            while (true)
            {
                Console.WriteLine();
                Console.WriteLine("----------------------");
                Console.WriteLine();
                Console.WriteLine("Box2dNet: Box2D v3.x .NET wrapper samples");
                Console.WriteLine();
                var i = 1;
                foreach (var sample in samples)
                {
                    Console.WriteLine($"{i++}. {sample.GetType().Name}");
                }
                Console.WriteLine();
                Console.WriteLine("Enter number of sample to run and press enter ...");
                var input = Console.ReadLine();
                if (int.TryParse(input, out var sampleNumber))
                {
                    if (sampleNumber > 0 && sampleNumber <= samples.Length)
                    {
                        Console.Clear();
                        var sample = samples[sampleNumber - 1];
                        sample.Run();
                        Console.WriteLine();
                        Console.WriteLine($"Sample {sample.GetType().Name} done. Press enter to return to menu ...");
                        Console.ReadLine();
                    }
                    else
                    {
                        Console.WriteLine();
                        Console.WriteLine("ERROR: Invalid menu input. Press enter to try again ...");
                        Console.ReadLine();
                    }
                }
                else
                {
                    Console.WriteLine();
                    Console.WriteLine("ERROR: Invalid menu input. Press enter to try again ...");
                    Console.ReadLine();
                }
            }
        }
    }
}
