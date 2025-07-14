using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Zapper.Data;
using Zapper.Core.Models;

namespace Zapper.Services;

public class IrCodeService(ZapperContext context, ILogger<IrCodeService> logger) : IIrCodeService
{

    public async Task<IEnumerable<IrCodeSet>> GetCodeSetsAsync()
    {
        return await context.IrCodeSets
            .Include(cs => cs.Codes)
            .OrderBy(cs => cs.Brand)
            .ThenBy(cs => cs.Model)
            .ToListAsync();
    }

    public async Task<IEnumerable<IrCodeSet>> SearchCodeSetsAsync(string? brand = null, string? model = null, DeviceType? deviceType = null)
    {
        var query = context.IrCodeSets.Include(cs => cs.Codes).AsQueryable();

        if (!string.IsNullOrEmpty(brand))
        {
            query = query.Where(cs => cs.Brand.ToLower().Contains(brand.ToLower()));
        }

        if (!string.IsNullOrEmpty(model))
        {
            query = query.Where(cs => cs.Model.ToLower().Contains(model.ToLower()));
        }

        if (deviceType.HasValue)
        {
            query = query.Where(cs => cs.DeviceType == deviceType.Value);
        }

        return await query
            .OrderBy(cs => cs.Brand)
            .ThenBy(cs => cs.Model)
            .ToListAsync();
    }

    public async Task<IrCodeSet?> GetCodeSetAsync(int id)
    {
        return await context.IrCodeSets
            .Include(cs => cs.Codes)
            .FirstOrDefaultAsync(cs => cs.Id == id);
    }

    public async Task<IrCodeSet?> GetCodeSetAsync(string brand, string model, DeviceType deviceType)
    {
        return await context.IrCodeSets
            .Include(cs => cs.Codes)
            .FirstOrDefaultAsync(cs => cs.Brand.ToLower() == brand.ToLower() 
                                    && cs.Model.ToLower() == model.ToLower() 
                                    && cs.DeviceType == deviceType);
    }

    public async Task<IEnumerable<IrCode>> GetCodesAsync(int codeSetId)
    {
        return await context.IrCodes
            .Where(c => EF.Property<int>(c, "IRCodeSetId") == codeSetId)
            .OrderBy(c => c.CommandName)
            .ToListAsync();
    }

    public async Task<IrCode?> GetCodeAsync(int codeSetId, string commandName)
    {
        return await context.IrCodes
            .FirstOrDefaultAsync(c => EF.Property<int>(c, "IRCodeSetId") == codeSetId 
                                   && c.CommandName.ToLower() == commandName.ToLower());
    }

    public async Task<IrCodeSet> CreateCodeSetAsync(IrCodeSet codeSet)
    {
        context.IrCodeSets.Add(codeSet);
        await context.SaveChangesAsync();
        return codeSet;
    }

    public async Task<IrCode> AddCodeAsync(int codeSetId, IrCode code)
    {
        var codeSet = await GetCodeSetAsync(codeSetId);
        if (codeSet == null)
        {
            throw new ArgumentException($"Code set with ID {codeSetId} not found");
        }

        context.Entry(code).Property("IRCodeSetId").CurrentValue = codeSetId;
        
        context.IrCodes.Add(code);
        await context.SaveChangesAsync();
        return code;
    }

    public async Task UpdateCodeSetAsync(IrCodeSet codeSet)
    {
        codeSet.UpdatedAt = DateTime.UtcNow;
        context.IrCodeSets.Update(codeSet);
        await context.SaveChangesAsync();
    }

    public async Task DeleteCodeSetAsync(int id)
    {
        var codeSet = await GetCodeSetAsync(id);
        if (codeSet != null)
        {
            context.IrCodeSets.Remove(codeSet);
            await context.SaveChangesAsync();
        }
    }

