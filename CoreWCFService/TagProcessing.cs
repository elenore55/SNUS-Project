using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.IO;
using System.Threading;

namespace CoreWCFService
{
    public class TagProcessing
    {
        static List<Tag> tags = new List<Tag>();
        static Dictionary<string, double> currentValues = new Dictionary<string, double>();
        
        static List<ActivatedAlarm> activatedAlarms = new List<ActivatedAlarm>();
        static readonly string CONFIG_FOLDER = @"C:\ScadaConfig\";
        static readonly string SCADA_CONFIG_FILE = @"scadaConfig.xml";
        static readonly string ALARMS_LOG_FILE = @"alarmsLog.txt";
        static readonly object tagsLock = new object();
        static readonly object tagValuesDBLock = new object();
        static readonly object activatedAlarmsLock = new object();
        static readonly object scadaConfigLock = new object();
        static readonly object alarmsLogLock = new object();
        static readonly object alarmsDBLock = new object();
        static Dictionary<string, Thread> threads = new Dictionary<string, Thread>();

        public delegate void TagValueChangedDelegate(InputTag tag, double value);
        public static event TagValueChangedDelegate OnTagValueChanged;
        public delegate void AlarmTriggeredDelegate(ActivatedAlarm alarm, double value);
        public static event AlarmTriggeredDelegate OnAlarmTriggered;

        public static List<ActivatedAlarm> GetActivatedAlarms()
        {
            lock (activatedAlarmsLock)
            {
                using (var db = new ActivatedAlarmsContext())
                {
                    return db.ActivatedAlarms.ToList();
                }
            }
        }

        public static List<TagValue> GetTagValuesHistory(string tagName)
        {
            using (var db = new TagValuesContext())
            {
                return (from tv in db.TagValues
                        where tv.TagName.Equals(tagName)
                        orderby tv.Value
                        select tv).ToList();       
            }
        } 
            
        public static bool AddTag(Tag tag)
        {
            if (currentValues.ContainsKey(tag.TagName))
                return false;
            lock (tagsLock)
            {
                tags.Add(tag);
                currentValues[tag.TagName] = (tag is OutputTag outTag) ? outTag.InitialValue : 0;
            }
            if (tag is InputTag inputTag)
            {
                Thread thread = new Thread(() => { SimulateInputTag(inputTag); });
                threads[inputTag.TagName] = thread;
                thread.Start();
            }
            SaveConfiguration();
            return true;
        }

        public static bool RemoveTag(string tagName)
        {
            for (int i = 0; i < tags.Count; i++)
            {
                if (tags[i].TagName == tagName)
                {
                    lock (tagsLock)
                    {
                        tags.RemoveAt(i);
                        currentValues.Remove(tagName);
                    }
                    if (threads.ContainsKey(tagName))
                    {
                        try
                        {
                            threads[tagName].Abort();
                        }
                        finally
                        {
                            threads.Remove(tagName);
                        }
                    }
                    SaveConfiguration();
                    lock (tagValuesDBLock)
                    {
                        using (var db = new TagValuesContext())
                        {
                            foreach (TagValue tv in db.TagValues)
                            {
                                if (tv.TagName == tagName)
                                {
                                    db.TagValues.Remove(tv);
                                }
                            }
                            db.SaveChanges();
                        }
                    }
                    lock (alarmsDBLock)
                    {
                        using (var db = new ActivatedAlarmsContext())
                        {
                            foreach (ActivatedAlarm alarm in db.ActivatedAlarms)
                            {
                                if (alarm.Alarm.TagName == tagName)
                                {
                                    db.ActivatedAlarms.Remove(alarm);
                                }
                            }
                            db.SaveChanges();
                        }
                    }
                    return true;
                }
            }
            return false;
        }

        public static void ChangeOnScanValue(string tagName)
        {
            for (int i = 0; i < tags.Count; i++)
            {
                if (tags[i].TagName == tagName && tags[i] is InputTag inputTag)
                {
                    inputTag.OnScan = !inputTag.OnScan;
                    lock (tagsLock) 
                        tags[i] = inputTag;
                    SaveConfiguration();
                    break;
                }
            }
        }

