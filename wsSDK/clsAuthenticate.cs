using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace x150
{
    class clsAuthenticate
    {

        public string Get_Win32_BaseBoard()
        {
            try
            {
                SelectQuery query = new SelectQuery("Win32_BaseBoard");
                ManagementObjectSearcher search = new ManagementObjectSearcher(query);
                string str_tempSHD = "";
                foreach (ManagementObject info in search.Get())
                {
                    str_tempSHD = info["SerialNumber"].ToString().Replace(" ", "");
                }
                if (str_tempSHD == "")
                {
                    str_tempSHD = GetMacAddress();
                }
                return str_tempSHD;
            }
            catch
            {
                return "";
            }
            return "";
        }

        string strPlus = "WSK+++";  //WSK+++
        public long EncryptionData(string strMac)
        {
            long sum = 0;
            int index = 1;
            string EncStrMAC = String.Format("{0}{1}", strMac, strPlus);
            try
            {
                foreach (Char ch in EncStrMAC)
                {
                    if (Char.IsDigit(ch))
                    {
                        sum += sum + (int)Char.GetNumericValue(ch) * (index * 2);
                    }
                    else if (Char.IsLetter(ch))
                    {
                        switch (ch.ToString().ToUpper())
                        {
                            case "A": sum += sum + 10 * (index * 2);
                                break;
                            case "B": sum += sum + 11 * (index * 2);
                                break;
                            case "C": sum += sum + 12 * (index * 2);
                                break;
                            case "D": sum += sum + 13 * (index * 2);
                                break;
                            case "E": sum += sum + 14 * (index * 2);
                                break;
                            case "F": sum += sum + 15 * (index * 2);
                                break;
                        }

                    }
                    index += 1;
                }
                index = 1;
                EncStrMAC = "";
                return sum;
            }
            catch
            {
                return 0;
            }

        }

        public string Generates16ByteUnique(string text)
        {
            System.Security.Cryptography.MD5 sec = new System.Security.Cryptography.MD5CryptoServiceProvider();
            System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
            byte[] bt = sec.ComputeHash(System.Text.Encoding.UTF8.GetBytes(text));

            byte b;
            int n = 0;
            int n1 = 0;
            int n2 = 0;
            string s = "";

            try
            {
                for (int i = 0; i < bt.Length; i++)
                {
                    b = bt[i];
                    n = Convert.ToInt32(b);
                    n1 = n & 15;
                    n2 = (n >> 4) & 15;

                    if (n2 > 9)
                    {
                        s += Convert.ToChar((n2 - 10) + 65);
                    }
                    else
                    {
                        s += n2.ToString();
                    }

                    if (n1 > 9)
                    {
                        s += Convert.ToChar((n1 - 10) + 65);
                    }
                    else
                    {
                        s += n1.ToString();
                    }

                    if ((i + 1) != bt.Length)
                    {
                        if (((i + 1) % 2) == 0)
                        {
                            s += "-";
                        }
                    }

                }

                return s;
            }
            catch (Exception ex)
            {
                return ex.Message.ToString();
            }


        }


        public string GetMacAddress()
        {
            string qstring = "SELECT * FROM  Win32_NetworkAdapterConfiguration where IPEnabled = true";
            try
            {
                string macaddress = "";
                foreach (System.Management.ManagementObject mo in new System.Management.ManagementObjectSearcher(qstring).Get())
                {
                    macaddress = mo["MacAddress"].ToString();
                    if ((macaddress != null) && (macaddress != "00:00:00:00:00:00"))
                    {
                        return macaddress;
                    }
                }
                return "";
            }
            catch
            {
                return "";
            }
        }

    }
}
