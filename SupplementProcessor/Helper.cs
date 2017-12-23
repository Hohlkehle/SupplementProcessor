using SupplementProcessor.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SupplementProcessor
{
    public class Helper
    {
        [StructLayout(LayoutKind.Sequential)]
        internal struct Win32Point
        {
            public Int32 X;
            public Int32 Y;
        };

        public static string LastError { set; get; }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(ref Win32Point pt);

        public static Point GetMousePosition()
        {
            Win32Point w32Mouse = new Win32Point();
            GetCursorPos(ref w32Mouse);
            return new Point(w32Mouse.X, w32Mouse.Y);
        }

        public static bool IsNoneBinding(string value)
        {
            return value == "Нет" || value == "None" || string.IsNullOrEmpty(value);
        }

        public static T Clamp<T>( T val, T min, T max) where T : IComparable<T>
        {
            if (val.CompareTo(min) < 0) return min;
            else if (val.CompareTo(max) > 0) return max;
            else return val;
        }

        public static double ToEmSize(double sizeInPoints, double dpi)
        {
            return Math.Ceiling(sizeInPoints * (dpi / 72.0));
        }

        public static double ToPointSize(double sizeInEm, double dpi)
        {
            return Math.Floor(sizeInEm / (dpi / 72.0));
        }

        public static float Lerp(float a, float b, float t)
        {
            return a + (b - a) * t;
        }

        public static double Lerp(double a, double b, double t)
        {
            return a + (b - a) * t;
        }


        public static string[] digitsUkr =
        {
            "зараховано",
            "один",
            "два",
            "три",
            "чотири",
            "п’ять",
            "шість",
            "сім",
            "вісім",
            "дев’ять",
            "десять",
            "одинадцать",
            "дванадцять"
        };

        public static string[][] educationRecived =
        { 
            new string[] 
            {
                "вся",
                "лась"
            },
            new string[] 
            {
                "в",
                "ла"
            }
        };

        public static string[][] assessmentsRecived =
        { 
            new string[] 
            {
                "в",
                "ла"
            },
            new string[] 
            {
                "ов",
                "ла"
            }
        };

        public static string[][] assessmentsGender =
        { 
            new string[] 
            {
                "звільнений",
                "не атестований"
            },
            new string[] 
            {
                "звільнена",
                "не атестована"
            }
        };

        public static string FormatAssessments(string assessments, int gender)
        {
            string outstr = "";

            if (assessments == "")
                return outstr;

            if (assessments.ToLower() == "зв")
            {
                return assessmentsGender[gender][0];
            }
            else if (assessments.ToLower() == "зар")
            {
                return digitsUkr[0];
            }
            else if (assessments.ToLower() == "на")
            {
                return assessmentsGender[gender][1];
            }
            else
            {
                int digit = 0;
                double ddigit = 0;
                if (!int.TryParse(assessments, out digit))
                {
                    if (!double.TryParse(assessments, NumberStyles.Integer | NumberStyles.AllowThousands | NumberStyles.AllowDecimalPoint, new CultureInfo("uk-UA"), out ddigit))
                   { //MessageBox.Show("Unable to parse assessments");
                       if (!double.TryParse(assessments, NumberStyles.Integer | NumberStyles.AllowThousands | NumberStyles.AllowDecimalPoint, new CultureInfo("en-US"), out ddigit))
                           LastError = "Unable to parse assessments '" + assessments + "'";
                     }
                }

                if (digit != 0)
                    outstr = string.Format("{0} ({1})", digit == 0 ? ddigit : digit, digitsUkr[digit]);
                if (ddigit != 0)
                    outstr = string.Format(System.Globalization.CultureInfo.GetCultureInfo("uk-UA"), "{0:0.0}", ddigit);
           
            }

            return outstr;
        }

        internal static string FormatAssessments(string assessments, int gender, SupplementFormatingInfo supplementFormatingInfo)
        {
            string outstr = "";

            if (assessments == "")
                return outstr;

            if (assessments.ToLower() == "зв")
            {
                return assessmentsGender[gender][0];
            }
            else if (assessments.ToLower() == "зар")
            {
                return digitsUkr[0];
            }
            else if (assessments.ToLower() == "на")
            {
                return assessmentsGender[gender][1];
            }
            else
            {
                int intDigit = 0;
                double doubleDigit = 0;
                if (!int.TryParse(assessments, out intDigit))
                {
                    if (!double.TryParse(assessments, NumberStyles.Integer | NumberStyles.AllowThousands | NumberStyles.AllowDecimalPoint, new CultureInfo("uk-UA"), out doubleDigit))
                    { 
                        //MessageBox.Show("Unable to parse assessments '" + assessments + "'"); 
                        if (!double.TryParse(assessments, NumberStyles.Integer | NumberStyles.AllowThousands | NumberStyles.AllowDecimalPoint, new CultureInfo("en-US"), out doubleDigit))
                            LastError = "Unable to parse assessments '" + assessments + "'";
                    }
                }

                var format = supplementFormatingInfo.AssessmentByWordsOnly ? 
                    "{1}" : 
                    "{0} ({1})";

                if (intDigit != 0)
                    outstr = string.Format(format, intDigit == 0 ? doubleDigit : intDigit, digitsUkr[intDigit]);

                // Средний бал.
                if (doubleDigit != 0)
                    outstr = string.Format(System.Globalization.CultureInfo.GetCultureInfo("uk-UA"), "{0:0.0}", doubleDigit);
               
            }

            return outstr;
        }

        public static string FormatDigit(int digit)
        {
            if (digit >= digitsUkr.Length)
                digit = digitsUkr.Length - 1;

            if (digit <= 0)
                return string.Format("{0}", digitsUkr[digit]);

            return string.Format("{0} ({1})", digit, digitsUkr[digit]);
        }

        public static string FormatDigit(string digit)
        {
            //if (!digitsUkr.Contains(digit))
            //    digit = (digitsUkr.Length - 1).ToString();
            string outstr = "";
            try { outstr = string.Format("{0} ({1})", digit, digitsUkr[int.Parse(digit)]); }
            catch (Exception) { }
            return outstr;
        }

        public static T DeepClone<T>(T from)
        {
            using (MemoryStream s = new MemoryStream())
            {
                BinaryFormatter f = new BinaryFormatter();
                f.Serialize(s, from);
                s.Position = 0;
                object clone = f.Deserialize(s);

                return (T)clone;
            }
        }

        public static string UppercaseFirst(string s)
        {
            // Check for empty string.
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }
            // Return char and concat substring.
            return char.ToUpper(s[0]) + s.Substring(1);
        }

        public static bool CheckForInternetConnectionPing(string host = "google.com")
        {
            try
            {
                var myPing = new System.Net.NetworkInformation.Ping();
                byte[] buffer = new byte[32];
                int timeout = 1000;
                var pingOptions = new System.Net.NetworkInformation.PingOptions();
                var reply = myPing.Send(host, timeout, buffer, pingOptions);

                return reply.Status == System.Net.NetworkInformation.IPStatus.Success;
            }
            catch
            {
                return false;
            }
        }

        public static bool CheckForInternetConnection()
        {
            try
            {
                using (var client = new WebClient())
                using (var stream = client.OpenRead("http://www.google.com"))
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public static string GetProcessorID()
        {
            //var mbs = new ManagementObjectSearcher("Select ProcessorId From Win32_processor");
            //ManagementObjectCollection mbsList = mbs.Get();
            //string id = "";
            //foreach (ManagementObject mo in mbsList)
            //{
            //    id = mo["ProcessorId"].ToString();
            //    break;
            //}
            return "id";
        }

        public static class NTP
        {
            public static DateTime GetNetworkTime(string ntpServer = "time.windows.com")
            {
                // NTP message size - 16 bytes of the digest (RFC 2030)
                var ntpData = new byte[48];

                //Setting the Leap Indicator, Version Number and Mode values
                ntpData[0] = 0x1B; //LI = 0 (no warning), VN = 3 (IPv4 only), Mode = 3 (Client Mode)

                var addresses = Dns.GetHostEntry(ntpServer).AddressList;

                //The UDP port number assigned to NTP is 123
                var ipEndPoint = new IPEndPoint(addresses[0], 123);
                //NTP uses UDP
                var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                socket.Connect(ipEndPoint);

                //Stops code hang if NTP is blocked
                socket.ReceiveTimeout = 3000;

                socket.Send(ntpData);
                socket.Receive(ntpData);
                socket.Close();

                //Offset to get to the "Transmit Timestamp" field (time at which the reply 
                //departed the server for the client, in 64-bit timestamp format."
                const byte serverReplyTime = 40;

                //Get the seconds part
                ulong intPart = BitConverter.ToUInt32(ntpData, serverReplyTime);

                //Get the seconds fraction
                ulong fractPart = BitConverter.ToUInt32(ntpData, serverReplyTime + 4);

                //Convert From big-endian to little-endian
                intPart = SwapEndianness(intPart);
                fractPart = SwapEndianness(fractPart);

                var milliseconds = (intPart * 1000) + ((fractPart * 1000) / 0x100000000L);

                //**UTC** time
                var networkDateTime = (new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc)).AddMilliseconds((long)milliseconds);

                return networkDateTime.ToLocalTime();
            }

            // stackoverflow.com/a/3294698/162671
            static uint SwapEndianness(ulong x)
            {
                return (uint)(((x & 0x000000ff) << 24) +
                               ((x & 0x0000ff00) << 8) +
                               ((x & 0x00ff0000) >> 8) +
                               ((x & 0xff000000) >> 24));
            }
        }
    }
}
