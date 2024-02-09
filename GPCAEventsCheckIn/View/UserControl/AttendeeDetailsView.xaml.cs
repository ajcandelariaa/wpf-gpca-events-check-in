using GPCAEventsCheckIn.Helper;
using GPCAEventsCheckIn.Model;
using GPCAEventsCheckIn.ViewModel;
using Patagames.Pdf.Net.Controls.Wpf;
using PdfSharp.Drawing;
using QRCoder;
using System.Drawing.Imaging;
using System.IO;
using System.Printing;
using System.Text;
using System.Windows;

namespace GPCAEventsCheckIn.View.UserControl
{
    /// <summary>
    /// Interaction logic for AttendeeDetailsView.xaml
    /// </summary>
    public partial class AttendeeDetailsView
    {
        private MainViewModel _mainViewModel;
        public string selectedPrinter;

        public AttendeeDetailsView(MainViewModel mainViewModel)
        {
            InitializeComponent();
            InitializePrinters();
            _mainViewModel = mainViewModel;
        }
        void InitializePrinters()
        {
            //PopulatePrinterComboBox();
            //cbPrinterList.SelectionChanged += PrinterComboBox_SelectionChanged;
            //if (cbPrinterList.Items.Count > 0)
            //{
            //    cbPrinterList.SelectedIndex = 0;
            //}
        }
        private void PopulatePrinterComboBox()
        {
            //List<string> printerList = GetPrintersList();
            //cbPrinterList.ItemsSource = printerList;
        }

        private List<string> GetPrintersList()
        {
            List<string> printerNames = new List<string>();
            try
            {
                LocalPrintServer printServer = new LocalPrintServer();
                PrintQueue defaultPrintQueue = LocalPrintServer.GetDefaultPrintQueue();
                printerNames.Add(defaultPrintQueue.Name);
                selectedPrinter = defaultPrintQueue.Name;

                foreach (PrintQueue printer in printServer.GetPrintQueues())
                {
                    if (printer.Name != defaultPrintQueue.Name)
                    {
                        printerNames.Add(printer.Name);
                    }
                }

            }
            catch (Exception ex)
            {
                printerNames.Add($"Error: {ex.Message}");
            }
            return printerNames;
        }

        //private void PrinterComboBox_SelectionChanged(object sender, RoutedEventArgs e)
        //{
        //    selectedPrinter = cbPrinterList.SelectedItem as string;
        //}


        private void Btn_Print(object sender, RoutedEventArgs e)
        {
            GeneratePdf();
        }

