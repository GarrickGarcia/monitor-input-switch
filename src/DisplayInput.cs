using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace MonitorInputSwitch
{
    internal static class DisplayInput
    {
        private const byte InputSelect = 0x60;

        [StructLayout(LayoutKind.Sequential)]
        private struct Rect
        {
            public int Left, Top, Right, Bottom;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct MonitorInfo
        {
            public uint Size;
            public Rect Monitor, Work;
            public uint Flags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] public string Device;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct DisplayDevice
        {
            public uint Size;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] public string Name;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)] public string Description;
            public uint Flags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)] public string Id;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)] public string Key;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct PhysicalMonitor
        {
            public IntPtr Handle;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)] public string Description;
        }

        private delegate bool MonitorCallback(IntPtr monitor, IntPtr dc, IntPtr bounds, IntPtr state);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool EnumDisplayMonitors(IntPtr dc, IntPtr clip, MonitorCallback callback, IntPtr state);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern bool GetMonitorInfo(IntPtr monitor, ref MonitorInfo info);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern bool EnumDisplayDevices(string device, uint index, ref DisplayDevice info, uint flags);

        [DllImport("dxva2.dll", SetLastError = true)]
        private static extern bool GetNumberOfPhysicalMonitorsFromHMONITOR(IntPtr monitor, out uint count);

        [DllImport("dxva2.dll", SetLastError = true)]
        private static extern bool GetPhysicalMonitorsFromHMONITOR(IntPtr monitor, uint count, [Out] PhysicalMonitor[] monitors);

        [DllImport("dxva2.dll")]
        private static extern bool DestroyPhysicalMonitors(uint count, PhysicalMonitor[] monitors);

        [DllImport("dxva2.dll", SetLastError = true)]
        private static extern bool GetVCPFeatureAndVCPFeatureReply(IntPtr monitor, byte code, IntPtr type, out uint current, IntPtr maximum);

        [DllImport("dxva2.dll", SetLastError = true)]
        private static extern bool SetVCPFeature(IntPtr monitor, byte code, uint value);

        private static Dictionary<string, List<IntPtr>> Discover()
        {
            var displays = new Dictionary<string, List<IntPtr>>(StringComparer.OrdinalIgnoreCase);
            bool enumerated = EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, (handle, dc, bounds, state) =>
            {
                var info = new MonitorInfo { Size = (uint)Marshal.SizeOf(typeof(MonitorInfo)) };
                var device = new DisplayDevice { Size = (uint)Marshal.SizeOf(typeof(DisplayDevice)) };
                if (!GetMonitorInfo(handle, ref info) || !EnumDisplayDevices(info.Device, 0, ref device, 0))
                    return true;

                string[] id = (device.Id ?? "").Split('\\');
                if (id.Length < 2 || id[1].Length == 0)
                    return true;

                List<IntPtr> matches;
                if (!displays.TryGetValue(id[1], out matches))
                {
                    matches = new List<IntPtr>();
                    displays.Add(id[1], matches);
                }
                matches.Add(handle);
                return true;
            }, IntPtr.Zero);

            if (!enumerated)
                throw NativeError("Enumerating Windows displays");

            return displays;
        }

        public static IEnumerable<string> ListModelIds()
        {
            return Discover().Keys;
        }

        public static uint Read(string modelId)
        {
            return WithMonitor(modelId, handle =>
            {
                uint value;
                if (!GetVCPFeatureAndVCPFeatureReply(handle, InputSelect, IntPtr.Zero, out value, IntPtr.Zero))
                    throw NativeError("Reading the input of " + modelId);
                return value;
            });
        }

        public static void Write(string modelId, uint value)
        {
            WithMonitor(modelId, handle =>
            {
                if (!SetVCPFeature(handle, InputSelect, value))
                    throw NativeError("Setting the input of " + modelId);
                return true;
            });
        }

        private static T WithMonitor<T>(string modelId, Func<IntPtr, T> action)
        {
            List<IntPtr> matches;
            if (!Discover().TryGetValue(modelId, out matches))
                throw new InvalidOperationException("Windows could not find monitor " + modelId + ".");
            if (matches.Count != 1)
                throw new InvalidOperationException("More than one Windows display has model ID " + modelId + ".");

            uint count;
            if (!GetNumberOfPhysicalMonitorsFromHMONITOR(matches[0], out count))
                throw NativeError("Counting physical monitors for " + modelId);
            if (count != 1)
                throw new InvalidOperationException(modelId + " must map to exactly one physical monitor.");

            var physical = new PhysicalMonitor[count];
            if (!GetPhysicalMonitorsFromHMONITOR(matches[0], count, physical))
                throw NativeError("Opening the physical monitor " + modelId);

            try
            {
                return action(physical[0].Handle);
            }
            finally
            {
                DestroyPhysicalMonitors(count, physical);
            }
        }

        private static InvalidOperationException NativeError(string operation)
        {
            int error = Marshal.GetLastWin32Error();
            return new InvalidOperationException(String.Format("{0} failed (0x{1:X8}).", operation, error));
        }
    }
}
