/* This file is part of redshift-tray.
   Copyright (c) Michael Scholz <development@mischolz.de>
*/
using redshift_tray.Properties;
using System;
using System.Windows;

namespace redshift_tray
{
  public partial class DebugConsole : Window
  {
    private bool isShown = false;

    public DebugConsole()
    {
      InitializeComponent();
      LoadPosition();
    }

    private void SavePosition()
    {
      Settings settings = Settings.Default;

      if(this.WindowState == WindowState.Maximized)
      {
        settings.DebugConsoleWindowState = this.WindowState;
      }
      else
      {
        settings.DebugConsoleLeft = this.Left;
        settings.DebugConsoleTop = this.Top;
        settings.DebugConsoleWidth = this.Width;
        settings.DebugConsoleHeight = this.Height;
      }
      settings.Save();
    }

    private void LoadPosition()
    {
      Settings settings = Settings.Default;

      if(Common.isOutOfBounds(settings.DebugConsoleLeft, settings.DebugConsoleTop))
      {
        return;
      }

      this.WindowStartupLocation = WindowStartupLocation.Manual;

      if(settings.DebugConsoleWindowState == WindowState.Maximized)
      {
        this.WindowState = settings.DebugConsoleWindowState;
        return;
      }

      this.Left = settings.DebugConsoleLeft;
      this.Top = settings.DebugConsoleTop;
      this.Width = settings.DebugConsoleWidth;
      this.Height = settings.DebugConsoleHeight;
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
      SavePosition();
      e.Cancel = true;
    }

    public void WriteLog(string message, LogType logType)
    {
      if(message.Length == 0)
      {
        return;
      }

      Output.Dispatcher.Invoke(() =>
      {
        string log = string.Format("{0} {1}: {2}", DateTime.Now.ToString("HH:mm:ss"), logType.ToString(), message);
        Output.Text += log + Environment.NewLine;
      });
    }

    private void Output_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
      Output.ScrollToEnd();
    }

    public enum LogType
    {
      Info,
      Error,
      Redshift
    }

  }
}
