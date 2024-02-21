﻿using GPCAEventsCheckIn.Helper;
using GPCAEventsCheckIn.Model;
using GPCAEventsCheckIn.View.Window;
using GPCAEventsCheckIn.ViewModel;
using Patagames.Pdf.Net;
using Patagames.Pdf.Net.Controls.Wpf;
using PdfSharp.Drawing;
using QRCoder;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.IO;
using System.Printing;
using System.Reflection.Metadata;
using System.Text;
using System.Windows;
using System.Xml.Linq;

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
                // Handle print queue related exceptions (e.g., printer offline, out of paper)
                MessageBox.Show($"PrintQueueException: {pqe.Message}");
            }
            catch (PrintSystemException pse)
            {
                // Handle print system related exceptions
                MessageBox.Show($"PrintSystemException: {pse.Message}");
            }
            catch (Exception ex)
            {
                // Catch other general exceptions
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
            PdfSharp.Pdf.PdfDocument document = new PdfSharp.Pdf.PdfDocument();
            MyFontResolver.Apply();
            try
            {
                    document.Info.Title = _mainViewModel.CurrentAttendee.FullName + " Badge";
                    
                    PdfSharp.Pdf.PdfPage page = document.AddPage();
                    page.Height = XUnit.FromMillimeter(148);
                    page.Width = XUnit.FromMillimeter(210);
                    page.TrimMargins.All = 0;

                    var originalHeight = XUnit.FromMillimeter(123);
                    var originalWidth = XUnit.FromMillimeter(178);

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
                            }
                            else
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

                        var yPos = 170;
                        var boxW = 255;
                        var boxH = 148;

                        XRect rectFront = new XRect(0, yPos, boxW, boxH + jobTitleMarginTop + companyMarginTop);
                        XRect rectBack = new XRect(boxW + 10, yPos, boxW, boxH + jobTitleMarginTop + companyMarginTop);
                        //gfx.DrawRectangle(XPens.Black, rectFront); //just for placeholder
                        //gfx.DrawRectangle(XPens.Black, rectBack); //just for placeholder

                        //So bali dito cinacalculate natin yung content height para maicenter sa rectangle height, yung +13 manual lang yan
                        double yFront = (rectFront.Top + (rectFront.Height - totalContentHeight) / 2) + 13;
                        double yBack = (rectBack.Top + (rectBack.Height - totalContentHeight) / 2) + 13;

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
                            double xFront = rectFront.Left + (rectFront.Width - lineWidth) / 2;
                            double xBack = rectBack.Left + (rectBack.Width - lineWidth) / 2;

                        // Draw the line
                        if (line.type == "companyName")
                            {
                                gfx.DrawString(line.line, line.font, XBrushes.Black, xFront, yFront + companyMarginTop);
                                yFront = yFront + line.font.GetHeight() + companyMarginTop;

                            
                                gfx.DrawString(line.line, line.font, XBrushes.Black, xBack, yBack + companyMarginTop);
                                yBack = yBack + line.font.GetHeight() + companyMarginTop;
                            }
                            else if (line.type == "jobTitle")
                            {
                                gfx.DrawString(line.line, line.font, XBrushes.Black, xFront, yFront + jobTitleMarginTop);
                                yFront = yFront + line.font.GetHeight() + jobTitleMarginTop;

                                gfx.DrawString(line.line, line.font, XBrushes.Black, xBack, yBack + jobTitleMarginTop);
                                yBack = yBack + line.font.GetHeight() + jobTitleMarginTop;
                            }
                            else
                            {
                                gfx.DrawString(line.line, line.font, XBrushes.Black, xFront, yFront);
                                yFront += line.font.GetHeight();

                                gfx.DrawString(line.line, line.font, XBrushes.Black, xBack, yBack);
                                yBack += line.font.GetHeight();
                            }
                        }
                        gfx.Dispose();
                        //FOR QR CODE 
                        //gfx.DrawImage(qrCodeImage, new XPoint(50, 270));
                    }
            }
            catch (Exception ex)
            {
                _mainViewModel.BackDropStatus = "Collapsed";
                _mainViewModel.LoadingProgressStatus = "Collapsed";
                MessageBox.Show("Error generating PDF: " + ex.Message);
                return;
            }

            string filename = _mainViewModel.CurrentAttendee.TransactionId + ".pdf";
            SavePdfDocument(document, filename);
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
                    pdfPrintDocument.DefaultPageSettings.PaperSize = new PaperSize("A5",
                        Convert.ToInt32(210 * 25.4),
                        Convert.ToInt32(148 * 25.4));
                    pdfPrintDocument.PrinterSettings.PrinterName = printerName;
                    pdfPrintDocument.Print();

                    _mainViewModel.BackDropStatus = "Collapsed";
                    _mainViewModel.LoadingProgressStatus = "Collapsed";

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
