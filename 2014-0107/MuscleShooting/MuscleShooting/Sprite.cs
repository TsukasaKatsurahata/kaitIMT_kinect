/*
 * 神奈川工科大学　情報学部　情報メディア学科
 * 1123157　桂畑 司
 * 2013/12/24
 */
/*
 * 描画のまねごとを行うクラス
 * 基本的にSpriteManagerによって管理される
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows;

namespace MuscleShooting
{
    public class Sprite
    {
        Image image;

        bool active;

        private static RotateTransform Identity = new RotateTransform(0, 0, 0);

        public Sprite() {
            image = new Image();
            image.Source = null;
            image.Stretch = Stretch.Fill;
            
            MainWindow.mainCanvas.Children.Add(image);

            active = false;
        }

        public void Remove() {
            image.Source = null;
            image.RenderTransform = Identity;
            active = false;
        }
        /*
        public void Draw(int bi, Point position, Point center, float scale) {
#if false
            image.Source = SpriteManager.images[bi];
            image.Width = SpriteManager.imageSize[bi] * scale;
            image.Height = SpriteManager.imageSize[bi] * scale;
#else
            image.Source = SpriteManager.spriteData[bi].imageData;
            image.Width = SpriteManager.spriteData[bi].Width;
            image.Height = SpriteManager.spriteData[bi].Height;
#endif
            center.X = center.X * scale; center.Y = center.Y * scale;
            image.Margin = new Thickness(position.X - center.X, position.Y - center.Y, 0, 0);
            active = true;
        }*/

        public void Draw(BitmapImage tex) {
            image.Source = tex;
            image.Width = 1024;
            image.Height = 768;
            image.Margin = new Thickness(0, 0, 0, 0);
            active = true;
        }

        public void Draw(BitmapImage tex, Point pos, Point sizeXY) { 
            image.Source = tex;
            image.Width = sizeXY.X;
            image.Height = sizeXY.Y;
            image.Margin = new Thickness(pos.X, pos.Y, 0, 0);
            active = true;
        }

        public void Draw(SpriteData tex, Point pos, Point center, float rot, float size) { 
            image.Source = tex.imageData;
            image.Width = tex.Width * size;
            image.Height = tex.Height * size;
            image.Margin = new Thickness(
                pos.X - center.X * size
                , pos.Y - center.Y * size
                , 0, 0);
            image.RenderTransform = new RotateTransform(rot / Math.PI * 180.0, center.X * size, center.Y * size);
            active = true;
        }
        public void Draw(SpriteData tex, Point pos, Point center, float size) { 
            image.Source = tex.imageData;
            image.Width = tex.Width * size;
            image.Height = tex.Height * size;
            image.Margin = new Thickness(
                pos.X - center.X * size
                , pos.Y - center.Y * size
                , 0, 0);
            image.RenderTransform = Identity;
            active = true;
        }
    }
}
