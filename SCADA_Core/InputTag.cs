using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SCADA_Core
{
    public class InputTag : Tag
    {
        public string Driver { get; set; }
        public int ScanTime { get; set; }
        public bool OnOffScan { get; set; }
        public string Type { get; set; }

        public InputTag() { }

        public InputTag(string tag, string desc, string io, string driver, int scan, bool onOff, string type) :base(tag, desc, io)
        {
            Driver = driver;
            ScanTime = scan;
            OnOffScan = onOff;
            Type = type;
        }
    }
}