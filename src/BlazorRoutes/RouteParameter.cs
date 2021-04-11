using System;
using System.Collections.Generic;

namespace BlazorRoutes
{
    public class RouteParameter
    {
        public RouteParameter(string name, string? type, bool isOptional)
        {
            this.Name = name;
            this.Type = type switch
                        {
                            "datetime" => nameof(DateTime),
                            "guid" => nameof(Guid),
                            null => "string",
                            _ => type,
                        };
            this.IsOptional = isOptional;
        }

        public string Name { get; }

        public string Type { get; }

        public bool IsOptional { get; }
    }
}
