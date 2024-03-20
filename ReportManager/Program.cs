using ReportManager.ServiceReference1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportManager
{
    class Program
    {
        static ServiceReference1.ReportManagerServiceClient client = new ServiceReference1.ReportManagerServiceClient();
        static void Main(string[] args)
        {
            while(true)
            {
                Console.WriteLine("\n#### Opcije ####");
                Console.WriteLine("1. Svi alarmi koji su se desili u odredjenom vremenskom periodu (sortiranje: prioritet, vreme)");
                Console.WriteLine("2. Svi alarmi odredjenog prioriteta (sortiranje: vreme)");
                Console.WriteLine("3. Sve vrednosti tagova koje su dospele na servis u odredjenom vremenskom periodu (sortiranje: vreme)");
                Console.WriteLine("4. Poslednja vrednost svih AI tagova (sortiranje: vreme)");
                Console.WriteLine("5. Poslednja vrednost svih DI tagova (sortiranje: vreme)");
                Console.WriteLine("6. Sve vrednosti taga sa odredjenim identifikatorom (sortiranje: vrednosti)");
                Console.WriteLine("0 <-- EXIT");
                int option = Choice();
                switch (option)
                {
                    case 0: System.Environment.Exit(1); break;
                    case 1: DisplayAlarmsByDate(); break;
                    case 2: DisplayAlarmsByPriority(); break;
                    case 3: DisplayTagsByDate(); break;
                    case 4: DisplayAnalogInputs(); break;
                    case 5: DisplayDigitalInputs(); break;
                    case 6: DisplayTagById(); break;
                    default:
                        {
                            Console.WriteLine("Nepostojeca opcija!");
                            break;
                        }
                }
            }
        }

        private static void DisplayTagById()
        {
            Console.WriteLine("\nUnesite ID taga >> ");
            string tagName = Console.ReadLine();

            Console.WriteLine("Sortiraj po vrednosti:\n1. Rastuce\n2. Opadajuce");
            string sortType = GetSortType();

            if (sortType == "error")
            {
                Console.WriteLine("Pogresan unos!");
                return;
            }

            ServiceReference1.TagValue[] tags = client.GetTagValuesByName(tagName, sortType);
            if (tags.Length == 0)
            {
                Console.WriteLine("Nema tagova!");
            }
            DisplayTags(tags);
            Console.ReadKey();
        }

        private static string GetSortType()
        {
            string sortType = Console.ReadLine();
            if (sortType == "1")
            {
                return "asc";
            }
            if (sortType == "2")
            {
                return "desc";
            }
            return "error";
        }

        private static void DisplayDigitalInputs()
        {
            Console.WriteLine("\nSortiranje:\n1. Rastuce\n2. Opadajuce");
            string sortType = GetSortType();
            if(sortType == "error")
            {
                Console.WriteLine("Pogresan unos!");
                return;
            }

            TagValue[] tags = client.GetLastValuesDI(sortType);
            if (tags.Length == 0)
            {
                Console.WriteLine("Nema tagova!");
            }
            DisplayTags(tags);
            Console.ReadKey();
        }

        private static void DisplayAnalogInputs()
        {
            Console.WriteLine("\nSortiranje:\n1. Rastuce\n2. Opadajuce");
            string sortType = GetSortType();
            if (sortType == "error")
            {
                Console.WriteLine("Pogresan unos!");
                return;
            }

            TagValue[] tags = client.GetLastValuesAI(sortType);
            if (tags.Length == 0)
            {
                Console.WriteLine("Nema tagova!");
            }
            DisplayTags(tags);
            Console.ReadKey();
        }

        private static void DisplayTagsByDate()
        {
            Console.WriteLine("\nnesite datum od (dd/MM/yyy HH:mm): ");
            string dateFrom = Console.ReadLine();
            Console.WriteLine("Unesite datum do (dd/MM/yyy HH:mm): ");
            string dateTo = Console.ReadLine();

            DateTime dtFrom, dtTo;

            if(DateTime.TryParse(dateFrom, out dtFrom) && DateTime.TryParse(dateTo, out dtTo))
            {
                Console.WriteLine("Sortiraj:\n1. Rastuce\n2. Opadajuce");
                string sortType = GetSortType();
                if (sortType == "error")
                {
                    Console.WriteLine("Pogresan unos!");
                    return;
                }

                TagValue[] tags = client.GetTagsInTimeRange(dtFrom, dtTo, sortType);
                if (tags.Length == 0)
                {
                    Console.WriteLine("Nema tagova!");
                }
                DisplayTags(tags);
            } else
            {
                Console.WriteLine("Pogresan unos!");
            }
            Console.ReadKey();

        }

        private static void DisplayAlarmsByPriority()
        {
            Console.WriteLine("\nUnesite prioritet >> ");
            int prioritet;
            bool success = Int32.TryParse(Console.ReadLine(), out prioritet);

            if (success)
            {
                if(prioritet != 1 && prioritet != 2 && prioritet != 3)
                {
                    Console.WriteLine("Pogresan unos!");
                    return;
                }
                Console.WriteLine("Sortiraj po vremenu:\n1.Rastuce\n2.Opadajuce");
                string sortType = GetSortType();
                if (sortType == "error")
                {
                    Console.WriteLine("Pogresan unos!");
                    return;
                }
                AlarmValue[] alarms = client.GetAlarmsByPriority(prioritet, sortType);
                if (alarms.Length == 0)
                {
                    Console.WriteLine("Nema alarma!");
                }
                foreach (var alarm in alarms)
                {
                    Console.WriteLine("-------------------------------------------");
                    Console.WriteLine($"Tag name: {alarm.TagName}, Limit: {alarm.Limit}, Trigger value: {alarm.TriggerValue}, Priority: {alarm.Priority}, Time: {alarm.Time}");
                }
            } else
            {
                Console.WriteLine("Pogresan unos!");
            }
            Console.ReadKey();
        }

        private static void DisplayAlarmsByDate()
        {
            Console.WriteLine("\nUnesite datum od (dd/MM/yyyy HH:mm):");
            string dateFrom = Console.ReadLine();
            Console.WriteLine("Unesite datum do (dd/MM/yyyy HH:mm):");
            string dateTo = Console.ReadLine();

            DateTime dtFrom, dtTo;

            if (DateTime.TryParse(dateFrom, out dtFrom) && DateTime.TryParse(dateTo, out dtTo))
            {
                Console.WriteLine("Sortiraj po:\n1. Prioritetu\n2. Vremenu");
                string sortParam = Console.ReadLine();
                if (sortParam == "1")
                {
                    sortParam = "prioritet";
                }
                else if (sortParam == "2")
                {
                    sortParam = "vreme";
                }
                else
                {
                    Console.WriteLine("Pogresan unos!");
                }

                Console.WriteLine("Sortiraj:\n1. Rastuce\n2. Opadajuce");
                string sortType = GetSortType();
                if (sortType == "error")
                {
                    Console.WriteLine("Pogresan unos!");
                    return;
                }

                AlarmValue[] alarms = client.GetAlarmsInTimeRange(dtFrom, dtTo, sortParam, sortType);
                if (alarms.Length == 0)
                {
                    Console.WriteLine("Nema alarma!");
                }
                foreach(var alarm in alarms)
                {
                    Console.WriteLine("-------------------------------------------");
                    Console.WriteLine($"Tag name: {alarm.TagName}, Limit: {alarm.Limit}, Trigger value: {alarm.TriggerValue}, Priority: {alarm.Priority}, Time: {alarm.Time}");
                }

            } else
            {
                Console.WriteLine("Pogresan unos datuma!");
            }
            Console.ReadKey();
        }

        private static int Choice()
        {
            bool success = false;
            int chosen = -1;
            while (!success)
            {
                success = Int32.TryParse(Console.ReadLine(), out chosen);
                if (!success)
                {
                    Console.WriteLine("Pogresan format unosa. Pokusajte ponovo");
                }
            }
            return chosen;
        }
        private static void DisplayTags(TagValue[] tags)
        {
            foreach (var tag in tags)
            {
                Console.WriteLine("-------------------------------------------");
                Console.WriteLine($"Tag name: {tag.TagName}, Tag: {tag.Tag}, Type: {tag.Type}, Value: {tag.Value}, Time: {tag.Time}");
            }
        }

    }
}
