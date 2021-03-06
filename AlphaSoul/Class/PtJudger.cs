﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AlphaSoul
{

    // 符数的计算类
    class FuResult
    {
        // 符計算の結果
        public int fu = 20;
        // 門前の場合 true
        public bool menqing = true;
        // ツモ和了の場合 true
        public bool zimo = true;

        // 順子の面子構成
        public Dictionary<string, int> shunzim = new Dictionary<string, int>();
        public Dictionary<string, int> shunzip = new Dictionary<string, int>();
        public Dictionary<string, int> shunzis = new Dictionary<string, int>();

        // 刻子の面子構成
        public int[] kezim = new int[10];
        public int[] kezip = new int[10];
        public int[] kezis = new int[10];
        public int[] keziz = new int[10];

        // 順子の数
        public int n_shunzi = 0;
        // 刻子の数(槓子を含む)
        public int n_kezi = 0;
        // 暗刻子の数(暗槓子を含む)
        public int n_ankezi = 0;
        // 槓子の数
        public int n_gangzi = 0;
        // 字牌面子の数(雀頭を含む)
        public int n_zipai = 0;
        // 幺九牌入り面子の数(雀頭を含む)
        public int n_yaojiu = 0;
        // 単騎待ちの場合 true
        public bool danqi = false;
        // 平和の場合 true
        public bool pinghu = false;
        // 場風(0: 東、1: 南、2: 西、3: 北)
        public int zhuangfeng = 0;
        // 自風(0: 東、1: 南、2: 西、3: 北)
        public int menfeng = 0;

        public FuResult()
        {

        }
    }

    /// <summary>
    /// 分数役种判定器
    /// </summary>
    class PtJudger
    {
        /// <summary>
        /// 和了分数判定
        /// </summary>
        public static PtResult GetFen(List<Pai> hand, List<string> fulu, Pai rongpai, PtParam param)
        {
            Dictionary<char, int[]> paiCount = PaiMaker.GetCount(hand);

            PtResult max = new PtResult();
            max.bao = new List<string>(param.baopai);
            max.libao = new List<string>(param.libaopai);
            // 状况役计算
            List<Yaku> pre_hupai = GetPreYaku(param);
            // 悬赏役计算
            List<Yaku> post_hupai = GetPostYaku(paiCount, param);
            // 和牌标记
            int delta = param.zifeng - (rongpai.stat - 1);
            string rongs = rongpai.code;
            if(delta == 0)
            {
                // 自摸
                rongs += "_";
            }
            else if (delta == 2 || delta == -2)
            {
                // 对家 =
                rongs += "=";
            }
            else if (delta == 1 || delta== -3)
            {
                // 下家 +
                rongs += "+";
            }
            else if(delta == -1 || delta == 3)
            {
                // 上家 -
                rongs += "-";
            }
            // 遍历可能的和了形
            List<Mianzi> mianzi_all = (new RonJudger()).Ron(hand, rongs, fulu);
            max.mianziall = mianzi_all;
            foreach (Mianzi mianzi in mianzi_all)
            {
                // 计算符
                FuResult hudi = GetFu(mianzi.paizu, param.changfeng, param.zifeng);
                // 役种计算 （含状况役）
                List<Yaku> hupai = GetYaku(mianzi.paizu, hudi, pre_hupai);
                // 无役的情况
                if (hupai.Count == 0) continue;
                // 符
                int fu = hudi.fu;
                // 总番数
                int fanshu = 0;
                // 基本点
                int basic_pt = 0;
                // 役满个数
                int yiman = 0;
                // 包牌计算
                int baojia = -1,bao_pt = 0;

                // 存在役满
                if (hupai[0].fanshu < 0)
                {
                    foreach (Yaku h in hupai)
                    {
                        // 多个役满累计
                        yiman += Math.Abs(h.fanshu); 
                        if (h.baojia != '0')
                        {
                            // 存在包牌 + 下家 = 对家 - 上家
                            baojia = h.baojia == '+' ? (param.zifeng + 1) % 4
                                    : h.baojia == '=' ? (param.zifeng + 2) % 4
                                    : h.baojia == '-' ? (param.zifeng + 3) % 4
                                    : -1;
                            /* パオ対象の基本点を求める */
                            bao_pt = 8000 * Math.Abs(h.fanshu);
                        }
                    }
                    /* パオを含む全体の基本点を求める */
                    basic_pt = 8000 * yiman;
                }
                else
                {
                    // 役満以外の場合
                    hupai = hupai.Concat(post_hupai).ToList();  // 懸賞役を加える
                    foreach (Yaku h in hupai) { fanshu += h.fanshu; }  // 翻数を決定する

                    /* 基本点を求める */
                    if (fanshu >= 13) basic_pt = 8000;  // 役満
                    else if (fanshu >= 11) basic_pt = 6000;  // 三倍満
                    else if (fanshu >= 8) basic_pt = 4000;  // 倍満
                    else if (fanshu >= 6) basic_pt = 3000;  // 跳満
                    else
                    {
                        basic_pt = fu * 2 * 2;               // 符を4倍する (場ゾロ)
                        for (int i = 0; i < fanshu; i++) { basic_pt *= 2; }
                        // 翻数分だけ2倍する
                        if (basic_pt >= 2000) basic_pt = 2000;  // 2000点を上限とする(満貫)
                    }
                }

                int[] fenpei = new int[] { 0, 0, 0, 0 };    // 収入と負担額を初期化する

                // 如果存在包牌 先结算
                if (bao_pt > 0)
                {
                    if ( param.zifeng != rongpai.stat - 1)
                    {
                        // 放铳 则和包家对半
                        bao_pt = bao_pt / 2;
                    }
                    // 基本点からパオ分を減算
                    basic_pt = basic_pt - bao_pt;
                    bao_pt = bao_pt * (param.zifeng == 0 ? 6 : 4);
                    // パオ分の負担額を求める
                    fenpei[param.zifeng] = bao_pt;   // 和了者の収入を加算
                    fenpei[baojia] = -bao_pt;   // パオ対象の負担額を減算
                }
                // 积累的场棒
                int changbang = param.changbang;
                // 积累的立直棒
                int lizhibang = param.lizhibang;

                if (rongpai.stat - 1 != param.zifeng)
                {
                    // 放铳的情况下
                    // 包家或者 -, +, = 全包的情况
                    var baojia2 = basic_pt == 0 ? baojia : rongpai.stat - 1;  // パオ1人払いは放銃者扱い

                    // 不满100点 向上取整数
                    basic_pt = (int)Math.Ceiling((double)basic_pt * (param.zifeng == 0 ? 6 : 4) / 100) * 100;
                    // 和了者 分数总和 含棒
                    fenpei[param.zifeng] += basic_pt + changbang * 300 + lizhibang * 1000;
                    // 放铳得人减分
                    fenpei[baojia2] += -basic_pt - changbang * 300;
                }
                else
                {
                    // 自摸
                    int zhuangjia = (int)Math.Ceiling((double)basic_pt * 2 / 100) * 100;  // 親の負担額
                    int sanjia = (int)Math.Ceiling((double)basic_pt / 100) * 100;  // 子の負担額

                    if (param.zifeng == 0)
                    {
                        // 親の和了の場合
                        basic_pt = zhuangjia * 3;       // 和了打点は 親の負担額 x 3
                        for (int l = 0; l < 4; l++)
                        {
                            if (l == param.zifeng)
                            {
                                // 和了者の収入を加算(供託含む)
                                fenpei[l] += basic_pt + changbang * 300 + lizhibang * 1000;
                            }
                            else
                            {
                                // 負担者の負担額を減算(供託含む)
                                fenpei[l] += -zhuangjia - changbang * 100;
                            }
                        }
                    }
                    else
                    {
                        // 子の和了の場合
                        basic_pt = zhuangjia + sanjia * 2;
                        // 和了打点は 親の負担額 + 子の負担額 x 2
                        for (int l = 0; l < 4; l++)
                        {
                            if (l == param.zifeng)
                            {
                                // 和了者の収入を加算(供託含む)
                                fenpei[l] += basic_pt + changbang * 300 + lizhibang * 1000;
                            }
                            else if (l == 0)
                            {
                                // 親の負担額を減算(供託含む)
                                fenpei[l] += -zhuangjia - changbang * 100;
                            }
                            else
                            {
                                // 子の負担額を減算(供託含む)
                                fenpei[l] += -sanjia - changbang * 100;
                            }
                        }
                    }
                }

                if (basic_pt + bao_pt > max.defen || basic_pt + bao_pt == max.defen
                && (fanshu == 0 || fanshu > max.fanshu
                    || fanshu == max.fanshu && fu > max.fu))
                {
                    max.mianzi = mianzi;
                    max.hupai = hupai;           // 和了役一覧
                    max.fu = fu;              // 符
                    max.fanshu = fanshu;          // 翻数
                    max.yiman = yiman;       // 役満複合数
                    max.defen = basic_pt + bao_pt;  // 和了打点
                    max.fenpei = fenpei;       // 局収支
                }
            }
            /* 得られた和了点の最大値を解とする */
            return max;
        }

        /// <summary>
        /// 状况役种
        /// </summary>
        private static List<Yaku> GetPreYaku(PtParam parm)
        {
            List<Yaku> res = new List<Yaku>();
            if (parm.lizhi == 1) res.Add(new Yaku("立直", 1));
            if (parm.lizhi == 2) res.Add(new Yaku("两立直", 2));
            if (parm.yifa) res.Add(new Yaku("一发", 1));
            if (parm.haidi == 1) res.Add(new Yaku("海底摸月", 1));
            if (parm.haidi == 2) res.Add(new Yaku("河底捞鱼", 1));
            if (parm.lingshang) res.Add(new Yaku("岭上开花", 1));
            if (parm.qianggang) res.Add(new Yaku("抢杠", 1));

            if (parm.tianhu == 1) res = new List<Yaku> { new Yaku("天和", -1) };
            if (parm.tianhu == 2) res = new List<Yaku> { new Yaku("地和", -1) };

            return res;
        }


        /// <summary>
        /// 懸賞役
        /// </summary>
        private static List<Yaku> GetPostYaku(Dictionary<char, int[]> shoupai, PtParam param)
        {
            List<Yaku> res = new List<Yaku>();
            List<string> baopai = param.baopai;
            List<string> libaopai = param.libaopai;

            // 宝牌
            int n_baopai = 0;
            foreach (string baostr in baopai)
            {
                // 根据宝牌代码获取真宝牌
                int num = baostr[0] - '0';
                char ch = baostr[1];
                if (ch == 'z')
                {
                    if (num == 4) num = 1;
                    else if (num == 7) num = 5;
                    else num = num + 1;
                }
                else
                {
                    num = (num == 9) ? 1 : num + 1;
                }
                n_baopai += shoupai[ch][num];
            }
            if (n_baopai > 0) res.Add(new Yaku("宝牌", n_baopai));
            // 红宝牌
            int n_hongpai = shoupai['m'][0] + shoupai['p'][0] + shoupai['s'][0];
            if (n_hongpai > 0) res.Add(new Yaku("红宝牌", n_hongpai));
            if (param.lizhi == 0) return res;
            // 里宝牌
            int n_libao = 0;
            foreach (string baostr in libaopai)
            {
                // 根据宝牌代码获取真宝牌
                int num = baostr[0] - '0';
                char ch = baostr[1];
                if (ch == 'z')
                {
                    if (num == 4) num = 1;
                    else if (num == 7) num = 5;
                    else num = num + 1;
                }
                else
                {
                    num = (num == 9) ? 1 : num + 1;
                }
                n_libao += shoupai[ch][num];
            }
            if (n_libao > 0) res.Add(new Yaku("里宝牌", n_libao));
            return res;
        }

        /// <summary>
        /// 符数计算
        /// </summary>
        private static FuResult GetFu(List<string> mlist, int cf, int zf)
        {
            // 场风
            string changfeng = "^z" + (cf + 1).ToString() + ".*$";
            // 自风
            string zifeng = "^z" + (zf + 1).ToString() + ".*$";
            // 三元牌 777z
            string sanyuan = "z^[567].*$";
            // 幺九牌
            string yaojiu = "^.*[z19].*$";
            // 字牌
            string zipai = "^z.*$";
            // 刻子(含杠)
            string kezi = @"^[mpsz](\d)\1\1.*$";
            // 暗刻子(含暗杠)
            string ankezi = @"^[mpsz](\d)\1\1(?:\1|_\!)?$";
            // 明杠
            string gangzi = @"^[mpsz](\d)\1\1.*\1.*$";
            // 听单张
            string danqi = @"^[mpsz](\d)\1[\-\+\=\\_]\!$";
            // 听坎张
            string kanzhang = @"^[mps]\d\d[\-\+\=\\_]\!\d$";
            // 听边张
            string bianzhang = @"^[mps](123[\-\+\=\\_]\!| 7[\-\+\=\\_]\!89)$";

            //各个面子检测
            FuResult res = new FuResult();
            foreach (string mz in mlist)
            {
                // 放铳 为false
                if (Regex.IsMatch(mz, @"[\-\+\=]\!")) res.zimo = false;
                // 副露 为false
                if (Regex.IsMatch(mz, @"[\-\+\=](?!\!)")) res.menqing = false;
                // 存在幺九牌加1
                if (Regex.IsMatch(mz, yaojiu)) res.n_yaojiu++;
                // 存在字牌加1
                if (Regex.IsMatch(mz, zipai)) res.n_zipai++;
                // 单骑 true
                if (Regex.IsMatch(mz, danqi)) res.danqi = true;

                // 非4面子1雀头形跳过
                if (mlist.Count != 5) continue;

                if (mz == mlist[0])
                {
                    // 雀头处理
                    int fu = 0;
                    // 场风加2符
                    if (Regex.IsMatch(mz, changfeng)) fu += 2;
                    // 自风加2符
                    if (Regex.IsMatch(mz, zifeng)) fu += 2;
                    // 三元牌加2符
                    if (Regex.IsMatch(mz, sanyuan)) fu += 2;
                    // 単騎待ちの場合2符加符 
                    if (res.danqi) fu += 2;

                    res.fu += fu;

                }
                else if (Regex.IsMatch(mz, kezi))
                {
                    // 刻子处理
                    // 刻子数加1
                    res.n_kezi++;
                    // 初始2符
                    int fu = 2;
                    // 幺九牌 2倍
                    if (Regex.IsMatch(mz, yaojiu)) fu *= 2;
                    // 暗刻2倍 数加1
                    if (Regex.IsMatch(mz, ankezi))
                    {
                        fu *= 2;
                        res.n_ankezi++;
                    }
                    // 杠4倍 数加1
                    if (Regex.IsMatch(mz, gangzi))
                    {
                        fu *= 4;
                        res.n_gangzi++;
                    }

                    res.fu += fu;

                    // 记录刻子组成
                    int num = mz[1] - '0';
                    if (mz[0] == 'm') res.kezim[num] = 1;
                    if (mz[0] == 'p') res.kezip[num] = 1;
                    if (mz[0] == 's') res.kezis[num] = 1;
                    if (mz[0] == 'z') res.keziz[num] = 1;
                }
                else
                {
                    // 顺子处理
                    res.n_shunzi++;
                    // 胡坎张 加2符
                    if (Regex.IsMatch(mz, kanzhang)) res.fu += 2;
                    // 胡边张 加2符
                    if (Regex.IsMatch(mz, bianzhang)) res.fu += 2;

                    /* 順子の構成を記録 */
                    string nnn = Regex.Replace(mz, @"[^\d]", "");
                    if (mz[0] == 'm')
                    {
                        if (!res.shunzim.ContainsKey(nnn)) res.shunzim.Add(nnn, 0);
                        res.shunzim[nnn] += 1;
                    }
                    if (mz[0] == 'p')
                    {
                        if (!res.shunzip.ContainsKey(nnn)) res.shunzip.Add(nnn, 0);
                        res.shunzip[nnn] += 1;
                    }
                    if (mz[0] == 's')
                    {
                        if (!res.shunzis.ContainsKey(nnn)) res.shunzis.Add(nnn, 0);
                        res.shunzis[nnn] += 1;
                    }

                }

            }

            if (mlist.Count == 7)
            {
                // 七对子固定25符
                res.fu = 25;
            }
            else if (mlist.Count == 5)
            {
                // 4面子1雀头
                // 门清 20符 即为平和
                res.pinghu = (res.menqing && res.fu == 20);

                if (res.zimo && !res.pinghu)
                {
                    // 自摸非平和 加2符
                    res.fu += 2;
                }
                else
                {
                    // 放铳和
                    // 门清加10符 但平胡为固定30符
                    if (res.menqing) res.fu += 10;
                    else if (res.fu == 20) res.fu = 30;
                }
                // 个位的1-9全部进位
                double fu = res.fu / 10;
                res.fu = ((int)Math.Ceiling(fu)) * 10;
            }

            return res;
        }

        // 役种统计
        private static List<Yaku> GetYaku(List<string> mlist, FuResult fu, List<Yaku> pre)
        {
            // 初始化役满 (天和、地和)
            List<Yaku> res = (pre.Count > 0 && pre[0].fanshu < 0) ? new List<Yaku>(pre) : new List<Yaku>();
            // 役满追加
            Yaku temp;
            temp = guoshiwushuang(mlist, fu);
            if (temp != null) res.Add(temp);
            temp = sianke(mlist, fu);
            if (temp != null) res.Add(temp);
            temp = dasanyuan(mlist, fu);
            if (temp != null) res.Add(temp);
            temp = sixihu(mlist, fu);
            if (temp != null) res.Add(temp);
            temp = ziyise(mlist, fu);
            if (temp != null) res.Add(temp);
            temp = lvyise(mlist, fu);
            if (temp != null) res.Add(temp);
            temp = qinglaotou(mlist, fu);
            if (temp != null) res.Add(temp);
            temp = sigangzi(mlist, fu);
            if (temp != null) res.Add(temp);
            temp = jiulianbaodeng(mlist, fu);
            if (temp != null) res.Add(temp);

            // 有役满直接返回役满
            if (res.Count > 0) return res;
            // 无役满 在状况役后追加役种
            res = new List<Yaku>(pre);

            temp = menqianqing(fu);
            if (temp != null) res.Add(temp);
            List<Yaku> yipailist = yipai(fu);
            if (yipailist != null)
            {
                foreach (Yaku y in yipailist) res.Add(y);
            }
            temp = pinghu(fu);
            if (temp != null) res.Add(temp);
            temp = duanyaojiu(fu);
            if (temp != null) res.Add(temp);
            temp = yibeikou(fu);
            if (temp != null) res.Add(temp);
            temp = sansetongshun(fu);
            if (temp != null) res.Add(temp);
            temp = yiqitongguan(fu);
            if (temp != null) res.Add(temp);
            temp = hunquandaiyaojiu(fu);
            if (temp != null) res.Add(temp);
            temp = qiduizi(mlist);
            if (temp != null) res.Add(temp);
            temp = duiduihu(fu);
            if (temp != null) res.Add(temp);
            temp = sananke(fu);
            if (temp != null) res.Add(temp);
            temp = sangangzi(fu);
            if (temp != null) res.Add(temp);
            temp = sansetongke(fu);
            if (temp != null) res.Add(temp);
            temp = hunlaotou(mlist, fu);
            if (temp != null) res.Add(temp);
            temp = xiaosanyuan(mlist, fu);
            if (temp != null) res.Add(temp);
            temp = hunyise(mlist, fu);
            if (temp != null) res.Add(temp);
            temp = chunquandaiyaojiu(fu);
            if (temp != null) res.Add(temp);
            temp = erbeikou(fu);
            if (temp != null) res.Add(temp);
            temp = qingyise(mlist, fu);
            if (temp != null) res.Add(temp);

            return res;
        }

        private static Yaku guoshiwushuang(List<string> mianzi, FuResult fu)
        {
            if (mianzi.Count != 13) return null;
            if (fu.danqi) return new Yaku("国士无双十三面", -2);
            return new Yaku("国士无双", -1);
        }

        private static Yaku sianke(List<string> mianzi, FuResult fu)
        {
            if (fu.n_ankezi != 4) return null;
            if (fu.danqi) return new Yaku("四暗刻单骑", -2);
            return new Yaku("四暗刻", -1);
        }

        private static Yaku dasanyuan(List<string> mianzi, FuResult fu)
        {
            if (fu.keziz[5] + fu.keziz[6] + fu.keziz[7] == 3)
            {
                List<string> baomian = new List<string>();
                // 是否存在包牌
                string bao = @"^z([567])*(?:[\-\+\=])";
                foreach (string mz in mianzi)
                {
                    if (Regex.IsMatch(mz, bao))
                    {
                        baomian.Add(mz);
                    }
                }
                if (baomian.Count > 2 && Regex.IsMatch(baomian[2], @"^[\+\-\=]"))
                {
                    char baojia = Regex.IsMatch(baomian[2], @"[\+\-\=]").ToString()[0];
                    return new Yaku("大三元", -1, baojia);
                }
                return new Yaku("大三元", -1);
            }
            return null;
        }

        private static Yaku sixihu(List<string> mianzi, FuResult fu)
        {
            if (fu.keziz[1] + fu.keziz[2] + fu.keziz[3] + fu.keziz[4] == 4)
            {
                List<string> baomian = new List<string>();
                // 是否存在包牌
                string bao = @"^z([1234])\1\1(?:[\-\+\=]|\1)(?!\!)";
                foreach (string mz in mianzi)
                {
                    if (Regex.IsMatch(mz, bao))
                    {
                        baomian.Add(mz);
                    }
                }
                if (baomian.Count > 3 && Regex.IsMatch(baomian[3], @"[\-\+\=]"))
                {
                    char baojia = Regex.IsMatch(baomian[3], @"[\+\-\=]").ToString()[0];
                    return new Yaku("大四喜", -2, baojia);
                }
                return new Yaku("大四喜", -2);
            }
            if (fu.keziz[1] + fu.keziz[2] + fu.keziz[3] + fu.keziz[4] == 3
                && Regex.IsMatch(mianzi[0], @"^z[1234]"))
            {
                return new Yaku("小四喜", -1);
            }
            return null;
        }

        private static Yaku ziyise(List<string> mianzi, FuResult fu)
        {
            if (fu.n_zipai == mianzi.Count) return new Yaku("字一色", -1);
            return null;
        }

        private static Yaku lvyise(List<string> mianzi, FuResult fu)
        {
            foreach (string mz in mianzi)
            {
                if (Regex.IsMatch(mz, "^[mp]")) return null;
                if (Regex.IsMatch(mz, "^z[^6]")) return null;
                if (Regex.IsMatch(mz, "^s.*[1579]")) return null;
            }
            return new Yaku("绿一色", -1);
        }

        private static Yaku qinglaotou(List<string> mianzi, FuResult fu)
        {
            if (fu.n_kezi == 4 && fu.n_yaojiu == 5 && fu.n_zipai == 0)
                return new Yaku("清老头", -1);
            return null;
        }

        private static Yaku sigangzi(List<string> mianzi, FuResult fu)
        {
            if (fu.n_gangzi == 4) return new Yaku("四杠子", -1);
            return null;
        }

        private static Yaku jiulianbaodeng(List<string> mianzi, FuResult fu)
        {
            if (mianzi.Count != 1) return null;
            if (Regex.IsMatch(mianzi[0], "^[mps]1112345678999"))
            {
                return new Yaku("纯正九莲宝灯", -2);
            }
            else
            {
                return new Yaku("九莲宝灯", -1);
            }

        }

        private static Yaku menqianqing(FuResult fu)
        {
            if (fu.menqing && fu.zimo)
                return new Yaku("门前清自摸和", 1);
            return null;
        }

        private static List<Yaku> yipai(FuResult fu)
        {
            string[] fengzi = new string[] { "东", "南", "西", "北" };
            List<Yaku> yipai_all = new List<Yaku>();
            if (fu.keziz[fu.zhuangfeng + 1] > 0)
            {
                yipai_all.Add(new Yaku("场风牌 " + fengzi[fu.zhuangfeng], 1));
            }
            if (fu.keziz[fu.menfeng + 1] > 0)
            {
                yipai_all.Add(new Yaku("门风牌 " + fengzi[fu.menfeng], 1));
            }
            if (fu.keziz[5] > 0) yipai_all.Add(new Yaku("役牌 白", 1));
            if (fu.keziz[6] > 0) yipai_all.Add(new Yaku("役牌 发", 1));
            if (fu.keziz[7] > 0) yipai_all.Add(new Yaku("役牌 中", 1));

            return yipai_all;
        }

        private static Yaku pinghu(FuResult fu)
        {
            if (fu.pinghu) return new Yaku("平和", 1);
            return null;
        }

        private static Yaku duanyaojiu(FuResult fu)
        {
            if (fu.n_yaojiu == 0) return new Yaku("断幺九", 1);
            return null;
        }

        private static Yaku yibeikou(FuResult fu)
        {
            if (!fu.menqing) return null;
            int beikou = 0;
            foreach (KeyValuePair<string, int> kv in fu.shunzim)
            {
                if (kv.Value > 3) beikou++;
                if (kv.Value > 1) beikou++;
            }
            foreach (KeyValuePair<string, int> kv in fu.shunzip)
            {
                if (kv.Value > 3) beikou++;
                if (kv.Value > 1) beikou++;
            }
            foreach (KeyValuePair<string, int> kv in fu.shunzis)
            {
                if (kv.Value > 3) beikou++;
                if (kv.Value > 1) beikou++;
            }
            if (beikou == 1) return new Yaku("一杯口", 1);
            return null;
        }

        private static Yaku sansetongshun(FuResult fu)
        {
            foreach (KeyValuePair<string, int> kv in fu.shunzim)
            {
                string m = kv.Key;
                if (!fu.shunzip.ContainsKey(m) || !fu.shunzis.ContainsKey(m)) continue;
                if (fu.shunzip[m] == 1 && fu.shunzis[m] == 1)
                    return new Yaku("三色同顺", fu.menqing ? 2 : 1);
            }
            return null;
        }

        private static Yaku yiqitongguan(FuResult fu)
        {
            if (!fu.shunzim.ContainsKey("123") || !fu.shunzim.ContainsKey("456") || !fu.shunzim.ContainsKey("789")) return null;
            if (fu.shunzim["123"] == 1 && fu.shunzim["456"] == 1 && fu.shunzim["789"] == 1)
                return new Yaku("一气通贯", fu.menqing ? 2 : 1);
            if (!fu.shunzip.ContainsKey("123") || !fu.shunzip.ContainsKey("456") || !fu.shunzip.ContainsKey("789")) return null;
            if (fu.shunzip["123"] == 1 && fu.shunzip["456"] == 1 && fu.shunzip["789"] == 1)
                return new Yaku("一气通贯", fu.menqing ? 2 : 1);
            if (!fu.shunzis.ContainsKey("123") || !fu.shunzis.ContainsKey("456") || !fu.shunzis.ContainsKey("789")) return null;
            if (fu.shunzis["123"] == 1 && fu.shunzis["456"] == 1 && fu.shunzis["789"] == 1)
                return new Yaku("一气通贯", fu.menqing ? 2 : 1);

            return null;
        }

        private static Yaku hunquandaiyaojiu(FuResult fu)
        {
            if (fu.n_yaojiu == 5 && fu.n_shunzi > 0 && fu.n_zipai > 0)
                return new Yaku("混全带幺九", fu.menqing ? 2 : 1);
            return null;
        }

        private static Yaku qiduizi(List<string> mianzi)
        {
            if (mianzi.Count == 7) return new Yaku("七对子", 2);
            return null;
        }

        private static Yaku duiduihu(FuResult fu)
        {
            if (fu.n_kezi == 4) return new Yaku("对对和", 2);
            return null;
        }

        private static Yaku sananke(FuResult fu)
        {
            if (fu.n_ankezi == 3) return new Yaku("三暗刻", 2);
            return null;
        }

        private static Yaku sangangzi(FuResult fu)
        {
            if (fu.n_gangzi == 3) return new Yaku("三杠子", 2);
            return null;
        }

        private static Yaku sansetongke(FuResult fu)
        {
            for (int n = 1; n <= 9; n++)
            {
                if (fu.kezim[n] + fu.kezip[n] + fu.kezis[n] == 3)
                    return new Yaku("三色同刻", 2);
            }
            return null;
        }

        private static Yaku hunlaotou(List<string> mianzi, FuResult fu)
        {
            if (fu.n_yaojiu == mianzi.Count
                && fu.n_shunzi == 0 && fu.n_zipai > 0)
                return new Yaku("混老头", 2);
            return null;
        }

        private static Yaku xiaosanyuan(List<string> mianzi, FuResult fu)
        {
            if (fu.keziz[5] + fu.keziz[6] + fu.keziz[7] == 2
                && Regex.IsMatch(mianzi[0], "^z[567]"))
                return new Yaku("小三元", 2);
            return null;
        }

        private static Yaku hunyise(List<string> mianzi, FuResult fu)
        {
            foreach (string s in new string[] { "m", "p", "s" })
            {
                string yise = "^[z" + s + "].*$";
                List<string> yisemianzi = new List<string>();
                foreach (string mz in mianzi)
                {
                    if (Regex.IsMatch(mz, yise))
                    {
                        yisemianzi.Add(mz);
                    }
                }
                if (yisemianzi.Count == mianzi.Count && fu.n_zipai > 0) return new Yaku("混一色", fu.menqing ? 3 : 2);
            }
            return null;
        }

        private static Yaku chunquandaiyaojiu(FuResult fu)
        {
            if (fu.n_yaojiu == 5 && fu.n_shunzi > 0 && fu.n_zipai == 0)
                return new Yaku("纯全带幺九", fu.menqing ? 3 : 2);
            return null;
        }

        private static Yaku erbeikou(FuResult fu)
        {
            if (!fu.menqing) return null;

            int beikou = 0;
            foreach (KeyValuePair<string, int> kv in fu.shunzim)
            {
                if (kv.Value > 3) beikou++;
                if (kv.Value > 1) beikou++;
            }
            foreach (KeyValuePair<string, int> kv in fu.shunzip)
            {
                if (kv.Value > 3) beikou++;
                if (kv.Value > 1) beikou++;
            }
            foreach (KeyValuePair<string, int> kv in fu.shunzis)
            {
                if (kv.Value > 3) beikou++;
                if (kv.Value > 1) beikou++;
            }
            if (beikou == 2) return new Yaku("二杯口", 3);
            return null;
        }

        private static Yaku qingyise(List<string> mianzi, FuResult fu)
        {
            foreach (string s in new string[] { "m", "p", "s" })
            {
                string yise = "^[" + s + "].*$";
                List<string> yisemianzi = new List<string>();
                foreach (string mz in mianzi)
                {
                    if (Regex.IsMatch(mz, yise))
                    {
                        yisemianzi.Add(mz);
                    }
                }
                if (yisemianzi.Count == mianzi.Count && fu.n_zipai == 0) return new Yaku("清一色", fu.menqing ? 6 : 5);
            }
            return null;
        }


    }

}
