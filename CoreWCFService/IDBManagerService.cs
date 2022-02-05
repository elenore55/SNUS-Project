using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace CoreWCFService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IDBManagerService" in both code and config file together.
    [ServiceContract]
    public interface IDBManagerService
    {
        [OperationContract]
        string LogIn(string username, string password);

        [OperationContract]
        void LogOut();

        [OperationContract]
        bool RegisterUser(string username, string password);

        [OperationContract]
        double GetOutputValue(string tagName);

        [OperationContract]
        bool AddTag(Tag tag);

        [OperationContract]
        bool RemoveTag(string tagName);

        [OperationContract]
        void ChangeOnScanValue(string tagName);

        [OperationContract]
        string GetOnScanValue(string tagName);

        [OperationContract]
        bool AddAlarm(Alarm alarm);

        [OperationContract]
        Dictionary<string, double> GetCurrentOutputValues();

        [OperationContract]
        bool SetOutputTagValue(string tagName, double value);
    }
}
