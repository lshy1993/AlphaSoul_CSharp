using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlphaSoul
{
    class AI_Core
    {
        public int ai_id;
        // 庄家位置
        private int qinjia;
        // 场风
        private int zhuangfeng;
        // 自风
        private int zifeng;
        // 局数
        private int jushu;
        // 储存场棒
        private int changbang;
        // 储存立直棒
        private int lizhibang;
        // 4家分数
        private int[] score;
        // 宝牌 里宝牌
        private List<string> bao;

        // 个人信息
        private List<Pai> handStack;
        private List<string> fulu;
        //private Pai mopai;

        public AI_Core(int id)
        {
            ai_id = id;
            fulu = new List<string>();
        }

        // 设置该局信息
        public void InitStatus(GameStatus gs)
        {
            qinjia = gs.qinjia;
            zhuangfeng = gs.changfeng;
            zifeng = gs.playerWind[ai_id];
            jushu = gs.jushu;
            changbang = gs.changbang;
            lizhibang = gs.lizhibang;
            score = gs.score;
            bao = gs.bao;
        }

        // 重新由系统发牌
        public void InitStack(List<Pai> shoupai)
        {
            handStack = new List<Pai>(shoupai);
            fulu.Clear();
        }

        public object Action(string type, object data)
        {
            // AI消息接收层与返回策略
            if (type == "zimo") return zimo(data);
            else if (type == "fulou") return dapai(data);
            else if (type == "gang") return gang(data);
            else if (type == "gangzimo") return zimo(data);
            else if (type == "hule") hule(data);
            else if (type == "liuju") liuju(data);
            else if (type == "jieju") jieju(data);
            return new object();
        }


        /// <summary>
        /// 摸牌时策略选择
        /// </summary>
        private object zimo(object data)
        {
            // 摸牌时可能的选项
            MopaiMessage msg = (MopaiMessage)data;
            if (msg.zimo)
            {
                // 能胡牌就胡
                return new HuMessage(msg.mopai, ai_id);
            }
            // 杠
            if (msg.gang)
            {
                // 选择杠牌
                return new GangMessage(msg.mopai,1);
            }
            if (msg.lizhi_stat)
            {
                // 立直只允许摸切
                return new QiepaiMessage(msg.mopai, ai_id);
            }
            // 流局
            if (msg.liuju)
            {
                return new LiujuMessage(handStack);
            }

            // 切牌  TODO 决策选择？
            //默认要打出去的牌为摸进牌
            Pai dapai = msg.mopai;
            handStack.Add(dapai);
            // 计算当前向听数
            int n_xiangting = TingJudger.xiangting(PaiMaker.GetCount(handStack), fulu);
            int max = 0;
            // 遍历手牌
            for (int i = 0; i < handStack.Count; i++)
            {
                Pai p = handStack[i];
                // 打牌可能な牌について以下を行う
                List<Pai> newhand = new List<Pai>(handStack);
                newhand.Remove(p);
                Dictionary<char, int[]> newPaiCount = PaiMaker.GetCount(newhand);
                // 不选择向听数减少的情形？
                if (TingJudger.xiangting(newPaiCount, fulu) > n_xiangting) continue;
                // 选择
                var tingpai = TingJudger.tingpai(newPaiCount, fulu);
                int n_tingpai = tingpai.Count;

                // 选择 切出后 进张总类数最多的牌
                if (n_tingpai >= max)
                {
                    max = n_tingpai;
                    dapai = p;
                }
            }
            // 直接摸切
            handStack.Remove(dapai);
            return new QiepaiMessage(dapai, ai_id);

        }

        /// <summary>
        /// 他家丢牌时策略
        /// </summary>
        private object dapai(object data)
        {
            //和了
            FuluMessage msg = (FuluMessage)data;
            if(msg.hu == true)
            {
                return new HuMessage(msg.dapai, ai_id);
            }
            //副露
            return new object();
        }

        /// <summary>
        /// 他家明杠时策略(抢杠)
        /// </summary>
        private object gang(object data)
        {
            return "hule";
        }

        /// <summary>
        /// 单局结束（和了）
        /// </summary>
        private void hule(object data)
        {
            // 统计工作
        }

        /// <summary>
        /// 单局结束（流局）
        /// </summary>
        private void liuju(object data)
        {
            // 统计工作
        }

        /// <summary>
        /// 南风结束
        /// </summary>
        private void jieju(object data)
        {
            // 统计工作
        }


    }
}
