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

namespace redshift_tray
{
  public class Redshift
  {
    public readonly static int[] MIN_REDSHIFT_VERSION = { 1, 10 };

    private Process RedshiftProcess;

    private static Redshift Instance;

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

      if(path == string.Empty)
      {
        Main.WriteLogMessage("Skipped. No config set. Using defaults.", DebugConsole.LogType.Info);
        return ConfigError.NotSet;
      }

      if(!File.Exists(path))
      {
        Main.WriteLogMessage("Config not found. Using defaults.", DebugConsole.LogType.Error);
        return ConfigError.NotFound;
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

    public static Redshift StartContinuous(string path, params string[] Args)
    {
      if(CheckExecutable(path) != ExecutableError.Ok)
        throw new Exception("Invalid redshift start.");

      if(Instance != null && !Instance.RedshiftProcess.HasExited)
      {
        Instance.RedshiftProcess.Kill();
      }

      Instance = new Redshift(path, Args);
      return Instance;
    }

    public static string StartAndWaitForOutput(string path, params string[] Args)
    {
      Redshift redshift = new Redshift(path, Args);
      redshift.RedshiftProcess.WaitForExit();

      return redshift.GetOutput();
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
      RedshiftProcess.Start();
    }

    public void Stop()
    {
      if(isRunning)
      {
        Main.WriteLogMessage("Stopped redshift instance.", DebugConsole.LogType.Info);
        RedshiftProcess.Kill();
      }
    }

    public string GetOutput()
    {
      if(RedshiftProcess == null || isRunning)
        return string.Empty;

      string output = RedshiftProcess.StandardOutput.ReadToEnd();
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
      NotSet,
      NotFound
    }

  }
}
