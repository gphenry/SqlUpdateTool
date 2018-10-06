using System;
using System.Collections.Generic;
using System.Data;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SqlUpdateTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            OutputTextBox.TextChanged += (sender, e) => { OutputTextBox.ScrollToEnd(); };
            SqlTextBox.TextChanged += (sender, e) => { SqlTextBox.ScrollToEnd(); };

            Database.TestFirebirdConnection();
        }

        private void FileSelectButton_OnClick(object sender, RoutedEventArgs e)
        {
            FileTextBox.Text = Service.GetFileName();
            _dt = Service.ExcelToDataTable(FileTextBox.Text, null);
            if (_dt == null)
            {
                InputDataGrid.ItemsSource = (new DataTable()).DefaultView;
                return;
            }

            InputDataGrid.ItemsSource = _dt.DefaultView;

        }

        private void ProcessButton_OnClick(object sender, RoutedEventArgs e)
        {
            OutputTextBox.Text = "";
            var errorHandler = new Service.StatusHandler(delegate (string msg)
            {
                OutputTextBox.Text = string.IsNullOrWhiteSpace(OutputTextBox.Text) ? msg : OutputTextBox.Text + "\r\n" + msg;
                Service.ProcessUiTasks();
            });

            SqlTextBox.Text = "";
            var sqlHandler = new Service.StatusHandler(delegate (string msg)
            {
                SqlTextBox.Text = string.IsNullOrWhiteSpace(SqlTextBox.Text) ? msg : SqlTextBox.Text + "\r\n" + msg;
                Service.ProcessUiTasks();
            });

            Service.ProcessDatatable(_dt, errorHandler, sqlHandler, true, false);
        }

        private DataTable _dt;
    }
}
