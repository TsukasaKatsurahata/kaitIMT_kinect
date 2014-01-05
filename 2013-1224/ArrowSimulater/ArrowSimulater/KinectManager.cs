/*
 * 神奈川工科大学　情報学部　情報メディア学科
 * 1123157　桂畑 司
 * 2013/12/24
 */
/*
 * Kinectの入力を管理するクラス
 * Kinectの入力を起点とする処理は全てここで行う
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
using System.IO;
using System.Globalization;
using System.Diagnostics;
using System.Windows.Threading;
using Microsoft.Kinect;

namespace ArrowSimulater
{
    public enum KinectState
    {
        Other, // 関係ないシーン
        None,  // 待機状態Idle
        Set,   // 構え
        Charge,// 弦を引く
        Aim,   // 向きを調節
        Shoot, // 放つ
    }
    class KinectManager
    {
        #region Field

        public KinectSensor sensor;

        public static KinectManager getInstance;

        private SolidColorBrush playerColor, alphaColor;

        #region Motion

        private Skeleton skeleton;

        private ColorImagePoint[] cip = new ColorImagePoint[20];

        private Vector4[] poseSet, poseCharge, poseAim, poseShoot;

        private DispatcherTimer dispatcherTimer;

        private Line[] bones;

        private KinectState nowState;

        private int counter;

        #endregion

        #region Coordinate

        private const DepthImageFormat DepthFormat = DepthImageFormat.Resolution320x240Fps30;

        private const ColorImageFormat ColorFormat = ColorImageFormat.RawBayerResolution640x480Fps30;

        private WriteableBitmap colorBitmap;

        private WriteableBitmap playerOpacityMaskImage = null;

        private DepthImagePixel[] depthPixels;

        private byte[] colorPixels;

        private int[] playerPixelData;

        private ColorImagePoint[] colorCoordinates;

        private int colorToDepthDivisor;

        private int depthWidth;

        private int depthHeight;

        private int opaquePixelValue = -1;

        private Image MaskedImage;

        #endregion

        #region Game

        public Point fromShoot, toShoot;

        #endregion

        #endregion

        #region Method

        public bool Initialize() {
            // インスタンスを代入
            getInstance = this;

            // 接続されているセンサを捜索
            foreach (var potentialSensor in KinectSensor.KinectSensors)
            {
                if (potentialSensor.Status == KinectStatus.Connected)
                {
                    this.sensor = potentialSensor;
                    break;
                }
            }

            if (null != this.sensor)
            {
                // SkeletonStreamを有効にする
                this.sensor.SkeletonStream.Enable();

                // SkeletonStreamにイベントを追加
                this.sensor.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(sensor_SkeletonFrameReady);

                // Turn on the depth stream to receive depth frames
                this.sensor.DepthStream.Enable(DepthFormat);

                this.depthWidth = this.sensor.DepthStream.FrameWidth;

                this.depthHeight = this.sensor.DepthStream.FrameHeight;

                this.sensor.ColorStream.Enable(ColorFormat);

                int colorWidth = this.sensor.ColorStream.FrameWidth;
                int colorHeight = this.sensor.ColorStream.FrameHeight;

                this.colorToDepthDivisor = colorWidth / this.depthWidth;

                // Turn on to get player masks
                this.sensor.SkeletonStream.Enable();

                // Allocate space to put the depth pixels we'll receive
                this.depthPixels = new DepthImagePixel[this.sensor.DepthStream.FramePixelDataLength];

                // Allocate space to put the color pixels we'll create
                this.colorPixels = new byte[this.sensor.ColorStream.FramePixelDataLength];

                this.playerPixelData = new int[this.sensor.DepthStream.FramePixelDataLength];

                this.colorCoordinates = new ColorImagePoint[this.sensor.DepthStream.FramePixelDataLength];

                // This is the bitmap we'll display on-screen
                this.colorBitmap = new WriteableBitmap(colorWidth, colorHeight, 96.0, 96.0, PixelFormats.Bgr32, null);

                // Set the image we display to point to the bitmap where we'll put the image data
                // Image に設定する
                //this.MaskedImage.Source = this.colorBitmap;

                // Add an event handler to be called whenever there is new depth frame data
                this.sensor.AllFramesReady += this.SensorAllFramesReady;

                // センサを起動
                try
                {
                    this.sensor.Start();
                }
                catch (IOException)
                {
                    this.sensor = null;
                }
            }
            else {
                //CompositionTarget.Rendering += UpdateImage;
            }

            // インスタンス化
            this.skeleton = new Skeleton();

            // スレッドでの呼び出し優先度指定
            this.dispatcherTimer = new DispatcherTimer();

            // 1秒ごとに処理()
            this.dispatcherTimer.Interval = new TimeSpan(100000);

            // イベント追加
            this.dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);

            // タイマー動作開始
            //this.dispatcherTimer.Start();

            this.poseSet    = new Vector4[20];
            this.poseCharge = new Vector4[20];
            this.poseAim    = new Vector4[20];
            this.poseShoot  = new Vector4[20];

            try
            {
                this.poseSet    = MotionIO.LoadJoint("poseSet");
                this.poseCharge = MotionIO.LoadJoint("poseCharge");
                this.poseAim    = MotionIO.LoadJoint("poseAim");
                this.poseShoot  = MotionIO.LoadJoint("poseShoot");
            }
            catch (Exception ex)
            {
                Console.WriteLine("読み込み失敗");
            }

            playerColor = Brushes.ForestGreen;
            alphaColor = new SolidColorBrush(Color.FromArgb((byte)0, (byte)255, (byte)255, (byte)255));

            this.bones = new Line[20];
            for (int i = 0; i < this.bones.Length; i++)
            {
                this.bones[i] = new Line();

                // 描画設定
                this.bones[i].HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                this.bones[i].VerticalAlignment = System.Windows.VerticalAlignment.Top;
                this.bones[i].StrokeThickness = 4;
                this.bones[i].Stroke = playerColor;
                
                // Canvasに追加
                MainWindow.mainCanvas.Children.Add(this.bones[i]);
            }

            nowState = KinectState.None;

            fromShoot = new Point();
            toShoot = new Point();

            return true;
        }

        public void Closed() { 
            if (null != this.sensor)
            {
                this.sensor.Stop();
                this.sensor = null;
            }
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (this.skeleton.TrackingState == SkeletonTrackingState.Tracked)
            {
                // 内積を計算して類似度を取得
                int SetSimilarity = (int)(MyMath.Dot(JointType.ShoulderLeft, JointType.HandLeft, this.skeleton, this.poseSet) * 100);
                int ChargeSimilarity = (int)(MyMath.Dot(JointType.ShoulderLeft, JointType.HandLeft, this.skeleton, this.poseCharge) * 100);
                int AimSimilarity = (int)(MyMath.Dot(JointType.ShoulderLeft, JointType.HandLeft, this.skeleton, this.poseAim) * 100);
                int ShootSimilarity = (int)(MyMath.Dot(JointType.ShoulderLeft, JointType.HandLeft, this.skeleton, this.poseShoot) * 100);
                
                // 状態遷移
#if false
                switch (this.nowState)
                {
                    case State.None:

                        if (kamaeSimilarity >= this.slider1.Value)
                        {
                            this.counter++;

                            if (this.counter >= slider4.Value)
                            {
                                this.counter = 0;
                                this.nowState = State.Kamae;
                            }
                        }
                        break;

                    case State.Kamae:

                        if (pose1Similarity >= this.slider2.Value)
                            this.nowState = State.Up;

                        if (pose2Similarity >= this.slider3.Value)
                            this.nowState = State.Down;
                        break;

                    case State.Up:
                    case State.Down:

                        this.counter++;

                        if (this.counter > this.slider4.Value)
                        {
                            this.counter = 0;
                            this.nowState = State.None;
                        }

                        break;

                    default:
                        break;
                }

                // 表示
                this.label4.Content = kamaeSimilarity + " / " + this.slider1.Value.ToString("F0");
                this.label5.Content = pose1Similarity + " / " + this.slider1.Value.ToString("F0");
                this.label6.Content = pose2Similarity + " / " + this.slider1.Value.ToString("F0");
#endif                

                // 骨格を描画
                for (int i = 0; i < this.skeleton.Joints.Count; i++)
                {
                    // 3次元座標を2次元座標に変換
                    cip[i] = this.sensor.CoordinateMapper.MapSkeletonPointToColorPoint(this.skeleton.Joints[(JointType)i].Position, ColorImageFormat.RgbResolution640x480Fps30);

                    // 描画範囲に合わせ収まるように縮小
                    cip[i].X /= 4;
                    cip[i].Y /= 4;
                }

                // 骨格情報をセット
                // 今は上半身のみ
                this.SetBonePoint(0, 3, 2);
                this.SetBonePoint(1, 2, 4);
                this.SetBonePoint(2, 4, 5);
                this.SetBonePoint(3, 5, 6);
                this.SetBonePoint(4, 6, 7);
                this.SetBonePoint(5, 2, 8);
                this.SetBonePoint(6, 8, 9);
                this.SetBonePoint(7, 9, 10);
                this.SetBonePoint(8, 10, 11);
            }
            else
            {
#if false
                this.label4.Content = this.slider1.Value.ToString("F0");
                this.label5.Content = this.slider2.Value.ToString("F0");
                this.label6.Content = this.slider3.Value.ToString("F0");
                this.label8.Content = this.slider4.Value.ToString("F0") + "[ms]";
#endif
                MainWindow.getInstance.Shoot(800, 420, -50, -4);
            }

//            this.label9.Content = "認識状態:" + this.nowState.ToString();
        }

        #region Motion's

        private void sensor_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            Skeleton[] skeletons = new Skeleton[0];

            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    skeletonFrame.CopySkeletonDataTo(skeletons);
                }
            }

            if (skeletons.Length != 0)
            {
                foreach (Skeleton skl in skeletons)
                {
                    if (skl.TrackingState == SkeletonTrackingState.Tracked)
                        this.skeleton = skl;
                }
            }
        }

        /// <summary>
        /// 骨格情報をセットする
        /// </summary>
        /// <param name="boneNo">描画する骨格番号</param>
        /// <param name="p1">座標変換した骨格位置情報のインデックス1</param>
        /// <param name="p2">座標変換した骨格位置情報のインデックス2</param>
        private void SetBonePoint(int boneNo, int p1, int p2)
        {
            float frameX = 800;
            float frameY = 420;

            this.bones[boneNo].X1 = cip[p1].X + frameX;
            this.bones[boneNo].Y1 = cip[p1].Y + frameY;
            this.bones[boneNo].X2 = cip[p2].X + frameX;
            this.bones[boneNo].Y2 = cip[p2].Y + frameY;

            this.bones[boneNo].Stroke = playerColor;
        }

        private void SetBonePoint(int boneNo, Point p1, Point p2) { 
            float frameX = 800;
            float frameY = 420;

            frameX = frameY = 0;

            this.bones[boneNo].X1 = p1.X + frameX;
            this.bones[boneNo].Y1 = p1.Y + frameY;
            this.bones[boneNo].X2 = p2.X + frameX;
            this.bones[boneNo].Y2 = p2.Y + frameY;
        }

        public void RemoveBone() {
            for (int i = 0; i < bones.Length; i++) {
                bones[i].Stroke = alphaColor;
            }
        }

        private void MotionSetPoseSet()
        {
            MotionIO.SaveJoint("poseSet", this.skeleton);
            this.poseSet = MotionIO.LoadJoint("poseSet");
        }

        private void MotionSetPoseCharge()
        {
            MotionIO.SaveJoint("poseCharge", this.skeleton);
            this.poseCharge = MotionIO.LoadJoint("poseCharge");
        }

        private void MotionSetPoseAim()
        {
            MotionIO.SaveJoint("poseAim", this.skeleton);
            this.poseAim = MotionIO.LoadJoint("poseAim");
        }

        private void MotionSetPoseShoot()
        {
            MotionIO.SaveJoint("poseShoot", this.skeleton);
            this.poseShoot = MotionIO.LoadJoint("poseShoot");
        }

        #endregion;

        #region Coordinate's

        /// <summary>
        /// Event handler for Kinect sensor's DepthFrameReady event
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void SensorAllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            // in the middle of shutting down, so nothing to do
            if (null == this.sensor)
            {
                return;
            }

            bool depthReceived = false;
            bool colorReceived = false;

            using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
            {
                if (null != depthFrame)
                {
                    // Copy the pixel data from the image to a temporary array
                    depthFrame.CopyDepthImagePixelDataTo(this.depthPixels);

                    depthReceived = true;
                }
            }

            using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
            {
                if (null != colorFrame)
                {
                    // Copy the pixel data from the image to a temporary array
                    colorFrame.CopyPixelDataTo(this.colorPixels);

                    colorReceived = true;
                }
            }

            // do our processing outside of the using block
            // so that we return resources to the kinect as soon as possible
            if (true == depthReceived)
            {
                this.sensor.CoordinateMapper.MapDepthFrameToColorFrame(
                    DepthFormat,
                    this.depthPixels,
                    ColorFormat,
                    this.colorCoordinates);

                Array.Clear(this.playerPixelData, 0, this.playerPixelData.Length);

                // loop over each row and column of the depth
                for (int y = 0; y < this.depthHeight; ++y)
                {
                    for (int x = 0; x < this.depthWidth; ++x)
                    {
                        // calculate index into depth array
                        int depthIndex = x + (y * this.depthWidth);

                        DepthImagePixel depthPixel = this.depthPixels[depthIndex];

                        int player = depthPixel.PlayerIndex;

                        // if we're tracking a player for the current pixel, sets it opacity to full
                        if (player > 0)
                        {
                            // retrieve the depth to color mapping for the current depth pixel
                            ColorImagePoint colorImagePoint = this.colorCoordinates[depthIndex];

                            // scale color coordinates to depth resolution
                            int colorInDepthX = colorImagePoint.X / this.colorToDepthDivisor;
                            int colorInDepthY = colorImagePoint.Y / this.colorToDepthDivisor;

                            // make sure the depth pixel maps to a valid point in color space
                            // check y > 0 and y < depthHeight to make sure we don't write outside of the array
                            // check x > 0 instead of >= 0 since to fill gaps we set opaque current pixel plus the one to the left
                            // because of how the sensor works it is more correct to do it this way than to set to the right
                            if (colorInDepthX > 0 && colorInDepthX < this.depthWidth && colorInDepthY >= 0 && colorInDepthY < this.depthHeight)
                            {
                                // calculate index into the player mask pixel array
                                int playerPixelIndex = colorInDepthX + (colorInDepthY * this.depthWidth);

                                // set opaque
                                this.playerPixelData[playerPixelIndex] = opaquePixelValue;

                                // compensate for depth/color not corresponding exactly by setting the pixel 
                                // to the left to opaque as well
                                this.playerPixelData[playerPixelIndex - 1] = opaquePixelValue;
                            }
                        }
                    }
                }
            }

            // do our processing outside of the using block
            // so that we return resources to the kinect as soon as possible
/*            if (true == colorReceived)
            {
                // Write the pixel data into our bitmap
                this.colorBitmap.WritePixels(
                    new Int32Rect(0, 0, this.colorBitmap.PixelWidth, this.colorBitmap.PixelHeight),
                    this.colorPixels,
                    this.colorBitmap.PixelWidth * sizeof(int),
                    0);

                if (this.playerOpacityMaskImage == null)
                {
                    this.playerOpacityMaskImage = new WriteableBitmap(
                        this.depthWidth,
                        this.depthHeight,
                        96,
                        96,
                        PixelFormats.Bgra32,
                        null);

#if true
                    MaskedImage.OpacityMask = new ImageBrush { ImageSource = this.playerOpacityMaskImage };
#endif
                }

                this.playerOpacityMaskImage.WritePixels(
                    new Int32Rect(0, 0, this.depthWidth, this.depthHeight),
                    this.playerPixelData,
                    this.depthWidth * ((this.playerOpacityMaskImage.Format.BitsPerPixel + 7) / 8),
                    0);
            }*/
        }

        /// <summary>
        /// Handles the user clicking on the screenshot button
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void ButtonScreenshotClick(object sender, RoutedEventArgs e)
        {
            if (null == this.sensor)
            {
                //this.statusBarText.Text = Properties.Resources.ConnectDeviceFirst;
                return;
            }

            int colorWidth = this.sensor.ColorStream.FrameWidth;
            int colorHeight = this.sensor.ColorStream.FrameHeight;

            // create a render target that we'll render our controls to
            RenderTargetBitmap renderBitmap = new RenderTargetBitmap(colorWidth, colorHeight, 96.0, 96.0, PixelFormats.Pbgra32);

            DrawingVisual dv = new DrawingVisual();
            using (DrawingContext dc = dv.RenderOpen())
            {
                // render the backdrop
                //VisualBrush backdropBrush = new VisualBrush(Backdrop);
                //dc.DrawRectangle(backdropBrush, null, new Rect(new Point(), new Size(colorWidth, colorHeight)));

                // render the color image masked out by players
                //VisualBrush colorBrush = new VisualBrush(MaskedColor);
                //dc.DrawRectangle(colorBrush, null, new Rect(new Point(), new Size(colorWidth, colorHeight)));
            }

            renderBitmap.Render(dv);
    
            // create a png bitmap encoder which knows how to save a .png file
            BitmapEncoder encoder = new PngBitmapEncoder();

            // create frame from the writable bitmap and add to encoder
            encoder.Frames.Add(BitmapFrame.Create(renderBitmap));

            string time = System.DateTime.Now.ToString("hh'-'mm'-'ss", CultureInfo.CurrentUICulture.DateTimeFormat);

            string myPhotos = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

            string path = System.IO.Path.Combine(myPhotos, "KinectSnapshot-" + time + ".png");

            // write the new file to disk
            try
            {
                using (FileStream fs = new FileStream(path, FileMode.Create))
                {
                    encoder.Save(fs);
                }

                //this.statusBarText.Text = string.Format(CultureInfo.InvariantCulture, "{0} {1}", Properties.Resources.ScreenshotWriteSuccess, path);
            }
            catch (IOException)
            {
                //this.statusBarText.Text = string.Format(CultureInfo.InvariantCulture, "{0} {1}", Properties.Resources.ScreenshotWriteFailed, path);
            }
        }
        
        /// <summary>
        /// Handles the checking or unchecking of the near mode combo box
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        public void ChangeNearMode(bool e)
        {
            if (this.sensor != null)
            {
                // will not function on non-Kinect for Windows devices
                try
                {
                    if (e)
                    {
                        this.sensor.DepthStream.Range = DepthRange.Near;
                    }
                    else
                    {
                        this.sensor.DepthStream.Range = DepthRange.Default;
                    }
                }
                catch (InvalidOperationException)
                {
                }
            }
        }

        #endregion

        #endregion

        #region OpenMethod
        
        public KinectState input()
        {
            if (this.skeleton.TrackingState == SkeletonTrackingState.Tracked)
            {
                // 内積を計算して類似度を取得
                // 構え１左肩→左手・左肘→左手・右肩→右手・右肘→右手
                int SetSimilarity    = (int)(
                    MyMath.Dot(JointType.ShoulderLeft, JointType.HandLeft, this.skeleton, this.poseSet) * 30
                    + MyMath.Dot(JointType.ElbowLeft , JointType.HandLeft, this.skeleton, this.poseSet) * 33
                    + MyMath.Dot(JointType.ShoulderRight, JointType.HandRight, this.skeleton, this.poseSet) * 20
                    + MyMath.Dot(JointType.ElbowRight   , JointType.HandRight, this.skeleton, this.poseSet) * 17
                    );
                // 構え２(引くまで)たぶん使わない
                int ChargeSimilarity = (int)(MyMath.Dot(JointType.ShoulderLeft, JointType.HandLeft, this.skeleton, this.poseCharge) * 100);
                // 狙いをつける　これもたぶん使わない
                int AimSimilarity    = (int)(MyMath.Dot(JointType.ShoulderLeft, JointType.HandLeft, this.skeleton, this.poseAim) * 100);
                // 放つ　たぶんこいつも・・・
                int ShootSimilarity  = (int)(MyMath.Dot(JointType.ShoulderLeft, JointType.HandLeft, this.skeleton, this.poseShoot) * 100);                

                // 骨格を描画
                for (int i = 0; i < this.skeleton.Joints.Count; i++)
                {
                    // 3次元座標を2次元座標に変換
                    cip[i] = this.sensor.CoordinateMapper.MapSkeletonPointToColorPoint(this.skeleton.Joints[(JointType)i].Position, ColorImageFormat.RgbResolution640x480Fps30);

                    // 描画範囲に合わせ収まるように縮小
                    cip[i].X /= 4;
                    cip[i].Y /= 4;
                }

                // 骨格情報をセット
                // 今は上半身のみ
                this.SetBonePoint(0, 3, 2);   // 頭→肩中
                this.SetBonePoint(1, 2, 4);   // 肩中→左肩
                this.SetBonePoint(2, 4, 5);   // 左肩→左肘
                this.SetBonePoint(3, 5, 6);   // 左肘→左手
                this.SetBonePoint(4, 6, 7);   // 左手→？
                this.SetBonePoint(5, 2, 8);   // 肩中→右肩
                this.SetBonePoint(6, 8, 9);   // 右肩→右肘
                this.SetBonePoint(7, 9, 10);  // 右肘→右手
                this.SetBonePoint(8, 10, 11); // 右手→？
                this.SetBonePoint(9, 2, 1);   // 肩中→腰
                this.SetBonePoint(10, 1, 0);  // 腰→尻
                this.SetBonePoint(11, 0, 12); // 尻→左尻
                this.SetBonePoint(12, 12, 13);// 左尻→左膝
                this.SetBonePoint(13, 13, 14);// 左膝→左足
                this.SetBonePoint(14, 14, 15);// 
                this.SetBonePoint(15, 0, 16); // 尻→右尻
                this.SetBonePoint(16, 16, 17);// 右尻→右膝
                this.SetBonePoint(17, 17, 18);// 右膝→右足
                this.SetBonePoint(18, 18, 19);// 

                this.bones[19].Stroke = alphaColor;
                // 状態遷移
#if true
                switch (this.nowState)
                {
                    case KinectState.Other:
                        this.nowState = KinectState.None;
                        Bow.getInstance.Idling(new Point(bones[4].X1, bones[4].Y1));
                        break;
                    case KinectState.None:
                        Bow.getInstance.Idling(new Point(bones[4].X1, bones[4].Y1));

                        if (SetSimilarity >= 90)
                        {
                            this.counter++;

                            if (this.counter >= 20)
                            {
                                this.counter = 5;
                                this.nowState = KinectState.Set;
                            }
                        }
                        else {
                            this.counter = 0;
                        }
                        break;

                    case KinectState.Set:
                        Bow.getInstance.Drawing(new Point(bones[4].X1, bones[4].Y1), new Point(bones[8].X1, bones[8].Y1));
                        // 左肘が伸びているか
                        if (MyMath.Dot(new Point(bones[3].X1, bones[3].Y1), new Point(bones[4].X1, bones[4].Y1), new Point(bones[2].X1, bones[2].Y1)) < -0.4f) { 
                            // 右肘が曲がっているか
                            if (MyMath.Dot(new Point(bones[7].X1, bones[7].Y1), new Point(bones[8].X1, bones[8].Y1), new Point(bones[4].X1, bones[4].Y1)) > 0.1f) { 
                                // 現在の位置を保存する
                                fromShoot.X = bones[8].X1; fromShoot.Y = bones[8].Y1;
                                toShoot.X = bones[4].X1; toShoot.Y = bones[4].Y1;

                                //this.SetBonePoint(19, 6, 10);
                                this.SetBonePoint(19, toShoot, fromShoot);

                                counter = (counter < 10) ? 10 : counter;
                            }
                        }
                        else {
                            counter--;
                            if (counter < 0) {
                                counter = 0;
                                this.nowState = KinectState.None;
                            }
                        }

#if false
                        if (MyMath.PointToLength(fromShoot, toShoot) > 34) {
                            counter++;
                            if (counter > 30) {
                                this.counter = 5;
                                this.nowState = State.Charge;
                                //MainWindow.getInstance.Shoot((float)toShoot.X, (float)toShoot.Y, (float)(toShoot.X - fromShoot.X) * 2.0f, (float)(toShoot.Y - fromShoot.Y) * 2.0f);
                            }
                        }
#else
                        if (MyMath.PointToLength(fromShoot, toShoot) > 
                            MyMath.PointToLength(
                            new Point(bones[3].X1, bones[3].Y1)
                            , new Point(bones[3].X2, bones[3].Y2)
                            ) * 3.1f) 
                        {
                            counter++;
                            if (counter > 30) {
                                this.counter = 5;
                                this.nowState = KinectState.Charge;
                                //MainWindow.getInstance.Shoot((float)toShoot.X, (float)toShoot.Y, (float)(toShoot.X - fromShoot.X) * 2.0f, (float)(toShoot.Y - fromShoot.Y) * 2.0f);
                            }
                        }
#endif

                        break;

                    case KinectState.Charge:
                        Bow.getInstance.Drawing(new Point(bones[4].X1, bones[4].Y1), new Point(bones[8].X1, bones[8].Y1));
                        //float d = -1.0f;

                        //d = MyMath.PointToLength(fromShoot, new Point(bones[8].X1, bones[8].Y1));
                        float fsL, tsL;
                        tsL = MyMath.PointToLength(toShoot, new Point(bones[4].X1, bones[4].Y1));
                        fsL = MyMath.PointToLength(fromShoot, new Point(bones[8].X1, bones[8].Y1));
                        
                        if (MyMath.Dot(new Point(bones[3].X1, bones[3].Y1), new Point(bones[4].X1, bones[4].Y1), new Point(bones[2].X1, bones[2].Y1)) < -0.5f) { 
                            // 右肘が曲がっているか
                            if (MyMath.Dot(new Point(bones[7].X1, bones[7].Y1), new Point(bones[8].X1, bones[8].Y1), new Point(bones[4].X1, bones[4].Y1)) > 0.26f) { 
                                // 現在の位置を保存する
                                //float fsL, tsL;
                                if (tsL > 0.9f && fsL > 0.6f && fsL < 6.5f) {
                                    // 動いた場合に移動する処理を行う
                                    fromShoot.X = bones[8].X1; fromShoot.Y = bones[8].Y1;
                                    toShoot.X = bones[4].X1; toShoot.Y = bones[4].Y1;
                                    counter = 20;
                                }
                                else {
                                    // 動かなかった場合にインクリメント
                                    counter++;
                                }
                                //this.SetBonePoint(19, 6, 10);
                                //this.SetBonePoint(19, toShoot, fromShoot);

                                counter = (counter < 20) ? 20 : counter;
                            }
                        }
                        else {
                            counter--;
                            if (counter < 0) {
                                counter = 0;
                                this.nowState = KinectState.None;
                            }
                        }

                        if (counter > 100) {
                            this.nowState = KinectState.Aim;
                        }
                        if (fsL > 4.9f && tsL < 3.0f) {
                            this.nowState = KinectState.Shoot;
                        }

                        break;
                    case KinectState.Aim:
                        Bow.getInstance.Drawing(new Point(bones[4].X1, bones[4].Y1), new Point(bones[8].X1, bones[8].Y1));
                        float a = -1.0f;

                        a = MyMath.PointToLength(fromShoot, new Point(bones[8].X1, bones[8].Y1));


                        if (MyMath.Dot(new Point(bones[2].X1, bones[2].Y1), new Point(bones[2].X1, bones[2].Y1 + 2.0f), new Point(bones[4].X1, bones[4].Y1)) < -0.5f) {
                            counter = 0;
                            this.nowState = KinectState.None;
                        }

                        // 左肘が伸びているか
                        if (MyMath.Dot(new Point(bones[3].X1, bones[3].Y1), new Point(bones[4].X1, bones[4].Y1), new Point(bones[2].X1, bones[2].Y1)) < -0.5f) { 
                            // 右肘が曲がっているか
                            if (MyMath.Dot(new Point(bones[7].X1, bones[7].Y1), new Point(bones[8].X1, bones[8].Y1), new Point(bones[4].X1, bones[4].Y1)) > 0.26f) { 
                                // 現在の位置を保存する
                                //float fsL, tsL;
                                fsL = a;
                                if ((tsL = MyMath.PointToLength(toShoot, new Point(bones[4].X1, bones[4].Y1))) > 3.2f) {
                                    fromShoot.X = bones[8].X1; fromShoot.Y = bones[8].Y1;
                                    toShoot.X = bones[4].X1; toShoot.Y = bones[4].Y1;
                                    this.nowState = KinectState.Charge;
                                }
                                else {
                                    counter++;
                                }
                                //this.SetBonePoint(19, 6, 10);
                                //this.SetBonePoint(19, toShoot, fromShoot);

                                //counter++;
                            }
                        }
                        else {
                            counter = 0;
                            this.nowState = KinectState.None;
                        }

#if false
                        if (this.counter > 60)
                        {
                            this.counter = 0;
                            this.nowState = State.None;
                            MainWindow.getInstance.Shoot((float)toShoot.X, (float)toShoot.Y, (float)(toShoot.X - fromShoot.X) * 2.0f, (float)(toShoot.Y - fromShoot.Y) * 2.0f);
                        }
#else
                        if (counter > 1 && a < 1.8f) { 
                            this.counter = 0;
                            this.nowState = KinectState.Shoot;
                            //MainWindow.getInstance.Shoot((float)toShoot.X, (float)toShoot.Y, (float)(toShoot.X - fromShoot.X) * 2.0f, (float)(toShoot.Y - fromShoot.Y) * 2.0f);
                        }
#endif

                        break;

                    case KinectState.Shoot:
                        Bow.getInstance.Drawing(new Point(bones[4].X1, bones[4].Y1), new Point(bones[8].X1, bones[8].Y1));
                        this.counter = 0;
                        this.nowState = KinectState.None;
                        MainWindow.getInstance.Shoot((float)toShoot.X, (float)toShoot.Y, (float)(toShoot.X - fromShoot.X) * 2.0f, (float)(toShoot.Y - fromShoot.Y) * 2.0f);
                        break;

                    default:
                        break;
                }
#endif                

            }
            else
            {
#if false
                this.label4.Content = this.slider1.Value.ToString("F0");
                this.label5.Content = this.slider2.Value.ToString("F0");
                this.label6.Content = this.slider3.Value.ToString("F0");
                this.label8.Content = this.slider4.Value.ToString("F0") + "[ms]";
#endif
//                MainWindow.getInstance.Shoot(800, 420, -50, -4);

                RemoveBone();
                Bow.getInstance.unDraw();
                return KinectState.Other;
            }

//            this.label9.Content = "認識状態:" + this.nowState.ToString();
            return nowState;
        }

        public void Reset() {
            nowState = KinectState.Other;

            RemoveBone();
            counter = 0;
        }

        #endregion
    }
}
