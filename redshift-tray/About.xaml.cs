/* This file is part of redshift-tray.
   Copyright (c) Michael Scholz <development@mischolz.de>
*/
using redshift_tray.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
  public partial class About : Window
  {

    private string VersionText
    {
      set { Version.Text = string.Format("Version: {0}", value); }
    }

    public About()
    {
      InitializeComponent();
      LoadPosition();
      VersionText = Main.VERSION;
    }

    private void SavePosition()
    {
      Settings settings = Settings.Default;

      settings.AboutLeft = this.Left;
      settings.AboutTop = this.Top;

      settings.Save();
    }

    private void LoadPosition()
    {
      Settings settings = Settings.Default;

      if(Common.isOutOfBounds(settings.AboutLeft, settings.AboutTop))
      {
        return;
      }

      this.WindowStartupLocation = WindowStartupLocation.Manual;
      this.Left = settings.AboutLeft;
      this.Top = settings.AboutTop;
    }

    private void RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
    {
      Process.Start(e.Uri.ToString());
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
      SavePosition();
    }

  }
}
