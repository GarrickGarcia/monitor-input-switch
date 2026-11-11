# Monitor Input Switch

Press **Ctrl+Alt+M** to switch shared monitors between two computers. The utility sends DDC/CI input-select commands over the existing display cables using the Windows monitor configuration API.

## Set up

Use Windows 10 or 11 with DDC/CI enabled on the monitors. Keep the computer whose inputs you want to select awake.

1. Copy `profiles.example.json` to `profiles.json`.
2. From Windows PowerShell in the checkout, run `.\MonitorInputSwitch.ps1 -List`. Confirm the model IDs. For other wiring, use the monitors’ physical input menus to select each computer and record the decimal input values reported by `-List`.
3. Edit `profiles.json` to match the inputs and double-click `Start-MonitorInputSwitch.cmd`. The notification area icon confirms the utility is running. Right-click it to exit.

The example uses DisplayPort 1 and HDMI 2 for the laptop, and HDMI 1 and DisplayPort for the desktop, on a ViewSonic VX3418-2KPC and a Gigabyte AORUS CV27Q respectively.

For a fresh Codex session on the desktop, follow the [desktop setup guide](docs/desktop-setup.md).

## Switch

Press **Ctrl+Alt+M** on the computer running the utility. When it runs on the laptop, keep the keyboard connected to the laptop while the monitors show the desktop. Press the same shortcut to return.

The utility verifies the reported input of each monitor after switching. If a handoff partially succeeds, pressing the shortcut again retries that profile. When the program starts with mixed inputs, the shortcut selects the laptop profile. A monitor may briefly go dark as it acquires the new signal.

For troubleshooting, run `.\MonitorInputSwitch.ps1 -Target Laptop` or `.\MonitorInputSwitch.ps1 -Target Desktop` to select and verify a profile directly. The log is in `%LOCALAPPDATA%\MonitorInputSwitch\monitor-switch.log`. Test from each PC where you plan to run the utility, since DDC/CI access on inactive inputs varies by monitor and connection.
