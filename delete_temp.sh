#!/bin/bash
# Temporary script to delete old settings files

# Delete the SettingsPages directory
rm -rf /home/grant/Code/personal/zapper-next-gen/src/Zapper.Blazor/Pages/SettingsPages/

# Delete empty settings files
rm -f /home/grant/Code/personal/zapper-next-gen/src/Zapper.Blazor/Pages/Settings.razor
rm -f /home/grant/Code/personal/zapper-next-gen/src/Zapper.Blazor/Pages/Settings.razor.cs
rm -f /home/grant/Code/personal/zapper-next-gen/src/Zapper.Blazor/Pages/SettingsOld.razor
rm -f /home/grant/Code/personal/zapper-next-gen/src/Zapper.Blazor/Pages/SettingsOld.razor.cs

echo "Cleanup completed successfully"