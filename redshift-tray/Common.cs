﻿/* This file is part of redshift-tray.
   Copyright (c) Michael Scholz <development@mischolz.de>
*/
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
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
        using (RegistryKey regPath = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", false))
        {
          return regPath.GetValue("Redshift Tray") != null;
        }
      }
      set
      {
        if(Autostart == value)
        {
          return;
        }

        using (RegistryKey regPath = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true)) { 
          switch(value)
          {
            case true:
              regPath.SetValue("Redshift Tray",
                Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, System.AppDomain.CurrentDomain.FriendlyName));
              break;
            case false:
              regPath.DeleteValue("Redshift Tray");
              break;
          }
        }
      }
    }

    public static AutoLocation DetectLocation()
    {
      Main.WriteLogMessage("Detecting location via API.", DebugConsole.LogType.Info);

      AutoLocation returnValue = new AutoLocation();

      try
      {
        Main.WriteLogMessage(string.Format("Pinging {0}", Main.GEO_API_DOMAIN), DebugConsole.LogType.Info);
        using(Ping ping = new Ping()) { 
          PingReply pingReply = ping.Send(Main.GEO_API_DOMAIN, 5000);
          if(pingReply.Status != IPStatus.Success)
          {
            Main.WriteLogMessage("API is not reachable.", DebugConsole.LogType.Error);
            returnValue.Success = false;
            returnValue.Errortext = string.Format("Location provider is not reachable.{0}Please make sure that your internet connection works properly and try again in a few minutes.", Environment.NewLine);
            return returnValue;
          }
          if(IPAddress.IsLoopback(pingReply.Address) || pingReply.Address.Equals(IPAddress.Any))
          {
            Main.WriteLogMessage("API is routed to localhost.", DebugConsole.LogType.Error);
            returnValue.Success = false;
            returnValue.Errortext = string.Format("The location provider is blocked by your proxy or hosts-file.{0}Please insert your location manually or allow connections to {1}.", Environment.NewLine, Main.GEO_API_DOMAIN);
            return returnValue;
          }
        }
      }
      catch(PingException)
      {
        Main.WriteLogMessage("API is not reachable.", DebugConsole.LogType.Error);
        returnValue.Success = false;
        returnValue.Errortext = string.Format("Location provider is not reachable.{0}Please make sure that your internet connection works properly and try again in a few minutes.", Environment.NewLine);
        return returnValue;
      }

      
      string latitude;
      string longitude;

      try
      {
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Main.GEO_API_TARGET);
        request.Proxy = null;

        using (HttpWebResponse response = (HttpWebResponse) request.GetResponse())
        {
          if(response.StatusCode != HttpStatusCode.OK)
          {
            Main.WriteLogMessage("A server side error occured.", DebugConsole.LogType.Error);
            returnValue.Success = false;
            returnValue.Errortext = string.Format("An error on the server side of the location provider occured.{0}Please try again later.", Environment.NewLine);
            return returnValue;
          }

          using (StreamReader reader = new StreamReader(response.GetResponseStream())) { 
            if (reader.EndOfStream || reader.ReadLine() != "success")
            {
              Main.WriteLogMessage("A server side error occured.", DebugConsole.LogType.Error);
              returnValue.Success = false;
              returnValue.Errortext = string.Format("An error on the server side of the location provider occured.{0}Please try again later.", Environment.NewLine);
              return returnValue;
            }

            latitude = reader.ReadLine();
            longitude = reader.ReadLine();
          }
        }
      }
      catch(WebException)
      {
        Main.WriteLogMessage("A server side error occured.", DebugConsole.LogType.Error);
        returnValue.Success = false;
        returnValue.Errortext = string.Format("An error on the server side of the location provider occured.{0}Please try again later.", Environment.NewLine);
        return returnValue;
      }

      Main.WriteLogMessage("Location detected", DebugConsole.LogType.Info);
      returnValue = ParseLocation(latitude, longitude);

      return returnValue;
    }

    public static AutoLocation ParseLocation(string latitude, string longitude)
    {
      AutoLocation returnValue = new AutoLocation();
      returnValue.Success = true;
      returnValue.Latitude = decimal.Parse(latitude, NumberStyles.Float, CultureInfo.InvariantCulture);
      returnValue.Longitude = decimal.Parse(longitude, NumberStyles.Float, CultureInfo.InvariantCulture);
      return returnValue;
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

  public struct AutoLocation
  {
    public bool Success;
    public decimal Latitude;
    public decimal Longitude;
    public string Errortext;
  }

  public enum Status
  {
    Automatic,
    Off
  }

}