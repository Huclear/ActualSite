namespace ActualSite.Domain
{
    public interface ITokenService
    {
        public string Generate(TokenConfig conf, List<TokenClaim> claims);

    }
}
