using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

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
