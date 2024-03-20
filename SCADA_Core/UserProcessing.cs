using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace SCADA_Core
{
    public class UserProcessing
    {
        internal static bool UserExists(string username)
        {
            using(var db = new DatabaseContext())
            {
                if (db.Users.Where(user => user.Username == username).FirstOrDefault() != null)
                {
                    return true;
                }
                return false;
            }
        }

        internal static void AddUser(string username, string password, string role)
        {
            using (var db = new DatabaseContext())
            {
                User user = new User(username, password, role);
                db.Users.Add(user);
                db.SaveChanges();
            }
        }

        internal static bool ValidateEncryptedData(string username, string password)
        {
            var user = GetUser(username);
            string[] arrValues = user.Password.Split(':');
            string encryptedDbValue = arrValues[0];
            string salt = arrValues[1];
            byte[] saltedValue = Encoding.UTF8.GetBytes(salt + password);
            using (var sha = new SHA256Managed())
            {
                byte[] hash = sha.ComputeHash(saltedValue);
                string enteredValueToValidate = Convert.ToBase64String(hash);
                return encryptedDbValue.Equals(enteredValueToValidate);
            }
        }

        internal static User GetUser(string username)
        {
            using( var db = new DatabaseContext())
            {
                return db.Users.Where(user => user.Username == username).FirstOrDefault();
            }
        }
    }
}