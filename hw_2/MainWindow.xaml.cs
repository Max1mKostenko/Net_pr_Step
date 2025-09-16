using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;

namespace WpfApp8
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void BtnBrowse_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Select a file to search",
                Filter = "Text files *.txt|All files *.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                txtFilePath.Text = openFileDialog.FileName;
            }
        }

        private async void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtSearchWord.Text.Trim()))
            {
                MessageBox.Show("Please enter a word to search");
                return;
            }

            if (string.IsNullOrEmpty(txtFilePath.Text.Trim()))
            {
                MessageBox.Show("Please select a file to search in");
                return;
            }

            if (!File.Exists(txtFilePath.Text))
            {
                MessageBox.Show("The file does not exist");
                return;
            }

            btnSearch.IsEnabled = false;
            txtResults.Text = "Searching";

            try
            {
                int count = await SearchWordAsync(txtSearchWord.Text.Trim(), txtFilePath.Text);

                string fileName = Path.GetFileName(txtFilePath.Text);
                txtResults.Text = $"Search completed!\n\n" +
                                 $"Word: '{txtSearchWord.Text.Trim()}'\n" +
                                 $"File: {fileName}\n" +
                                 $"Found: {count} time(s)";
            }
            catch (Exception ex)
            {
                txtResults.Text = $"Error: {ex.Message}";
            }
            finally
            {
                btnSearch.IsEnabled = true;
            }
        }

        private async Task<int> SearchWordAsync(string word, string filePath)
        {
            return await Task.Run(() =>
            {
                try
                {
                    string content = File.ReadAllText(filePath);

                    string lowerContent = content.ToLower();
                    string lowerWord = word.ToLower();

                    int count = 0;
                    int index = 0;

                    while ((index = lowerContent.IndexOf(lowerWord, index)) != -1)
                    {
                        count++;
                        index += lowerWord.Length;
                    }

                    return count;
                }
                catch
                {
                    throw;
                }
            });
        }

        private void txtResults_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
        }
    }
}
