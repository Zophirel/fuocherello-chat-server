using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;


namespace SignalRChatServer.Singleton.JwtManager
{
    
    public class JwtManager : IJwtManager
    {
        private readonly JwtSecurityTokenHandler _tokenHandler = new();
        private readonly TokenValidationParameters? _validationParameters;

        // Private constructor to prevent instantiation from outside
        public JwtManager(RSA key)
        {
            
            var securityKey = new RsaSecurityKey(key);
            _validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = false,
                ValidIssuer = "https://www.zophirel.it:8443",
                IssuerSigningKey = securityKey
            };

            RSA rsaKey = RSA.Create();
            rsaKey.ImportRSAPrivateKey(File.ReadAllBytes("key"), out _);
        }

        private static bool ValidateLifetTime(ClaimsPrincipal payload){
            string? expStringValue = payload.Claims.FirstOrDefault(c => c.Type == "exp")?.Value;
            if(expStringValue is not null){
                DateTime exp = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expStringValue)).DateTime;
                if(DateTime.UtcNow > exp){
                    return false;
                }else{
                    return true;
                }
            }
            return false;
        }

        private static string ReadASJson(string token){
            var jwtHandler = new JwtSecurityTokenHandler();
            var jwt = jwtHandler.ReadJwtToken(token);
            return jwt.Payload.SerializeToJson();
        }

        public string? ExtractType(string token){
 
            JObject payload = JObject.Parse(ReadASJson(token));
            if(payload["type"] != null){
                return payload["type"]!.ToString();
            }else{
                return null;
            }
        } 
 
        public TokenStatus ValidateAccessToken(string jwt){
            var payload = _tokenHandler.ValidateToken(jwt, _validationParameters, out _);
            if(payload is not null && ExtractType(jwt) == "Access"){
                if(ValidateLifetTime(payload)){
                    return TokenStatus.Valid;
                }else{
                    
                    return TokenStatus.Expired;
                }
            }
            //Token non valido
            return TokenStatus.Invalid;
        }
    }
}

