using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Mono.Debugging.Client;
using Mono.Debugging.Soft;
using GodotAddin.Utils;
using GodotTools.IdeMessaging.Requests;
using System.Collections.Generic;
using GodotTools.IdeMessaging;

namespace GodotAddin.Debugging
{
    public class GodotDebuggerSession : SoftDebuggerSession
    {
        private bool _attached;
        private NetworkStream _godotRemoteDebuggerStream;
        private GodotExecutionCommand _godotCmd;
        private Process _process;

        public void SendReloadScripts()
        {
            switch (_godotCmd.ExecutionType)
            {
                case ExecutionType.Launch:
                    GodotVariantEncoder.Encode(
                        new List<GodotVariant> { "reload_scripts" },
                        _godotRemoteDebuggerStream
                    );
                    break;
                case ExecutionType.PlayInEditor:
                case ExecutionType.Attach:
                    _godotCmd.GodotIdeClient.SendRequest<ReloadScriptsResponse>(new ReloadScriptsRequest());
                    break;
                default:
                    throw new NotImplementedException(_godotCmd.ExecutionType.ToString());
            }
        }

        private struct ThreadStartArgs
        {
            public bool IsStdErr;
            public StreamReader Stream;
        }

        private string GetGodotExecutablePath()
        {
            if (Settings.AlwaysUseConfiguredExecutable)
                return Settings.GodotExecutablePath;

            string godotPath = _godotCmd.GodotIdeClient.GodotEditorExecutablePath;

            if (!string.IsNullOrEmpty(godotPath) && File.Exists(godotPath))
            {
                // If the setting is not yet assigned any value, set it to the currently connected Godot editor path
                if (string.IsNullOrEmpty(Settings.GodotExecutablePath))
                    Settings.GodotExecutablePath.Value = godotPath;
                return godotPath;
            }

            return Settings.GodotExecutablePath;
        }

        private void EndSessionWithError(string errorMessage)
        {
            MonoDevelop.Ide.MessageService.ShowError(errorMessage);
            EndSession();
        }

        protected override void OnRun(DebuggerStartInfo startInfo)
        {
            var godotStartInfo = (GodotDebuggerStartInfo)startInfo;

            _godotCmd = godotStartInfo.GodotCmd;

            switch (_godotCmd.ExecutionType)
            {
                case ExecutionType.PlayInEditor:
                {
                    _attached = false;
                    StartListening(godotStartInfo, out var assignedDebugPort);

                    var godotMessagingClient = _godotCmd.GodotIdeClient;

                    if (!godotMessagingClient.IsConnected)
                    {
                        EndSessionWithError("No Godot editor instance connected");
                        return;
                    }

                    string host = "127.0.0.1";

                    var playRequest = new DebugPlayRequest
                    {
                        DebuggerHost = host,
                        DebuggerPort = assignedDebugPort,
                        BuildBeforePlaying = false
                    };

                    _ = godotMessagingClient.SendRequest<DebugPlayResponse>(playRequest)
                        .ContinueWith(t =>
                        {
                            if (t.Result.Status != GodotTools.IdeMessaging.MessageStatus.Ok)
                                EndSessionWithError($"Received Play response with status: {MessageStatus.Ok}");
                        });

                    // TODO: Read the editor player stdout and stderr somehow

                    break;
                }
                case ExecutionType.Launch:
                {
                    _attached = false;
                    StartListening(godotStartInfo, out var assignedDebugPort);

                    // Listener to replace the Godot editor remote debugger.
                    // We use it to notify the game when assemblies should be reloaded.
                    var remoteDebugListener = new TcpListener(IPAddress.Any, 0);
                    remoteDebugListener.Start();
                    _ = remoteDebugListener.AcceptTcpClientAsync().ContinueWith(OnGodotRemoteDebuggerConnected);

                    string workingDir = startInfo.WorkingDirectory;
                    string host = "127.0.0.1";
                    int remoteDebugPort = ((IPEndPoint)remoteDebugListener.LocalEndpoint).Port;

                    // Launch Godot to run the game and connect to our remote debugger

                    var processStartInfo = new ProcessStartInfo(GetGodotExecutablePath())
                    {
                        Arguments = $"--path {workingDir} --remote-debug {host}:{remoteDebugPort}",
                        WorkingDirectory = workingDir,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    // Tells Godot to connect to the mono debugger we just started
                    processStartInfo.EnvironmentVariables["GODOT_MONO_DEBUGGER_AGENT"] =
                        "--debugger-agent=transport=dt_socket" +
                        $",address={host}:{assignedDebugPort}" +
                        ",server=n";

                    _process = new Process { StartInfo = processStartInfo };

                    try
                    {
                        if (!_process.Start())
                        {
                            EndSessionWithError("Failed to start Godot process");
                            return;
                        }
                    }
                    catch (System.ComponentModel.Win32Exception e)
                    {
                        EndSessionWithError($"Failed to start Godot process: {e.Message}");
                        return;
                    }

                    if (_process.HasExited)
                    {
                        EndSessionWithError($"Godot process exited with code: {_process.ExitCode}");
                        return;
                    }

                    // Listen for StdOut and StdErr

                    var stdOutThread = new Thread(OutputReader)
                    {
                        Name = "Godot StandardOutput Reader",
                        IsBackground = true
                    };
                    stdOutThread.Start(new ThreadStartArgs
                    {
                        IsStdErr = false,
                        Stream = _process.StandardOutput
                    });

                    var stdErrThread = new Thread(OutputReader)
                    {
                        Name = "Godot StandardError Reader",
                        IsBackground = true
                    };
                    stdErrThread.Start(new ThreadStartArgs
                    {
                        IsStdErr = true,
                        Stream = _process.StandardError
                    });

                    _process.Exited += (sender, args) => EndSession();

                    OnDebuggerOutput(false, $"Godot PID:{_process.Id}{Environment.NewLine}");

                    break;
                }
                case ExecutionType.Attach:
                {
                    _attached = true;
                    StartConnecting(godotStartInfo);
                    break;
                }
                default:
                    throw new NotImplementedException(_godotCmd.ExecutionType.ToString());
            }
        }

        private async Task OnGodotRemoteDebuggerConnected(Task<TcpClient> task)
        {
            var tcpClient = task.Result;
            _godotRemoteDebuggerStream = tcpClient.GetStream();
            byte[] buffer = new byte[1000];
            while (tcpClient.Connected)
            {
                // There is no library to decode this messages, so
                // we just pump buffer so it doesn't go out of memory
                var readBytes = await _godotRemoteDebuggerStream.ReadAsync(buffer, 0, buffer.Length);
                _ = readBytes;
            }
        }

        protected override bool HandleException(Exception ex)
        {
            // When we attach to running Mono process it sends us AssemblyLoad, ThreadStart and other
            // delayed events, problem is, that we send VM_START command back since we have to do that on
            // every event, but in this case when Mono is sending delayed events when we attach
            // runtime is not really suspended, hence it's throwing this exceptions, just ignore...
            if (_attached && ex is Mono.Debugger.Soft.VMNotSuspendedException)
                return true;
            return base.HandleException(ex);
        }

        protected override void OnExit()
        {
            if (_attached)
            {
                base.OnDetach();
            }
            else
            {
                base.OnExit();

                if (_process != null && !_process.HasExited)
                    _process.Kill();
            }
        }

        private void OutputReader(object args)
        {
            var startArgs = (ThreadStartArgs)args;

            foreach (string line in startArgs.Stream.EnumerateLines())
            {
                try
                {
                    OnTargetOutput(startArgs.IsStdErr, line + Environment.NewLine);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }
    }
}
