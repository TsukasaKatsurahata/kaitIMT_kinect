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

namespace ArrowSimulater
{
    public class Sprite
    {
        Image image;

        bool active;

        public Sprite() {
            image = new Image();
            image.Source = null;

            MainWindow.mainCanvas.Children.Add(image);

            active = false;
        }

        public void Remove() {
            image.Source = null;
            active = false;
        }

        public void Draw(int bi, Point position, Point center, float scale) {
            image.Source = SpriteManager.images[bi];
            image.Width = SpriteManager.imageSize[bi] * scale;
            image.Height = SpriteManager.imageSize[bi] * scale;
            center.X = center.X * scale; center.Y = center.Y * scale;
            image.Margin = new Thickness(position.X - center.X, position.Y - center.Y, 0, 0);
            active = true;
        }

        public void Draw(BitmapImage tex) {
            image.Source = tex;
            image.Width = 960;
            image.Height = 540;
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
    }
}
