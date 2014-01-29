using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace MuscleShooting
{
    public enum ObjectTag {
        DEFAULT,
        PLAYER,
        ENEMY,
        PLAYERS_SHOOT,
        ENEMYS_SHOOT,
        PLAYERS_GARD_SHOOT,
        BOMB_SHOOOT,
    };
    public class ObjectBase
    {
        public float px, py;
        public float vx, vy;

        protected float hp;

        public virtual ObjectTag tag { get { return ObjectTag.DEFAULT; } } // 衝突判定に使うタグ
        public virtual float scale { get { return 1.0f; } } // 大きさ
        public virtual float length { get { return 1.0f; } }// 判定範囲

        public virtual int HP_MAX { get { return 1; } }

        public virtual bool TagCheck(ObjectTag tags) {
            return true;
        }

        protected SpriteAnimation sAnim;
        public bool active;

        public bool AnimTypeDeleted { get { return sAnim.getType() != AnimationType.Delete; } }

        public ObjectBase() {
            Initialize();
        }

        public virtual void Initialize() {
            px = py = vx = vy = 0;
            sAnim = new SpriteAnimation(1);
            sAnim.addList(0, 0, 0, 1, AnimationType.Delete);
            sAnim.init();

            hp = HP_MAX;
            active = false;
        }

        public virtual void setPosition(float ix, float iy) {
            px = ix;
            py = iy;
            vx = vy = 0;
            active = true;
            sAnim.init();
        }

        public virtual void update() { 
        }

        public virtual bool Collision(ObjectBase obj) {
            if (!obj.active) return false;
            if (!this.active) return false;
            if (!TagCheck(obj.tag)) return false;
            float col = MyMath.Collision(new Point(obj.px, obj.py), new Point(px, py), new Point(obj.vx, obj.vy), new Point(vx, vy), obj.length * obj.scale + length * scale);
            return CollsionEnter(col, obj.tag);
        }
        protected virtual bool CollsionEnter(float dt, ObjectTag tags) {
            if (dt >= 0.0f && dt <= 1.0f) {
                Break();
                return true;
            }
            return false;
        }

        public virtual void Break() {
            Remove();
        }

        public virtual void Remove() {
            active = false;
            sAnim.init();
        }

        public virtual void draw() {
        }
    }
}
