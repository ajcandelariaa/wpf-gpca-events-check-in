using AForge.Video;
using AForge.Video.DirectShow;
using GPCAEventsCheckIn.ViewModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;
using ZXing;

namespace GPCAEventsCheckIn.View.Window
{
    public partial class QRCodeScannerView
    {
        private MainViewModel _mainViewModel;
        private FilterInfoCollection videoDevices;
        private VideoCaptureDevice videoSource;
        private bool cameraStopped = false;

        public QRCodeScannerView(MainViewModel mainViewModel)
        {
            InitializeComponent();
            InitializeListOfCameras();
            _mainViewModel = mainViewModel;
        }

        private void InitializeListOfCameras()
        {
            try
            {
                videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);

                if (videoDevices.Count == 0)
                {
                    MessageBox.Show("No video devices found.");
                    return;
                }

                foreach (FilterInfo device in videoDevices)
                {
                    cbVideoCaptureDeviceList.Items.Add(device.Name);
                }

                cbVideoCaptureDeviceList.SelectedIndex = 0;
                InitializeCamera(0);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing cameras: {ex.Message}");
                Application.Current.Windows.OfType<QRCodeScannerView>().FirstOrDefault()?.Close();
                _mainViewModel.BackDropStatus = "Collapsed";
            }
        }

        private void InitializeCamera(int cameraIndex)
        {
            if (videoSource != null && videoSource.IsRunning)
            {
                StopCamera();
            }

            try
            {
                videoSource = new VideoCaptureDevice(videoDevices[cameraIndex].MonikerString);
                videoSource.NewFrame += VideoSource_NewFrame;
                videoSource.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error InitializeListOfCameras: {ex.Message}");
                Application.Current.Windows.OfType<QRCodeScannerView>().FirstOrDefault()?.Close();
                _mainViewModel.BackDropStatus = "Collapsed";
            }
        }

        private void cbVideoCaptureDeviceList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            InitializeCamera(cbVideoCaptureDeviceList.SelectedIndex);
        }


        private void StopCamera()
        {
            videoSource.SignalToStop();
            videoSource.WaitForStop();
        }


        private void VideoSource_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            try
            {
                BarcodeReader barcodeReader = new BarcodeReader();
                using (Bitmap bitmap = (Bitmap)eventArgs.Frame.Clone())
                {
                    Dispatcher.Invoke(() =>
                    {
                        videoSourcePlayer.Source = ConvertBitmapToBitmapImage(bitmap);

                        var result = barcodeReader.Decode(bitmap);
                        if (result != null)
                        {
                            int checker = 0;
                            string qrCodeResult = result.Text;

                            if (qrCodeResult != null && qrCodeResult.Length >= 4)
                            {
                                string firstTwoContent = qrCodeResult.Substring(0, 2);
                                string lastTwoContent = qrCodeResult.Substring(qrCodeResult.Length - 2);
                                string combinedContent = lastTwoContent + firstTwoContent;

                                if (combinedContent != "gpca")
                                {
                                    MessageBox.Show("Invalid QR Code. Please try again!");
                                }
                                else
                                {
                                    string encrypTextTextContent = qrCodeResult.Substring(2, qrCodeResult.Length - 4);

                                    try
                                    {
                                        string decryptedText = DecodeBase64String(encrypTextTextContent);
                                        string[] arrayDecryptedText = decryptedText.Split(',');

                                        if (arrayDecryptedText.Length >= 5 && arrayDecryptedText[0] == "gpca@reg")
                                        {
                                            string delegateId = arrayDecryptedText[3];
                                            string delegateType = arrayDecryptedText[4];

                                            bool attendeeFound = _mainViewModel.AttendeeViewModel.ConfirmedAttendees
                                                .Any(attendee =>
                                                    delegateId == attendee.Id.ToString() &&
                                                    delegateType == attendee.DelegateType
                                                );

                                            if (attendeeFound)
                                            {
                                                _mainViewModel.CurrentAttendee = _mainViewModel.AttendeeViewModel.ConfirmedAttendees
                                                    .First(attendee =>
                                                        delegateId == attendee.Id.ToString() &&
                                                        delegateType == attendee.DelegateType
                                                    );

                                                checker++;

                                                _mainViewModel.NavigateToAttendeeDetailsView("HomeView");
                                                cameraStopped = true;
                                                Application.Current.Windows.OfType<QRCodeScannerView>().FirstOrDefault()?.Close();
                                                _mainViewModel.BackDropStatus = "Collapsed";
                                            }
                                            else
                                            {
                                                MessageBox.Show("Attendee not found. Please try again!");
                                            }
                                        }
                                        else
                                        {
                                            MessageBox.Show("Invalid QR Code. Please try again!");
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        MessageBox.Show("Invalid QR Code. Please try again!");
                                    }
                                }
                            }
                            else
                            {
                                MessageBox.Show("Invalid QR Code. Please try again!");
                            }
                        }
                    });
                }

                if (cameraStopped)
                {
                    StopCamera();
                    cameraStopped = false;
                }
            }
            catch (Exception ex)
            {
                StopCamera();
                MessageBox.Show($"Error VideoSource_NewFrame: {ex.Message}");
                Application.Current.Windows.OfType<QRCodeScannerView>().FirstOrDefault()?.Close();
                _mainViewModel.BackDropStatus = "Collapsed";
            }
        }


        private BitmapImage ConvertBitmapToBitmapImage(Bitmap bitmap)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                bitmap.Save(memoryStream, ImageFormat.Bmp);
                memoryStream.Position = 0;

                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memoryStream;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();

                return bitmapImage;
            }
        }

        private static string DecodeBase64String(string base64EncodedString)
        {
            try
            {
                byte[] bytes = Convert.FromBase64String(base64EncodedString);
                return Encoding.UTF8.GetString(bytes);
            }
            catch (Exception ex)
            {
                MessageBox.Show("QR Invalid" +ex);
                return null;
            }
        }

        private void Btn_Cancel(object sender, RoutedEventArgs e)
        {
            if (videoSource != null && videoSource.IsRunning)
            {
                StopCamera();
            }
            Application.Current.Windows.OfType<QRCodeScannerView>().FirstOrDefault()?.Close();
            _mainViewModel.BackDropStatus = "Collapsed";
        }
    }
}
