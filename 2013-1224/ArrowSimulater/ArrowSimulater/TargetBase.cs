/*
 * 神奈川工科大学　情報学部　情報メディア学科
 * 1123157　桂畑 司
 * 2013/12/24
 */
/*
 * 的のスーパークラス
 * 基本的に継承によって機能拡張を行う
 * 衝突判定、描画などの内部の処理が判らなくても利用できるようにする
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace ArrowSimulater
{
    class TargetBase
    {
        public    float px, py;
        public    float length;
        protected float scale;

        protected SpriteAnimation sAnim;
        public bool active;   // 衝突判定の際に処理するかどうか

        public bool AnimTypeDeleted { get { return sAnim.getType() != AnimationType.Delete; } }

        public virtual int point { get { return 1; } }     // 撃ち抜いた時のポイント

        public TargetBase() {
            Initialize();
        }

        public virtual void Initialize() {
            px = py = 0;
            length = 32;
            scale = 1.0f;

            sAnim = new SpriteAnimation(1);
            sAnim.addList(0, 1, 4, 0, AnimationType.Up);
            sAnim.init();

            active = false;
        }

        public virtual void update() {
            sAnim.update(4);
        }

        public virtual void setPosition(float x, float y) {
            px = x;
            py = y;
            active = true;
        }

        public virtual bool Collision(Shoot shoot) {
            if (!active) return false;
            float col = MyMath.Collision(new Point(shoot.px, shoot.py), new Point(px, py), new Point(shoot.vx, shoot.vy), length, new Point(4, 1));
            if (col <= 1.0f && col >= 0.0f) {
                shoot.Hit(point);
                Break();
                return true;
            }
            return false;
        }

        public virtual void Break() {
            ScoreManager.getInstance.PointInc(point);
            //length /= 2;
            //scale *= 0.5f;
            active = false;
        }

        public virtual void Remove() {
            active = false;
            sAnim.init();
        }

        public virtual void draw() {
            if (sAnim.getDrawable)
                SpriteManager.getInstance.DrawCenter(sAnim.index, new Point(px, py), scale);
        }
    }
}
