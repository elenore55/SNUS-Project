using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DatabaseManager.ServiceReference;

namespace DatabaseManager
{
    class Program
    {
        private static string[] menuItems = { "Dodavanje taga", "Uklanjanje taga", "Registracije korisnika", "Upisivanje vrednosti", 
                                              "Prikaz trenutnih vrednosti", "Podešavanje skeniranja ulaznih tagova", "Dodavanje alarma", "Izloguj se" };
        private static readonly string INPUT_ERROR_MSG = "Unos nije validan, pokušajte ponovo.";

        static void Main(string[] args)
        {
            DBManagerServiceClient proxy = new DBManagerServiceClient();

            bool loggedIn = false;
            while (!loggedIn)
            {
                Console.Write("Korisnicko ime >> ");
                string username = Console.ReadLine();
                Console.Write("Lozinka >> ");
                string password = Console.ReadLine();
                string token = proxy.LogIn(username, password);
                if (token != "Login failed")
                    loggedIn = true;
                else
                    Console.WriteLine(token);
            }
            

            while (loggedIn)
            {
                for (int i = 0; i < menuItems.Length; i++)
                {
                    Console.WriteLine($"{i + 1}. {menuItems[i]}");
                }
                Console.Write(">> ");
                string input = Console.ReadLine().Trim();
                while (!ValidateInput(input))
                {
                    Console.WriteLine(INPUT_ERROR_MSG);
                    Console.Write(">> ");
                    input = Console.ReadLine().Trim();
                }
                if (input == menuItems.Length.ToString()) 
                    break;
                switch (input)
                {
                    case "1":
                        AddTag(proxy);
                        break;
                    case "2":
                        RemoveTag(proxy);
                        break;
                    case "3":
                        RegisterUser(proxy);
                        break;
                    case "4":
                        EnterOutputTagValue(proxy);
                        break;
                    case "5":
                        DisplayCurrentOutputTagValues(proxy);
                        break;
                    case "6":
                        SetupScanProperty(proxy);
                        break;
                    case "7":
                        AddAlarm(proxy);
                        break;
                    case "8":
                        proxy.LogOut();
                        loggedIn = false;
                        break;
                }
            }

            Console.ReadKey();
        }

        private static void AddTag(DBManagerServiceClient proxy)
        {
            string[] tagTypes = { "Analog input", "Analog output", "Digital input", "Digital output" };
            Console.WriteLine("Tip taga: ");
            for (int i = 0; i < tagTypes.Length; i++)
            {
                Console.WriteLine($"{i + 1}. {tagTypes[i]}");
            }
            Console.WriteLine(">> ");
            string type = Console.ReadLine().Trim();
            switch(type)
            {
                case "1":
                    AddAITag(proxy);
                    break;
                case "2":
                    AddAOTag(proxy);
                    break;
                case "3":
                    AddDITag(proxy);
                    break;
                case "4":
                    AddDOTag(proxy);
                    break;
                default:
                    Console.WriteLine(INPUT_ERROR_MSG);
                    break;
            }
        }

        private static void RemoveTag(DBManagerServiceClient proxy)
        {
            Console.Write("Naziv taga za brisanje >> ");
            string tagName = Console.ReadLine();
            bool success = proxy.RemoveTag(tagName);
            Console.WriteLine((success) ? "Tag je uspesno obrisan" : "Greska pri brisanju taga");
        }

        private static void EnterOutputTagValue(DBManagerServiceClient proxy)
        {
            Console.Write("Naziv izlaznog taga >> ");
            string tagName = Console.ReadLine();
            double currentValue = proxy.GetOutputValue(tagName);
            if (currentValue == -1)
            {
                Console.WriteLine("Izlazni tag sa unesenim nazivom ne postoji");
                return;
            }
            Console.WriteLine($"Trenutna vrednost taga {tagName} : {currentValue}");
            while (true)
            {
                Console.Write("Nova vrednost >> ");
                string input = Console.ReadLine();
                if (double.TryParse(input, out double value))
                {
                    bool success = proxy.SetOutputTagValue(tagName, value);
                    Console.WriteLine(success ? "Vrednost je uspešno postavljena" : "Vrednost nije postavljena");
                    return;
                }
                Console.WriteLine(INPUT_ERROR_MSG);
            }
        }

