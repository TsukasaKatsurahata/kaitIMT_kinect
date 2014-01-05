/*
 * 神奈川工科大学　情報学部　情報メディア学科
 * 1123157　桂畑 司
 * 2013/12/24
 */
/*
 * アニメーションを管理するクラス
 * このクラスでは、現在再生中のアニメーションを数値で返すことができる
 * また、SpriteManagerで画像を全て数値（配列の番号）で管理しているため、メソッドにそのまま代入できる
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArrowSimulater
{
    public enum AnimationType { 
        Up, 
        Down, 
        Stay, 
        Delete, 
        Max, 
    };

    public class AnimationList {
        public int start;   ///< 開始点
        public int end;     ///< 終了点
        public int next;    ///< 次のアニメーション
        public AnimationType type; ///< アニメーションのタイプ

        public AnimationList(int s, int e, int n, AnimationType t) {
            start = s; end = e; next = n; type = t;
        }
    }

    public class SpriteAnimation
    {
        private AnimationList[] list;
        public int index;
        public int anim_index;
        private int timer;
        public int getTimer()      { return timer % 5; }
        public int getTimer(int d) { return timer % d; }
        public bool getDrawable { get { return (getType() == AnimationType.Delete) ? false : true; } }
        public AnimationType getType() { return list[anim_index].type; }

        public void nextSet(int d) {
            anim_index = d;
#if false
            switch (list[anim_index].type) {
                case AnimationType:
                    index = list[anim_index].min;
                    break;
                case 2:
                    index = list[anim_index].max;
                    break;
                case 3:
                    index = list[anim_index].min;
                    break;
                default:
                    break;
            }
#else
            index = list[anim_index].start;
#endif
        }
        public void animSet(int d) {
            index = d;
        }
        public void addList(int d, int start, int end, int next, AnimationType type) {
            list[d] = new AnimationList(start, end, next, type);
        }
        public SpriteAnimation(int d) {
            list = new AnimationList[d];
            index = 0;
            anim_index = 0;
            timer = 0;
        }
        public void init() {
            index = 0;
            nextSet(index);
            timer = 0;
        }
        public void update() {
            timer++;
            if (timer % 5 == 0)   // アニメーションのフレームレート調整
            switch (list[anim_index].type) { 
                case AnimationType.Up:
                    index++;
                    if (index > list[anim_index].end) {
                        this.nextSet(list[anim_index].next);
                    }
                    break;
                case AnimationType.Down:
                    index--;
                    if (index < list[anim_index].end) {
                        this.nextSet(list[anim_index].next);
                    }
                    break;
                default:
                    break;
            }
        }
        public void update(int t) {
            timer++;
            if (timer % t == 0)   // アニメーションのフレームレート調整
            switch (list[anim_index].type) { 
                case AnimationType.Up:
                    index++;
                    if (index > list[anim_index].end) {
                        this.nextSet(list[anim_index].next);
                    }
                    break;
                case AnimationType.Down:
                    index--;
                    if (index < list[anim_index].end) {
                        this.nextSet(list[anim_index].next);
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
