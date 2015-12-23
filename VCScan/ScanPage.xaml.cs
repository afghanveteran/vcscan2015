using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Devices.Sensors;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
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
using WindowsPreview.Media.Ocr;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace VCScan
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ScanPage : Page
    {
        OcrEngine ocrEngine;
        UInt32 width;
        UInt32 height;

        string recognizedText;

        public ScanPage()
        {
            this.InitializeComponent();

            ocrEngine = new OcrEngine(OcrLanguage.English);
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            var file = await ApplicationData.Current.LocalFolder.GetFileAsync("photo.png");
            using (var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read))
            {
                
                var decoder = await BitmapDecoder.CreateAsync(stream);

               
                width = decoder.PixelWidth;
                height = decoder.PixelHeight;

                
                var pixels = await decoder.GetPixelDataAsync(
                    BitmapPixelFormat.Bgra8,
                    BitmapAlphaMode.Straight,
                    new BitmapTransform(),
                    ExifOrientationMode.RespectExifOrientation,
                    ColorManagementMode.ColorManageToSRgb);

               
                OcrResult result = await ocrEngine.RecognizeAsync(height, width, pixels.DetachPixelData());

                recognizedText = "";
              
                if (result.Lines != null)
                {
                    foreach (var line in result.Lines)
                    {
                        foreach (var word in line.Words)
                        {
                            recognizedText += word.Text + " ";
                            OcrText.Text += word.Text + " ";
                        }
                        OcrText.Text += Environment.NewLine;
                    }
                }
                txtEmail.Text += " " + scanEmails(recognizedText);
                txtPhone.Text += " " + scanPhones(recognizedText);
            }
        }

        private string scanEmails(string text)
        {
            const string MatchEmailPattern =
               @"(([\w-]+\.)+[\w-]+|([a-zA-Z]{1}|[\w-]{2,}))@"
             + @"((([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\."
             + @"([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])){1}|"
             + @"([a-zA-Z]+[\w-]+\.)+[a-zA-Z]{2,4})";
            Regex rx = new Regex(MatchEmailPattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

            MatchCollection emailMatches = rx.Matches(text);

            StringBuilder sb = new StringBuilder();
            foreach (Match emailMatch in emailMatches)
                sb.AppendLine(emailMatch.Value + " ");
            return sb.ToString();
        }

        private string scanPhones(string text)
        {
            const string MatchPhonePattern = @"((8|\+7)-?)?\(?\d{3}\)?-?\d{1}-?\d{1}-?\d{1}-?\d{1}-?\d{1}-?\d{1}-?\d{1}";
            Regex rx = new Regex(MatchPhonePattern);

            MatchCollection phonesMatches = rx.Matches(text);

            StringBuilder sb = new StringBuilder();
            foreach (Match phoneMatch in phonesMatches)
                sb.AppendLine(phoneMatch.Value + " ");
            return sb.ToString();
        }
    }
}
