using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigServices.Service
{
    public class AppPaths : IAppPaths
    {
        private readonly string _company;
        private readonly string _app;
        private readonly string _fileName;

        public AppPaths(string company, string app, string fileName = "ServersConfig.xml")
        {
            _company = company;
            _app = app;
            _fileName = fileName;
        }

        public string GetPreferredConfigPath()
        {
            var appDir = AppDomain.CurrentDomain.BaseDirectory;
            if (IsDirWritable(appDir))
                return Path.Combine(appDir, _fileName);

            return Path.Combine(GetProgramDataPath(), _fileName);
        }

        public string GetProgramDataPath()
        {
            var root = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            var dir = Path.Combine(root, _company, _app);
            Directory.CreateDirectory(dir);
            return dir;
        }

        private static bool IsDirWritable(string dir)
        {
            try
            {
                var probe = Path.Combine(dir, $".__w_{Guid.NewGuid():N}.tmp");
                using (File.Create(probe, 1, FileOptions.DeleteOnClose)) { }
                return true;
            }
            catch { return false; }
        }
    }
}
