using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hardcodet.Wpf.TaskbarNotification;

namespace redshift_tray
{
  class TrayIcon
  {

    private static TrayIcon TrayIconInstance;
    private TaskbarIcon TaskbarIconInstance;

    private TrayIcon()
    {
      if(TrayIconInstance != null)
      {
        TrayIconInstance.TaskbarIconInstance.Dispose();
      }

      TaskbarIconInstance = new TaskbarIcon();
      TaskbarIconInstance.Icon = Properties.Resources.TrayIcon;
    }

    public static TrayIcon Create()
    {
      TrayIconInstance = new TrayIcon();
      return TrayIconInstance;
    }

    public static TrayIcon CreateOrGet()
    {
      if(TrayIconInstance == null)
        return Create();
      return TrayIconInstance;
    }

  }
}
