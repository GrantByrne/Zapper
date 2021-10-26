using System;
using System.Collections.Generic;

namespace Zapper.Core.WebOs
{
    public interface IWebOsActions
    {
        IEnumerable<string> GetAll();

        Action Get(string key);
    }
}