    public async Task<bool> ImportCodeSetAsync(string filePath)
    {
        try
        {
            var json = await File.ReadAllTextAsync(filePath);
            var importData = JsonSerializer.Deserialize<IrCodeSetImport>(json);
            
            if (importData == null) return false;

            var codeSet = new IrCodeSet
            {
                Brand = importData.Brand,
                Model = importData.Model,
                DeviceType = importData.DeviceType,
                Description = importData.Description,
                IsVerified = false,
                Codes = importData.Codes.Select(c => new IrCode
                {
                    Brand = importData.Brand,
                    Model = importData.Model,
                    DeviceType = importData.DeviceType,
                    CommandName = c.CommandName,
                    Protocol = c.Protocol,
                    HexCode = c.HexCode,
                    Frequency = c.Frequency,
                    RawData = c.RawData,
                    Notes = c.Notes
                }).ToList()
            };

            await CreateCodeSetAsync(codeSet);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to import IR code set from {FilePath}", filePath);
            return false;
        }
    }

    public async Task<string> ExportCodeSetAsync(int id)
    {
        var codeSet = await GetCodeSetAsync(id);
        if (codeSet == null)
        {
            throw new ArgumentException($"Code set with ID {id} not found");
        }

        var exportData = new IrCodeSetImport
        {
            Brand = codeSet.Brand,
            Model = codeSet.Model,
            DeviceType = codeSet.DeviceType,
            Description = codeSet.Description,
            Codes = codeSet.Codes.Select(c => new IrCodeImport
            {
                CommandName = c.CommandName,
                Protocol = c.Protocol,
                HexCode = c.HexCode,
                Frequency = c.Frequency,
                RawData = c.RawData,
                Notes = c.Notes
            }).ToList()
        };

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        return JsonSerializer.Serialize(exportData, options);
    }

    public async Task SeedDefaultCodesAsync()
    {
        if (await context.IrCodeSets.AnyAsync())
        {
            return;
        }

        logger.LogInformation("Seeding default IR codes...");

        var defaultCodeSets = GetDefaultCodeSets();
        
        foreach (var codeSet in defaultCodeSets)
        {
            await CreateCodeSetAsync(codeSet);
        }

        logger.LogInformation("Seeded {Count} default IR code sets", defaultCodeSets.Count);
    }

