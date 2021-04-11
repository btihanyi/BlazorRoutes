using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace BlazorRoutes
{
    public class Route
    {
        private readonly List<RouteParameter>? parameters;

        public Route(string path)
        {
            this.Path = path;

            var regex = new Regex(@"\{(?<name>\w+)(:(?<type>\w+))?(?<optional>\?)?\}", RegexOptions.ExplicitCapture);
            var matches = regex.Matches(path);

            parameters = new List<RouteParameter>(matches.Count);
            parameters.AddRange(
                matches.Cast<Match>().Select(m => new RouteParameter(
                    name: m.Groups["name"].Value,
                    type: m.Groups["type"].Success ? m.Groups["type"].Value : null,
                    isOptional: m.Groups["optional"].Success)));

            int i = 0;
            FormatPattern = regex.Replace(path, m => $"{{{i++}}}");
        }

        public string Path { get; }

        public string FormatPattern { get; }

        public IReadOnlyCollection<RouteParameter> Parameters => (IReadOnlyCollection<RouteParameter>?) parameters ?? Array.Empty<RouteParameter>();
    }
}
