using Microsoft.Win32;
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

    private Redshift.RedshiftError RedshiftInfoLabel
    {
      set
      {
        switch(value)
        {
          case Redshift.RedshiftError.NotFound:
            redshiftInfo.Foreground = Brushes.Red;
            redshiftInfo.Content = "Invalid path to Redshift executable.";
            break;
          case Redshift.RedshiftError.WrongApplication:
            redshiftInfo.Foreground = Brushes.Red;
            redshiftInfo.Content = "Executable seems not to be a valid Redshift binary.";
            break;
          case Redshift.RedshiftError.WrongVersion:
            redshiftInfo.Foreground = Brushes.Red;
            redshiftInfo.Content = string.Format("The Redshift version is be too old. Please use at least version {0}.{1}.", Redshift.MIN_REDSHIFT_VERSION[0], Redshift.MIN_REDSHIFT_VERSION[1]);
            break;
          case Redshift.RedshiftError.Ok:
            redshiftInfo.Foreground = Brushes.Green;
            redshiftInfo.Content = "Redshift executable is suitable.";
            break;
        }
      }
    }

    public SettingsWindow(Redshift.RedshiftError initialErrorNote)
    {
      InitializeComponent();
      RedshiftInfoLabel = initialErrorNote;
    }

    private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
    {
      System.Diagnostics.Process.Start("https://github.com/jonls/redshift/releases");
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

  }
}
