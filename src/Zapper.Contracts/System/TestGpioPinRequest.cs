namespace Zapper.Contracts.System;

public class TestGpioPinRequest
{
    public int Pin { get; set; }
    public bool IsOutput { get; set; } = true;
}