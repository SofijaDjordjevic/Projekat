using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading;

namespace SCADA_Core
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "DatabaseManagerService" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select DatabaseManagerService.svc or DatabaseManagerService.svc.cs at the Solution Explorer and start debugging.
    public class DatabaseManager : IDatabaseManager
    {

        public bool isDatabaseEmpty()
        {
            using (var db = new DatabaseContext())
            {
                return !db.Users.Any();
            }
        }

        public bool Registration(string username, string password, string role)
        {
            if (UserProcessing.UserExists(username))
            {
                return false;
            }
            UserProcessing.AddUser(username, password, role);
            return true;
        }

        public string SignIn(string username, string password)
        {
            if(UserProcessing.UserExists(username))
            {
                if (UserProcessing.ValidateEncryptedData(username, password))
                {
                    return UserProcessing.GetUser(username).Role;
                }
                return "Lozinka nije dobra!";
            }
            return "Korisnicko ime ne postoji!";
        }
        public string AddInputTag(string tagName, string desc, string io, string driver, int scanTime, bool onOffScan, double lowLimit, double highLimit, string type, string unit)
        {
            if (TagProcessing.inputTags.ContainsKey(tagName))
            {
                return "Tag sa tim id-em postoji!";
            }
            if(!TagProcessing.DriverAddressValidation(driver, io))
            {
                return "Driver nije dobar!";
            }
            if(type == "digital")
            {
                DigitalInput di = new DigitalInput(tagName, desc, io, driver, scanTime, onOffScan, type);
                Thread t = TagProcessing.AddInputTag(di);
                t.Start();
            } else
            {
                AnalogInput ai = new AnalogInput(tagName, desc, io, driver, scanTime, onOffScan, type, lowLimit, highLimit, unit);
                Thread t = TagProcessing.AddInputTag(ai);
                t.Start();
            }
            TagProcessing.UpdateScadaConfig();
            return "Tag uspesno dodat!";

        }

        public string RemoveInputTag(string tagName)
        {
            if (!TagProcessing.inputTags.ContainsKey(tagName))
            {
                return "Tag sa tim ID-em ne postoji!";
            }
            TagProcessing.RemoveInputTag(tagName);
            return "Tag uspesno obrisan!";
        }

        public string AddOutputTag(string tagName, string desc, string io, double init, double lowLimit, double highLimit, string type, string unit)
        {
            if (TagProcessing.outputTags.ContainsKey(tagName))
            {
                return "Tag sa tim Id-em postoji!";
            }
            if(type == "digital")
            {
                DigitalOutput DO = new DigitalOutput(tagName, desc, io, init, type);
                TagProcessing.AddOutputTag(DO);
            }
            else
            {
                AnalogOutput AO = new AnalogOutput(tagName, desc, io, init, lowLimit, highLimit, unit, type);
                TagProcessing.AddOutputTag(AO);
            }
            TagProcessing.UpdateScadaConfig();
            return "Tag uspesno dodat!";
        }

        public string RemoveOutputTag(string tagName)
        {
            if (!TagProcessing.outputTags.ContainsKey(tagName))
            {
                return "Tag sa tim ID-em ne postoji!";
            }
            TagProcessing.RemoveOutputTag(tagName);
            return "Tag uspesno obrisan!";
        }

        public string AddAlarm(string tagName, string type, double limit, int priority)
        {
            if (TagProcessing.inputTags.ContainsKey(tagName) && !TagProcessing.alarms.ContainsKey(type + tagName + limit))
            {
                if (TagProcessing.inputTags[tagName].Type == "analog")
                {
                    AnalogInput ai = (AnalogInput)TagProcessing.inputTags[tagName];
                    if (type == "low" && limit < ai.LowLimit)
                    {
                        return "Neuspesno dodavanje alarma. Limit alarma je manji od limita taga";
                    }
                    else if (type == "high" && limit > ai.HighLimit)
                    {
                        return "Neuspesno dodavanje alarma. Limit alarma je veci od limita taga";
                    }
                    TagProcessing.AddAlarm(new Alarm(type, priority, tagName, limit));
                    TagProcessing.UpdateAlarmConfig();
                    return "Alarm uspesno dodat!";
                }
                return "Tag sa tim ID-em nije analogni!";
            }
            return "Tag sa tim ID-em ne postoji ili vec postoji takav alarm!";
        }
        public string RemoveAlarm(string id)
        {
            if (TagProcessing.alarms.ContainsKey(id))
            {
                TagProcessing.RemoveAlarm(id);
                TagProcessing.UpdateAlarmConfig();
                return "Alarm uspesno obrisan!";
            }
            return "Ne posotji alarm sa tim ID-em!";
        }

        public string ChangeOutputValue(string tagName, double value)
        {
            if (!TagProcessing.outputTags.ContainsKey(tagName))
            {
                return "Tag sa tim ID-em ne postoji!";
            }
            return TagProcessing.ChangeOutputValue(tagName, value);
        }

        public string GetOutputValues()
        {
            string response = "";
            foreach(OutputTag o in TagProcessing.outputTags.Values)
            {
                response += "----------------------------------------------------------------------------------\n";
                response += $"Tag: {o.TagName}, Address: {o.IOAddress}, Value: {o.InitialValue}, Type: {o.Type}\n";
            }
            return response;
        }

        public string TurnScanOnOff(string tagName)
        {
            if (!TagProcessing.inputTags.ContainsKey(tagName))
            {
                return "Tag sa tim ID-em ne postoji!";
            }

            bool scan = TagProcessing.inputTags[tagName].OnOffScan;
            TagProcessing.inputTags[tagName].OnOffScan = !scan;
            TagProcessing.UpdateScadaConfig();
        
            if (scan)
            {
                return "Tag uspesno iskljucen!";
            }
            else
            {
                return "Tag uspesno ukljucen!";
            }
        }

    }
}
