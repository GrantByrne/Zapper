using Microsoft.AspNetCore.Components;

namespace Zapper.Web.Shared;

public partial class Modal
{
    [Parameter]
    public RenderFragment Title { get; set; }

    [Parameter]
    public RenderFragment Body { get; set; }

    [Parameter]
    public RenderFragment Footer { get; set; }

    private string _modalDisplay = "none;";
    private string _modalClass = "";
    private bool _showBackdrop = false;

    public void Open()
    {
        _modalDisplay = "block;";
        _modalClass = "show";
        _showBackdrop = true;
        
        StateHasChanged();
    }

    public void Close()
    {
        _modalDisplay = "none";
        _modalClass = "";
        _showBackdrop = false;
        
        StateHasChanged();
    }
}