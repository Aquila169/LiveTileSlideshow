using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Windows.ApplicationModel.Background;
using Windows.Data.Xml.Dom;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using PhotoManagement;

namespace BackgroundTasks
{
    public sealed class SlideShowBackgroundTask : IBackgroundTask
    {
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            //To prevent this task from closing while async code is running.
            BackgroundTaskDeferral deferral = taskInstance.GetDeferral();

            Random rand = new Random(DateTime.Now.Minute * DateTime.Now.Second);
            await PhotoManager.LoadFromDisk(false);
            List<SlideshowImage> randomImages = new List<SlideshowImage>();
            for (int i = 0; i < 40; i++)
            {
                SlideshowImage image = PhotoManager.SlideshowImages[rand.Next(PhotoManager.SlideshowImages.Count)];
                //   await image.GetBitmapImage();
                randomImages.Add(image);
            }
            await RemoveOldImages();
            //await CopyToAppdata(randomImages);
            int c = 0;
            foreach (SlideshowImage img in randomImages)
                BitmapTransformTest(img, c++);

            UpdateFeed(randomImages);
            deferral.Complete();
        }

        private void UpdateFeed(IEnumerable<SlideshowImage> images)
        {
            // Create a tile update manager
            TileUpdater updater = TileUpdateManager.CreateTileUpdaterForApplication();
            updater.EnableNotificationQueue(true);
            updater.Clear();

            // Keep track of the number feed items that get tile notifications. 
            int itemCount = 0;

            // Create a tile notification for each feed item.
            foreach (SlideshowImage item in images)
            {
                //XmlDocument xml = new XmlDocument();
                //xml.LoadXml(GetXml(itemCount));
                XmlDocument tileXml = TileUpdateManager.GetTemplateContent(TileTemplateType.TileSquare310x310Image);
                XmlNodeList tileImageAttributes = tileXml.GetElementsByTagName("image");
                tileImageAttributes[0].Attributes[1].NodeValue = "ms-appdata:///local/" + itemCount + ".jpg";
                // Create a new tile notification. 
                updater.Update(new TileNotification(tileXml));

                // Don't create more than 5 notifications.
                if (itemCount++ > 40) break;
            }
        }

        private async Task RemoveOldImages()
        {
            foreach (
                StorageFile file in
                    (await ApplicationData.Current.LocalFolder.GetFilesAsync()).Where(x => x.Name != "photodb.txt"))
            {
                await file.DeleteAsync(StorageDeleteOption.PermanentDelete);
            }
        }

        async private void BitmapTransformTest(SlideshowImage slideshowImage, int i)
        {
            StorageFile file = await slideshowImage.GetStorageFile();            

            // create a stream from the file and decode the image
            var fileStream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read);
            BitmapDecoder decoder = await BitmapDecoder.CreateAsync(fileStream);
            
            // create a new stream and encoder for the new image
            IRandomAccessStream ras = await
                (await ApplicationData.Current.LocalFolder.CreateFileAsync(i + ".jpg")).OpenAsync(
                    FileAccessMode.ReadWrite);
            BitmapEncoder enc = await BitmapEncoder.CreateForTranscodingAsync(ras, decoder);

            // convert the entire bitmap to a 100px by 100px bitmap
            uint height =// 310;
            (uint)
                (decoder.PixelHeight > decoder.PixelWidth
                    ? (310.0f / (float)decoder.PixelWidth) * decoder.PixelHeight
                    : 310);
            uint width =// 310;
            (uint)
                (decoder.PixelWidth > decoder.PixelHeight
                    ? (310.0f / (float)decoder.PixelHeight) * decoder.PixelWidth
                    : 310);

            enc.BitmapTransform.ScaledHeight =  height;
            enc.BitmapTransform.ScaledWidth = width;

            BitmapBounds bounds = new BitmapBounds();
            bounds.Height = height;
            bounds.Width = width;
            bounds.X = 0;
            bounds.Y = 0;
            enc.BitmapTransform.Bounds = bounds;

            // write out to the stream
            try
            {
                await enc.FlushAsync();
            }
            catch (Exception ex)
            {
                string s = ex.ToString();
            }
        }

    }
}
