using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SCADA_Core
{
    public class AnalogOutput : OutputTag
    {
        public double LowLimit { get; set; }

        public double HighLimit { get; set; }

        public string Units { get; set; }

        public AnalogOutput() { }

        public AnalogOutput(string tag, string desc, string io, double init, double low, double high, string unit, string type)
            : base(tag, desc, io, init, type)
        {
            LowLimit = low;
            HighLimit = high;
            Units = unit;
        }

    }
}