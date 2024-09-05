using GPCAEventsCheckIn.Helper;
using GPCAEventsCheckIn.Model;
using GPCAEventsCheckIn.ViewModel;
using PdfSharp.Drawing;
using QRCoder;
using System.Configuration;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace GPCAEventsCheckIn.View.BadgeDesign.PVC
{
    public class BadgePVCPDF
    {
        private MainViewModel _mainViewModel;

        public BadgePVCPDF(MainViewModel mainViewModel)
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
                page.Height = XUnit.FromInch(5.5);
                page.Width = XUnit.FromInch(3.5);
                page.TrimMargins.All = 0;

                var qrGenerator = new QRCodeGenerator();
                string combinedStringScan = ConfigurationManager.AppSettings["ScanningCode"] + ',' + ConfigurationManager.AppSettings["EventId"] + ',' + ConfigurationManager.AppSettings["EventCategory"] + ',' + _mainViewModel.CurrentAttendee.Id + ',' + _mainViewModel.CurrentAttendee.DelegateType;
                byte[] combinedBytes = Encoding.UTF8.GetBytes(combinedStringScan);
                string finalCryptStringScan = Convert.ToBase64String(combinedBytes);
                string scanDelegateUrl = "gpca" + finalCryptStringScan;
                var qrCodeData = qrGenerator.CreateQrCode(scanDelegateUrl, QRCodeGenerator.ECCLevel.Q);
                var qrCode = new QRCode(qrCodeData);
                var qrCodeBitmap = qrCode.GetGraphic(20);

                MemoryStream stream = new MemoryStream();
                qrCodeBitmap.Save(stream, ImageFormat.Png);
                stream.Position = 0;
                XImage qrCodeImage = XImage.FromStream(stream);

                string fullName = _mainViewModel.CurrentAttendee.FullName;
                string jobTitle = _mainViewModel.CurrentAttendee.JobTitle;
                string companyName = _mainViewModel.CurrentAttendee.CompanyName;
                string badgeType = _mainViewModel.CurrentAttendee.BadgeType;
                string accessType = _mainViewModel.CurrentAttendee.AccessType;

                XFont fullNameFont = new XFont("Arial", 22, XFontStyleEx.Bold);
                XFont jobTitleFont = new XFont("Arial", 14, XFontStyleEx.Italic);
                XFont companyNameFont = new XFont("Arial", 14, XFontStyleEx.Bold);
                XFont badgeTypeFont = new XFont("Arial", 12, XFontStyleEx.Bold);
                XFont accessTypeFont = new XFont("Arial", 11, XFontStyleEx.Italic);


                string[] completeDetails = [fullName, jobTitle, companyName];

                using (var gfx = XGraphics.FromPdfPage(page))
                {
                    int jobTitleMarginTop = 8;
                    int companyMarginTop = 2;

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

                    var yPos = 200;
                    var boxW = 252;
                    var boxH = 148;

                    XRect rectFront = new XRect(0, yPos, boxW, boxH + jobTitleMarginTop + companyMarginTop);
                    //gfx.DrawRectangle(XPens.Black, rectFront); //just for placeholder

                    //So bali dito cinacalculate natin yung content height para maicenter sa rectangle height, yung +13 manual lang yan
                    double yFront = (rectFront.Top + (rectFront.Height - totalContentHeight) / 2) + 13;
                    Color customColor = (Color)ColorConverter.ConvertFromString("#000000");
                    XColor xCustomColor = XColor.FromArgb(customColor.A, customColor.R, customColor.G, customColor.B);

                    foreach (LineModel line in finalLines)
                    {
                        //TEXT ALIGN CENTER
                        double lineWidth = gfx.MeasureString(line.line, line.font).Width;
                        double xFront = rectFront.Left + (rectFront.Width - lineWidth) / 2;


                        if (line.type == "companyName")
                        {
                            gfx.DrawString(line.line, line.font, new XSolidBrush(xCustomColor), xFront, yFront + companyMarginTop);
                            yFront = yFront + line.font.GetHeight() + companyMarginTop;
                        }
                        else if (line.type == "jobTitle")
                        {
                            gfx.DrawString(line.line, line.font, new XSolidBrush(xCustomColor), xFront, yFront + jobTitleMarginTop);
                            yFront = yFront + line.font.GetHeight() + jobTitleMarginTop;
                        }
                        else
                        {
                            gfx.DrawString(line.line, line.font, new XSolidBrush(xCustomColor), xFront, yFront);
                            yFront += line.font.GetHeight();
                        }
                    }

                    //double lineStartX = 0;
                    //double lineStartY = yPos + boxH + 20;
                    //double lineEndX = 252;
                    //double lineEndY = yPos + boxH + 20;
                    //gfx.DrawLine(XPens.Black, lineStartX, lineStartY, lineEndX, lineEndY);

                    //So bali yung +28 sa lineY, manual lang yan para maicenter vertically
                    double lineY = yPos + boxH + 28;
                    double badgeTypeWidth = gfx.MeasureString(badgeType, badgeTypeFont).Width;
                    double badgeTypeX = rectFront.Left + (rectFront.Width - badgeTypeWidth) / 2;
                    double badgeTypeY = lineY + 10;
                    gfx.DrawString(badgeType, badgeTypeFont, new XSolidBrush(xCustomColor), badgeTypeX, badgeTypeY);

                    //Para sa access type
                    double lineY2 = yPos + boxH + 20;
                    double accessTypeWidth = gfx.MeasureString(accessType, accessTypeFont).Width;
                    double accessTypeX = rectFront.Left + 15;
                    double accessTypeY = lineY2 + 10;
                    gfx.DrawString(accessType, accessTypeFont, new XSolidBrush(xCustomColor), accessTypeX, accessTypeY);

                    double targetWidth = 35; // Adjust as needed
                    double targetHeight = 35; // Adjust as needed
                    gfx.DrawImage(qrCodeImage, new XRect(205, badgeTypeY - 30, targetWidth, targetHeight));
                    //gfx.DrawImage(qrCodeImage, new XPoint(220, badgeTypeY-20));
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
