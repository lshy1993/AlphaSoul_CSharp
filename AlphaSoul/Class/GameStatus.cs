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
        /// 东南西北的各家ID
        /// </summary>
        public int[] playerWind;

        /// <summary>
        /// 场风
        /// </summary>
        public int changfeng;

        /// <summary>
        /// 储存场棒
        /// </summary>
        public int changbang;

        /// <summary>
        /// 储存立直棒
        /// </summary>
        public int lizhibang;

        /// <summary>
        /// 4家分数
        /// </summary>
        public int[] score;

        /// <summary>
        /// 宝牌
        /// </summary>
        public List<string> bao;
        /// <summary>
        /// 里宝牌
        /// </summary>
        public List<string> libao;

        public int[] zifeng;
        public int[] lizhi;
        public bool[] yifa;
        public int[] maxlian;
        public int[] xunshu;

        private int liannum;

        /// <summary>
        /// 是否是两立直状态
        /// </summary>
        public bool[] diyizimo = new bool[4];

        // 总局数
        public int jushu;
        public static string[] windStr = new string[4] { "东", "南", "西", "北" };

        public GameStatus()
        {
            playerOrder = new int[4] { 0, 1, 2, 3 };
            qinjia = 0;
            playerWind = ShiftOrder(qinjia);
            changfeng = 0;
            jushu = 1;
            changbang = 0;
            lizhibang = 0;
            score = new int[4] { 25000, 25000, 25000, 25000 };
            zifeng = new int[4] { 0, 1, 2, 3 };
            lizhi = new int[4];
            yifa = new bool[4];
            maxlian = new int[4];
            xunshu = new int[4];
            bao = new List<string>();
            libao = new List<string>();

            for (int i = 0; i < 4; i++)
            {
                int aid = playerWind[i];
                zifeng[aid] = i;
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

        private int[] ShiftOrder(int qinjia)
        {
            // 例qinjia=0, 右移0次
            Queue<int> res = new Queue<int>(playerOrder);
            //res = new Queue<int>(res.Reverse());
            for (int n = 0; n < qinjia; n++)
            {
                int temp = res.Dequeue();
                res.Enqueue(temp);
            }
            return res.ToArray();
        }

        /// <summary>
        /// 下一风场
        /// </summary>
        public void NextPlayer(bool lianzhuang)
        {
            if (!lianzhuang)
            {
                // 记录最大连庄
                int aid = playerWind[0];
                maxlian[aid] = Math.Max(maxlian[aid], liannum);
                liannum = 0;
                // 决定新序列
                qinjia++;
                if (qinjia > 3)
                {
                    qinjia = 0;
                    changfeng++;
                }
                playerWind = ShiftOrder(qinjia);
                changbang = 0;
            }
            else
            {
                liannum++;
                changbang++;
            }
            // 更新玩家的自风
            for (int i = 0; i < 4; i++)
            {
                int aid = playerWind[i];
                zifeng[aid] = i;
            }
            // 重置第一自摸立直
            lizhi = new int[] { 0, 0, 0, 0 };
            diyizimo = new bool[] { true, true, true, true };
            yifa = new bool[] { false, false, false, false };
            xunshu = new int[4];
            jushu++;
        }

        public PtParam getPlayerParam(int aid)
        {
            PtParam pt = new PtParam(this, zifeng[aid]);
            pt.lizhi = lizhi[aid];
            pt.yifa = yifa[aid];
            return pt;
        }


        public string GetPtList()
        {
            return string.Format("[{0},{1},{2},{3}]", score[0], score[1], score[2], score[3]);
        }

        public string GetWind()
        {
            return string.Format("{0}{1}局", windStr[changfeng], qinjia + 1);
        }

        public string GetAIOrder()
        {
            string str = "AI：";
            for (int i = 0; i < 4; i++)
            {
                str += playerWind[i] + " ";
            }
            return str;
        }

        public string GetWindOrder()
        {
            string str = "";
            for(int i = 0; i < 4; i++)
            {
                str += string.Format("{0}：{1} ", windStr[i], playerWind[i]);
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
