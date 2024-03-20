using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Xml.Linq;

namespace SCADA_Core.App_Code
{
    public class init
    {
        public static void AppInitialize()
        {
            LoadTags();
            LoadAlarms();
            TagProcessing.StartContextThread();
            TagProcessing.StartThreads();
        }

        private static void LoadTags()
        {
            XElement scadaConfig = XElement.Load(HostingEnvironment.ApplicationPhysicalPath + "scadaConfig.xml");
            var tags = scadaConfig.Descendants("Tag");
            foreach (var tag in tags)
            {
                string type = tag.Attribute("type").Value;
                string name = tag.Attribute("name").Value;
                string ioAddr = tag.Attribute("ioAddress").Value;
                string tagType = tag.Attribute("tagType").Value;
                string description = tag.Value;
                string driver;
                int scanTime;
                bool onOffScan;
                double initValue, lowLimit, highLimit;

                if (tagType == "input")
                {
                    driver = tag.Attribute("driver").Value;
                    scanTime = Convert.ToInt32(tag.Attribute("scanTime").Value);
                    onOffScan = Convert.ToBoolean(tag.Attribute("onOffScan").Value);
                    InputTag input;
                    if (type == "digital")
                    {
                        input = new DigitalInput(name, description, ioAddr, driver, scanTime, onOffScan, type);
                    }
                    else
                    {
                        string unit = tag.Attribute("unit").Value;
                        lowLimit = Convert.ToDouble(tag.Attribute("lowLimit").Value);
                        highLimit = Convert.ToDouble(tag.Attribute("highLimit").Value);
                        input = new AnalogInput(name, description, ioAddr, driver, scanTime, onOffScan, type, lowLimit, highLimit, unit);
                    }
                    TagProcessing.AddInputTag(input);
                }
                else
                {
                    initValue = Convert.ToDouble(tag.Attribute("initValue").Value);
                    OutputTag output;
                    if (type == "digital")
                    {
                        output = new DigitalOutput(name, description, ioAddr, initValue, "digital");
                    }
                    else
                    {
                        string unit = tag.Attribute("unit").Value;
                        lowLimit = Convert.ToDouble(tag.Attribute("lowLimit").Value);
                        highLimit = Convert.ToDouble(tag.Attribute("highLimit").Value);
                        output = new AnalogOutput(name, description, ioAddr, initValue, lowLimit, highLimit, unit, "analog");
                    }
                    TagProcessing.AddOutputTag(output);
                }
            }
        }

        private static void LoadAlarms()
        {
            XElement alarmConfig = XElement.Load(HostingEnvironment.ApplicationPhysicalPath + "\\alarmConfig.xml");
            var alarms = alarmConfig.Descendants("Alarm");
            foreach(var al in alarms)
            {
                string tagName = al.Attribute("tagName").Value;
                string type = al.Attribute("type").Value;
                int prioritet = Convert.ToInt32(al.Attribute("priority").Value);
                double limit = Convert.ToDouble(al.Value);

                TagProcessing.AddAlarm(new Alarm(type, prioritet, tagName, limit));
            }
        }
    }
}