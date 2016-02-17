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
using System.Windows;
using redshift_tray.Properties;
using System;

namespace redshift_tray
{
  class Main
  {

    public const string VERSION = "0.2.1a";

    private static DebugConsole debugConsole;

    private Redshift RedshiftInstance;
    private TrayIcon TrayIconInstance;
    private string RedshiftPath;
    private string ConfigPath;

    public static void WriteLogMessage(string message, DebugConsole.LogType logType)
    {
      debugConsole.WriteLog(message, logType);
    }

    public Main()
    {
      debugConsole = new DebugConsole();
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
      Redshift.ExecutableError exeError = Redshift.CheckExecutable(RedshiftPath);
      Redshift.ConfigError confError = Redshift.CheckConfig(ConfigPath);

      if(exeError != Redshift.ExecutableError.Ok || confError != Redshift.ConfigError.Ok)
      {
        SettingsWindow settingsWindow = new SettingsWindow(exeError, confError);
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
      Redshift.CheckConfig(ConfigPath);

      string argConfig = string.Format("-c \"{0}\"", ConfigPath);
      RedshiftInstance = Redshift.StartContinuous(RedshiftInstance_OnRedshiftQuit, RedshiftPath, argConfig);
    }

    private bool StopRedshiftContinuous()
    {
      if(RedshiftInstance.isRunning)
      {
        RedshiftInstance.Stop();
        ResetScreen();
        return true;
      }
      return false;
    }

    private void ResetScreen()
    {
      Redshift.StartAndWaitForOutput(RedshiftPath, "-x");
    }

    private void StartTrayIcon()
    {
      TrayIconInstance = TrayIcon.Create(TrayIcon.TrayIconStatus.Automatic);

      TrayIconInstance.OnTrayIconLeftClick += (sender, e) =>
      {
        switch(TrayIconInstance.Status)
        {
          case TrayIcon.TrayIconStatus.Automatic:
            StopRedshiftContinuous();
            TrayIconInstance.Status = TrayIcon.TrayIconStatus.Off;
            break;
          case TrayIcon.TrayIconStatus.Off:
            StartRedshiftContinuous();
            TrayIconInstance.Status = TrayIcon.TrayIconStatus.Automatic;
            break;
        }
      };

      TrayIconInstance.OnMenuItemExitClicked += (sender, e) =>
        {
          StopRedshiftContinuous();
          Application.Current.Shutdown(0);
        };

      TrayIconInstance.OnMenuItemLogClicked += (sender, e) =>
      {
        debugConsole.ShowOrUnhide();
      };

      TrayIconInstance.OnMenuItemSettingsClicked += (sender, e) =>
        {
          SettingsWindow settingsWindow = new SettingsWindow();
          if((bool)settingsWindow.ShowDialog())
          {
            StopRedshiftContinuous();
            LoadSettings();
            StartRedshiftContinuous();
          }
        };
    }

    void RedshiftInstance_OnRedshiftQuit(object sender, RedshiftQuitArgs e)
    {
      if(!e.ManualKill)
      {
        Application.Current.Dispatcher.Invoke(() =>
        {
          MessageBox.Show(string.Format("Redshift crashed with the following output:{0}{0}{1}", Environment.NewLine, e.ErrorOutput), "Redshift Tray", MessageBoxButton.OK, MessageBoxImage.Error);

          SettingsWindow settingsWindow = new SettingsWindow();
          if((bool)settingsWindow.ShowDialog())
          {
            LoadSettings();
            StartRedshiftContinuous();
          }
          else
          {
            Application.Current.Shutdown(-1);
          }
        });
      }
    }
  }
}
