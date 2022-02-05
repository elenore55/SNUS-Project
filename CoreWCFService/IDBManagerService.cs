using System.Collections.Generic;
using System.ServiceModel;

namespace CoreWCFService
{
    [ServiceContract]
    public interface IDBManagerService
    {
        [OperationContract]
        string LogIn(string username, string password);

        [OperationContract]
        void LogOut(string token);

        [OperationContract]
        bool RegisterUser(string username, string password, string token);

        [OperationContract]
        double GetOutputValue(string tagName, string token);

        [OperationContract]
        bool AddTag(Tag tag, string token);

        [OperationContract]
        bool RemoveTag(string tagName, string token);

        [OperationContract]
        void ChangeOnScanValue(string tagName, string token);

        [OperationContract]
        string GetOnScanValue(string tagName, string token);

        [OperationContract]
        bool AddAlarm(Alarm alarm, string token);

        [OperationContract]
        Dictionary<string, double> GetCurrentOutputValues(string token);

        [OperationContract]
        bool SetOutputTagValue(string tagName, double value, string token);
    }
}
