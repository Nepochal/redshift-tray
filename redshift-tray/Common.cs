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

namespace redshift_tray
{

  static class Common
  {

    public static bool Autostart
    {
      get
      {
        RegistryKey regPath = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", false);
        return regPath.GetValue("Redshift Tray") != null;
      }
      set
      {
        if(Autostart == value)
        {
          return;
        }

        RegistryKey regPath = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
        switch(value)
        {
          case true:
            regPath.SetValue("Redshift Tray", Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, System.AppDomain.CurrentDomain.FriendlyName));
            break;
          case false:
            regPath.DeleteValue("Redshift Tray");
            break;
        }
      }
    }

    public static bool isOutOfBounds(double x, double y)
    {
      if(x <= SystemParameters.VirtualScreenLeft) return true;
      if(y <= SystemParameters.VirtualScreenTop) return true;
      if(x >= SystemParameters.VirtualScreenLeft + SystemParameters.VirtualScreenWidth) return true;
      if(y >= SystemParameters.VirtualScreenTop + SystemParameters.VirtualScreenHeight) return true;
      return false;
    }

    public static bool WindowExistsFocus<T>(out T windowInstance) where T : Window
    {
      bool returnValue;
      if(returnValue = WindowExists<T>(out windowInstance))
      {
        windowInstance.Focus();
      }
      return returnValue;
    }

    public static bool WindowExists<T>(out T windowInstance) where T : Window
    {
      windowInstance = Application.Current.Windows.OfType<T>().FirstOrDefault();
      return (windowInstance != null);
    }

    public static bool WindowExists<T>() where T : Window
    {
      return Application.Current.Windows.OfType<T>().Any();
    }

  }

  public enum Status
  {
    Automatic,
    Off
  }

}
