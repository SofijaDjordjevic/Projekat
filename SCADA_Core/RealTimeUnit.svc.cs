using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.ServiceModel;
using System.Text;
using System.Threading;

namespace SCADA_Core
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "RealTimeUnit" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select RealTimeUnit.svc or RealTimeUnit.svc.cs at the Solution Explorer and start debugging.
    public class RealTimeUnit : IRealTimeUnit
    {
        static readonly object locker = new object();
        static List<string> driverIDs = new List<string>();

        private CspParameters csp;
        private RSACryptoServiceProvider rsa;

        const string IMPORT_FOLDER = @"C:\public_key\";
        const string PUBLIC_KEY_FILE = @"rsaPublicKey.txt";

        public RealTimeUnit()
        {
            ImportPublicKeys();
        }

        private void ImportPublicKeys()
        {
            string path = Path.Combine(IMPORT_FOLDER, PUBLIC_KEY_FILE);
            FileInfo fi = new FileInfo(path);

            if (fi.Exists)
            {
                using (StreamReader reader = new StreamReader(path))
                {
                    csp = new CspParameters();
                    rsa = new RSACryptoServiceProvider(csp);
                    string publicKeyText = reader.ReadToEnd();
                    rsa.FromXmlString(publicKeyText);
                }
            }
        }

        public bool initialize(string id, string io, double lowLimit, double highLimit, byte[] signature, string message)
        {
            if (RealTimeDriver.values.ContainsKey(io) || driverIDs.Contains(id))
            {
                return false;
            }
            if (ValidateMessage(message, signature))
            {
                Thread t = new Thread(() => { doWork(id, io, lowLimit, highLimit); });
                t.Start();
                driverIDs.Add(id);
                return true;
            }
            return false;
        }

        private static void doWork(string id, string io, double lowLimit, double highLimit)
        {
            double newValue = 0;
            Random r = new Random();
            while (true)
            {
                lock(locker)
                {
                    newValue = r.NextDouble() * (highLimit - lowLimit) + lowLimit;
                    RealTimeDriver.values[io] = newValue;
                }

                Thread.Sleep(5000);
                
            }
        }

        private bool ValidateMessage(string message, byte[] signature)
        {
            using (var sha = SHA256Managed.Create())
            {
                var hashValue = sha.ComputeHash(Encoding.UTF8.GetBytes(message));

                var deformatter = new RSAPKCS1SignatureDeformatter(rsa);
                deformatter.SetHashAlgorithm("SHA256");

                return deformatter.VerifySignature(hashValue, signature);
            }
        }

        public void ValueToAddres(string io, int number)
        {
            lock (locker)
            {
                RealTimeDriver.values[io] = number;
            }
        }
    }
}
