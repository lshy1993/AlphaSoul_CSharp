using System;
using System.Collections.Generic;
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
        public double[] rate { get; set; } = new double[] { 0, 0, 0, 0 };
        private long[] sumRank = new long[] { 0, 0, 0, 0 };
        // 平均顺位
        public double aveRank {
            get {
                return (sumRank[0] + sumRank[1] * 2 + sumRank[2] * 3 + sumRank[3] * 4) / (plays == 0 ? 1 : plays);
            }
        }
        // 被飞率
        public double rateMinus {
            get { return sumMinus / (plays == 0 ? 1 : plays); }
        }
        private long sumMinus;


        // 总场数
        private long subplays;
        // 最大连庄
        public int maxZhuang { get; set; }
        // 和了巡数
        public double aveXun {
            get { return sumXum / (sumHu == 0 ? 1 : sumHu); }
        }
        private int sumXum;
        // 平均打点
        public double avePt {
            get {
                return sumPt / (sumHu == 0 ? 1 : sumHu);
            }
        }
        private long sumPt;
        // 和了率
        public double rateHu {
            get { return sumHu / (subplays == 0 ? 1 : subplays); }
        }
        private long sumHu;
        // 自摸率
        public double rateZimo {
            get { return sumZimo / (sumHu == 0 ? 1 : sumHu); }
        }
        private long sumZimo;
        // 放铳率
        public double rateChong {
            get { return sumChong / (subplays == 0 ? 1 : subplays); }
        }
        private long sumChong;
        // 副露率
        public double rateFulu {
            get { return sumFulu / (subplays == 0 ? 1 : subplays); }
        }
        private long sumFulu;
        // 立直率
        public double rateLizhi {
            get { return sumLizhi / (subplays == 0 ? 1 : subplays); }
        }
        private long sumLizhi;

        // 最近大铳
        public string maxPt
        {
            get { return getMaxPt(); }
        }
        private PtResult maxWin;
        private PtResult maxLose;

        public AIStatic()
        {
            maxWin = new PtResult();
            maxLose = new PtResult();
        }

        /// <summary>
        /// 胡牌结束
        /// </summary>
        public void Update(PtResult res, bool lizhi, bool fulu)
        {
            // 场数+1
            subplays++;
            if (fulu) sumFulu += 1;
            if (lizhi) sumLizhi += 1;
            sumHu += 1;
        }

        /// <summary>
        /// 流局结束
        /// </summary>
        public void Update(bool lizhi, bool fulu)
        {
            // 场数+1
            subplays++;
            if (fulu) sumFulu += 1;
            if (lizhi) sumLizhi += 1;
        }

        public void EndPlay()
        {
            // 总局数+1
            plays++;
            // 位次更新
        }

        public string getMaxPt()
        {
            if (maxWin == null || maxWin.hupai.Count == 0) return "";
            string res = "";
            foreach(Yaku mz in maxWin.hupai)
            {
                res += string.Format("{0}: {1}\r\n", mz.name, mz.fanshu);
            }
            return res + maxWin.defen;
        }
    }
}
