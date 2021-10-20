using System.Collections.Generic;

namespace Zapper.Core.WebOs
{
    public static class WebOsActions
    {
        public static IEnumerable<string> GetAll()
        {
            return new[]
            {
                "Mute",
                "Unmute",
                "Volume Up",
                "Volume Down",
                "Channel Down",
                "Channel Up",
                "Turn On 3D",
                "Turn Off 3D",
                "Home",
                "Back",
                "Up",
                "Down",
                "Left",
                "Right",
                "Red",
                "Blue",
                "Yellow",
                "Green"
            };
        }
    }
}