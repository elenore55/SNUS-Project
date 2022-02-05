using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace CoreWCFService
{
    public enum AlarmType { low, high };

    [DataContract]
    public class Alarm
    {
        [DataMember] 
        public AlarmType Type { get; set; }
        
        [DataMember] 
        public int Priority { get; set; }

        [DataMember]
        public double Threshold { get; set; }

        [DataMember]
        public string TagName { get; set; }

        public override string ToString()
        {
            return $"Alarm for {TagName}\t Type: {Type}\t Priority: {Priority}\t Threshold: {Threshold}";
        }
    }

    [DataContract]
    public class ActivatedAlarm
    {
        [DataMember]
        [Key]
        public int Id { get; set; }

        [DataMember]
        public Alarm Alarm { get; set; }

        [DataMember]
        public DateTime ActivatedAt { get; set; }

        public override string ToString()
        {
            return $"{Alarm}\t Activated at: {ActivatedAt}";
        }
    }
}