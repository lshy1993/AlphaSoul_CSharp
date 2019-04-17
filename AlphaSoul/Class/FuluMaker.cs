using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlphaSoul
{
    class FuluMianzi
    {
        public string combination;
        public int type;

        public FuluMianzi(string comb, int type)
        {
            this.combination = comb;
            this.type = type;
        }
    }

    class FuluMaker
    {
        // 是否能吃
        public static List<FuluMianzi> get_chi_mianzi(Dictionary<char, int[]> pCount, List<string> fulu, string p)
        {
            List<FuluMianzi> mianzi = new List<FuluMianzi>();
            char s = p[1];
            int n = p[0] == '0' ? 5 : (p[0] - '0');
            char d = p[2];
            var bingpai = pCount[s];
            // 上家打牌 && 非字牌
            if (s != 'z' && d == '-')
            {
                // n-2 n-1 n
                if (3 <= n && bingpai[n - 2] > 0 && bingpai[n - 1] > 0)
                {
                    string p1 = ((n - 2 == 5 && bingpai[0] > 0) ? 0 : n - 2).ToString() + s;
                    string p2 = ((n - 1 == 5 && bingpai[0] > 0) ? 0 : n - 1).ToString() + s;
                    if (!(fulu.Count == 3 && bingpai[n] == 1 && 3 < n && bingpai[n - 3] == 1))
                    {
                        mianzi.Add(new FuluMianzi(p1 + "|" + p2 + "|" + p, 2));
                    }
                }
                // n n+1 n+2
                if (n <= 7 && bingpai[n + 1] > 0 && bingpai[n + 2] > 0)
                {
                    string p1 = ((n + 1 == 5 && bingpai[0] > 0) ? 0 : n + 1).ToString() + s;
                    string p2 = ((n + 2 == 5 && bingpai[0] > 0) ? 0 : n + 2).ToString() + s;
                    if (!(fulu.Count == 3 && bingpai[n] == 1 && n < 7 && bingpai[n + 3] == 1))
                    {
                        mianzi.Add(new FuluMianzi(p + "|" + p1 + "|" + p2, 2));
                    }
                }
                // n-1 n n+1
                if (2 <= n && n <= 8 && bingpai[n - 1] > 0 && bingpai[n + 1] > 0)
                {
                    string p1 = ((n - 1 == 5 && bingpai[0] > 0) ? 0 : n - 1).ToString() + s;
                    string p2 = ((n + 1 == 5 && bingpai[0] > 0) ? 0 : n + 1).ToString() + s;
                    mianzi.Add(new FuluMianzi(p1 + "|" + p + "|" + p2, 2));
                }
            }
            return mianzi;
        }
        // 是否能碰
        public static List<FuluMianzi> get_peng_mianzi(Dictionary<char, int[]> pCount, List<string> fulu, string p)
        {
            List<FuluMianzi> mianzi = new List<FuluMianzi>();
            char s = p[1];
            int n = p[0] == '0' ? 5 : (p[0] - '0');
            char d = p[2];
            var bingpai = pCount[s];
            if (d != '_' && bingpai[n] >= 2)
            {
                string p1 = ((n == 5 && bingpai[0] > 1) ? 0 : n).ToString() + s;
                string p2 = ((n == 5 && bingpai[0] > 0) ? 0 : n).ToString() + s;
                mianzi.Add(new FuluMianzi(p1 + "|" + p2 + "|" + p, 3));
            }
            return mianzi;
        }
        // 是否能杠
        public static List<FuluMianzi> get_gang_mianzi(Dictionary<char, int[]> pCount, List<string> fulu, string p)
        {
            List<FuluMianzi> mianzi = new List<FuluMianzi>();
            char s = p[1];
            int n = p[0] == '0' ? 5 : (p[0] - '0');
            char d = p[2];
            var shoupai = pCount;
            var bingpai = shoupai[s];
            // 明杠
            if (bingpai[n] == 3)
            {
                string p1 = ((n == 5 && bingpai[0] > 2) ? 0 : n).ToString() + s;
                string p2 = ((n == 5 && bingpai[0] > 1) ? 0 : n).ToString() + s;
                string p3 = ((n == 5 && bingpai[0] > 0) ? 0 : n).ToString() + s;
                mianzi.Add(new FuluMianzi(p1 + "|" + p2 + "|" + p3 + "|" + p, 5));
            }
            // 暗/加杠
            foreach (KeyValuePair<char,int[]> kv in shoupai)
            {
                var bp = kv.Value;
                for (var i = 1; i < bp.Count(); i++)
                {
                    if (bp[i] == 0) continue;
                    if (bp[i] == 4)
                    {
                        string p0 = ((i == 5 && bp[0] > 3) ? 0 : i).ToString() + s;
                        string p1 = ((i == 5 && bp[0] > 2) ? 0 : i).ToString() + s;
                        string p2 = ((i == 5 && bp[0] > 1) ? 0 : i).ToString() + s;
                        string p3 = ((i == 5 && bp[0] > 0) ? 0 : i).ToString() + s;
                        mianzi.Add(new FuluMianzi(p0 + "|" + p1 + "|" + p2 + "|" + p3, 4));
                    }
                    else
                    {
                        foreach (string m in fulu)
                        {
                            if (m.Replace('0','5').Substring(0, 4) == s + n.ToString() + n.ToString() + n.ToString())
                            {
                                string p0 = ((n == 5 && bp[0] > 0) ? 0 : n).ToString() + s;
                                mianzi.Add(new FuluMianzi(m + "|" + p0, 6));
                            }
                        }
                    }
                }
            }
            return mianzi;
        }

        public static List<FuluMianzi> GetFuluMianzi(List<Pai> handStack,List<string> fuluStack, Pai hupai)
        {
            var pCount = PaiMaker.GetCount(handStack);
            List<FuluMianzi> fulumz = get_gang_mianzi(pCount, fuluStack, hupai.code + '-');
            fulumz = fulumz.Concat(get_peng_mianzi(pCount, fuluStack, hupai.code + '-')).ToList();
            fulumz = fulumz.Concat(get_chi_mianzi(pCount, fuluStack, hupai.code + '-')).ToList();
            return fulumz;
        }

    }
}
