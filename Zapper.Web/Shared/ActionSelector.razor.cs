using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Zapper.Core.Devices;

namespace Zapper.Web.Shared;

public partial class ActionSelector
{
    [Parameter]
    public Core.Devices.Device SelectedDevice { get; set; }
    
    public DeviceAction SelectedAction { get; set; }

    private string SearchText
    {
        get => _searchText;
        set
        {
            if (_searchText == value)
                return;
            
            _searchText = value;

            _filteredActions = GetFilteredActions();
        }
    }

    private IEnumerable<DeviceAction> GetFilteredActions()
    {
        if (string.IsNullOrWhiteSpace(_searchText))
        {
            return SelectedDevice.AvailableActions;
        }

        return SelectedDevice.AvailableActions
            .Where(a => Contains(a.Action, _searchText) || a.Action == _searchText);
    }

    private IEnumerable<DeviceAction> _filteredActions = new List<DeviceAction>();
    private string _searchText;

    [Inject] 
    public ILogger<ActionSelector> Logger { get; set; }

    private void SelectAction(DeviceAction deviceAction)
    {
        try
        {
            SelectedAction = deviceAction;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error occured while selecting device action");
        }
    }

    protected override void OnInitialized()
    {
        _filteredActions = GetFilteredActions();
    }

    public void Clear()
    {
        SelectedAction = null;
    }
    
    private static bool Contains(
        string source, 
        string toCheck, 
        StringComparison comp = StringComparison.InvariantCultureIgnoreCase)
    {
        return source?.IndexOf(toCheck, comp) >= 0;
    }
}