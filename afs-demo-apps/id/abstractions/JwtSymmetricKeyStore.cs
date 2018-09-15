namespace afs.jwt.abstractions
{
    public sealed class JwtSymmetricKeyStore : ITokenKeyStore
    {
        public KeyType KeyType => KeyType.HSSymmetricKey;

        private readonly string _key;

        public JwtSymmetricKeyStore(string key)
        {
            _key = key;
        }

        public void Load() { }
        public string GetKey(string keyPairId = null) => _key;
        public (string keyPairId, string key) GetLatestKey() => (null, _key);
        public void Dispose() { }
    }
}