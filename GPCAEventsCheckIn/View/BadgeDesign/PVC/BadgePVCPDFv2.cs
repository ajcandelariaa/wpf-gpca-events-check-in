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
    public class BadgePVCPDFv2
    {
        private MainViewModel _mainViewModel;

        public BadgePVCPDFv2(MainViewModel mainViewModel)
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
                Uri imageUri = new Uri("pack://application:,,,/GPCAEventsCheckIn;component/Assets/Images/Badges/Sponsor/acwa.png", UriKind.Absolute);
                BitmapImage bitmap = new BitmapImage(imageUri);
                XImage badgeSponsorLogo;
                using (MemoryStream ms = new MemoryStream())
                {
                    BitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(bitmap));
                    encoder.Save(ms);
                    ms.Seek(0, SeekOrigin.Begin);
                    badgeSponsorLogo = XImage.FromStream(ms);
                }

                string fullName = _mainViewModel.CurrentAttendee.FullName;
                string jobTitle = _mainViewModel.CurrentAttendee.JobTitle;
                string companyName = _mainViewModel.CurrentAttendee.CompanyName;
                string badgeType = _mainViewModel.CurrentAttendee.BadgeType;
                string seatNumber = _mainViewModel.CurrentAttendee.SeatNumber;

                if (seatNumber == "N/A")
                {
                    seatNumber = "";
                }

                XFont fullNameFont = new XFont("Arial", 22, XFontStyleEx.Bold);
                XFont jobTitleFont = new XFont("Arial", 16, XFontStyleEx.Italic);
                XFont companyNameFont = new XFont("Arial", 16, XFontStyleEx.Bold);
                XFont badgeTypeFont = new XFont("Arial", 17, XFontStyleEx.Bold);
                XFont seatNumberFont = new XFont("Arial", 15, XFontStyleEx.Bold);

                string[] completeDetails = [fullName, jobTitle, companyName];

                using (var gfx = XGraphics.FromPdfPage(page))
                {
                    int jobTitleMarginTop = 5;
                    int companyMarginTop = 15;

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

                        List<string> lines = lineHelper.GetLines(completeDetails[i], innerFont, 180, gfx);

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

                    var yPos = 160;
                    var xPos = 40;
                    var boxW = 210;
                    var boxH = 175;

                    XRect rectFront = new XRect(xPos, yPos, boxW, boxH + jobTitleMarginTop + companyMarginTop);
                    //gfx.DrawRectangle(XPens.Black, rectFront); //just for placeholder

                    //So bali dito cinacalculate natin yung content height para maicenter sa rectangle height, yung +15 manual lang yan
                    double yFront = (rectFront.Top + (rectFront.Height - totalContentHeight) / 2) + 15;
                    //double yFront = rectFront.Top + 15;
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

                    // Set up the vertical line properties
                    double lineX = xPos;  // X position of the vertical line, adjust as needed
                    double lineStartY = yPos - 20;  // Starting Y position of the vertical line
                    double lineEndY = yPos + boxH + 20;  // Ending Y position of the vertical line

                    // Draw the vertical line separating badge type from the details
                    gfx.DrawLine(XPens.Black, lineX, lineStartY, lineX, lineEndY);

                    gfx.Save();  // Save the current graphics state

                    double badgeTypeX = lineX - 10;  // X position of the badge type (closer to the left edge)
                    double badgeTypeY = (yPos - 10) + (boxH + 20) / 2;  // Y position at the middle of the box

                    // Set the transformation to rotate the text
                    gfx.RotateAtTransform(-90, new XPoint(badgeTypeX, badgeTypeY));

                    // Measure the width of the rotated text (for proper centering)
                    double rotatedBadgeTypeWidth = gfx.MeasureString(badgeType, badgeTypeFont).Width;

                    // Draw the rotated badge type text
                    gfx.DrawString(badgeType, badgeTypeFont, new XSolidBrush(xCustomColor), badgeTypeX - rotatedBadgeTypeWidth / 2, badgeTypeY);

                    // Restore the original graphics state
                    gfx.Restore();

                    // Add QR code and other details (unchanged)
                    double targetWidth = 35; // Adjust as needed
                    double targetHeight = 35; // Adjust as needed
                    gfx.DrawImage(qrCodeImage, new XRect(8, yPos + boxH + 23, targetWidth, targetHeight));

                    //ADD CUSTOM LOGO FOR ALL DELEGATES EXCEPT YOUTH FORUM & YOUTH COUNCIL
                    if (badgeType != "YOUTH FORUM" && badgeType != "YOUTH COUNCIL")
                    {
                        double targetWidth2 = 100; // Adjust as needed
                        double targetHeight2 = 32; // Adjust as needed
                        gfx.DrawImage(badgeSponsorLogo, new XRect(148, yPos + boxH + 25, targetWidth2, targetHeight2));
                    }

                    //Para sa seat number
                    double lineY2 = yPos + boxH + 20;
                    double accessTypeWidth = gfx.MeasureString(seatNumber, seatNumberFont).Width;
                    double accessTypeX = rectFront.Left + 5;
                    double accessTypeY = lineY2 + 25;
                    gfx.DrawString(seatNumber, seatNumberFont, new XSolidBrush(xCustomColor), accessTypeX, accessTypeY);

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
