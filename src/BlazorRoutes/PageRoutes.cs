using System;
using System.Collections.Generic;
using System.Linq;

namespace BlazorRoutes
{
    public class PageRoutes
    {
        public string PageName { get; set; } = string.Empty;

        public IEnumerable<Route> Routes { get; set; } = Enumerable.Empty<Route>();
    }
}
