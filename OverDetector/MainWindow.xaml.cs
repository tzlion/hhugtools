using System;
using System.Windows;
using Common.Rom;

namespace OverDetector
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        
        
        private void DropAFile(object sender, DragEventArgs e)
        {
            try
            {
                if (e.Data.GetDataPresent("FileName"))
                {
                    string[] filenames = (string[])e.Data.GetData("FileName");
                    OverdumpDetector odd = new OverdumpDetector(filenames[0]);
                    MessageBox.Show(odd.GetSizeInfo());
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show("An error occurred:\r\n" + exception.Message);
            }
        }

    }
}