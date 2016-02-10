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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using redshift_tray.Properties;

namespace redshift_tray
{
  class Main
  {

    private static DebugConsole debugConsole;

    private Redshift RedshiftInstance;
    private TrayIcon TrayIconInstance;
    private string RedshiftPath;
    private string ConfigPath;

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
      LoadSettings();
      if(!CheckSettings())
        return false;

      StartTrayIcon();
      StartRedshiftContinuous();

      return true;
    }

    private void LoadSettings()
    {
      RedshiftPath = Settings.Default.RedshiftAppPath;
      ConfigPath = Settings.Default.RedshiftConfigPath;
    }

    private bool CheckSettings()
    {
      Redshift.ExecutableError error = Redshift.CheckExecutable(RedshiftPath);
      if(error != Redshift.ExecutableError.Ok)
      {
        SettingsWindow settingsWindow = new SettingsWindow(error);
        if((bool)settingsWindow.ShowDialog())
        {
          LoadSettings();
          return true;
        }
        return false;
      }
      return true;
    }

    private void StartRedshiftContinuous()
    {
      if(ConfigPath == string.Empty)
      {
        RedshiftInstance = Redshift.StartContinuous(RedshiftPath, string.Empty);
      }
      else
      {
        string argConfig = string.Format("-c \"{0}\"", ConfigPath);
        RedshiftInstance = Redshift.StartContinuous(RedshiftPath, argConfig);
      }
    }

    private void StartTrayIcon()
    {
      TrayIconInstance = TrayIcon.Create();
      TrayIconInstance.OnMenuItemExitClicked += TrayIconInstance_OnMenuItemExitClicked;
      TrayIconInstance.OnMenuItemSettingsClicked += TrayIconInstance_OnMenuItemSettingsClicked;
    }

    void TrayIconInstance_OnMenuItemSettingsClicked(object sender, RoutedEventArgs e)
    {
      SettingsWindow settingsWindow = new SettingsWindow();
      if((bool)settingsWindow.ShowDialog())
      {
        RedshiftInstance.Stop();
        LoadSettings();
        StartRedshiftContinuous();
      }
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
