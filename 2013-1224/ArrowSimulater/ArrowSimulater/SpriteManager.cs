/*
 * 神奈川工科大学　情報学部　情報メディア学科
 * 1123157　桂畑 司
 * 2013/12/24
 */
/*
 * 画像の描画を一括して管理するクラス
 * 予め、mainWindowのキャンバスにImageを代入しておき、処理としては、ImageのSourceを入れ替えるだけのものとする
 * また、画像の読み込みも一括して行う
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace ArrowSimulater
{
    // 描画オブジェクトの制御を行う
    class SpriteManager
    {
        public static SpriteManager getInstance;

        public static BitmapImage[] images;  // 表示可能な全てのイメージをここで管理する

        public static int[] imageSize;      // 画像の大きさ

        private int SpriteMax = 50;

        private Sprite[] sprite;

        private int index;

        #region 雑用変数

        private Point Center = new Point(0, 0);

        private float Scale = 1.0f;

        #endregion

        public void Initialize() {
            getInstance = this;

            images = new BitmapImage[24];
            imageSize = new int[24];
            try {
                images[0] = new BitmapImage(new Uri("/ArrowSimulater;component/Resource/TargetSample.png", System.UriKind.Relative));
                imageSize[0] = 64;
                for (int i = 0; i < 4; i++) {
                    images[i + 1] = new BitmapImage(new Uri("/ArrowSimulater;component/Resource/fire" + i.ToString() + ".png", System.UriKind.Relative));
                    imageSize[i + 1] = 64; 
                }//5
                for (int i = 0; i < 7; i++) {
                    images[i + 5] = new BitmapImage(new Uri("/ArrowSimulater;component/Resource/Target/Sample/TargetSample" + i.ToString() + ".png", System.UriKind.Relative));
                    imageSize[i + 5] = 128; 
                }//12
                images[12] = new BitmapImage(new Uri("/ArrowSimulater;component/Resource/Score/Kakejiku.png", System.UriKind.Relative));
                imageSize[12] = 100;
                for (int i = 0; i < 10; i++) { 
                    images[i + 13] = new BitmapImage(new Uri("/ArrowSimulater;component/Resource/Score/ArrowSimulaterScore" + i.ToString() + ".png", System.UriKind.Relative));
                    imageSize[i + 13] = 64; 
                }//23
                images[23] = new BitmapImage(new Uri("/ArrowSimulater;component/Resource/Score/Kakejiku2.png", System.UriKind.Relative));
                imageSize[23] = 100;
            }
            catch {
                Console.WriteLine("false");
            }

            sprite = new Sprite[SpriteMax];
            for (int i = 0; i < SpriteMax; i++) sprite[i] = new Sprite();

            index = 0;
        }

        public void Clear() {
            index = 0;
            for (int i = 0; i < SpriteMax; i++)
                sprite[i].Remove();
        }

        public void Draw(int di, Point position)
        { 
            if (index >= SpriteMax) return;
            sprite[index].Draw(di, position, Center, Scale);
            index++;
        }
        public void Draw(int di, Point position, Point center) { 
            if (index >= SpriteMax) return;
            sprite[index].Draw(di, position, center, Scale);
            index++;
        }
        public void Draw(int di, Point position, Point center, float scale) { 
            if (index >= SpriteMax) return;
            sprite[index].Draw(di, position, center, scale);
            index++;
        }
        public void DrawCenter(int di, Point position, float scale) { 
            if (index >= SpriteMax) return;
            sprite[index].Draw(di, position, new Point(imageSize[di] / 2.0f, imageSize[di] / 2.0f), scale);
            index++;
        }
        public void Draw(BitmapImage bmi) { 
            if (index >= SpriteMax) return;
            sprite[index].Draw(bmi);
            index++;
        }
        public void Draw(BitmapImage bmi, Point pos, Point size) {
            if (index >= SpriteMax) return;
            sprite[index].Draw(bmi, pos, size);
            index++;
        }
    }
}
