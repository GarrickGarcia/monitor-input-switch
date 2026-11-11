# Desktop setup with Codex

Set up the existing monitor switcher on this Windows desktop. The laptop already runs the same `Ctrl+Alt+M` shortcut. This setup tests whether the desktop can switch both monitors back and forth while the keyboard stays connected to it.

## Install on the desktop

1. Clone the private `GarrickGarcia/monitor-input-switch` repository under `%USERPROFILE%\source\repos` using the personal GitHub account `GarrickGarcia`. Reuse an existing checkout if present.
2. Copy `profiles.example.json` to `profiles.json` if the local configuration is missing. With both monitors showing the desktop, run `.\MonitorInputSwitch.ps1 -List` in Windows PowerShell. Check the model IDs and active input values against this table.

| Monitor | Model ID | Laptop input | Desktop input |
| --- | --- | --- | --- |
| ViewSonic VX3418-2KPC | `VSC613B` | DisplayPort 1 (`15`) | HDMI 1 (`17`) |
| Gigabyte AORUS CV27Q | `GBT2704` | HDMI 2 (`18`) | DisplayPort (`15`) |

3. Run `Start-MonitorInputSwitch.cmd` and verify that the tray icon or log confirms registration of `Ctrl+Alt+M`.

## Test the hotkey

1. Keep both computers awake. Start with both monitors showing the desktop and the keyboard connected to the desktop through the Logitech Lightspeed receiver.
2. Ask me to press `Ctrl+Alt+M` and confirm that both screens show the laptop. Keep the keyboard on Lightspeed.
3. Once the screens settle, ask me to press `Ctrl+Alt+M` again and confirm that both screens return to the desktop.

Check `%LOCALAPPDATA%\MonitorInputSwitch\monitor-switch.log` for both verified profiles. If a screen stays on the laptop, use its physical input menu to restore the desktop input in the table. Check which monitor the desktop can still query over DDC/CI while the monitor shows the laptop.

Report the result for each screen in both directions and leave the utility running when the test passes.
