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
        public const string DiscoverWebOS = $"{Base}/discover/webos";
        public const string PairWebOS = $"{Base}/pair/webos";
        public const string BluetoothControl = $"{Base}/bluetooth/control";
        public const string BluetoothDiscovery = $"{Base}/discover/bluetooth";
    }
    
    public static class Activities
    {
        public const string Base = $"{BaseUrl}/activities";
        public const string GetAll = Base;
        public const string Execute = $"{Base}/{{id}}/execute";
    }
    
    public static class IRCodes
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