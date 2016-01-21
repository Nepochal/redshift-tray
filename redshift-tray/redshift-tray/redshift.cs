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

    private Process _RedshiftProcess;

    public static RedshiftError Check()
    {
      if(!File.Exists(REDSHIFTPATH))
        return RedshiftError.NotFound;

      Create("-V");
      string[] version = Instance.getOutputLine().Split(' ');

      if(version.Length < 2 || version[0] != "redshift")
        return RedshiftError.WrongApplication;

      if(!CheckVersion(version[1]))
        return RedshiftError.WrongVersion;

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

      return (majorversion >= MIN_REDSHIFT_VERSION[0] && minorVersion >= MIN_REDSHIFT_VERSION[1]);
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

      _RedshiftProcess = new Process();
      _RedshiftProcess.StartInfo.FileName = REDSHIFTPATH;
      _RedshiftProcess.StartInfo.Arguments = arglist;
      _RedshiftProcess.StartInfo.UseShellExecute = false;
      _RedshiftProcess.StartInfo.CreateNoWindow = true;
      _RedshiftProcess.StartInfo.RedirectStandardOutput = true;
      _RedshiftProcess.Start();
    }

    public string getOutputLine()
    {
      if(_RedshiftProcess.StandardOutput.EndOfStream)
        return string.Empty;

      return _RedshiftProcess.StandardOutput.ReadLine();
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
