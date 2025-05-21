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
using System.Windows.Media.Imaging;

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

                //FOR LOGO
                //Uri imageUri = new Uri("pack://application:,,,/GPCAEventsCheckIn;component/Assets/Images/Badges/Sponsor/sabic.png", UriKind.Absolute);
                //BitmapImage bitmap = new BitmapImage(imageUri);
                //XImage badgeSponsorLogo;
                //using (MemoryStream ms = new MemoryStream())
                //{
                //    BitmapEncoder encoder = new PngBitmapEncoder();
                //    encoder.Frames.Add(BitmapFrame.Create(bitmap));
                //    encoder.Save(ms);
                //    ms.Seek(0, SeekOrigin.Begin);
                //    badgeSponsorLogo = XImage.FromStream(ms);
                //}

                string fullName = _mainViewModel.CurrentAttendee.FullName;
                string jobTitle = _mainViewModel.CurrentAttendee.JobTitle;
                string companyName = _mainViewModel.CurrentAttendee.CompanyName;
                string badgeType = _mainViewModel.CurrentAttendee.BadgeType;
                string accessType = _mainViewModel.CurrentAttendee.AccessType;
                //string accessType = "";

                //XFont fullNameFont = new XFont("Arial", 22, XFontStyleEx.Bold); //ANC & PC
                //XFont jobTitleFont = new XFont("Arial", 14, XFontStyleEx.Italic); //ANC & PC
                //XFont companyNameFont = new XFont("Arial", 14, XFontStyleEx.Bold); //ANC & PC
                //XFont fullNameFont = new XFont("Arial", 21, XFontStyleEx.Bold); //PSC 
                //XFont jobTitleFont = new XFont("Arial", 13, XFontStyleEx.Italic); //PSC
                //XFont companyNameFont = new XFont("Arial", 13, XFontStyleEx.Bold); //PSC
                XFont fullNameFont = new XFont("Arial", 18, XFontStyleEx.Bold); //SCC
                XFont jobTitleFont = new XFont("Arial", 11, XFontStyleEx.Italic); //SCC
                XFont companyNameFont = new XFont("Arial", 11, XFontStyleEx.Bold); //SCC
                XFont badgeTypeFont = new XFont("Arial", 11, XFontStyleEx.Bold);
                //XFont sponsorTextFont = new XFont("Arial", 7, XFontStyleEx.Bold);
                XFont accessTypeFont = new XFont("Arial", 9, XFontStyleEx.Italic);


                string[] completeDetails = [fullName, jobTitle, companyName];

                using (var gfx = XGraphics.FromPdfPage(page))
                {
                    //int jobTitleMarginTop = 8; // ANC & PC
                    int jobTitleMarginTop = 0; // PSC & SCC
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

                    //FOR SCC EVENT
                    var yPos = 230;
                    var boxW = 252;
                    var boxH = 120;

                    ////FOR PC EVENT
                    //var yPos = 217;
                    //var boxW = 252;
                    //var boxH = 130;

                    //FOR ANC EVENT
                    //var yPos = 200;
                    //var boxW = 252;
                    //var boxH = 148;

                    //FOR PSC EVENT
                    //var yPos = 250;
                    //var boxW = 252;
                    //var boxH = 100;


                    //Color customColor2 = (Color)ColorConverter.ConvertFromString("#0c6853");
                    //XColor xCustomColor2 = XColor.FromArgb(customColor2.A, customColor2.R, customColor2.G, customColor2.B);
                    //double sponsorTextX = 109;
                    //double sponsorTextY = yPos - 17;
                    //gfx.DrawString("SPONSORED BY", sponsorTextFont, new XSolidBrush(xCustomColor2), sponsorTextX, sponsorTextY);

                    //double targetWidth2 = 55; // Adjust as needed
                    //double targetHeight2 = 30; // Adjust as needed
                    //gfx.DrawImage(badgeSponsorLogo, new XRect(180, yPos - 46, targetWidth2, targetHeight2));

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
                       
                    double lineY = yPos + boxH + 28;
                    double badgeTypeWidth = gfx.MeasureString(badgeType, badgeTypeFont).Width;
                    double badgeTypeX = rectFront.Left + (rectFront.Width - badgeTypeWidth) / 2;
                    //double badgeTypeY = lineY + 10;  //PC
                    double badgeTypeY = lineY - 10;  //SCC
                    gfx.DrawString(badgeType, badgeTypeFont, new XSolidBrush(xCustomColor), badgeTypeX, badgeTypeY + 5); // SCC
                    //gfx.DrawString(badgeType, badgeTypeFont, new XSolidBrush(xCustomColor), badgeTypeX, badgeTypeY);

                    //Para sa access type
                    //double lineY2 = yPos + boxH + 20; //PC
                    double lineY2 = yPos + boxH; //SCC
                    double accessTypeWidth = gfx.MeasureString(accessType, accessTypeFont).Width;
                    double accessTypeX = 190; //SCC
                    //double accessTypeX = rectFront.Left + 15; //PC
                    double accessTypeY = lineY2 + 42; // SCC
                    //double accessTypeY = lineY2 + 18; // PSC
                    //double accessTypeY = lineY2 + 10; // ANC
                    gfx.DrawString(accessType, accessTypeFont, new XSolidBrush(xCustomColor), accessTypeX, accessTypeY);

                    //Para sa QR Code
                    double targetWidth = 35; // Adjust as needed
                    double targetHeight = 35; // Adjust as needed
                    badgeTypeY -= 5; //SCC
                    //gfx.DrawImage(qrCodeImage, new XPoint(220, badgeTypeY-20));
                    //gfx.DrawImage(qrCodeImage, new XRect(220, badgeTypeY - 30, targetWidth, targetHeight)); //PSC
                    //gfx.DrawImage(qrCodeImage, new XRect(205, badgeTypeY - 30, targetWidth, targetHeight)); //ANC
                    //gfx.DrawImage(qrCodeImage, new XRect(215, badgeTypeY - 27, targetWidth, targetHeight)); //PC
                    gfx.DrawImage(qrCodeImage, new XRect(215, badgeTypeY, targetWidth, targetHeight)); //SCC
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
