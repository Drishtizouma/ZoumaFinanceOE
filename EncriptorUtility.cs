using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace ZoumaFinance
{
    public static class EncriptorUtility
    {
        private const string encriptionKey = "kZe13KlAdsee44erxll";

        public static string Encrypt(this string strToEncrypt, bool bURLEncode = true)
        {
            try
            {
                string sOutPut = string.Empty;
                var objDESCrypto = TripleDES.Create();
                var objHashMD5 = MD5.Create();
                byte[] byteHash, byteBuff;
                byteHash = objHashMD5.ComputeHash(ASCIIEncoding.ASCII.GetBytes(encriptionKey));
                objHashMD5 = null;
                objDESCrypto.Key = byteHash;
                objDESCrypto.Mode = CipherMode.ECB;
                byteBuff = ASCIIEncoding.ASCII.GetBytes(strToEncrypt);
                sOutPut = Convert.ToBase64String(objDESCrypto.CreateEncryptor().
                TransformFinalBlock(byteBuff, 0, byteBuff.Length));

                if (bURLEncode)
                    sOutPut = HttpUtility.UrlEncode(sOutPut);
                return sOutPut;
            }
            catch (Exception)
            {
                return "";
            }
        }
        public static string Encrypt(this int intToEncrypt, bool bURLEncode = true)
        {
            string strToEncrypt = intToEncrypt.ToString();
            return strToEncrypt.Encrypt(bURLEncode);
        }
        public static string Encrypt(this int? intToEncrypt, bool bURLEncode = true)
        {
            if (!intToEncrypt.HasValue)
                return string.Empty;
            return intToEncrypt.Value.ToString().Encrypt(bURLEncode);
        }

        public static string Decrypt(this string strEncrypted, bool bURLDecode = true)
        {
            if (string.IsNullOrWhiteSpace(strEncrypted))
                return string.Empty;
            try
            {
                if (bURLDecode)
                {
                    strEncrypted = HttpUtility.UrlDecode(strEncrypted);
                    if (strEncrypted.Contains((string)" "))
                        strEncrypted = strEncrypted.Replace(" ", "+");
                }
                var objDESCrypto = TripleDES.Create();
                var objHashMD5 = MD5.Create();
                byte[] byteHash, byteBuff;
                byteHash = objHashMD5.ComputeHash(ASCIIEncoding.ASCII.GetBytes(encriptionKey));
                objHashMD5 = null;
                objDESCrypto.Key = byteHash;
                objDESCrypto.Mode = CipherMode.ECB;
                byteBuff = Convert.FromBase64String(strEncrypted);
                string strDecrypted = ASCIIEncoding.ASCII.GetString(objDESCrypto.CreateDecryptor().TransformFinalBlock(byteBuff, 0, byteBuff.Length));
                objDESCrypto = null;
                return strDecrypted;
            }
            catch (Exception)
            {
                return "";
            }
        }
    }

}
