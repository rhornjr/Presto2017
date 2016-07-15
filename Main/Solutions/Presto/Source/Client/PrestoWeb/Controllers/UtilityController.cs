using System;
using System.Configuration;
using System.Web.Configuration;
using System.Web.Http;
using System.Web.Http.Cors;
using Xanico.Core;

namespace PrestoWeb.Controllers
{
    // The origin is the web server.
    [EnableCors(origins: "http://apps.firstsolar.com", headers: "*", methods: "*")]
    public class UtilityController : ApiController
    {
        [Route("api/utility/getServiceAddress")]
        public string GetServiceAddress()
        {
            return ConfigurationManager.AppSettings["prestoServiceAddress"];
        }

        [Route("api/utility/getAuthorizationSettings")]
        public AuthorizationSetting GetAuthorizationSettings()
        {
            try
            {
                var config = WebConfigurationManager.OpenWebConfiguration("~");
                var section = config.GetSection("system.web/authorization") as AuthorizationSection;

                var authSetting = new AuthorizationSetting();

                // Don't evaluate the last rule because it seems to always be to allow *.
                int count = 1;
                int numberOfRules = section.Rules.Count;
                foreach (AuthorizationRule rule in section.Rules)
                {
                    if (count == numberOfRules) { break; }

                    if (rule.Action.ToString().ToLower() == "allow")
                    {
                        authSetting.AllowedRoles = string.Concat(rule.Roles);
                        authSetting.AllowedUsers = string.Concat(rule.Users);
                    }

                    if (rule.Action.ToString().ToLower() == "deny")
                    {
                        authSetting.DeniedRoles = string.Concat(rule.Roles);
                        authSetting.DeniedUsers = string.Concat(rule.Users);
                    }

                    count++;
                }

                return authSetting;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                throw Helper.CreateHttpResponseException(ex, "Error Getting Authorization Settings");
            }
        }

        public class AuthorizationSetting
        {
            public string AllowedUsers { get; set; }
            public string AllowedRoles { get; set; }
            public string DeniedUsers { get; set; }
            public string DeniedRoles { get; set; }
        }
    }
}