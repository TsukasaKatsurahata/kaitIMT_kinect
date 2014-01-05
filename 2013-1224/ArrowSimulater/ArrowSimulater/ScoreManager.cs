///***   1123151  関 紘太郎 さんによって作成  ***///
/// 処理は主に関さんが記述。
/// スコアのソートするリストを利用するための処理の追加や
/// 外部ファイルへの出力する記述などは1123157 桂畑が記述

using System;
using System.Windows;

//getinstance実装工事中
namespace ArrowSimulater
{
    class ScoreManager
    {
        public static string SaveName = "scoreList";

        public static ScoreManager getInstance;

        //得点記録用数値Pを用意
        public int Counter;  // 現在のプレイヤーの記録
        public int Ranking;

        // ソートではなくて、Socreクラスによるリスト構造にすると効率が良いかもしれない
        public Score scoreRoot;

        // ファイル書き出しは配列の方が都合が良さそうなので雑用変数として書き出すメソッドを作る
        public int[] scoreList;

        public void Initialize() {
            getInstance = this;

            Counter = 0;

            scoreRoot = new Score(0x7FFFFFFF); // ルート（スコアとして換算はしないが重要な役割も持つ）：int型の最大の数値を代入する

            scoreRoot.Add(FileIO.LoadScore(SaveName));
        }
        
        //的を射抜いた得点を加算
        public int PointInc(int point)
        {
            Counter = Counter + point;
            return Counter;
        }


        //カウンタリセット //静的変数の開放がいまいちわかんなかったので調整求む
        public void PointDeleter() {
            Counter = 0;
        }
        
        

        //以下現在編集中

        //静的関数Rankingにランキング値を入力、点数をソートして配列を返す
        public int[] ScoreSort(int score)
        {
            scoreList = scoreRoot.ScoreList();

            int R = 0;
            int checker = score; //checkerにscoreの値をコピー
            for (int n = 0; n < scoreList.Length; n++)
            {
                //点数をソート
                if (scoreList[n] < score)
                {
                    int T = scoreList[n];
                    scoreList[n] = score;
                    score = T;
                }

                //ソート時にscoreの値が変動するため、比較する際に
                //checker(score初期値)とscoreの中身が同じ時のみ順位をカウントする
                if (checker < scoreList[n] && checker == score)
                {
                    R++;
                }
            }

            // ScoreRootの子に代入された得点を保存する
            scoreRoot.Add(new Score(checker));

            // ファイルとして書き出す
            FileIO.SaveScore(SaveName, scoreRoot.ScoreList(), true);
            FileIO.SaveScore(SaveName, scoreRoot.ScoreList(), false);

            scoreList = scoreRoot.ScoreList();

            //Rankingに数値代入、ソートし直した配列を返還
            Ranking = R + 1;
            return scoreList;
        }

        // 現在の得点を文字列化するための変数
        public int[] ToStringInt() {
            int[] res;
            int index = 0;
            int cal = 1;
            // 10進法における桁数を算出
            while (Counter / cal > 0) {
                index++;
                cal *= 10;
            }
            if (index < 1) {
                index = 1;
            }
            res = new int[index];

            cal = Counter;
            for (int i = 0; i < index; i++) {
                res[i] = cal % 10;
                cal /= 10;
            }

            return res;
        }
        // こちらはランキング用
        public int[] ToStringInt(int setPoint) {
            int[] res;
            int index = 0;
            int cal = 1;
            // 10進法における桁数を算出
            while (setPoint / cal > 0) {
                index++;
                cal *= 10;
            }
            if (index < 1) {
                index = 1;
            }
            res = new int[index];

            cal = setPoint;
            for (int i = 0; i < index; i++) {
                res[i] = cal % 10;
                cal /= 10;
            }

            return res;
        }

        public void DrawScore() {
            int[] point = this.ToStringInt();
            SpriteManager.getInstance.Draw(SpriteManager.images[12], new Point(850, 8), new Point(100, 360));
            for (int i = point.Length - 1; i >= 0; i--) { 
                SpriteManager.getInstance.Draw(13 + point[i], new Point(868, 158 + 30 * (point.Length - 1 - i)));
            }
        }

        public void DrawRanking()
        { 
            int[] sL = scoreRoot.ScoreList();
            int[] point;
            for (int k = 0; k < 8 && k < sL.Length; k++) {
                point = this.ToStringInt(sL[k]);
                for (int i = point.Length - 1; i >= 0; i--) {
                    SpriteManager.getInstance.Draw(13 + point[i], new Point(715 - 80 * k, 270 + 30 * (point.Length - 1 - i)), new Point(16, 0));
                }
            }
            if (Ranking > 8) {
                point = this.ToStringInt(Ranking);
                for (int i = point.Length - 1; i >= 0; i--) {
                    SpriteManager.getInstance.Draw(13 + point[i], new Point(65, 111 + 30 * (point.Length - 1 - i)), new Point(16, 0), 0.94f);
                }
                point = this.ToStringInt(sL[Ranking]);
                for (int i = point.Length - 1; i >= 0; i--) {
                    SpriteManager.getInstance.Draw(13 + point[i], new Point(65, 270 + 30 * (point.Length - 1 - i)), new Point(16, 0), 0.94f);
                }
            }
        }

    }
}
