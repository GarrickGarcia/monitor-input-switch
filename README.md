# Monitor Input Switch

**One shortcut for the screens you share between two computers.**

Press **Ctrl+Alt+M** to switch your monitors between a laptop and a desktop, then press it again to switch back. Monitor Input Switch is a small Windows tray utility that sends input-selection commands through your existing display cables using DDC/CI.

If you regularly reach for the buttons under your monitors when you change computers, this tool is for you. It supports one or more monitors, reads their current inputs, and checks that each switch succeeded.

[Get started](#get-started) · [Detailed setup](docs/setup.md) · [Troubleshooting](#troubleshooting)

## Before you start

- **Windows 10 or 11**, using the built-in **Windows PowerShell 5.1** (`powershell.exe`). There is no separate build step or package to install.
- Each shared monitor connected to **both computers**, with **DDC/CI enabled** in its on-screen menu.
- Monitors and connections that support DDC/CI input selection. Switching back also requires access over the input that is no longer displaying this computer.
- Both computers awake, with the keyboard connected to the computer running the utility.

This switches **video inputs**. Your keyboard, mouse, and other USB devices stay connected where they are. `Laptop` and `Desktop` are the two profile names; you can use them for any pair of computers.

## Get started

### 1. Download and find your monitors

With Git installed, clone the repository:

```powershell
git clone https://github.com/GarrickGarcia/monitor-input-switch.git
cd monitor-input-switch
```

Alternatively, choose **Code → Download ZIP** on GitHub and extract it to a folder you want to keep.

For either route, open **Windows PowerShell in the folder containing `MonitorInputSwitch.ps1`**, then run:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\MonitorInputSwitch.ps1 -List
```

`-List` only reads the connected monitors. For example:

```text
VSC613B: 17 (0x11)
GBT2705: 15 (0x0F)
```

The value before the colon is the model ID; the decimal number after it is the current input. Your IDs and values may differ.

### 2. Configure the two computers

Copy `profiles.example.json` to `profiles.json`, preserving an existing local configuration:

```powershell
if (-not (Test-Path .\profiles.json)) {
    Copy-Item .\profiles.example.json .\profiles.json
}
```

Edit `profiles.json` with your monitor IDs and the input values for each computer. **The included values describe an example setup, not automatic detection of your wiring.** Use the monitors' physical input menus and `-List` to record the correct values before testing.

The [setup guide](docs/setup.md) walks through finding those values, editing the file, and checking both directions.

### 3. Start switching

Double-click **`Start-MonitorInputSwitch.cmd`**. Look for **Monitor Input Switch (Ctrl+Alt+M)** in the notification area; the icon may be in the hidden-icons menu.

Press **Ctrl+Alt+M**, let the screens settle, then press it again to return. Keep the keyboard connected to the computer running the utility throughout this first test. Right-click the tray icon to exit.

The launcher uses a process-only execution-policy bypass to run the local script. It does not change your saved PowerShell policy; organization-managed policies may still prevent it from running.

## Everyday use

| Action | How |
| --- | --- |
| Switch between profiles | **Ctrl+Alt+M** |
| Read monitor IDs and inputs | `powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\MonitorInputSwitch.ps1 -List` |
| Select a particular computer | Use the same command with `-Target Laptop` or `-Target Desktop` instead of `-List` |
| Use another configuration file | Add `-ConfigPath "C:\path\to\profiles.json"` |
| Stop the utility | Right-click its tray icon and choose **Exit Monitor Input Switch** |

If the monitors start on mixed inputs, the first shortcut selects `Laptop`. After a partial switch, another press retries the pending profile when the monitors are still readable. Errors appear in a tray notification and in `%LOCALAPPDATA%\MonitorInputSwitch\monitor-switch.log`.

To start at sign-in, follow the [optional startup instructions](docs/setup.md#start-at-sign-in-optional).

## Troubleshooting

| What you see | What to check |
| --- | --- |
| No tray icon | Run the script in a visible PowerShell window using the command below to see the error. |
| `Could not register Ctrl+Alt+M` | Another instance or app owns the shortcut. Check existing tray icons and shortcut settings. NVIDIA Overlay's microphone toggle is one known conflict. |
| A monitor is missing or cannot be read | Check its DDC/CI setting, connection, and model ID using `-List`. A dock, adapter, driver, or monitor may limit access. |
| Only some monitors switch | Compare the configured entries in `profiles.json` with the actual inputs. All configured monitors must be readable before a switch starts. |
| Switching back fails | Restore the input using the monitor's physical menu. Test DDC/CI access while the other computer is displayed; some connections cannot control an inactive input. |
| Two monitors report the same model ID | This version cannot distinguish them. Each configured model ID must identify exactly one monitor. |

```powershell
powershell.exe -NoProfile -STA -ExecutionPolicy Bypass -File .\MonitorInputSwitch.ps1
```

Hardware support varies. A successful test from one computer does not establish that switching works from the other. See [testing and recovery](docs/setup.md#test-both-directions) for the full check.

## Contributing

Bug reports, documentation fixes, and compatibility reports are welcome. [Open an issue](https://github.com/GarrickGarcia/monitor-input-switch/issues) with the monitor models, connection types, Windows version, `-List` output, and which direction fails. Review logs for personal information before sharing them.

The PowerShell entry point loads the C# files in `src/` at startup. Keep changes small, describe how you checked them, and keep your local `profiles.json` out of commits.

## License

[MIT](LICENSE) — see the license for terms.
