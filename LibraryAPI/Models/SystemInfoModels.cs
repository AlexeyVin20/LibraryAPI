using System.Collections.Generic;

namespace LibraryAPI.Models
{
    public class EndpointInfo
    {
        public string Controller { get; set; }
        public string Action { get; set; }
        public string Route { get; set; }
        public List<string> Methods { get; set; }
        public List<ParameterInfo> Parameters { get; set; }
        public List<string> RequiredRoles { get; set; }
        public bool IsAnonymous { get; set; }
    }

    public class ParameterInfo
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public bool IsOptional { get; set; }
    }
} 