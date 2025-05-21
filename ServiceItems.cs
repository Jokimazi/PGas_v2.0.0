using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xaml;

namespace PGas_v2._0._0
{
    public class ServiceItems
    {
        public List<ServiceItem> AllServices { get; set; }

        public class ServiceItem
        {
            public string Name { get; set; }
            public string Icon { get; set; }
            public string Url { get; set; }
            public bool IsCustom { get; set; } = false;
        }

        public ServiceItems()
        {
            AllServices = new List<ServiceItem>
            {
                new ServiceItem { Name = "Другой сервис", Icon = "/resources/services_logos/blank_logo.png", Url="https://google.com", IsCustom = true },
                new ServiceItem { Name = "Google", Icon = "/resources/services_logos/google_logo.png", Url="https://google.com" },
                new ServiceItem { Name = "VK", Icon = "/resources/services_logos/vk_logo.png", Url = "https://vk.ru" },
                new ServiceItem { Name = "OK", Icon = "/resources/services_logos/ok_logo.png", Url = "https://ok.ru"},
                new ServiceItem { Name = "Pinterest", Icon = "/resources/services_logos/pinterest_logo.png", Url = "https://pinterest.com"},
                new ServiceItem { Name = "GitHub", Icon = "/resources/services_logos/github_logo.png", Url = "https://gihub.com" },
            };

            AllServices = AllServices
                .OrderBy(s => s.Name != "Другой сервис")
                .ThenBy(s => s.Name.ToLower())
                .ToList();
        }
    }
}
