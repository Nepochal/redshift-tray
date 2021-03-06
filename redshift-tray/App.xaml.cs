﻿/* This file is part of redshift-tray.
   Copyright (c) Michael Scholz <development@mischolz.de>
*/
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace redshift_tray
{
  public partial class App : Application
  {

    void Main(object sender, StartupEventArgs e)
    {
      if(Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Count() > 1)
      {
        Application.Current.Shutdown(0);
      }
      else
      {
        bool dummyMethod = e.Args.Any(arg => (arg.ToLower() == "/dummy"));
        Main main = new Main(dummyMethod);

        if(!main.Initialize())
        {
          Application.Current.Shutdown(-1);
        }
      }
    }

  }
}
