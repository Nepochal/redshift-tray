using redshift_tray.Properties;
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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace redshift_tray
{
  public class Redshift
  {
    public readonly static int[] MIN_REDSHIFT_VERSION = { 1, 10 };

    private static Redshift Instance;

    private Process RedshiftProcess;

    public delegate void RedshiftQuitHandler(object sender, RedshiftQuitArgs e);
    public event RedshiftQuitHandler OnRedshiftQuit;
    private void RedshiftQuit(bool manualKill)
    {
      if(OnRedshiftQuit != null)
      {
        RedshiftQuitArgs e = new RedshiftQuitArgs();
        e.ManualKill = manualKill;
        e.StandardOutput = GetStandardOutput();
        e.ErrorOutput = GetErrorOutput();

        OnRedshiftQuit(this, e);
      }
    }

    public bool isRunning
    {
      get { return !RedshiftProcess.HasExited; }
    }

    public static string[] GetArgsBySettings()
    {
      Settings settings = Settings.Default;
      List<string> returnValue = new List<string>();

      //Location
      returnValue.Add(string.Format("-l {0}:{1}", settings.RedshiftLatitude.ToString().Replace(',', '.'), settings.RedshiftLongitude.ToString().Replace(',', '.')));

      //Temperature
      returnValue.Add(string.Format("-t {0}:{1}", settings.RedshiftTemperatureDay, settings.RedshiftTemperatureNight));

      //Transition
      if(!settings.RedshiftTransition)
      {
        returnValue.Add("-r");
      }

      return returnValue.ToArray();
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
      {
        return false;
      }

      int majorversion = 0;
      int minorVersion = 0;
      int.TryParse(versionnr[0], out majorversion);
      int.TryParse(versionnr[1], out minorVersion);

      if(majorversion > MIN_REDSHIFT_VERSION[0])
      {
        return true;
      }

      return (majorversion == MIN_REDSHIFT_VERSION[0] && minorVersion >= MIN_REDSHIFT_VERSION[1]);
    }

    public static Redshift StartContinuous(string path, RedshiftQuitHandler onRedshiftQuit = null, params string[] Args)
    {
      InitializeContinuousStart(path, Args);
      if(onRedshiftQuit != null)
      {
        Instance.OnRedshiftQuit += onRedshiftQuit;
      }
      Instance.Start();
      return Instance;
    }

    private static Redshift InitializeContinuousStart(string path, params string[] Args)
    {
      if(CheckExecutable(path) != ExecutableError.Ok)
        throw new Exception("Invalid redshift start.");

      if(Instance != null && !Instance.RedshiftProcess.HasExited)
      {
        Instance.RedshiftProcess.Exited -= Instance.RedshiftProcess_Crashed;
        Instance.RedshiftProcess.Kill();
        Instance.RedshiftQuit(true);
      }

      Instance = new Redshift(path, Args);

      return Instance;
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
      RedshiftProcess.EnableRaisingEvents = true;
      RedshiftProcess.Exited += RedshiftProcess_Crashed;
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
        RedshiftProcess.Exited -= RedshiftProcess_Crashed;
        RedshiftProcess.Kill();
        RedshiftQuit(true);
      }
    }

    public string GetStandardOutput()
    {
      if(RedshiftProcess == null || isRunning)
      {
        return string.Empty;
      }

      string output = RedshiftProcess.StandardOutput.ReadToEnd();
      Main.WriteLogMessage(output, DebugConsole.LogType.Redshift);

      return output;
    }

    public string GetErrorOutput()
    {
      if(RedshiftProcess == null || isRunning)
      {
        return string.Empty;
      }

      string output = RedshiftProcess.StandardError.ReadToEnd();
      Main.WriteLogMessage(output, DebugConsole.LogType.Redshift);

      return output;
    }

    void RedshiftProcess_Crashed(object sender, EventArgs e)
    {
      RedshiftQuit(false);
    }

    public enum ExecutableError
    {
      Ok,
      NotFound,
      WrongVersion,
      WrongApplication
    }

  }

  public class RedshiftQuitArgs : EventArgs
  {
    public bool ManualKill { get; set; }
    public string StandardOutput { get; set; }
    public string ErrorOutput { get; set; }
  }
}
