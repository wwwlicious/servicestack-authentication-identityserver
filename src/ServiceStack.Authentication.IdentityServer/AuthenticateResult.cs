namespace ServiceStack.Authentication.IdentityServer
{
    public class AuthenticateResult
    {
        public string Code { get; set; }

        public string IdToken { get; set; }

        public bool IsEmpty => string.IsNullOrEmpty(Code) || string.IsNullOrEmpty(IdToken);
    }
}
