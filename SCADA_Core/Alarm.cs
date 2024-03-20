using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace SCADA_Core
{

    [DataContract]
    public class Alarm
    {
        [DataMember]
        public string Type { get; set; }
        [DataMember]
        public int Priority { get; set; }
        [DataMember]
        public string TagName { get; set; }
        [DataMember]
        public double Limit { get; set; }

        public Alarm() { }

        public Alarm(string type, int priority, string tagName, double limit)
        {
            Type = type;
            Priority = priority;
            TagName = tagName;
            Limit = limit;
        }
    }

    [DataContract]
    public class AlarmValue : Alarm
    {
        [Key]
        public int Id { get; set; }
        [DataMember]
        public DateTime Time { get; set; }
        [DataMember]
        public double TriggerValue { get; set; }

        public AlarmValue() { }

        public AlarmValue(string type, int priority, string tag, double limit, DateTime time, double trigger)
            :base(type, priority, tag, limit)
        {
            Time = time;
            TriggerValue = trigger;
        }

        public override string ToString() 
        {
            return $"Tag name: {TagName}, Limit: {Limit}, Trigger value: {TriggerValue}, Priority: {Priority}, Time: {Time}";
        }
    }
}