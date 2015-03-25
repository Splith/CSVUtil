using Microsoft.Win32;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CsvUtil.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.Initialized += MainWindow_Initialized;

            InitializeComponent();
        }

        void MainWindow_Initialized(object sender, EventArgs e)
        {
            this.DataContext = new CsvUtil.MVVM.MasterParts.PartsViewModel();
        }

        private void FilePicker_Click(object sender, RoutedEventArgs e)
        {
            var fd = new OpenFileDialog();
            fd.Filter = "csv files (*.csv)|*.csv|All files (*.*)|*.*";
            fd.InitialDirectory = System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile);

            if (fd.ShowDialog() == true)
                this.FilePath.Text = fd.FileName;
        }

    }
}
