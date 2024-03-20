using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SCADA_Core
{
    public class DigitalInput : InputTag
    {
        public DigitalInput()
        {

        }

        public DigitalInput(string tagName, string desc, string io, string driver, int scan, bool onOff, string type)
            : base(tagName, desc, io, driver, scan, onOff, type)
        {
        }

    }
}