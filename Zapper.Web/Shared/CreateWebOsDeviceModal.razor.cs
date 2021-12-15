using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Zapper.WebOs;

namespace Zapper.Web.Shared;

public partial class CreateWebOsDeviceModal
{
    [Parameter]
    public Action OnSaveChanges { get; set; }
    
    private string _name;
    private string _ipAddress;
    private string _macAddress;

    private string _connectionTestSummary;
    private bool _connectTestSuccess;
    private Modal _model;

    private async Task TestConnection()
    {
        try
        {
            _logger.LogInformation("Testing out the WebOS Connection");

            var service = new Service();
            await service.ConnectAsync(_ipAddress);

            _connectionTestSummary = $"Successfully connected to {_ipAddress}";
            _connectTestSuccess = true;

            _logger.LogInformation("Finished testing the WebOS Connection");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while testing the WebOS Connection");
            _connectTestSuccess = false;
            _connectionTestSummary = ex.Message;
        }
    }

    private void SaveChanges()
    {
        try
        {
            _logger.LogInformation("Attempting to save changes");
            
            _deviceManager.CreateWebOsDevice(_name, _ipAddress, _macAddress);

            _name = "";
            _ipAddress = "";
            _macAddress = "";

            _model.Close();

            OnSaveChanges?.Invoke();
            
            _logger.LogInformation("Finished saving changes");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save changes");
        }
    }

    public void Open()
    {
        _model.Open();
    }
}