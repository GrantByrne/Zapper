using System.Collections;
using System.Collections.Generic;

namespace Zapper.Core.WebOs
{
    public class WebOsActions
    {
        public IEnumerable<WebOsAction> GetAll()
        {
            var actions = new[]
            {
                new WebOsAction { Name = "Volume Down" },
                new WebOsAction { Name = "Volume Up" },
            };

            return actions;
        }
    }

    public class WebOsAction
    {
        public string Name { get; set; }
    }
}