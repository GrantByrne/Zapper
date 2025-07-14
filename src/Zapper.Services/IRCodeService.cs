using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Zapper.Data;
using Zapper.Core.Models;

namespace Zapper.Services;

public class IRCodeService(ZapperContext context, ILogger<IRCodeService> logger) : IIRCodeService
{

    public async Task<IEnumerable<IRCodeSet>> GetCodeSetsAsync()
    {
        return await context.IRCodeSets
            .Include(cs => cs.Codes)
            .OrderBy(cs => cs.Brand)
            .ThenBy(cs => cs.Model)
            .ToListAsync();
    }

    public async Task<IEnumerable<IRCodeSet>> SearchCodeSetsAsync(string? brand = null, string? model = null, DeviceType? deviceType = null)
    {
        var query = context.IRCodeSets.Include(cs => cs.Codes).AsQueryable();

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

    public async Task<IRCodeSet?> GetCodeSetAsync(int id)
    {
        return await context.IRCodeSets
            .Include(cs => cs.Codes)
            .FirstOrDefaultAsync(cs => cs.Id == id);
    }

    public async Task<IRCodeSet?> GetCodeSetAsync(string brand, string model, DeviceType deviceType)
    {
        return await context.IRCodeSets
            .Include(cs => cs.Codes)
            .FirstOrDefaultAsync(cs => cs.Brand.ToLower() == brand.ToLower() 
                                    && cs.Model.ToLower() == model.ToLower() 
                                    && cs.DeviceType == deviceType);
    }

    public async Task<IEnumerable<IRCode>> GetCodesAsync(int codeSetId)
    {
        return await context.IRCodes
            .Where(c => EF.Property<int>(c, "IRCodeSetId") == codeSetId)
            .OrderBy(c => c.CommandName)
            .ToListAsync();
    }

    public async Task<IRCode?> GetCodeAsync(int codeSetId, string commandName)
    {
        return await context.IRCodes
            .FirstOrDefaultAsync(c => EF.Property<int>(c, "IRCodeSetId") == codeSetId 
                                   && c.CommandName.ToLower() == commandName.ToLower());
    }

    public async Task<IRCodeSet> CreateCodeSetAsync(IRCodeSet codeSet)
    {
        context.IRCodeSets.Add(codeSet);
        await context.SaveChangesAsync();
        return codeSet;
    }

    public async Task<IRCode> AddCodeAsync(int codeSetId, IRCode code)
    {
        var codeSet = await GetCodeSetAsync(codeSetId);
        if (codeSet == null)
        {
            throw new ArgumentException($"Code set with ID {codeSetId} not found");
        }

        // Set the foreign key
        context.Entry(code).Property("IRCodeSetId").CurrentValue = codeSetId;
        
        context.IRCodes.Add(code);
        await context.SaveChangesAsync();
        return code;
    }

    public async Task UpdateCodeSetAsync(IRCodeSet codeSet)
    {
        codeSet.UpdatedAt = DateTime.UtcNow;
        context.IRCodeSets.Update(codeSet);
        await context.SaveChangesAsync();
    }

    public async Task DeleteCodeSetAsync(int id)
    {
        var codeSet = await GetCodeSetAsync(id);
        if (codeSet != null)
        {
            context.IRCodeSets.Remove(codeSet);
            await context.SaveChangesAsync();
        }
    }

    public async Task<bool> ImportCodeSetAsync(string filePath)
    {
        try
        {
            var json = await File.ReadAllTextAsync(filePath);
            var importData = JsonSerializer.Deserialize<IRCodeSetImport>(json);
            
            if (importData == null) return false;

            var codeSet = new IRCodeSet
            {
                Brand = importData.Brand,
                Model = importData.Model,
                DeviceType = importData.DeviceType,
                Description = importData.Description,
                IsVerified = false,
                Codes = importData.Codes.Select(c => new IRCode
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

        var exportData = new IRCodeSetImport
        {
            Brand = codeSet.Brand,
            Model = codeSet.Model,
            DeviceType = codeSet.DeviceType,
            Description = codeSet.Description,
            Codes = codeSet.Codes.Select(c => new IRCodeImport
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
        if (await context.IRCodeSets.AnyAsync())
        {
            return; // Already seeded
        }

        logger.LogInformation("Seeding default IR codes...");

        var defaultCodeSets = GetDefaultCodeSets();
        
        foreach (var codeSet in defaultCodeSets)
        {
            await CreateCodeSetAsync(codeSet);
        }

        logger.LogInformation("Seeded {Count} default IR code sets", defaultCodeSets.Count);
    }

    private static List<IRCodeSet> GetDefaultCodeSets()
    {
        return
        [
            new IRCodeSet
            {
                Brand = "Samsung",
                Model = "Generic TV",
                DeviceType = DeviceType.Television,
                Description = "Generic Samsung TV IR codes",
                IsVerified = true,
                Codes =
                [
                    new IRCode { CommandName = "Power", Protocol = "NEC", HexCode = "0xE0E040BF", Frequency = 38000 },
                    new IRCode
                    {
                        CommandName = "VolumeUp", Protocol = "NEC", HexCode = "0xE0E0E01F", Frequency = 38000
                    },

                    new IRCode
                    {
                        CommandName = "VolumeDown", Protocol = "NEC", HexCode = "0xE0E0D02F", Frequency = 38000
                    },

                    new IRCode { CommandName = "Mute", Protocol = "NEC", HexCode = "0xE0E0F00F", Frequency = 38000 },
                    new IRCode
                    {
                        CommandName = "DirectionalUp", Protocol = "NEC", HexCode = "0xE0E006F9", Frequency = 38000
                    },

                    new IRCode
                    {
                        CommandName = "DirectionalDown", Protocol = "NEC", HexCode = "0xE0E08679", Frequency = 38000
                    },

                    new IRCode
                    {
                        CommandName = "DirectionalLeft", Protocol = "NEC", HexCode = "0xE0E0A659", Frequency = 38000
                    },

                    new IRCode
                    {
                        CommandName = "DirectionalRight", Protocol = "NEC", HexCode = "0xE0E046B9", Frequency = 38000
                    },

                    new IRCode { CommandName = "OK", Protocol = "NEC", HexCode = "0xE0E016E9", Frequency = 38000 },
                    new IRCode { CommandName = "Menu", Protocol = "NEC", HexCode = "0xE0E058A7", Frequency = 38000 },
                    new IRCode { CommandName = "Back", Protocol = "NEC", HexCode = "0xE0E01AE5", Frequency = 38000 },
                    new IRCode { CommandName = "Home", Protocol = "NEC", HexCode = "0xE0E079F6", Frequency = 38000 }
                ]
            },

            // LG TV

            new IRCodeSet
            {
                Brand = "LG",
                Model = "Generic TV",
                DeviceType = DeviceType.Television,
                Description = "Generic LG TV IR codes",
                IsVerified = true,
                Codes =
                [
                    new IRCode { CommandName = "Power", Protocol = "NEC", HexCode = "0x20DF10EF", Frequency = 38000 },
                    new IRCode
                    {
                        CommandName = "VolumeUp", Protocol = "NEC", HexCode = "0x20DF40BF", Frequency = 38000
                    },

                    new IRCode
                    {
                        CommandName = "VolumeDown", Protocol = "NEC", HexCode = "0x20DFC03F", Frequency = 38000
                    },

                    new IRCode { CommandName = "Mute", Protocol = "NEC", HexCode = "0x20DF906F", Frequency = 38000 },
                    new IRCode
                    {
                        CommandName = "DirectionalUp", Protocol = "NEC", HexCode = "0x20DF02FD", Frequency = 38000
                    },

                    new IRCode
                    {
                        CommandName = "DirectionalDown", Protocol = "NEC", HexCode = "0x20DF827D", Frequency = 38000
                    },

                    new IRCode
                    {
                        CommandName = "DirectionalLeft", Protocol = "NEC", HexCode = "0x20DFE01F", Frequency = 38000
                    },

                    new IRCode
                    {
                        CommandName = "DirectionalRight", Protocol = "NEC", HexCode = "0x20DF609F", Frequency = 38000
                    },

                    new IRCode { CommandName = "OK", Protocol = "NEC", HexCode = "0x20DF22DD", Frequency = 38000 },
                    new IRCode { CommandName = "Menu", Protocol = "NEC", HexCode = "0x20DFC23D", Frequency = 38000 },
                    new IRCode { CommandName = "Back", Protocol = "NEC", HexCode = "0x20DF14EB", Frequency = 38000 },
                    new IRCode { CommandName = "Home", Protocol = "NEC", HexCode = "0x20DF3EC1", Frequency = 38000 }
                ]
            },

            // Sony TV

            new IRCodeSet
            {
                Brand = "Sony",
                Model = "Generic TV",
                DeviceType = DeviceType.Television,
                Description = "Generic Sony TV IR codes",
                IsVerified = true,
                Codes =
                [
                    new IRCode { CommandName = "Power", Protocol = "SONY", HexCode = "0xA90", Frequency = 40000 },
                    new IRCode { CommandName = "VolumeUp", Protocol = "SONY", HexCode = "0x490", Frequency = 40000 },
                    new IRCode { CommandName = "VolumeDown", Protocol = "SONY", HexCode = "0xC90", Frequency = 40000 },
                    new IRCode { CommandName = "Mute", Protocol = "SONY", HexCode = "0x290", Frequency = 40000 },
                    new IRCode
                    {
                        CommandName = "DirectionalUp", Protocol = "SONY", HexCode = "0x2F0", Frequency = 40000
                    },

                    new IRCode
                    {
                        CommandName = "DirectionalDown", Protocol = "SONY", HexCode = "0xAF0", Frequency = 40000
                    },

                    new IRCode
                    {
                        CommandName = "DirectionalLeft", Protocol = "SONY", HexCode = "0x2D0", Frequency = 40000
                    },

                    new IRCode
                    {
                        CommandName = "DirectionalRight", Protocol = "SONY", HexCode = "0xCD0", Frequency = 40000
                    },

                    new IRCode { CommandName = "OK", Protocol = "SONY", HexCode = "0xA70", Frequency = 40000 },
                    new IRCode { CommandName = "Menu", Protocol = "SONY", HexCode = "0x070", Frequency = 40000 },
                    new IRCode { CommandName = "Back", Protocol = "SONY", HexCode = "0x870", Frequency = 40000 },
                    new IRCode { CommandName = "Home", Protocol = "SONY", HexCode = "0x670", Frequency = 40000 }
                ]
            }
        ];
    }
}

public class IRCodeSetImport
{
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public DeviceType DeviceType { get; set; }
    public string? Description { get; set; }
    public List<IRCodeImport> Codes { get; set; } = [];
}

public class IRCodeImport
{
    public string CommandName { get; set; } = string.Empty;
    public string Protocol { get; set; } = string.Empty;
    public string HexCode { get; set; } = string.Empty;
    public int Frequency { get; set; } = 38000;
    public string? RawData { get; set; }
    public string? Notes { get; set; }
}