        private static void DisplayCurrentOutputTagValues(DBManagerServiceClient proxy)
        {
            foreach (KeyValuePair<string, double> entry in proxy.GetCurrentOutputValues())
            {
                Console.WriteLine($"{entry.Key} -> {entry.Value}");
            }
        }

        private static void SetupScanProperty(DBManagerServiceClient proxy)
        {
            Console.Write("Naziv ulaznog taga >> ");
            string tagName = Console.ReadLine();
            string currentOnScanValue = proxy.GetOnScanValue(tagName);
            if (currentOnScanValue != "On" && currentOnScanValue != "Off")
            {
                Console.WriteLine("Ne postoji ulazni tag sa zadatim imenom.");
                return;
            }
            Console.WriteLine($"Scan je trenutno: {currentOnScanValue}");
            Console.WriteLine("Da li želite da promenite vrednost?");
            while (true)
            {
                Console.Write("(y/n) >> ");
                string input = Console.ReadLine().Trim().ToLower();
                if (input != "y" && input != "no")
                {
                    Console.WriteLine(INPUT_ERROR_MSG);
                    continue;
                }
                if (input == "y")
                {
                    proxy.ChangeOnScanValue(tagName);
                }
                return;
            }
        }

        private static void AddAITag(DBManagerServiceClient proxy)
        {
            AI tag = new AI();
            EnterGeneralTagProperties(tag);
            EnterInputTagProperties(tag);
            tag.LowLimit = EnterLimitProperty("Low");
            tag.HighLimit = EnterLimitProperty("High");
            tag.Units = EnterUnitsProperty();
            bool success = proxy.AddTag(tag);
            Console.WriteLine((success) ? "Tag je uspesno dodat" : "Greska pri dodavanju taga");
        }

        private static void AddAOTag(DBManagerServiceClient proxy)
        {
            AO tag = new AO();
            EnterGeneralTagProperties(tag);
            EnterOutputTagProperties(tag);
            tag.LowLimit = EnterLimitProperty("Low");
            tag.HighLimit = EnterLimitProperty("High");
            tag.Units = EnterUnitsProperty();
            bool success = proxy.AddTag(tag);
            Console.WriteLine((success) ? "Tag je uspesno dodat" : "Greska pri dodavanju taga");
        }

        private static void AddDITag(DBManagerServiceClient proxy)
        {
            DI tag = new DI();
            EnterGeneralTagProperties(tag);
            EnterInputTagProperties(tag);
            bool success = proxy.AddTag(tag);
            Console.WriteLine((success) ? "Tag je uspesno dodat" : "Greska pri dodavanju taga");
        }

        private static void AddDOTag(DBManagerServiceClient proxy)
        {
            DO tag = new DO();
            EnterGeneralTagProperties(tag);
            EnterOutputTagProperties(tag);
            bool success = proxy.AddTag(tag);
            Console.WriteLine((success) ? "Tag je uspesno dodat" : "Greska pri dodavanju taga");
        }

        private static void EnterGeneralTagProperties(Tag tag)
        {
            Console.Write("Tag name >> ");
            tag.TagName = Console.ReadLine();
            Console.Write("Description >> ");
            tag.Description = Console.ReadLine();
            Console.Write("I/O address >> ");
            tag.IOAddress = Console.ReadLine();
        }

        private static void EnterInputTagProperties(InputTag tag)
        {
            EnterDriverProperty(tag);
            EnterScanTimeProperty(tag);
            EnterOnScanProperty(tag);
        }

