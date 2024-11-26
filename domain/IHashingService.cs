namespace ActualSite.Domain
{
    public interface IHashingService
    {
        public SaltedHash GenerateHash(string value, int saltLength = 32);
        public bool VerifyHash(string value, SaltedHash hash);
    }
}