        public static string GetOnScanValue(string tagName)
        {
            foreach (Tag tag in tags)
            {
                if (tag.TagName == tagName && tag is InputTag inputTag)
                {
                    return inputTag.OnScan ? "On" : "Off";
                }
            }
            return "Non-existing tag";
        }

        public static bool SetOutputTagValue(string tagName, double value)
        {
            foreach (Tag tag in tags)
            {
                if (tag.TagName == tagName && tag is OutputTag)
                {
                    lock (tagsLock)
                        currentValues[tagName] = value;
                    RealTimeDriver.valuesOnAddresses[tag.IOAddress] = value;
                    SaveTagValueToDB(tag, value);
                    return true;
                }
            }
            return false;
        }

        public static double GetOutputTagValue(string tagName)
        {
            if (currentValues.ContainsKey(tagName)) return currentValues[tagName];
            return -1;
        }

        public static Dictionary<string, double> GetCurrentValues()
        {
            Dictionary<string, double> retVal = new Dictionary<string, double>();
            foreach (Tag tag in tags)
            {
                if (tag is OutputTag outTag)
                {
                    retVal[tag.TagName] = currentValues[tag.TagName];
                }
            }
            return retVal;
        }

        public static bool EnterOutputTagValue(string tagName, double value)
        {
            lock (tagsLock)
            {
                if (!currentValues.ContainsKey(tagName))
                    return false;
                currentValues[tagName] = value;
            }
            SaveConfiguration();
            return true;
        }

        public static bool AddAlarm(Alarm alarm)
        {
            for (int i = 0; i < tags.Count; i++)
            {
                if (tags[i].TagName == alarm.TagName)
                {
                    if (tags[i] is AI current)
                    {
                        current.AddAlarm(alarm);
                        lock (tagsLock)
                            tags[i] = current;
                        SaveConfiguration();
                        return true;
                    }
                }
            }
            return false;
        }

        public static void Simulate()
        {
            for (int i = 0; i < tags.Count; i++)
            {
                if (tags[i] is InputTag inputTag)
                {
                    Thread thread = new Thread(() => { SimulateInputTag(inputTag); });
                    threads[inputTag.TagName] = thread;
                    thread.Start();
                }
            }
        }

        private static void SimulateInputTag(InputTag tag)
        {
            while (true)
            {
                if (tag.OnScan)
                {
                    double newValue = tag.Driver.ReturnValue(tag.IOAddress); 
                    int index = tags.FindIndex(x => x == tag);
                    if (tag is AI aiTag)
                    {
                        if (newValue != currentValues[tag.TagName])
                        {
                            if (newValue >= aiTag.LowLimit && newValue <= aiTag.HighLimit)
                            {
                                lock (tagsLock)
                                {
                                    currentValues[tag.TagName] = newValue;
                                    tags[index] = tag;
                                }
                                OnTagValueChanged?.Invoke(tag, newValue);
                            }
                            foreach (Alarm alarm in aiTag.Alarms)
                            {
                                if ((alarm.Type == AlarmType.high && newValue > alarm.Threshold) || (alarm.Type == AlarmType.low && newValue < alarm.Threshold))
                                {
                                    AddActivatedAlarm(new ActivatedAlarm { Id = activatedAlarms.Count, Alarm = alarm, ActivatedAt = DateTime.Now }, newValue);
                                }
                            }
                            SaveConfiguration();
                            SaveTagValueToDB(tag, newValue);
                        }
                    }
                    else
                    {
                        newValue = newValue > 0 ? 1 : 0;
                        if (newValue != currentValues[tag.TagName])
                        {
                            lock (tagsLock)
                            {
                                currentValues[tag.TagName] = newValue;
                                tags[index] = tag;
                                OnTagValueChanged?.Invoke(tag, newValue);
                            }
                            SaveConfiguration();
                            SaveTagValueToDB(tag, newValue);
                        }
                    }
                    Thread.Sleep(tag.ScanTime * 1000);
                }
            }
        }

        private static void SaveTagValueToDB(Tag tag, double value)
        {
            lock (tagValuesDBLock)
            {
                using (var db = new TagValuesContext())
                {
                    db.TagValues.Add(new TagValue
                    {
                        Id = db.TagValues.Count(),
                        TagType = tag.GetType().Name,
                        TagName = tag.TagName,
                        Value = value,
                        ArrivedAt = DateTime.Now
                    });
                    db.SaveChanges();
                }
            }
        }

