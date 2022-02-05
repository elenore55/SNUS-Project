using System.Collections.Generic;

namespace CoreWCFService
{
    public class DBManagerService : IDBManagerService
    {
        UserProcessing userProcessing = new UserProcessing();
        TagProcessing tagProcessing = new TagProcessing();

        public bool AddAlarm(Alarm alarm)
        {
            return tagProcessing.AddAlarm(alarm);
        }

        public bool AddTag(Tag tag)
        {
            return tagProcessing.AddTag(tag);
        }

        public void ChangeOnScanValue(string tagName)
        {
            tagProcessing.GetOnScanValue(tagName);
        }

        public Dictionary<string, double> GetCurrentOutputValues()
        {
            return tagProcessing.GetCurrentValues();
        }

        public string GetOnScanValue(string tagName)
        {
            return tagProcessing.GetOnScanValue(tagName);
        }

        public double GetOutputValue(string tagName)
        {
            return tagProcessing.GetOutputTagValue(tagName);
        }

        public string LogIn(string username, string password)
        {
            return userProcessing.LogIn(username, password);
        }

        public void LogOut()
        {
            userProcessing.LogOut();
        }

        public bool RegisterUser(string username, string password)
        {
            return userProcessing.RegisterUser(username, password);
        }

        public bool RemoveTag(string tagName)
        {
            return tagProcessing.RemoveTag(tagName);
        }

        public bool SetOutputTagValue(string tagName, double value)
        {
            return tagProcessing.SetOutputTagValue(tagName, value);
        }

        
    }
}
