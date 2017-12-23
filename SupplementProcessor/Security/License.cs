using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppSoftware.LicenceEngine.Common;
using AppSoftware.LicenceEngine.KeyVerification;

namespace SupplementProcessor.Security
{
    internal static class License
    {
        const int SETLEN = 16;
        static KeyByteSet[] keyByteSets = new[]
        {
            new KeyByteSet(1, 46, 192, 9),
            new KeyByteSet(6, 125, 23, 78), 
            new KeyByteSet(9, 58, 6, 97),
            new KeyByteSet(15, 50, 126, 197)
        };
        internal static event EventHandler OnChanged;
        static string m_LicenseName;
        static string m_LicenseEmail;
        static string m_LicenseCode;
        internal static string LicenseName { set { if (OnChanged != null) OnChanged(m_LicenseName, null); m_LicenseName = value; } get { return m_LicenseName; } }
        internal static string LicenseEmail { set { if (OnChanged != null) OnChanged(m_LicenseEmail, null); m_LicenseEmail = value; } get { return m_LicenseEmail; } }
        internal static string LicenseCode { set { if (OnChanged != null) OnChanged(m_LicenseCode, null); m_LicenseCode = value; } get { return m_LicenseCode; } }

        internal static void SetLicense(string licenseName, string licenseEmail, string licenseCode)
        {
            m_LicenseName = licenseName;
            m_LicenseEmail = licenseEmail;
            m_LicenseCode = licenseCode;

           
        }

        internal static bool CheckKey(string key)
        {
            var pkvKeyCheck = new PkvKeyCheck();
            if (pkvKeyCheck.CheckKey(key,
                                    new[] { keyByteSets[0], keyByteSets[1], keyByteSets[2] },
                                    SETLEN,
                                    null) == PkvLicenceKeyResult.KeyGood)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
