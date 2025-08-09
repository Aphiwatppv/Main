using ConfigServices.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml;
using AdvancedLogging;

namespace ConfigServices.Service
{
    public class ServerConfigService : IServerConfigService
    {
        private readonly IAppPaths _paths;
        private readonly ILogger _log;
        private readonly object _sync = new();

        private static readonly XmlSerializer _serializer = new XmlSerializer(typeof(ServerConfigList));
        private static readonly XmlWriterSettings _xmlWriterSettings = new XmlWriterSettings
        {
            Indent = true,
            NewLineOnAttributes = false,
            OmitXmlDeclaration = false,
            Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)
        };

        public ServerConfigService(IAppPaths paths, ILogger logger)
        {
            _paths = paths ?? throw new ArgumentNullException(nameof(paths));
            _log = logger ?? throw new ArgumentNullException(nameof(logger));
            _log.Debug("ServerConfigService constructed with config path {Path}",
                ("Path", _paths.GetPreferredConfigPath()));
        }

        // ---------------- File-level ----------------
        public ServerConfigList Load()
        {
            using (_log.BeginScope("op", "load"))
            {
                lock (_sync)
                {
                    try
                    {
                        var list = LoadFromDisk();
                        _log.Info("Loaded {Count} server config(s)", ("Count", list.Items.Count));
                        return list;
                    }
                    catch (Exception ex)
                    {
                        _log.Error("Load failed", ex);
                        throw;
                    }
                }
            }
        }

        public void Save(ServerConfigList data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            using (_log.BeginScope("op", "save"))
            {
                lock (_sync)
                {
                    try
                    {
                        EnsureIds(data);
                        SaveToDisk(data);
                        _log.Info("Saved {Count} server config(s)", ("Count", data.Items.Count));
                    }
                    catch (Exception ex)
                    {
                        _log.Error("Save failed", ex);
                        throw;
                    }
                }
            }
        }

        // ---------------- Queries ----------------
        public IReadOnlyList<ServerConfig> GetAll()
        {
            lock (_sync)
            {
                var list = LoadFromDisk();
                _log.Debug("GetAll -> {Count}", ("Count", list.Items.Count));
                return list.Items.AsReadOnly();
            }
        }

        public ServerConfig? GetById(Guid id)
        {
            lock (_sync)
            {
                var list = LoadFromDisk();
                var item = list.Items.FirstOrDefault(x => x.Id == id);
                _log.Debug("GetById({Id}) -> {Found}", ("Id", id), ("Found", item != null));
                return item;
            }
        }

        public ServerConfig GetRequired(Guid id)
        {
            var it = GetById(id);
            if (it == null)
            {
                _log.Warn("GetRequired({Id}) not found", ("Id", id));
                throw new KeyNotFoundException($"ServerConfig with Id '{id}' was not found.");
            }
            return it;
        }

        public IReadOnlyList<ServerConfig> GetByLocation(string location)
        {
            var loc = Normalize(location);
            lock (_sync)
            {
                var list = LoadFromDisk();
                var results = list.Items.Where(x => StringEquals(Normalize(x.Location), loc)).ToList();
                _log.Debug("GetByLocation({Location}) -> {Count}", ("Location", loc), ("Count", results.Count));
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
                    .Where(x => StringEquals(Normalize(x.Location), loc) &&
                                StringEquals(Normalize(x.Instance), inst))
                    .ToList();
                _log.Debug("GetByLocationInstance({Location},{Instance}) -> {Count}",
                    ("Location", loc), ("Instance", inst), ("Count", results.Count));
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
                var result = list.Items.FirstOrDefault(x =>
                    StringEquals(Normalize(x.Location), loc) &&
                    StringEquals(Normalize(x.Instance), inst) &&
                    StringEquals(Normalize(x.Username), user));
                _log.Debug("GetByLocationInstanceUsername({Location},{Instance},{User}) -> {Found}",
                    ("Location", loc), ("Instance", inst), ("User", user), ("Found", result != null));
                return result;
            }
        }

        public IReadOnlyList<string> GetDistinctLocations(bool sorted = true)
        {
            lock (_sync)
            {
                var list = LoadFromDisk();
                var query = list.Items
                    .Select(x => Normalize(x.Location))
                    .Where(s => !string.IsNullOrEmpty(s))
                    .Distinct(StringComparer.OrdinalIgnoreCase);

                if (sorted) query = query.OrderBy(s => s, StringComparer.OrdinalIgnoreCase);

                var result = query.ToList().AsReadOnly();
                _log.Debug("GetDistinctLocations(sorted={Sorted}) -> {Count}", ("Sorted", sorted), ("Count", result.Count));
                return result;
            }
        }

