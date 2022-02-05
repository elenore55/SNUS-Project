using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.ServiceModel;
using System.Text;
using System.Web;

namespace CoreWCFService
{
    public class UserProcessing
    {
        private static Dictionary<string, User> authenticatedUsers = new Dictionary<string, User>();

        public string LogIn(string username, string password)
        {
            using (var db = new UserContext())
            {
                foreach (var user in db.Users)
                {
                    if (username == user.Username &&
                    ValidateEncryptedData(password, user.Password))
                    {
                        string token = GenerateToken(username);
                        authenticatedUsers.Add(token, user);
                        return token;
                    }
                }
            }
            return "Login failed";
        }

        public void LogOut()
        {

        }

        public bool RegisterUser(string username, string password)
        {
            string encryptedPassword = EncryptValue(password);
            User user = new User()
            {
                Username = username,
                Password = encryptedPassword
            };
            using (var db = new UserContext())
            {
                try
                {
                    db.Users.Add(user);
                    db.SaveChanges();
                }
                catch (Exception)
                {
                    return false;
                }
            }
            return true;
        }

        private string EncryptValue(string value)
        {
            string saltValue = GenerateSalt();
            byte[] saltedPassword = Encoding.UTF8.GetBytes(saltValue + value);
            using (SHA256Managed sha = new SHA256Managed())
            {
                byte[] hash = sha.ComputeHash(saltedPassword);
                return $"{Convert.ToBase64String(hash)}:{saltValue}";
            }
        }

        private string GenerateSalt()
        {
            RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider();
            byte[] salt = new byte[32];
            crypto.GetBytes(salt);
            return Convert.ToBase64String(salt);
        }

        private bool ValidateEncryptedData(string valueToValidate, string valueFromDatabase)
        {
            string[] arrValues = valueFromDatabase.Split(':');
            string encryptedDbValue = arrValues[0];
            string salt = arrValues[1];
            byte[] saltedValue = Encoding.UTF8.GetBytes(salt + valueToValidate);
            using (var sha = new SHA256Managed())
            {
                byte[] hash = sha.ComputeHash(saltedValue);
                string enteredValueToValidate = Convert.ToBase64String(hash);
                return encryptedDbValue.Equals(enteredValueToValidate);
            }
        }

        private string GenerateToken(string username)
        {
            RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider();
            byte[] randVal = new byte[32];
            crypto.GetBytes(randVal);
            string randStr = Convert.ToBase64String(randVal);
            return username + randStr;
        }
    }
}