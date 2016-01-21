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
    public DebugConsole()
    {
      InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      App.DEBUG = true;
    }

    private void Window_Closed(object sender, EventArgs e)
    {
      App.DEBUG = false;
    }

    private void ButtonClipboard_Click(object sender, RoutedEventArgs e)
    {
      Clipboard.SetText(Output.Text);
    }

    private void ButtonClose_Click(object sender, RoutedEventArgs e)
    {
      Close();
    }

  }
}
