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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Markup;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Windows.Automation.Peers;

namespace PGas_v2._0._0
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private string ACCESS_TOKEN { get; set; }
        private string REFRESH_TOKEN { get; set; }

        public ApiServiceMode API_SERVICE_MODE { get; set; }

        private const string REST_API_URL = "https://pgas.jokimazi.site/rest-api/";

        public enum ApiServiceMode
        {
            Authentication,
            DataInteraction
        }

        public ApiService(ApiServiceMode mode)
        {
            _httpClient = new HttpClient();

            ACCESS_TOKEN = App.ACCESS_TOKEN;
            REFRESH_TOKEN = App.REFRESH_TOKEN;

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

                App.ACCESS_TOKEN = result.AccessToken;
                App.REFRESH_TOKEN = result.RefreshToken;

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
                return false;
            }
        }


        public async Task<List<UserAccount>> GetDataAsync()
        {
            async Task<HttpResponseMessage> Wrapper()
            {
                string URL = REST_API_URL + "get-data/";

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ACCESS_TOKEN);

                return await _httpClient.GetAsync(URL);
            }

            async Task<List<UserAccount>> AddIcon(HttpResponseMessage resp)
            {
                string json = await resp.Content.ReadAsStringAsync();
                var accounts = JsonConvert.DeserializeObject<List<UserAccount>>(json);

                var services = new ServiceItems().AllServices;

                foreach (var acc in accounts)
                {
                    var service = services.FirstOrDefault(s => s.Name == acc.Service);
                    acc.Icon = service?.Icon ?? "/resources/services_logos/blank_logo.png";
                }

                return accounts;
            }

            if (API_SERVICE_MODE != ApiServiceMode.DataInteraction)
                throw new InvalidOperationException("The operation is available only in data interaction mode.");

            HttpResponseMessage response = await Wrapper();

            if (response.IsSuccessStatusCode)
            {
                return await AddIcon(response);
            }
            else if ((int)response.StatusCode == 401)
            {
                string errorJson = await response.Content.ReadAsStringAsync();

                if (errorJson.Contains("token_not_valid") && errorJson.Contains("Token is expired"))
                {
                    bool refreshed = await RefreshTokenAsync();
                    if (refreshed)
                    {
                        response = await Wrapper();
                        if (response.IsSuccessStatusCode)
                        {
                            return await AddIcon(response);
                        }
                    }
                }

                throw new UnauthorizedAccessException("Access token is invalid and could not be refreshed.");
            }
            else
            {
                response.EnsureSuccessStatusCode();
                return null;
            }
        }

        public async Task<bool> AddDataAsync(string service, string url, string login, string password, string aao = null)
        {
            if (API_SERVICE_MODE != ApiServiceMode.DataInteraction)
                throw new InvalidOperationException("The operation is available only in data interaction mode.");

            async Task<HttpResponseMessage> Wrapper()
            {
                string URL = REST_API_URL + "add-data/";

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ACCESS_TOKEN);

                var data = new
                {
                    name = service,
                    url = url,
                    login = login,
                    crypt_password = password,
                    advanced_auth_options = aao
                };

                string json = JsonConvert.SerializeObject(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                return await _httpClient.PostAsync(URL, content);
            }


            HttpResponseMessage response = await Wrapper();

            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            else if ((int)response.StatusCode == 401)
            {
                string errorJson = await response.Content.ReadAsStringAsync();

                if (errorJson.Contains("token_not_valid") && errorJson.Contains("Token is expired"))
                {
                    bool refreshed = await RefreshTokenAsync();
                    if (refreshed)
                    {
                        response = await Wrapper();
                        if (response.IsSuccessStatusCode)
                        {
                            return true;
                        }
                    }
                }

                throw new UnauthorizedAccessException("Access token is invalid and could not be refreshed.");
            }
            else
            {
                response.EnsureSuccessStatusCode();
                return false;
            }
        }

        public async Task<bool> DeleteDataAsync(int id)
        {
            async Task<HttpResponseMessage> Wrapper()
            {
                string URL = REST_API_URL + $"delete-data/{id}/";

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ACCESS_TOKEN);

                return await _httpClient.DeleteAsync(URL);
            };

            if (API_SERVICE_MODE != ApiServiceMode.DataInteraction)
                throw new InvalidOperationException("The operation is available only in data interaction mode.");

            HttpResponseMessage response = await Wrapper();

            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            else if ((int)response.StatusCode == 401)
            {
                string errorJson = await response.Content.ReadAsStringAsync();

                if (errorJson.Contains("token_not_valid") && errorJson.Contains("Token is expired"))
                {
                    bool refreshed = await RefreshTokenAsync();
                    if (refreshed)
                    {
                        response = await Wrapper();
                        if (response.IsSuccessStatusCode)
                        {
                            return true;
                        }
                    }
                }

                throw new UnauthorizedAccessException("Access token is invalid and could not be refreshed.");
            }
            else
            {
                response.EnsureSuccessStatusCode();
                return false;
            }
        }
    }
}
