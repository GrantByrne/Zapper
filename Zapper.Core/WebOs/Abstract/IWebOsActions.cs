using System;
using System.Collections.Generic;

namespace Zapper.Core.WebOs.Abstract
{
    public interface IWebOsActions
    {
        IEnumerable<string> GetAll();

        Action Get(string key);
    }
}