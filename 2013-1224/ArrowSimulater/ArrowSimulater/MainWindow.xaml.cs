/*
 * 神奈川工科大学　情報学部　情報メディア学科
 * 1123157　桂畑 司
 * 2013/12/24
 */
/*
 * 全体を管理する中心となるクラス
 * 描画関係は、このクラスの子として代入して処理を行う
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ArrowSimulater
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private float mdx = 0, mdy = 0; // マウスポインタでデバグ

        private DispatcherTimer dispatcherTimer;

        public static Random rand;


        #region Gameの制御に必要な変数

        // 状態遷移を管理するための変数
        enum GAMESTATE_ENUM {
            INITIALIZE, 
            TITLE, 
            TUTORIAL, 
            PLAYING, 
            RESULT, 
            RUNKING, 
            GAMESTATE_MAX,
        };

        private delegate void GAMESTATE();
        GAMESTATE[] GameState = new GAMESTATE[(int)GAMESTATE_ENUM.GAMESTATE_MAX];

        private GAMESTATE_ENUM state_index;
        private int state_timer;

        private BitmapImage titleTex;
        private BitmapImage[] tutorialTex;
        private BitmapImage resultTex;
        private BitmapImage rankingTex;

        private BitmapImage BackgroundTex;
        private BitmapImage ResultBackTex;
        private BitmapImage SoremadeTex;

        private Image background;


        private Shoot shoot;

        private TargetBase[] target;

        private int timeLimit = 40 * 60 + 38;

        private int resultScore;

        private int resultTime; // 残り時間を記録する

        private int HitCount;   // 的を射抜いた数を記録する

        private int TimeBounus = 20;
        private int HitBounus = 10;

        private int targetIndex; // 的の出現に使う

        private int TMax;


        Point numAsset = new Point(16, 0);
        

        #endregion

        #region public

        public static MainWindow getInstance;

        public static Canvas mainCanvas;

        #endregion

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            getInstance = this;

            rand = new Random();

            mainCanvas = MainCanvas;

            background = new Image();
            background.Source = null;
            background.Width = 960;
            background.Height = 540;
            background.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            background.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            background.Margin = new Thickness(0, 0, 0, 0);
            mainCanvas.Children.Add(background);

            stateSetInit();

            titleTex = new BitmapImage(new Uri("/ArrowSimulater;component/Resource/Back/ArrowSimulaterTitle.png", System.UriKind.Relative));

            tutorialTex = new BitmapImage[4];
            for (int i = 0; i < 4; i++) { 
                tutorialTex[i] = new BitmapImage(new Uri("/ArrowSimulater;component/Resource/Back/ArrowSimulaterTutorial0" + i.ToString() + ".png", System.UriKind.Relative));
            }
            BackgroundTex =  new BitmapImage(new Uri("/ArrowSimulater;component/Resource/Back/ArrowSimulaterBackground01.png", System.UriKind.Relative));
            ResultBackTex = new BitmapImage(new Uri("/ArrowSimulater;component/Resource/Back/ArrowSimulaterBackground00.png", System.UriKind.Relative));
            resultTex = new BitmapImage(new Uri("/ArrowSimulater;component/Resource/Score/ArrowSimulaterResult.png", System.UriKind.Relative));
            rankingTex = new BitmapImage(new Uri("/ArrowSimulater;component/Resource/Score/ArrowSimulaterRanking.png", System.UriKind.Relative));
            SoremadeTex = new BitmapImage(new Uri("/ArrowSimulater;component/Resource/Back/soremade.png", System.UriKind.Relative));

            KinectManager kinectManager = new KinectManager();
            kinectManager.Initialize();

            Bow bow = new Bow();
            bow.Initialize();

            SpriteManager spriteManager = new SpriteManager();
            spriteManager.Initialize();

            ShootLine shootLine = new ShootLine();
            shootLine.Initialize();

            dispatcherTimer = new DispatcherTimer();

            dispatcherTimer.Interval = new TimeSpan(166666L);

            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);

            dispatcherTimer.Start();

            shoot = new Shoot();

            ScoreManager scoreManager = new ScoreManager();
            scoreManager.Initialize();

            //int[] sL = { 100, 300, 500, 1000, 2000, 4000, 8000 };
            //ScoreManager.getInstance.scoreRoot.Add(sL);

            TMax = 40;
            target = new TargetBase[TMax];

            for (int i = 0; i < TMax - 15; i++) {
                target[i] = new TargetSimple();
                //target[i].setPosition((float)rand.NextDouble() * 250 + 50, (float)rand.NextDouble() * 400 + 100);
            }
            for (int i = TMax - 15; i < TMax - 3; i++) {
                target[i] = new TargetSmall();
            }
            for (int i = TMax - 3; i < TMax; i++) {
                target[i] = new TargetFire();
            }
            target[0].setPosition(100, 400);

            targetIndex = 0;
            
            //imageSet.Source = SpriteManager.images[0];
        }

        private void stateSetInit()
        { 
            // 状態遷移を管理する関数を作る
            state_index = GAMESTATE_ENUM.INITIALIZE;
            state_timer = 0;
            GameState[(int)GAMESTATE_ENUM.INITIALIZE] = stateInitialize;
            GameState[(int)GAMESTATE_ENUM.TITLE] = stateTitle;
            GameState[(int)GAMESTATE_ENUM.TUTORIAL] = stateTutorial;
            GameState[(int)GAMESTATE_ENUM.PLAYING] = statePlaying;
            GameState[(int)GAMESTATE_ENUM.RESULT] = stateResult;
            GameState[(int)GAMESTATE_ENUM.RUNKING] = stateRunking;
        }

        private void stateUpdate() {
            state_timer++;
        }

        private void stateNext(GAMESTATE_ENUM gse) {
            state_index = gse;
            state_timer = 0;
        }

        private void MainCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            Point FPoint = e.GetPosition((UIElement)sender);

            ShootLine.getInstance.setLine(mdx, mdy, (float)FPoint.X, (float)FPoint.Y);

            mdx = (float)FPoint.X; mdy = (float)FPoint.Y;
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e) {
#if false
            ShootLine.getInstance.update();

            // ここから先　制御関連

            KinectManager.getInstance.input();

            for (int i = 0; i < target.Length; i++) {
                target[i].Collision(shoot);
            }

            // ここから先　更新関連

            shoot.update();

            //Bow.getInstance.Idling(new Point(0, 40));
            //Bow.getInstance.unDraw();

            for (int i = 0; i < target.Length; i++)
                target[i].update();

            // ここから先　描画関連
            SpriteManager.getInstance.Clear();

            for (int i = 0; i < target.Length; i++) 
                target[i].draw();

            //SpriteManager.getInstance.Draw(0, new Point(mdx, mdy), new Point(32, 32));
#else
            GameState[(int)state_index]();
#endif
        }

        public void Shoot(float px, float py, float vx, float vy) {
            shoot.setVelocity(px, py, vx, vy);
        }

        private void DrawScore(int input, Point pos, float scale) { 
            int[] timeString = ScoreManager.getInstance.ToStringInt(input);
            for (int i = timeString.Length - 1; i >= 0; i--) { 
                SpriteManager.getInstance.Draw(13 + timeString[i], new Point(pos.X, pos.Y + 30.0f * scale * (timeString.Length - 1 - i)), numAsset, scale);
            }
        }

        #region stateMethod

        private void stateInitialize() {
            stateNext(GAMESTATE_ENUM.TITLE);

            shoot.active = false;
            ShootLine.getInstance.Remove();

            KinectManager.getInstance.RemoveBone();

            Bow.getInstance.unDraw();

            ScoreManager.getInstance.PointDeleter();

            SpriteManager.getInstance.Clear();

            SpriteManager.getInstance.Draw(titleTex);

            stateNext(GAMESTATE_ENUM.TITLE);

            resultScore = 0;
            resultTime = 0;
            HitCount = 0;

            background.Source = BackgroundTex;

            for (int i = 0; i < TMax; i++) {
                target[i].Remove();
            }
            target[0].setPosition(100, 400);

            targetIndex = 0;
        }

        private void stateTitle()
        {
            stateUpdate();

            if (state_timer > 360) {
                stateNext(GAMESTATE_ENUM.TUTORIAL);
                //state_timer = timeLimit;
            }

            SpriteManager.getInstance.Clear();

            SpriteManager.getInstance.Draw(titleTex);

            //ScoreManager.getInstance.DrawScore();
        }

        private void stateTutorial() {
            stateUpdate();
            SpriteManager.getInstance.Clear();

            if (state_timer < 2000)
            {
                switch (KinectManager.getInstance.input())
                {
                    case KinectState.Other:
                    case KinectState.None:
                        if (state_timer > 1000 && state_timer < 2000) {
                            stateNext(GAMESTATE_ENUM.INITIALIZE);
                        }
                        SpriteManager.getInstance.Draw(tutorialTex[0]);
                        break;
                    case KinectState.Set:
                        SpriteManager.getInstance.Draw(tutorialTex[1]);
                        break;
                    case KinectState.Charge:
                        SpriteManager.getInstance.Draw(tutorialTex[2]);
                        break;
                    case KinectState.Aim:
                        SpriteManager.getInstance.Draw(tutorialTex[2]);
                        break;
                    case KinectState.Shoot:
                        SpriteManager.getInstance.Draw(tutorialTex[3]);
                        state_timer = 3000;
                        break;
                    default:
                        break;
                }
            }
            else
            {
                //stateUpdate();
                SpriteManager.getInstance.Draw(tutorialTex[3]);
                if (state_timer > 3240) {
                    stateNext(GAMESTATE_ENUM.PLAYING);
                    KinectManager.getInstance.Reset();
                    shoot.active = false;
                    state_timer = timeLimit;
                }
            }
        }

        private void statePlaying() {
            //stateUpdate();
            state_timer--;

            ShootLine.getInstance.update();

            // ここから先　制御関連

            if (state_timer > 0) KinectManager.getInstance.input();

            for (int i = 0; i < target.Length; i++) {
                if (target[i].Collision(shoot))
                    HitCount++;
            }

            bool targetActive = false;
            for (int i = 0; i < target.Length; i++) {
                targetActive = targetActive || target[i].active;
            }

            #region 的の出現関数
            if (!targetActive) {
                switch (targetIndex) { 
                    case 0:
                        target[1].setPosition(50, 200);
                        target[2].setPosition(50, 320);
                        target[3].setPosition(50, 460);
                        state_timer += 40 * 30;
                        targetIndex++;
                        break;
                    case 1:
                        target[4].setPosition(200, 100);
                        target[5].setPosition(100, 150);
                        target[6].setPosition(100, 350);
                        target[7].setPosition(200, 470);
                        target[8].setPosition(300, 120);
                        state_timer += 40 * 45;
                        targetIndex++;
                        ScoreManager.getInstance.PointInc(30);
                        break;
                    case 2:
                        target[25].setPosition(70, 190);
                        target[26].setPosition(160, 230);
                        target[27].setPosition(250, 270);
                        state_timer += 40 * 30;
                        targetIndex++;
                        ScoreManager.getInstance.PointInc(60);
                        break;
                    case 3:
                        target[28].setPosition(120, 100);
                        target[29].setPosition(180, 147);
                        target[30].setPosition(70, 180);
                        target[31].setPosition(120, 212);
                        target[32].setPosition(60, 400);
                        target[33].setPosition(112, 410);
                        state_timer += 40 * 45;
                        targetIndex++;
                        ScoreManager.getInstance.PointInc(60);
                        break;
                    case 4:
                        target[37].setPosition(250, 300);
                        state_timer += 40 * 45;
                        targetIndex++;
                        if (state_timer > 40 * 50) ScoreManager.getInstance.PointInc(150);
                        break;
                    case 5:
                        target[37].setPosition(300, 260);
                        target[38].setPosition(160, 140);
                        target[39].setPosition(160, 370);
                        state_timer += 40 * 60;
                        targetIndex++;
                        ScoreManager.getInstance.PointInc(100);
                        break;
                    case 6:
                        target[0].setPosition(70, 100);
                        target[1].setPosition(150, 100);
                        target[2].setPosition(220, 100);
                        target[25].setPosition(70, 440);
                        target[26].setPosition(150, 440);
                        target[27].setPosition(220, 440);
                        target[37].setPosition(300, 300);
                        target[38].setPosition(200, 200);
                        target[39].setPosition(150, 360);
                        state_timer += 40 * 30;
                        targetIndex++;
                        ScoreManager.getInstance.PointInc((state_timer > 40 * 120) ? 200 : (state_timer > 40 * 60) ? 100 : 20);
                        break;
                    case 7:
                        targetActive = false;
                        for (int i = 0; i < TMax; i++)
                            targetActive = targetActive || (target[i].AnimTypeDeleted);
                        if (!targetActive) {
                            for (int i = 0; i < TMax; i++) 
                                target[i].setPosition((float)rand.NextDouble() * 250 + 50, (float)rand.NextDouble() * 430 + 70);
                            targetIndex++;
                            ScoreManager.getInstance.PointInc((state_timer > 40 * 120) ? 350 : (state_timer > 40 * 60) ? 150 : 40);
                        }
                        else { state_timer++; }
                        break;
                    case 8:
                        if (resultTime < 1) resultTime = state_timer;
                        break;
                    default:
                        //if (resultTime < 1) resultTime = state_timer;
                        break;
                }
            }
            #endregion

#if false
            if (state_timer % 40 == 0) for (int i = 0; i < TMax; i++) if (target[i].active) target[i].Break();
#endif

            if (state_timer < resultTime - 80) {
                stateNext(GAMESTATE_ENUM.RESULT);
                background.Source = ResultBackTex;
                resultScore = ScoreManager.getInstance.Counter;
                resultTime /= 40;
                ScoreManager.getInstance.PointInc(resultTime * TimeBounus + HitCount * HitBounus);
            }

            // ここから先　更新関連

            shoot.update();

            //Bow.getInstance.Idling(new Point(0, 40));
            //Bow.getInstance.unDraw();

            for (int i = 0; i < target.Length; i++)
                target[i].update();

            // ここから先　描画関連
            SpriteManager.getInstance.Clear();

            for (int i = 0; i < target.Length; i++) 
                target[i].draw();

            ScoreManager.getInstance.DrawScore();

            SpriteManager.getInstance.Draw(SpriteManager.images[23], new Point(740, 8), new Point(100, 360));

            int st = state_timer / 40;
            if (st < 0) st = 0;
            int[] timeString = ScoreManager.getInstance.ToStringInt(st);
            for (int i = timeString.Length - 1; i >= 0; i--) { 
                SpriteManager.getInstance.Draw(13 + timeString[i], new Point(758, 224 + 30 * (timeString.Length - 1 - i)));
            }

            if (state_timer < resultTime)
                SpriteManager.getInstance.Draw(SoremadeTex);


            //SpriteManager.getInstance.Draw(0, new Point(mdx, mdy), new Point(32, 32));
        }

        private void stateResult() {
            stateUpdate();

            // 制御関連処理

            if (state_timer > 240) {
                stateNext(GAMESTATE_ENUM.RUNKING);
                ScoreManager.getInstance.ScoreSort(ScoreManager.getInstance.Counter);
            }

            // 描画関連処理
            SpriteManager.getInstance.Clear();

            SpriteManager.getInstance.Draw(resultTex);

            if (state_timer > 30) DrawScore(resultScore, new Point(750, 265), 1.4f);
            //DrawScore(13905, new Point(750, 265), 1.4f);

            if (state_timer > 55) DrawScore(resultTime, new Point(634, 265), 1.4f);

            if (state_timer > 80) DrawScore(HitCount, new Point(508, 265), 1.4f);

            if (state_timer > 110) DrawScore(resultTime * TimeBounus + HitCount * HitBounus, new Point(285, 265), 1.5f);

            if (state_timer > 160) DrawScore(ScoreManager.getInstance.Counter, new Point(28, 258), 1.76f);
        }

        private void stateRunking() {
            stateUpdate();

            // 制御関連処理

            if (state_timer > 240) {
                stateNext(GAMESTATE_ENUM.INITIALIZE);
            }

            // 描画関連処理
            SpriteManager.getInstance.Clear();

            SpriteManager.getInstance.Draw(rankingTex);

            ScoreManager.getInstance.DrawRanking();

            int rank = ScoreManager.getInstance.Ranking;
            if (rank < 9) {
                SpriteManager.getInstance.DrawCenter(1 + (state_timer / 4) % 4, new Point(729 - 80 * (rank - 1), 75), 1.2f);
            }
            else {
                SpriteManager.getInstance.DrawCenter(1 + (state_timer / 6) % 4, new Point(78, 75), 0.9f);
            }
        }

        #endregion
    }
}
