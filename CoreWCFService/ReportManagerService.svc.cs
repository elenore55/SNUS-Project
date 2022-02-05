using System;
using System.Collections.Generic;
using System.Linq;

namespace CoreWCFService
{
    public class ReportManagerService : IReportManagerService
    {

        TagProcessing tagProcessing = new TagProcessing();

        public List<ActivatedAlarm> GetAlarmsOfPriority(int priority)
        {
            List<ActivatedAlarm> allActivatedAlarms = tagProcessing.GetActivatedAlarms();
            return allActivatedAlarms.Where(x => x.Alarm.Priority == priority).OrderBy(x => x.ActivatedAt).ToList();
        }

        public List<ActivatedAlarm> GetAlarmsWithinPeriod(DateTime start, DateTime end)
        {
            List<ActivatedAlarm> allActivatedAlarms = tagProcessing.GetActivatedAlarms();
            return allActivatedAlarms.Where(x => x.ActivatedAt >= start && x.ActivatedAt <= end)
                .OrderBy(x => x.Alarm.Priority).OrderBy(x => x.ActivatedAt).ToList();
        }

        public List<TagValue> GetLastValuesOfTags(string type)
        {
            List<TagValue> tagValues = tagProcessing.GetAllTagValues();
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
            return tagProcessing.GetTagValuesHistory(tagName);
        }

        public List<TagValue> GetTagValuesWithinPeriod(DateTime start, DateTime end)
        {
            List<TagValue> tagValues = tagProcessing.GetAllTagValues();
            return tagValues.Where(x => x.ArrivedAt >= start && x.ArrivedAt <= end).OrderBy(x => x.ArrivedAt).ToList();
        }
    }
}
