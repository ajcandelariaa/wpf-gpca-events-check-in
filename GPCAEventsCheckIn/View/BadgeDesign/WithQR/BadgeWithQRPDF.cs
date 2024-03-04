using GPCAEventsCheckIn.Helper;
using GPCAEventsCheckIn.Model;
using GPCAEventsCheckIn.ViewModel;
using Microsoft.Extensions.Logging;
using PdfSharp.Drawing;
using QRCoder;
using System;
using System.Configuration;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Windows;

namespace GPCAEventsCheckIn.View.BadgeDesign.WithQR
{
    internal class BadgeWithQRPDF
    {
        private MainViewModel _mainViewModel;

        public BadgeWithQRPDF(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
        }

        public PdfSharp.Pdf.PdfDocument GeneratePdf()
        {
            PdfSharp.Pdf.PdfDocument document = new PdfSharp.Pdf.PdfDocument();
            MyFontResolver.Apply();
            LinesHelper lineHelper = new LinesHelper();
            try
            {
                document.Info.Title = _mainViewModel.CurrentAttendee.FullName + " Badge";

                PdfSharp.Pdf.PdfPage page = document.AddPage();
                page.Height = XUnit.FromMillimeter(148);
                page.Width = XUnit.FromMillimeter(210);
                page.TrimMargins.All = 0;

                var originalHeight = XUnit.FromMillimeter(123);
                var originalWidth = XUnit.FromMillimeter(178);

                var qrGenerator = new QRCodeGenerator();
                string combinedStringScan = ConfigurationManager.AppSettings["ScanningCode"] + ',' + ConfigurationManager.AppSettings["EventId"] + ',' + ConfigurationManager.AppSettings["EventCategory"] + ',' + _mainViewModel.CurrentAttendee.Id + ',' + _mainViewModel.CurrentAttendee.DelegateType;
                byte[] combinedBytes = Encoding.UTF8.GetBytes(combinedStringScan);
                string finalCryptStringScan = Convert.ToBase64String(combinedBytes);
                string scanDelegateUrl = "gpca" + finalCryptStringScan;
                var qrCodeData = qrGenerator.CreateQrCode(scanDelegateUrl, QRCodeGenerator.ECCLevel.Q);
                var qrCode = new QRCode(qrCodeData);
                var qrCodeBitmap = qrCode.GetGraphic(4);

                MemoryStream stream = new MemoryStream();
                qrCodeBitmap.Save(stream, ImageFormat.Png);
                stream.Position = 0;
                XImage qrCodeImage = XImage.FromStream(stream);

                string fullName = _mainViewModel.CurrentAttendee.FullName;
                string jobTitle = _mainViewModel.CurrentAttendee.JobTitle;
                string companyName = _mainViewModel.CurrentAttendee.CompanyName;

                XFont fullNameFont = new XFont("Arial", 20, XFontStyleEx.Bold);
                XFont jobTitleFont = new XFont("Arial", 16, XFontStyleEx.Italic);
                XFont companyNameFont = new XFont("Arial", 16, XFontStyleEx.Italic);

                string[] completeDetails = [fullName, jobTitle, companyName];

                using (var gfx = XGraphics.FromPdfPage(page))
                {
                    int jobTitleMarginTop = 10;
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

                        List<string> lines = lineHelper.GetLines(completeDetails[i], innerFont, 200, gfx);

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

                    gfx.DrawRectangle(XPens.Black, rectFront); //just for placeholder
                    gfx.DrawRectangle(XPens.Black, rectBack); //just for placeholder

                    double yFront = (rectFront.Top + (rectFront.Height - totalContentHeight) / 2) + 13;
                    double yBack = (rectBack.Top + (rectBack.Height - totalContentHeight) / 2) + 13;

                    foreach (LineModel line in finalLines)
                    {
                        double lineWidth = gfx.MeasureString(line.line, line.font).Width;
                        double xFront = rectFront.Left + (rectFront.Width - lineWidth) / 2;

                        if (line.type == "companyName")
                        {
                            gfx.DrawString(line.line, line.font, XBrushes.Black, xFront, yFront + companyMarginTop);
                            yFront = yFront + line.font.GetHeight() + companyMarginTop;
                        }
                        else if (line.type == "jobTitle")
                        {
                            gfx.DrawString(line.line, line.font, XBrushes.Black, xFront, yFront + jobTitleMarginTop);
                            yFront = yFront + line.font.GetHeight() + jobTitleMarginTop;
                        }
                        else
                        {
                            gfx.DrawString(line.line, line.font, XBrushes.Black, xFront, yFront);
                            yFront += line.font.GetHeight();
                        }
                    }

                    //Manual na lang yung +62 at +15
                    gfx.DrawImage(qrCodeImage, new XPoint(rectBack.Left + 62, rectBack.Top + 15));
                    gfx.Dispose();
                }
                return document;
            }
            catch (Exception ex)
            {
                _mainViewModel.BackDropStatus = "Collapsed";
                _mainViewModel.LoadingProgressStatus = "Collapsed";
                MessageBox.Show("Error generating PDF: " + ex.Message);
                return null;
            }
        }
    }
}
