using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlphaSoul
{
    /// <summary>
    /// 玩家共享的对局信息
    /// </summary>
    public class GameStatus
    {
        /// <summary>
        /// 4家原始顺序 [2,4,3,1] 分别代表 AI2 4 3 1
        /// </summary>
        public int[] playerOrder;

        /// <summary>
        /// 当前庄家位置
        /// </summary>
        public int qinjia;

        /// <summary>
        /// 东南西北的各家
        /// </summary>
        public int[] playerWind;

        // 场风
        public int changfeng;
        // 总局数
        public int jushu;
        // 储存场棒
        public int changbang;
        // 储存立直棒
        public int lizhibang;
        // 4家分数
        public int[] score;
        // 宝牌 里宝牌
        public List<string> bao;
        public List<string> libao;

        // 每个玩家单独信息
        public PtParam[] playerParam = new PtParam[4];

        public static string[] windStr = new string[4] { "东", "南", "西", "北" };

        public GameStatus()
        {
            playerOrder = InitOrder();
            //qinjia = new Random().Next(4);
            qinjia = 0;
            // 例qinjia=2, 左移2次
            playerWind = (int[])playerOrder.Clone();
            for (int n = 0; n < qinjia; n++)
            {
                int temp = playerWind[0];
                for (int i = 1; i < 4; i++)
                {
                    playerWind[i - 1] = playerWind[i];
                }
                playerWind[3] = temp;
            }
            changfeng = 0;
            jushu = 1;
            changbang = 0;
            lizhibang = 0;
            score = new int[4] { 25000, 25000, 25000, 25000 };
            bao = new List<string>();
            libao = new List<string>();

            for (int i = 0; i < 4; i++)
            {
                playerParam[i] = new PtParam(ref changfeng, playerWind[i], ref bao, ref libao, ref changbang, ref lizhibang);
            }
        }

        private int[] InitOrder()
        {
            // 随机分配坐位
            Random rd = new Random();
            int[] resArr = new int[4] { 0, 1, 2, 3 };
            for (int i = 3; i >= 0; i--)
            {
                int k = rd.Next(i + 1);
                int temp = resArr[k];
                resArr[k] = resArr[i];
                resArr[i] = temp;
            }
            return resArr;
        }

        /// <summary>
        /// 换庄家
        /// </summary>
        public void NextPlayer()
        {
            qinjia++;
            if (qinjia > 3)
            {
                qinjia = 0;
                changfeng++;
            }
            for (int n = 0; n < qinjia; n++)
            {
                int temp = playerWind[0];
                for (int i = 1; i < 4; i++)
                {
                    playerWind[i - 1] = playerWind[i];
                }
                playerWind[3] = temp;
            }
            jushu++;
        }

        public string GetWind()
        {
            return string.Format("{0}{1}局", windStr[changfeng], jushu);
        }

        public string GetAIOrder()
        {
            string str = "AI：";
            for (int i = 0; i < 4; i++)
            {
                str += playerOrder[i] + " ";
            }
            return str;
        }

        public string GetWindOrder()
        {
            string str = "";
            for(int i = 0; i < 4; i++)
            {
                str += windStr[i] + ": " + playerWind[i];
            }
            return str;
        }

        public string GetBao(bool li = false)
        {
            string str = (li ? "里" : "") + "宝牌：";
            List<string> strlist = li ? libao : bao;
            foreach (string bao in strlist)
            {
                str += bao + " ";
            }
            return str;
        }
    }

}
