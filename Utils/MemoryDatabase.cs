using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace WebAgent.Utils
{
    public class ShorterData
    {
        public string Nounce { get; set; }
        public string WSocketID { get; set; }
        public string MessageData { get; set; }
        public int NumClients { get; set; }

        public bool IssueCredential { get; set; }

        public void IncreaseClients()
        {
            this.NumClients++;
        }

    }

    public static class MemoryDatabase
    {
        public static List<ShorterData> listado = new List<ShorterData>();
        
        public static bool AddRegister(string _nounce, string _data, bool newCred=false)
        {
            
            string randomID = GenerateRandomString(64);
            if (listado.Where(t => t.Nounce == _nounce || t.WSocketID== randomID).Count()==0)
            {
                listado.Add(new ShorterData() { Nounce = _nounce, WSocketID = randomID, MessageData = _data, NumClients = 0, IssueCredential = newCred });
                return true;
            }
            return false;
        }

        public static ShorterData GetRegisterByNonce(string nounce)
        {
            return listado.Where(t => t.Nounce == nounce).FirstOrDefault();
        }

        public static ShorterData GetRegisterByWSID(string wsid)
        {
            return listado.Where(t => t.WSocketID == wsid).FirstOrDefault();
        }

        public static bool RemoveRegister(string nounce)
        {
            return listado.Remove(GetRegisterByNonce(nounce));
        }

        public static string GenerateRandomString(int length)
        {

            const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            StringBuilder res = new StringBuilder();
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                byte[] uintBuffer = new byte[sizeof(uint)];

                while (length-- > 0)
                {
                    rng.GetBytes(uintBuffer);
                    uint num = BitConverter.ToUInt32(uintBuffer, 0);
                    res.Append(valid[(int)(num % (uint)valid.Length)]);
                }
            }

            return res.ToString();
        }

    }

  

}
