using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Hosting;
using System.Xml.Linq;

namespace SCADA_Core
{
    public class TagProcessing
    {
        public delegate void ChangeInputTagDelegate(string input, double value);
        public static event ChangeInputTagDelegate inputChanged;
        public delegate void AlarmCalledDelegate(string alarm, int prioritet);
        public static event AlarmCalledDelegate alarmCalled;

        public static Dictionary<string, Thread> inputTagThreads = new Dictionary<string, Thread>(); 
        public static Dictionary<string, InputTag> inputTags = new Dictionary<string, InputTag>();
        public static Dictionary<string, OutputTag> outputTags = new Dictionary<string, OutputTag>();
        public static Dictionary<string, Alarm> alarms = new Dictionary<string, Alarm>();

        public static DatabaseContext db = new DatabaseContext();

        public static readonly object locker = new object();


        public static void StartThreads()
        {
            foreach (KeyValuePair<string, Thread> kvp in inputTagThreads)
            {
                kvp.Value.Start();
            }
        }

        public static void StartContextThread()
        {
            Thread t = new Thread(() =>
            {
                while (true)
                {
                    lock (locker)
                    {
                        db.SaveChanges();
                    }

                    Thread.Sleep(5000);
                }
            });
            t.Start();
        }

        public static Thread AddInputTag(InputTag input)
        {
            inputTags.Add(input.TagName, input);
            Thread t = new Thread(() => { DoWork(input); });
            inputTagThreads.Add(input.TagName, t);
            return t;
        }

        public static void AddOutputTag(OutputTag output)
        {
            outputTags.Add(output.TagName, output);

            TagValue tagValue = new TagValue();
            tagValue.TagName = output.TagName;
            tagValue.Time = DateTime.Now;
            tagValue.Value = output.InitialValue;
            tagValue.Tag = "output";
            tagValue.Type = output.Type;

            db.Values.Add(tagValue);
            db.SaveChanges();
        }

        public static string ChangeOutputValue(string tagName, double value)
        {
            OutputTag ot = outputTags[tagName];

            if(ot.Type == "digital")
            {
                if(value != 0 && value != 1)
                {
                    return "Vrednost digitalnog taga moze biti samo 0 ili 1";
                }
            } else
            {
                AnalogOutput ao = (AnalogOutput)ot;
                if(value > ao.HighLimit || value < ao.LowLimit)
                {
                    return $"Vrednost ({value}) mora biti izmedju low({ao.LowLimit}) i high({ao.HighLimit}) limita";
                }
            }
            outputTags[tagName].InitialValue = value;

            TagValue tagValue = new TagValue();
            tagValue.TagName = ot.TagName;
            tagValue.Time = DateTime.Now;
            tagValue.Value = ot.InitialValue;
            tagValue.Tag = "output";
            tagValue.Type = ot.Type;

            db.Values.Add(tagValue);
            db.SaveChanges();

            TagProcessing.UpdateScadaConfig();

            return "Uspesna izmena vrednosti!";
        }

        public static void RemoveOutputTag(string tagName)
        {
            outputTags.Remove(tagName);
            UpdateScadaConfig();
        }

        public static void RemoveInputTag(string tagName)
        {
            inputTags.Remove(tagName);
            inputTagThreads[tagName].Abort();
            inputTagThreads.Remove(tagName);

            List<Alarm> tagAlarms = GetTagAlarms(tagName);
            foreach(Alarm al in tagAlarms) 
            {
                alarms.Remove(al.Type + al.TagName + al.Limit);
            }
            UpdateScadaConfig();
        }

        private static List<Alarm> GetTagAlarms(string tagName)
        {
            List<Alarm> tagAlarms = new List<Alarm>();

            foreach(Alarm al in alarms.Values)
            {
                if (al.TagName == tagName) 
                {
                    tagAlarms.Add(al);
                }

            }
            return tagAlarms;
        }
        public static double ReadValueFromDriver(InputTag input)
        {
            double value = input.Driver == "SD" ? SimulationDriver.ReturnValue(input.IOAddress) : RealTimeDriver.ReturnValue(input.IOAddress);
            return value;
        }

