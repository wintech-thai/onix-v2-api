
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Its.Onix.Api.Services
{
    public class AuthService : BaseService, IAuthService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string tokenEndpoint = "";
        //private readonly string issuer = "";
        //private readonly string signedKeyUrl = "";
        private readonly string? clientId = "";
        private readonly string? clientSecret = "";

        public AuthService(IHttpClientFactory httpClientFactory) : base()
        {
            _httpClientFactory = httpClientFactory;

            var realm = Environment.GetEnvironmentVariable("IDP_REALM");
            var urlPrefix = Environment.GetEnvironmentVariable("IDP_URL_PREFIX");

            clientId = Environment.GetEnvironmentVariable("IDP_CLIENT_ID");
            clientSecret = Environment.GetEnvironmentVariable("IDP_CLIENT_SECRET");

            //issuer = $"{urlPrefix}/auth/realms/{realm}";
            tokenEndpoint = $"{urlPrefix}/auth/realms/{realm}/protocol/openid-connect/token";
            //signedKeyUrl = $"{urlPrefix}/auth/realms/{realm}/protocol/openid-connect/certs";
        }

        public string GetPreferredUsername(string accessToken)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(accessToken);

            // อ่าน claim preferred_username
            var username = jwtToken.Claims.FirstOrDefault(c => c.Type == "preferred_username")?.Value;

            return username!;
        }
        
        private UserToken GetUserToken(KeyValuePair<string, string>[] form)
        {
            var userToken = new UserToken();
            userToken.Status = "Success";

            var client = _httpClientFactory.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Post, tokenEndpoint)
            {
                Content = new FormUrlEncodedContent(form)
            };
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

            try
            {
                var response = client.SendAsync(request).Result;
                if (!response.IsSuccessStatusCode)
                {
                    userToken.Status = response.StatusCode.ToString();
                    return userToken;
                }

                var content = response.Content.ReadAsStringAsync().Result;
                userToken.Token = JsonSerializer.Deserialize<KeycloakTokenResponse>(content)!;
            }
            catch (HttpRequestException ex)
            {
                userToken.Status = "FAILED";
                userToken.Message = ex.Message;
            }

            return userToken;
        }

        public UserToken Login(UserLogin userLogin)
        {
            var form = new[]
            {
                new KeyValuePair<string,string>("grant_type", "password"),
                new KeyValuePair<string,string>("response_type", "token"),
                new KeyValuePair<string,string>("scope", "openid offline_access"),
                new KeyValuePair<string,string>("client_id", clientId!),
                new KeyValuePair<string,string>("client_secret", clientSecret!),
                new KeyValuePair<string,string>("username", userLogin.UserName),
                new KeyValuePair<string,string>("password", userLogin.Password)
            };
            Console.WriteLine($"==== [{userLogin.Password}] ====");
            var userToken = GetUserToken(form);
            userToken.UserName = userLogin.UserName;

            return userToken;
        }

        public UserToken RefreshToken(string refreshToken)
        {
            var form = new[]
            {
                new KeyValuePair<string,string>("grant_type", "refresh_token"),
                new KeyValuePair<string,string>("client_id", clientId!),
                new KeyValuePair<string,string>("client_secret", clientSecret!),
                new KeyValuePair<string,string>("refresh_token", refreshToken),
            };

            var userToken = GetUserToken(form);
            if (userToken.Status == "Success")
            {
                userToken.UserName = GetPreferredUsername(userToken.Token.AccessToken!);
            }

            return userToken;
        }
    }
}
