using System;
using Trending.ServiceReference;
using System.ServiceModel;

namespace Trending
{
    public class TrendingCallback : ITrendingServiceCallback
    {
        // mozda vrijeme dodati
        public void TagValueChanged(InputTag tag, double value)
        {
            Console.WriteLine(tag is AI ? "Analog input" : "Digital input");
            Console.WriteLine($"Tag name: {tag.TagName}");
            Console.WriteLine($"Description: {tag.Description}");
            Console.WriteLine(tag.Driver is SimulationDriver ? "Simulation driver" : "Real-Time driver");
            Console.WriteLine($"Scan time: {tag.ScanTime}");
            Console.WriteLine(tag.OnScan ? "Scan On" : "Scan Off");
            Console.WriteLine($"Current value: {value}");
            Console.WriteLine();
        }
    }


    class Program
    {
        static TrendingServiceClient proxy;

        static void Main(string[] args)
        {
            proxy = new TrendingServiceClient(new InstanceContext(new TrendingCallback()));
            proxy.InitializeTrending();
            Console.ReadKey();
        }
    }
}
