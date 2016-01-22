using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace redshift_tray
{
  class Redshift
  {
    private readonly static string REDSHIFTPATH = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "redshift.exe");
    public readonly static int[] MIN_REDSHIFT_VERSION = { 1, 10 };

    private static Redshift Instance;

    private Process RedshiftProcess;

    public static RedshiftError Check()
    {
      App.WriteLogMessage("Checking redshift executable", DebugConsole.LogType.Info);

      if(!File.Exists(REDSHIFTPATH))
      {
        App.WriteLogMessage("Redshift executable not found", DebugConsole.LogType.Error);
        return RedshiftError.NotFound;
      }

      Create("-V");
      string[] version = Instance.getOutputLine().Split(' ');

      if(version.Length < 2 || version[0] != "redshift")
      {
        App.WriteLogMessage("Redshift executable is not a valid redshift binary", DebugConsole.LogType.Error);
        return RedshiftError.WrongApplication;
      }

      App.WriteLogMessage(string.Format("Checking redshift version >= {0}.{1}", MIN_REDSHIFT_VERSION[0], MIN_REDSHIFT_VERSION[1]), DebugConsole.LogType.Info);

      if(!CheckVersion(version[1]))
      {
        App.WriteLogMessage("Redshift version is too low", DebugConsole.LogType.Error);
        return RedshiftError.WrongVersion;
      }

      return RedshiftError.Ok;
    }

    private static bool CheckVersion(string version)
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

    private static void Create(params string[] Args)
    {
      if(Instance != null)
        throw new Exception("Only one instance is allowed!");

      Instance = new Redshift(Args);
    }

    private Redshift(params string[] Args)
    {
      string arglist = string.Join(" ", Args);

      App.WriteLogMessage(string.Format("Starting redshift with args '{0}'", arglist), DebugConsole.LogType.Info);

      RedshiftProcess = new Process();
      RedshiftProcess.StartInfo.FileName = REDSHIFTPATH;
      RedshiftProcess.StartInfo.Arguments = arglist;
      RedshiftProcess.StartInfo.UseShellExecute = false;
      RedshiftProcess.StartInfo.CreateNoWindow = true;
      RedshiftProcess.StartInfo.RedirectStandardOutput = true;
      RedshiftProcess.Start();
    }

    public string getOutputLine()
    {
      if(RedshiftProcess == null || RedshiftProcess.StandardOutput.EndOfStream)
        return string.Empty;

      string line = RedshiftProcess.StandardOutput.ReadLine();
      App.WriteLogMessage(line, DebugConsole.LogType.Redshift);

      return line;
    }

    public enum RedshiftError
    {
      Ok,
      NotFound,
      WrongVersion,
      WrongApplication
    }

  }
}
