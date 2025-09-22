
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Its.Onix.Api.Models;
using Microsoft.IdentityModel.Tokens;

namespace Its.Onix.Api.Services
{
    public class AuthService : BaseService, IAuthService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string tokenEndpoint = "";
        private readonly string userEndpoint = "";
        private readonly string issuer = "";
        private readonly string signedKeyUrl = "";
        private readonly string? clientId = "";
        private readonly string? clientSecret = "";
        private IJwtSigner signer = new JwtSigner();

        public AuthService(IHttpClientFactory httpClientFactory) : base()
        {
            _httpClientFactory = httpClientFactory;

            var realm = Environment.GetEnvironmentVariable("IDP_REALM");
            var urlPrefix = Environment.GetEnvironmentVariable("IDP_URL_PREFIX");

            clientId = Environment.GetEnvironmentVariable("IDP_CLIENT_ID");
            clientSecret = Environment.GetEnvironmentVariable("IDP_CLIENT_SECRET");

            issuer = $"{urlPrefix}/auth/realms/{realm}";
            tokenEndpoint = $"{urlPrefix}/auth/realms/{realm}/protocol/openid-connect/token";
            signedKeyUrl = $"{urlPrefix}/auth/realms/{realm}/protocol/openid-connect/certs";

            userEndpoint = $"{urlPrefix}/auth/admin/realms/{realm}/users";
        }

        private string GetPreferredUsername(string accessToken)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(accessToken);

            // อ่าน claim preferred_username
            var username = jwtToken.Claims.FirstOrDefault(c => c.Type == "preferred_username")?.Value;

            return username!;
        }

        private UserToken GetToken(KeyValuePair<string, string>[] form)
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
            //Console.WriteLine($"==== [{userLogin.Password}] ====");
            var userToken = GetToken(form);
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

            var userToken = GetToken(form);
            if (userToken.Status == "Success")
            {
                userToken.UserName = GetPreferredUsername(userToken.Token.AccessToken!);
            }

            return userToken;
        }

        private UserToken GetServiceAccountToken()
        {
            var form = new[]
            {
                new KeyValuePair<string,string>("grant_type", "client_credentials"),
                new KeyValuePair<string,string>("client_id", clientId!),
                new KeyValuePair<string,string>("client_secret", clientSecret!),
            };

            var saToken = GetToken(form);
            return saToken;
        }

        private async Task<IdpResult> CreateUserAsync(string token, MOrganizeRegistration orgUser)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var user = new
            {
                username = orgUser.UserName,
                email = orgUser.Email,
                enabled = true,
                firstName = orgUser.Name,
                lastName = orgUser.Lastname,
            };

            var content = new StringContent(JsonSerializer.Serialize(user), Encoding.UTF8, "application/json");
            var response = await client.PostAsync(userEndpoint, content);

            var result = new IdpResult()
            {
                Success = true,
                Message = $"User [{orgUser.UserName}] [{orgUser.Email}] created successfully!",
            };

            if (!response.IsSuccessStatusCode)
            {
                result.Success = false;
                result.Message = await response.Content.ReadAsStringAsync();
            }

            return result;
        }

        public async Task<IdpResult> AddUserToIDP(MOrganizeRegistration orgUser)
        {
            var token = GetServiceAccountToken();
            return await CreateUserAsync(token.Token.AccessToken, orgUser);
        }

        public SecurityToken ValidateAccessToken(string accessToken, JwtSecurityTokenHandler tokenHandler)
        {
            //Important : In Keycloak keys setting we must enable only 1 key proder 'RS256'.
            //https://keycloak.devops.napbiotec.io/auth/realms/rtarf-ads-dev/protocol/openid-connect/certs
            var securityKey = signer.GetSignedKey(signedKeyUrl);
            //Console.WriteLine($"=== {accessToken} ===");
            var param = new TokenValidationParameters()
            {
                ValidIssuer = issuer,
                ValidAudience = "account",
                IssuerSigningKey = securityKey,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
            };

            SecurityToken validatedToken;
            tokenHandler.ValidateToken(accessToken, param, out validatedToken);

            return validatedToken;
        }
    }
}
