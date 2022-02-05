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
        static readonly string scadaConfigPath = @"C:\Users\Milica\Desktop\Fakultet\Semestar5\SNUS\Projekat\ProjekatSNUS\scadaConfig.xml";
        static readonly string alarmsLogPath = @"C:\Users\Milica\Desktop\Fakultet\Semestar5\SNUS\Projekat\ProjekatSNUS\alarmsLog.txt";
        static readonly object locker = new object();
        static readonly object alarmLock = new object();

        public delegate void TagValueChangedDelegate(InputTag tag, double value);
        public static event TagValueChangedDelegate OnTagValueChanged;
        public delegate void AlarmTriggeredDelegate(Alarm alarm);
        public static event AlarmTriggeredDelegate OnAlarmTriggered;

        public static List<ActivatedAlarm> GetActivatedAlarms()
        {
            return activatedAlarms;
        }

        public static List<TagValue> GetTagValuesHistory(string tagName)
        {
            using (var db = new TagValueContext())
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
            tags.Add(tag);
            currentValues[tag.TagName] = (tag is OutputTag outTag) ? outTag.InitialValue : 0;
            SaveConfiguration();
            return true;
        }

        public static bool RemoveTag(string tagName)
        {
            for (int i = 0; i < tags.Count; i++)
            {
                if (tags[i].TagName == tagName)
                {
                    tags.RemoveAt(i);
                    currentValues.Remove(tagName);
                    SaveConfiguration();
                    using (var db = new TagValueContext())
                    {
                        db.TagValues.Remove(db.TagValues.Single(x => x.TagName == tagName));
                        db.SaveChanges();
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
            return currentValues;
        }

        public static bool EnterOutputTagValue(string tagName, double value)
        {
            if (!currentValues.ContainsKey(tagName))
                return false;
            currentValues[tagName] = value;
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
                    thread.Start();
                    // SimulateInputTag(inputTag);
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
                    lock (locker)
                    {
                        
                        int index = tags.FindIndex(x => x == tag);
                        if (tag is AI aiTag)
                        {
                            if (newValue != currentValues[tag.TagName])
                            {
                                if (newValue >= aiTag.LowLimit && newValue <= aiTag.HighLimit)
                                {
                                    currentValues[tag.TagName] = newValue;
                                    tags[index] = tag;
                                    OnTagValueChanged?.Invoke(tag, newValue);
                                }
                                foreach (Alarm alarm in aiTag.Alarms)
                                {
                                    if ((alarm.Type == AlarmType.high && newValue > alarm.Threshold) || (alarm.Type == AlarmType.low && newValue < alarm.Threshold))
                                    {
                                        AddActivatedAlarm(new ActivatedAlarm { Id = activatedAlarms.Count, Alarm = alarm, ActivatedAt = DateTime.Now });
                                        OnAlarmTriggered?.Invoke(alarm);
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
                                currentValues[tag.TagName] = newValue;
                                tags[index] = tag;
                                OnTagValueChanged?.Invoke(tag, newValue);
                                SaveConfiguration();
                                SaveTagValueToDB(tag, newValue);
                            }
                        }
                    }
                    Thread.Sleep(tag.ScanTime);
                }
            }
        }

        private static void SaveTagValueToDB(Tag tag, double value)
        {
            using (var db = new TagValueContext())
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

        public static List<TagValue> GetAllTagValues()
        {
            using (var db = new TagValueContext())
            {
                return db.TagValues.ToList();
            }
        }

        private static void AddActivatedAlarm(ActivatedAlarm activatedAlarm)
        {
            lock (alarmLock)
            {
                activatedAlarms.Add(activatedAlarm);
                using (StreamWriter writer = File.AppendText(alarmsLogPath))
                {
                    writer.WriteLine(activatedAlarm.ToString());
                }
                SaveAlarmToDB(activatedAlarm);
            }
        }

        private static void SaveAlarmToDB(ActivatedAlarm activatedAlarm)
        {
            using (var db = new AlarmContext())
            {
                try
                {
                    db.ActivatedAlarms.Add(activatedAlarm);
                    db.SaveChanges();
                }
                catch(Exception)
                {
                }
            }
        }

        public static void LoadConfiguration()
        {
            XElement xmlData = XElement.Load(scadaConfigPath);
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

            using (var writer = new StreamWriter(scadaConfigPath))
            {
                writer.Write(tagsXML);
            }
        }
    }
}