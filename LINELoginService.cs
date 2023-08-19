using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json.Nodes;

namespace ClassLibrary.SocialLogin
{
    public class LINELoginService : ISocialLoginService
    {
        private readonly ILogger<LINELoginService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        private readonly string? _API;
        private readonly string? _redirectUri;
        private readonly string? _clientID;
        private readonly string? _screct;

        public LINELoginService(ILogger<LINELoginService> logger, IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _API = configuration["SocialLogin:LINE:API"];
            _redirectUri = configuration["SocialLogin:LINE:redirectUri"];
            _clientID = configuration["SocialLogin:LINE:clientID"];
            _screct = configuration["SocialLogin:LINE:screct"];
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<string> ExchangeToken(string code)
        {
            string token = null;

            try
            {
                var client = _httpClientFactory.CreateClient();

                var request = new HttpRequestMessage(HttpMethod.Post, new Uri(new Uri(_API), "token"));

                request.Content = new FormUrlEncodedContent(
                    new Dictionary<string, string>
                    {
                        ["grant_type"] = "authorization_code",
                        ["code"] = code,
                        ["redirect_uri"] = _redirectUri,
                        ["client_id"] = _clientID,
                        ["client_secret"] = _screct
                    });

                var response = await client.SendAsync(request);

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();

                var result = JsonNode.Parse(content);

                token = result["id_token"].GetValue<string>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }

            return token;
        }

        public async Task<string> GetUserInfo(string token)
        {
            string result = null;

            try
            {
                var client = _httpClientFactory.CreateClient();

                var request = new HttpRequestMessage(HttpMethod.Post, new Uri(new Uri(_API), "verify"));

                request.Content = new FormUrlEncodedContent(
                    new Dictionary<string, string>
                    {
                        ["id_token"] = token,
                        ["client_id"] = _clientID,
                    });

                var response = await client.SendAsync(request);

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    return null;
                }

                result = await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }

            return result;
        }
    }
}