        private static void DoWork(InputTag input)
        {
            bool init;
            double newValue;
            lock (locker)
            {
                init = true;
                newValue = -100;
            }
            while (true)
            {
                bool onOff = inputTags[input.TagName].OnOffScan;
                if(onOff)
                {
                    if (input.Driver == "RTD")
                    {
                        if (!IsRTUActive(input))
                        {
                            continue;
                        }
                    }
                    double value = ReadValueFromDriver(input);

                    lock (locker)
                    {
                        if (input.Type == "analog")
                        {
                            AnalogInput ai = (AnalogInput)input;

                            value = value < ai.LowLimit ? ai.LowLimit : value;
                            value = value > ai.HighLimit ? ai.HighLimit : value;

                            CheckAlarm(input, value);
                        } else
                        {
                            value = value < 0.5 ? 0 : 1;
                        }
                        if (init)
                        {
                            newValue = value;
                        } else
                        {
                            if (value != newValue)
                            {
                                TagValue tagValue = new TagValue();
                                tagValue.TagName = input.TagName;
                                tagValue.Time = DateTime.Now;
                                tagValue.Value = value;
                                tagValue.Tag = "input";
                                tagValue.Type = input.Type;
                                db.Values.Add(tagValue);
                                newValue = value;
                            }
                        }
                        init = false;
                    }
                    inputChanged?.Invoke(input.TagName, value);
                }

                Thread.Sleep(input.ScanTime * 1000);
            }
        }

        internal static List<OutputTag> GetOutputTags()
        {
            List<OutputTag> tags = new List<OutputTag>();
            foreach (OutputTag o in TagProcessing.outputTags.Values)
            {
                tags.Add(o);
            }

            return tags;
        }

        private static bool IsRTUActive(InputTag input)
        {
            return RealTimeDriver.values.ContainsKey(input.IOAddress);
        }

        public static void AddAlarm(Alarm al)
        {
            string id = al.Type + al.TagName + al.Limit;
            alarms.Add(id, al);
        }

        public static void RemoveAlarm(string id)
        {
            alarms.Remove(id);
        }

        private static void CheckAlarm(InputTag input, double value)
        {
            foreach(Alarm al in alarms.Values)
            {
                if(al.TagName == input.TagName)
                {
                    if(al.Type == "low" && value <= al.Limit || al.Type == "high" && value >= al.Limit)
                    {
                        AlarmValue av = new AlarmValue(al.Type, al.Priority, al.TagName, al.Limit, DateTime.Now, value);
                        db.Alarms.Add(av);

                        alarmCalled?.Invoke($"Tag: {av.TagName}, Alarm type: {av.Type}, Alarm limit: {av.Limit}, Trigger Value: {av.TriggerValue}, Time {av.Time}", av.Priority);
                        UpdateAlarmsLog(av);
                    }
                }
            }
            db.SaveChanges();
        }
        public static bool DriverAddressValidation(string driver, string address)
        {
            if (driver == "SD" && (address != "S" && address != "C" && address != "R"))
                return false;

            if (driver == "RTD" && !RealTimeDriver.values.ContainsKey(address))
                return false;

            return true;
        }

