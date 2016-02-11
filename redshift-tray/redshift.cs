/* This file is part of redshift-tray.
   Redshift-tray is free software: you can redistribute it and/or modify
   it under the terms of the GNU General Public License as published by
   the Free Software Foundation, either version 3 of the License, or
   (at your option) any later version.
   Redshift-tray is distributed in the hope that it will be useful,
   but WITHOUT ANY WARRANTY; without even the implied warranty of
   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
   GNU General Public License for more details.
   You should have received a copy of the GNU General Public License
   along with redshift-tray.  If not, see <http://www.gnu.org/licenses/>.
   Copyright (c) Michael Scholz <development@mischolz.de>
*/
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace redshift_tray
{
  public class Redshift
  {
    public readonly static int[] MIN_REDSHIFT_VERSION = { 1, 10 };

    private Process RedshiftProcess;

    private static Redshift Instance;

    public delegate void RedshiftQuitHandler(object sender, RedshiftQuitArgs e);
    public event RedshiftQuitHandler OnRedshiftQuit;
    private void RedshiftQuit(bool manualKill)
    {
      if(OnRedshiftQuit != null)
      {
        RedshiftQuitArgs e = new RedshiftQuitArgs();
        e.ManualKill = manualKill;
        e.ExitCode = RedshiftProcess.ExitCode;
        e.StandardOutput = GetStandardOutput();
        e.ErrorOutput = GetErrorOutput();

        OnRedshiftQuit(this, e);
      }
    }

    public bool isRunning
    {
      get
      {
        return !RedshiftProcess.HasExited;
      }
    }

    public static ConfigError CheckConfig(string path)
    {
      Main.WriteLogMessage("Checking redshift config.", DebugConsole.LogType.Info);

      if(!File.Exists(path))
      {
        Main.WriteLogMessage("Config not found.", DebugConsole.LogType.Error);
        return ConfigError.NotFound;
      }

      bool hasMode = false;
      bool hasLat = false;
      bool hasLon = false;
      //superficial check if all mandatory information are given
      foreach(string line in File.ReadAllLines(path))
      {
        if(line.Length >= 8 && line.Substring(0, 8) == "[manual]")
          hasMode = true;
        if(line.Length >= 4 && line.Substring(0, 4) == "lat=")
          hasLat = true;
        if(line.Length >= 4 && line.Substring(0, 4) == "lon=")
          hasLon = true;
      }
      if(!hasMode || !hasLat || !hasLon)
      {
        Main.WriteLogMessage("Missing mandatory information in config.", DebugConsole.LogType.Error);
        return ConfigError.MissingMandatoryField;
      }

      return ConfigError.Ok;
    }

    public static ExecutableError CheckExecutable(string path)
    {
      Main.WriteLogMessage("Checking redshift executable", DebugConsole.LogType.Info);

      if(!File.Exists(path))
      {
        Main.WriteLogMessage("Redshift executable not found", DebugConsole.LogType.Error);
        return ExecutableError.NotFound;
      }

      string[] version = StartAndWaitForOutput(path, "-V").Split(' ');

      if(version.Length < 2 || version[0] != "redshift")
      {
        Main.WriteLogMessage("Redshift executable is not a valid redshift binary", DebugConsole.LogType.Error);
        return ExecutableError.WrongApplication;
      }

      Main.WriteLogMessage(string.Format("Checking redshift version >= {0}.{1}", MIN_REDSHIFT_VERSION[0], MIN_REDSHIFT_VERSION[1]), DebugConsole.LogType.Info);

      if(!CheckExecutableVersion(version[1]))
      {
        Main.WriteLogMessage("Redshift version is too low", DebugConsole.LogType.Error);
        return ExecutableError.WrongVersion;
      }

      return ExecutableError.Ok;
    }

    private static bool CheckExecutableVersion(string version)
    {
      string[] versionnr = version.Split('.');
      if(versionnr.Length < 2)
        return false;

      int majorversion = 0;
      int minorVersion = 0;
      int.TryParse(versionnr[0], out majorversion);
      int.TryParse(versionnr[1], out minorVersion);

      if(majorversion > MIN_REDSHIFT_VERSION[0])
        return true;

      return (majorversion == MIN_REDSHIFT_VERSION[0] && minorVersion >= MIN_REDSHIFT_VERSION[1]);
    }

    public static Redshift StartContinuous(RedshiftQuitHandler onRedshiftQuit, string path, params string[] Args)
    {
      InitializeContinuousStart(path, Args);
      Instance.Start();
      Instance.OnRedshiftQuit += onRedshiftQuit;
      Instance.StartContinuousCheckerThread();
      return Instance;
    }

    public static Redshift StartContinuous(string path, params string[] Args)
    {
      InitializeContinuousStart(path, Args);
      Instance.Start();
      Instance.StartContinuousCheckerThread();
      return Instance;
    }

    private static Redshift InitializeContinuousStart(string path, params string[] Args)
    {
      if(CheckExecutable(path) != ExecutableError.Ok)
        throw new Exception("Invalid redshift start.");

      if(Instance != null && !Instance.RedshiftProcess.HasExited)
      {
        Instance.RedshiftProcess.Kill();
        Instance.RedshiftQuit(true);
      }

      Instance = new Redshift(path, Args);

      return Instance;
    }

    //Start a thread that checks after 5 seconds, if the redshift instance has quit/aborted
    private void StartContinuousCheckerThread()
    {
      Thread checkerThread = new Thread(() =>
      {
        Thread.Sleep(5000);
        if(!Instance.isRunning)
        {
          Instance.RedshiftQuit(false);
        }
      });
      checkerThread.IsBackground = true;
      checkerThread.Start();
    }

    public static string StartAndWaitForOutput(string path, params string[] Args)
    {
      Redshift redshift = new Redshift(path, Args);
      redshift.Start();
      redshift.RedshiftProcess.WaitForExit();

      return redshift.GetStandardOutput();
    }

    private Redshift(string path, params string[] Args)
    {
      string arglist = string.Join(" ", Args);

      Main.WriteLogMessage(string.Format("Starting redshift with args '{0}'", arglist), DebugConsole.LogType.Info);

      RedshiftProcess = new Process();
      RedshiftProcess.StartInfo.FileName = path;
      RedshiftProcess.StartInfo.Arguments = arglist;
      RedshiftProcess.StartInfo.UseShellExecute = false;
      RedshiftProcess.StartInfo.CreateNoWindow = true;
      RedshiftProcess.StartInfo.RedirectStandardOutput = true;
      RedshiftProcess.StartInfo.RedirectStandardError = true;
    }

    private void Start()
    {
      RedshiftProcess.Start();
    }

    public void Stop()
    {
      if(isRunning)
      {
        Main.WriteLogMessage("Stopped redshift instance.", DebugConsole.LogType.Info);
        RedshiftProcess.Kill();
        RedshiftQuit(true);
      }
    }

    public string GetStandardOutput()
    {
      if(RedshiftProcess == null || isRunning)
        return string.Empty;

      string output = RedshiftProcess.StandardOutput.ReadToEnd();
      Main.WriteLogMessage(output, DebugConsole.LogType.Redshift);

      return output;
    }

    public string GetErrorOutput()
    {
      if(RedshiftProcess == null || isRunning)
        return string.Empty;

      string output = RedshiftProcess.StandardError.ReadToEnd();
      Main.WriteLogMessage(output, DebugConsole.LogType.Redshift);

      return output;
    }

    public enum ExecutableError
    {
      Ok,
      NotFound,
      WrongVersion,
      WrongApplication
    }

    public enum ConfigError
    {
      Ok,
      NotFound,
      MissingMandatoryField
    }

  }

  public class RedshiftQuitArgs : EventArgs
  {
    public bool ManualKill { get; set; }
    public int ExitCode { get; set; }
    public string StandardOutput { get; set; }
    public string ErrorOutput { get; set; }
  }
}
