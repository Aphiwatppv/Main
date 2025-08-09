using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ConfigServices.Model
{
    [XmlRoot("Servers")]
    public class ServerConfigList
    {
        [XmlElement("Server")]
        public List<ServerConfig> Items { get; set; } = new();
    }
}
