using DatabaseManager.ServiceReference;
using SCADA_Core;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;

namespace DatabaseManager
{
    class Program
    {
        static ServiceReference.DatabaseManagerClient dbClient = new ServiceReference.DatabaseManagerClient();
        private static bool signedIn = false;
        private static string role;
        static void Main(string[] args)
        {
            TagProcessing.StartContextThread();
            TagProcessing.StartThreads();
            Console.WriteLine("Ucitavanje podataka...\n");
            while (true)
            {
                if (dbClient.isDatabaseEmpty())
                {
                    Registration();
                } 
                else if (signedIn)
                {
                    OptionsMenu();
                }
                else
                {
                    SignInMenu();
                }
            }
        }

        private static void SignInMenu()
        {
            Console.WriteLine("\n### Opcije ###");
            Console.WriteLine("----------------------------");
            Console.WriteLine("1. Prijava");
            Console.WriteLine("0. Prekid programa");
            int option = Choice();
            switch (option)
            {
                case 0: System.Environment.Exit(1); break;
                case 1: SignIn(); break;
                default: Console.WriteLine("Pogresan unos!"); break;
            }
        }

        private static void OptionsMenu()
        {
            Console.WriteLine("\n### Opcije ###");
            Console.WriteLine("----------------------------");
            Console.WriteLine("1. Unos vrednosti izlaznog taga");
            Console.WriteLine("2. Prikaz vrednosti izlaznih tagova");
            Console.WriteLine("3. Dodavanje ulaznog taga");
            Console.WriteLine("4. Dodavanje izlaznog taga");
            Console.WriteLine("5. Brisanje ulaznog taga");
            Console.WriteLine("6. Brisanje izlaznog taga");
            Console.WriteLine("7. On/Off skeniranje ulaznih tagova");
            Console.WriteLine("8. Dodavanje alarma");
            Console.WriteLine("9. Brisanje alarma");
            Console.WriteLine("10. Odjava");
            if (role == "admin")
            {
                Console.WriteLine("11. Dodavanje novog korisnika");
            }
            Console.WriteLine("0. Prekid programa");
            int option = Choice();
            switch (option)
            {
                case 0: System.Environment.Exit(1); break;
                case 1: ChangeOutputValue(); break;
                case 2: GetOutputValues(); break;
                case 3: AddInputTag(); break;
                case 4: AddOutputTag(); break;
                case 5: RemoveInputTag(); break;
                case 6: RemoveOutputTag(); break;
                case 7: TurnScanOnOff(); break;
                case 8: AddAlarm(); break;
                case 9: DeleteAlarm(); break;
                case 10: SignOut(); break;
                case 11: 
                    if (role == "admin")
                    {
                        Registration(); break;
                    } else
                    {
                        Console.WriteLine("Pogresan unos"); break;
                    }
                default: Console.WriteLine("Pogresan unos"); break;

            }
        }

        private static void Registration()
        {
            Console.WriteLine("\nRegistracija\n");
            Console.WriteLine("----------------------------");
            Console.WriteLine("Korisnicko ime >> ");
            string username = Console.ReadLine();
            Console.WriteLine("Lozinka >> ");
            string password = Console.ReadLine();

            if (username.Trim() == "" || password.Trim() == "")
            {
                Console.WriteLine("Pogresan unos.");
                return;
            }
            string encryptedPass = EncryptData(password);
            string role = dbClient.isDatabaseEmpty() ? "admin" : "korisnik";

            if (dbClient.Registration(username, encryptedPass, role))
            {
                Console.WriteLine("Registracija uspesno obavljena!");
            }
            else
            {
                Console.WriteLine("Registracija neuspesna!");
            }
        }

        private static void SignIn()
        {
            Console.WriteLine("\nKorisnicko ime:");
            string username = Console.ReadLine();
            Console.WriteLine("Lozinka:");
            string password = Console.ReadLine();

            if (username.Trim() == "" || password.Trim() == "")
            {
                Console.WriteLine("Pogresan unos.");
                return;
            }

            string response = dbClient.SignIn(username, password);
            if(response == "admin" || response == "korisnik")
            {
                signedIn = true;
                role = response;
            } else
            {
                Console.WriteLine(response);
            }
        }

        private static void SignOut()
        {
            signedIn = false;
            Console.WriteLine("Korisnik odjavljen!");
        }

