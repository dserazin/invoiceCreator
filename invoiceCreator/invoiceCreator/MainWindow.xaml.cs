using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using invoiceCreator;
using Microsoft.Win32;

namespace InvoiceCreator
{

    public partial class MainWindow : Window
    {
        private string dataFilePath = "CustomerData.csv";

        public MainWindow()
        {
            InitializeComponent();
            LoadCustomerData();  // Beim Start gespeicherte Daten laden

            // CodePagesEncodingProvider registrieren
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        }
        private void SaveCustomerData()
        {
            // Daten aus den Eingabefeldern lesen
            var customerData = new CustomerData
            {
                CustomerName = txtCustomerName.Text,
                Address = txtCustomerAddress.Text,
                City = txtCustomerCity.Text,
                ProductName = txtProductName.Text,
                Quantity = int.TryParse(txtQuantity.Text, out int q) ? q : 0,
                UnitPrice = decimal.TryParse(txtUnitPrice.Text, out decimal up) ? up : 0
            };

            // Daten in CSV-Format konvertieren und speichern
            using (var writer = new StreamWriter(dataFilePath, true)) // 'true' für Anhängen
            {
                writer.WriteLine($"{customerData.CustomerName},{customerData.Address},{customerData.City},{customerData.ProductName},{customerData.Quantity},{customerData.UnitPrice}");
            }
        }

        private void LoadCustomerData()
        {
            if (File.Exists(dataFilePath))
            {
                using (var reader = new StreamReader(dataFilePath))
                {
                    List<CustomerData> customerList = new List<CustomerData>();
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        var parts = line.Split(',');
                        if (parts.Length == 6)
                        {
                            customerList.Add(new CustomerData
                            {
                                CustomerName = parts[0],
                                Address = parts[1],
                                City = parts[2],
                                ProductName = parts[3],
                                Quantity = int.Parse(parts[4]),
                                UnitPrice = decimal.Parse(parts[5])
                            });
                        }
                    }

                    lstCustomerData.ItemsSource = customerList;
                }
            }
        }

