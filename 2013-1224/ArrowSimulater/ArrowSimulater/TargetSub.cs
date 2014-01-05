/*
 * 神奈川工科大学　情報学部　情報メディア学科
 * 1123157　桂畑 司
 * 2013/12/24
 */
/*
 * TargetBaseを継承したクラス群
 * 単純なものから特殊なものまで
 * 基本的にはインスタンスを作って、setPositionメソッドで位置を設定、Collisionで衝突判定をさせれば
 * 他はどんな処理でも影響なく動く
 * 移動する的、大きくなったり小さくなったりする的、矢が貫通しない的など多種多様な的を作成できる
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArrowSimulater
{
    class TargetSimple : TargetBase
    {
        public override int point { get { return 10; } }

        public override void Initialize()
        {
            px = py = 0;
            length = 32;
            scale = 1.0f;

            sAnim = new SpriteAnimation(3);
            sAnim.addList(1, 5, 5, 1, AnimationType.Stay);
            sAnim.addList(2, 5, 11, 0, AnimationType.Up);
            sAnim.addList(0, 11, 11, 0, AnimationType.Delete);
            sAnim.init();

            active = false;
        }

        public override void setPosition(float x, float y)
        {
            base.setPosition(x, y);
            sAnim.nextSet(1);
        }

        public override void Break() {
            base.Break();
            sAnim.nextSet(2);
            //active = false;
        }
    }

    class TargetSmall : TargetBase
    {
        public override int point { get { return 50; } }

        public override void Initialize()
        {
            px = py = 0;
            length = 24;
            scale = 0.75f;

            sAnim = new SpriteAnimation(3);
            sAnim.addList(1, 5, 5, 1, AnimationType.Stay);
            sAnim.addList(2, 5, 11, 0, AnimationType.Up);
            sAnim.addList(0, 11, 11, 0, AnimationType.Delete);
            sAnim.init();

            active = false;
        }

        public override void setPosition(float x, float y)
        {
            base.setPosition(x, y);
            sAnim.nextSet(1);
        }

        public override void Break() {
            base.Break();
            sAnim.nextSet(2);
            //active = false;
        }
    }

    class TargetFire : TargetBase { 
        public override int point { get { return (int)(30 / (scale * scale)); } }

        public override void Initialize()
        {
            px = py = 0;
            length = 32;
            scale = 1.0f;

            sAnim = new SpriteAnimation(2);
            sAnim.addList(0, 11, 11, 0, AnimationType.Delete);
            sAnim.addList(1, 1, 4, 1, AnimationType.Up);
            sAnim.init();

            active = false;
        }

        public override void setPosition(float x, float y)
        {
            base.setPosition(x, y);
            sAnim.nextSet(1);
            scale = 1.0f;
            length = 32;
        }

        public override void Break() {
            ScoreManager.getInstance.PointInc(point);
            if (scale < 0.6f) {
                active = false;
                sAnim.nextSet(0);
            }
            scale *= 0.7f;
            length *= 0.7f;
        }
    }
}
