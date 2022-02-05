using System.ServiceModel;

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
