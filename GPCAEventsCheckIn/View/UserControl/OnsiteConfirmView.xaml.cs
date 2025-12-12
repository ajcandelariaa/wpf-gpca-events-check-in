using GPCAEventsCheckIn.Model;
using GPCAEventsCheckIn.View.BadgeDesign.PVC;
using GPCAEventsCheckIn.ViewModel;
using Patagames.Pdf.Net;
using Patagames.Pdf.Net.Controls.Wpf;
using System;
using System.Configuration;
using System.Drawing.Printing;
using System.IO;
using System.Printing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GPCAEventsCheckIn.View.UserControl
{
    public partial class OnsiteConfirmView
    {
        private readonly MainViewModel _mainViewModel;
        private string _selectedPrinter;

        public OnsiteConfirmView(MainViewModel mainViewModel)
        {
            InitializeComponent();
            _mainViewModel = mainViewModel;
            GetDefaultPrinter();
            Loaded += OnsiteConfirmView_Loaded;
        }

        private void OnsiteConfirmView_Loaded(object sender, RoutedEventArgs e)
        {
            var onsiteVm = _mainViewModel.OnsiteRegistrationViewModel;

            if (onsiteVm == null || onsiteVm.CurrentDraft == null)
            {
                MessageBox.Show("No onsite draft found. Please start again.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);

                // Go back to AddAttendeeView
                _mainViewModel.NavigateToAddAttendeeView("SearchCategoriesView");
                return;
            }

            var draft = onsiteVm.CurrentDraft;

            // Fill attendee details
            Tb_FullName.Text = draft.Delegate.FullName;
            Tb_JobTitle.Text = draft.Delegate.JobTitle;
            Tb_CompanyName.Text = draft.Delegate.CompanyName;
            Tb_BadgeType.Text = draft.Delegate.BadgeType;

            // Fill pricing details
            Tb_PricingDescription.Text = draft.Pricing.Description;

            Tb_UnitPrice.Text = $"USD {draft.Pricing.UnitPrice:N2}";
            Tb_DiscountPrice.Text = $"USD {draft.Pricing.DiscountPrice:N2}";
            Tb_NetAmount.Text = $"USD {draft.Pricing.NetAmount:N2}";
            Tb_TotalBeforeVat.Text = $"USD {draft.Pricing.TotalBeforeVat:N2}";
            Tb_VatAmount.Text = $"USD {draft.Pricing.VatPrice:N2}";
            Tb_TotalAmount.Text = $"USD {draft.Pricing.TotalAmount:N2}";

            Cb_IsPaid.IsChecked = false;
            Cb_IsPaid.IsEnabled = true;

            Cb_PrintBadge.IsChecked = true;
            Cb_SendEmail.IsChecked = true;
            Cb_AddToApp.IsChecked = false;

            UpdateStatusPreview();
        }

        private void UpdateStatusPreview()
        {
            var onsiteVm = _mainViewModel.OnsiteRegistrationViewModel;
            if (onsiteVm == null || onsiteVm.CurrentDraft == null)
                return;

            var pricing = onsiteVm.CurrentDraft.Pricing;

            bool isFree = pricing.TotalAmount == 0m;
            bool isPaid = Cb_IsPaid.IsChecked == true;

            string registrationStatus;
            string paymentStatus;

            if (!isFree && isPaid)
            {
                registrationStatus = "Confirmed";
                paymentStatus = "Paid";
            }
            else if (!isFree && !isPaid)
            {
                registrationStatus = "Pending";
                paymentStatus = "Unpaid";
            }
            else if (isFree && isPaid)
            {
                registrationStatus = "Confirmed";
                paymentStatus = "Free";
            }
            else
            {
                // free && !paid
                registrationStatus = "Pending";
                paymentStatus = "Free";
            }

            string badgeText = (Cb_PrintBadge.IsChecked == true) ? "Badge will be printed." : "Badge will not be printed.";
            string emailText = (Cb_SendEmail.IsChecked == true) ? "An email will be sent." : "No email will be sent.";
            string appText = (Cb_AddToApp.IsChecked == true) ? "Attendee will be added to the app." : "Attendee will not be added to the app.";

            Tb_StatusPreview.Text =
                $"Registration Status: {registrationStatus}\n" +
                $"Payment Status: {paymentStatus}\n\n" +
                $"{badgeText}\n{emailText}\n{appText}";
        }

        private void OptionChanged(object sender, RoutedEventArgs e)
        {
            UpdateStatusPreview();
        }

        private void Btn_Return(object sender, MouseButtonEventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to cancel this onsite registration?\nThis will reset the process.",
                "Cancel onsite registration",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                if (_mainViewModel.OnsiteRegistrationViewModel != null)
                {
                    _mainViewModel.OnsiteRegistrationViewModel.CurrentDraft = null;
                }

                _mainViewModel.NavigateToAddAttendeeView("SearchCategoriesView");
            }
        }


        private async void Btn_Submit(object sender, RoutedEventArgs e)
        {
            var onsiteVm = _mainViewModel.OnsiteRegistrationViewModel;

            if (onsiteVm == null || onsiteVm.CurrentDraft == null)
            {
                MessageBox.Show("No onsite draft found. Please start again.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var pricing = onsiteVm.CurrentDraft.Pricing;
            bool isPaid = Cb_IsPaid.IsChecked == true;
            bool sendEmail = Cb_SendEmail.IsChecked == true;
            bool printBadge = Cb_PrintBadge.IsChecked == true;
            bool addToApp = Cb_AddToApp.IsChecked == true;

            // Rule: cannot print if not marked as paid
            if (printBadge && !isPaid)
            {
                MessageBox.Show(
                    "Badge cannot be printed while payment is not marked as received.\n\n" +
                    "Please either mark as paid or uncheck 'Print badge now'.",
                    "Invalid option",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            if (addToApp && !isPaid)
            {
                MessageBox.Show(
                    "Attendee cannot be added to the app while payment is not marked as received.\n\n" +
                    "Please either mark as paid or uncheck 'Add attendee to app'.",
                    "Invalid option",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            var confirmResult = MessageBox.Show(
                "Are you sure you want to confirm this onsite registration?",
                "Confirm onsite registration",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirmResult != MessageBoxResult.Yes)
            {
                return;
            }

            _mainViewModel.BackDropStatus = "Visible";
            _mainViewModel.LoadingProgressStatus = "Visible";
            _mainViewModel.LoadingProgressMessage = "Confirming onsite registration...";

            try
            {
                // Call backend /confirm and get the attendeeDetails back
                var newAttendee = await onsiteVm.ConfirmOnsiteRegistrationAsync(
                    isPaid,
                    sendEmail,
                    printBadge,
                    addToApp);

                if (newAttendee == null)
                {
                    _mainViewModel.LoadingProgressStatus = "Collapsed";
                    _mainViewModel.BackDropStatus = "Collapsed";
                    _mainViewModel.LoadingProgressMessage = "Loading...";
                    return;
                }

                // Add to list (no full refresh)
                if (_mainViewModel.AttendeeViewModel != null)
                {
                    var list = _mainViewModel.AttendeeViewModel.ConfirmedAttendees;
                    var existing = list.FirstOrDefault(a =>
                        a.Id == newAttendee.Id && a.DelegateType == newAttendee.DelegateType);

                    if (existing == null)
                    {
                        list.Add(newAttendee);
                    }
                }

                // Set as current attendee so the badge generator uses it
                _mainViewModel.CurrentAttendee = newAttendee;

                // If paid + print checked -> generate PDF and send to printer
                if (printBadge && isPaid)
                {
                    _mainViewModel.LoadingProgressMessage = "Generating badge...";
                    await Task.Delay(1000);
                    GeneratePdfAndPrint(newAttendee);
                }

                // Clear draft and close loader
                onsiteVm.CurrentDraft = null;

                _mainViewModel.LoadingProgressStatus = "Collapsed";
                _mainViewModel.BackDropStatus = "Collapsed";
                _mainViewModel.LoadingProgressMessage = "Loading...";

                MessageBox.Show(
                    printBadge && isPaid
                        ? "Onsite registration completed and badge is printing."
                        : "Onsite registration has been completed successfully.",
                    "Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                _mainViewModel.NavigateToSearchCategoryViewV2();
            }
            catch (Exception ex)
            {
                _mainViewModel.LoadingProgressStatus = "Collapsed";
                _mainViewModel.BackDropStatus = "Collapsed";
                _mainViewModel.LoadingProgressMessage = "Loading...";

                MessageBox.Show($"Error confirming registration: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void GetDefaultPrinter()
        {
            try
            {
                LocalPrintServer printServer = new LocalPrintServer();
                PrintQueue defaultPrintQueue = LocalPrintServer.GetDefaultPrintQueue();
                _selectedPrinter = defaultPrintQueue.Name;
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

        private void GeneratePdfAndPrint(AttendeeModel attendee)
        {
            try
            {
                BadgePVCPDF generatedBadge = new BadgePVCPDF(_mainViewModel);
                var document = generatedBadge.GeneratePdf();

                if (document == null)
                {
                    return;
                }

                string filename = attendee.TransactionId + ".pdf";
                SavePdfDocumentAndPrint(document, filename);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error generating badge: " + ex.Message);
            }
        }

        private void SavePdfDocumentAndPrint(PdfSharp.Pdf.PdfDocument document, string filePath)
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

                PrintPdf(pdfFilePath, _selectedPrinter);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving PDF: " + ex.Message);
            }
        }

        private void PrintPdf(string pdfFilePath, string printerName)
        {
            if (!File.Exists(pdfFilePath))
            {
                MessageBox.Show("File not found: " + pdfFilePath);
                return;
            }

            try
            {
                _mainViewModel.LoadingProgressMessage = "Printing badge...";

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
                }
                finally
                {
                    pdfPrintDocument?.Dispose();
                    pdfDocument?.Dispose();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Printing failed: " + ex.Message);
            }
        }


    }
}
