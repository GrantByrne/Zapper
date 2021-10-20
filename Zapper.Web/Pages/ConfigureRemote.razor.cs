using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Components;
using Serilog;
using Zapper.Core.KeyboardMouse;
using Zapper.Web.Data;
using Zapper.Web.Data.Abstract;
using Zapper.Web.Shared;

namespace Zapper.Web.Pages
{
    public partial class ConfigureRemote : ComponentBase
    {
        private Dictionary<string, RemoteButton> _buttons = new();
        private RemoteButton _selectedButton;
        private bool _scanning;
        private string _scanButtonText = "Scan for Input";
        private Device[] _devices = Array.Empty<Device>();
        private Device _selectedDevice;
        private DeviceAction _selectedAction;
        private bool _dirty;

        private Modal Modal { get; set; }
        
        [Inject]
        public IAggregateInputReader AggregateInputReader { get; set; }
        
        [Inject]
        public IDeviceManager DeviceManager { get; set; }
        
        [Inject]
        public IRemoteManager RemoteManager { get; set; }

        protected override void OnInitialized()
        {
            var buttons = RemoteManager.Get();

            _buttons = buttons.ToDictionary(p => p.Name);
            
            AggregateInputReader.OnKeyPress += AddButton;
        }

        private void AddButton(KeyPressEvent e)
        {
            try
            {
                if (!_scanning)
                {
                    Log.Information("Not scanning for keypresses, so moving on");
                    return;
                }

                Log.Information("Received a key press event update");

                var val = e.Code.ToString();
                if (_buttons.ContainsKey(val))
                {
                    Log.Information($"The keypress {val} is already registered. Moving on.");
                    return;
                }

                Log.Information($"The keypress {val} is new. Registering it.");

                _buttons.Add(val, new RemoteButton { Name = val });
                _dirty = true;
                InvokeAsync(StateHasChanged);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error occurred while processing button event");
            }
        }

        private void SelectButton(RemoteButton button)
        {
            try
            {
                if (_scanning)
                    return;
        
                _selectedButton = button;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error occured while try to select a button to focus");
            }
        }

        private void Scan()
        {
            try
            {
                _scanning = !_scanning;
                _selectedButton = null;

                _scanButtonText = _scanning ? "Scanning" : "Scan for Input";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error occurred while stopping/starting a scan");   
            }
        }

        private void AddAction()
        {
            try
            {
                var devices = DeviceManager.Get();
                _devices = devices;
                
                Modal.Open();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error occurred while trying to bring up the modal to selection an action");
            }
        }

        private void SelectDevice(Device device)
        {
            try
            {
                _selectedDevice = device;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error occurred while selecting device");
            }
        }

        private void SelectAction(DeviceAction deviceAction)
        {
            try
            {
                _selectedAction = deviceAction;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error occured while selecting device action");
            }
        }

        private void SaveChanges()
        {
            try
            {
                var buttons = _buttons.Values;
                RemoteManager.Update(buttons);
                _dirty = false;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error occurred while trying to save changes");
            }
        }

        private void AddActionToButton()
        {
            try
            {
                _selectedButton.Action = _selectedAction.Action;
                _dirty = true;
                Modal.Close();
                _selectedAction = null;
                _selectedDevice = null;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while trying to add an action to a button");
            }
        }

        private void CloseAddActionButton()
        {
            try
            {
                Modal.Close();
                _selectedAction = null;
                _selectedDevice = null;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to close the add action modal");
            }
        }

        private void RemoveButton()
        {
            try
            {
                _buttons.Remove(_selectedButton.Name);
                _selectedButton = null;
                _dirty = true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error occurred while trying to remove the button");
            }
        }
    }
}