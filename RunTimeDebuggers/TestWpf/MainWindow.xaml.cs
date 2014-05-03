using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TestWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();


            lstItems.Items.Add("Item 1");
            lstItems.Items.Add("Item 2");
            lstItems.Items.Add("Item 3");
            lstItems.Items.Add("Item 4");
            lstItems.Items.Add("Item 5");
        }

        private void mnuNew_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("New");

        }


        private void btnTest_Click(object sender, RoutedEventArgs e)
        {
            txtTest.Text = "This is some text";
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            
            RunTimeDebuggers.ActionsForm frm = new RunTimeDebuggers.ActionsForm();
            frm.Show();


        }
    }
}
