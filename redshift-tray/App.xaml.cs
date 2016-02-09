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
        Redshift.Start(string.Empty);
    }

  }
}
