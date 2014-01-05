/*
 * 神奈川工科大学　情報学部　情報メディア学科
 * 1123157　桂畑 司
 * 2013/12/24
 */
/*
 * 弓の変形を計算するクラス
 * ベジエ曲線と直線のみで構成されており、弦の長さを一定に保つように変形する
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;


namespace ArrowSimulater
{
    class Bow
    {
        public static Bow getInstance;

        private Point toPoint, fromPoint; // 持っている場所の位置
        private Point upPoint, downPoint; // 弦の張る箇所
        private Point up1, up2, down1, down2; // 曲線の制御点
        private bool drawing; // 構えているか
        private double Length; // 引いている距離

        private Point rot_mat; // 現在の回転行列

        private static Point segment1 = new Point(-2, 10); // 曲線の制御点
        private static Point segment2 = new Point(1, 30); // 曲線の制御点
        private static Point segmentRoot = new Point(8, 40); // 曲線の制御点
        private static double LimitPoint = 61.0;  // 弦を引く時の変形の限界

        private SolidColorBrush BowColor = new SolidColorBrush(Color.FromRgb((byte)222, (byte)61, (byte)29));
        private SolidColorBrush StingColor = new SolidColorBrush(Color.FromArgb((byte)255, (byte)40, (byte)44, (byte)52));
        private SolidColorBrush ArrowColor = new SolidColorBrush(Color.FromArgb((byte)255, (byte)251, (byte)255, (byte)237));
        private SolidColorBrush IdleColor = new SolidColorBrush(Color.FromArgb((byte)0, (byte)255, (byte)255, (byte)255));

        Path upPath, downPath;
        Line upSting, downSting;
        Line Arrow;

        public Bow() { }

        public void Initialize() {
            getInstance = this;

            toPoint = new Point(400, 400);
            fromPoint = new Point(400 + segmentRoot.X, 400);

            rot_mat = new Point(1, 0);

            Length = segmentRoot.X;

            #region パス１
            PathFigure PF = new PathFigure();
            PF.StartPoint = toPoint;
            PF.Segments.Add(new BezierSegment(new Point(toPoint.X + segment1.X, toPoint.Y - segment1.Y), new Point(toPoint.X + segment2.X, toPoint.Y - segment2.Y), new Point(toPoint.X + segmentRoot.X, toPoint.Y - segmentRoot.Y), true));

            PathGeometry PG = new PathGeometry();
            PG.Figures.Add(PF);

            upPath = new Path();
            upPath.Stroke = BowColor;
            upPath.StrokeThickness = 4;
            upPath.Data = PG;
            #endregion
            #region パス２
            PF = new PathFigure();
            PF.StartPoint = toPoint;
            PF.Segments.Add(new BezierSegment(new Point(toPoint.X + segment1.X, toPoint.Y + segment1.Y), new Point(toPoint.X + segment2.X, toPoint.Y + segment2.Y), new Point(toPoint.X + segmentRoot.X, toPoint.Y + segmentRoot.Y), true));

            PG = new PathGeometry();
            PG.Figures.Add(PF);

            downPath = new Path();
            downPath.Stroke = BowColor;
            downPath.StrokeThickness = 4;
            downPath.Data = PG;
            #endregion

            upSting = new Line();
            upSting.Stroke = StingColor;
            upSting.StrokeThickness = 1;
            upSting.X1 = toPoint.X + segmentRoot.X;
            upSting.Y1 = toPoint.Y - segmentRoot.Y;
            upSting.X2 = fromPoint.X;
            upSting.Y2 = fromPoint.Y;

            downSting = new Line();
            downSting.Stroke = StingColor;
            downSting.StrokeThickness = 1;
            downSting.X1 = toPoint.X + segmentRoot.X;
            downSting.Y1 = toPoint.Y + segmentRoot.Y;
            downSting.X2 = fromPoint.X;
            downSting.Y2 = fromPoint.Y;

            Arrow = new Line();
            Arrow.Stroke = IdleColor;
            Arrow.StrokeThickness = 2;

            MainWindow.getInstance.MainCanvas.Children.Add(upSting);
            MainWindow.getInstance.MainCanvas.Children.Add(downSting);
            MainWindow.getInstance.MainCanvas.Children.Add(upPath);
            MainWindow.getInstance.MainCanvas.Children.Add(downPath);
            MainWindow.getInstance.MainCanvas.Children.Add(Arrow);

            //Drawing(new Point(0, 40), new Point(0, 41));
            unDraw();
        }

        public void Drawing(Point t, Point f) {
            if (t.X == f.X && t.Y == f.Y) return;
            toPoint = t; fromPoint = f;
            // 引いている距離を求める
            Length = MyMath.PointToLength(t, f);
            if (!(Length >= 0.1 && Length < 100)) Length = 0.1;
            // 回転行列を計算
            rot_mat = new Point((f.X - t.X) / Length, (t.Y - f.Y) / Length);

            // 変形計算のための雑用変数
            double L = Length - segmentRoot.X;
            if (L < 0) {
                L = 0;
                Length = segmentRoot.X;
                fromPoint.X = t.X + segmentRoot.X * rot_mat.X;
                fromPoint.Y = t.Y - segmentRoot.X * rot_mat.Y;
            }
            double D = -segmentRoot.Y / LimitPoint;

            // 変形する際の変位を計算
            double cal = -D*D * L*L -  2 * segmentRoot.Y * D * L + segmentRoot.Y * segmentRoot.Y * D*D;
            double dx, dy;
            if (cal <= 0.0)
            {
                cal = 0;
                dx = 0;
                dy = 0;
            }
            else
            {
                dx = (L - segmentRoot.Y * D - Math.Sqrt(cal)) / (D*D + 1);
                if (dx < 0) dx = 0;
                dy = D * dx;// +segmentRoot.Y;
            }
            #region 変形と座標変換の計算
#if false
            double px = segmentRoot.X + dx;
            double py = segmentRoot.Y + dy;

            double pudx, pudy, pddx, pddy;
            pudx = px * rot_mat.X - py * rot_mat.Y;
            pudy = -py * rot_mat.X - px * rot_mat.Y;
            pddx = px * rot_mat.X + py * rot_mat.Y;
            pddy = py * rot_mat.X - px * rot_mat.Y;

            double p1 = 0.05, p2 = 0.4;

            double sx1, sy1, sx2, sy2;
            sx1 = segment1.X + dx * p1;
            sy1 = segment1.Y + dy * p1;
            sx2 = segment2.X + dx * p2;
            sy2 = segment2.Y + dy * p2;

            double sudx1, sudy1, sudx2, sudy2;
            sudx1 = sx1 * rot_mat.X - sy1 * rot_mat.Y;
            sudy1 = -sy1 * rot_mat.X - sx1 * rot_mat.Y;
            sudx2 = sx2 * rot_mat.X - sy2 * rot_mat.Y;
            sudy2 = -sy2 * rot_mat.X - sx2 * rot_mat.Y;

            double sddx1, sddy1, sddx2, sddy2;
            sddx1 = sx1 * rot_mat.X + sy1 * rot_mat.Y;
            sddy1 = sy1 * rot_mat.X - sx1 * rot_mat.Y;
            sddx2 = sx2 * rot_mat.X + sy2 * rot_mat.Y;
            sddy2 = sy2 * rot_mat.X - sx2 * rot_mat.Y;

            up1.X = t.X + sudx1; up1.Y = t.Y + sudy1;
            up2.X = t.X + sudx2; up2.Y = t.Y + sudy2;
            upPoint.X = t.X + pudx;
            upPoint.Y = t.Y + pudy;

            down1.X = t.X + sddx1; down1.Y = t.Y + sddy1;
            down2.X = t.X + sddx2; down2.Y = t.Y + sddy2;
            downPoint.X = t.X + pddx;
            downPoint.Y = t.Y + pddy;
#else
            Rotation(t, dx, dy);
#endif
            #endregion

            #region パス１
            PathFigure PF = new PathFigure();
            PF.StartPoint = toPoint;
            PF.Segments.Add(new BezierSegment(up1, up2, upPoint, true));

            PathGeometry PG = new PathGeometry();
            PG.Figures.Add(PF);

//            upPath = new Path();
            upPath.Stroke = BowColor;
//            upPath.StrokeThickness = 4;
            upPath.Data = PG;
            #endregion
            #region パス２
            PF = new PathFigure();
            PF.StartPoint = toPoint;
            PF.Segments.Add(new BezierSegment(down1, down2, downPoint, true));

            PG = new PathGeometry();
            PG.Figures.Add(PF);

//            downPath = new Path();
            downPath.Stroke = BowColor;
//            downPath.StrokeThickness = 4;
            downPath.Data = PG;
            #endregion

//            upSting = new Line();
            upSting.Stroke = StingColor;
//            upSting.StrokeThickness = 1;
            upSting.X1 = upPoint.X;
            upSting.Y1 = upPoint.Y;
            upSting.X2 = fromPoint.X;
            upSting.Y2 = fromPoint.Y;

//            downSting = new Line();
            downSting.Stroke = StingColor;
//            downSting.StrokeThickness = 1;
            downSting.X1 = downPoint.X;
            downSting.Y1 = downPoint.Y;
            downSting.X2 = fromPoint.X;
            downSting.Y2 = fromPoint.Y;

            Arrow.Stroke = ArrowColor;
            Arrow.X1 = f.X;
            Arrow.Y1 = f.Y;
            Arrow.X2 = f.X - rot_mat.X * 60.0;
            Arrow.Y2 = f.Y + rot_mat.Y * 60.0;
        }

        public void Idling(Point t) {
            toPoint = t;
            // 引いている距離を求める
            //Length = MyMath.PointToLength(t, fromPoint);
            // 回転行列を計算
            //Point rot_mat = new Point((fromPoint.X - t.X) / Length, (fromPoint.Y - t.Y) / Length);

            // 変形計算のための雑用変数
            double L = Length - segmentRoot.X;
            double D = -segmentRoot.Y / LimitPoint;

            // 弦を戻す
            L *= 0.46;
            Length = L + segmentRoot.X;

            // 回転行列を元の状態へ
            if (L < 1.0) {
                rot_mat.X += (1.0 - rot_mat.X) / 12.0;
                rot_mat.Y += (0.0 - rot_mat.Y) / 12.0;
                double dL = MyMath.PointToLength(new Point(0, 0), rot_mat);
                rot_mat.X /= dL;
                rot_mat.Y /= dL;
            }

            // 弦の位置を計算する
            fromPoint.X = t.X + Length * rot_mat.X;
            fromPoint.Y = t.Y - Length * rot_mat.Y;

            // 変形する際の変位を計算
            double cal = -D*D * L*L -  2 * segmentRoot.Y * D * L + segmentRoot.Y * segmentRoot.Y * D*D;
            double dx, dy;
            if (cal <= 0.0)
            {
                cal = 0;
                dx = 0;
                dy = 0;
            }
            else
            {
                dx = (L - segmentRoot.Y * D - Math.Sqrt(cal)) / (D*D + 1);
                if (dx < 0) dx = 0;
                dy = D * dx;// +segmentRoot.Y;
            }
            #region 変形と座標変換の計算
#if false
            double px = segmentRoot.X + dx;
            double py = segmentRoot.Y + dy;

            double pudx, pudy, pddx, pddy;
            pudx = px * rot_mat.X - py * rot_mat.Y;
            pudy = -py * rot_mat.X + px * rot_mat.Y;
            pddx = px * rot_mat.X + py * rot_mat.Y;
            pddy = py * rot_mat.X + px * rot_mat.Y;

            double p1 = 0.05, p2 = 0.4;

            double sx1, sy1, sx2, sy2;
            sx1 = segment1.X + dx * p1;
            sy1 = segment1.Y + dy * p1;
            sx2 = segment2.X + dx * p2;
            sy2 = segment2.Y + dy * p2;

            double sudx1, sudy1, sudx2, sudy2;
            sudx1 = sx1 * rot_mat.X - sy1 * rot_mat.Y;
            sudy1 = -sy1 * rot_mat.X + sx1 * rot_mat.Y;
            sudx2 = sx2 * rot_mat.X - sy2 * rot_mat.Y;
            sudy2 = -sy2 * rot_mat.X + sx2 * rot_mat.Y;

            double sddx1, sddy1, sddx2, sddy2;
            sddx1 = sx1 * rot_mat.X + sy1 * rot_mat.Y;
            sddy1 = sy1 * rot_mat.X + sx1 * rot_mat.Y;
            sddx2 = sx2 * rot_mat.X + sy2 * rot_mat.Y;
            sddy2 = sy2 * rot_mat.X + sx2 * rot_mat.Y;

            up1.X = t.X + sudx1; up1.Y = t.Y + sudy1;
            up2.X = t.X + sudx2; up2.Y = t.Y + sudy2;
            upPoint.X = t.X + pudx;
            upPoint.Y = t.Y + pudy;

            down1.X = t.X + sddx1; down1.Y = t.Y + sddy1;
            down2.X = t.X + sddx2; down2.Y = t.Y + sddy2;
            downPoint.X = t.X + pddx;
            downPoint.Y = t.Y + pddy;
#else
            Rotation(t, dx, dy);
#endif
            #endregion

            #region パス１
            PathFigure PF = new PathFigure();
            PF.StartPoint = toPoint;
            PF.Segments.Add(new BezierSegment(up1, up2, upPoint, true));

            PathGeometry PG = new PathGeometry();
            PG.Figures.Add(PF);

//            upPath = new Path();
            upPath.Stroke = BowColor;
//            upPath.StrokeThickness = 4;
            upPath.Data = PG;
            #endregion
            #region パス２
            PF = new PathFigure();
            PF.StartPoint = toPoint;
            PF.Segments.Add(new BezierSegment(down1, down2, downPoint, true));

            PG = new PathGeometry();
            PG.Figures.Add(PF);

//            downPath = new Path();
            downPath.Stroke = BowColor;
//            downPath.StrokeThickness = 4;
            downPath.Data = PG;
            #endregion

//            upSting = new Line();
            upSting.Stroke = StingColor;
//            upSting.StrokeThickness = 1;
            upSting.X1 = upPoint.X;
            upSting.Y1 = upPoint.Y;
            upSting.X2 = fromPoint.X;
            upSting.Y2 = fromPoint.Y;

//            downSting = new Line();
            downSting.Stroke = StingColor;
//            downSting.StrokeThickness = 1;
            downSting.X1 = downPoint.X;
            downSting.Y1 = downPoint.Y;
            downSting.X2 = fromPoint.X;
            downSting.Y2 = fromPoint.Y;

            Arrow.Stroke = IdleColor;
        }

        private void Rotation(Point t, double dx, double dy) { 
            double px = segmentRoot.X + dx;
            double py = segmentRoot.Y + dy;

            double pudx, pudy, pddx, pddy;
            pudx = px * rot_mat.X - py * rot_mat.Y;
            pudy = -py * rot_mat.X - px * rot_mat.Y;
            pddx = px * rot_mat.X + py * rot_mat.Y;
            pddy = py * rot_mat.X - px * rot_mat.Y;

            double p1 = 0.05, p2 = 0.4;

            double sx1, sy1, sx2, sy2;
            sx1 = segment1.X + dx * p1;
            sy1 = segment1.Y + dy * p1;
            sx2 = segment2.X + dx * p2;
            sy2 = segment2.Y + dy * p2;

            double sudx1, sudy1, sudx2, sudy2;
            sudx1 = sx1 * rot_mat.X - sy1 * rot_mat.Y;
            sudy1 = -sy1 * rot_mat.X - sx1 * rot_mat.Y;
            sudx2 = sx2 * rot_mat.X - sy2 * rot_mat.Y;
            sudy2 = -sy2 * rot_mat.X - sx2 * rot_mat.Y;

            double sddx1, sddy1, sddx2, sddy2;
            sddx1 = sx1 * rot_mat.X + sy1 * rot_mat.Y;
            sddy1 = sy1 * rot_mat.X - sx1 * rot_mat.Y;
            sddx2 = sx2 * rot_mat.X + sy2 * rot_mat.Y;
            sddy2 = sy2 * rot_mat.X - sx2 * rot_mat.Y;

            up1.X = t.X + sudx1; up1.Y = t.Y + sudy1;
            up2.X = t.X + sudx2; up2.Y = t.Y + sudy2;
            upPoint.X = t.X + pudx;
            upPoint.Y = t.Y + pudy;

            down1.X = t.X + sddx1; down1.Y = t.Y + sddy1;
            down2.X = t.X + sddx2; down2.Y = t.Y + sddy2;
            downPoint.X = t.X + pddx;
            downPoint.Y = t.Y + pddy;
        }

        public void unDraw() {
            upPath.Stroke = IdleColor;
            downPath.Stroke = IdleColor;
            upSting.Stroke = IdleColor;
            downSting.Stroke = IdleColor;
            Arrow.Stroke = IdleColor;
        }
    }
}