        public static List<TagValue> GetAllTagValues()
        {
            lock (tagValuesDBLock)
            {
                using (var db = new TagValuesContext())
                {
                    return db.TagValues.ToList();
                }
            }
        }

        private static void AddActivatedAlarm(ActivatedAlarm activatedAlarm, double value)
        {
            if (activatedAlarms.Count > 0)
            {
                foreach (ActivatedAlarm existing in activatedAlarms)
                {
                    var diffSeconds = (activatedAlarm.ActivatedAt - existing.ActivatedAt).TotalSeconds;
                    if (existing.Alarm.TagName == activatedAlarm.Alarm.TagName && existing.Alarm.Type == activatedAlarm.Alarm.Type && diffSeconds < 5) return;
                }    
            }
            OnAlarmTriggered?.Invoke(activatedAlarm, value);
            lock (activatedAlarmsLock)
            {
                activatedAlarms.Add(activatedAlarm);
            }
            lock (alarmsLogLock)
            {
                using (StreamWriter writer = File.AppendText(GetPath(ALARMS_LOG_FILE)))
                {
                    writer.WriteLine(activatedAlarm.ToString());
                }
            }
            SaveAlarmToDB(activatedAlarm);
        }

        private static void SaveAlarmToDB(ActivatedAlarm activatedAlarm)
        {
            lock (alarmsDBLock)
            {
                using (var db = new ActivatedAlarmsContext())
                {
                    db.ActivatedAlarms.Add(activatedAlarm);
                    db.SaveChanges();
                }
            }
        }

        public static void LoadConfiguration()
        {
            XElement xmlData = XElement.Load(GetPath(SCADA_CONFIG_FILE));
            var AITags = xmlData.Descendants("AI");
            var AOTags = xmlData.Descendants("AO");
            var DITags = xmlData.Descendants("DI");
            var DOTags = xmlData.Descendants("DO");

            if (AITags != null)
            {
                foreach (var tag in AITags)
                {
                    AI ait = new AI
                    {
                        TagName = tag.Attribute("tagName").Value,
                        Description = tag.Attribute("description").Value,
                        IOAddress = tag.Attribute("IOAddress").Value,
                        Units = tag.Attribute("units").Value,
                        LowLimit = double.Parse(tag.Attribute("lowLimit").Value),
                        HighLimit = double.Parse(tag.Attribute("highLimit").Value),
                        ScanTime = int.Parse(tag.Attribute("scanTime").Value),
                        OnScan = bool.Parse(tag.Attribute("onScan").Value),
                    };
                    if (tag.Attribute("driver").Value == "SimulationDriver") ait.Driver = new SimulationDriver();
                    else ait.Driver = new RealTimeDriver();
                    foreach (var alarm in tag.Descendants("alarm"))
                    {
                        ait.AddAlarm(new Alarm
                        {
                            TagName = ait.TagName,
                            Priority = int.Parse(alarm.Attribute("priority").Value),
                            Threshold = double.Parse(alarm.Attribute("threshold").Value),
                            Type = (alarm.Attribute("type").Value == "low") ? AlarmType.low : AlarmType.high
                        });
                    }
                    tags.Add(ait);
                    currentValues[tag.Attribute("tagName").Value] = 0;
                }
            }
            
            if (AOTags != null)
            {
                foreach (var tag in AOTags)
                {
                    tags.Add(new AO
                    {
                        TagName = tag.Attribute("tagName").Value,
                        Description = tag.Attribute("description").Value,
                        IOAddress = tag.Attribute("IOAddress").Value,
                        InitialValue = int.Parse(tag.Attribute("initialValue").Value),
                        Units = tag.Attribute("units").Value,
                        LowLimit = double.Parse(tag.Attribute("lowLimit").Value),
                        HighLimit = double.Parse(tag.Attribute("highLimit").Value)
                    });
                    currentValues[tag.Attribute("tagName").Value] = int.Parse(tag.Attribute("initialValue").Value);
                }
            }
            
            if (DITags != null)
            {
                foreach (var tag in DITags)
                {
                    DI dit = new DI
                    {
                        TagName = tag.Attribute("tagName").Value,
                        Description = tag.Attribute("description").Value,
                        IOAddress = tag.Attribute("IOAddress").Value,
                        ScanTime = int.Parse(tag.Attribute("scanTime").Value),
                        OnScan = bool.Parse(tag.Attribute("onScan").Value)
                    };
                    if (tag.Attribute("driver").Value == "SimulationDriver") dit.Driver = new SimulationDriver();
                    else dit.Driver = new RealTimeDriver();
                    tags.Add(dit);
                    currentValues[tag.Attribute("tagName").Value] = 0;
                }
            }
            
            if (DOTags != null)
            {
                foreach (var tag in DOTags)
                {
                    tags.Add(new DO
                    {
                        TagName = tag.Attribute("tagName").Value,
                        Description = tag.Attribute("description").Value,
                        IOAddress = tag.Attribute("IOAddress").Value,
                        InitialValue = int.Parse(tag.Attribute("initialValue").Value)
                    });
                    currentValues[tag.Attribute("tagName").Value] = int.Parse(tag.Attribute("initialValue").Value);
                }
            }
        }

