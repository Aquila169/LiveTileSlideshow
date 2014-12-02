using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using PhotoManagement;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace LiveTileSlideshow
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            this.Loaded += (o,args) => Initialize();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.RegisterBackgroundTask();
        }

        private async void RegisterBackgroundTask()
        {
            var backgroundAccessStatus = await BackgroundExecutionManager.RequestAccessAsync();
            if (backgroundAccessStatus == BackgroundAccessStatus.AllowedMayUseActiveRealTimeConnectivity ||
                backgroundAccessStatus == BackgroundAccessStatus.AllowedWithAlwaysOnRealTimeConnectivity)
            {
                foreach (var task in BackgroundTaskRegistration.AllTasks)
                {
                    if (task.Value.Name == "SlideShowBackgroundTask")
                    {
                        task.Value.Unregister(true);
                    }
                }

                BackgroundTaskBuilder taskBuilder = new BackgroundTaskBuilder();
                taskBuilder.Name = "SlideShowBackgroundTask";
                taskBuilder.TaskEntryPoint = "BackgroundTasks.SlideShowBackgroundTask";
                taskBuilder.SetTrigger(new TimeTrigger(15, false));
                var registration = taskBuilder.Register();
            }
        }

        public async void AddImageHandler(object sender, RoutedEventArgs args)
        {
            IReadOnlyList<StorageFile> imageFile = await FileHelper.PickImageFiles();
            if(imageFile != null)
                PhotoManager.AddImages(imageFile);
        }

        public async void AddFolderHandler(object sender, RoutedEventArgs args)
        {
            StorageFolder folder = await FileHelper.PickFolder();
            if(folder != null)
                PhotoManager.AddFolder(folder);
        }

        public async Task Initialize()
        {
            SlideshowImages.Source = PhotoManager.SlideshowImages;
            await PhotoManager.LoadFromDisk(true);
        }

        private void SlideshowImagesGridView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Appbar.IsOpen = SlideshowImagesGridView.SelectedItems.Count != 0;
            DeleteItemButton.Visibility = SlideshowImagesGridView.SelectedItems.Count != 0 ? Visibility.Visible : Visibility.Collapsed;
            Appbar.IsSticky = true;
        }

        private void RemoveButton_OnClick(object sender, RoutedEventArgs e)
        {
            IEnumerable<SlideshowImage> selectedItems = SlideshowImagesGridView.SelectedItems.Cast<SlideshowImage>();
            PhotoManager.RemoveImages(selectedItems);
        }
    }
}
