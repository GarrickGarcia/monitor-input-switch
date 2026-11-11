using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonitorInputSwitch
{
    public static class HotkeyApp
    {
        public static void Run(InputProfiles profiles)
        {
            using (var app = new TrayContext(profiles))
                Application.Run(app);
        }

        private sealed class TrayContext : ApplicationContext
        {
            private readonly InputProfiles profiles;
            private readonly HotkeyWindow window;
            private readonly NotifyIcon tray;
            private readonly ContextMenuStrip menu;
            private readonly ToolStripMenuItem exitItem;
            private bool switching;

            public TrayContext(InputProfiles profiles)
            {
                this.profiles = profiles;
                window = new HotkeyWindow(OnHotkey);
                menu = new ContextMenuStrip();
                exitItem = new ToolStripMenuItem("Exit Monitor Input Switch");
                exitItem.Click += (sender, args) => ExitThread();
                menu.Items.Add(exitItem);
                tray = new NotifyIcon
                {
                    Icon = SystemIcons.Application,
                    Text = "Monitor Input Switch (Ctrl+Alt+M)",
                    ContextMenuStrip = menu,
                    Visible = true
                };
                SwitchLog.Write("Listening for Ctrl+Alt+M");
            }

            private async void OnHotkey()
            {
                if (switching)
                    return;

                switching = true;
                exitItem.Enabled = false;
                SwitchLog.Write("Received Ctrl+Alt+M");

                try
                {
                    await Task.Run(() => profiles.Toggle());
                }
                catch (Exception error)
                {
                    SwitchLog.Write(error.Message);
                    tray.ShowBalloonTip(5000, "Monitor switch incomplete", error.Message, ToolTipIcon.Warning);
                }
                finally
                {
                    switching = false;
                    exitItem.Enabled = true;
                }
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    tray.Dispose();
                    menu.Dispose();
                    window.Dispose();
                }
                base.Dispose(disposing);
            }
        }

        private sealed class HotkeyWindow : NativeWindow, IDisposable
        {
            private const int HotkeyMessage = 0x0312;
            private const int HotkeyId = 1;
            private const uint Alt = 0x0001;
            private const uint Control = 0x0002;
            private const uint NoRepeat = 0x4000;

            private readonly Action onHotkey;

            [DllImport("user32.dll", SetLastError = true)]
            private static extern bool RegisterHotKey(IntPtr window, int id, uint modifiers, uint key);

            [DllImport("user32.dll")]
            private static extern bool UnregisterHotKey(IntPtr window, int id);

            public HotkeyWindow(Action onHotkey)
            {
                this.onHotkey = onHotkey;
                CreateHandle(new CreateParams { Caption = "Monitor Input Switch" });
                if (!RegisterHotKey(Handle, HotkeyId, Control | Alt | NoRepeat, (uint)Keys.M))
                {
                    int error = Marshal.GetLastWin32Error();
                    DestroyHandle();
                    throw new Win32Exception(error, "Could not register Ctrl+Alt+M. Another app may be using it.");
                }
            }

            protected override void WndProc(ref Message message)
            {
                if (message.Msg == HotkeyMessage && message.WParam.ToInt32() == HotkeyId)
                    onHotkey();
                else
                    base.WndProc(ref message);
            }

            public void Dispose()
            {
                if (Handle != IntPtr.Zero)
                {
                    UnregisterHotKey(Handle, HotkeyId);
                    DestroyHandle();
                }
            }
        }
    }
}