        private static void DeleteAlarm()
        {
            Console.WriteLine("\nTag name alarma >> ");
            string tagName = Console.ReadLine();
            Console.WriteLine("Tip (low/high) alarma >> ");
            string type = Console.ReadLine();
            Console.WriteLine("Limit alarma >> ");
            string limit = Console.ReadLine();
            string response = dbClient.RemoveAlarm(type + tagName + limit);
            Console.WriteLine(response);
        }

        private static void AddAlarm()
        {
            Console.WriteLine("\nTip alarma (low/high) >> ");
            string type = Console.ReadLine();
            Console.WriteLine("Id taga za koji je namjenje alarm >> ");
            string tagName = Console.ReadLine();
            Console.WriteLine("Limit >> ");
            double limit = ValidateDouble(Console.ReadLine());
            Console.WriteLine("Priorite(1,2,3) >> ");
            int prioritet = ValidateInt(Console.ReadLine());
            if (((type != "low" && type != "high") || limit == -1 || prioritet == -1) || (prioritet != 1 && prioritet != 2 && prioritet != 3))
            {
                Console.WriteLine("Pogresan unos!");
                return;
            }
            string response = dbClient.AddAlarm(tagName, type, limit, prioritet);
            Console.WriteLine(response);
        }

        private static void TurnScanOnOff()
        {
            Console.WriteLine("\nUnesite id ulaznog taga >> ");
            string tagName = Console.ReadLine();

            string response = dbClient.TurnScanOnOff(tagName);
            Console.WriteLine(response);
        }

        private static void RemoveOutputTag()
        {
            Console.WriteLine("\nUnesite izlaznog id taga koji zelite obrisati >> ");
            string tagName = Console.ReadLine();
            string response = dbClient.RemoveOutputTag(tagName);
            Console.WriteLine(response);
        }

        private static void AddOutputTag()
        {
            Console.WriteLine("\nTip taga:\n1. Analogni\n2. Digitalni");
            int choice = Choice();
            Console.WriteLine("Naziv taga >> ");
            string tagName = Console.ReadLine();
            Console.WriteLine("Opis >> ");
            string desc = Console.ReadLine();
            Console.WriteLine("I/O Address(S,C,R) >> ");
            string addr = Console.ReadLine();
            Console.WriteLine("Pocetna vrednost >> ");
            double init;
            bool succes = Double.TryParse(Console.ReadLine(), out init);
            if (tagName.Trim() == "" || desc.Trim() == "" || addr.Trim() == "" || !succes)
            {
                Console.WriteLine("Pogresan unos!");
                return;
            }
            string response, unit = "";
            switch (choice)
            {
                case 1:
                    Console.WriteLine("Low limit >> ");
                    double lowLimit = ValidateDouble(Console.ReadLine());
                    Console.WriteLine("High limit >> ");
                    double highLimit = ValidateDouble(Console.ReadLine());
                    if (lowLimit == -1 || highLimit == -1 || lowLimit > highLimit)
                    {
                        Console.WriteLine("Pogresan unos!");
                        return;
                    }
                    if (init > highLimit || init < lowLimit)
                    {
                        Console.WriteLine("Pocetna vrednost mora biti izmedju low i high!");
                        return;
                    }
                    Console.WriteLine("Unit >> ");
                    unit = Console.ReadLine();
                    response = dbClient.AddOutputTag(tagName, desc, addr, init, lowLimit, highLimit, "analog", unit);
                    Console.WriteLine(response);
                    break;
                case 2:
                    if (init != 0 && init != 1)
                    {
                        Console.WriteLine("Pocetna vrednost moze biti samo 0 ili 1!");
                        return;
                    }
                    response = dbClient.AddOutputTag(tagName, desc, addr, init, 0, 0, "digital", unit);
                    Console.WriteLine(response);
                    break;
                default: Console.WriteLine("Tip taga ne postoji!"); break;
            }
        }

        private static void RemoveInputTag()
        {
            Console.WriteLine("\nUnesite id ulaznog taga koji zelite obrisati >> ");
            string tagName = Console.ReadLine();
            string response = dbClient.RemoveInputTag(tagName);
            Console.WriteLine(response);
        }

