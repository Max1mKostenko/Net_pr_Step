using System;
using System.Windows;
using System.Windows.Input;

namespace WpfApp11
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            lvClients.MouseDoubleClick += LvClients_MouseDoubleClick;

            try
            {
                Server.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Application.Current.Shutdown();
            }
        }

        private void LvClients_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lvClients.SelectedItem is Client selectedClient)
            {
                var result = MessageBox.Show(
                    $"Disconnect client '{selectedClient.Login}'?",
                    "Confirm Disconnect",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    Server.DisconnectClient(selectedClient);
                }
            }
        }
    }
}