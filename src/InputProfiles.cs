using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace MonitorInputSwitch
{
    public enum Profile { Laptop, Desktop }

    public sealed class MonitorRoute
    {
        public readonly string Name;
        public readonly string ModelId;
        private readonly uint laptop;
        private readonly uint desktop;

        public MonitorRoute(string name, string modelId, uint laptop, uint desktop)
        {
            Name = name;
            ModelId = modelId;
            this.laptop = laptop;
            this.desktop = desktop;
        }

        public uint InputFor(Profile profile)
        {
            return profile == Profile.Laptop ? laptop : desktop;
        }
    }

    public sealed class InputProfiles
    {
        private static readonly TimeSpan SettleTimeout = TimeSpan.FromSeconds(8);
        private const int PollIntervalMilliseconds = 500;

        private readonly MonitorRoute[] routes;
        private Profile? pendingProfile;

        public InputProfiles(MonitorRoute[] routes)
        {
            this.routes = routes;
        }

        public static string[] ListInputs()
        {
            var lines = new List<string>();
            foreach (string modelId in DisplayInput.ListModelIds())
            {
                try
                {
                    uint value = DisplayInput.Read(modelId);
                    lines.Add(String.Format("{0}: {1} (0x{1:X2})", modelId, value));
                }
                catch (InvalidOperationException error)
                {
                    lines.Add(error.Message);
                }
            }
            return lines.ToArray();
        }

        public Profile Toggle()
        {
            uint[] current = new uint[routes.Length];
            WaitForInputs(current, null);

            Profile target = Profile.Laptop;
            if (Matches(current, Profile.Laptop))
                target = Profile.Desktop;
            else if (!Matches(current, Profile.Desktop) && pendingProfile.HasValue)
                target = pendingProfile.Value;

            Apply(target, current);
            return target;
        }

        public void Select(Profile profile)
        {
            uint[] current = new uint[routes.Length];
            WaitForInputs(current, null);
            Apply(profile, current);
        }

        private bool Matches(uint[] current, Profile profile)
        {
            for (int i = 0; i < routes.Length; i++)
                if (current[i] != routes[i].InputFor(profile))
                    return false;

            return true;
        }

        private void Apply(Profile profile, uint[] current)
        {
            pendingProfile = profile;
            for (int i = 0; i < routes.Length; i++)
            {
                MonitorRoute route = routes[i];
                uint target = route.InputFor(profile);
                if (current[i] == target)
                    continue;

                try
                {
                    DisplayInput.Write(route.ModelId, target);
                }
                catch (InvalidOperationException error)
                {
                    SwitchLog.Write(error.Message);
                }
            }

            WaitForInputs(current, profile);
            pendingProfile = null;
            SwitchLog.Write("Verified profile: " + profile);
        }

        private void WaitForInputs(uint[] current, Profile? expectedProfile)
        {
            var elapsed = Stopwatch.StartNew();
            var problems = new List<string>();
            do
            {
                problems.Clear();
                for (int i = 0; i < routes.Length; i++)
                {
                    MonitorRoute route = routes[i];
                    try
                    {
                        current[i] = DisplayInput.Read(route.ModelId);
                        if (expectedProfile.HasValue && current[i] != route.InputFor(expectedProfile.Value))
                        {
                            problems.Add(String.Format("{0} reports 0x{1:X2}; expected 0x{2:X2}.",
                                route.Name, current[i], route.InputFor(expectedProfile.Value)));
                        }
                    }
                    catch (InvalidOperationException error)
                    {
                        problems.Add(route.Name + ": " + error.Message);
                    }
                }

                if (problems.Count == 0)
                    return;

                Thread.Sleep(PollIntervalMilliseconds);
            } while (elapsed.Elapsed < SettleTimeout);

            throw new InvalidOperationException(String.Join(" ", problems));
        }
    }

    public static class SwitchLog
    {
        private static readonly string directory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MonitorInputSwitch");

        public static void Write(string message)
        {
            try
            {
                Directory.CreateDirectory(directory);
                File.AppendAllText(Path.Combine(directory, "monitor-switch.log"),
                    DateTimeOffset.Now.ToString("o") + " " + message + Environment.NewLine);
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }
    }
}
