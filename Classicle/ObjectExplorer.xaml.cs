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

namespace Classicle
{
    /// <summary>
    /// Interaction logic for ObjectExplorer.xaml
    /// </summary>
    public partial class ObjectExplorer : Window
    {
        private IDatabase _serverType;
        private readonly List<ObjectViewModel> _tableObjects;
        private readonly List<ObjectViewModel> _viewObjects;

        internal List<ObjectViewModel> SelectedObjects { get; private set; }

        internal IDatabase ServerType
        {
            get { return _serverType; }
            set
            {
                _serverType = value;
                RefreshList();
            }
        }

        #region "Constructor"
        public ObjectExplorer()
        {
            InitializeComponent();

            _tableObjects = new List<ObjectViewModel>();
            _viewObjects = new List<ObjectViewModel>();
        }
        #endregion

        #region "Event Handlers"
        private void SelectAllViewsButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (ObjectViewModel obj in _viewObjects) obj.IsChecked = true;
        }

        private void DeselectAllViewsButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (ObjectViewModel obj in _viewObjects) obj.IsChecked = false;
        }

        private void SelectAllTablesButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (ObjectViewModel obj in _tableObjects) obj.IsChecked = true;
        }

        private void DeselectAllTablesButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (ObjectViewModel obj in _tableObjects) obj.IsChecked = false;
        }

        private void FinishedButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedObjects = new List<ObjectViewModel>();

            SelectedObjects.AddRange(_tableObjects.Where(z => z.IsChecked));
            SelectedObjects.AddRange(_viewObjects.Where(z => z.IsChecked));

            if (SelectedObjects.Count == 0)
            {
                MessageBox.Show("You must select at least one object!", "Error!", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                return;
            }

            DialogResult = true;
        }
        #endregion

        #region "Private Methods"
        private void RefreshList()
        {
            _tableObjects.Clear();

            List<ClassicleObject> objects = _serverType.GetTablesAndViews();

            foreach (ClassicleObject obj in objects.Where(z => !z.IsView).OrderBy(z => z.ObjectName))
            {
                _tableObjects.Add(new ObjectViewModel { IsChecked = false, ObjectName = obj.ObjectName, IsView = false });
            }

            foreach (ClassicleObject obj in objects.Where(z => z.IsView).OrderBy(z => z.ObjectName))
            {
                _viewObjects.Add(new ObjectViewModel { IsChecked = false, ObjectName = obj.ObjectName, IsView = true });
            }

            TablesListView.ItemsSource = _tableObjects;
            ViewsListView.ItemsSource = _viewObjects;
        }
        #endregion
    }
}
