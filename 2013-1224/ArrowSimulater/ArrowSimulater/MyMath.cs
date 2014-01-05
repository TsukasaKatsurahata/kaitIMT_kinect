/*
 * 神奈川工科大学　情報学部　情報メディア学科
 * 1123157　桂畑 司
 * 2013/12/24
 */
/*
 * 内積や距離、衝突判定などの雑用計算メソッドを格納するクラス
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Microsoft.Kinect;

namespace ArrowSimulater
{
    class MyMath
    {
        public static float Dot(JointType root, JointType tar, Skeleton user, Vector4[] file)
        {
            Vector4 vec1, vec2;
            vec1 = new Vector4();
            vec2 = new Vector4();

            vec1.X = user.Joints[root].Position.X - user.Joints[tar].Position.X;
            vec1.Y = user.Joints[root].Position.Y - user.Joints[tar].Position.Y;
            vec1.Z = user.Joints[root].Position.Z - user.Joints[tar].Position.Z;
            
            vec2.X = file[(int)root].X - file[(int)tar].X;
            vec2.Y = file[(int)root].Y - file[(int)tar].Y;
            vec2.Z = file[(int)root].Z - file[(int)tar].Z;

            float AA, BB, AB;

            AA = vec1.X * vec1.X + vec1.Y * vec1.Y + vec1.Z * vec1.Z;
            BB = vec2.X * vec2.X + vec2.Y * vec2.Y + vec2.Z * vec2.Z;
            AB = vec1.X * vec2.X + vec1.Y * vec2.Y + vec1.Z * vec2.Z;

            return (float)(AB / (System.Math.Sqrt(AA) * System.Math.Sqrt(BB)));
        }

        public static float Dot(Point Root, Point p1, Point p2) {
            double vx1, vy1, vx2, vy2;
            vx1 = p1.X - Root.X;
            vy1 = p1.Y - Root.Y;
            vx2 = p2.X - Root.X;
            vy2 = p2.Y - Root.Y;

            double t1 = vx1 * vx1 + vy1 * vy1;
            double t2 = vx2 * vx2 + vy2 * vy2;
            double tt = vx1 * vx2 + vy1 * vy2;

            return (float)(tt / (Math.Sqrt(t1) * Math.Sqrt(t2)));
        }

        public static float PointToLength(Point ff, Point tt) {
            ff.X -= tt.X;
            ff.Y -= tt.Y;
            return (float)Math.Sqrt(ff.X * ff.X + ff.Y * ff.Y);
        }

        public static float Collision(float fx, float fy, float tx, float ty, float vx, float vy, float L) {
            double tt1, tt2;
            double xx = (double)(tx - fx), yy = (double)(ty - fy), vxx = (double)vx, vyy = (double)vy;
	        double col = -vxx*vxx*yy*yy -vyy*vyy*xx*xx + 2*vxx*vyy*xx*yy + L*L*vxx*vxx + L*L*vyy*vyy;
            if (col >= 0) {
                tt1 = (-vxx * xx - vyy * yy - Math.Sqrt(col)) / (vxx * vxx + vyy * vyy);
                tt2 = (-vxx * xx - vyy * yy + Math.Sqrt(col)) / (vxx * vxx + vyy * vyy);
                if ((tt1 >= 0 && tt1 <= 1) && (tt2 >= 0 && tt2 <= 1))
                {
                    if (tt1 > tt2) tt1 = tt2;		// 求めたｔの小さい方の値を返す
                }
                if (tt1 >= 0.0 && tt1 <= 1.0) return (float)tt1;
                if (tt2 >= 0.0 && tt2 <= 1.0) return (float)tt2;
                if (tt1 * tt2 < 0.0) return 0.0f;
            }
            tt1 = 1.0d;

            return 1.0f;
        }
        public static float Collision(Point from, Point Target, Point Vector, float L) {
            return Collision((float)from.X, (float)from.Y, (float)Target.X, (float)Target.Y, (float)Vector.X, (float)Vector.Y, L);
        }
        public static float Collision(float fx, float fy, float tx, float ty, float vx, float vy, float L, double ex, double ey) {
            double tt1, tt2;
            double xx = ex * (tx - fx), yy = ey * (ty - fy), vxx = ex * vx, vyy = ey * vy;
	        double col = -vxx*vxx*yy*yy -vyy*vyy*xx*xx + 2*vxx*vyy*xx*yy + L*L*vxx*vxx + L*L*vyy*vyy;
            if (col >= 0) {
                tt1 = (-vxx * xx - vyy * yy - Math.Sqrt(col)) / (vxx * vxx + vyy * vyy);
                tt2 = (-vxx * xx - vyy * yy + Math.Sqrt(col)) / (vxx * vxx + vyy * vyy);
                if ((tt1 >= 0 && tt1 <= 1) && (tt2 >= 0 && tt2 <= 1))
                {
                    if (tt1 > tt2) tt1 = tt2;		// 求めたｔの小さい方の値を返す
                }
                if (tt1 >= 0.0 && tt1 <= 1.0) return (float)tt1;
                if (tt2 >= 0.0 && tt2 <= 1.0) return (float)tt2;
                if (tt1 * tt2 < 0.0) return 0.0f;
            }
            tt1 = 1.0d;

            return -1.0f;
        }
        public static float Collision(Point from, Point Target, Point Vector, float L, Point ellipse) {
            return Collision((float)from.X, (float)from.Y, (float)Target.X, (float)Target.Y, (float)Vector.X, (float)Vector.Y, L, ellipse.X, ellipse.Y);
        }
    }
}
