namespace Zapper.Contracts;

/// <summary>
/// Defines the types of devices that can be controlled by the Zapper system.
/// Each device type has different capabilities and command sets.
/// </summary>
public enum DeviceType
{
    /// <summary>
    /// A traditional television that typically supports basic commands like power, channel, and volume.
    /// </summary>
    Television,

    /// <summary>
    /// An audio/video receiver that manages audio output and input switching for multiple devices.
    /// </summary>
    Receiver,

    /// <summary>
    /// A cable or satellite TV set-top box that provides TV channels and guide functionality.
    /// </summary>
    CableBox,

    /// <summary>
    /// A streaming media device (e.g., Roku, Fire TV, Chromecast) that provides access to online content.
    /// </summary>
    StreamingDevice,

    /// <summary>
    /// A video game console (e.g., PlayStation, Xbox, Nintendo Switch) that supports gaming and media playback.
    /// </summary>
    GameConsole,

    /// <summary>
    /// A sound bar or audio system that enhances TV audio output.
    /// </summary>
    SoundBar,

    /// <summary>
    /// A DVD player for playing physical DVD media.
    /// </summary>
    DvdPlayer,

    /// <summary>
    /// A Blu-ray player for playing high-definition physical media.
    /// </summary>
    BluRayPlayer,

    /// <summary>
    /// A smart TV that combines traditional television with internet connectivity and apps.
    /// </summary>
    SmartTv,

    /// <summary>
    /// An Apple TV streaming device that provides access to Apple's ecosystem and services.
    /// </summary>
    AppleTv
}