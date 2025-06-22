using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using Zapper.Core.Models;
using Zapper.Services;

namespace Zapper.Endpoints;

public class GetIRCodeSetsEndpoint : EndpointWithoutRequest<IEnumerable<IRCodeSet>>
{
    public IIRCodeService IRCodeService { get; set; } = null!;

    public override void Configure()
    {
        Get("/api/ir-codes/sets");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var codeSets = await IRCodeService.GetCodeSetsAsync();
        await SendOkAsync(codeSets, ct);
    }
}

public class SearchIRCodeSetsRequest
{
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public DeviceType? DeviceType { get; set; }
}

public class SearchIRCodeSetsEndpoint : Endpoint<SearchIRCodeSetsRequest, IEnumerable<IRCodeSet>>
{
    public IIRCodeService IRCodeService { get; set; } = null!;

    public override void Configure()
    {
        Get("/api/ir-codes/sets/search");
        AllowAnonymous();
    }

    public override async Task HandleAsync(SearchIRCodeSetsRequest req, CancellationToken ct)
    {
        var codeSets = await IRCodeService.SearchCodeSetsAsync(req.Brand, req.Model, req.DeviceType);
        await SendOkAsync(codeSets, ct);
    }
}

public class GetIRCodeSetRequest
{
    public int Id { get; set; }
}

public class GetIRCodeSetEndpoint : Endpoint<GetIRCodeSetRequest, IRCodeSet?>
{
    public IIRCodeService IRCodeService { get; set; } = null!;

    public override void Configure()
    {
        Get("/api/ir-codes/sets/{id}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(GetIRCodeSetRequest req, CancellationToken ct)
    {
        var codeSet = await IRCodeService.GetCodeSetAsync(req.Id);
        if (codeSet == null)
        {
            await SendNotFoundAsync(ct);
            return;
        }
        
        await SendOkAsync(codeSet, ct);
    }
}

public class GetIRCodesRequest
{
    public int CodeSetId { get; set; }
}

public class GetIRCodesEndpoint : Endpoint<GetIRCodesRequest, IEnumerable<IRCode>>
{
    public IIRCodeService IRCodeService { get; set; } = null!;

    public override void Configure()
    {
        Get("/api/ir-codes/sets/{codeSetId}/codes");
        AllowAnonymous();
    }

    public override async Task HandleAsync(GetIRCodesRequest req, CancellationToken ct)
    {
        var codes = await IRCodeService.GetCodesAsync(req.CodeSetId);
        await SendOkAsync(codes, ct);
    }
}

public class GetIRCodeRequest
{
    public int CodeSetId { get; set; }
    public string CommandName { get; set; } = string.Empty;
}

public class GetIRCodeEndpoint : Endpoint<GetIRCodeRequest, IRCode?>
{
    public IIRCodeService IRCodeService { get; set; } = null!;

    public override void Configure()
    {
        Get("/api/ir-codes/sets/{codeSetId}/codes/{commandName}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(GetIRCodeRequest req, CancellationToken ct)
    {
        var code = await IRCodeService.GetCodeAsync(req.CodeSetId, req.CommandName);
        if (code == null)
        {
            await SendNotFoundAsync(ct);
            return;
        }
        
        await SendOkAsync(code, ct);
    }
}

public class CreateIRCodeSetEndpoint : Endpoint<IRCodeSet, IRCodeSet>
{
    public IIRCodeService IRCodeService { get; set; } = null!;

    public override void Configure()
    {
        Post("/api/ir-codes/sets");
        AllowAnonymous();
    }

    public override async Task HandleAsync(IRCodeSet req, CancellationToken ct)
    {
        var codeSet = await IRCodeService.CreateCodeSetAsync(req);
        await SendCreatedAtAsync<GetIRCodeSetEndpoint>(new { Id = codeSet.Id }, codeSet, cancellation: ct);
    }
}

public class AddIRCodeRequest
{
    public int CodeSetId { get; set; }
    public IRCode Code { get; set; } = null!;
}

public class AddIRCodeEndpoint : Endpoint<AddIRCodeRequest, IRCode>
{
    public IIRCodeService IRCodeService { get; set; } = null!;

    public override void Configure()
    {
        Post("/api/ir-codes/sets/{codeSetId}/codes");
        AllowAnonymous();
    }

    public override async Task HandleAsync(AddIRCodeRequest req, CancellationToken ct)
    {
        try
        {
            var code = await IRCodeService.AddCodeAsync(req.CodeSetId, req.Code);
            await SendOkAsync(code, ct);
        }
        catch (ArgumentException ex)
        {
            AddError(ex.Message);
            await SendErrorsAsync(cancellation: ct);
        }
    }
}

public class DeleteIRCodeSetRequest
{
    public int Id { get; set; }
}

public class DeleteIRCodeSetEndpoint : Endpoint<DeleteIRCodeSetRequest>
{
    public IIRCodeService IRCodeService { get; set; } = null!;

    public override void Configure()
    {
        Delete("/api/ir-codes/sets/{id}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(DeleteIRCodeSetRequest req, CancellationToken ct)
    {
        await IRCodeService.DeleteCodeSetAsync(req.Id);
        await SendNoContentAsync(ct);
    }
}

public class ExportIRCodeSetRequest
{
    public int Id { get; set; }
}

public class ExportIRCodeSetEndpoint : Endpoint<ExportIRCodeSetRequest, string>
{
    public IIRCodeService IRCodeService { get; set; } = null!;

    public override void Configure()
    {
        Get("/api/ir-codes/sets/{id}/export");
        AllowAnonymous();
    }

    public override async Task HandleAsync(ExportIRCodeSetRequest req, CancellationToken ct)
    {
        try
        {
            var json = await IRCodeService.ExportCodeSetAsync(req.Id);
            
            HttpContext.Response.ContentType = "application/json";
            HttpContext.Response.Headers["Content-Disposition"] = $"attachment; filename=\"ir-codes-{req.Id}.json\"";
            
            await SendStringAsync(json, cancellation: ct);
        }
        catch (ArgumentException ex)
        {
            AddError(ex.Message);
            await SendErrorsAsync(cancellation: ct);
        }
    }
}

public class SeedDefaultCodesEndpoint : EndpointWithoutRequest
{
    public IIRCodeService IRCodeService { get; set; } = null!;

    public override void Configure()
    {
        Post("/api/ir-codes/seed");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await IRCodeService.SeedDefaultCodesAsync();
        await SendOkAsync("Default IR codes seeded successfully", ct);
    }
}