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
using Hardcodet.Wpf.TaskbarNotification;
using System.Windows.Controls;
using System.Windows;

namespace redshift_tray
{
  class TrayIcon
  {

    private static TrayIcon TrayIconInstance;
    private TaskbarIcon TaskbarIconInstance;

    public event RoutedEventHandler OnMenuItemExitClicked;

    private void MenuItemExitClicked(RoutedEventArgs e)
    {
      if(OnMenuItemExitClicked != null)
      {
        OnMenuItemExitClicked(this, e);
      }
    }

    public event RoutedEventHandler OnMenuItemSettingsClicked;

    private void MenuItemSettingsClicked(RoutedEventArgs e)
    {
      if(OnMenuItemSettingsClicked != null)
      {
        OnMenuItemSettingsClicked(this, e);
      }
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

    private TrayIcon()
    {
      if(TrayIconInstance != null)
      {
        TrayIconInstance.TaskbarIconInstance.Dispose();
      }

      TaskbarIconInstance = new TaskbarIcon();
      TaskbarIconInstance.Icon = Properties.Resources.TrayIcon;
      TaskbarIconInstance.ToolTipText = "Redshift Tray";
      TaskbarIconInstance.ContextMenu = getContextMenu();
    }

    private ContextMenu getContextMenu()
    {
      ContextMenu contextMenu = new ContextMenu();

      MenuItem menuItemSettings = new MenuItem();
      menuItemSettings.Header = "Settings";
      menuItemSettings.Click += menuItemSettings_Click;
      contextMenu.Items.Add(menuItemSettings);

      contextMenu.Items.Add(new Separator());

      MenuItem menuItemExit = new MenuItem();
      menuItemExit.Header = "Exit";
      menuItemExit.Click += menuItemExit_Click;
      contextMenu.Items.Add(menuItemExit);

      return contextMenu;
    }

    void menuItemSettings_Click(object sender, RoutedEventArgs e)
    {
      MenuItemSettingsClicked(e);
    }

    void menuItemExit_Click(object sender, RoutedEventArgs e)
    {
      MenuItemExitClicked(e);
    }

  }
}
