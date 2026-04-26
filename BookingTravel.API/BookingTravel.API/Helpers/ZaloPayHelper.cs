using System;
using System.Security.Cryptography;
using System.Text;

namespace BookingTravel.API.Helpers
{
    public class ZaloPayHelper
    {
        public static string CreateMac(string data, string key)
        {
            try
            {
                byte[] keyByte = Encoding.UTF8.GetBytes(key);
                byte[] messageBytes = Encoding.UTF8.GetBytes(data);
                using (var hmacsha256 = new HMACSHA256(keyByte))
                {
                    byte[] hashmessage = hmacsha256.ComputeHash(messageBytes);
                    return BitConverter.ToString(hashmessage).Replace("-", "").ToLower();
                }
            }
            catch (Exception)
            {
                return "";
            }
        }
    }
}
