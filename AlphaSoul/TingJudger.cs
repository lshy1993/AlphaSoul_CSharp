using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlphaSoul
{
    /// <summary>
    /// 向听数判断器
    /// </summary>
    public static class TingJudger
    {

        private static int _xiangting(int m, int d,int g,bool j)
        {
            // 雀頭がない場合は5ブロック必要
            var n = j ? 4 : 5;  
            // 面子過多の補正
            if (m > 4) {
                d += m - 4;
                m = 4;
            }
            // 搭子過多の補正
            if (m + d > 4) {
                g += m + d - 4;
                d = 4 - m;
            }
            // 孤立牌過多の補正
            if (m + d + g > n) {
                g = n - m - d;
            }
            // 雀頭がある場合は搭子として数える
            if (j) d++;
            return 13 - m * 3 - d * 2 - g;
        }

        public static int xiangting(Dictionary<char, int[]> paiCount, List<string> fulu)
        {
            int max = xiangting_yiban(paiCount, fulu);
            int xg = xiangting_guoshi(paiCount);
            int xq = xiangting_qiduizi(paiCount);
            return Math.Min(max, Math.Min(xg, xq));
        }

        private static int xiangting_yiban(Dictionary<char, int[]> paiCount, List<string> fulu)
        {
            // 没有指定雀头的情况下向听数作为最小值
            int min_xiangting = mianzi_all(paiCount, fulu);

            /* 可能な雀頭を抜き取り mianzi_all() を呼出し、向聴数を計算させる */
            // 遍历4种牌
            foreach (KeyValuePair<char, int[]> kv in paiCount)
            {
                char ch = kv.Key;
                int[] shoupai = kv.Value;
                for (int n = 1; n < shoupai.Length; n++)
                {
                    //非雀头
                    if (shoupai[n] < 2) continue;
                    //剩余手牌拆面子
                    paiCount[ch][n] -= 2;
                    int xiangting = mianzi_all(paiCount, fulu, true);
                    paiCount[ch][n] += 2;
                    // 替换最小值
                    min_xiangting = Math.Min(xiangting, min_xiangting);
                }
            }

            return min_xiangting;
        }

        private static int mianzi_all(Dictionary<char,int[]> paiCount, List<string> fulu, bool jiangpai = false)
        {
            // 分别计算 m p s 的面子与搭子数目
            List<int[]> rm = mianzi(paiCount['m'], 1);
            List<int[]> rp = mianzi(paiCount['p'], 1);
            List<int[]> rs = mianzi(paiCount['s'], 1);

            // 字牌 
            var z = new int[] { 0, 0, 0 };
            for (var n = 1; n <= 7; n++)
            {
                // 面子
                if (paiCount['z'][n] >= 3) z[0]++;
                // 搭子
                if (paiCount['z'][n] == 2) z[1]++;
                // 字牌の孤立牌数取得を追加
                if (paiCount['z'][n] == 1) z[2]++;
            }

            // 副露牌作为面子
            int n_fulou = fulu.Count;

            // 最小向聴数 最大值8
            int min_xiangting = 13;

            /* 萬子、筒子、索子、字牌それぞれの面子・搭子の数についてパターンA、Bの
               組合わせで向聴数を計算し、最小値を解とする */
            foreach (int[] m in rm)
            {
                foreach (int[] p in rp)
                {
                    foreach (int[] s in rs)
                    {
                        int n_mianzi = m[0] + p[0] + s[0] + z[0] + n_fulou;
                        int n_dazi = m[1] + p[1] + s[1] + z[1];
                        int n_guli = m[2] + p[2] + s[2] + z[2];
                        if (n_mianzi + n_dazi > 4) n_dazi = 4 - n_mianzi;
                        // 搭子过多修正
                        int xiangting = _xiangting(n_mianzi, n_dazi, n_guli, jiangpai);
                        min_xiangting = Math.Min(xiangting, min_xiangting);
                    }
                }
            }

            return min_xiangting;
        }

        private static List<int[]> mianzi(int[] bingpai, int n)
        {
            if (n > 9) return dazi(bingpai);

            /* まずは面子を抜かず位置を1つ進め試行 */
            var max = mianzi(bingpai, n + 1);

            /* 順子抜き取り */
            if (n <= 7 && bingpai[n] > 0 && bingpai[n + 1] > 0 && bingpai[n + 2] > 0)
            {
                bingpai[n]--;
                bingpai[n + 1]--;
                bingpai[n + 2]--;
                // 抜き取ったら同じ位置でもう一度試行
                var r = mianzi(bingpai, n);
                bingpai[n]++;
                bingpai[n + 1]++;
                bingpai[n + 2]++;
                // 各パターンの面子の数を1増やす
                r[0][0]++;
                r[1][0]++;
                /* 必要であれば最適値の入替えをする */
                if (r[0][0] * 2 + r[0][1] > max[0][0] * 2 + max[0][1]) max[0] = r[0];
                if (r[1][0] * 10 + r[1][1] > max[1][0] * 10 + max[1][1]) max[1] = r[1];
            }

            /* 刻子抜き取り */
            if (bingpai[n] >= 3)
            {
                bingpai[n] -= 3;
                var r = mianzi(bingpai, n);
                bingpai[n] += 3;
                r[0][0]++;
                r[1][0]++;
                if (r[0][0] * 2 + r[0][1] > max[0][0] * 2 + max[0][1]) max[0] = r[0];
                if (r[1][0] * 10 + r[1][1] > max[1][0] * 10 + max[1][1]) max[1] = r[1];
            }

            return max;
        }

        // 计算搭子数
        private static List<int[]> dazi(int[] bingpai)
        {
            int n_pai = 0;
            int n_dazi = 0;
            int n_guli = 0;
            for (var n = 1; n <= 9; n++)
            {
                n_pai += bingpai[n];
                if (n <= 7 && bingpai[n + 1] == 0 && bingpai[n + 2] == 0)
                {
                    //n_dazi += n_pai / 2;
                    n_dazi += n_pai >> 1;
                    n_guli += n_pai % 2;
                    n_pai = 0;
                }
            }
            //n_dazi += n_pai / 2;
            n_dazi += n_pai >> 1;
            n_guli += n_pai % 2;

            List<int[]> res = new List<int[]>();
            res.Add(new int[] { 0, n_dazi, n_guli });
            res.Add(new int[] { 0, n_dazi, n_guli });

            return res;
        }

        // 七对子形的向听数
        private static int xiangting_qiduizi(Dictionary<char, int[]> paiCount)
        {
            int n_duizi = 0;
            int n_danqi = 0;

            foreach (KeyValuePair<char,int[]> kv in paiCount)
            {
                int[] pnum = kv.Value;
                for (var n = 1; n < pnum.Length; n++)
                {
                    if (pnum[n] >= 2) n_duizi++;
                    else if (pnum[n] == 1) n_danqi++;
                }
            }

            if (n_duizi > 7) n_duizi = 7;             // 対子過多の補正
            if (n_duizi + n_danqi > 7) n_danqi = 7 - n_duizi;   // 孤立牌過多の補正

            return 13 - n_duizi * 2 - n_danqi;
        }

        // 国士无双 向听
        private static int xiangting_guoshi(Dictionary<char, int[]> paiCount)
        {
            int n_yaojiu = 0;
            bool you_duizi = false;
            foreach (KeyValuePair<char, int[]> kv in paiCount)
            {
                var bingpai = kv.Value;
                var nn = (kv.Key == 'z') ? new int[] { 1, 2, 3, 4, 5, 6, 7 } : new int[] { 1, 9 };
                foreach (int n in nn)
                {
                    if (bingpai[n] > 0) n_yaojiu++;
                    if (bingpai[n] > 1) you_duizi = true;
                }
            }

            return you_duizi ? 12 - n_yaojiu : 13 - n_yaojiu;
        }

        /// <summary>
        /// 求可以（进张）听的牌
        /// </summary>
        public static List<string> tingpai(Dictionary<char,int[]> paiCount, List<string> fulu)
        {
            List<string> pai = new List<string>();
            // 原先向听数
            var n_xiangting = xiangting(paiCount, fulu);
            foreach (KeyValuePair<char,int[]> kv in paiCount)
            {
                char ch = kv.Key;
                int[] bingpai = kv.Value;
                for (var n = 1; n < bingpai.Length; n++)
                {
                    if (bingpai[n] >= 4) continue;
                    paiCount[ch][n]++;
                    if (xiangting(paiCount, fulu) < n_xiangting) pai.Add(ch + n.ToString());
                    paiCount[ch][n]--;
                }
            }

            return pai;
        }

    }
}
