/*
 * 神奈川工科大学　情報学部　情報メディア学科
 * 1123157　桂畑 司
 * 2013/12/24
 */
/*
 * スコアのソートをリストで管理するためのクラス
 * 相互パスにしておくことで、リストの間に内部で代入することが可能である
 * リストを下っていき、適する場所に代入する
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArrowSimulater
{
    class Score
    {
        private int point;

        public Score next; // リストの次
        public Score back; // リストの前

        private static int index = 0; // 無限ループ回避のための雑用変数

        public int score { get { return point; } set { } }

        public Score() {
            point = 0;
            next = null;
            back = null;
        }
        public Score(int di) {
            point = di;
            next = null;
            back = null;
        }

        public bool Add(Score New) {
            // 無限ループの原因となる全く同じインスタンスが来た場合はスルーする
            if (this.Equals(New)) return false;

            // 自身よりも追加する物の方が高い場合
            // 手前に代入する
            if (New.score > this.score) {
                this.back.next = New;
                New.back = this.back;
                New.next = this;
                this.back = New;
                index = -1;
                // 代入した場合はtrueを返す
                return true;
            }
            // 自身以下の場合は後に追加する
            else { 
                // 次のオブジェクトがない場合はそのまま代入する
                if (next == null) {
                    this.next = New;
                    New.back = this;
                    New.next = null;
                    index = -1;
                    return true;
                }
                else {
                    index++;
                    if (index > 1000) return false;
                    if (this.next.Add(New))
                        return true;
                }
            }

            return false;
        }

        // リストを最後まで辿る(一番上のリストから作成)
        public int[] ScoreList() {
            if (index < 0) index = 0;

            // 雑用変数
            int[] sL = new int[1];

            // とりあえず、最後まで辿る
            Score SL = this.next;
            while (SL != null) {
                index++;
                SL = SL.next;
            }
            if (index == 0) {
                sL[0] = this.score;
                return sL;
            }

            sL = new int[index];
            index = 0;
            SL = this.next;
            while (SL != null) {
                sL[index] = SL.score;
                index++;
                SL = SL.next;
            }
            index = -1;

            return sL;
        }

        // 読み込んだデータからリストを作る
        public void Add(int[] sL) {
            for (int i = 0; i < sL.Length; i++) {
                this.Add(new Score(sL[i]));
            }
        }
    }
}
