using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SCADA_Core
{
    public class AnalogInput : InputTag
    {
        List<Alarm> Alarms { get; set; }
        public double LowLimit { get; set; }

        public double HighLimit { get; set; }

        public string Units { get; set; }

        public AnalogInput() {}

        public AnalogInput(string tag, string desc, string io, string driver, int scan, bool onOff, string type, double low, double high, string unit) 
            :base(tag, desc, io, driver, scan, onOff, type)
        {
            LowLimit = low;
            HighLimit = high;
            Units = unit;
            Alarms = new List<Alarm>();
        }
    }
}