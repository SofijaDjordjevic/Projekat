using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SCADA_Core
{
    public class OutputTag : Tag
    {
        public double InitialValue { get; set; }

        public string Type { get; set; }

        public OutputTag() { }

        public OutputTag(string tag, string desc, string io, double init, string type) : base(tag, desc, io)
        {
            InitialValue = init;
            Type = type;
        }
    }
}