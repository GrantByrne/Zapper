using System;
using System.Threading.Tasks;
using WebOsTv.Net.Commands.Media;
using WebOsTv.Net.Commands.System;
using WebOsTv.Net.Commands.Tv;
using WebOsTv.Net.Responses.Media;
using WebOsTv.Net.Responses.System;

namespace WebOsTv.Net.Services
{
    public class ControlService : IControlService
    {
        private readonly IClient _client;

        public enum ControlIntent
        {
            // Button Presses
            Home,
            Back,
            Up,
            Down,
            Left,
            Right,
            Red,
            Blue,
            Yellow,
            Green,

            // Other Commands
            FastForward,
            Pause,
            Play,
            Rewind,
            Stop,
            PowerOff,
            
            Enter,
            Dash,
            Info,
            One,
            Two,
            Three,
            Four,
            Five,
            Six,
            Seven,
            Eight,
            Nine,
            Zero,
            Asterisk,
            Cc,
            Exit,
            Menu
        }

        internal ControlService(IClient client)
        {
            _client = client;
        }

        public async Task SendIntentAsync(ControlIntent intent)
        {
            switch (intent)
            {
                case ControlIntent.Home:
                case ControlIntent.Back:
                case ControlIntent.Up:
                case ControlIntent.Down:
                case ControlIntent.Left:
                case ControlIntent.Right:
                case ControlIntent.Red:
                case ControlIntent.Blue:
                case ControlIntent.Yellow:
                case ControlIntent.Green:
                case ControlIntent.Dash:
                case ControlIntent.Enter:
                case ControlIntent.Info:
                case ControlIntent.Menu:
                    await _client.SendButtonAsync(ButtonType.GetByDisplayName(intent.ToString()));
                    break;
                case ControlIntent.FastForward:
                    await _client.SendCommandAsync<ControlFastForwardResponse>(new ControlFastForwardCommand());
                    break;
                case ControlIntent.Pause:
                    await _client.SendCommandAsync<ControlPauseResponse>(new ControlPauseCommand());
                    break;
                case ControlIntent.Play:
                    await _client.SendCommandAsync<ControlPlayResponse>(new ControlPlayCommand());
                    break;
                case ControlIntent.Rewind:
                    await _client.SendCommandAsync<ControlRewindResponse>(new ControlRewindCommand());
                    break;
                case ControlIntent.Stop:
                    await _client.SendCommandAsync<ControlStopResponse>(new ControlStopCommand());
                    break;
                case ControlIntent.PowerOff:
                    await _client.SendCommandAsync<PowerOffResponse>(new PowerOffCommand());
                    break;
                case ControlIntent.One:
                    await _client.SendButtonAsync(ButtonType.One);
                    break;
                case ControlIntent.Two:
                    await _client.SendButtonAsync(ButtonType.Two);
                    break;
                case ControlIntent.Three:
                    await _client.SendButtonAsync(ButtonType.Three);
                    break;
                case ControlIntent.Four:
                    await _client.SendButtonAsync(ButtonType.Four);
                    break;
                case ControlIntent.Five:
                    await _client.SendButtonAsync(ButtonType.Five);
                    break;
                case ControlIntent.Six:
                    await _client.SendButtonAsync(ButtonType.Six);
                    break;
                case ControlIntent.Seven:
                    await _client.SendButtonAsync(ButtonType.Seven);
                    break;
                case ControlIntent.Eight:
                    await _client.SendButtonAsync(ButtonType.Eight);
                    break;
                case ControlIntent.Nine:
                    await _client.SendButtonAsync(ButtonType.Nine);
                    break;
                case ControlIntent.Zero:
                    await _client.SendButtonAsync(ButtonType.Zero);
                    break;
                case ControlIntent.Asterisk:
                    await _client.SendButtonAsync(ButtonType.Asterisk);
                    break;
                case ControlIntent.Cc:
                    await _client.SendButtonAsync(ButtonType.Cc);
                    break;
                case ControlIntent.Exit:
                    await _client.SendButtonAsync(ButtonType.Exit);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(intent), intent, null);
            }
        }
    }
}