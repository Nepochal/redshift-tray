﻿using Microsoft.Win32;
using redshift_tray.Properties;
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
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace redshift_tray
{
  /// <summary>
  /// Interaktionslogik für Settings.xaml
  /// </summary>
  public partial class SettingsWindow : Window
  {

    private Redshift.ExecutableError RedshiftInfoLabel
    {
      set
      {
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
        OkButton.IsEnabled = (value==Redshift.ExecutableError.Ok);
      }
    }

    public SettingsWindow()
    {
      InitializeComponent();
      LoadConfig();
      CheckConfig();
    }

    public SettingsWindow(Redshift.ExecutableError initialErrorNote)
    {
      InitializeComponent();
      LoadConfig();
      RedshiftInfoLabel = initialErrorNote;
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

    private void CheckConfig()
    {
      RedshiftInfoLabel = Redshift.CheckExecutable(redshiftPath.Text);
    }

    private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
    {
      System.Diagnostics.Process.Start("https://github.com/jonls/redshift/releases");
    }

    private void redshiftPath_LostFocus(object sender, RoutedEventArgs e)
    {
      RedshiftInfoLabel = Redshift.CheckExecutable(redshiftPath.Text);
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
        RedshiftInfoLabel = Redshift.CheckExecutable(redshiftPath.Text);
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