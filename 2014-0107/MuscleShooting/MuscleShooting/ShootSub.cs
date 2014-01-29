using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace MuscleShooting
{
    public class ShootNormal : ObjectBase
    {
        public override ObjectTag tag { get { return ObjectTag.PLAYERS_SHOOT; } }
        public override float length { get { return 28; } }
        public override float scale { get { return 0.7f; } }
        public override int HP_MAX { get { return 5; } }

        public override bool TagCheck(ObjectTag tags) {
            return (tags == ObjectTag.ENEMY);
        }
        public override void Initialize() {
            base.Initialize();

            sAnim = new SpriteAnimation(2);
            sAnim.addList(0, 37, 37, 0, AnimationType.Delete);
            sAnim.addList(1, 37, 37, 1, AnimationType.Stay);
            sAnim.init();
        }
        public override void setPosition(float ix, float iy) {
            base.setPosition(ix, iy);
            sAnim.nextSet(1);
        }
        public override void update() {
            px += vx;
            py += vy;

            if (px > 1200 || px < -200 || py < -100 || py > 960) {
                Remove();
            }
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
        public override void draw() {
            if (sAnim.getDrawable)
                SpriteManager.getInstance.Draw(sAnim.index, new Point(px, py), new Point(32, 32), 0.0f, scale);
        }
    }
    public class ShootGuard : ObjectBase
    {
        public override ObjectTag tag { get { return ObjectTag.PLAYERS_GARD_SHOOT; } }
        public override float length { get { return 28; } }
        public override float scale { get { return 0.6f; } }
        public override int HP_MAX { get { return 1; } }

        public override bool TagCheck(ObjectTag tags) {
            return (tags == ObjectTag.ENEMY);
        }
        public override void Initialize() {
            base.Initialize();

            sAnim = new SpriteAnimation(2);
            sAnim.addList(0, 39, 39, 0, AnimationType.Delete);
            sAnim.addList(1, 39, 39, 1, AnimationType.Stay);
            sAnim.init();
        }
        public override void setPosition(float ix, float iy) {
            base.setPosition(ix, iy);
            sAnim.nextSet(1);
        }
        public override void update() {
            px += vx;
            py += vy;

            if (px > 1400 || px < -400 || py < -300 || py > 960) {
                Remove();
            }
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
        public override void draw() {
            if (sAnim.getDrawable)
                SpriteManager.getInstance.Draw(sAnim.index, new Point(px, py), new Point(32, 32), 0.0f, scale);
        }
    }
    public class ShootSpecial : ObjectBase
    {
        public override ObjectTag tag { get { return ObjectTag.PLAYERS_GARD_SHOOT; } }
        public override float length { get { return 28; } }
        public override float scale { get { return 1.0f; } }
        public override int HP_MAX { get { return 20; } }

        public override bool TagCheck(ObjectTag tags) {
            return (tags == ObjectTag.ENEMY || tags == ObjectTag.ENEMYS_SHOOT);
        }
        public override void Initialize() {
            base.Initialize();

            sAnim = new SpriteAnimation(3);
            sAnim.addList(0, 39, 39, 0, AnimationType.Delete);
            sAnim.addList(1, 39, 39, 1, AnimationType.Stay);
            sAnim.addList(2, 39, 39, 2, AnimationType.Stay);
            sAnim.init();
        }
        public override void setPosition(float ix, float iy) {
            base.setPosition(ix, iy);
            sAnim.nextSet(1);
        }
        public override void update() {
            px += vx;
            py += vy;

            if (sAnim.anim_index == 1)
            {
                vx *= 0.94f;
                vy *= 0.94f;
                if (vx * vx < 0.05f)
                    sAnim.nextSet(2);
            }
            if (sAnim.anim_index == 2) {
                vx = 0.0f;
                vy += MyMath.PointToLength(new Point(Player.getInstance.px, Player.getInstance.py), new Point(px, py)) / 60.0f;
            }

            if (px > 1400 || px < -400 || py > 960) {
                Remove();
            }
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
        public override void draw() {
            if (sAnim.getDrawable)
                SpriteManager.getInstance.Draw(sAnim.index, new Point(px, py), new Point(32, 32), 0.0f, scale);
        }
    }
}
