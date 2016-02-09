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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace redshift_tray
{
  class Main
  {

    private static DebugConsole debugConsole;

    private Redshift RedshiftInstance;
    private TrayIcon TrayIconInstance;

    public static void WriteLogMessage(string message, DebugConsole.LogType logType)
    {
      debugConsole.WriteLog(message, logType);
    }

    public Main(bool showDebugLog)
    {
      debugConsole = new DebugConsole();
      if(showDebugLog)
      {
        debugConsole.ShowOrUnhide();
      }
    }

    public bool Initialize()
    {
      if(!CheckRedshiftVersion())
        return false;

      StartTrayIcon();
      StartRedshift(string.Empty);

      return true;
    }

    private bool CheckRedshiftVersion()
    {
      switch(Redshift.Check())
      {
        case Redshift.RedshiftError.NotFound:
          MessageBox.Show("Can not find a redshift.exe in the application startup path.");
          return false;
        case Redshift.RedshiftError.WrongApplication:
          MessageBox.Show("Your redshift.exe seems not to be a valid redshift binary.");
          return false;
        case Redshift.RedshiftError.WrongVersion:
          MessageBox.Show(string.Format("Your redshift.exe seems to be too old. Please use at least version {0}.{1}", Redshift.MIN_REDSHIFT_VERSION[0], Redshift.MIN_REDSHIFT_VERSION[1]));
          return false;
        case Redshift.RedshiftError.Ok:
          return true;
      }
      return false;
    }

    private void StartRedshift(params string[] Args)
    {
      RedshiftInstance = Redshift.Start(Args);
    }

    private void StartTrayIcon()
    {
      TrayIconInstance = TrayIcon.Create();
      TrayIconInstance.OnMenuItemExitClicked += TrayIconInstance_OnMenuItemExitClicked;
    }

    void TrayIconInstance_OnMenuItemExitClicked(object sender, RoutedEventArgs e)
    {
      if(RedshiftInstance.isRunning)
      {
        RedshiftInstance.Stop();
      }

      Application.Current.Shutdown(0);
    }

  }
}