        private static void AddInputTag()
        {
            Console.WriteLine("\nTip taga:\n1. Analogni\n2. Digitalni");
            int choice = Choice();
            Console.WriteLine("Naziv taga >> ");
            string tagName = Console.ReadLine();
            Console.WriteLine("Opis >> ");
            string desc = Console.ReadLine();
            Console.WriteLine("Driver(SD/RTD) >> ");
            string driver = Console.ReadLine();
            if (driver == "RTD")
            {
                Console.WriteLine("I/O Address >> ");
            } else 
            {
                Console.WriteLine("I/O Address(S,C,R) >> ");
            }
            string addr = Console.ReadLine();
            Console.WriteLine("Scan time >> ");
            int scanTime;
            bool succesST = Int32.TryParse(Console.ReadLine(), out scanTime);
            if (scanTime < 1)
            {
                Console.WriteLine("Scan time mora biti veci od 0");
                return;
            }
            Console.WriteLine("On/Off scan (true/false) >> ");
            bool OnOff;
            bool successOnOFF = Boolean.TryParse(Console.ReadLine(), out OnOff);
            if(!(((tagName.Trim() == "" || desc.Trim() == "" || addr.Trim() == "") || !(succesST && successOnOFF) || (driver != "SD" && driver != "RTD")) ? false : true))
            {
                Console.WriteLine("Pogresan unos!");
                return;
            }
            string response, unit = "";
            switch (choice)
            {
                case 1:
                    int lowLimit, highLimit;
                    Console.WriteLine("Low limit >> ");
                    bool lowLimitSucces = Int32.TryParse(Console.ReadLine(), out lowLimit);
                    Console.WriteLine("High limit >> ");
                    bool highLimitSucces = Int32.TryParse(Console.ReadLine(), out highLimit);
                    if(!(lowLimitSucces && highLimitSucces))
                    {
                        Console.WriteLine("Pogresan unos!");
                        return;
                    }
                    Console.WriteLine("Unit >> ");
                    unit = Console.ReadLine();
                    response = dbClient.AddInputTag(tagName, desc, addr, driver, scanTime, OnOff, lowLimit, highLimit, "analog", unit);
                    Console.WriteLine(response);
                    break;
                case 2:
                    response = dbClient.AddInputTag(tagName, desc, addr, driver, scanTime, OnOff, 0, 0, "digital", unit);
                    Console.WriteLine(response); break;
                default: Console.WriteLine("Tip taga ne postoji!"); break;

            }
        }

        private static void GetOutputValues()
        {
            string response = dbClient.GetOutputValues();
            Console.WriteLine(response);
            Console.ReadLine();
        }

        private static void ChangeOutputValue()
        {
            Console.WriteLine("\nUnetise id Taga >> ");
            string tagName = Console.ReadLine();
            Console.WriteLine("Unesite novu vrednost >> ");
            double newValue = ValidateDouble(Console.ReadLine());

            if (newValue != -1)
            {
                string response = dbClient.ChangeOutputValue(tagName, newValue);
                Console.WriteLine(response);
            } else 
            {
                Console.WriteLine("Pogresan unos!");
                return;
            }
        }
        private static double ValidateDouble(string number)
        {
            double doubleNumber;
            bool isDouble = Double.TryParse(number, out doubleNumber);
            if (isDouble)
            {
                return doubleNumber;
            }
            return -1;
        }
        private static int ValidateInt(string number)
        {
            int intNumber;
            bool isInt = Int32.TryParse(number, out intNumber);
            if (isInt)
            {
                return intNumber;
            }
            return -1;
        }
        private static int Choice()
        {
            bool entered = false;
            int choice = -1;
            while (!entered)
            {
                entered = Int32.TryParse(Console.ReadLine(), out choice);
                if (!entered)
                {
                    Console.WriteLine("Morate izabrati opciju!");
                }
            }
            return choice;
        }
        private static string EncryptData(string valueToEncrypt)
        {
            string GenerateSalt()
            {
                RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider();
                byte[] salt = new byte[32];
                crypto.GetBytes(salt);
                return Convert.ToBase64String(salt);
            }
            string EncryptValue(string strValue)
            {
                string saltValue = GenerateSalt();
                byte[] saltedPassword = Encoding.UTF8.GetBytes(saltValue + strValue);
                using (SHA256Managed sha = new SHA256Managed())
                {
                    byte[] hash = sha.ComputeHash(saltedPassword);
                    return $"{Convert.ToBase64String(hash)}:{saltValue}";
                }
            }
            return EncryptValue(valueToEncrypt);
        }

    }
}
