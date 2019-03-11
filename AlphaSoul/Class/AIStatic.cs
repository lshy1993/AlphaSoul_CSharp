using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlphaSoul
{
    // ai 统计信息
    public class AIStatic
    {
        // 总局数
        public long plays { get; set; }
        // 1-4位率
        public double rate1 {
            get { return (double)sumRank[0] / (plays == 0 ? 1 : plays); }
        }
        public double rate2
        {
            get { return (double)sumRank[1] / (plays == 0 ? 1 : plays); }
        }
        public double rate3
        {
            get { return (double)sumRank[2] / (plays == 0 ? 1 : plays); }
        }
        public double rate4
        {
            get { return (double)sumRank[3] / (plays == 0 ? 1 : plays); }
        }
        private long[] sumRank = new long[] { 0, 0, 0, 0 };
        // 平均顺位
        public double aveRank {
            get {
                return (sumRank[0] + sumRank[1] * 2 + sumRank[2] * 3 + sumRank[3] * 4) / (double)(plays == 0 ? 1 : plays);
            }
        }
        // 被飞率
        public double rateMinus {
            get { return (double)sumMinus / (plays == 0 ? 1 : plays); }
        }
        private long sumMinus;


        // 总场数
        public long subplays { get; set; }
        // 最大连庄
        public int maxZhuang { get; set; }
        // 和了巡数
        public double aveXun {
            get { return (double)sumXun / (sumHu == 0 ? 1 : sumHu); }
        }
        private int sumXun;
        // 平均打点
        public double avePt {
            get {
                if (sumHu > 0) return ((double)sumPt / sumHu);
                return 0;
            }
        }
        private long sumPt;
        // 和了率
        public double rateHu
        {
            get {
                if (subplays > 0) return ((double)sumHu / subplays);
                return 0;
            }
        }
        private long sumHu;
        // 自摸率
        public double rateZimo {
            get { return (double)sumZimo / (sumHu == 0 ? 1 : sumHu); }
        }
        private long sumZimo;
        // 放铳率
        public double rateChong {
            get { return (double)sumChong / (subplays == 0 ? 1 : subplays); }
        }
        private long sumChong;
        // 副露率
        public double rateFulu {
            get { return (double)sumFulu / (subplays == 0 ? 1 : subplays); }
        }
        private long sumFulu;
        // 立直率
        public double rateLizhi {
            get { return (double)sumLizhi / (subplays == 0 ? 1 : subplays); }
        }
        private long sumLizhi;

        // 最近大铳
        public string maxPt
        {
            get { return getMaxPt(); }
        }

        public Dictionary<string,int> YakuDic { get; set; }

        private PtResult maxWin;
        private PtResult maxLose;

        public AIStatic()
        {
            maxWin = new PtResult();
            maxLose = new PtResult();
            YakuDic = new Dictionary<string, int>();
        }

        /// <summary>
        /// 胡牌结束
        /// </summary>
        public void Update(EndMessage msg)
        {
            PtResult res = msg.res;
            // 场数+1
            subplays++;
            if (msg.flag_fu) sumFulu++;
            if (msg.flag_lizhi) sumLizhi++;
            // 非自己胡牌则跳出
            if (res == null) return;
            if (msg.flag_zimo) sumZimo++;
            if (msg.flag_chong) sumChong++;
            sumHu++;
            sumPt += res.defen;
            sumXun += msg.xunshu;
            if(res.yiman > maxWin.yiman)
            {
                maxWin = res;
            }
            else if(res.defen > maxWin.defen)
            {
                maxWin = res;
            }
            // TODO 役种统计
            foreach(Yaku y in res.hupai)
            {
                string name = y.name;
                if (name.Contains("场风牌")) name = "场风牌";
                if (name.Contains("门风牌")) name = "门风牌";

                if (!YakuDic.ContainsKey(name))
                {
                    YakuDic.Add(name, 1);
                }
                else
                {
                    YakuDic[name]++;
                }

            }
        }

        /// <summary>
        /// 流局结束
        /// </summary>
        public void Update(LiujuMessage msg)
        {
            // 场数+1
            subplays++;
            if (msg.flag_fu) sumFulu++;
            if (msg.flag_li) sumLizhi++;
        }

        public void EndPlay(ZhongMessage msg)
        {
            // 总局数+1
            plays++;
            // 位次更新
            sumRank[msg.rank]++;
            if (msg.pt < 0) sumMinus++;
            maxZhuang = Math.Max(maxZhuang, msg.maxzhuang);
        }

        public string getMaxPt()
        {
            if (maxWin == null || maxWin.hupai.Count == 0) return "";
            string res = "";
            foreach(string ss in maxWin.mianzi.paizu)
            {
                res += ss + " ";
            }
            // 宝牌
            string i = "\r\n宝牌：";
            foreach (string ss in maxWin.bao) i += ss+" ";
            res += i;
            i = "里宝牌：";
            foreach (string ss in maxWin.bao) i += ss + " ";
            res += i + "\r\n";
            // 番
            res += maxWin.GetYaku();
            return res;
        }
    }
}
