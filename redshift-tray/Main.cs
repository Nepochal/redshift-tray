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

    public const string VERSION = "0.4.1a";

    private static DebugConsole debugConsole;

    private Status _ProgramStatus;
    private Redshift RedshiftInstance;
    private TrayIcon TrayIconInstance;
    private string RedshiftPath;
    private Settings Settings;

    private Status ProgramStatus
    {
      get { return _ProgramStatus; }
      set
      {
        switch(value)
        {
          case Status.Automatic:
            WriteLogMessage("Switching to automatic mode.", DebugConsole.LogType.Info);
            StartRedshiftAutomatic();
            break;
          case Status.Off:
            WriteLogMessage("Switching to off mode.", DebugConsole.LogType.Info);
            StopRedshift();
            break;
        }
        if(TrayIconInstance != null)
        {
          TrayIconInstance.TrayStatus = value;
        }
        _ProgramStatus = value;
      }
    }

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
      {
        return false;
      }

      ProgramStatus = Status.Automatic;
      StartTrayIcon();

      return true;
    }

    private void LoadSettings()
    {
      RedshiftPath = Settings.Default.RedshiftAppPath;
      Settings = Settings.Default;
    }

    private bool CheckSettings()
    {
      Redshift.ExecutableError exeError = Redshift.CheckExecutable(RedshiftPath);

      if(exeError != Redshift.ExecutableError.Ok)
      {
        SettingsWindow settingsWindow;
        if(!Common.WindowExistsFocus(out settingsWindow))
        {
          settingsWindow = new SettingsWindow();
          if((bool)settingsWindow.ShowDialog())
          {
            LoadSettings();
            return true;
          }
          return false;
        }
      }
      return true;
    }

    private void StartRedshiftAutomatic()
    {
      string[] args = Redshift.GetArgsBySettings();
      RedshiftInstance = Redshift.StartContinuous(RedshiftPath, RedshiftInstance_OnRedshiftQuit, args);
    }

    private bool StopRedshift()
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
      string[] args = { "-m wingdi", "-x" };
      Redshift.StartAndWaitForOutput(RedshiftPath, args);
    }

    private void StartTrayIcon()
    {
      TrayIconInstance = TrayIcon.Create(ProgramStatus);

      TrayIconInstance.OnTrayIconLeftClick += (sender, e) =>
      {
        switch(ProgramStatus)
        {
          case Status.Automatic:
            ProgramStatus = Status.Off;
            break;
          case Status.Off:
            ProgramStatus = Status.Automatic;
            break;
        }
      };

      TrayIconInstance.OnMenuItemExitClicked += (sender, e) =>
        {
          StopRedshift();
          Application.Current.Shutdown(0);
        };

      TrayIconInstance.OnMenuItemLogClicked += (sender, e) =>
      {
        debugConsole.ShowOrUnhide();
      };

      TrayIconInstance.OnMenuItemSettingsClicked += (sender, e) =>
        {
          SettingsWindow settingsWindow;
          if(!Common.WindowExistsFocus(out settingsWindow))
          {
            settingsWindow = new SettingsWindow();
            if((bool)settingsWindow.ShowDialog())
            {
              LoadSettings();
              if(ProgramStatus == Status.Automatic)
              {
                StartRedshiftAutomatic();
              }
            }
          }
        };
    }

    private void RedshiftInstance_OnRedshiftQuit(object sender, RedshiftQuitArgs e)
    {
      if(!e.ManualKill)
      {
        Application.Current.Dispatcher.Invoke(() =>
        {
          MessageBox.Show(string.Format("Redshift crashed with the following output:{0}{0}{1}", Environment.NewLine, e.ErrorOutput), "Redshift Tray", MessageBoxButton.OK, MessageBoxImage.Error);

          SettingsWindow settingsWindow;
          if(!Common.WindowExistsFocus(out settingsWindow))
          {
            settingsWindow = new SettingsWindow();
            if((bool)settingsWindow.ShowDialog())
            {
              LoadSettings();
              StartRedshiftAutomatic();
            }
            else
            {
              Application.Current.Shutdown(-1);
            }
          }
        });
      }
    }
  }
}
