using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace SCADA_Core
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "ReportManagerService" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select ReportManagerService.svc or ReportManagerService.svc.cs at the Solution Explorer and start debugging.
    public class ReportManagerService : IReportManagerService
    {
        public List<AlarmValue> GetAlarmsByPriority(int prioritet, string sortType)
        {
            List<AlarmValue> alarms = new List<AlarmValue>();

            using(var db = new DatabaseContext())
            {
                if (sortType == "asc")
                {
                    alarms = db.Alarms.Where(a => a.Priority == prioritet).OrderBy(a => a.Time).ToList();
                } else
                {
                    alarms = db.Alarms.Where(a => a.Priority == prioritet).OrderByDescending(a => a.Time).ToList();
                }
            }
            return alarms;
        }

        public List<AlarmValue> GetAlarmsInTimeRange(DateTime from, DateTime to, string sortBy, string sortType)
        {
            List<AlarmValue> alarms = new List<AlarmValue>();

            using (var db = new DatabaseContext())
            {
                if (sortType == "asc")
                {
                    if (sortBy == "prioritet")
                    {
                        alarms = db.Alarms.Where(t => t.Time.CompareTo(from) >= 0 && t.Time.CompareTo(to) <= 0).OrderBy(v => v.Priority).ToList();
                    }else
                    {
                        alarms = db.Alarms.Where(t => t.Time.CompareTo(from) >= 0 && t.Time.CompareTo(to) <= 0).OrderBy(v => v.Time).ToList();
                    }
                }
                else
                {
                    if (sortBy == "prioritet")
                    {
                        alarms = db.Alarms.Where(t => t.Time.CompareTo(from) >= 0 && t.Time.CompareTo(to) <= 0).OrderByDescending(v => v.Priority).ToList();
                    } 
                    else
                    {
                        alarms = db.Alarms.Where(t => t.Time.CompareTo(from) >= 0 && t.Time.CompareTo(to) <= 0).OrderByDescending(v => v.Time).ToList();
                    }
                }
            }

            return alarms;
        }

        public List<TagValue> GetLastValuesAI(string sortType)
        {
            List<TagValue> allInputs = new List<TagValue>();
            List<TagValue> inputs = new List<TagValue>();
            List<string> tags = new List<string>();

            using (var db = new DatabaseContext())
            {
                if (sortType == "asc")
                {
                    allInputs = db.Values.Where(t => t.Type == "analog" && t.Tag == "input").OrderBy(v => v.Time).ToList();
                }else
                {
                    allInputs = db.Values.Where(t => t.Type == "analog" && t.Tag == "input").OrderByDescending(v => v.Time).ToList();
                }
            }
            if (sortType == "asc")
            {
                for (int i = allInputs.Count - 1; i >= 0; i--)
                {
                    TagValue t = allInputs[i];
                    if (!tags.Contains(t.TagName))
                    {
                        tags.Add(t.TagName);
                        inputs.Add(t);
                    }
                }
            }
            else
            {
                foreach (TagValue t in allInputs)
                {
                    if (!tags.Contains(t.TagName))
                    {
                        tags.Add(t.TagName);
                        inputs.Add(t);
                    }
                }
            }

            return inputs;
        }

        public List<TagValue> GetLastValuesDI(string sortType)
        {
            List<TagValue> allInputs = new List<TagValue>();
            List<TagValue> inputs = new List<TagValue>();
            List<string> tags = new List<string>();

            using (var db = new DatabaseContext())
            {
                if (sortType == "asc")
                {
                    allInputs = db.Values.Where(t => t.Type == "digital" && t.Tag == "input").OrderBy(v => v.Time).ToList();
                }
                else
                {
                    allInputs = db.Values.Where(t => t.Type == "digital" && t.Tag == "input").OrderByDescending(v => v.Time).ToList();
                }
            }
            if (sortType == "asc")
            {
                for (int i = allInputs.Count - 1; i >= 0; i--)
                {
                    TagValue t = allInputs[i];
                    if (!tags.Contains(t.TagName))
                    {
                        tags.Add(t.TagName);
                        inputs.Add(t);
                    }
                }
            }
            else
            {
                foreach (TagValue t in allInputs)
                {
                    if (!tags.Contains(t.TagName))
                    {
                        tags.Add(t.TagName);
                        inputs.Add(t);
                    }
                }
            }

            return inputs;
        }

        public List<TagValue> GetTagsInTimeRange(DateTime from, DateTime to, string sortType)
        {
            List<TagValue> tags = new List<TagValue>();

            using (var db = new DatabaseContext())
            {
                if(sortType == "asc")
                {
                    tags = db.Values.Where(t => t.Time.CompareTo(from) >= 0 && t.Time.CompareTo(to) <= 0).OrderBy(v => v.Time).ToList();
                } 
                else
                {
                    tags = db.Values.Where(t => t.Time.CompareTo(from) >= 0 && t.Time.CompareTo(to) <= 0).OrderByDescending(v => v.Time).ToList();
                }
            }

            return tags;
        }

        public List<TagValue> GetTagValuesByName(string tagName, string sortType)
        {
            List<TagValue> tags = new List<TagValue>();

            using(var db = new DatabaseContext())
            {
                if(sortType == "asc")
                {
                    tags = db.Values.Where(t => t.TagName == tagName).OrderBy(v => v.Time).ToList();
                }
                else
                {
                    tags = db.Values.Where(t => t.TagName == tagName).OrderByDescending(v => v.Time).ToList();
                }
            }

            return tags;
        }
    }
}
