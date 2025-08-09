using ConfigServices.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml;

namespace ConfigServices.Service
{
    public class ServerConfigService : IServerConfigService
    {
        private readonly IAppPaths _paths;
        private readonly object _sync = new object();

        private static readonly XmlSerializer _serializer = new XmlSerializer(typeof(ServerConfigList));
        private static readonly XmlWriterSettings _xmlWriterSettings = new XmlWriterSettings
        {
            Indent = true,
            NewLineOnAttributes = false,
            OmitXmlDeclaration = false,
            Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)
        };

        public ServerConfigService(IAppPaths paths)
        {
            _paths = paths ?? throw new ArgumentNullException(nameof(paths));
        }

        // ---------------- File-level ----------------
        public ServerConfigList Load()
        {
            lock (_sync) return LoadFromDisk();
        }

        public void Save(ServerConfigList data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            lock (_sync)
            {
                EnsureIds(data);
                SaveToDisk(data);
            }
        }

        // ---------------- Queries ----------------
        public IReadOnlyList<ServerConfig> GetAll()
        {
            lock (_sync) return LoadFromDisk().Items.AsReadOnly();
        }

        public ServerConfig? GetById(Guid id)
        {
            lock (_sync)
            {
                var list = LoadFromDisk();
                return list.Items.FirstOrDefault(x => x.Id == id);
            }
        }

        public ServerConfig GetRequired(Guid id)
        {
            var it = GetById(id);
            if (it == null) throw new KeyNotFoundException($"ServerConfig with Id '{id}' was not found.");
            return it;
        }

        public IReadOnlyList<ServerConfig> GetByLocation(string location)
        {
            var loc = Normalize(location);
            lock (_sync)
            {
                var list = LoadFromDisk();
                var results = list.Items
                    .Where(x => StringEquals(Normalize(x.Location), loc))
                    .ToList();
                return results.AsReadOnly();
            }
        }

        public IReadOnlyList<ServerConfig> GetByLocationInstance(string location, string instance)
        {
            var loc = Normalize(location);
            var inst = Normalize(instance);
            lock (_sync)
            {
                var list = LoadFromDisk();
                var results = list.Items
                    .Where(x => StringEquals(Normalize(x.Location), loc)
                             && StringEquals(Normalize(x.Instance), inst))
                    .ToList();
                return results.AsReadOnly();
            }
        }

        public ServerConfig? GetByLocationInstanceUsername(string location, string instance, string username)
        {
            var loc = Normalize(location);
            var inst = Normalize(instance);
            var user = Normalize(username);
            lock (_sync)
            {
                var list = LoadFromDisk();
                return list.Items.FirstOrDefault(x =>
                    StringEquals(Normalize(x.Location), loc) &&
                    StringEquals(Normalize(x.Instance), inst) &&
                    StringEquals(Normalize(x.Username), user));
            }
        }

        // ---------------- Mutations ----------------
        public Guid Add(ServerConfig config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));

            lock (_sync)
            {
                var list = LoadFromDisk();

                if (config.Id == Guid.Empty)
                    config.Id = Guid.NewGuid();

                // Guard against duplicates by (Location, Instance); remove if not desired.
                var dup = list.Items.Any(x =>
                    StringEquals(Normalize(x.Location), Normalize(config.Location)) &&
                    StringEquals(Normalize(x.Instance), Normalize(config.Instance)));

                if (dup)
                    throw new InvalidOperationException(
                        $"A server with Location='{config.Location}' and Instance='{config.Instance}' already exists.");

                list.Items.Add(config);
                SaveToDisk(list);
                return config.Id;
            }
        }

        public bool Edit(Guid id, ServerConfig updated)
        {
            if (updated == null) throw new ArgumentNullException(nameof(updated));

            lock (_sync)
            {
                var list = LoadFromDisk();
                var existing = list.Items.FirstOrDefault(x => x.Id == id);
                if (existing == null) return false;

                // Keep the same Id
                existing.Location = updated.Location;
                existing.Instance = updated.Instance;
                existing.InstanceType = updated.InstanceType;
                existing.Username = updated.Username;
                existing.Password = updated.Password;
                existing.TNS = updated.TNS;

                SaveToDisk(list);
                return true;
            }
        }

        public bool Delete(Guid id)
        {
            lock (_sync)
            {
                var list = LoadFromDisk();
                var removed = list.Items.RemoveAll(x => x.Id == id);
                if (removed == 0) return false;

                SaveToDisk(list);
                return true;
            }
        }

        // ---------------- Internals ----------------
        private ServerConfigList LoadFromDisk()
        {
            var path = _paths.GetPreferredConfigPath();

            if (!File.Exists(path))
                return new ServerConfigList();

            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var reader = new StreamReader(fs, Encoding.UTF8, true))
            {
                var data = (ServerConfigList)_serializer.Deserialize(reader);
                EnsureIds(data);
                return data;
            }
        }

        private void SaveToDisk(ServerConfigList data)
        {
            var path = _paths.GetPreferredConfigPath();
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);

            var temp = path + ".tmp";

            using (var fs = new FileStream(temp, FileMode.Create, FileAccess.Write, FileShare.None))
            using (var writer = XmlWriter.Create(fs, _xmlWriterSettings))
            {
                _serializer.Serialize(writer, data);
            }

            if (File.Exists(path))
                File.Replace(temp, path, destinationBackupFileName: null);
            else
                File.Move(temp, path);
        }

        private static void EnsureIds(ServerConfigList data)
        {
            foreach (var it in data.Items)
                if (it.Id == Guid.Empty)
                    it.Id = Guid.NewGuid();
        }

        private static string Normalize(string? s) => (s ?? string.Empty).Trim();
        private static bool StringEquals(string a, string b) =>
            a.Equals(b, StringComparison.OrdinalIgnoreCase);



        public IReadOnlyList<string> GetDistinctLocations(bool sorted = true)
        {
            lock (_sync)
            {
                var list = LoadFromDisk();

                var query = list.Items
                    .Select(x => Normalize(x.Location))
                    .Where(s => !string.IsNullOrEmpty(s))
                    .Distinct(StringComparer.OrdinalIgnoreCase);

                if (sorted)
                    query = query.OrderBy(s => s, StringComparer.OrdinalIgnoreCase);

                return query.ToList().AsReadOnly();
            }
        }

    }

}
