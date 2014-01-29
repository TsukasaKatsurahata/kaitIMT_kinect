using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MuscleShooting
{
    public class SpriteData
    {
        private BitmapImage image;

        private float width, height;

        private static string component = "/MuscleShooting;component/";

        public BitmapImage imageData { get { return image; } set { } }
        public float Width { get { return width; } set { } }
        public float Height { get { return height; } set { } }

        public static SpriteData Load(string name, float w, float h) {
            SpriteData sd = new SpriteData().Load(name, w, h, true);
            return sd;
        }

        public SpriteData Load(string name, float w, float h, bool b) { 
            image = new BitmapImage(new Uri(component + name, System.UriKind.Relative));
            width = w;
            height = h;
            return this;
        }

        public SpriteData() {
            imageData = null;
            width = 1;
            height = 1;
        }
    }
}
