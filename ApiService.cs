using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Wpf.Ui.Controls;
using System.Security.Policy;

namespace PGas_v2._0._0
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private string ACCESS_TOKEN { get; set; }
        private string REFRESH_TOKEN { get; set; }

        private ApiServiceMode API_SERVICE_MODE { get; set; }

        private const string REST_API_URL = "https://pgas.jokimazi.site/rest-api/";

        public enum ApiServiceMode
        {
            Authentication,
            DataInteraction
        }

        public ApiService(ApiServiceMode mode)
        {
            _httpClient = new HttpClient();

            API_SERVICE_MODE = mode;
        }

        public class AuthResponseModel
        {
            [JsonProperty("access")]
            public string AccessToken { get; set; }

            [JsonProperty("refresh")]
            public string RefreshToken { get; set; }

            [JsonProperty("detail")]
            public string ErrorMessage { get; set; }
        }

        private string ParseErrorMessage(string json)
        {
            try
            {
                var errorObj = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                return errorObj.Values.FirstOrDefault()?.ToString() ?? "Неизвестная ошибка";
            }
            catch
            {
                return "Ошибка при разборе сообщения: " + json;
            }
        }

        public async Task<AuthResponseModel> AuthAsync(string username, string password)
        {
            if (API_SERVICE_MODE != ApiServiceMode.Authentication)
                throw new InvalidOperationException("The operation is available only in data interaction mode.");

            string URL = REST_API_URL + "token-obtain-pair/";

            var data = new
            {
                username = username,
                password = password
            };

            string json = JsonConvert.SerializeObject(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _httpClient.PostAsync(URL, content);

            string responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                AuthResponseModel result = JsonConvert.DeserializeObject<AuthResponseModel>(responseContent);

                ACCESS_TOKEN = result.AccessToken;
                REFRESH_TOKEN = result.RefreshToken;
                Console.WriteLine(ACCESS_TOKEN);

                return result;
            }
            else
            {
                return new AuthResponseModel
                {
                    ErrorMessage = ParseErrorMessage(responseContent)
                };
            }
            
        }

        public async Task<bool> RefreshTokenAsync()
        {
            if (API_SERVICE_MODE != ApiServiceMode.DataInteraction)
                throw new InvalidOperationException("The operation is available only in data interaction mode.");

            string URL = REST_API_URL + "token-refresh/";

            var data = new
            {
                refresh = REFRESH_TOKEN
            };

            string json = JsonConvert.SerializeObject(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _httpClient.PostAsync(URL, content);
            string responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonConvert.DeserializeObject<AuthResponseModel>(responseContent);
                ACCESS_TOKEN = result.AccessToken;
                return true;
            }
            else
            {
                // Токен истёк или refresh недействителен
                return false;
            }
        }


        public async Task<List<UserAccount>> GetDataAsync()
        {
            if (API_SERVICE_MODE != ApiServiceMode.DataInteraction)
                throw new InvalidOperationException("The operation is available only in data interaction mode.");

            async Task<HttpResponseMessage> SendRequest()
            {
                Console.WriteLine(ACCESS_TOKEN);
                string URL = REST_API_URL + "get-data/";
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", ACCESS_TOKEN);
                Console.WriteLine(ACCESS_TOKEN);
                return await _httpClient.GetAsync(URL);
            }

            HttpResponseMessage response = await SendRequest();

            if (response.IsSuccessStatusCode)
            {
                string json = await response.Content.ReadAsStringAsync();
                var accounts = JsonConvert.DeserializeObject<List<UserAccount>>(json);

                var services = new ServiceItems().AllServices;

                foreach (var acc in accounts)
                {
                    var service = services.FirstOrDefault(s => s.Name == acc.Service);
                    acc.Icon = service?.Icon ?? "/resources/services_logos/blank_logo.png";
                }

                return accounts;
            }
            else if ((int)response.StatusCode == 401)
            {
                string errorJson = await response.Content.ReadAsStringAsync();

                // Проверяем, что причина — истекший access токен
                if (errorJson.Contains("token_not_valid") && errorJson.Contains("Token is expired"))
                {
                    bool refreshed = await RefreshTokenAsync();
                    if (refreshed)
                    {
                        // Повторяем запрос с новым access токеном
                        response = await SendRequest();
                        if (response.IsSuccessStatusCode)
                        {
                            string json = await response.Content.ReadAsStringAsync();
                            var accounts = JsonConvert.DeserializeObject<List<UserAccount>>(json);

                            var services = new ServiceItems().AllServices;

                            foreach (var acc in accounts)
                            {
                                var service = services.FirstOrDefault(s => s.Name == acc.Service);
                                acc.Icon = service?.Icon ?? "/resources/services_logos/blank_logo.png";
                            }

                            return accounts;
                        }
                    }
                }

                throw new UnauthorizedAccessException("Access token is invalid and could not be refreshed.");
            }
            else
            {
                response.EnsureSuccessStatusCode(); // Бросаем исключение для всех других ошибок
                return null; // никогда не достигнется
            }
        }


    }
}
