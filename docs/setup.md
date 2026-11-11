# Set up your monitors

Start with both computers awake and all shared monitors showing the computer you want to control from. Keep your keyboard connected to that computer while testing. Leave the monitors' physical input controls within reach so you can restore the picture if a switch fails.

The examples run from the repository folder. Use Windows PowerShell 5.1 (`powershell.exe`); the utility compiles its C# helpers against the Windows .NET Framework when it starts.

## Find the input values

Enable DDC/CI in each monitor's on-screen menu, then run:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\MonitorInputSwitch.ps1 -List
```

Each line gives a model ID and the selected input in decimal and hexadecimal:

```text
VSC613B: 17 (0x11)
```

Record `VSC613B` as the model ID and `17` as the input for the computer currently displayed. Use your actual output, even if your monitor has the same product name as an example.

Next, prepare the command below in PowerShell **without pressing Enter**. Use the monitor's physical menu to select the other computer, then press Enter with the keyboard still connected to the original computer. After the command finishes, manually switch back and open `%TEMP%\monitor-inputs.txt` to record the new value:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\MonitorInputSwitch.ps1 -List > "$env:TEMP\monitor-inputs.txt"
```

If the monitor stops responding while it displays the other input, return using its physical menu. You can read the other input from the second computer, but that does not prove the first computer will be able to switch it back. Test the full round trip before relying on the shortcut.

Repeat for each shared monitor. Do not assume an HDMI or DisplayPort label has the same numeric value on every display.

## Edit the configuration

Copy `profiles.example.json` to `profiles.json` if you have not already done so. For one monitor, the file could look like this:

```json
{
  "Monitors": [
    {
      "Name": "Desk monitor",
      "ModelId": "VSC613B",
      "Laptop": 15,
      "Desktop": 17
    }
  ]
}
```

These are example values. Replace them with the values you recorded.

| Field | Meaning |
| --- | --- |
| `Name` | A friendly name used in error messages. |
| `ModelId` | The ID printed by `-List` on the computer running the utility. |
| `Laptop` | Decimal input value for the first computer. |
| `Desktop` | Decimal input value for the second computer. |

Add one entry per shared monitor, separated by commas. Remove entries for monitors you do not use. Both input values must be integers from 0 to 255, and they must differ. `Laptop` and `Desktop` are fixed field names, even for two desktops or two laptops.

Two monitors reporting the same model ID are not supported. Each Windows display must also map to exactly one physical monitor.

`profiles.json` is ignored by Git. When setting up the second computer, discover its IDs separately and keep its configuration local.

## Test both directions

1. Start with all shared monitors showing the computer running the utility. Confirm their inputs with `-List`.
2. Double-click `Start-MonitorInputSwitch.cmd`. Check for its tray icon, or for `Listening for Ctrl+Alt+M` in the log.
3. Press **Ctrl+Alt+M**. Allow the monitors to settle and confirm that each shows the other computer. Keep the keyboard connected to the original computer.
4. Press **Ctrl+Alt+M** again. Confirm that every monitor returns.
5. Check the log for `Verified profile: Laptop` and `Verified profile: Desktop`:

```powershell
Get-Content "$env:LOCALAPPDATA\MonitorInputSwitch\monitor-switch.log" -Tail 20
```

These entries confirm the input values reported by the monitors. Also check the actual picture: the destination computer must be awake and sending a signal.

If a screen stays on the wrong input, restore it using its physical menu. Run `-List` and check the configuration and log before retrying. Input switching is sequential, so one monitor can change even if another fails.

You can also select either profile directly. Run **one command at a time**; each changes the screens immediately.

To select the laptop:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\MonitorInputSwitch.ps1 -Target Laptop
```

To select the desktop:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\MonitorInputSwitch.ps1 -Target Desktop
```

Only run one listener per computer. To use the shortcut from either computer, configure and test each independently, with the keyboard connected to the computer whose listener you are using.

## Start at sign-in (optional)

After the round-trip test passes:

1. Keep the repository in a permanent folder.
2. Press **Win+R**, enter `shell:startup`, and press Enter.
3. Create a shortcut in that folder pointing to `Start-MonitorInputSwitch.cmd`. Create a shortcut to the file; do not move the launcher itself.

The utility will start when you sign in. Remove that shortcut to stop automatic startup. If you move the repository folder, update the shortcut's target.

The launcher and commands use `-ExecutionPolicy Bypass` for the new PowerShell process only. They do not change a machine-wide or user-wide execution policy. If a managed computer blocks scripts, follow the organization's software policy.
