using System.Collections.Generic;
using System.ServiceModel;

namespace CoreWCFService
{
    [ServiceContract(SessionMode = SessionMode.Required)]
    public interface IDBManagerService
    {
        [OperationContract(IsInitiating = true)]
        string LogIn(string username, string password);

        [OperationContract(IsInitiating = false, IsTerminating = true)]
        void LogOut(string token);

        [OperationContract(IsInitiating = false)]
        bool RegisterUser(string username, string password, string token);

        [OperationContract(IsInitiating = false)]
        double GetOutputValue(string tagName, string token);

        [OperationContract(IsInitiating = false)]
        bool AddTag(Tag tag, string token);

        [OperationContract(IsInitiating = false)]
        bool RemoveTag(string tagName, string token);

        [OperationContract(IsInitiating = false)]
        void ChangeOnScanValue(string tagName, string token);

        [OperationContract(IsInitiating = false)]
        string GetOnScanValue(string tagName, string token);

        [OperationContract(IsInitiating = false)]
        bool AddAlarm(Alarm alarm, string token);

        [OperationContract(IsInitiating = false)]
        Dictionary<string, double> GetCurrentOutputValues(string token);

        [OperationContract(IsInitiating = false)]
        bool SetOutputTagValue(string tagName, double value, string token);
    }
}
