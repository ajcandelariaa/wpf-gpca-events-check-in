using GPCAEventsCheckIn.Helper;
using GPCAEventsCheckIn.Model;
using GPCAEventsCheckIn.View.Window;
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
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        private void Btn_Print(object sender, RoutedEventArgs e)
        {
            GeneratePdf();
        }

        async Task GeneratePdf()
        {
            MyFontResolver.Apply();

            PdfSharp.Pdf.PdfDocument document = new PdfSharp.Pdf.PdfDocument();
            document.Info.Title = _mainViewModel.CurrentAttendee.FullName + " Badge";

            PdfSharp.Pdf.PdfPage page = document.AddPage();
            page.Height = XUnit.FromCentimeter(12.2);
            page.Width = XUnit.FromCentimeter(18.2);

            //THIS FOR QR CODE
            //var qrGenerator = new QRCodeGenerator();
            //var qrCodeData = qrGenerator.CreateQrCode(_mainViewModel.CurrentAttendee.TransactionId, QRCodeGenerator.ECCLevel.Q);
            //var qrCode = new QRCode(qrCodeData);
            //var qrCodeBitmap = qrCode.GetGraphic(3);

            //MemoryStream stream = new MemoryStream();
            //qrCodeBitmap.Save(stream, ImageFormat.Png);
            //stream.Position = 0;
            //XImage qrCodeImage = XImage.FromStream(stream);

            string fullName = _mainViewModel.CurrentAttendee.FullName;
            string jobTitle = _mainViewModel.CurrentAttendee.JobTitle;
            string companyName = _mainViewModel.CurrentAttendee.CompanyName;

            XFont fullNameFont = new XFont("Arial", 14, XFontStyleEx.Bold);
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
                    else if (i == 2)
                    {
                        innerFont = companyNameFont;
                        innerType = "companyName";
                    } else
                    {
                        innerFont = fullNameFont;
                        innerType = "others";
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

                //yung 110 na value is height ng name details
                XRect rect = new XRect(10, 130, 200, 110 + jobTitleMarginTop + companyMarginTop);
                gfx.DrawRectangle(XPens.Black, rect); //just for placeholder

                //So bali dito cinacalculate natin yung content height para maicenter sa rectangle height, yung +13 manual lang yan
                double y = (rect.Top + (rect.Height - totalContentHeight) / 2) + 13;

                foreach (LineModel line in finalLines)
                {
                    //TEXT ALIGN LEFT
                    //if (line.type == "companyName")
                    //{
                    //    gfx.DrawString(line.line, line.font, XBrushes.Black, rect.Left, y + companyMarginTop);
                    //    y = y + line.font.GetHeight() + companyMarginTop;
                    //} else if (line.type == "jobTitle")
                    //{
                    //    gfx.DrawString(line.line, line.font, XBrushes.Black, rect.Left, y + jobTitleMarginTop);
                    //    y = y + line.font.GetHeight() + jobTitleMarginTop;
                    //}
                    //else
                    //{
                    //    gfx.DrawString(line.line, line.font, XBrushes.Black, rect.Left, y);
                    //    y += line.font.GetHeight();
                    //}

                    //TEXT ALIGN CENTER
                    // Calculate X-coordinate for center alignment
                    double lineWidth = gfx.MeasureString(line.line, line.font).Width;
                    double x = rect.Left + (rect.Width - lineWidth) / 2;

                    // Draw the line
                    if (line.type == "companyName")
                    {
                        gfx.DrawString(line.line, line.font, XBrushes.Black, x, y + companyMarginTop);
                        y = y + line.font.GetHeight() + companyMarginTop;
                    } else if (line.type == "jobTitle")
                    {
                        gfx.DrawString(line.line, line.font, XBrushes.Black, x, y + jobTitleMarginTop);
                        y = y + line.font.GetHeight() + jobTitleMarginTop;
                    }
                    else
                    {
                        gfx.DrawString(line.line, line.font, XBrushes.Black, x, y);
                        y += line.font.GetHeight();
                    }
                }

                //FOR QR CODE 
                //gfx.DrawImage(qrCodeImage, new XPoint(50, 270));
            }

            string filename = _mainViewModel.CurrentAttendee.TransactionId + ".pdf";
            await SavePdfDocumentAsync(document, filename);

            FileInfo f = new FileInfo(filename);
            string pdfFilePath = f.FullName;

            PrintPdf(pdfFilePath, selectedPrinter);
        }

        static async Task SavePdfDocumentAsync(PdfSharp.Pdf.PdfDocument document, string filePath)
        {
            await Task.Run(() =>
            {
                if(File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
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
