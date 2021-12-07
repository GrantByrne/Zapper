using System;

namespace Zapper.WebOs.Exceptions
{
    public class CommandException : Exception
    {
        public CommandException(string error) : base(error){}
    }
}
