using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace PhotoManagement
{
    public static class FileHelper
    {
        public static async Task<IReadOnlyList<StorageFile>> PickImageFiles()
        {
            FileOpenPicker openPicker = new FileOpenPicker();

            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".jpeg");
            openPicker.FileTypeFilter.Add(".png");
            openPicker.FileTypeFilter.Add(".bmp");

            IReadOnlyList<StorageFile> file = await openPicker.PickMultipleFilesAsync();
            return file;
        }

        public static async Task<StorageFolder> PickFolder()
        {
            FolderPicker folderPicker = new FolderPicker();
            folderPicker.ViewMode = PickerViewMode.List;
            folderPicker.SuggestedStartLocation = PickerLocationId.VideosLibrary;
            folderPicker.FileTypeFilter.Add(".jpg");
            folderPicker.FileTypeFilter.Add(".jpeg");
            folderPicker.FileTypeFilter.Add(".png");
            folderPicker.FileTypeFilter.Add(".bmp");

            StorageFolder pickedFolder = await folderPicker.PickSingleFolderAsync();
            return pickedFolder;
        }

        public static bool IsImageFile(string filename)
        {
            return filename.ToLower().EndsWith(".jpg") || filename.ToLower().EndsWith(".jpeg") ||
                   filename.ToLower().EndsWith(".png") || filename.ToLower().EndsWith(".bmp");
        }
    }
}