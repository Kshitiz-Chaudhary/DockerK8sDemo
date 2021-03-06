﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using afs.jwt.abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace afs.jwt.issuer
{
    /// <summary>Abstraction to hold value of a private key used for JWT token signature</summary>
    /// <remarks>The key is loaded into memory from a secure file at app start or when content of this file changes</remarks>
    public sealed class JwtPrivateKeyStore : ITokenKeyStore
    {
        public KeyType KeyType => KeyType.RsaPrivateKey;

        private readonly object updateLock = new object();
        private IReadOnlyDictionary<string, string> _keys;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        private readonly JwtKeyOptions _keyOptions;
        private readonly ILogger _logger;

        public JwtPrivateKeyStore(IOptions<JwtKeyOptions> keyOptions, ILoggerFactory loggerFactory)
        {
            _keyOptions = keyOptions.Value;
            _logger = loggerFactory.CreateLogger("JwtPrivateKeyStore");

            // Note! generate default directory, if does not exist
            _keyOptions.JwtKeysDirectory.TryEnsureDir();

            // load at start
            Load();

            if (_keyOptions.JwtWatchKeyChanges)
                this.StartMonitor(_keyOptions.JwtKeysDirectory, _keyOptions.JwtPrivateKeyFileNameFormat, _cancellationTokenSource.Token, Load);
        }

        public void Load()
        {
            var absoluteJwtKeysDirPath = _keyOptions.JwtKeysDirectory.ToAbsolutePath();
            _logger.LogDebug("Loading private keys from {JwtKeysDirectory}", absoluteJwtKeysDirPath);

            lock (updateLock)
                _keys = absoluteJwtKeysDirPath.LoadKeys(_keyOptions.JwtPrivateKeyFileNameFormat);

            _logger.LogDebug("Loaded {PrivateKeys}", String.Join(",", _keys?.Keys ?? Enumerable.Empty<string>()));
        }

        public string GetKey(string keyPairId = null)
            => keyPairId != null && _keys.ContainsKey(keyPairId)
                ? _keys[keyPairId]
                : _keys.LastOrDefault().Value;

        public (string keyPairId, string key) GetLatestKey()
        {
            var lastKeyEntry = _keys.LastOrDefault();
            return (lastKeyEntry.Key, lastKeyEntry.Value);
        }

        public void Dispose() { _cancellationTokenSource.Cancel(); }

        public override string ToString()
            => $"[JwtPrivateKeyStore keys: {String.Join(",", _keys?.Keys ?? Enumerable.Empty<string>())}]";
    }
}