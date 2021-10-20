using System;
using System.Collections.Generic;

namespace Zapper.Core.WebOs
{
    public interface IWebOsActions
    {
        IEnumerable<string> GetAll();
    }

    public class WebOsActions : IWebOsActions
    {
        private readonly Dictionary<string, Action> _actions = new();

        public WebOsActions()
        {
            _actions.Add("Mute", () => {} );
            _actions.Add("Unmute", () => {} );
            _actions.Add("Volume Up", () => {} );
            _actions.Add("Volume Down", () => {} );
            _actions.Add("Channel Down", () => {} );
            _actions.Add("Channel Up", () => {} );
            _actions.Add("Turn On 3D", () => {} );
            _actions.Add("Turn Off 3D", () => {} );
            _actions.Add("Home", () => {} );
            _actions.Add("Back", () => {} );
            _actions.Add("Up", () => {} );
            _actions.Add("Down", () => {} );
            _actions.Add("Left", () => {} );
            _actions.Add("Right", () => {} );
            _actions.Add("Red", () => {} );
            _actions.Add("Blue", () => {} );
            _actions.Add("Yellow", () => {} );
            _actions.Add("Green", () => {} );
        }

        public IEnumerable<string> GetAll()
        {
            return _actions.Keys;
        }
    }
}