        public static void UpdateAlarmsLog(AlarmValue av)
        {
            lock(locker)
            {
                using (StreamWriter sw = File.AppendText(HostingEnvironment.ApplicationPhysicalPath + "\\alarmsConfig.txt"))
                {
                    sw.WriteLine($"Tag: {av.TagName}, Alarm type: {av.Type}, Alarm limit: {av.Limit}, Value: {av.TriggerValue}, Time: {av.Time}");
                }
            }
        }
        public static void UpdateScadaConfig()
        {
            XElement scadaXML = new XElement("Tags",
                from input in inputTags
                where input.Value.Type == "digital"
                select new XElement("Tag", input.Value.Description, new XAttribute("tagType", "input"),
                                                                    new XAttribute("type", input.Value.Type),
                                                                    new XAttribute("name", input.Value.TagName),
                                                                    new XAttribute("ioAddress", input.Value.IOAddress),
                                                                    new XAttribute("driver", input.Value.Driver),
                                                                    new XAttribute("scanTime", input.Value.ScanTime),
                                                                    new XAttribute("onOffScan", input.Value.OnOffScan)),
                from analogInput in GetAnalogInputs()
                select new XElement("Tag", analogInput.Description, new XAttribute("tagType", "input"),
                                                                    new XAttribute("type", analogInput.Type),
                                                                    new XAttribute("name", analogInput.TagName),
                                                                    new XAttribute("ioAddress", analogInput.IOAddress),
                                                                    new XAttribute("driver", analogInput.Driver),
                                                                    new XAttribute("scanTime", analogInput.ScanTime),
                                                                    new XAttribute("onOffScan", analogInput.OnOffScan),
                                                                    new XAttribute("lowLimit", analogInput.LowLimit),
                                                                    new XAttribute("highLimit", analogInput.HighLimit),
                                                                    new XAttribute("unit", analogInput.Units)),
                from output in outputTags
                where output.Value.Type == "digital"
                select new XElement("Tag", output.Value.Description, new XAttribute("tagType", "output"),
                                                                     new XAttribute("type", output.Value.Type),
                                                                     new XAttribute("name", output.Value.TagName),
                                                                     new XAttribute("ioAddress", output.Value.IOAddress),
                                                                     new XAttribute("initValue", output.Value.InitialValue)),
                from analogOutput in GetAnalogOutputs()
                select new XElement("Tag", analogOutput.Description, new XAttribute("tagType", "output"),
                                                                new XAttribute("type", analogOutput.Type),
                                                                new XAttribute("name", analogOutput.TagName),
                                                                new XAttribute("ioAddress", analogOutput.IOAddress),
                                                                new XAttribute("initValue", analogOutput.InitialValue),
                                                                new XAttribute("lowLimit", analogOutput.LowLimit),
                                                                new XAttribute("highLimit", analogOutput.HighLimit),
                                                                new XAttribute("unit", analogOutput.Units)
                ));
            using (StreamWriter sw = File.CreateText(HostingEnvironment.ApplicationPhysicalPath + "\\scadaConfig.xml"))
            {
                sw.Write(scadaXML);
            }
        }
        public static void UpdateAlarmConfig()
        {
            List<AlarmValue> alarmList = new List<AlarmValue>();

            XElement alarmXML = new XElement("Alarms",
                from alarm in alarms.Values
                select new XElement("Alarm", alarm.Limit, new XAttribute("type", alarm.Type),
                                                            new XAttribute("tagName", alarm.TagName),
                                                            new XAttribute("priority", alarm.Priority)
                                                            ));

            using (StreamWriter sw = File.CreateText(HostingEnvironment.ApplicationPhysicalPath + "\\alarmConfig.xml"))
            {
                sw.Write(alarmXML);
            }
        }

        private static List<AnalogOutput> GetAnalogOutputs()
        {
            List<AnalogOutput> outputs= new List<AnalogOutput>();

            foreach (OutputTag o in outputTags.Values)
            {
                if (o.Type == "analog")
                {
                    outputs.Add((AnalogOutput)o);
                }
            }
            return outputs;
        }
        private static List<AnalogInput> GetAnalogInputs()
        {
            List<AnalogInput> inputs = new List<AnalogInput>();

            foreach (InputTag i in inputTags.Values)
            {
                if (i.Type == "analog")
                {
                    inputs.Add((AnalogInput)i);
                }
            }
            return inputs;
        }
    }

 }