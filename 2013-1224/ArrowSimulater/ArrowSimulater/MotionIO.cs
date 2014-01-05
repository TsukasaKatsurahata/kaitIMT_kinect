/*
 * 神奈川工科大学　情報学部　情報メディア学科
 * 1123157　桂畑 司
 * 2013/12/24
 */
/*
 * ジェスチャー入力の情報を保存・読み込みするためのクラス
 * 結局読み取り以外使わなかった
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Kinect;

namespace ArrowSimulater
{
    class MotionIO
    {
        public static void SaveJoint(String name, Skeleton skl)
        {
            using (StreamWriter sw = new StreamWriter(name + ".csv"))
            {
                foreach (Joint joint in skl.Joints)
                    sw.Write(joint.Position.X + "," + joint.Position.Y + "," + joint.Position.Z + "\n");

                sw.Close();
            }
        }

        public static Vector4[] LoadJoint(String name)
        {
            Vector4[] jointPos = new Vector4[20];

            using (StreamReader sr = new StreamReader(name + ".csv"))
            {
                // すべての文字列を読み込み
                string str = sr.ReadToEnd();

                // 文字列を指定した文字で区切り分割する
                string[] buff = str.Split(new char[] { ',', '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);

                for (int i = 0; i < jointPos.Length; i++)
                {
                    jointPos[i].X = float.Parse(buff[i * 3]);
                    jointPos[i].Y = float.Parse(buff[i * 3 + 1]); ;
                    jointPos[i].Z = float.Parse(buff[i * 3 + 2]); ;
                }

                sr.Close();
            }

            return jointPos;
        }
    }
}
