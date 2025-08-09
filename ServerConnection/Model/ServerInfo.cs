using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerConnection.Model
{
    public class ServerInfo
    {
        public string TNS { get; set; } = "";      // e.g., "XE.WORLD" or full EZCONNECT
        public string Username { get; set; } = ""; // DB user
        public string Password { get; set; } = ""; // DB password
    }
}
