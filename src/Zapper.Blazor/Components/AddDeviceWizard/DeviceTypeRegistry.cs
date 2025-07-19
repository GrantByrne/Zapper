namespace Zapper.Blazor.Components.AddDeviceWizard;

public interface IDeviceTypeRegistry
{
    IEnumerable<IDeviceTypeDefinition> GetAllDeviceTypes();
    IDeviceTypeDefinition? GetDeviceType(string typeName);
}

public class DeviceTypeRegistry(IEnumerable<IDeviceTypeDefinition> deviceTypes) : IDeviceTypeRegistry
{
    private readonly Dictionary<string, IDeviceTypeDefinition> _deviceTypes = deviceTypes.ToDictionary(d => d.DisplayName, d => d);

    public IEnumerable<IDeviceTypeDefinition> GetAllDeviceTypes() => _deviceTypes.Values;

    public IDeviceTypeDefinition? GetDeviceType(string typeName) =>
        _deviceTypes.TryGetValue(typeName, out var definition) ? definition : null;
}