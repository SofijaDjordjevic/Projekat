using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SCADA_Core
{
    public class RealTimeDriver
    {
        public static Dictionary<string, double> values = new Dictionary<string, double>();

        public static double ReturnValue(string address)
        {
            return values[address];
        }
    }
}