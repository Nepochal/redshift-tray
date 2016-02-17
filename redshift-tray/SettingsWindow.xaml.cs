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
using Microsoft.Win32;
using redshift_tray.Properties;
using System.IO;
using System.Windows;
using System.Windows.Media;

namespace redshift_tray
{
  public partial class SettingsWindow : Window
  {

    private Redshift.ExecutableError _ExecutableErrorState;
    private Redshift.ConfigError _ConfigErrorState;

    private Redshift.ExecutableError ExecutableErrorState
    {
      get { return _ExecutableErrorState; }
      set
      {
        _ExecutableErrorState = value;
        switch(value)
        {
          case Redshift.ExecutableError.NotFound:
            redshiftInfo.Foreground = Brushes.Red;
            redshiftInfo.Content = "Invalid path to Redshift executable.";
            break;
          case Redshift.ExecutableError.WrongApplication:
            redshiftInfo.Foreground = Brushes.Red;
            redshiftInfo.Content = "Executable seems not to be a valid Redshift binary.";
            break;
          case Redshift.ExecutableError.WrongVersion:
            redshiftInfo.Foreground = Brushes.Red;
            redshiftInfo.Content = string.Format("The Redshift version is be too old. Please use at least version {0}.{1}.", Redshift.MIN_REDSHIFT_VERSION[0], Redshift.MIN_REDSHIFT_VERSION[1]);
            break;
          case Redshift.ExecutableError.Ok:
            redshiftInfo.Foreground = Brushes.Green;
            redshiftInfo.Content = "Redshift executable is suitable.";
            break;
        }
        SetOkButtonEnabled();
      }
    }

    private Redshift.ConfigError ConfigErrorState
    {
      get { return _ConfigErrorState; }
      set
      {
        _ConfigErrorState = value;
        switch(value)
        {
          case Redshift.ConfigError.NotFound:
            configInfo.Foreground = Brushes.Red;
            configInfo.Content = "Invalid path to config.";
            break;
          case Redshift.ConfigError.MissingMandatoryField:
            configInfo.Foreground = Brushes.Red;
            configInfo.Content = "Missing information about your location.";
            break;
          case Redshift.ConfigError.Ok:
            configInfo.Foreground = Brushes.Green;
            configInfo.Content = "Redshift config is suitable.";
            break;
        }
        SetOkButtonEnabled();
      }
    }

    public SettingsWindow()
    {
      InitializeComponent();
      LoadConfig();
      ExecutableErrorState = Redshift.CheckExecutable(redshiftPath.Text);
      ConfigErrorState = Redshift.CheckConfig(configPath.Text);
      SetOkButtonEnabled();
    }

    public SettingsWindow(Redshift.ExecutableError initialRedshiftErrorNote, Redshift.ConfigError initialConfigErrorNote)
    {
      InitializeComponent();
      LoadConfig();
      ExecutableErrorState = initialRedshiftErrorNote;
      ConfigErrorState = initialConfigErrorNote;
    }

    private void SaveConfig()
    {
      Settings.Default.RedshiftAppPath = redshiftPath.Text;
      Settings.Default.RedshiftConfigPath = configPath.Text;
      Settings.Default.Save();
    }

    private void LoadConfig()
    {
      redshiftPath.Text = Settings.Default.RedshiftAppPath;
      configPath.Text = Settings.Default.RedshiftConfigPath;
    }

    private bool CheckConfig()
    {
      return (_ExecutableErrorState == Redshift.ExecutableError.Ok && _ConfigErrorState == Redshift.ConfigError.Ok);
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
      ExecutableErrorState = Redshift.CheckExecutable(redshiftPath.Text);
    }

    private void ButtonRedshift_Click(object sender, RoutedEventArgs e)
    {
      OpenFileDialog openFileDialog = new OpenFileDialog();
      openFileDialog.Title = "Redshift path";
      openFileDialog.Filter = "Redshift|redshift.exe|All executables|*.exe";
      openFileDialog.CheckFileExists = true;

      if(File.Exists(redshiftPath.Text))
      {
        openFileDialog.InitialDirectory = Path.GetDirectoryName(redshiftPath.Text);
      }

      if((bool)openFileDialog.ShowDialog())
      {
        redshiftPath.Text = openFileDialog.FileName;
        ExecutableErrorState = Redshift.CheckExecutable(redshiftPath.Text);
      }
    }

    private void ButtonConfig_Click(object sender, RoutedEventArgs e)
    {
      OpenFileDialog openFileDialog = new OpenFileDialog();
      openFileDialog.Title = "Config path";
      openFileDialog.Filter = "redshift.conf|redshift.conf|All files|*.*";
      openFileDialog.CheckFileExists = true;

      if(File.Exists(configPath.Text))
      {
        openFileDialog.InitialDirectory = Path.GetDirectoryName(configPath.Text);
      }

      if((bool)openFileDialog.ShowDialog())
      {
        configPath.Text = openFileDialog.FileName;
        ConfigErrorState = Redshift.CheckConfig(configPath.Text);
      }
    }

    private void configPath_LostFocus(object sender, RoutedEventArgs e)
    {
      ConfigErrorState = Redshift.CheckConfig(configPath.Text);
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
      SaveConfig();
      DialogResult = true;
      Close();
    }

  }
}
