using System.Windows;
using System.Windows.Controls;

namespace Orchestrator;

public partial class MainWindow : Window
{
    private List<string> _workerAddresses;
    
    public MainWindow()
    {
        InitializeComponent();
    }

    private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {

    }

    private async void AcceptWrokersButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(WorkersAddressesTextBox.Text))
        {
            MessageBox.Show("Please enter workers addresses!!!");

            return;
        }

        if (string.IsNullOrWhiteSpace(PasswordTextBox.Text))
        {
            MessageBox.Show("Please enter password!!!");

            return;
        }

        _workerAddresses = WorkersAddressesTextBox.Text.Trim().Split(',', ' ').ToList();

        await InitializeConnectionAsync(ResultTextBlock);
    }

    private async Task InitializeConnectionAsync(TextBlock resultTextBox)
    {
        var startTime = DateTime.Now;

        try
        {
            var orchestrator = new PasswordOrchestrator(_workerAddresses, resultTextBox.Text);

            await orchestrator.DistributeAndCrackPasswordAsync(resultTextBox);
        }
        catch (Grpc.Core.RpcException e)
        {
            MessageBox.Show($"Ошибка gRPC: {e.Message}");
        }
        catch (Exception e)
        {
            MessageBox.Show($"Произошла ошибка: {e.Message}");
        }
        finally
        {
            ResultTextBlock.Text += $"\nWorking time: {(DateTime.Now - startTime).TotalSeconds} seconds";
        }
    }

    private void ClearResultsButton_Click(object sender, RoutedEventArgs e)
    {
        ResultTextBlock.Text = string.Empty;
    }

    private void AdressesClearButton_Click(object sender, RoutedEventArgs e)
    {
        WorkersAddressesTextBox.Text = string.Empty;
    }

    private void PasswordClearButton_Click(object sender, RoutedEventArgs e)
    {
        PasswordTextBox.Text = string.Empty;
    }
}