        async Task GeneratePdf()
        {
            MyFontResolver.Apply();

            PdfSharp.Pdf.PdfDocument document = new PdfSharp.Pdf.PdfDocument();
            document.Info.Title = "Badge";

            PdfSharp.Pdf.PdfPage page = document.AddPage();
            page.Height = XUnit.FromCentimeter(13.1);
            page.Width = XUnit.FromCentimeter(18.2);

            var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(_mainViewModel.CurrentAttendee.TransactionId, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new QRCode(qrCodeData);
            var qrCodeBitmap = qrCode.GetGraphic(3);

            MemoryStream stream = new MemoryStream();
            qrCodeBitmap.Save(stream, ImageFormat.Png);
            stream.Position = 0;
            XImage qrCodeImage = XImage.FromStream(stream);

            string fullName = _mainViewModel.CurrentAttendee.FullName;
            string jobTitle = _mainViewModel.CurrentAttendee.JobTitle;
            string companyName = _mainViewModel.CurrentAttendee.CompanyName;

            XFont fullNameFont = new XFont("Arial", 14);
            XFont jobTitleFont = new XFont("Arial", 14, XFontStyleEx.Italic);
            XFont companyNameFont = new XFont("Arial", 14, XFontStyleEx.Bold);

            string[] completeDetails = [fullName, jobTitle, companyName];

            using (var gfx = XGraphics.FromPdfPage(page))
            {
                int jobTitleMarginTop = 0;
                int companyMarginTop = 0;

                List<LineModel> finalLines = new List<LineModel>();
                double totalContentHeight = jobTitleMarginTop + companyMarginTop;

                for (int i = 0; i < completeDetails.Length; i++)
                {
                    XFont innerFont;
                    string innerType;

                    if (i == 0)
                    {
                        innerFont = fullNameFont;
                        innerType = "name";
                    }
                    else if (i == 1)
                    {
                        innerFont = jobTitleFont;
                        innerType = "jobTitle";
                    }
                    else
                    {
                        innerFont = companyNameFont;
                        innerType = "companyName";
                    }

                    List<string> lines = GetLines(completeDetails[i], innerFont, 200, gfx);

                    for (int j = 0; j < lines.Count; j++)
                    {
                        if (j == 0)
                        {
                            finalLines.Add(new LineModel { line = lines[j], font = innerFont, type = innerType });
                        }
                        else
                        {

                            finalLines.Add(new LineModel { line = lines[j], font = innerFont, type = null });
                        }
                    }
                    totalContentHeight += lines.Count * innerFont.GetHeight();
                }

                //So bali dito cinacalculate natin yung content height para maicenter sa rectangle height, yung +13 manual lang yan
                XRect rect = new XRect(30, 130, 200, 110 + jobTitleMarginTop + companyMarginTop);
                //gfx.DrawRectangle(XPens.Black, rect); just for placeholder


                double y = (rect.Top + (rect.Height - totalContentHeight) / 2) + 13;

                foreach (LineModel line in finalLines)
                {
                    if (line.type == "companyName")
                    {
                        gfx.DrawString(line.line, line.font, XBrushes.Black, rect.Left, y + companyMarginTop);
                        y = y + line.font.GetHeight() + companyMarginTop;
                    }
                    else
                    {
                        gfx.DrawString(line.line, line.font, XBrushes.Black, rect.Left, y);
                        y += line.font.GetHeight();
                    }
                }

                gfx.DrawImage(qrCodeImage, new XPoint(50, 270));
            }

            const string filename = "HelloWorld3.pdf";
            await SavePdfDocumentAsync(document, filename);

            FileInfo f = new FileInfo(filename);
            string pdfFilePath = f.FullName;

            PrintPdf(pdfFilePath, selectedPrinter);
        }

        static async Task SavePdfDocumentAsync(PdfSharp.Pdf.PdfDocument document, string filePath)
        {
            await Task.Run(() =>
            {
                document.Save(filePath);
            });
        }

        void PrintPdf(string pdfFilePath, string printerName)
        {
            if (File.Exists(pdfFilePath))
            {
                try
                {
                    //PaperSize paperSize = new PaperSize("A5", (int)(5.83 * 100), (int)(8.27 * 100));
                    Patagames.Pdf.Net.PdfDocument pdfDocument = Patagames.Pdf.Net.PdfDocument.Load(pdfFilePath);
                    PdfPrintDocument pdfPrintDocument = new PdfPrintDocument(pdfDocument);
                    pdfPrintDocument.PrinterSettings.PrinterName = printerName;
                    //pdfPrintDocument.PrinterSettings.DefaultPageSettings.PaperSize = paperSize;
                    pdfPrintDocument.Print();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Printing failed: " + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("File not found: " + pdfFilePath);
            }
        }

        private List<string> GetLines(string text, XFont font, double maxWidth, XGraphics gfx)
        {
            List<string> lines = new List<string>();
            string[] words = text.Split(' ');

            StringBuilder currentLine = new StringBuilder();
            double currentLineWidth = 0;

            foreach (string word in words)
            {
                double wordWidth = gfx.MeasureString(word, font).Width;

                if (currentLineWidth + wordWidth <= maxWidth)
                {
                    currentLine.Append(word + " ");
                    currentLineWidth += wordWidth;
                }
                else
                {
                    lines.Add(currentLine.ToString().Trim());
                    currentLine.Clear();
                    currentLine.Append(word + " ");
                    currentLineWidth = wordWidth;
                }
            }

            if (currentLine.Length > 0)
            {
                lines.Add(currentLine.ToString().Trim());
            }

            return lines;
        }

        private void Btn_Cancel(object sender, RoutedEventArgs e)
        {
            _mainViewModel.ReturnBack();
        }
    }
}
