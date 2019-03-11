using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AlphaSoul
{
    public class RonJudger
    {
        /// <summary>
        /// 荣和判定面子拆分
        /// </summary>
        public List<Mianzi> Ron(List<Pai> hand, string moPai, List<string> fulu)
        {
            Dictionary<char, int[]> paiCount = PaiMaker.GetCount(hand);
            List<Mianzi> mianzi = new List<Mianzi>();
            // 七对子判定
            Mianzi temp = Ron_qiduizi(paiCount, moPai);
            if (temp != null) mianzi.Add(temp);
            // 国士无双判定
            temp = Ron_guoshi(paiCount, moPai);
            if (temp != null) mianzi.Add(temp);
            // 九宝莲灯
            temp = Ron_jiulian(paiCount, moPai);
            if (temp != null) mianzi.Add(temp);
            // 一般 4面1头形
            foreach (Mianzi mz in Ron_normal(paiCount, moPai, fulu))
            {
                if (mz == null) continue;
                mianzi.Add(mz);
            }

            return mianzi;
        }

        /// <summary>
        /// 七对子判定
        /// </summary>
        public Mianzi Ron_qiduizi(Dictionary<char, int[]> paiCount, string hulepai)
        {
            Mianzi mz = new Mianzi();
            List<string> paixing = new List<string>();
            foreach (KeyValuePair<char, int[]> kv in paiCount)
            {
                char ch = kv.Key;
                int[] shoupai = kv.Value;
                for (int n = 1; n < shoupai.Length; n++)
                {
                    int pcount = shoupai[n];
                    if (pcount == 0) continue;
                    if (pcount == 2)
                    {
                        string p = ch + n.ToString() + n.ToString();
                        if (ch == hulepai[1] && n == (hulepai[0]-'0'))
                        {
                            p += hulepai[2] + "!";
                        }
                        paixing.Add(p);
                    }
                    else return null;  // 対子でないものがあった場合、和了形でない。
                }
            }

            if (paixing.Count == 7)
            {
                mz.paizu = paixing;
            }
            return mz;
        }

        /// <summary>
        /// 国士无双
        /// </summary>
        public Mianzi Ron_guoshi(Dictionary<char, int[]> paiCount, string hulepai)
        {
            Mianzi mz = new Mianzi();
            List<string> paixing = new List<string>();

            //if (shoupai._fulou.length > 0) return mz;

            bool you_duizi = false;
            foreach (KeyValuePair<char, int[]> kv in paiCount)
            {
                char ch = kv.Key;
                int[] shoupai = kv.Value;
                int[] nn;
                if (ch == 'z')
                {
                    nn = new int[] { 1, 2, 3, 4, 5, 6, 7 };
                }
                else
                {
                    nn = new int[] { 1, 9 };
                }
                foreach (int n in nn) {
                    if (shoupai[n] == 2)
                    {
                        string p = ch + n.ToString() + n.ToString();
                        if (ch == hulepai[1] && n == (hulepai[0]-'0'))
                        {
                            p += hulepai[2] + "!";
                        }
                        paixing.Add(p);
                        you_duizi = true;
                    }
                    else if (shoupai[n] == 1)
                    {
                        string p = ch + n.ToString();
                        if (ch == hulepai[1] && n == (hulepai[0] - '0'))
                        {
                            p += hulepai[2] + "!";
                        }
                        paixing.Add(p);
                    }
                    else return null;  // 足りない幺九牌があった場合、和了形でない。
                }
            }
            if (you_duizi)
            {
                mz.paizu = paixing;
            }
            return mz;
        }

        public Mianzi Ron_jiulian(Dictionary<char, int[]> paiCount, string hulepai)
        {
            Mianzi mz = new Mianzi();
            List<string> paixing = new List<string>();

            //如果存在字牌 则不是九莲
            if (paiCount['z'].Sum() != 0) return null;

            //遍历4种牌
            foreach (KeyValuePair<char, int[]> kv in paiCount)
            {
                char ch = kv.Key;
                int[] shoupai = kv.Value;
                
                //只能存在1种牌
                if (shoupai.Sum() == 0 || ch == 'z') continue;

                string p = "";
                //对于该牌型检查
                for (int n = 1; n <= 9; n++)
                {
                    //1和9不满3张则无效
                    if ((n == 1 || n == 9) && shoupai[n] < 3) return null;
                    //缺少某一个数字则无效 足りない数牌がある場合、和了形でない
                    if (shoupai[n] == 0) return null;
                    //牌数
                    int nn = (n == hulepai[0] - '0') ? shoupai[n] - 1 : shoupai[n];
                    for (var i = 0; i < nn; i++)
                    {
                        p += n;
                    }
                }
                p += hulepai[0] + "!";
                paixing.Add(p);
                mz.paizu = paixing;
                return mz;
            }

            return null;

        }

        /// <summary>
        /// 一般型判定
        /// </summary>
        private List<Mianzi> Ron_normal(Dictionary<char, int[]> paiCount, string hulepai, List<string> fulu)
        {
            List<Mianzi> mzlist = new List<Mianzi>();

            //遍历4种牌
            foreach (KeyValuePair<char, int[]> kv in paiCount)
            {
                char ch = kv.Key;
                int[] shoupai = kv.Value;
                for (int n = 1; n < shoupai.Length; n++)
                {
                    //非雀头
                    if (shoupai[n] < 2) continue;
                    string jiangpai = ch + n.ToString() + n.ToString();
                    //剩余手牌拆面子
                    paiCount[ch][n] -= 2;
                    List<Mianzi> mlist = MianziDevide(paiCount);
                    paiCount[ch][n] += 2;
                    if (mlist == null) continue;
                    foreach (Mianzi mianzi in mlist)
                    {
                        List<string> temp = new List<string>();
                        temp.Add(jiangpai);
                        mianzi.paizu = temp.Concat(mianzi.paizu).Concat(fulu).ToList();
                        if (mianzi.paizu.Count() != 5) continue;
                        // TODO: 整理
                        foreach (Mianzi mark in AddMark(mianzi.paizu, hulepai))
                        {
                            mzlist.Add(mark);
                        }

                    }
                    
                }
            }

            return mzlist;
        }

        /// <summary>
        /// 面子拆分
        /// </summary>
        private List<Mianzi> MianziDevide(Dictionary<char, int[]> paiCount)
        {
            List<List<string>> allmianzi = new List<List<string>>();
            //万饼索分别检测
            foreach (char ch in new char[] { 'm', 'p', 's' })
            {
                List<List<string>> mianzi_m = new List<List<string>>();
                MianziPick(ch, paiCount[ch], 1, new List<string>(), ref mianzi_m);
                if (mianzi_m.Count == 0) continue;

                List<List<string>> newmianzi = new List<List<string>>();
                if (allmianzi.Count != 0)
                {
                    for (int i = 0; i < allmianzi.Count; i++)
                    {
                        foreach (List<string> strp in mianzi_m)
                        {
                            newmianzi.Add(allmianzi[i].Concat(strp).ToList());
                        }
                    }
                }
                else
                {
                    foreach (List<string> strp in mianzi_m)
                    {
                        newmianzi.Add(strp);
                    }
                }
                allmianzi = new List<List<string>>(newmianzi);

            }
            //字牌检测
            List<string> mianzi_z = new List<string>();
            for (var n = 1; n <= 7; n++)
            {
                if (paiCount['z'][n] == 0) continue;
                if (paiCount['z'][n] != 3) return null;
                mianzi_z.Add('z' + n.ToString() + n.ToString() + n.ToString());
            }
            List<Mianzi> mzlist = new List<Mianzi>();
            //组合
            if(allmianzi.Count == 0)
            {
                Mianzi mm = new Mianzi();
                mm.paizu = mianzi_z;
                mzlist.Add(mm);
            }
            else
            {
                foreach (List<string> ori in allmianzi)
                {
                    Mianzi mm = new Mianzi();
                    mm.paizu = ori.Concat(mianzi_z).ToList();
                    mzlist.Add(mm);
                }
            }

            return mzlist;
        }

        //面子拆分搜索
        private void MianziPick(char s,int[] shoupai, int n, List<string> path, ref List<List<string>> all)
        {
            if (n > 9) {

                if (path.Count > 0 && shoupai.Sum() == 0)
                {
                    all.Add(path);
                }
                return;
            }

            /* 面子を抜き取り終わったら、次の位置に進む */
            if (shoupai[n] == 0)
            {
                MianziPick(s, shoupai, n + 1, path, ref all);
            }

            /* 順子を抜き取る */
            if (n <= 7 && shoupai[n] > 0 && shoupai[n + 1] > 0 && shoupai[n + 2] > 0)
            {
                List<string> npath = new List<string>(path);
                npath.Add(s + n.ToString() + (n + 1).ToString() + (n + 2).ToString());
                int[] nsp = (int[])shoupai.Clone();
                nsp[n]--;
                nsp[n + 1]--;
                nsp[n + 2]--;
                MianziPick(s, nsp, n, npath, ref all);  // 抜き取ったら同じ位置でもう一度試行
            }

            /* 刻子を抜き取る */
            if (shoupai[n] >= 3)
            {
                List<string> npath = new List<string>(path);
                npath.Add(s + n.ToString() + n.ToString() + n.ToString());
                int[] nsp = (int[])shoupai.Clone();
                nsp[n] -= 3;
                MianziPick(s, nsp, n, npath, ref all);    // 抜き取ったら同じ位置でもう一度試行
            }

            return;
        }

        private List<Mianzi> AddMark(List<string> mianzi, string p)
        {
            //string regexp = "^(" + p.type + ".*" + (p.num != 0 ? p.num.ToString() : "5") + ")";
            string regexp = "^(" + p[1] + ".*" + (p[0] != '0' ? p[0].ToString() : "5") + ")";
            string replacer = "$1" + p[2] + "!";

            List<Mianzi> new_mianzi = new List<Mianzi>();

            for (int i=0;i<mianzi.Count;i++)
            {
                // 副露面
                if (Regex.IsMatch(mianzi[i],@"[\-\+\=]")) continue;
                // 相同略
                if (i > 0 && mianzi[i] == mianzi[i - 1]) continue;
                string m = Regex.Replace(mianzi[i], regexp, replacer);
                if (m == mianzi[i]) continue;
                var tmp_mianzi = new List<string>(mianzi);
                tmp_mianzi[i] = m;
                new_mianzi.Add(new Mianzi(tmp_mianzi));
            }
            return new_mianzi;
        }

    }
}
