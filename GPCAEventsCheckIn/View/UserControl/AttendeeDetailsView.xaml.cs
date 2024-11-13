using GPCAEventsCheckIn.View.BadgeDesign.Common;
using GPCAEventsCheckIn.View.BadgeDesign.PVC;
using GPCAEventsCheckIn.View.BadgeDesign.WithQR;
using GPCAEventsCheckIn.View.Window;
using GPCAEventsCheckIn.ViewModel;
using Patagames.Pdf.Net;
using Patagames.Pdf.Net.Controls.Wpf;
using System.Configuration;
using System.Drawing.Printing;
using System.IO;
using System.Printing;
using System.Windows;

namespace GPCAEventsCheckIn.View.UserControl
{
    public partial class AttendeeDetailsView
    {
        private MainViewModel _mainViewModel;
        public string selectedPrinter;

        public AttendeeDetailsView(MainViewModel mainViewModel)
        {
            InitializeComponent();
            GetDefaultPrinter();
            _mainViewModel = mainViewModel;
        }

        private void GetDefaultPrinter()
        {
            try
            {
                LocalPrintServer printServer = new LocalPrintServer();
                PrintQueue defaultPrintQueue = LocalPrintServer.GetDefaultPrintQueue();
                selectedPrinter = defaultPrintQueue.Name;
            }
            catch (PrintQueueException pqe)
            {
                MessageBox.Show($"PrintQueueException: {pqe.Message}");
            }
            catch (PrintSystemException pse)
            {
                MessageBox.Show($"PrintSystemException: {pse.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An unexpected error occurred: {ex.Message}");
            }
        }


        private void Btn_Print(object sender, RoutedEventArgs e)
        {
            _mainViewModel.BackDropStatus = "Visible";
            _mainViewModel.LoadingProgressStatus = "Visible";
            _mainViewModel.LoadingProgressMessage = "Generating badge...";
            GeneratePdf();
        }

        private void GeneratePdf()
        {
            //BadgeCommonPDF generatedBadge = new BadgeCommonPDF(_mainViewModel);
            //BadgeWithQRPDF generatedBadge = new BadgeWithQRPDF(_mainViewModel);
            //BadgePVCPDF generatedBadge = new BadgePVCPDF(_mainViewModel);
            BadgePVCPDFv2 generatedBadge = new BadgePVCPDFv2(_mainViewModel);
            var document = generatedBadge.GeneratePdf();

            if(document != null)
            {
                string filename = _mainViewModel.CurrentAttendee.TransactionId + ".pdf";
                SavePdfDocument(document, filename);
            } else
            {
                return;
            }
        }

        private void SavePdfDocument(PdfSharp.Pdf.PdfDocument document, string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                { 
                    File.Delete(filePath);
                }
                document.Save(filePath);
                document.Dispose();

                FileInfo f = new FileInfo(filePath);
                string pdfFilePath = f.FullName;
                PrintPdf(pdfFilePath, selectedPrinter);
                _mainViewModel.BackDropStatus = "Collapsed";
                _mainViewModel.LoadingProgressStatus = "Collapsed";
            }
            catch (Exception ex)
            {
                _mainViewModel.BackDropStatus = "Collapsed";
                _mainViewModel.LoadingProgressStatus = "Collapsed";
                MessageBox.Show("Error saving PDF: " + ex.Message);
                return;
            }
        }

        private async void PrintPdf(string pdfFilePath, string printerName)
        {
            _mainViewModel.LoadingProgressMessage = "Printing badge...";
            if (File.Exists(pdfFilePath))
            {
                Patagames.Pdf.Net.PdfCommon.Initialize(ConfigurationManager.AppSettings["PATAGAMES_KEY"]);
                Patagames.Pdf.Net.Controls.Wpf.PdfPrintDocument pdfPrintDocument = null;
                Patagames.Pdf.Net.PdfDocument pdfDocument = null;
                try
                {
                    pdfDocument = PdfDocument.Load(pdfFilePath);
                    pdfPrintDocument = new PdfPrintDocument(pdfDocument);
                    pdfPrintDocument.DefaultPageSettings.Margins = new Margins(0, 0, 0, 0);
                    pdfPrintDocument.PrinterSettings.PrinterName = printerName;
                    pdfPrintDocument.Print();

                    //await _mainViewModel.AttendeeViewModel.PrintBadge(
                    //    ConfigurationManager.AppSettings["ApiCode"],
                    //    _mainViewModel.CurrentAttendee.Id,
                    //    _mainViewModel.CurrentAttendee.DelegateType
                    // );
                }
                catch (Exception ex)
                {
                    _mainViewModel.BackDropStatus = "Collapsed";
                    _mainViewModel.LoadingProgressStatus = "Collapsed";
                    MessageBox.Show("Printing failed: " + ex.Message);
                    return;
                }
                finally
                {
                    pdfPrintDocument?.Dispose();
                    pdfDocument?.Dispose();
                }
            }
            else
            {
                _mainViewModel.BackDropStatus = "Collapsed";
                _mainViewModel.LoadingProgressStatus = "Collapsed";
                MessageBox.Show("File not found: " + pdfFilePath);
                return;
            }
        }

        private void Btn_Cancel(object sender, RoutedEventArgs e)
        {
            _mainViewModel.ReturnBack();
        }

        private void Btn_Edit(object sender, RoutedEventArgs e)
        {
            _mainViewModel.BackDropStatus = "Visible";
            AdminLoginDetailsView adminLoginDetailsView = new AdminLoginDetailsView(_mainViewModel);
            adminLoginDetailsView.Owner = Application.Current.MainWindow;
            adminLoginDetailsView.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            adminLoginDetailsView.Topmost = true;
            adminLoginDetailsView.ShowDialog();
        }
    }
}
