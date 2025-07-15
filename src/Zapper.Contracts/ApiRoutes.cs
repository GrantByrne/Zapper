namespace Zapper.Contracts;

public static class ApiRoutes
{
    public const string BaseUrl = "/api";

    public static class Devices
    {
        public const string Base = $"{BaseUrl}/devices";
        public const string GetAll = Base;
        public const string GetById = $"{Base}/{{id}}";
        public const string Create = Base;
        public const string Update = $"{Base}/{{id}}";
        public const string Delete = $"{Base}/{{id}}";
        public const string SendCommand = $"{Base}/{{id}}/command";
        public const string DiscoverRoku = $"{Base}/discover/roku";
        public const string DiscoverWebOs = $"{Base}/discover/webos";
        public const string PairWebOs = $"{Base}/pair/webos";
        public const string BluetoothControl = $"{Base}/bluetooth/control";
        public const string BluetoothDiscovery = $"{Base}/discover/bluetooth";
        public const string BluetoothScan = $"{Base}/scan/bluetooth";
        public const string BluetoothScanStop = $"{Base}/scan/bluetooth/stop";
        public const string WebOsScan = $"{Base}/scan/webos";
        public const string WebOsScanStop = $"{Base}/scan/webos/stop";
    }

    public static class Activities
    {
        public const string Base = $"{BaseUrl}/activities";
        public const string GetAll = Base;
        public const string GetById = $"{Base}/{{id}}";
        public const string Create = Base;
        public const string Update = $"{Base}/{{id}}";
        public const string Delete = $"{Base}/{{id}}";
        public const string Execute = $"{Base}/{{id}}/execute";
    }

    public static class IrCodes
    {
        public const string Base = $"{BaseUrl}/ircodes";
        public const string GetAll = Base;
        public const string GetById = $"{Base}/{{id}}";
        public const string Add = Base;
        public const string CreateSet = $"{Base}/sets";
        public const string GetSets = $"{Base}/sets";
        public const string GetSet = $"{Base}/sets/{{id}}";
        public const string DeleteSet = $"{Base}/sets/{{id}}";
        public const string ExportSet = $"{Base}/sets/{{id}}/export";
        public const string SearchSets = $"{Base}/sets/search";
        public const string SeedDefaults = $"{Base}/seed";
    }

    public static class System
    {
        public const string Base = $"{BaseUrl}/system";
        public const string Status = $"{Base}/status";
    }
}