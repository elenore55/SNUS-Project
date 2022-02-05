using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace CoreWCFService
{
    [ServiceContract (CallbackContract = typeof(IAlarmDisplayCallback))]
    public interface IAlarmDisplayService
    {
        [OperationContract (IsOneWay = true)]
        void InitializeAlarmDisplay();
    }

    public interface IAlarmDisplayCallback
    {
        [OperationContract (IsOneWay = true)]
        void AlarmTriggered(Alarm alarm);
    }
}
