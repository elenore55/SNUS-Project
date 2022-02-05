using System;
using System.Collections.Generic;
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
            string token = null;
            while (!loggedIn)
            {
                Console.Write("Korisničko ime >> ");
                string username = Console.ReadLine();
                Console.Write("Lozinka >> ");
                string password = Console.ReadLine();
                token = proxy.LogIn(username, password);
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
                        AddTag(proxy, token);
                        break;
                    case "2":
                        RemoveTag(proxy, token);
                        break;
                    case "3":
                        RegisterUser(proxy, token);
                        break;
                    case "4":
                        EnterOutputTagValue(proxy, token);
                        break;
                    case "5":
                        DisplayCurrentOutputTagValues(proxy, token);
                        break;
                    case "6":
                        SetupScanProperty(proxy, token);
                        break;
                    case "7":
                        AddAlarm(proxy, token);
                        break;
                    case "8":
                        proxy.LogOut(token);
                        loggedIn = false;
                        break;
                }
            }

            Console.ReadKey();
        }

        private static void AddTag(DBManagerServiceClient proxy, string token)
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
                    AddAITag(proxy, token);
                    break;
                case "2":
                    AddAOTag(proxy, token);
                    break;
                case "3":
                    AddDITag(proxy, token);
                    break;
                case "4":
                    AddDOTag(proxy, token);
                    AddDOTag(proxy, token);
                    break;
                default:
                    Console.WriteLine(INPUT_ERROR_MSG);
                    break;
            }
        }

        private static void RemoveTag(DBManagerServiceClient proxy, string token)
        {
            Console.Write("Naziv taga za brisanje >> ");
            string tagName = Console.ReadLine();
            bool success = proxy.RemoveTag(tagName, token);
            Console.WriteLine((success) ? "Tag je uspešno obrisan" : "Greška pri brisanju taga");
        }

        private static void EnterOutputTagValue(DBManagerServiceClient proxy, string token)
        {
            Console.Write("Naziv izlaznog taga >> ");
            string tagName = Console.ReadLine();
            double currentValue = proxy.GetOutputValue(tagName, token);
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
                    bool success = proxy.SetOutputTagValue(tagName, value, token);
                    Console.WriteLine(success ? "Vrednost je uspešno postavljena" : "Vrednost nije postavljena");
                    return;
                }
                Console.WriteLine(INPUT_ERROR_MSG);
            }
        }

        private static void DisplayCurrentOutputTagValues(DBManagerServiceClient proxy, string token)
        {
            foreach (KeyValuePair<string, double> entry in proxy.GetCurrentOutputValues(token))
            {
                Console.WriteLine($"{entry.Key} -> {entry.Value}");
            }
        }

        private static void SetupScanProperty(DBManagerServiceClient proxy, string token)
        {
            Console.Write("Naziv ulaznog taga >> ");
            string tagName = Console.ReadLine();
            string currentOnScanValue = proxy.GetOnScanValue(tagName, token);
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
                    proxy.ChangeOnScanValue(tagName, token);
                }
                return;
            }
        }

        private static void AddAITag(DBManagerServiceClient proxy, string token)
        {
            AI tag = new AI();
            EnterGeneralTagProperties(tag);
            EnterInputTagProperties(tag);
            tag.LowLimit = EnterLimitProperty("Low");
            tag.HighLimit = EnterLimitProperty("High");
            tag.Units = EnterUnitsProperty();
            bool success = proxy.AddTag(tag, token);
            Console.WriteLine((success) ? "Tag je uspešno dodat" : "Greška pri dodavanju taga");
        }

        private static void AddAOTag(DBManagerServiceClient proxy, string token)
        {
            AO tag = new AO();
            EnterGeneralTagProperties(tag);
            EnterOutputTagProperties(tag);
            tag.LowLimit = EnterLimitProperty("Low");
            tag.HighLimit = EnterLimitProperty("High");
            tag.Units = EnterUnitsProperty();
            bool success = proxy.AddTag(tag, token);
            Console.WriteLine((success) ? "Tag je uspešno dodat" : "Greška pri dodavanju taga");
        }

        private static void AddDITag(DBManagerServiceClient proxy, string token)
        {
            DI tag = new DI();
            EnterGeneralTagProperties(tag);
            EnterInputTagProperties(tag);
            bool success = proxy.AddTag(tag, token);
            Console.WriteLine((success) ? "Tag je uspešno dodat" : "Greška pri dodavanju taga");
        }

        private static void AddDOTag(DBManagerServiceClient proxy, string token)
        {
            DO tag = new DO();
            EnterGeneralTagProperties(tag);
            EnterOutputTagProperties(tag);
            bool success = proxy.AddTag(tag, token);
            Console.WriteLine((success) ? "Tag je uspešno dodat" : "Greška pri dodavanju taga");
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

        private static void AddAlarm(DBManagerServiceClient proxy, string token)
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
            proxy.AddAlarm(alarm, token);
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

        private static void RegisterUser(DBManagerServiceClient proxy, string token)
        {
            Console.Write("Korisničko ime >> ");
            string username = Console.ReadLine();
            Console.Write("Lozinka >> ");
            string password = Console.ReadLine();
            bool success = proxy.RegisterUser(username, password, token);
            Console.WriteLine(success ? "Korisnik je uspešno registrovan" : "Korisnik sa zadatim korisničkim imenom već postoji");
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
