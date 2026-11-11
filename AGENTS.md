# Working in this repository

Read `README.md` and `docs/setup.md` before setting up the tool or changing behavior.

- Use Windows PowerShell 5.1 (`powershell.exe`). The entry script compiles `src/*.cs` at startup; there are no package-install or separate build steps.
- Preserve an existing `profiles.json`. Discover this computer's monitor IDs and input values; do not assume the example matches its wiring.
- For a read-only check, run `powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\MonitorInputSwitch.ps1 -List`. This compiles the helpers and reads inputs; it does not test switching.
- Coordinate display-switch tests with the user and follow the recovery steps in `docs/setup.md`. Verify both directions and the visible result before reporting success.
- Check for an existing listener and shortcut conflicts before launching another instance. Add startup behavior only when requested.
- Keep changes focused and update the docs when behavior changes. Do not commit local profiles, logs, or personal paths. Report which checks ran and any hardware checks still needed.
