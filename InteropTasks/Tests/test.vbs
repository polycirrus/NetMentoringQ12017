Dim manager
Set manager = CreateObject("PowerStateManagement.PowerStateManager")

Dim powerInfo
Set powerInfo = manager.GetPowerInformation

WScript.Echo powerInfo.TimeRemaining