        private static void SaveConfiguration()
        {
            XElement tagsXML = new XElement("tags",
                from dot in (from t in tags where t is DO select (DO)t)
                select new XElement("DO", currentValues[dot.TagName],
                    new XAttribute("tagName", dot.TagName),
                    new XAttribute("description", dot.Description),
                    new XAttribute("IOAddress", dot.IOAddress),
                    new XAttribute("initialValue", dot.InitialValue)
                )
            );

            tagsXML.Add(
                from dit in (from t in tags where t is DI select (DI)t)
                select new XElement("DI", currentValues[dit.TagName],
                    new XAttribute("tagName", dit.TagName),
                    new XAttribute("description", dit.Description),
                    new XAttribute("IOAddress", dit.IOAddress),
                    new XAttribute("driver", dit.Driver.GetType().Name),
                    new XAttribute("scanTime", dit.ScanTime),
                    new XAttribute("onScan", dit.OnScan)
                )
            );

            tagsXML.Add(
                from aot in (from t in tags where t is AO select (AO)t)
                select new XElement("AO", currentValues[aot.TagName],
                    new XAttribute("tagName", aot.TagName),
                    new XAttribute("description", aot.Description),
                    new XAttribute("IOAddress", aot.IOAddress),
                    new XAttribute("initialValue", aot.InitialValue),
                    new XAttribute("lowLimit", aot.LowLimit),
                    new XAttribute("highLimit", aot.HighLimit),
                    new XAttribute("units", aot.Units)
                )
            );

            tagsXML.Add(
                from ait in (from t in tags where t is AI select (AI)t)
                select new XElement("AI", currentValues[ait.TagName],
                    new XAttribute("tagName", ait.TagName),
                    new XAttribute("description", ait.Description),
                    new XAttribute("IOAddress", ait.IOAddress),
                    new XAttribute("driver", ait.Driver.GetType().Name),
                    new XAttribute("scanTime", ait.ScanTime),
                    new XAttribute("onScan", ait.OnScan),
                    new XAttribute("lowLimit", ait.LowLimit),
                    new XAttribute("highLimit", ait.HighLimit),
                    new XAttribute("units", ait.Units),
                    new XElement("alarms",
                        from alarm in ait.Alarms
                        select new XElement("alarm",
                            new XAttribute("type", alarm.Type),
                            new XAttribute("priority", alarm.Priority),
                            new XAttribute("threshold", alarm.Threshold)
                        )
                    )
                )
            );
            lock (scadaConfigLock)
            {
                using (var writer = new StreamWriter(GetPath(SCADA_CONFIG_FILE)))
                {
                    writer.Write(tagsXML);
                }
            }
        }

        private static string GetPath(string fileName)
        {
            if (!(Directory.Exists(CONFIG_FOLDER)))
                Directory.CreateDirectory(CONFIG_FOLDER);
            return Path.Combine(CONFIG_FOLDER, fileName);
        }
    }
}