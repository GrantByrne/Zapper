using Microsoft.AspNetCore.Components;

namespace Zapper.Blazor.Pages.Settings;

public partial class Index : ComponentBase
{
    [Inject] private NavigationManager Navigation { get; set; } = null!;

    private void NavigateToSettings(string section)
    {
        Navigation.NavigateTo($"/settings/{section}");
    }
}