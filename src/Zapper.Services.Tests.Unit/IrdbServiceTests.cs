using System.Text.Json;

namespace Zapper.Services.Tests.Unit;

public class IrdbServiceTests
{
    private readonly HttpClient _httpClient;
    private readonly ZapperContext _context;
    private readonly ILogger<IrdbService> _logger;
    private readonly IrdbService _service;

    public IrdbServiceTests()
    {
        _httpClient = Substitute.For<HttpClient>();
        _logger = Substitute.For<ILogger<IrdbService>>();
        
        var options = new DbContextOptionsBuilder<ZapperContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ZapperContext(options);
        
        _service = new IrdbService(_httpClient, _context, _logger);
    }

    [Fact]
    public async Task GetAvailableManufacturersAsync_ReturnsManufacturers_WhenIndexResponseIsValid()
    {
        var mockIndexContent = "Samsung/TV/Generic,1.csv\nLG/TV/Generic,1.csv\nSony/TV/Generic,1.csv";
        _httpClient.GetStringAsync(Arg.Any<string>()).Returns(mockIndexContent);

        var result = await _service.GetAvailableManufacturersAsync();

        result.Should().Contain("Samsung", "LG", "Sony");
    }

    [Fact]
    public async Task SearchDevicesAsync_ReturnsFilteredDevices_WhenManufacturerFilterProvided()
    {
        var mockIndexContent = "Samsung/TV/Generic,1.csv\nLG/TV/Generic,1.csv\nSamsung/DVD/Player,1.csv";
        _httpClient.GetStringAsync(Arg.Any<string>()).Returns(mockIndexContent);

        var result = await _service.SearchDevicesAsync(manufacturer: "Samsung");

        result.Should().HaveCount(2);
        result.Should().AllSatisfy(d => d.Manufacturer.Should().Be("Samsung"));
    }

    [Fact]
    public async Task GetCodeSetAsync_ReturnsNull_WhenCsvContentIsEmpty()
    {
        _httpClient.GetStringAsync(Arg.Any<string>()).Returns("");

        var result = await _service.GetCodeSetAsync("Samsung", "TV", "Generic", "1");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetCodeSetAsync_ParsesCsvAndReturnsCodeSet_WhenValidCsvProvided()
    {
        var mockCsvContent = "Power,NEC,1,1\nVolumeUp,NEC,1,2\nVolumeDown,NEC,1,3";
        _httpClient.GetStringAsync(Arg.Any<string>()).Returns(mockCsvContent);

        var result = await _service.GetCodeSetAsync("Samsung", "TV", "Generic", "1");

        result.Should().NotBeNull();
        result!.Brand.Should().Be("Samsung");
        result.Model.Should().Be("Generic");
        result.DeviceType.Should().Be(DeviceType.Television);
        result.Codes.Should().HaveCount(3);
        result.Codes.Should().Contain(c => c.CommandName == "Power");
        result.Codes.Should().Contain(c => c.CommandName == "VolumeUp");
        result.Codes.Should().Contain(c => c.CommandName == "VolumeDown");
    }

    [Fact]
    public async Task IsAvailableAsync_ReturnsTrue_WhenHttpRequestSucceeds()
    {
        var mockResponse = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
        _httpClient.GetAsync(Arg.Any<string>()).Returns(mockResponse);

        var result = await _service.IsAvailableAsync();

        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsAvailableAsync_ReturnsFalse_WhenHttpRequestFails()
    {
        _httpClient.GetAsync(Arg.Any<string>()).Returns(Task.FromException<HttpResponseMessage>(new HttpRequestException()));

        var result = await _service.IsAvailableAsync();

        result.Should().BeFalse();
    }

    [Fact]
    public async Task InvalidateCacheAsync_RemovesCacheEntries_WithIrdbPrefix()
    {
        _context.ExternalIrCodeCache.AddRange(
            new ExternalIrCodeCache { CacheKey = "irdb:test1", CachedData = "data1", ExpiresAt = DateTime.UtcNow.AddDays(1) },
            new ExternalIrCodeCache { CacheKey = "other:test2", CachedData = "data2", ExpiresAt = DateTime.UtcNow.AddDays(1) },
            new ExternalIrCodeCache { CacheKey = "irdb:test3", CachedData = "data3", ExpiresAt = DateTime.UtcNow.AddDays(1) }
        );
        await _context.SaveChangesAsync();

        await _service.InvalidateCacheAsync();

        var remainingEntries = await _context.ExternalIrCodeCache.ToListAsync();
        remainingEntries.Should().HaveCount(1);
        remainingEntries.Single().CacheKey.Should().Be("other:test2");
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}