        private void LstCustomerData_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstCustomerData.SelectedItem is CustomerData selectedData)
            {
                txtCustomerName.Text = selectedData.CustomerName;
                txtCustomerAddress.Text = selectedData.Address;
                txtCustomerCity.Text = selectedData.City;
                txtProductName.Text = selectedData.ProductName;
                txtQuantity.Text = selectedData.Quantity.ToString();
                txtUnitPrice.Text = selectedData.UnitPrice.ToString("0.00");
            }
        }

        private void SaveToPDF_Click(object sender, RoutedEventArgs e)
        {
            // Daten aus den Eingabefeldern lesen
            string customerName = txtCustomerName.Text;
            string customerAddress = txtCustomerAddress.Text;
            string customerCity = txtCustomerCity.Text;
            string productName = txtProductName.Text;
            int quantity = int.TryParse(txtQuantity.Text, out int q) ? q : 0;
            decimal unitPrice = decimal.TryParse(txtUnitPrice.Text, out decimal up) ? up : 0;
            decimal totalPrice = CalculateTotalPrice(quantity, unitPrice);

            // Setze den Gesamtpreis in txtTotalPrice und txtTotal
            txtTotalPrice.Text = totalPrice.ToString("0.00 €");
            

            // Auswahloption: Speichern oder Vorschau
            MessageBoxResult result = MessageBox.Show("Möchten Sie die PDF speichern?", "PDF speichern oder Vorschau anzeigen", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // Datei speichern
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "PDF files (*.pdf)|*.pdf",
                    FileName = "Rechnung.pdf"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    PDFHelper.GenerateInvoicePDF(saveFileDialog.FileName, customerName, customerAddress, customerCity, productName, quantity, unitPrice, cmbCalculationMethod.Text, totalPrice);
                    MessageBox.Show("PDF wurde erfolgreich erstellt und gespeichert.", "Erfolg", MessageBoxButton.OK, MessageBoxImage.Information);

                    SaveCustomerData(); // Daten speichern
                    MessageBox.Show("Kundendaten wurden gespeichert.", "Erfolg", MessageBoxButton.OK, MessageBoxImage.Information);

                    // PDF im Standard-PDF-Viewer Ã¶ffnen
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = saveFileDialog.FileName,
                        UseShellExecute = true
                    });
                }
            }
            else if (result == MessageBoxResult.No)
            {
                // TemporÃ¤re Datei fÃ¼r die Vorschau erstellen
                string tempFilePath = Path.Combine(Path.GetTempPath(), "Rechnung_Preview.pdf");

                // PDF in der temporÃ¤ren Datei erstellen
                PDFHelper.GenerateInvoicePDF(tempFilePath, customerName, customerAddress, customerCity, productName, quantity, unitPrice, cmbCalculationMethod.Text, totalPrice);

                // PDF im Standard-PDF-Viewer Ã¶ffnen
                Process pdfProcess = Process.Start(new ProcessStartInfo
                {
                    FileName = tempFilePath,
                    UseShellExecute = true
                }
                );

                // Warte, bis der Benutzer das PDF-Fenster schlieÃŸt, und lÃ¶sche dann die Datei
                pdfProcess.WaitForExit();
                File.Delete(tempFilePath);
            }
            // Wenn Abbrechen ausgewÃ¤hlt wurde, wird nichts weiter ausgefÃ¼hrt
        }

        private void cmbCalculationMethod_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateCostFields();
            // Stelle sicher, dass die Methode erst ausgefÃ¼hrt wird, wenn eine Auswahl getroffen wurde
            if (cmbCalculationMethod.SelectedItem != null)
            {
                string method = (cmbCalculationMethod.SelectedItem as ComboBoxItem).Content.ToString();
                decimal productionCost = 0.00m;
                decimal overheadCost = 0.00m;
                decimal setupFee = 0.00m;
                decimal projectTransfer = 0.00m;

                // Produktions-, Overhead-, Setup- und ProjektÃ¼bergabekosten basierend auf der ausgewÃ¤hlten Methode berechnen
                if (method == "DTF")
                {
                    productionCost = 8.67m;
                    overheadCost = 6.00m;
                    setupFee = 4.00m;
                    projectTransfer = 2.30m;
                }
                else if (method == "Flock")
                {
                    productionCost = 12.67m;
                    overheadCost = 8.00m;
                    setupFee = 4.00m;
                    projectTransfer = 2.00m;
                }
                else if (method == "Flex")
                {
                    productionCost = 13.67m;
                    overheadCost = 8.00m;
                    setupFee = 4.00m;
                    projectTransfer = 2.00m;
                }

                // Aktualisiere die TextBoxen fÃ¼r Produktions-, Overhead-, Setup- und ProjektÃ¼bergabekosten
                txtProductionCost.Text = productionCost.ToString("0.00");
                txtOverheadCost.Text = overheadCost.ToString("0.00");
                txtSetupFee.Text = setupFee.ToString("0.00");
                txtProjectTransfer.Text = projectTransfer.ToString("0.00");

                // Berechne den Gesamtpreis basierend auf den aktuellen Werten
                UpdateCostFields();
            }
        }

        private void InputFields_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateCostFields();
        }

        private void txtUnitPrice_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateCostFields();
        }

        private void txtProductionCost_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateCostFields();
        }

        private void txtOverheadCost_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateCostFields();
        }

        private void UpdateCostFields()
        {
            // Berechnungen nur durchfÃ¼hren, wenn die Werte gÃ¼ltig sind
            if (int.TryParse(txtQuantity.Text, out int quantity) &&
                decimal.TryParse(txtUnitPrice.Text, out decimal unitPrice))
            {
                decimal totalPrice = CalculateTotalPrice(quantity, unitPrice);

                // Setze den Gesamtpreis
                txtTotalPrice.Text = totalPrice.ToString("0.00 €");
                
            }
        }

        private decimal CalculateTotalPrice(int quantity, decimal unitPrice)
        {
            decimal setupFee = decimal.TryParse(txtSetupFee.Text, out decimal parsedSetupFee) ? parsedSetupFee : 0.00m;
            decimal projectTransfer = decimal.TryParse(txtProjectTransfer.Text, out decimal parsedProjectTransfer) ? parsedProjectTransfer : 0.00m;
            decimal productionCostValue = decimal.TryParse(txtProductionCost.Text, out decimal parsedProductionCost) ? parsedProductionCost : 0.00m;
            decimal overheadCostValue = decimal.TryParse(txtOverheadCost.Text, out decimal parsedOverheadCost) ? parsedOverheadCost : 0.00m;

            decimal setupFeeTotal = quantity * setupFee;
            decimal productionCostTotal = quantity * productionCostValue;
            decimal overheadCostTotal = quantity * overheadCostValue;
            decimal projectTransferTotal = quantity * projectTransfer;
            decimal textilePriceTotal = quantity * unitPrice;

            decimal totalPrice = setupFeeTotal + productionCostTotal + overheadCostTotal + projectTransferTotal + textilePriceTotal;

            return totalPrice;
        }
    }
}
