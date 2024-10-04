using GPCAEventsCheckIn.Helper;
using GPCAEventsCheckIn.Model;
using GPCAEventsCheckIn.ViewModel;
using PdfSharp.Drawing;
using System.Windows;

namespace GPCAEventsCheckIn.View.BadgeDesign.Common
{
    public  class BadgeCommonPDF
    {
        private MainViewModel _mainViewModel;

        public BadgeCommonPDF(MainViewModel mainViewModel) {
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

                XFont fullNameFont = new XFont("Arial", 18, XFontStyleEx.Bold);
                XFont jobTitleFont = new XFont("Arial", 14, XFontStyleEx.Italic);
                XFont companyNameFont = new XFont("Arial", 14, XFontStyleEx.Bold);


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

                    //CAIPW1 2024
                    //var yPos = 242; //kung gaano kataas yung container nung details
                    //var boxW = 255; //width container
                    //var boxH = 143; //height container

                    //IPAW 2024
                    var yPos = 183; //kung gaano kataas yung container nung details
                    var boxW = 255; //width container
                    var boxH = 120; //height container

                    //IPAW 2024
                    //var yPos = 259; //kung gaano kataas yung container nung details
                    //var boxW = 255; //width container
                    //var boxH = 120; //height container

                    //GLF 2024
                    //var yPos = 170;
                    //var boxW = 255;
                    //var boxH = 148;

                    XRect rectFront = new XRect(40, yPos, boxW, boxH + jobTitleMarginTop + companyMarginTop);

                    //default
                    //XRect rectFront = new XRect(0, yPos, boxW, boxH + jobTitleMarginTop + companyMarginTop);
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


                            //gfx.DrawString(line.line, line.font, XBrushes.Black, xBack, yBack + companyMarginTop);
                            //yBack = yBack + line.font.GetHeight() + companyMarginTop;
                        }
                        else if (line.type == "jobTitle")
                        {
                            gfx.DrawString(line.line, line.font, XBrushes.Black, xFront, yFront + jobTitleMarginTop);
                            yFront = yFront + line.font.GetHeight() + jobTitleMarginTop;

                            //gfx.DrawString(line.line, line.font, XBrushes.Black, xBack, yBack + jobTitleMarginTop);
                            //yBack = yBack + line.font.GetHeight() + jobTitleMarginTop;
                        }
                        else
                        {
                            gfx.DrawString(line.line, line.font, XBrushes.Black, xFront, yFront);
                            yFront += line.font.GetHeight();

                            //gfx.DrawString(line.line, line.font, XBrushes.Black, xBack, yBack);
                            //yBack += line.font.GetHeight();
                        }
                    }
                    gfx.Dispose();
                    //FOR QR CODE 
                    //gfx.DrawImage(qrCodeImage, new XPoint(50, 270));
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
