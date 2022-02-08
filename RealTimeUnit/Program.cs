using System;
using System.IO;
using System.Threading;
using System.Security.Cryptography;
using System.Text;
using RealTimeUnit.ServiceReference;

namespace RealTimeUnit
{
    class Program
    {
        private static readonly string INPUT_ERROR_MSG = "Unos nije validan, pokušajte ponovo.";
        private static CspParameters csp;
        private static RSACryptoServiceProvider rsa;
        const string EXPORT_FOLDER = @"C:\public_key\";
        const string PUBLIC_KEY_FILE = @"rsaPublicKey.txt";

        static void Main(string[] args)
        {
            RTUServiceClient proxy = new RTUServiceClient();
            string id;
            while (true)
            {
                id = EnterStringValue("ID");
                if (!proxy.IsIdTaken(id)) break;
                Console.WriteLine("ID je zauzet.");
            }
            double lowLimit = EnterLimit("Donja");
            double highLimit = EnterLimit("Gornja");
            string address;
            while (true)
            {
                address = EnterStringValue("Adresa");
                if (!proxy.IsAddressTaken(address)) break;
                Console.WriteLine("Adresa je zauzeta.");
            }

            Random rnd = new Random();
            double value = rnd.NextDouble() * (highLimit - lowLimit) + lowLimit;
            int seconds = EnterSeconds();
            while (true)
            {
                string message = $"id:{id},value:{value},address:{address}";
                CreateAsmKeys();
                byte[] signature = SignMessage(message);
                ExportPublicKey();
                bool success = proxy.SendMessage(message, signature);
                Console.WriteLine(success ? "Poruka uspešno poslata" : "Poruka nije poslata");
                Thread.Sleep(seconds * 1000);
            }
        }

        public static void CreateAsmKeys()
        {
            csp = new CspParameters();
            rsa = new RSACryptoServiceProvider(csp);
        }

        private static byte[] SignMessage(string message)
        {
            using (SHA256 sha = SHA256Managed.Create())
            {
                var hashValue = sha.ComputeHash(Encoding.UTF8.GetBytes(message));
                var formatter = new RSAPKCS1SignatureFormatter(rsa);
                formatter.SetHashAlgorithm("SHA256");
                return formatter.CreateSignature(hashValue);
            }
        }

        private static void ExportPublicKey()
        {
            //Kreiranje foldera za eksport ukoliko on ne postoji
            if (!(Directory.Exists(EXPORT_FOLDER)))
                Directory.CreateDirectory(EXPORT_FOLDER);
            string path = Path.Combine(EXPORT_FOLDER, PUBLIC_KEY_FILE);
            using (StreamWriter writer = new StreamWriter(path))
            {
                writer.Write(rsa.ToXmlString(false));
            }
        }

        private static string EnterStringValue(string name)
        {
            Console.Write($"{name} >> ");
            return Console.ReadLine();
        }

        private static double EnterLimit(string type)
        {
            while (true)
            {
                Console.Write($"{type} granica >> ");
                string input = Console.ReadLine();
                if (double.TryParse(input, out double limit))
                {
                    return limit;
                }
                Console.WriteLine(INPUT_ERROR_MSG);
            }
        }

        private static int EnterSeconds()
        {
            while (true)
            {
                Console.Write("Vremenski perion slanja poruke >> ");
                string secondsStr = Console.ReadLine();
                if (int.TryParse(secondsStr, out int seconds))
                {
                    return seconds;
                }
                Console.WriteLine(INPUT_ERROR_MSG);
            }
        }
    }
}
