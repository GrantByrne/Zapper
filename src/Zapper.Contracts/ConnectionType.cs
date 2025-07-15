namespace Zapper.Contracts;

/// <summary>
/// Defines the connection methods available for communicating with devices.
/// Each connection type uses different protocols and hardware interfaces.
/// </summary>
public enum ConnectionType
{
    /// <summary>
    /// Infrared (IR) communication using LED transmitters and receivers.
    /// This is the traditional method used by most universal remotes.
    /// </summary>
    InfraredIr,

    /// <summary>
    /// Radio frequency (RF) communication for devices that support wireless RF protocols.
    /// Typically used for longer-range or more reliable wireless control.
    /// </summary>
    RadioFrequencyRf,

    /// <summary>
    /// Network communication using TCP sockets for direct device control.
    /// Used for devices that expose TCP-based control APIs.
    /// </summary>
    NetworkTcp,

    /// <summary>
    /// Network communication using WebSocket connections for real-time bidirectional communication.
    /// Commonly used by smart TVs and streaming devices.
    /// </summary>
    NetworkWebSocket,

    /// <summary>
    /// Network communication using HTTP/HTTPS REST APIs for device control.
    /// Used by devices that expose web-based control interfaces.
    /// </summary>
    NetworkHttp,

    /// <summary>
    /// Bluetooth communication for wireless device control.
    /// Used for devices like Android TV, Apple TV, and some game consoles.
    /// </summary>
    Bluetooth,

    /// <summary>
    /// USB communication for devices that can be controlled via USB HID protocols.
    /// Used for some gaming devices and specialized hardware.
    /// </summary>
    Usb,

    /// <summary>
    /// WebOS-specific protocol for controlling LG smart TVs.
    /// Uses WebSocket connections with WebOS-specific message formats.
    /// </summary>
    WebOs
}