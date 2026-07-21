using Authorization.Domain.Entities;
using Settings.Application.DTO;
using Settings.Domain.Entities;
using System;
using System.Collections.Generic;
//using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Users.Domain.Entities;

namespace Infrastructure.Persistence.Seeders
{
    public class SettingSeeder
    {
        private readonly ApplicationDbContext _context;

        public SettingSeeder(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task SeedAsync()
        {
            if (!_context.Settings.Any())
            {
                // Création des données de configuration
                var settings = new[]
                {
                    new Setting { Key = SettingKeys.APP_NAME_KEY, Value = "BASE PROJECT"},
                    new Setting { Key = SettingKeys.COMPANY_NAME_KEY, Value = "Société Générale Cameroun"},
                    new Setting { Key = SettingKeys.COMPANY_WEBSITE_KEY, Value = "https://societegenerale.cm/fr/"},
                    new Setting { Key = SettingKeys.Auth_Key, Value = SettingKeys.Auth_Default_Key},
                    // LDAP : ElementConnectLDAP 1:email et 2:matricule
                    new Setting {
                        Key = SettingKeys.LDAPKey,
                        Value = @"
                        {
                          ""LDAPDirectory"": ""LDAP://"",
                          ""LDAPDomain"": """",
                          ""LDAPEmail"": """",
                          ""LDAPMatricule"": """",
                          ""LDAPPassword"": """",
                          ""ElementConnectLDAP"": ""2""
                        }"
                    },
                    new Setting {
                        Key = SettingKeys.EmailKey,
                        Value = @"
                        {
                          ""SmtpHost"": ""sandbox.smtp.mailtrap.io"",
                          ""SmtpPort"": 587,
                          ""SmtpUser"": ""dfa385dd4a1e52"",
                          ""SmtpPassword"": ""a4145b38bc5af4"",
                          ""FromEmail"": ""report@socgen.com""
                        }"
                    }
                };
                _context.Settings.AddRange(settings);
                _context.SaveChanges();
            }
        }
    }
}
