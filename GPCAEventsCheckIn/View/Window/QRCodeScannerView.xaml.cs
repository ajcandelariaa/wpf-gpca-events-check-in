using AForge.Video;
using AForge.Video.DirectShow;
using GPCAEventsCheckIn.ViewModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
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
                            for (int i = 0; i < _mainViewModel.AttendeeViewModel.ConfirmedAttendees.Count; i++)
                            {
                                if (result.Text == _mainViewModel.AttendeeViewModel.ConfirmedAttendees[i].TransactionId)
                                {
                                    _mainViewModel.CurrentAttendee = _mainViewModel.AttendeeViewModel.ConfirmedAttendees[i];
                                    checker++;
                                    break;
                                }
                            }
                            if (checker > 0)
                            {
                                _mainViewModel.NavigateToAttendeeDetailsView("HomeView");
                                cameraStopped = true;
                                Application.Current.Windows.OfType<QRCodeScannerView>().FirstOrDefault()?.Close();
                                _mainViewModel.BackDropStatus = "Collapsed";
                            }
                            else
                            {
                                MessageBox.Show("Invalid QR Code, Please try again!");

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
