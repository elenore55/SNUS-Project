using System;
using System.Collections.Generic;
using System.Linq;

namespace CoreWCFService
{
    public class ReportManagerService : IReportManagerService
    {
        private static readonly int LIMIT = 50;
        public List<ActivatedAlarm> GetAlarmsOfPriority(int priority)
        {
            List<ActivatedAlarm> allActivatedAlarms = TagProcessing.GetActivatedAlarms();
            List<ActivatedAlarm> retVal = allActivatedAlarms.Where(x => x.Alarm.Priority == priority).OrderBy(x => x.ActivatedAt).ToList();
            return retVal.Skip(Math.Max(0, retVal.Count() - LIMIT)).ToList();
        }

        public List<ActivatedAlarm> GetAlarmsWithinPeriod(DateTime start, DateTime end)
        {
            List<ActivatedAlarm> allActivatedAlarms = TagProcessing.GetActivatedAlarms();
            List<ActivatedAlarm> retVal = allActivatedAlarms.Where(x => x.ActivatedAt >= start && x.ActivatedAt <= end)
                .OrderBy(x => x.Alarm.Priority).OrderBy(x => x.ActivatedAt).ToList();
            return retVal.Skip(Math.Max(0, retVal.Count() - LIMIT)).ToList();
        }

        public List<TagValue> GetLastValuesOfTags(string type)
        {
            List<TagValue> tagValues = TagProcessing.GetAllTagValues();
            Dictionary<string, TagValue> byTagName = new Dictionary<string, TagValue>();
            foreach (TagValue tv in tagValues)
            {
                if (tv.TagType == type)
                {
                    if (!byTagName.ContainsKey(tv.TagName))
                    {
                        byTagName[tv.TagName] = tv;
                    }
                    else if (byTagName[tv.TagName].ArrivedAt < tv.ArrivedAt)
                    {
                        byTagName[tv.TagName] = tv;
                    }
                }
            }
            return byTagName.Values.OrderBy(x => x.ArrivedAt).ToList();
        }

        public List<TagValue> GetTagValues(string tagName)
        {
            List<TagValue> retVal = TagProcessing.GetTagValuesHistory(tagName);
            return retVal.Skip(Math.Max(0, retVal.Count() - LIMIT)).ToList();
        }

        public List<TagValue> GetTagValuesWithinPeriod(DateTime start, DateTime end)
        {
            List<TagValue> tagValues = TagProcessing.GetAllTagValues();
            List<TagValue> retVal =  tagValues.Where(x => x.ArrivedAt >= start && x.ArrivedAt <= end).OrderBy(x => x.ArrivedAt).ToList();
            return retVal.Skip(Math.Max(0, retVal.Count() - LIMIT)).ToList();
        }
    }
}
