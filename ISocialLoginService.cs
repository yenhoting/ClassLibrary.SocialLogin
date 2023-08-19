namespace ClassLibrary.SocialLogin
{
    public interface ISocialLoginService
    {
        Task<string> ExchangeToken(string code);

        Task<string> GetUserInfo(string token);
    }
}
