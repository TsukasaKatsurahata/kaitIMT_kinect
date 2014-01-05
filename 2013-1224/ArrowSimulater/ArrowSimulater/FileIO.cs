///***  1123151  関 純太郎 さんによって作成  ***///

using System;
using System.IO;
using System.Globalization;


namespace ArrowSimulater
{
    public static class FileIO
    {
        
         public static void SaveScore(String name, int[] ScoreList, bool DateAdd)
        {
            using (StreamWriter sw = new StreamWriter(name + ((DateAdd) ? System.DateTime.Now.ToString("yyyy'-'MM'-'dd'-'HH'-'mm", CultureInfo.CurrentUICulture.DateTimeFormat) : "") + ".csv"))
            {
                foreach (int Score in ScoreList)
                    sw.Write(Score + "\n");
                sw.Close();
            }
        }

        public static int[] LoadScore(String name)
        {
            int[] ScoreList;// = new int[20];

            using (StreamReader sr = new StreamReader(name + ".csv"))
            {
                // すべての文字列を読み込み
                string str = sr.ReadToEnd();

                // 文字列を指定した文字で区切り分割する
                string[] buff = str.Split(new char[] { '\n' }, System.StringSplitOptions.RemoveEmptyEntries);

                ScoreList = new int[buff.Length];

                for (int i = 0; i < ScoreList.Length; i++)
                {
                    ScoreList[i] = int.Parse(buff[i]);
                }

                sr.Close();
            }

            return ScoreList;
        }
        
    }
}
