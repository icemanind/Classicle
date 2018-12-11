using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using IWin32Window = System.Windows.Forms.IWin32Window;

namespace Classicle
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region "Fields"
        private readonly ISettings _settings = new Implementations.Settings.StaticSettings();
        #endregion

        #region "Constructor"
        public MainWindow()
        {
            InitializeComponent();

            Settings settings = _settings.LoadSettings();
            SetFormValues(settings);
        }
        #endregion

        #region "Event Handler Methods"
        private void Window_Closing(object sender, EventArgs e)
        {
            _settings.SaveSettings(GetFormValues());
        }

        private void SqlServerDatabaseName_LostFocus(object sender, RoutedEventArgs e)
        {
            RefreshNamespaceName();
        }

        private void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshNamespaceName();
        }

        private void SqlServerServerPort_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var regex = new Regex("[^0-9]+");

            e.Handled = regex.IsMatch(e.Text);
        }

        private void SqlServerServerPort_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            var regex = new Regex("[^0-9]+");

            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text = (string)e.DataObject.GetData(typeof(string));
                if (regex.IsMatch(text ?? ""))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        private void SqliteFileNameBrowse_Click(object sender, RoutedEventArgs e)
        {
            var f = new OpenFileDialog
            {
                ShowReadOnly = false,
                ShowHelp = false,
                SupportMultiDottedExtensions = false,
                Title = @"Open SQLite Database"
            };
            IWin32Window win32Window = new NativeWindow();
            ((NativeWindow)win32Window).AssignHandle(new WindowInteropHelper(this).Handle);

            DialogResult dr = f.ShowDialog(win32Window);
            if (dr == System.Windows.Forms.DialogResult.Cancel) return;
            SqliteFileName.Text = f.FileName;
        }

        private void OutputBrowse_Click(object sender, RoutedEventArgs e)
        {
            var f = new FolderBrowserDialog { Description = @"Select a folder for Classicle to place output files" };
            IWin32Window win32Window = new NativeWindow();
            ((NativeWindow)win32Window).AssignHandle(new WindowInteropHelper(this).Handle);

            DialogResult dr = f.ShowDialog(win32Window);

            if (dr == System.Windows.Forms.DialogResult.Cancel) return;
            OutputPath.Text = f.SelectedPath;
        }

        private void CreateClasses_Click(object sender, RoutedEventArgs e)
        {
            CreateClasses.IsEnabled = false;
            var oe = new ObjectExplorer();
            IDatabase database = new Implementations.Database.None();
            string name = (string)((ComboBoxItem)ServerTypeComboBox.SelectedValue).Content;
            Settings settings = GetFormValues();

            if (name == "SQL Server")
            {
                database = new Implementations.Database.SqlServer
                {
                    DatabaseName = settings.SqlServerDatabaseName,
                    DefaultSchema = settings.SqlServerSchemaName,
                    ServerName = settings.SqlServerServerName,
                    ServerPort = settings.SqlServerServerPort,
                    TrustedConnection = settings.SqlServerTrustedConnection
                };
                if (!database.TrustedConnection)
                {
                    database.Username = settings.SqlServerUsername;
                    database.Password = settings.SqlServerPassword;
                }
            }

            if (name == "Sqlite")
            {
                database = new Implementations.Database.Sqlite
                {
                    DatabaseName = settings.SqliteFileName,
                    Password = settings.SqlitePassword
                };
            }

            if (name == "MySQL")
            {
                database = new Implementations.Database.MySql
                {
                    DatabaseName = settings.MySqlDatabaseName,
                    ServerName = settings.MySqlServerName,
                    ServerPort = settings.MySqlServerPort,
                    Username = settings.MySqlUsername,
                    Password = settings.MySqlPassword
                };
            }

            database.Language = settings.Language;
            database.DefaultNamespace = settings.Namespace;
            database.UseDapperExtensions = settings.UseDapperExtensions;
            database.UseBackingFields = settings.UseBackingFields;

            oe.ServerType = database;

            bool? dr = oe.ShowDialog();

            CreateClasses.IsEnabled = true;

            if (!dr.HasValue || !dr.Value) return;
            CreateClasses.IsEnabled = false;
            List<ObjectViewModel> objects = oe.SelectedObjects;

            database?.CreateLayers(objects, settings.OutputFolder);
            CreateClasses.IsEnabled = true;
            System.Windows.MessageBox.Show("Classes Created!", "Successful!", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        #endregion

        #region "Private Methods"
        private void RefreshNamespaceName()
        {
            if (string.IsNullOrWhiteSpace(SqlServerDatabaseName.Text))
            {
                NamespaceName.Text = "";
                return;
            }

            NamespaceName.Text = Utility.GetPascalCaseName(SqlServerDatabaseName.Text);
        }

        private void SetFormValues(Settings settings)
        {
            ServerTypeComboBox.SelectedIndex = (int)settings.ServerType - 1;
            SqlServerServerName.Text = settings.SqlServerServerName;
            SqlServerServerPort.Text = settings.SqlServerServerPort.ToString();
            SqlServerDatabaseName.Text = settings.SqlServerDatabaseName;
            SqlServerDefaultSchema.Text = settings.SqlServerSchemaName;
            SqlServerTrustedConnection.IsChecked = settings.SqlServerTrustedConnection;
            SqlServerUsername.Text = settings.SqlServerUsername;
            SqlServerPassword.Password = settings.SqlServerPassword;

            OutputPath.Text = settings.OutputFolder;
            LanguageComboBox.SelectedIndex = (int)settings.Language - 1;
            NamespaceName.Text = settings.Namespace;
            UseDapperExtensionsCheckBox.IsChecked = settings.UseDapperExtensions;
            UseBackingFieldsCheckBox.IsChecked = settings.UseBackingFields;

            SqliteFileName.Text = settings.SqliteFileName;
            SqlitePassword.Password = settings.SqlitePassword;

            MySqlServerName.Text = settings.MySqlServerName;
            MySqlServerPort.Text = settings.MySqlServerPort.ToString();
            MySqlUsername.Text = settings.MySqlUsername;
            MySqlPassword.Password = settings.MySqlPassword;
            MySqlDatabaseName.Text = settings.MySqlDatabaseName;
        }

        private Settings GetFormValues()
        {
            var settings = new Settings
            {
                ServerType = (Settings.ServerTypes) (ServerTypeComboBox.SelectedIndex + 1),
                SqlServerServerName = SqlServerServerName.Text,
                SqlServerServerPort = int.Parse(SqlServerServerPort.Text),
                SqlServerDatabaseName = SqlServerDatabaseName.Text,
                SqlServerSchemaName = SqlServerDefaultSchema.Text,
                SqlServerTrustedConnection = SqlServerTrustedConnection.IsChecked ?? false,
                SqlServerUsername = SqlServerUsername.Text,
                SqlServerPassword = SqlServerPassword.Password,
                OutputFolder = OutputPath.Text,
                Language = (Settings.Languages) (LanguageComboBox.SelectedIndex + 1),
                Namespace = NamespaceName.Text,
                UseDapperExtensions = UseDapperExtensionsCheckBox.IsChecked ?? true,
                UseBackingFields = UseBackingFieldsCheckBox.IsChecked ?? false,
                SqliteFileName = SqliteFileName.Text,
                SqlitePassword = SqlitePassword.Password,
                MySqlServerName = MySqlServerName.Text,
                MySqlDatabaseName = MySqlDatabaseName.Text,
                MySqlUsername = MySqlUsername.Text,
                MySqlPassword = MySqlPassword.Password,
                MySqlServerPort = int.Parse(MySqlServerPort.Text)
            };

            return settings;
        }
        #endregion
    }
}
