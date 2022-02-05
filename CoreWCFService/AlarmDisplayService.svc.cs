using System.ServiceModel;

namespace CoreWCFService
{
    public class AlarmDisplayService : IAlarmDisplayService
    {
        public void InitializeAlarmDisplay()
        {
            TagProcessing.OnAlarmTriggered += OperationContext.Current.GetCallbackChannel<IAlarmDisplayCallback>().AlarmTriggered;
        }
    }
}
