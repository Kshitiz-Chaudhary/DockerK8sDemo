using System;

namespace afs.jwt.abstractions
{
    public interface ITokenKeyStore : IDisposable
    {
        KeyType KeyType { get; }

        /// <summary>Load keys into the store</summary>
        void Load();

        /// <summary>Get key</summary>
        /// <param name="keyPairId">Optional id of asymmetric key pair, when asymmetric key store is used</param>
        string GetKey(string keyPairId = null);

        /// <summary>Get latest key by Creation time UTC</summary>
        (string keyPairId, string key) GetLatestKey();
    }
}