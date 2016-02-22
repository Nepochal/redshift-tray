﻿/* This file is part of redshift-tray.
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
using Microsoft.Win32;
using redshift_tray.Properties;
using System.IO;
using System.Windows;
using System.Windows.Media;
using Xceed.Wpf.Toolkit;

namespace redshift_tray
{
  public partial class SettingsWindow : Window
  {

    private Redshift.ExecutableError _ExecutableErrorState;

    private Redshift.ExecutableError ExecutableErrorState
    {
      get { return _ExecutableErrorState; }
      set
      {
        _ExecutableErrorState = value;
        switch(value)
        {
          case Redshift.ExecutableError.NotFound:
            RedshiftInfo.Foreground = Brushes.Red;
            RedshiftInfo.Content = "Invalid path to Redshift executable.";
            break;
          case Redshift.ExecutableError.WrongApplication:
            RedshiftInfo.Foreground = Brushes.Red;
            RedshiftInfo.Content = "Executable seems not to be a valid Redshift binary.";
            break;
          case Redshift.ExecutableError.WrongVersion:
            RedshiftInfo.Foreground = Brushes.Red;
            RedshiftInfo.Content = string.Format("The Redshift version is be too old. Please use at least version {0}.{1}.", Redshift.MIN_REDSHIFT_VERSION[0], Redshift.MIN_REDSHIFT_VERSION[1]);
            break;
          case Redshift.ExecutableError.Ok:
            RedshiftInfo.Foreground = Brushes.Green;
            RedshiftInfo.Content = "Redshift executable is suitable.";
            break;
        }
        SetOkButtonEnabled();
      }
    }

    public SettingsWindow()
    {
      InitializeComponent();
      LoadConfig();
      ExecutableErrorState = Redshift.CheckExecutable(RedshiftPath.Text);
      SetOkButtonEnabled();
    }

    public SettingsWindow(Redshift.ExecutableError initialRedshiftErrorNote)
    {
      InitializeComponent();
      LoadConfig();
      ExecutableErrorState = initialRedshiftErrorNote;
    }

    private void SaveConfig()
    {
      Settings.Default.RedshiftAppPath = RedshiftPath.Text;
      Settings.Default.RedshiftLatitude = (decimal)Latitude.Value;
      Settings.Default.RedshiftLongitude = (decimal)Longitude.Value;
      Settings.Default.RedshiftTemperatureDay = (int)TemperatureDay.Value;
      Settings.Default.RedshiftTemperatureNight = (int)TemperatureNight.Value;
      Settings.Default.RedshiftTransition = (bool)Transition.IsChecked;

      Settings.Default.Save();
    }

    private void LoadConfig()
    {
      RedshiftPath.Text = Settings.Default.RedshiftAppPath;
      Latitude.Value = Settings.Default.RedshiftLatitude;
      Longitude.Value = Settings.Default.RedshiftLongitude;
      TemperatureDay.Value = Settings.Default.RedshiftTemperatureDay;
      TemperatureNight.Value = Settings.Default.RedshiftTemperatureNight;
      Transition.IsChecked = Settings.Default.RedshiftTransition;
    }

    private bool CheckConfig()
    {
      return (_ExecutableErrorState == Redshift.ExecutableError.Ok);
    }

    private void SetOkButtonEnabled()
    {
      OkButton.IsEnabled = CheckConfig();
    }

    private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
    {
      System.Diagnostics.Process.Start("https://github.com/jonls/redshift/releases");
    }

    private void redshiftPath_LostFocus(object sender, RoutedEventArgs e)
    {
      ExecutableErrorState = Redshift.CheckExecutable(RedshiftPath.Text);
    }

    private void ButtonRedshift_Click(object sender, RoutedEventArgs e)
    {
      OpenFileDialog openFileDialog = new OpenFileDialog();
      openFileDialog.Title = "Redshift path";
      openFileDialog.Filter = "Redshift|redshift.exe|All executables|*.exe";
      openFileDialog.CheckFileExists = true;

      if(File.Exists(RedshiftPath.Text))
      {
        openFileDialog.InitialDirectory = Path.GetDirectoryName(RedshiftPath.Text);
      }

      if((bool)openFileDialog.ShowDialog())
      {
        RedshiftPath.Text = openFileDialog.FileName;
        ExecutableErrorState = Redshift.CheckExecutable(RedshiftPath.Text);
      }
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
      SaveConfig();
      DialogResult = true;
      Close();
    }

  }
}
