using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace PhotoManagement
{
    public class SlideshowImage : INotifyPropertyChanged
    {
        public string FileName { get; private set; }
        public string Token { get; private set; }

        public TokenType TokenType { get; private set; }

        public BitmapImage BitmapImage { get { return bitmapImage; } private set { bitmapImage = value; OnPropertyChanged(); } }
        private BitmapImage bitmapImage;

        public SlideshowImage(string token, TokenType type, string filename)
        {
            Token = token;
            TokenType = type;
            FileName = filename;
        }

        public static SlideshowImage Create(StorageFile imageFile)
        {
            return new SlideshowImage(StorageApplicationPermissions.FutureAccessList.Add(imageFile), TokenType.File, imageFile.Name);   
        }

        public static SlideshowImage Create(StorageFolder imageFolder, string fileName)
        {
            return new SlideshowImage(StorageApplicationPermissions.FutureAccessList.Add(imageFolder), TokenType.Folder, fileName);
        }

        public async Task<StorageFile> GetStorageFile()
        {
            StorageFile bitmapFile;
            if (TokenType == TokenType.Folder)
            {
                bitmapFile = await (await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(Token)).GetFileAsync(FileName);
            }
            else
            {
                bitmapFile = await StorageApplicationPermissions.FutureAccessList.GetFileAsync(Token);
            }
            return bitmapFile;
        }

        public async Task<BitmapImage> GetBitmapImage()
        {
            StorageFile bitmapFile = await GetStorageFile();
            FileRandomAccessStream fileStream = (FileRandomAccessStream)await bitmapFile.OpenAsync(FileAccessMode.Read);
            BitmapImage image = new BitmapImage
            {
                DecodePixelHeight = 350
            };
            await image.SetSourceAsync(fileStream);
            BitmapImage = image;
            return image;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public enum TokenType
    {
        Folder,
        File
    }
}
