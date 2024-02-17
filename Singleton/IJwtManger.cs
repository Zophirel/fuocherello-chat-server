using SignalRChatServer.Singleton.JwtManager;
namespace SignalRChatServer.Singleton.JwtManager
{
    public enum TokenStatus {Valid, Invalid, Expired}
 
    public interface IJwtManager
    {
        public TokenStatus ValidateAccessToken(string jwt);

    }
}
