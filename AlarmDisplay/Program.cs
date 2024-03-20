using AlarmDisplay.ServiceReference1;
using System;
using System.ServiceModel;

namespace AlarmDisplay
{
    public class AlarmDisplayCallback : IAlarmDisplayCallback
    {
        public void OnAlarmInvoked(string alarm, int prioritet)
        {
            for(int i = 0; i< prioritet; i++)
            {
                Console.WriteLine(alarm);
            }
            Console.WriteLine("----------------------------------------------------");
        }
    }
    class Program
    {
        static InstanceContext ic = new InstanceContext(new AlarmDisplayCallback());
        static AlarmDisplayClient alarmClient = new AlarmDisplayClient(ic);
        static void Main(string[] args)
        {
            Console.WriteLine("Start");
            alarmClient.initialize();
            Console.ReadKey();
        }
    }
}
