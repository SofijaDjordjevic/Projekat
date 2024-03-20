using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SCADA_Core
{
    public class DigitalOutput : OutputTag
    { 
        public DigitalOutput() { }

        public DigitalOutput(string tag, string desc, string io, double init, string type)
            :base(tag, desc, io, init, type)
        {

        }
    }
}