        private static void EnterOutputTagProperties(OutputTag tag)
        {
            while (true)
            {
                Console.Write("Initial value >> ");
                string input = Console.ReadLine();
                if (double.TryParse(input, out double initialValue))
                {
                    if (initialValue >= 0)
                    {
                        tag.InitialValue = initialValue;
                        return;
                    }
                }
                Console.WriteLine(INPUT_ERROR_MSG);
            }
        }

        private static void EnterDriverProperty(InputTag tag)
        {
            Console.WriteLine("Driver: ");
            Console.WriteLine("1. Simulation Driver");
            Console.WriteLine("2. Real-Time Driver");
            while (true)
            {
                Console.Write(">> ");
                string input = Console.ReadLine().Trim();
                if (input == "1")
                {
                    tag.Driver = new SimulationDriver();
                    break;
                } else if (input == "2")
                {
                    tag.Driver = new RealTimeDriver();
                    break;
                }
                Console.WriteLine(INPUT_ERROR_MSG);
            }
        }

        private static void EnterScanTimeProperty(InputTag tag)
        {
            while (true)
            {
                Console.Write("Scan time >> ");
                string input = Console.ReadLine();
                if (int.TryParse(input, out int scanTime))
                {
                    if (scanTime > 0)
                    {
                        tag.ScanTime = scanTime;
                        return;
                    }
                }
                Console.WriteLine(INPUT_ERROR_MSG);
            }
        }

        private static void EnterOnScanProperty(InputTag tag)
        {
            Console.WriteLine("On/Off scan: ");
            Console.WriteLine("1. On scan");
            Console.WriteLine("2. Off scan");
            while (true)
            {
                Console.Write(">> ");
                string input = Console.ReadLine().Trim();
                if (input == "1")
                {
                    tag.OnScan = true;
                    break;
                }
                else if (input == "2")
                {
                    tag.OnScan = false;
                    break;
                }
                Console.WriteLine(INPUT_ERROR_MSG);
            }
        }

        private static double EnterLimitProperty(string limitType)
        {
            while (true)
            {
                Console.Write($"{limitType} limit >> ");
                string input = Console.ReadLine();
                if (double.TryParse(input, out double limit))
                {
                    if (limit >= 0)
                    {
                        return limit;
                    }
                }
                Console.WriteLine(INPUT_ERROR_MSG);
            }
        }
        
        private static string EnterUnitsProperty()
        {
            Console.Write("Units >> ");
            return Console.ReadLine();
        }

        private static void AddAlarm(DBManagerServiceClient proxy)
        {
            Console.Write("Tag name >> ");
            string tagName = Console.ReadLine();
            Alarm alarm = new Alarm() 
            {
                Type = EnterAlarmType(),
                Priority = EnterAlarmPriority(),
                Threshold = EnterAlarmThreshold(),
                TagName = tagName
            };
            proxy.AddAlarm(alarm);
        }

        private static AlarmType EnterAlarmType()
        {
            while (true)
            {
                Console.Write("Type (low/high) >> ");
                string input = Console.ReadLine().Trim().ToLower();
                if (input == "low")
                {
                    return AlarmType.low;
                }
                if (input == "high")
                {
                    return AlarmType.high;
                }
                Console.WriteLine(INPUT_ERROR_MSG);
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

        private static double EnterAlarmThreshold()
        {
            while (true)
            {
                Console.Write("Threshold >> ");
                string input = Console.ReadLine();
                if (double.TryParse(input, out double threshold))
                {
                    return threshold;
                }
                Console.WriteLine(INPUT_ERROR_MSG);
            }
        }

        private static void RegisterUser(DBManagerServiceClient proxy)
        {
            Console.Write("Korisnicko ime >> ");
            string username = Console.ReadLine();
            Console.Write("Lozinka >> ");
            string password = Console.ReadLine();
            bool success = proxy.RegisterUser(username, password);
            Console.WriteLine(success);
        }

        private static bool ValidateInput(string input)
        {
            try
            {
                int num = int.Parse(input);
                if (num > 0 && num <= menuItems.Length)
                    return true;
                return false;
            } 
            catch
            {
                return false;
            }
        }
    }
}
