using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace MuscleShooting
{
    public class Player : ObjectBase
    {
        public static Player getInstance;

        public ObjectBase[] shoot;
        private int sIndex = 0;

        private ObjectBase[] manageShoot; // 制御用
        private int mIndex;
        private int mMax;

        public override ObjectTag tag { get { return ObjectTag.PLAYER; } }

        public override float length { get { return 20; } }

        public override bool TagCheck(ObjectTag tags) {
            return (tags == ObjectTag.ENEMY || tags == ObjectTag.ENEMYS_SHOOT);
        }

        private float dvx1, dvy1, dvx2, dvy2;
        private int timer;

        public override void Initialize() {
            getInstance = this;
            base.Initialize();

            sAnim = new SpriteAnimation(9);
            sAnim.addList(0, 25, 25, 0, AnimationType.Delete);
            sAnim.addList(1, 25, 25, 1, AnimationType.Stay);
            sAnim.addList(2, 26, 26, 2, AnimationType.Stay);
            sAnim.addList(3, 27, 27, 3, AnimationType.Stay);
            sAnim.addList(4, 28, 28, 4, AnimationType.Stay);
            sAnim.addList(5, 29, 29, 5, AnimationType.Stay);
            sAnim.addList(6, 30, 30, 6, AnimationType.Stay);
            sAnim.addList(7, 31, 31, 7, AnimationType.Stay);
            sAnim.addList(8, 32, 32, 8, AnimationType.Stay);

            dvx1 = dvx2 = dvy1 = dvy2 = 0;
            manageShoot = new ObjectBase[1];
            manageShoot[0] = new ShootGuard();
            mIndex = 0;
            mMax = 1;
        }

        public override void setPosition(float ix, float iy) {
            base.setPosition(ix, iy);
            sAnim.nextSet(1);
            timer = 0;
        }

        public void VectorPath(Point vector1, Point vector2) {
            dvx1 = (float)vector1.X;
            dvy1 = (float)vector1.Y;
            dvx2 = (float)vector2.X;
            dvy2 = (float)vector2.Y;
        }

        public void Shoot(ObjectBase obj, Point vector) {
            shoot[sIndex] = obj;
            shoot[sIndex].setPosition(px, py);
            shoot[sIndex].vx = (float)vector.X;
            shoot[sIndex].vy = (float)vector.Y;
            sIndex = (sIndex + 1) % shoot.Length;
        }

        public override void update() {
            timer++;
            px += vx;
            px = (px < 100 || px > 924) ? px - vx : px;
            //py += vy;
            vx *= 0.94f;

            //if (timer % 6 == 0) Shoot(new ShootNormal(), new Point((MainWindow.rand.NextDouble() - 0.5) * 3.0, 8.0));

            // アニメーションのインデックスで姿勢・状態を判断
            switch (sAnim.anim_index) { 
                case 0: // Delete
                    break;
                case 1: // Idle
                    mIndex = 0;
                    break;
                case 2: // MoveSet
                    if (KinectManager.getInstance.nowState == KinectState.MoveRight) vx = -10.0f;
                    if (KinectManager.getInstance.nowState == KinectState.MoveLeft) vx = 10.0f;
                    break;
                case 3: // Gard
                    // initilize
                    if (mMax < 5) {
                        mMax = 80;
                        manageShoot = new ObjectBase[mMax];
                        for (int i = 0; i < mMax; i++) {
                            manageShoot[i] = new ShootGuard();
                        }
                        mIndex = 0;
                    }
                    // update
                    if (timer % 7 == 0 && mIndex < mMax) {
                        for (int i = 0; i < 8; i++) {
                            if (mIndex + i < manageShoot.Length) 
                                Shoot(manageShoot[mIndex + i], new Point(8.0 * Math.Cos(Math.PI * i / 4.0), 8.0 * Math.Sin(Math.PI * i / 4.0)));
                        }
                        mIndex = (mIndex + 8);
                    }
                    // 雑用変数
                    float dpx, dpy;
                    float dvx, dvy;
                    float dL;
                    for (int i = 0; i < mIndex; i++) {
                        dpx = manageShoot[i].px - px;
                        dpy = manageShoot[i].py - py;
                        if (dpx * dpx + dpy * dpy > 290) {
                            dvx = -dpy;
                            dvy = dpx;
                            dL = (float)Math.Sqrt(dvx * dvx + dvy * dvy);
                            dvx /= dL;
                            dvy /= dL;
                            dvx *= 1.3f;
                            dvy *= 1.3f;
                            manageShoot[i].vx = dvx;
                            manageShoot[i].vy = dvy;
                        }
                    }
                    if (!(
                        (KinectManager.getInstance.nowState == KinectState.Guard)
                        || (KinectManager.getInstance.nowState == KinectState.GuardParge)
                        ))
                    {
                        for (int i = 0; i < manageShoot.Length; i++)
                            manageShoot[i].Break();
                        manageShoot = new ObjectBase[1];
                        manageShoot[0] = new ShootGuard();
                        mMax = 1;
                        mIndex = 0;
                    }
                    else {
                        if (KinectManager.getInstance.nowState == KinectState.GuardParge) {
                            for (int i = 0; i < manageShoot.Length && i < mIndex; i++) {
                                dpx = manageShoot[i].px - px;
                                dpy = manageShoot[i].py - py;
                                if (dpx * dpx + dpy * dpy > 100) {
                                    manageShoot[i].vx = dpx / 4.0f;
                                    manageShoot[i].vy = dpy / 4.0f;
                                }
                            }
                            manageShoot = new ObjectBase[1];
                            manageShoot[0] = new ShootGuard();
                            mMax = 1;
                            mIndex = 0;
                        }
                    }
                    break;
                case 4: // Parge
                    break;
                case 5: // Bomb
                    if (manageShoot.Length < 5 && mIndex < 1) {
                        mMax = 60;
                        manageShoot = new ObjectBase[mMax];
                        for (int i = 0; i < manageShoot.Length; i++) { 
                            manageShoot[i] = new ShootSpecial();
                        }
                        mIndex = 0;
                    }
                    if (mIndex < manageShoot.Length) { 
                        manageShoot[mIndex].setPosition(px, py);
                        manageShoot[mIndex].vx = (float)MainWindow.rand.NextDouble() * mIndex * 1.2f;
                        manageShoot[mIndex].vy = -6.0f + (float)MainWindow.rand.NextDouble() * 2.0f;
                        this.Shoot(manageShoot[mIndex], new Point((float)(MainWindow.rand.NextDouble() - 0.5) * mIndex * 2.5f, -6.0f - mIndex * 0.2f + (float)MainWindow.rand.NextDouble() * 3.0f));
                        mIndex++;
                    }
                    else { 
                        mMax = 1;
                        manageShoot = new ObjectBase[mMax];
                        manageShoot[0] = new ShootGuard();
                        //mIndex = 0;
                    }
                    break;
                case 6: // Fire
                    if (timer % 24 == 0) {
                        double ddvx, ddvy;
                        ddvx = dvx1 + dvx2 + 5.0 * (MainWindow.rand.NextDouble() - 0.5);
                        ddvy = dvy1 + dvy2;
                        double ddL = Math.Sqrt(ddvx * ddvx + ddvy * ddvy);
                        ddvx /= ddL;
                        ddvy /= ddL;
                        double[] drx, dry;
                        drx = new double[4]; dry = new double[4];
                        drx[0] = 1.0f; drx[1] = Math.Cos(Math.PI / 8.0); drx[2] = Math.Cos(Math.PI / 4.0); drx[3] = Math.Cos(Math.PI * 3.0 / 8.0);
                        dry[0] = 0.0f; dry[1] = Math.Sin(Math.PI / 8.0); dry[2] = Math.Sin(Math.PI / 4.0); dry[3] = Math.Sin(Math.PI * 3.0 / 8.0);
                        double[] rx = new double[4], ry = new double[4];
                        for (int i = 0; i < 4; i++) {
                            rx[i] = drx[i] * ddvx + dry[i] * ddvy;
                            ry[i] = -drx[i] * ddvy + dry[i] * ddvx;
                            rx[i] *= 5.5;
                            ry[i] *= 5.5;
                        }
                        for (int i = 0; i < 4; i++) {
                            Shoot(new ShootNormal(), new Point(rx[i], ry[i]));
                            Shoot(new ShootNormal(), new Point(ry[i], -rx[i]));
                            Shoot(new ShootNormal(), new Point(-rx[i], -ry[i]));
                            Shoot(new ShootNormal(), new Point(-ry[i], rx[i]));
                        }
                    }
                    break;
                case 7: // right
                    if (KinectManager.getInstance.nowState != KinectState.MoveRight) vx = 0.0f;
                    break;
                case 8: // left
                    if (KinectManager.getInstance.nowState != KinectState.MoveLeft) vx = 0.0f;
                    break;
                default:
                    break;
            }
            #region AI処理後、見た目の変更する（姿勢を反映）
            switch (KinectManager.getInstance.nowState) { 
                case KinectState.Other:
                    sAnim.nextSet(0);
                    break;
                case KinectState.Idle:
                    sAnim.nextSet(1);
                    break;
                case KinectState.MoveSet:
                    sAnim.nextSet(2);
                    break;
                case KinectState.Guard:
                    sAnim.nextSet(3);
                    break;
                case KinectState.GuardParge:
                    sAnim.nextSet(4);
                    break;
                case KinectState.AraburuTakaNoPose:
                    sAnim.nextSet(5);
                    break;
                case KinectState.MuscleAtack:
                    sAnim.nextSet(6);
                    break;
                case KinectState.MoveRight:
                    sAnim.nextSet(7);
                    break;
                case KinectState.MoveLeft:
                    sAnim.nextSet(8);
                    break;
                default:
                    break;
            }
            #endregion

            for (int i = 0; i < shoot.Length; i++) {
                shoot[i].update();
            }
        }

        protected override bool CollsionEnter(float dt, ObjectTag tags)
        {
            if (dt >= 0.0f && dt <= 1.0f) {
                if (tags == ObjectTag.ENEMY) return true;
                if (tags == ObjectTag.ENEMYS_SHOOT) hp--;
            }
            return false;
        }

        public override void draw() {
            if (sAnim.getDrawable)
                SpriteManager.getInstance.Draw(sAnim.index, new Point(px, py), new Point(64, 80), 0.0f, 1.0f);
        }
    }
}
