using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace redshift_tray
{
  /// <summary>
  /// Interaktionslogik für "App.xaml"
  /// </summary>
  public partial class App : Application
  {

    private static DebugConsole debugConsole;

    public static void WriteLogMessage(string message, DebugConsole.LogType logType)
    {
      debugConsole.WriteLog(message, logType);
    }

    void Main(object sender, StartupEventArgs e)
    {
      debugConsole = new DebugConsole();

      if(e.Args.Contains("/debug"))
      {
        debugConsole.ShowOrUnhide();
      }

      bool error = false;
      switch(Redshift.Check())
      {
        case Redshift.RedshiftError.NotFound:
          MessageBox.Show("Can not find a redshift.exe in the application startup path.");
          error = true;
          break;
        case Redshift.RedshiftError.WrongApplication:
          MessageBox.Show("Your redshift.exe seems not to be a valid redshift binary.");
          error = true;
          break;
        case Redshift.RedshiftError.WrongVersion:
          MessageBox.Show(string.Format("Your redshift.exe seems to be too old. Please use at least version {0}.{1}", Redshift.MIN_REDSHIFT_VERSION[0], Redshift.MIN_REDSHIFT_VERSION[1]));
          error = true;
          break;
      }

      if(error)
        Application.Current.Shutdown(-1);
      else
        Redshift.Start("-v");
    }

  }
}
