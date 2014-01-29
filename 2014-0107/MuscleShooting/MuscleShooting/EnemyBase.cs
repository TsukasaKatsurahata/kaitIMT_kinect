using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace MuscleShooting
{
    class EnemyBase : ObjectBase
    {
        public override ObjectTag tag { get { return ObjectTag.ENEMY; } }

        public override float length { get { return 20; } }

        public override int HP_MAX { get { return 6; } }

        public override bool TagCheck(ObjectTag tags) {
            return !(tags == ObjectTag.ENEMY || tags == ObjectTag.ENEMYS_SHOOT || tags == ObjectTag.PLAYER);
        }

        private float rot;

        public override void Initialize() {
            base.Initialize();

            sAnim = new SpriteAnimation(2);
            sAnim.addList(0, 33, 33, 0, AnimationType.Delete);
            sAnim.addList(1, 33, 36, 1, AnimationType.Up);
            sAnim.init();

            rot = 0.0f;
        }

        public override void setPosition(float ix, float iy) {
            base.setPosition(ix, iy);
            sAnim.nextSet(1);
        }

        public override void update() {
            if (!sAnim.getDrawable) return;
            sAnim.update(5);
            px += vx;
            py += vy;

            Point vec = new Point();
            vec.X = Player.getInstance.px - px;
            vec.Y = Player.getInstance.py - py;
            double dL = vec.X * vec.X + vec.Y * vec.Y;
            dL = Math.Sqrt(dL);
            vec.X /= dL;
            vec.Y /= dL;
            vx = (float)vec.X * 2.0f;
            vy = (float)vec.Y * 2.0f;
            rot = (float)Math.Atan2(vx, -vy);
        }

        protected override bool CollsionEnter(float dt, ObjectTag tags) {
            if (dt >= 0.0f && dt <= 1.0f) {
                hp--;
                if (hp <= 0) {
                    Break();
                    EffectManager.getInstance.setEffect(px, py, 0.7f);
                }
                return true;
            }
            return false;
        }
        public override void Break() {
            base.Break();
            EffectManager.getInstance.setEffect(px, py, 1.0f);
        }

        public override void draw() { 
            if (sAnim.getDrawable)
                SpriteManager.getInstance.Draw(sAnim.index, new Point(px, py), new Point(32, 32), rot, 1.0f);
        }
    }
}
