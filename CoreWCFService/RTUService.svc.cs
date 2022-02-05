using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace CoreWCFService
{
    public class RTUService : IRTUService
    {
        private static CspParameters csp;
        private static RSACryptoServiceProvider rsa;
        const string IMPORT_FOLDER = @"C:\public_key\";
        const string PUBLIC_KEY_FILE = @"rsaPublicKey.txt";

        public bool SendMessage(string message, byte[] signature)
        {
            ImportPublicKey();
            if (VerifySignedMessage(message, signature))
            {
                RealTimeDriver.MessageArrived(message);
                return true;
            }
            return false;
        }

        private static void ImportPublicKey()
        {
            string path = Path.Combine(IMPORT_FOLDER, PUBLIC_KEY_FILE);
            //Provera da li fajl sa javnim ključem postoji na prosleđenoj lokaciji
            FileInfo fi = new FileInfo(path);
            if (fi.Exists)
            {
                using (StreamReader reader = new StreamReader(path))
                {
                    csp = new CspParameters();
                    rsa = new RSACryptoServiceProvider(csp);
                    string publicKeyText = reader.ReadToEnd();
                    rsa.FromXmlString(publicKeyText);
                }
            }
        }

        private static bool VerifySignedMessage(string message, byte[] signature)
        {
            using (SHA256 sha = SHA256Managed.Create())
            {
                var hashValue = sha.ComputeHash(Encoding.UTF8.GetBytes(message));
                var deformatter = new RSAPKCS1SignatureDeformatter(rsa);
                deformatter.SetHashAlgorithm("SHA256");
                return deformatter.VerifySignature(hashValue, signature);
            }
        }
    }
}
