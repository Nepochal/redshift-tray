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
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace redshift_tray
{
  /// <summary>
  /// Interaktionslogik für DebugConsole.xaml
  /// </summary>
  public partial class DebugConsole : Window
  {
    private bool isShown = false;

    public DebugConsole()
    {
      InitializeComponent();
    }

    public void ShowOrUnhide()
    {
      if(isShown)
      {
        Visibility = System.Windows.Visibility.Visible;
      }
      else
      {
        Show();
        isShown = true;
      }
    }

    public new void Hide()
    {
      Visibility = System.Windows.Visibility.Hidden;
    }

    private void ButtonClipboard_Click(object sender, RoutedEventArgs e)
    {
      Clipboard.SetText(Output.Text);
    }

    private void ButtonClose_Click(object sender, RoutedEventArgs e)
    {
      Close();
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
      Hide();
      e.Cancel = true;
    }

    public void WriteLog(string message, LogType logType)
    {
      Output.Dispatcher.Invoke(() =>
      {
        string log = string.Format("{0} {1}: {2}", DateTime.Now.ToString("HH:mm:ss"), logType.ToString(), message);
        Output.Text += log + Environment.NewLine;
      });
    }

    public enum LogType
    {
      Info,
      Error,
      Redshift
    }

  }
}
