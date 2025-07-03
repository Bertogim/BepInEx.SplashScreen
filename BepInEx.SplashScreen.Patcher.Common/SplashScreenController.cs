#if !GUI
using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

namespace BepInEx.SplashScreen
{
    public static class SplashScreenController
    {
        internal static readonly ManualLogSource Logger = Logging.Logger.CreateLogSource("Splash");
        private static readonly Queue _StatusQueue = Queue.Synchronized(new Queue(10, 2));
        private static LoadingLogListener _logListener;
        private static Process _guiProcess;

        public static void SpawnSplash()
        {
            try
            {
                var config = new ConfigFile(Path.Combine(Paths.ConfigPath, "Bertogim.LoadingScreen.cfg"), true);

                var isEnabled = config.Bind("LoadingScreen", "Enabled", true, "Display a loading window with information about game load progress on game start-up.").Value;
//#if DEBUG
//                const bool onlyNoConsoleDefault = false;
//#else
//                const bool onlyNoConsoleDefault = true;
//#endif
                //var consoleNotAllowed = config.Bind("LoadingScreen", "OnlyNoConsole", onlyNoConsoleDefault, "Only display the splash screen if the logging console is turned off.").Value;
                
                var windowType = config.Bind("LoadingScreen", "WindowType", "FakeGame", "FakeGame = Makes a window with the same icon as the game, tries to mimic the game till it appears \nFixedWindow = A fixed loading screen on top of all windows, cant move or close and is not on the taskbar (Same behavior as v1.0.5 and less).").Value;
                
                var extraWaitTime = config.Bind("LoadingScreen", "ExtraWaitTime", 1, "Seconds extra to mantain the loading screen starting when the game window shows up \nGood for big modpacks where the lethal company window stays blank loading for a few seconds").Value;

                var windowWidth = config.Bind("LoadingScreen", "WindowWidth", 640, "The window width in pixels (Gets affected by windows screen scale config) \nHeight is automatically calculated by the image aspect ratio.").Value;
                
                var textColor = config.Bind("LoadingScreen", "TextColor", "#FFFFFF", "Text color in hex format (e.g. #FFFFFF for white).").Value;

                var textFont = config.Bind("LoadingScreen", "TextFont", "Segoe UI", "Font name used for the loading text (e.g. Arial, Segoe UI, Consolas). Must match an installed system font.\nFor a list of default Windows fonts, visit: https://learn.microsoft.com/en-us/typography/fonts/windows-core-fonts").Value;
                
                var backgroundColor = config.Bind("LoadingScreen", "BackgroundColor", "#000000", "Background color in hex format (e.g. #000000 for black).").Value;
       

                
                if (!isEnabled)
                {
                    Logger.LogDebug("Not showing splash because the Enabled setting is off");
                    return;
                }

                //if (consoleNotAllowed)
                //{
                //    if (config.TryGetEntry("Logging.Console", "Enabled", out ConfigEntry<bool> entry) && entry.Value)
                //    {
                //        Logger.LogDebug("Not showing splash because the console is enabled");
                //        return;
                //    }
                //}

                var guiExecutablePath = Path.Combine(Path.GetDirectoryName(typeof(SplashScreenController).Assembly.Location) ?? Path.Combine(Paths.PatcherPluginPath, "LoadingScreen"), "LoadingScreen.GUI.exe");

                if (!File.Exists(guiExecutablePath))
                    throw new FileNotFoundException("Executable not found or inaccessible at " + guiExecutablePath);

                Logger.Log(LogLevel.Debug, "Starting GUI process: " + guiExecutablePath);

                var psi = new ProcessStartInfo(guiExecutablePath, Process.GetCurrentProcess().Id.ToString())
                {
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8,
                };
                _guiProcess = Process.Start(psi);

                new Thread(CommunicationThread) { IsBackground = true }.Start(_guiProcess);

                _logListener = LoadingLogListener.StartListening();
            }
            catch (Exception e)
            {
                Logger.LogError("Failed to start GUI: " + e);
                KillSplash();
            }
        }

        internal static void SendMessage(string message)
        {
            _StatusQueue.Enqueue(message);
        }

        private static void CommunicationThread(object processArg)
        {
            try
            {
                var guiProcess = (Process)processArg;

                guiProcess.Exited += (sender, args) => KillSplash();

                guiProcess.OutputDataReceived += (sender, args) =>
                {
                    if (args.Data != null) Logger.Log(LogLevel.Debug, "[GUI] " + args.Data.Replace('\t', '\n').TrimEnd('\n'));
                };
                guiProcess.BeginOutputReadLine();

                guiProcess.ErrorDataReceived += (sender, args) =>
                {
                    if (args.Data != null) Logger.Log(LogLevel.Error, "[GUI] " + args.Data.Replace('\t', '\n').TrimEnd('\n'));
                };
                guiProcess.BeginErrorReadLine();

                guiProcess.StandardInput.AutoFlush = false;

                Logger.LogDebug("Connected to the GUI process");

                var any = false;
                while (!guiProcess.HasExited)
                {
                    while (_StatusQueue.Count > 0 && guiProcess.StandardInput.BaseStream.CanWrite)
                    {
                        guiProcess.StandardInput.WriteLine(_StatusQueue.Dequeue());
                        any = true;
                    }

                    if (any)
                    {
                        any = false;
                        guiProcess.StandardInput.Flush();
                    }

                    Thread.Sleep(150);
                }
            }
            catch (ThreadAbortException)
            {
                // I am die, thank you forever
            }
            catch (Exception e)
            {
                Logger.LogError((object)$"Crash in {nameof(CommunicationThread)}, aborting. Exception: {e}");
            }
            finally
            {
                KillSplash();
            }
        }

        internal static void KillSplash()
        {
            try
            {
                _logListener?.Dispose();

                _StatusQueue.Clear();
                _StatusQueue.TrimToSize();

                try
                {
                    if (_guiProcess != null && !_guiProcess.HasExited)
                    {
                        Logger.LogDebug("Closing GUI process");
                        _guiProcess.Kill();
                    }
                }
                catch (Exception)
                {
                    // _guiProcess already quit so Kill threw
                }

                Logger.Dispose();
                // todo not thread safe
                // Logging.Logger.Sources.Remove(Logger);
            }
            catch (Exception e)
            {
                // Welp, no Logger left to use. This shouldn't ever happen annyways.
                Console.WriteLine(e);
            }
        }
    }
}
#endif