using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace CoreWCFService
{
    [ServiceContract]
    public interface IReportManagerService
    {
        [OperationContract]
        List<ActivatedAlarm> GetAlarmsWithinPeriod(DateTime start, DateTime end);

        [OperationContract]
        List<ActivatedAlarm> GetAlarmsOfPriority(int priority);

        [OperationContract]
        List<TagValue> GetTagValues(string tagName);

        [OperationContract]
        List<TagValue> GetLastValuesOfTags(string type);

        [OperationContract]
        List<TagValue> GetTagValuesWithinPeriod(DateTime start, DateTime end);
    }
}
