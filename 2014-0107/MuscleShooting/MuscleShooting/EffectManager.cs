using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MuscleShooting
{
    public class EffectManager
    {
        public static EffectManager getInstance;

        private EffectParge[] effect;
        private int index;

        private int max = 120;

        public void Initilize() {
            getInstance = this;

            index = 0;
            effect = new EffectParge[max];
            for (int i = 0; i < effect.Length; i++) {
                effect[i] = new EffectParge();
            }
        }

        public void setEffect(float ix, float iy, float scale) {
            effect[index].setPosition(ix, iy, scale);
            index = (index + 1) % effect.Length;
        }

        public void Remove() {
            for (int i = 0; i < effect.Length; i++) {
                effect[i].Remove();
            }
        }

        public void draw()
        {
            for (int i = 0; i < effect.Length; i++) {
                effect[i].draw();
            }
        }
    }
}
