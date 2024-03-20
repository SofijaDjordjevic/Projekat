using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Trending.ServiceReference1;

namespace Trending
{
    public class TrendingCallback : ITrendingCallback
    {
        public void OnInputTagChange(string input, double value)
        {
            Console.WriteLine("-------------------------------------------");
            Console.WriteLine($"Tag name: {input}, Value: {value}");
        }
    }
    class Program
    {
        static InstanceContext ic = new InstanceContext(new TrendingCallback());
        static TrendingClient client = new TrendingClient(ic);
        static void Main(string[] args)
        {
            Console.WriteLine("Start");
            client.initialize();
            Console.ReadKey();
        }
    }
}
