using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoManagement
{
    class PhotoSet
    {
        public ObservableCollection<SlideshowImage> SlideshowImages { get; set; }
        public string Name { get; set; }
        public bool Active { get; private set; }

        public PhotoSet(IEnumerable<SlideshowImage> slideshowImages, string name)
        {
            Name = name;
            SlideshowImages = new ObservableCollection<SlideshowImage>(slideshowImages);
        }

        public void Activate()
        {
            Active = true;
        }
    }
}
