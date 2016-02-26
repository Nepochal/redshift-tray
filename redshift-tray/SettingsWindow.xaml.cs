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
using Microsoft.Win32;
using redshift_tray.Properties;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using Xceed.Wpf.Toolkit;
using Xceed.Wpf.Toolkit.Core.Input;

namespace redshift_tray
{
  public partial class SettingsWindow : Window
  {

    private Redshift.ExecutableError _ExecutableErrorState;

    private Redshift.ExecutableError ExecutableErrorState
    {
      get { return _ExecutableErrorState; }
      set
      {
        _ExecutableErrorState = value;
        switch(value)
        {
          case Redshift.ExecutableError.MissingPath:
            Run run;
            RedshiftInfo.Foreground = Brushes.Black;
            RedshiftInfo.Inlines.Clear();

            run = new Run("The required Redshift executable can be downloaded ");
            RedshiftInfo.Inlines.Add(run);

            run = new Run("here on Github");
            Hyperlink github = new Hyperlink(run);
            github.NavigateUri = new System.Uri(Main.RELEASES_PAGE);
            github.RequestNavigate += Hyperlink_RequestNavigate;
            RedshiftInfo.Inlines.Add(github);

            run = new Run(".");
            RedshiftInfo.Inlines.Add(run);
            break;
          case Redshift.ExecutableError.NotFound:
            RedshiftInfo.Foreground = Brushes.Red;
            RedshiftInfo.Text = "Invalid path to Redshift executable.";
            break;
          case Redshift.ExecutableError.WrongApplication:
            RedshiftInfo.Foreground = Brushes.Red;
            RedshiftInfo.Text = "Executable seems not to be a valid Redshift binary.";
            break;
          case Redshift.ExecutableError.WrongVersion:
            RedshiftInfo.Foreground = Brushes.Red;
            RedshiftInfo.Text = string.Format("The Redshift version is be too old. Please use at least version {0}.{1}.", Redshift.MIN_REDSHIFT_VERSION[0], Redshift.MIN_REDSHIFT_VERSION[1]);
            break;
          case Redshift.ExecutableError.Ok:
            RedshiftInfo.Foreground = Brushes.Green;
            RedshiftInfo.Text = "Redshift executable is suitable.";
            break;
        }
        SetOkButtonEnabled();
      }
    }

    public SettingsWindow()
    {
      InitializeComponent();
      LoadPosition();
      LoadConfig();
      ExecutableErrorState = Redshift.CheckExecutable(RedshiftPath.Text);
      SetOkButtonEnabled();
    }

    public SettingsWindow(Redshift.ExecutableError initialRedshiftErrorNote)
    {
      InitializeComponent();
      LoadPosition();
      LoadConfig();
      ExecutableErrorState = initialRedshiftErrorNote;
    }

    private void SavePosition()
    {
      Settings settings = Settings.Default;

      settings.SettingsWindowLeft = this.Left;
      settings.SettingsWindowTop = this.Top;

      settings.Save();
    }

    private void LoadPosition()
    {
      Settings settings = Settings.Default;

      if(Common.isOutOfBounds(settings.SettingsWindowLeft, settings.SettingsWindowTop))
      {
        return;
      }

      this.WindowStartupLocation = System.Windows.WindowStartupLocation.Manual;
      this.Left = settings.SettingsWindowLeft;
      this.Top = settings.SettingsWindowTop;
    }

    private void SaveConfig()
    {
      Settings settings = Settings.Default;

      settings.RedshiftAppPath = RedshiftPath.Text;
      settings.RedshiftEnabledOnStart = (bool)EnabledOnStart.IsChecked;
      settings.RedshiftLatitude = (decimal)Latitude.Value;
      settings.RedshiftLongitude = (decimal)Longitude.Value;
      settings.RedshiftTemperatureDay = (int)TemperatureDay.Value;
      settings.RedshiftTemperatureNight = (int)TemperatureNight.Value;
      settings.RedshiftTransition = (bool)Transition.IsChecked;
      settings.RedshiftBrightnessDay = (decimal)BrightnessDay.Value;
      settings.RedshiftBrightnessNight = (decimal)BrightnessNight.Value;
      settings.RedshiftGammaRed = (decimal)GammaRed.Value;
      settings.RedshiftGammaGreen = (decimal)GammaGreen.Value;
      settings.RedshiftGammaBlue = (decimal)GammaBlue.Value;

      Settings.Default.Save();
    }

    private void LoadConfig()
    {
      Settings settings = Settings.Default;

      RedshiftPath.Text = settings.RedshiftAppPath;
      EnabledOnStart.IsChecked = settings.RedshiftEnabledOnStart;
      Latitude.Value = settings.RedshiftLatitude;
      Longitude.Value = settings.RedshiftLongitude;
      TemperatureDay.Value = settings.RedshiftTemperatureDay;
      TemperatureNight.Value = settings.RedshiftTemperatureNight;
      Transition.IsChecked = settings.RedshiftTransition;
      BrightnessDay.Value = settings.RedshiftBrightnessDay;
      BrightnessNight.Value = settings.RedshiftBrightnessNight;
      GammaRed.Value = settings.RedshiftGammaRed;
      GammaGreen.Value = settings.RedshiftGammaGreen;
      GammaBlue.Value = settings.RedshiftGammaBlue;
    }

    private bool CheckConfig()
    {
      return (_ExecutableErrorState == Redshift.ExecutableError.Ok);
    }

    private void SetOkButtonEnabled()
    {
      OkButton.IsEnabled = CheckConfig();
    }

    private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
    {
      System.Diagnostics.Process.Start("https://github.com/jonls/redshift/releases");
    }

    private void redshiftPath_LostFocus(object sender, RoutedEventArgs e)
    {
      ExecutableErrorState = Redshift.CheckExecutable(RedshiftPath.Text);
    }

    private void ButtonRedshift_Click(object sender, RoutedEventArgs e)
    {
      OpenFileDialog openFileDialog = new OpenFileDialog();
      openFileDialog.Title = "Redshift path";
      openFileDialog.Filter = "Redshift|redshift.exe|All executables|*.exe";
      openFileDialog.CheckFileExists = true;

      if(File.Exists(RedshiftPath.Text))
      {
        openFileDialog.InitialDirectory = Path.GetDirectoryName(RedshiftPath.Text);
      }

      if((bool)openFileDialog.ShowDialog())
      {
        RedshiftPath.Text = openFileDialog.FileName;
        ExecutableErrorState = Redshift.CheckExecutable(RedshiftPath.Text);
      }
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
      SaveConfig();
      DialogResult = true;
      Close();
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
      SavePosition();
    }

    private void Decimal_InputValidationError(object sender, InputValidationErrorEventArgs e)
    {
      string value = ((DecimalUpDown)sender).Text;
      value = value.Replace(',', '.');

      decimal parseValue;
      if(decimal.TryParse(value, System.Globalization.NumberStyles.Float, new CultureInfo("en-US"), out parseValue))
      {
        ((DecimalUpDown)sender).Value = parseValue;
      }
    }

  }
}
