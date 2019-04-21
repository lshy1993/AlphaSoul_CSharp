using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
        // 统计剩余牌数
        private Dictionary<char, int[]> paishu;

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
            paishu = new Dictionary<char, int[]>();
            paishu.Add('s',new int[] { 1, 4, 4, 4, 4, 4, 4, 4, 4, 4 });
            paishu.Add('m', new int[] { 1, 4, 4, 4, 4, 4, 4, 4, 4, 4 });
            paishu.Add('p', new int[] { 1, 4, 4, 4, 4, 4, 4, 4, 4, 4 });
            paishu.Add('z', new int[] { 0, 4, 4, 4, 4, 4, 4, 4 });
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


        // 副露向听数判定
        private int fulu_xiangting(List<Pai> hand, List<string> fulu)
        {
            var shoupai = PaiMaker.GetCount(hand);
            /* 各役向けの向聴数のうち最低の向聴数を選択する */
            var menqing = xiangting_menqing(shoupai, fulu);
            var fanpai = xiangting_fanpai(shoupai, fulu);
            var duanyao = xiangting_duanyao(shoupai, fulu);
            var duidui = xiangting_duidui(shoupai, fulu);
            var yisem = xiangting_yise(shoupai, fulu, 'm');
            var yisep = xiangting_yise(shoupai, fulu, 'p');
            var yises = xiangting_yise(shoupai, fulu, 's');

            //console.log({"鸣牌":fulu,"门清":menqing,"役牌":fanpai,"断幺九":duanyao,"对对":duidui,"m清/混一色":yisem,"p清/混一色":yisep,"s清/混一色":yises});
            var ss = new int[] { menqing, fanpai, duanyao, duidui, yisem, yisep, yises };
            return ss.Min();
        }

        // 向听数-门清
        private int xiangting_menqing(Dictionary<char,int[]> shoupai, List<string> fulu)
        {
            // 有副露则无穷大
            foreach(string m in fulu)
            {
                if (Regex.IsMatch(m,@"[\-\+\=]")) return 999;
            }
            // 调用一般思路
            return TingJudger.xiangting(shoupai, fulu);
        }

        // 向听数-役牌
        private int xiangting_fanpai(Dictionary<char, int[]> shoupai, List<string> fulu)
        {
            int n_fanpai = 0;
            int back = 0;
            // 自风与场风 三元牌
            foreach (var n in new int[] { zhuangfeng + 1, zifeng + 1, 5, 6, 7 })
            {
                if (shoupai['z'][n] >= 3) n_fanpai++;
                // 存在可以碰的役牌
                else if (shoupai['z'][n] == 2 && paishu['z'][n] > 0) back = n;

                // 鳴ける可能性がある
                foreach (string m in fulu)
                {
                    if (m[0] == 'z' && (m[1]-'0') == n) n_fanpai++;
                }
            }
            if (n_fanpai != 0) return TingJudger.xiangting(shoupai, fulu);
            // 役牌可碰
            if (back > 0)
            {
                // 手牌を複製し、
                var new_shoupai = new Dictionary<char,int[]>(shoupai);            
                new_shoupai['z'][back] = 0;             // バック対象の牌で
                string num = back.ToString();
                List<string> new_fulu = new List<string>(fulu);
                new_fulu.Add('z' + num + num + num + '=');       // 明刻を作る
                // 汎用の向聴数計算ルーチンの結果に1加える
                return TingJudger.xiangting(new_shoupai, new_fulu) + 1;
            }
            return 999;
        }

        // 向听数-断19
        private int xiangting_duanyao(Dictionary<char, int[]> shoupai, List<string> fulu)
        {
            // 副露含有19牌则返回无限大
            foreach(string m in fulu)
            {
                if (Regex.IsMatch(m,@"^ z |[19] ")) return 999;
            }
            // 手牌を複製し
            var new_shoupai = new Dictionary<char, int[]>(shoupai);
            foreach(KeyValuePair<char,int[]>kv in new_shoupai)
            {
                char s = kv.Key;
                int[] tt = kv.Value;
                if (s == 'z') {
                    // 字牌はすべて不要
                    tt = new int[] { 0, 0, 0, 0, 0, 0, 0, 0 };
                    continue;
                }
                // 一九牌を引き抜く
                tt[1] = 0;
                tt[9] = 0;
            }
            // 汎用の向聴数計算ルーチンに処理を任せる
            return TingJudger.xiangting(new_shoupai, fulu);
        }

        // 向听数-对对胡
        private int xiangting_duidui(Dictionary<char, int[]> shoupai, List<string> fulu)
        {
            // 副露顺子向听无限大
            foreach(string m in fulu)
            {
                if (Regex.IsMatch(m,@"^[mpsz](\d)\1\1 ")) return 999;
            }
            /* 刻子(槓子を含む)と対子の数を数える */
            int n_kezi = fulu.Count;
            int n_duizi = 0;
            foreach (KeyValuePair<char,int[]>kv in shoupai)
            {
                var bingpai = kv.Value;
                for (var n = 1; n < bingpai.Length; n++)
                {
                    if (bingpai[n] >= 3) n_kezi++;
                    if (bingpai[n] == 2) n_duizi++;
                }
            }
            // 搭子过多修正
            if (n_kezi + n_duizi > 5) n_duizi = 5 - n_kezi;
            // 向聴数を計算
            return 8 - n_kezi * 2 - n_duizi;
        }

        private int xiangting_yise(Dictionary<char, int[]> shoupai, List<string> fulu, char sort)
        {
            /* sort 以外の色の副露がある場合、向聴数は無限大 */
            var regexp = new Regex("^[^z" + sort + "]");
            foreach(string m in fulu)
            {
                if (regexp.IsMatch(m)) return 999;
            }
            /* 手牌を複製し、sort 以外の色の牌をすべて引き抜く */
            var new_shoupai = new Dictionary<char, int[]>(shoupai);
            foreach(char s in new char[] {'s','m','p' })
            {
                if (s != sort)
                {
                    new_shoupai[s] = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                }
            }
            // 汎用の向聴数計算ルーチンに処理を任せる
            return TingJudger.xiangting(new_shoupai, fulu);
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
            int n_xiangting = fulu_xiangting(handStack, fuluStack);
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
                if (fulu_xiangting(newhand, fuluStack) > n_xiangting) continue;
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
            FuluMessage msg = (FuluMessage)data;
            // 和了
            if (msg.hu)
            {
                return new HuMessage(msg.dapai, ai_id);
            }
            //副露
            else if (msg.fulu != null && msg.fulu.Count  != 0)
            {
                int n_xiangting = fulu_xiangting(handStack, fuluStack);
                if (n_xiangting >= 0 && n_xiangting < 3)
                {
                    var max = EvalHand(handStack, fuluStack, paishu);
                    FuluMianzi fulou = null;
                    // 计算每种副露形式的收益
                    foreach (FuluMianzi m in msg.fulu)
                    {
                        var new_handStack = PaiMaker.GetFuluOff(handStack, m);
                        var new_fuluStack = PaiMaker.GetFulu(fuluStack, m);
                        // 副露后还是3向听以上则不选用
                        int fxt = fulu_xiangting(new_handStack, new_fuluStack);
                        if (fxt >= 3) continue;
                        // TODO: 评价手牌
                        int ev = EvalHand(new_handStack, new_fuluStack, paishu);
                        if (ev > max)
                        {
                            max = ev;
                            fulou = m;
                        }
                    }
                    // 发回服务端
                    if(fulou != null)
                    {
                        var cb = new ReturnMessage();
                        cb.from = this.ai_id;
                        cb.type = fulou.type;
                        cb.combination = fulou.combination;
                        return cb;
                    }
                }
                // 默认不副露
                return new ReturnMessage();
            }
        }


        // 综合评价手牌
        private int EvalHand(List<Pai> handStack, List<string> fuluStack, Dictionary<char,int[]> paishu)
        {
            // 当前向听数
            var n_xiangting = fulu_xiangting(handStack, fuluStack);
            // 已经和牌情形
            if (n_xiangting == -1)
            {
                return GetFen(handStack, fuluStack);
            }
            // 如果是需要切牌的状态
            if (handStack.Count + fuluStack.Count * 3 == 14)
            {
                // 摸牌/副露后 选择不减少向听数里最大的
                var max = 0;
                var plist = this.PickDapai(handStack, fuluStack);
                foreach (var p in plist)
                {
                    // 克隆一副手牌
                    var new_handStack = new List<Pai>(handStack);
                    new_handStack.Remove(p);
                    if (this.fulu_xiangting(new_handStack, fuluStack) > n_xiangting) continue;
                    var r = this.EvalHand(new_handStack, fuluStack, paishu);
                    if (r > max) max = r;
                }
                return max;
            }
            // 3向听以内
            if (n_xiangting < 3)
            {
                // 价值为 进章的得分*枚数    
                var r = 0;
                var tingpai = this.fulu_tingpai(handStack, fuluStack);
                for (var p of tingpai)
                {
                    let num = p[0];
                    let ch = p[1];
                    if (paishu[ch][num] == 0) continue;
                    var new_shoupai = handStack.concat(p + '_');
                    paishu[ch][num]--;
                    // 继续搜索
                    var ev = this.EvalHand(new_shoupai, fuluStack, paishu);
                    paishu[ch][num]++;
                    r += ev * paishu[ch][num];
                }
                return r;
            }
            else
            {
                /* 3向聴以前の場合は今までのアルゴリズムで評価 */
                var r = 0;
                for (var p of this.fulu_tingpai(handStack, fuluStack))
                {
                    let num = p[0];
                    let ch = p[1];
                    if (paishu[ch][num] == 0) continue;
                    r += paishu[ch][num] * (p[2] == '+' ? 4 : p[2] == '-' ? 2 : 1);
                }
                return r;
            }

        }

        // 从目前手牌中挑选能够打出的牌
        private List<Pai> PickDapai(List<Pai> handStack, List<string> fuluStack)
        {
            var pai = new List<Pai>();
            // 禁手
            var deny = new Dictionary<string,bool>();
            string mopai = "";
            if (fuluStack.Count > 0)
            {
                mopai = fuluStack[fuluStack.Count - 1];
            }
            bool flag = !string.IsNullOrEmpty(mopai) && Regex.IsMatch(mopai, @"\d(?=[\-\+\=])");
            var n = Convert.ToInt32(Regex.Match(mopai, @"\d(?=[\-\+\=])").ToString());
            // 设置副露吃碰后的禁手
            if (flag)
            {
                char s = mopai[mopai.Length - 1];
                if (deny.ContainsKey(s+n.ToString())) deny[s + n.ToString()] = true;
                else deny.Add(s + n.ToString(), true);
                if (!Regex.IsMatch(mopai,@"^[mpsz](\d)\1\1.*$"))
                {
                    if (n < 7 && mopai.match(/^[mps]\d\-\d\d$/)) deny[(n + 3) + s] = true;
                    if (3 < n && mopai.match(/^[mps]\d\d\d\-$/)) deny[(n - 3) + s] = true;
                }
            }
            var pCount = PaiMaker.GetCount(handStack);
            foreach (KeyValuePair<char,int[]> kv in pCount)
            {
                var bingpai = pCount[s];
                for (var n = 1; n < bingpai.length; n++)
                {
                    if (bingpai[n] == 0) continue;
                    if (deny[n + s]) continue;
                    if (n != 5)
                    {
                        pai.push(n + s);
                    }
                    else
                    {
                        if (bingpai[0] > 0) pai.push('0' + s);
                        if (bingpai[0] < bingpai[5]) pai.push('5' + s);
                    }
                }
            }
            return pai;
        }

        private int GetFen(List<Pai> handStack, List<string> fuluStack)
        {
            // 预估胡牌分用参数
            PtParam param = new PtParam();
            param.changfeng = zhuangfeng;
            param.zifeng = zifeng;
            param.lizhi = 0;
            param.yifa = false;
            param.qianggang = false;
            param.lingshang = false;
            param.changbang = changbang;
            param.lizhibang = lizhibang;
            param.baopai = bao;
            param.libaopai = new List<string>();
            param.haidi = 0;
            param.tianhu = 0;
            // 价值则为胡牌得分
            var hupai = handStack[handStack.Count - 1];
            var ptrest = PtJudger.GetFen(handStack, fuluStack, hupai, param);
            return ptrest.defen;
        }

        // 求可以（进张）听的牌
        private List<Pai> fulu_tingpai(List<Pai> hand, List<string> fulu)
        {
            var pai = new List<Pai>();
            // 原先向听数
            var n_xiangting = this.fulu_xiangting(hand, fulu);
            var paiCount = PaiMaker.GetCount(hand);
            foreach (KeyValuePair<char, int[]> kv in paiCount)
            {
                char ch = kv.Key;
                var bingpai = paiCount[ch];
                for (var n = 1; n < bingpai.Length; n++)
                {
                    if (bingpai[n] >= 4) continue;
                    var new_hand = new List<Pai>(hand);
                    new_hand.Add(new Pai(ch, n));
                    if (this.fulu_xiangting(new_hand, fulu) < n_xiangting)
                    {
                        pai.Add(new Pai(ch, n));
                        if (n == 5 && this.paishu[ch][0] > 0)
                        {
                            pai.Add(new Pai(ch, 0));
                        }
                    }
                }
            }
            return pai;
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
