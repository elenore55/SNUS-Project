using System.ServiceModel;

namespace CoreWCFService
{
    [ServiceContract]
    public interface IRTUService
    {
        [OperationContract]
        bool SendMessage(string message, byte[] signature);

        [OperationContract]
        bool IsAddressTaken(string address);

        [OperationContract]
        bool IsIdTaken(string id);
    }
}
