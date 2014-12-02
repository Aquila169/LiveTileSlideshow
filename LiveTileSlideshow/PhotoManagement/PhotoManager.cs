using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace PhotoManagement
{
    public static class PhotoManager
    {
        private static bool Loaded = false;
        public static ObservableCollection<SlideshowImage> SlideshowImages = new ObservableCollection<SlideshowImage>();

        public async static Task LoadFromDisk(bool loadImagesInMemory)
        {
            if (!Loaded)
            {
                //Load from disk
                StorageFolder folder = ApplicationData.Current.LocalFolder;
                if (!((await folder.GetFilesAsync()).Count(x => x.Name == "photodb.txt") > 0))
                    await folder.CreateFileAsync("photodb.txt");
                StorageFile database = await folder.GetFileAsync("photodb.txt");
                List<string> lines = (await FileIO.ReadLinesAsync(database)).ToList();

                //Create SlideshowImages
                foreach (string line in lines)
                {
                    string[] split = line.Split('|');
                    var image = new SlideshowImage(split[0], (TokenType)int.Parse(split[1]), split[2]);
                    if(loadImagesInMemory)
                        image.GetBitmapImage();
                    SlideshowImages.Add(image);
                }
            }
            Loaded = true;
        }

        private async static Task FlushToDisk()
        {
            //Flush to disk
            StringBuilder db = new StringBuilder();
            foreach (SlideshowImage slideshowImage in SlideshowImages)
            {
                string serialized = slideshowImage.Token + "|" + (int)slideshowImage.TokenType + "|" +
                                    slideshowImage.FileName + "\n";
                db.Append(serialized);
            }
            StorageFile dbFile = await ApplicationData.Current.LocalFolder.GetFileAsync("photodb.txt");
            await FileIO.WriteTextAsync(dbFile, db.ToString());
        }

        public async static void AddImage(StorageFile imageFile)
        {
            if (!Loaded)
                await LoadFromDisk(true);

            var iamge = SlideshowImage.Create(imageFile);
            iamge.GetBitmapImage();
            SlideshowImages.Add(iamge);
            await FlushToDisk();
        }

        public async static void AddImages(IEnumerable<StorageFile> files)
        {
            if (!Loaded)
                await LoadFromDisk(true);

            foreach (StorageFile file in files.Where(x => FileHelper.IsImageFile(x.Name)))
            {
                var iamge = SlideshowImage.Create(file);
                iamge.GetBitmapImage();
                SlideshowImages.Add(iamge);
            }
            await FlushToDisk();
        }

        public async static void AddFolder(StorageFolder folder)
        {
            if (!Loaded)
                await LoadFromDisk(true);

            AddImages(await folder.GetFilesAsync());
        }

        public async static void RemoveImages(IEnumerable<SlideshowImage> slideshowImages)
        {
            if (!Loaded)
                await LoadFromDisk(true);

            foreach (var x in slideshowImages.ToList())
            {
                SlideshowImages.Remove(x);
            }
            await FlushToDisk();
        }
    }
}
