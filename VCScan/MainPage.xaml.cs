using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Sensors;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Phone.UI.Input;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=391641

namespace VCScan
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        MediaCapture takePhotoManager;

        public MainPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;

            HardwareButtons.BackPressed += HardwareButtons_BackPressed;
        }

        private void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e)
        {
            if (Frame.CanGoBack)
            {
                e.Handled = true;
                Frame.GoBack();
            }
            else
                Application.Current.Exit();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            InitCamera();
        }

        private static async Task<DeviceInformation> GetCameraID(Windows.Devices.Enumeration.Panel desired)
        {
            return (await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture))
                .FirstOrDefault(x => x.EnclosureLocation != null && x.EnclosureLocation.Panel == desired);
        }

        async private void InitCamera()
        {
            var cameraID = await GetCameraID(Windows.Devices.Enumeration.Panel.Back);
            takePhotoManager = new MediaCapture();
            await takePhotoManager.InitializeAsync(new MediaCaptureInitializationSettings
            {
                StreamingCaptureMode = StreamingCaptureMode.Video,
                PhotoCaptureSource = PhotoCaptureSource.Photo,
                AudioDeviceId = string.Empty,
                VideoDeviceId = cameraID.Id
            });

            photoPreview.Source = takePhotoManager;
         
            await takePhotoManager.StartPreviewAsync();
        }

        private async void btnPhoto_Click(object sender, RoutedEventArgs e)
        {
            btnPhoto.IsEnabled = false;

            ImageEncodingProperties imgFormat = ImageEncodingProperties.CreatePng();
            imgFormat.Width = 2572;
            imgFormat.Height = 1228;

            StorageFile file = await ApplicationData.Current.LocalFolder.CreateFileAsync(
                    "photo.png", CreationCollisionOption.ReplaceExisting);

            await takePhotoManager.CapturePhotoToStorageFileAsync(imgFormat, file);

            BitmapImage bmpImage = new BitmapImage(new Uri(file.Path));
            myImage.Source = bmpImage;


            btnScan.Visibility = Visibility.Visible;
            btnDelete.Visibility = Visibility.Visible;

            myImage.Visibility = Visibility.Visible;
        }

        private async void photoPreview_Tapped(object sender, TappedRoutedEventArgs e)
        {
            takePhotoManager.VideoDeviceController.FocusControl.Configure(new Windows.Media.Devices.FocusSettings { Mode = Windows.Media.Devices.FocusMode.Auto });
            await takePhotoManager.VideoDeviceController.FocusControl.FocusAsync();
        }

        private async void btnScan_Click(object sender, RoutedEventArgs e)
        {
            await takePhotoManager.StopPreviewAsync();
            Frame.Navigate(typeof(ScanPage));
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            myImage.Source = null;
            myImage.Visibility = Visibility.Collapsed;

            btnDelete.Visibility = Visibility.Collapsed;
            btnScan.Visibility = Visibility.Collapsed;

            btnPhoto.IsEnabled = true;
        }
    }
}
