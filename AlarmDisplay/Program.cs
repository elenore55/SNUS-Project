using System;
using AlarmDisplay.ServiceReference;
using System.ServiceModel;

namespace AlarmDisplay
{
    class AlarmDisplayCallback : IAlarmDisplayServiceCallback
    {
        public void AlarmTriggered(Alarm alarm)
        {
            for (int i = 0; i < alarm.Priority; i++)
            {
                Console.WriteLine($"Alarm for: {alarm.TagName}\tThreshold: {alarm.Threshold}\tPriority: {alarm.Priority}");
            }
        }
    }


    class Program
    {
        static AlarmDisplayServiceClient proxy;

        static void Main(string[] args)
        {
            proxy = new AlarmDisplayServiceClient(new InstanceContext(new AlarmDisplayCallback()));
            proxy.InitializeAlarmDisplay();
            Console.ReadKey();
        }
    }
}
