using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xaml;

namespace PGas_v2._0._0
{
    public class UserAccount
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Service { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("login")]
        public string Login { get; set; }

        [JsonProperty("crypt_password")]
        public string Password { get; set; }

        [JsonProperty("advanced_auth_options")]
        public string AddvancedOptions { get; set; }
        public string Icon { get; set; }

        public UserAccount()
        {

        }

        public UserAccount CreateUser(string serviceName, string url, string login, string password, string advopt = null)
        {
            var service = new ServiceItems().AllServices.FirstOrDefault(s => s.Name == serviceName);

            return new UserAccount
            {
                Service = serviceName,
                Url = url,
                Login = login,
                Password = password,
                AddvancedOptions = advopt,
                Icon = service?.Icon ?? "/resources/services_logos/blank_logo.png"
            };
        }

    }
}
