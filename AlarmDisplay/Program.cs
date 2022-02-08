using System;
using AlarmDisplay.ServiceReference;
using System.ServiceModel;

namespace AlarmDisplay
{
    class AlarmDisplayCallback : IAlarmDisplayServiceCallback
    {
        private readonly object consoleLock = new object();

        public void AlarmTriggered(ActivatedAlarm alarm, double value)
        {
            lock (consoleLock)
            {
                for (int i = 0; i < alarm.Alarm.Priority; i++)
                {
                    Console.WriteLine($"Alarm for: {alarm.Alarm.TagName}\tThreshold: {alarm.Alarm.Threshold}\t" +
                        $"Priority: {alarm.Alarm.Priority}\t Activated at: {alarm.ActivatedAt}\t For: {value}");
                }
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
