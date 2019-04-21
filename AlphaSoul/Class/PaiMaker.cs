using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlphaSoul
{

    /// <summary>
    /// 牌山生成
    /// </summary>
    public static class PaiMaker
    {
        //原始序列
        //private static int[] oriArr = new int[136];
        //生成序列
        //private int[] resArr = new int[136];
        //类序列
        //private ObservableCollection<Pai> resList = new ObservableCollection<Pai>();

        private static Random rd;

        public static ObservableCollection<Pai> ShufflePai()
        {
            ObservableCollection<Pai> resList = new ObservableCollection<Pai>();
            int[] resArr = Shuffle();
            // 生成牌
            for (int i = 0; i < 136; i++)
            {
                int p = resArr[i];
                int k = p % 34;
                // 初始宝牌设定
                if (p == 4 || p == 13 || p == 22)
                {
                    // 第一张5万5筒5索
                    resList.Add(new Pai(k, true));
                }
                else
                {
                    Pai pp = new Pai(k);
                    // 默认王牌
                    if (i >= 136 - 14) pp.stat = 9;
                    resList.Add(pp);
                }

            }
            return resList;
        }

        //随机打乱牌
        private static int[] Shuffle()
        {
            int[] oriArr = new int[136];
            for (int i = 0; i < 136; i++)
            {
                oriArr[i] = i;
            }
            int[] resArr = (int[])oriArr.Clone();
            rd = new Random();
            for (int i = 135; i > 0; i--)
            {
                int k = rd.Next(i + 1);
                int temp = resArr[k];
                resArr[k] = resArr[i];
                resArr[i] = temp;
            }
            return resArr;
        }

        /// <summary>
        /// 手牌统计
        /// </summary>
        /// <param name="plist"></param>
        /// <returns></returns>
        public static Dictionary<char, int[]> GetCount(List<Pai> plist)
        {
            Dictionary<char, int[]> paiCount = new Dictionary<char, int[]>();
            paiCount.Add('m', new int[10]);
            paiCount.Add('p', new int[10]);
            paiCount.Add('s', new int[10]);
            paiCount.Add('z', new int[8]);
            foreach (Pai p in plist)
            {
                // 红宝计算2次
                if (p.num == 0) paiCount[p.type][5] += 1;
                paiCount[p.type][p.num] += 1;
            }
            return paiCount;
        }


        /// <summary>
        /// 获取牌代码
        /// </summary>
        /// <param name="plist"></param>
        public static string GetCode(List<Pai> plist)
        {
            string codeStr = "";
            foreach (Pai p in plist)
            {
                codeStr += p.code;
            }
            return codeStr;
        }

        public static List<string> GetCodeList(List<Pai> plist)
        {
            List<string> codeStr = new List<string>();
            foreach (Pai p in plist)
            {
                codeStr.Add(p.code);
            }
            return codeStr;
        }

        /// <summary>
        /// 获取牌面
        /// </summary>
        /// <param name="plist"></param>
        public static string GetDisp(List<Pai> plist)
        {
            string codeStr = "";
            foreach (Pai p in plist)
            {
                codeStr += p.display;
            }
            return codeStr;
        }


        // 计算副露后的统计
        public static List<Pai> GetFuluOff(List<Pai>plist, FuluMianzi mianzi)
        {
            var newHand = new List<Pai>(plist);
            var mz = new List<string>();
            if (mianzi.type == 6)
            {
                // 加杠牌
                mz.Add(mianzi.combination.Split('|')[1]);
            }
            else
            {
                foreach(string str in mianzi.combination.Split('|'))
                {
                    mz.Add(str);
                }
            }
            foreach (var p in mz)
            {
                foreach(Pai hd in newHand)
                {
                    if (hd.code == p) newHand.Remove(hd);;
                    break;
                }
                
            }
            return newHand;
        }

        // 获得新的副露堆
        public static List<string> GetFulu(List<string>fulu, FuluMianzi mianzi)
        {
            var new_fulu = new List<string>(fulu);
            if (mianzi.type == 6)
            {
                // 加杠
                var ori = mianzi.combination.Split('|')[0];
                for(var i=0;i<new_fulu.Count();i++)
                {
                    if (new_fulu[i] == ori) new_fulu[i] = FuluCode(mianzi.combination);
                }
            }
            else
            {
                new_fulu.Add(FuluCode(mianzi.combination));
            }
            return new_fulu;
        }

        // 获取副露后的代码
        static string FuluCode(string mianzi)
        {
            char ch = ' ';
            string code = "";
            foreach (string p in mianzi.Split('|'))
            {
                ch = p[1];
                code += p[0];
            }
            return ch + code;
        }

    }
}
