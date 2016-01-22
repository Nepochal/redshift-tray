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

    public bool isOpen { get; private set; }

    public DebugConsole()
    {
      InitializeComponent();
      isOpen = true;
    }

    private void Window_Closed(object sender, EventArgs e)
    {
      isOpen = false;
    }

    private void ButtonClipboard_Click(object sender, RoutedEventArgs e)
    {
      Clipboard.SetText(Output.Text);
    }

    private void ButtonClose_Click(object sender, RoutedEventArgs e)
    {
      Close();
    }

    public void WriteLog(string message, LogType logType)
    {
      string log = string.Format("{0} {1}: {2}", DateTime.Now.ToString("HH:mm:ss"), logType.ToString(), message);
      Output.Text += log + Environment.NewLine;
    }

    public enum LogType 
    {
      Info,
      Error,
      Redshift
    }

  }
}
