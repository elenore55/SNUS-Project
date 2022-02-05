using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Web;

namespace CoreWCFService
{
    [DataContract]
    [KnownType(typeof(InputTag))]
    [KnownType(typeof(OutputTag))]
    public class Tag
    {
        [DataMember] public string TagName { get; set; }
        [DataMember] public string Description { get; set; }
        [DataMember] public string IOAddress { get; set; }
    }

    [DataContract]
    [KnownType(typeof(AI))]
    [KnownType(typeof(DI))]
    public class InputTag : Tag
    {
        [DataMember] public Driver Driver { get; set; }
        [DataMember] public int ScanTime { get; set; }
        [DataMember] public bool OnScan { get; set; }
    }

    [DataContract]
    [KnownType(typeof(AO))]
    [KnownType(typeof(DO))]
    public class OutputTag : Tag
    {
        [DataMember] public double InitialValue { get; set; }
    }

    [DataContract]
    public class AI : InputTag
    {
        private List<Alarm> alarms;

        [DataMember] public double LowLimit { get; set; }
        [DataMember] public double HighLimit { get; set; }
        [DataMember] public string Units { get; set; }
        [DataMember] public List<Alarm> Alarms 
        {
            get 
            {
                if (alarms == null) alarms = new List<Alarm>();
                return alarms;
            }
            set
            {
                alarms = value;
            }
        }

        public AI()
        {
            alarms = new List<Alarm>();
        }

        public void AddAlarm(Alarm alarm)
        {
            if (alarms == null)
                alarms = new List<Alarm>();
            alarms.Add(alarm);
        }
    }

    [DataContract]
    public class AO : OutputTag
    {
        [DataMember] public double LowLimit { get; set; }
        [DataMember] public double HighLimit { get; set; }
        [DataMember] public string Units { get; set; }
    }

    [DataContract]
    public class DI : InputTag
    {

    }

    [DataContract]
    public class DO : OutputTag
    {
        
    }

    [DataContract]
    public class TagValue
    {
        [DataMember] 
        [Key]
        public int Id { get; set; }

        [DataMember] 
        public string TagType { get; set; }

        [DataMember] 
        public string TagName { get; set; }

        [DataMember] 
        public double Value { get; set; }

        [DataMember] 
        public DateTime ArrivedAt { get; set; }

        public override string ToString()
        {
            return $"{TagName}\t {TagType}\t Value: {Value}\t Arrived at: {ArrivedAt}";
        }
    }
}