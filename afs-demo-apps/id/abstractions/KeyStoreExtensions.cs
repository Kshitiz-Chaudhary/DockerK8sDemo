using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace afs.jwt.abstractions
{
    public static class KeyStoreExtensions
    {
        // start file-change monitor to detect, when key is updated
        // Note! required to be a singleton
        public static Task StartMonitor(this ITokenKeyStore store, string path, string filter,
            CancellationToken cancellationToken, Action changeAction)
            => Task.Factory.StartNew(
                () =>
                {
                    var watcher = new FileSystemWatcher
                    {
                        NotifyFilter = NotifyFilters.LastWrite,
                        Path = Path.GetDirectoryName(path),
                        Filter = filter
                    };

                    while (!cancellationToken.IsCancellationRequested)
                    {
                        var result = watcher.WaitForChanged(WatcherChangeTypes.Changed);
                        if (result.ChangeType == WatcherChangeTypes.Changed)
                            changeAction();
                    }
                }, cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);

        public static bool TryEnsureDir(this string dirPath)
        {
            if (String.IsNullOrEmpty(dirPath))
                return false;

            try
            {
                if (!Directory.Exists(dirPath))
                    Directory.CreateDirectory(dirPath);

                return true;
            }
            catch // Security exception
            {
                return false;
            }
        }

        /// <summary>Load key files from a selected directory</summary>
        /// <param name="absoluteDirPath">Absolute path to the keys directory</param>
        /// <param name="searchPattern">Regex pattern to filter found PEM files by</param>
        /// <remarks>Loaded keys are ordered by CreationTimeUtc</remarks>
        public static IReadOnlyDictionary<string, string> LoadKeys(this string absoluteDirPath, string searchPattern)
        {
            if (!Path.IsPathRooted(absoluteDirPath))
                throw new ArgumentOutOfRangeException(nameof(absoluteDirPath),
                    "absoluteDirPath must be an absolute path");

            var dirInfo = new DirectoryInfo(absoluteDirPath);

            IEnumerable<(string sigId, string key)> LoadPairs(DirectoryInfo directoryInfo)
                => from f1 in directoryInfo.EnumerateFiles("*.pem").OrderBy(f => f.CreationTimeUtc)
                    let match = Regex.Match(f1.Name, searchPattern)
                    where match.Success
                    let sigIdGroup = match.Groups["sig_id"]
                    where sigIdGroup.Success
                    select (sigIdGroup.Value, File.ReadAllText(f1.FullName, Encoding.ASCII));

            return LoadPairs(dirInfo).ToDictionary(pair => pair.sigId, pair => pair.key);
        }
    }

    public static class FileExtensions
    {
        public static string ToAbsolutePath(this string dirPath)
        {
            if (Path.IsPathRooted(dirPath))
                return dirPath;

            var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly(); // netcore ?? full framework (still invalid in TestHost)
            var codeBase = assembly.CodeBase;
            var uri = new UriBuilder(codeBase);
            var path = Uri.UnescapeDataString(uri.Path);
            var basePath = Path.GetDirectoryName(path);

            return Path.Combine(basePath ?? String.Empty, dirPath.Trim('/').Trim('\\'));
        }
    }
}