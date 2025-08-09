using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigServices.Model
{
    public class ServerConfig
    {

        public Guid Id { get; set; } = Guid.NewGuid();
        public string Location { get; set; } = "";
        public string Instance { get; set; } = "";
        public string InstanceType { get; set; } = "";
        public string Username { get; set; } = "";
        public string? Password { get; set; }
        public string TNS { get; set; } = "";
    }
}
