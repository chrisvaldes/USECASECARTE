using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Settings.Application.DTO
{
    public static class SettingKeys
    {
        public const string AUTH_MODE = "AUTH_MODE"; // DEFAULT | LDAP
        public const string LDAP_CONFIG = "LDAP_CONFIG";

        public static string Internal_Domain_key = "internal_domain";
        public static string Auth_Key = "AUTH_MODE";
        public static string Auth_Default_Key = "DEFAULT";
        public static string Auth_AD_Key = "LDAP";
        public static string APP_NAME_KEY = "APP_NAME";
        public static string COMPANY_NAME_KEY = "COMPANY_NAME";
        public static string COMPANY_WEBSITE_KEY = "COMPANY_WEBSITE";
        public static string LDAPKey = "LDAP_SETTING";
        public static string EmailKey = "MAIL_SETTING";
    }
}
