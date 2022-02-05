using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ReportManager.ServiceReference;

namespace ReportManager
{
    class Program
    {
        static ReportManagerServiceClient proxy;
        static readonly string[] options = { "Alarmi u određenom periodu", "Alarmi određenog prioriteta", "Vrednosti tagova u određenom periodu", 
                                             "Poslednja vrednost AI tagova", "Poslednja vrednost DI tagova", "Sve vrednosti određenog taga" };
        static readonly string INPUT_ERROR_MSG = "Unos nije valida, pokušajte ponovo.";

        static void Main(string[] args)
        {
            proxy = new ReportManagerServiceClient();
            while (true)
            {
                int chosenOption = GetMenuOption();
                switch (chosenOption)
                {
                    case 1:
                        DisplayAlarms(proxy.GetAlarmsWithinPeriod(EnterDateTime("početni"), EnterDateTime("krajnji")).ToList());
                        break;
                    case 2:
                        DisplayAlarms(proxy.GetAlarmsOfPriority(EnterAlarmPriority()).ToList());
                        break;
                    case 3:
                        DisplayTagValues(proxy.GetTagValuesWithinPeriod(EnterDateTime("početni"), EnterDateTime("krajnji")).ToList());
                        break;
                    case 4:
                        DisplayTagValues(proxy.GetLastValuesOfTags("AI").ToList());
                        break;
                    case 5:
                        DisplayTagValues(proxy.GetLastValuesOfTags("DI").ToList());
                        break;
                    case 6:
                        Console.Write("Naziv taga >> ");
                        string tagName = Console.ReadLine();
                        DisplayTagValues(proxy.GetTagValues(tagName).ToList());  // max quota exceeded
                        break;
                }
            }
        }

        static int GetMenuOption()
        {
            for (int i = 0; i < options.Length; i++)
            {
                Console.WriteLine($"{i + 1}. {options[i]}");
            } 
            while (true)
            {
                Console.Write(">> ");
                string input = Console.ReadLine();
                if (int.TryParse(input, out int choice))
                {
                    if (choice > 0 && choice <= options.Length)
                    {
                        return choice;
                    }
                }
                Console.WriteLine(INPUT_ERROR_MSG);
            }
        }

        private static DateTime EnterDateTime(string type)
        {
            var cultureInfo = new CultureInfo("sr-Latn-CS");
            while (true)
            {
                Console.Write($"Unesite {type} datum i vreme (dd/MM/yyyy HH:mm) ");
                string input = Console.ReadLine();
                try
                {
                    return DateTime.Parse(input, cultureInfo);
                } 
                catch (Exception)
                {
                    Console.WriteLine(INPUT_ERROR_MSG);
                }
            }
        }

        private static int EnterAlarmPriority()
        {
            while (true)
            {
                Console.Write("Priority (1/2/3) >> ");
                string input = Console.ReadLine().Trim();
                if (input == "1" || input == "2" || input == "3")
                {
                    return int.Parse(input);
                }
                Console.WriteLine(INPUT_ERROR_MSG);
            }
        }

        private static void DisplayAlarms(List<ActivatedAlarm> alarms)
        {
            foreach (ActivatedAlarm alarm in alarms)
            {
                Console.WriteLine($"Alarm for {alarm.Alarm.TagName}\t Type: {alarm.Alarm.Type}\t " +
                    $"Priority: {alarm.Alarm.Priority}\t Threshold: {alarm.Alarm.Threshold}\t Activated at: {alarm.ActivatedAt}");
            }
        }

        private static void DisplayTagValues(List<TagValue> values)
        {
            foreach (TagValue val in values)
            {
                Console.WriteLine($"{val.TagName}\t {val.TagType}\t Value: {Math.Round(val.Value, 2)}\t Arrived at: {val.ArrivedAt}");
            }
        }
    }
}
