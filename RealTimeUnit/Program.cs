using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;


namespace RealTimeUnit
{
    class Program
    {
        static ServiceReference1.RealTimeUnitClient client = new ServiceReference1.RealTimeUnitClient();
        private static CspParameters csp;
        private static RSACryptoServiceProvider rsa;
        const string EXPORT_FOLDER = @"C:\public_key\";
        const string PUBLIC_KEY_FILE = @"rsaPublicKey.txt";

        static void Main(string[] args)
        {
            Console.WriteLine("### Registracija RTU ###");
            Console.WriteLine("Id >> ");
            string id = Console.ReadLine();
            Console.WriteLine("Gornja granica >> ");
            double highLimit = ValidateInt(Console.ReadLine());
            Console.WriteLine("Donja granica >> ");
            double lowLimit = ValidateInt(Console.ReadLine());
            if (highLimit == -1 || lowLimit == -1 || lowLimit > highLimit)
            {
                Console.WriteLine("Pogresan unos!");
                Console.ReadKey();
                System.Environment.Exit(1);
            }

            Console.WriteLine("Adresa RTD >> ");
            string address = Console.ReadLine();

            csp = new CspParameters();
            rsa = new RSACryptoServiceProvider(csp);

            if (!(Directory.Exists(EXPORT_FOLDER)))
                Directory.CreateDirectory(EXPORT_FOLDER);
            string path = Path.Combine(EXPORT_FOLDER, PUBLIC_KEY_FILE);
            using (StreamWriter writer = new StreamWriter(path))
            {
                writer.Write(rsa.ToXmlString(false));
            }

            string message = id + address + lowLimit + highLimit;
            byte[] signature;
            using (SHA256 sha = SHA256Managed.Create())
            {
                var hashValue = sha.ComputeHash(Encoding.UTF8.GetBytes(message));
                var formatter = new RSAPKCS1SignatureFormatter(rsa);
                formatter.SetHashAlgorithm("SHA256");
                signature = formatter.CreateSignature(hashValue);
            }

            if(!client.initialize(id, address, lowLimit, highLimit, signature, message))
            {
                Console.WriteLine("Registracija neuspesna!");
                Console.ReadKey();
                System.Environment.Exit(1);
            }
            Console.WriteLine("Uspesno dodat!");
            Console.ReadKey();
        }

        private static int ValidateInt(string v)
        {
            int intNumber;
            bool isInt = Int32.TryParse(v, out intNumber);
            if (isInt)
            {
                return intNumber;
            }
            return -1;
        }
    }
}
