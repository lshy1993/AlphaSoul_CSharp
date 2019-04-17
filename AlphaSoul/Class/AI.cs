using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlphaSoul
{
    public class AI_Core
    {
        public int ai_id;
        // 庄家位置
        public int qinjia;
        // 场风
        public int zhuangfeng;
        // 自风
        public int zifeng;
        // 局数
        public int jushu;
        // 储存场棒
        public int changbang;
        // 储存立直棒
        public int lizhibang;
        // 4家分数
        public int[] score;
        // 宝牌
        public List<string> bao;

        // 个人手牌堆
        private List<Pai> handStack;

        public List<string> _handStack;
        // 个人展露堆（含暗杠）
        public List<string> fuluStack;

        // 是否立直过
        //private bool flag_li;
        // 是否副露过
        //private bool flag_fu;

        // ai统计信息
        [Newtonsoft.Json.JsonIgnore()]
        public AIStatic ailog;

        private GameServer server;

        public AI_Core(int id, GameServer server = null)
        {
            ai_id = id;
            fuluStack = new List<string>();
            ailog = new AIStatic();
            this.server = server;
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
            _handStack = PaiMaker.GetCodeList(shoupai);
            fuluStack.Clear();
            if(server != null && server.allsockets.Count != 0)
            {
                server.InitStatus(ai_id, this);
            }
        }

        public object Action(string type, object data)
        {
            // AI消息接收层与返回策略
            if (server != null && server.allsockets.Count != 0)
            {
                return SocketAction(type, data);
            }
            if (type == "zimo") return zimo(data);
            else if (type == "fulu") return fulu(data);
            else if (type == "gang") return gang(data);
            else if (type == "gangzimo") return zimo(data);
            else if (type == "hule") hule(data);
            else if (type == "liuju") liuju(data);
            else if (type == "zhongju") zhongju(data);
            return new object();
        }

        public object SocketAction(string type, object data)
        {
            if (type == "zimo")
            {
                MopaiMessage msg = (MopaiMessage)data;
                JObject resObj = server.GetZimoRes(ai_id, msg.ToJsonData());
                if (resObj["type"].ToString() == "zimo")
                {
                    return new HuMessage(msg.mopai, ai_id);
                }
                // 暗加杠
                if (resObj["type"].ToString() == "gang")
                {
                    // 选择杠牌
                    return new FuluMessage(msg.mopai, ai_id);
                }
                // 流局
                if (resObj["type"].ToString() == "liuju")
                {
                    return new LiujuMessage(handStack);
                }
                // 切牌
                if (resObj["type"].ToString() == "qiepai")
                {
                    string pai = resObj["dapai"].ToString();
                    QiepaiMessage qm = new QiepaiMessage(pai, ai_id);
                    qm.lizhi = resObj["lizhi"].ToObject<bool>();
                    return qm;
                }
            }
            else if (type == "hule")
            {
                hule(data);
            }
            else if (type == "liuju") liuju(data);
            else if (type == "zhongju") zhongju(data);
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
            // 暗加杠
            if (msg.gang)
            {
                // 选择杠牌
                return new FuluMessage(msg.mopai, ai_id);
            }
            if (msg.lizhi_stat)
            {
                // 立直只允许摸切
                return new QiepaiMessage(msg.mopai.code, ai_id);
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
            int n_xiangting = TingJudger.xiangting(PaiMaker.GetCount(handStack), fuluStack);
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
                if (TingJudger.xiangting(newPaiCount, fuluStack) > n_xiangting) continue;
                // 选择
                var tingpai = TingJudger.tingpai(newPaiCount, fuluStack);
                int n_tingpai = tingpai.Count;

                // 选择 切出后 进张总类数最多的牌
                if (n_tingpai >= max)
                {
                    max = n_tingpai;
                    dapai = p;
                }
            }

            // 是否立直摸切
            handStack.Remove(dapai);
            QiepaiMessage qm = new QiepaiMessage(dapai.code, ai_id);
            qm.lizhi = msg.lizhi;
            return qm;

        }

        /// <summary>
        /// 他家丢牌时策略
        /// </summary>
        private object fulu(object data)
        {
            //和了
            FuluMessage msg = (FuluMessage)data;
            if (msg.hu)
            {
                return new HuMessage(msg.dapai, ai_id);
            }
            //副露
            else if (msg.fulu != null && msg.fulu.Count  != 0)
            {
                // TODO: ai副露选择

            }
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
            ailog.Update((EndMessage)data);
        }

        /// <summary>
        /// 单局结束（流局）
        /// </summary>
        private void liuju(object data)
        {
            // 统计工作
            ailog.Update((LiujuMessage)data);
        }

        /// <summary>
        /// 半庄结束
        /// </summary>
        private void zhongju(object data)
        {
            // 统计工作
            ailog.EndPlay((ZhongMessage)data);
        }


    }
}
