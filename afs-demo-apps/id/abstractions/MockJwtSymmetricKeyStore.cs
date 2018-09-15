using afs.jwt.abstractions;

namespace afs.jwt.example
{
    public sealed class MockJwtSymmetricKeyStore : ITokenKeyStore
    {
        private const string _key = @"Qwerty!234567890";

        public KeyType KeyType => KeyType.HSSymmetricKey;
        public void Load() { }
        public string GetKey(string keyPairId = null) => _key;
        public (string keyPairId, string key) GetLatestKey() => (null, _key);
        public void Dispose() { }
    }
}