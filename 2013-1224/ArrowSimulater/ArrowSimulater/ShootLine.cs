/*
 * 神奈川工科大学　情報学部　情報メディア学科
 * 1123157　桂畑 司
 * 2013/12/24
 */
/*
 * 矢の軌跡表示の管理をするクラス
 * 描画色を徐々に薄くしていく
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Shapes;
using System.Windows.Media;

namespace ArrowSimulater
{
    public class ShootLine
    {
        Line[] line;
        int Lmax = 200;
        float[] alpha;

        int index;

        public static ShootLine getInstance;

        public void Initialize() {
            getInstance = this;

            line = new Line[Lmax];
            alpha = new float[Lmax];
            for (int i = 0; i < Lmax; i++) {
                line[i] = new Line();
                line[i].HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                line[i].VerticalAlignment = System.Windows.VerticalAlignment.Top;
                line[i].StrokeThickness = 2;
                line[i].Stroke = Brushes.White;

                alpha[i] = 0.0f;

                MainWindow.mainCanvas.Children.Add(line[i]);
            }

            index = 0;
        }

        public void Remove() { 
            byte bm = (byte)255, mm = (byte)0;
            for (int i = 0; i < Lmax; i++) {
                alpha[i] = 0.0f;
                line[i].Stroke = new SolidColorBrush(Color.FromArgb(mm, bm, bm, bm));
            }
        }

        public void update() {
            // 雑用変数
            byte bm = (byte)255, mm = (byte)0;
            for (int i = 0; i < Lmax; i++) {
                if (alpha[i] > 0.0003f) {
                    alpha[i] *= 0.9f;
                    line[i].Stroke = new SolidColorBrush(Color.FromArgb((byte)(alpha[i] * 255), bm, bm, bm));
                }
                else {
                    alpha[i] = 0.0f;
                    line[i].Stroke = new SolidColorBrush(Color.FromArgb(mm, bm, bm, bm));
                }
            }
        }

        public void setLine(float px, float py, float tx, float ty) {
            alpha[index] = 1.0f;

            line[index].X1 = px;
            line[index].Y1 = py;
            line[index].X2 = tx;
            line[index].Y2 = ty;

            line[index].Stroke = Brushes.White;

            index = (index + 1) % Lmax;
        }
    }
}