        // ---------------- Mutations ----------------
        public Guid Add(ServerConfig config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));

            using (_log.BeginScope(new Dictionary<string, object>
            {
                ["op"] = "add",
                ["location"] = config.Location,
                ["instance"] = config.Instance
            }))
            {
                lock (_sync)
                {
                    try
                    {
                        var list = LoadFromDisk();

                        if (config.Id == Guid.Empty) config.Id = Guid.NewGuid();

                        // Optional duplicate guard by (Location, Instance)
                        var dup = list.Items.Any(x =>
                            StringEquals(Normalize(x.Location), Normalize(config.Location)) &&
                            StringEquals(Normalize(x.Instance), Normalize(config.Instance)));

                        if (dup)
                        {
                            _log.Warn("Add rejected: duplicate {Location}/{Instance}",
                                ("Location", config.Location), ("Instance", config.Instance));
                            throw new InvalidOperationException(
                                $"A server with Location='{config.Location}' and Instance='{config.Instance}' already exists.");
                        }

                        list.Items.Add(config);
                        SaveToDisk(list);

                        _log.Info("Add succeeded (Id={Id}, Type={Type}, User={User})",
                            ("Id", config.Id), ("Type", config.InstanceType), ("User", config.Username));
                        return config.Id;
                    }
                    catch (Exception ex)
                    {
                        _log.Error("Add failed", ex, ("Location", config.Location), ("Instance", config.Instance));
                        throw;
                    }
                }
            }
        }

        public bool Edit(Guid id, ServerConfig updated)
        {
            if (updated == null) throw new ArgumentNullException(nameof(updated));

            using (_log.BeginScope(new Dictionary<string, object>
            {
                ["op"] = "edit",
                ["id"] = id
            }))
            {
                lock (_sync)
                {
                    try
                    {
                        var list = LoadFromDisk();
                        var existing = list.Items.FirstOrDefault(x => x.Id == id);
                        if (existing == null)
                        {
                            _log.Warn("Edit not found (Id={Id})", ("Id", id));
                            return false;
                        }

                        existing.Location = updated.Location;
                        existing.Instance = updated.Instance;
                        existing.InstanceType = updated.InstanceType;
                        existing.Username = updated.Username;
                        // Do NOT log or transform password content
                        existing.Password = updated.Password;
                        existing.TNS = updated.TNS;

                        SaveToDisk(list);
                        _log.Info("Edit succeeded -> {Location}/{Instance} (Type={Type}, User={User})",
                            ("Location", existing.Location), ("Instance", existing.Instance),
                            ("Type", existing.InstanceType), ("User", existing.Username));
                        return true;
                    }
                    catch (Exception ex)
                    {
                        _log.Error("Edit failed (Id={Id})", ex, ("Id", id));
                        throw;
                    }
                }
            }
        }

        public bool Delete(Guid id)
        {
            using (_log.BeginScope(new Dictionary<string, object>
            {
                ["op"] = "delete",
                ["id"] = id
            }))
            {
                lock (_sync)
                {
                    try
                    {
                        var list = LoadFromDisk();
                        var existing = list.Items.FirstOrDefault(x => x.Id == id);
                        if (existing == null)
                        {
                            _log.Warn("Delete not found (Id={Id})", ("Id", id));
                            return false;
                        }

                        list.Items.Remove(existing);
                        SaveToDisk(list);
                        _log.Info("Delete succeeded -> {Location}/{Instance}",
                            ("Location", existing.Location), ("Instance", existing.Instance));
                        return true;
                    }
                    catch (Exception ex)
                    {
                        _log.Error("Delete failed (Id={Id})", ex, ("Id", id));
                        throw;
                    }
                }
            }
        }

        // ---------------- Internals ----------------
        private ServerConfigList LoadFromDisk()
        {
            var path = _paths.GetPreferredConfigPath();

            if (!File.Exists(path))
            {
                _log.Debug("No config file found at {Path}, returning empty list", ("Path", path));
                return new ServerConfigList();
            }

            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var reader = new StreamReader(fs, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
            var data = (ServerConfigList)_serializer.Deserialize(reader);
            EnsureIds(data);
            return data;
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
    }
}
