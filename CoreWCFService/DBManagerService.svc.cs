using System.Collections.Generic;

namespace CoreWCFService
{
    public class DBManagerService : IDBManagerService
    {
        public bool AddAlarm(Alarm alarm, string token)
        {
            if (UserProcessing.IsAuthenticatedUser(token))
                return TagProcessing.AddAlarm(alarm);
            return false;
        }

        public bool AddTag(Tag tag, string token)
        {
            if (UserProcessing.IsAuthenticatedUser(token))
                return TagProcessing.AddTag(tag);
            return false;
        }

        public void ChangeOnScanValue(string tagName, string token)
        {
            if (UserProcessing.IsAuthenticatedUser(token))
                TagProcessing.GetOnScanValue(tagName);
        }

        public Dictionary<string, double> GetCurrentOutputValues(string token)
        {
            if (UserProcessing.IsAuthenticatedUser(token))
                return TagProcessing.GetCurrentValues();
            return null;
        }

        public string GetOnScanValue(string tagName, string token)
        {
            if (UserProcessing.IsAuthenticatedUser(token))
                return TagProcessing.GetOnScanValue(tagName);
            return "";
        }

        public double GetOutputValue(string tagName, string token)
        {
            if (UserProcessing.IsAuthenticatedUser(token))
                return TagProcessing.GetOutputTagValue(tagName);
            return -1;
        }

        public string LogIn(string username, string password)
        {
            return UserProcessing.LogIn(username, password);
        }

        public void LogOut(string token)
        {
            UserProcessing.LogOut(token);
        }

        public bool RegisterUser(string username, string password, string token)
        {
            if (UserProcessing.IsAuthenticatedUser(token))
                return UserProcessing.RegisterUser(username, password);
            return false;
        }

        public bool RemoveTag(string tagName, string token)
        {
            if (UserProcessing.IsAuthenticatedUser(token))
                return TagProcessing.RemoveTag(tagName);
            return false;
        }

        public bool SetOutputTagValue(string tagName, double value, string token)
        {
            if (UserProcessing.IsAuthenticatedUser(token))
                return TagProcessing.SetOutputTagValue(tagName, value);
            return false;
        }
    }
}
