/*
 * 神奈川工科大学　情報学部　情報メディア学科
 * 1123157　桂畑 司
 * 2013/12/24
 */
/*
 * 放つ矢のオブジェクトクラス
 * 運動の計算、衝突判定などで扱われる、また軌跡（別のクラス）を表示する
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArrowSimulater
{
    class Shoot
    {
        public float px, py;
        public float vx, vy;

        public bool active;

        private int point;   // 一回で複数の的にヒットした場合に加算する
        private int counter; // 複数回ヒットしたかを判断するための雑用変数

        public Shoot() {
            px = py = -10;
            vx = 0;
            vy = 0;

            active = false;

            point = 0;
            counter = -1;
        }

        public void setVelocity(float px, float py, float vx, float vy) {
            this.px = px;
            this.py = py;
            this.vx = vx;
            this.vy = vy;

            active = true;

            point = 0;
            counter = -1;
        }

        public void update() {
            if (!active) return;

            ShootLine.getInstance.setLine(px, py, px + vx, py + vy);
            px += vx;
            py += vy;

            vy += (float)(9.8 * 60 / 60 / 60);

            if (px < -50) {
                vx = 0;
                vy = 0;
                active = false;
                if (counter > 0) ScoreManager.getInstance.PointInc((int)(point * Math.Sqrt(counter)));
            }
            if (py > 540) {
                vx = 0;
                vy = 0;
                active = false;
                if (counter > 0) ScoreManager.getInstance.PointInc((int)(point * Math.Sqrt(counter)));
            }
            ShootLine.getInstance.setLine(px, py, px - vx, py - vy);
        }

        public void Hit(int input) {
            point += input;
            counter++;
        }
    }
}
