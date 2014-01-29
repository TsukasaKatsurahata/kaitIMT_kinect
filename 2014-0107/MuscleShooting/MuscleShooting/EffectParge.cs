using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace MuscleShooting
{
    public class EffectParge
    {
        private float px, py;
        private float scale;
        SpriteAnimation sAnim;

        public EffectParge() {
            px = py = scale = 0.0f;
            sAnim = new SpriteAnimation(2);
            sAnim.addList(0, 40, 40, 0, AnimationType.Delete);
            sAnim.addList(1, 40, 49, 0, AnimationType.Up);
            sAnim.init();
        }
        public void setPosition(float ix, float iy, float isc) {
            px = ix;
            py = iy;
            scale = isc;
            sAnim.nextSet(1);
        }
        public void Remove() {
            sAnim.init();
        }
        public void draw() {
            if (!sAnim.getDrawable) return;
            SpriteManager.getInstance.Draw(sAnim.index, new Point(px, py), new Point(64, 64), scale);
            sAnim.update((int)(2 * Math.Sqrt(scale)));
        }
    }
}