    private static List<IrCodeSet> GetDefaultCodeSets()
    {
        return
        [
            new IrCodeSet
            {
                Brand = "Samsung",
                Model = "Generic TV",
                DeviceType = DeviceType.Television,
                Description = "Generic Samsung TV IR codes",
                IsVerified = true,
                Codes =
                [
                    new IrCode { CommandName = "Power", Protocol = "NEC", HexCode = "0xE0E040BF", Frequency = 38000 },
                    new IrCode
                    {
                        CommandName = "VolumeUp", Protocol = "NEC", HexCode = "0xE0E0E01F", Frequency = 38000
                    },

                    new IrCode
                    {
                        CommandName = "VolumeDown", Protocol = "NEC", HexCode = "0xE0E0D02F", Frequency = 38000
                    },

                    new IrCode { CommandName = "Mute", Protocol = "NEC", HexCode = "0xE0E0F00F", Frequency = 38000 },
                    new IrCode
                    {
                        CommandName = "DirectionalUp", Protocol = "NEC", HexCode = "0xE0E006F9", Frequency = 38000
                    },

                    new IrCode
                    {
                        CommandName = "DirectionalDown", Protocol = "NEC", HexCode = "0xE0E08679", Frequency = 38000
                    },

                    new IrCode
                    {
                        CommandName = "DirectionalLeft", Protocol = "NEC", HexCode = "0xE0E0A659", Frequency = 38000
                    },

                    new IrCode
                    {
                        CommandName = "DirectionalRight", Protocol = "NEC", HexCode = "0xE0E046B9", Frequency = 38000
                    },

                    new IrCode { CommandName = "OK", Protocol = "NEC", HexCode = "0xE0E016E9", Frequency = 38000 },
                    new IrCode { CommandName = "Menu", Protocol = "NEC", HexCode = "0xE0E058A7", Frequency = 38000 },
                    new IrCode { CommandName = "Back", Protocol = "NEC", HexCode = "0xE0E01AE5", Frequency = 38000 },
                    new IrCode { CommandName = "Home", Protocol = "NEC", HexCode = "0xE0E079F6", Frequency = 38000 }
                ]
            },

            new IrCodeSet
            {
                Brand = "LG",
                Model = "Generic TV",
                DeviceType = DeviceType.Television,
                Description = "Generic LG TV IR codes",
                IsVerified = true,
                Codes =
                [
                    new IrCode { CommandName = "Power", Protocol = "NEC", HexCode = "0x20DF10EF", Frequency = 38000 },
                    new IrCode
                    {
                        CommandName = "VolumeUp", Protocol = "NEC", HexCode = "0x20DF40BF", Frequency = 38000
                    },

                    new IrCode
                    {
                        CommandName = "VolumeDown", Protocol = "NEC", HexCode = "0x20DFC03F", Frequency = 38000
                    },

                    new IrCode { CommandName = "Mute", Protocol = "NEC", HexCode = "0x20DF906F", Frequency = 38000 },
                    new IrCode
                    {
                        CommandName = "DirectionalUp", Protocol = "NEC", HexCode = "0x20DF02FD", Frequency = 38000
                    },

                    new IrCode
                    {
                        CommandName = "DirectionalDown", Protocol = "NEC", HexCode = "0x20DF827D", Frequency = 38000
                    },

                    new IrCode
                    {
                        CommandName = "DirectionalLeft", Protocol = "NEC", HexCode = "0x20DFE01F", Frequency = 38000
                    },

                    new IrCode
                    {
                        CommandName = "DirectionalRight", Protocol = "NEC", HexCode = "0x20DF609F", Frequency = 38000
                    },

                    new IrCode { CommandName = "OK", Protocol = "NEC", HexCode = "0x20DF22DD", Frequency = 38000 },
                    new IrCode { CommandName = "Menu", Protocol = "NEC", HexCode = "0x20DFC23D", Frequency = 38000 },
                    new IrCode { CommandName = "Back", Protocol = "NEC", HexCode = "0x20DF14EB", Frequency = 38000 },
                    new IrCode { CommandName = "Home", Protocol = "NEC", HexCode = "0x20DF3EC1", Frequency = 38000 }
                ]
            },

            new IrCodeSet
            {
                Brand = "Sony",
                Model = "Generic TV",
                DeviceType = DeviceType.Television,
                Description = "Generic Sony TV IR codes",
                IsVerified = true,
                Codes =
                [
                    new IrCode { CommandName = "Power", Protocol = "SONY", HexCode = "0xA90", Frequency = 40000 },
                    new IrCode { CommandName = "VolumeUp", Protocol = "SONY", HexCode = "0x490", Frequency = 40000 },
                    new IrCode { CommandName = "VolumeDown", Protocol = "SONY", HexCode = "0xC90", Frequency = 40000 },
                    new IrCode { CommandName = "Mute", Protocol = "SONY", HexCode = "0x290", Frequency = 40000 },
                    new IrCode
                    {
                        CommandName = "DirectionalUp", Protocol = "SONY", HexCode = "0x2F0", Frequency = 40000
                    },

                    new IrCode
                    {
                        CommandName = "DirectionalDown", Protocol = "SONY", HexCode = "0xAF0", Frequency = 40000
                    },

                    new IrCode
                    {
                        CommandName = "DirectionalLeft", Protocol = "SONY", HexCode = "0x2D0", Frequency = 40000
                    },

                    new IrCode
                    {
                        CommandName = "DirectionalRight", Protocol = "SONY", HexCode = "0xCD0", Frequency = 40000
                    },

                    new IrCode { CommandName = "OK", Protocol = "SONY", HexCode = "0xA70", Frequency = 40000 },
                    new IrCode { CommandName = "Menu", Protocol = "SONY", HexCode = "0x070", Frequency = 40000 },
                    new IrCode { CommandName = "Back", Protocol = "SONY", HexCode = "0x870", Frequency = 40000 },
                    new IrCode { CommandName = "Home", Protocol = "SONY", HexCode = "0x670", Frequency = 40000 }
                ]
            }
        ];
    }
}

public class IrCodeSetImport
{
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public DeviceType DeviceType { get; set; }
    public string? Description { get; set; }
    public List<IrCodeImport> Codes { get; set; } = [];
}

public class IrCodeImport
{
    public string CommandName { get; set; } = string.Empty;
    public string Protocol { get; set; } = string.Empty;
    public string HexCode { get; set; } = string.Empty;
    public int Frequency { get; set; } = 38000;
    public string? RawData { get; set; }
    public string? Notes { get; set; }
}