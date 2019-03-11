using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace AlphaSoul
{
    /// <summary>
    /// 负责牌山生成 发牌 可执行操作的通知
    /// </summary>
    public class Game
    {
        public MainWindow mw;
        // 总牌山
        public ObservableCollection<Pai> yama = new ObservableCollection<Pai>();
        // 玩家手牌
        public List<Pai>[] PaiStack = new List<Pai>[4];
        // 牌河
        public ObservableCollection<Pai>[] River = new ObservableCollection<Pai>[4];
        // 鸣牌区域
        //public ObservableCollection<Pai>[] Fulu = new ObservableCollection<Pai>[4];
        public List<string>[] Fulu = new List<string>[4];

        // 牌山位置指示
        private int yamaPos, yamaLast;
        private int baoPos;
        // 风位置
        private int curWind = 0;
        // 终局指示
        private bool endGame;

        private int loop = 1000;

        // 当前对局信息
        public GameStatus curStatus;
        // 历史对局
        public ObservableCollection<GameHistory> history = new ObservableCollection<GameHistory>();

        private AI_Core[] player;

        public Game(MainWindow mw)
        {
            yama = PaiMaker.ShufflePai();
            this.mw = mw;
            for (int p = 0; p < 4; p++)
            {
                PaiStack[p] = new List<Pai>();
                River[p] = new ObservableCollection<Pai>();
                //Fulu[p] = new ObservableCollection<Pai>();
                Fulu[p] = new List<string>();
            }
            // 四个AI
            player = new AI_Core[4] { new AI_Core(0), new AI_Core(1), new AI_Core(2), new AI_Core(3) };
        }

        public void LoopGame()
        {
            for (int i = 0; i < loop; i++) NewGame();
        }

        // 游戏开始
        public void NewGame()
        {
            // 游戏参数初始化
            curStatus = new GameStatus();
            mw.Dispatcher.Invoke(new Action(() =>
            {
                history.Clear();
            }));
            // 半庄开始
            endGame = false;
            while (!endGame)
            {
                // 初始化
                NewWind();
                // 开始对局
                PlayWind();
            }
            // 计算pt与排名
            List<PtRank> rank = new List<PtRank>();
            for(int i = 0; i < 4; i++)
            {
                int score = curStatus.score[i];
                //int feng = curStatus.playerParam[i].zifeng;
                int feng = curStatus.zifeng[i];
                rank.Add(new PtRank(score, feng, i));
            }
            rank.Sort();
            rank.Reverse();
            // 发送终局消息
            for (int i = 0; i < 4; i++)
            {
                PtRank rk = rank[i];
                ZhongMessage zmsg = new ZhongMessage(rk.score, i, curStatus.maxlian[i]);
                player[rk.aid].Action("zhongju", zmsg);
            }
            mw.Dispatcher.Invoke(new Action(() =>
            {
                mw.RefreshAIUI();
                mw.aiTextBlock.Text = string.Format("半庄结束！");
            }));
        }

        public AIStatic getStatic(int d = 0)
        {
            return player[d].ailog;
        }

        /// <summary>
        /// 初始化开始一局
        /// </summary>
        private void NewWind()
        {
            // 随机生成新的牌山
            yama = PaiMaker.ShufflePai();
            // 指向牌顶
            yamaPos = -1;
            // 指向杠牌
            yamaLast = 135;
            // 宝牌位置
            baoPos = 130;

            // 翻开第一张宝牌 (宝牌倒着开)
            curStatus.bao = new List<string>();
            curStatus.libao = new List<string>();
            curStatus.bao.Add(yama[baoPos].code);
            curStatus.libao.Add(yama[baoPos + 1].code);
            yama[baoPos].stat = 10;
            yama[baoPos + 1].stat = 11;
            // 下一个宝牌位置
            baoPos -= 2;
            // 玩家状态重置
            

            // 玩家手牌清空
            for (int p = 0; p < 4; p++)
            {
                PaiStack[p].Clear();
                River[p].Clear();
                Fulu[p].Clear();
            }
            // 摸4轮牌
            for (int t = 0; t < 4; t++)
            {
                // 4个玩家按照自风依次
                foreach (int p in curStatus.playerWind)
                {
                    if (t < 3)
                    {
                        // 前3轮 每轮连续摸4张
                        for (int i = 0; i < 4; i++)
                        {
                            yamaPos++;
                            yama[yamaPos].stat = p + 1;
                            Pai mpai = yama[yamaPos];
                            PaiStack[p].Add(mpai);
                        }
                    }
                    else
                    {
                        //第四轮
                        yamaPos++;
                        yama[yamaPos].stat = p + 1;
                        Pai mpai = yama[yamaPos];
                        PaiStack[p].Add(mpai);
                    }

                }
            }
            // ui显示
            //FreshUI();
        }

        private void PlayWind()
        {
            // 发送初始化信息给player
            foreach (int playerid in curStatus.playerWind)
            {
                player[playerid].InitStatus(curStatus);
                player[playerid].InitStack(PaiStack[playerid]);
            }
            // 自动牌局 东风起手
            curWind = 0;
            bool endSection = false;
            bool lianzhuang = false;
            object callback = null;
            // 主循环
            while (!endSection)
            {
                // ui显示
                //FreshUI(1500);
                // 处理player的回应
                if (callback == null)
                {
                    int aid = curStatus.playerWind[curWind];
                    // 从牌顶 摸牌
                    yamaPos++;
                    Pai mo = yama[yamaPos];
                    yama[yamaPos].stat = curWind + 1;
                    // 手牌+1
                    PaiStack[aid].Add(mo);
                    // 通知生成（暗杠 立直 胡牌 流局）
                    MopaiMessage mmsg = allow_zimo(aid, mo);
                    // 向玩家发送消息
                    callback = player[aid].Action("zimo", mmsg);
                    // 巡数+1
                    curStatus.xunshu[aid]++;
                }
                else if (callback.GetType() == typeof(HuMessage))
                {
                    // 单人自摸
                    HuMessage hmsg = (HuMessage)callback;
                    List<HuMessage> dic = new List<HuMessage>();
                    dic.Add(hmsg);
                    lianzhuang = HuLe(dic);
                    endSection = true;
                }
                else if (callback.GetType() == typeof(QiepaiMessage))
                {
                    // 玩家切牌
                    QiepaiMessage qm = (QiepaiMessage)callback;
                    int playerid = qm.from;
                    Pai dapai = qm.qiepai;
                    PaiStack[playerid].Remove(dapai);
                    // 收到立直信号
                    if (qm.lizhi)
                    {
                        //curStatus.playerParam[playerid].lizhi = curStatus.diyizimo[playerid] ? 2 : 1;
                        curStatus.lizhi[playerid] = curStatus.diyizimo[playerid] ? 2 : 1;
                    }
                    // 第一自摸状态结束
                    if (curStatus.diyizimo[playerid]) curStatus.diyizimo[playerid] = false;

                    // 如果有玩家可以副露 则发送消息
                    List<HuMessage> mlist_hu = new List<HuMessage>();
                    List<GangMessage> mlist_gang = new List<GangMessage>();
                    List<PengMessage> mlist_peng = new List<PengMessage>();
                    List<ChiMessage> mlist_chi = new List<ChiMessage>();
                    foreach (int id in curStatus.playerWind)
                    {
                        object subcallback = null;
                        if (id != playerid)
                        {
                            FuluMessage fmsg = allow_fulu(id, dapai);
                            subcallback = player[id].Action("fulu", fmsg);
                        }
                        if (subcallback == null) continue;
                        if (subcallback.GetType() == typeof(HuMessage))
                        {
                            mlist_hu.Add((HuMessage)subcallback);
                        }
                        else if (subcallback.GetType() == typeof(GangMessage))
                        {
                            mlist_gang.Add((GangMessage)subcallback);
                        }
                        else if (subcallback.GetType() == typeof(PengMessage))
                        {
                            mlist_peng.Add((PengMessage)subcallback);
                        }
                        else if (subcallback.GetType() == typeof(ChiMessage))
                        {
                            mlist_chi.Add((ChiMessage)subcallback);
                        }
                    }
                    // 判定优先级 胡>碰>吃
                    if (mlist_hu.Count > 0)
                    {
                        lianzhuang = HuLe(mlist_hu);
                        endSection = true;
                    }
                    else
                    {
                        // 立直委托棒
                        if (qm.lizhi)
                        {
                            curStatus.score[playerid] -= 1000;
                            curStatus.lizhibang++;
                        }

                        if (mlist_gang.Count == 1)
                        {
                            // 明杠
                            curStatus.diyizimo = new bool[] { false, false, false, false };
                        }
                        else if (mlist_peng.Count == 1)
                        {
                            // 碰
                            curStatus.diyizimo = new bool[] { false, false, false, false };
                        }
                        else if (mlist_chi.Count == 1)
                        {
                            // 吃
                            curStatus.diyizimo = new bool[] { false, false, false, false };
                        }
                        else
                        {
                            // 无人回应 则弃牌进入牌河
                            River[playerid].Add(dapai);
                            dapai.stat = curStatus.zifeng[playerid] + 5;
                            // 超过第一巡则两立状态取消

                            // 4家立直
                            if (curStatus.lizhi[0] > 0 && curStatus.lizhi[1] > 0
                            && curStatus.lizhi[2] > 0 && curStatus.lizhi[3] > 0)
                            {
                                // 向所有玩家发送流局（结束）消息
                                LiujuMessage lj = new LiujuMessage(1);
                                foreach (int id in curStatus.playerWind)
                                {
                                    player[id].Action("liuju", lj);
                                }
                                GameHistory gg = new GameHistory();
                                gg.wind = curStatus.GetWind();
                                gg.zhuang = curStatus.playerWind[0];
                                gg.res = "4家立直";
                                gg.pt = lj.fenpei;
                                mw.Dispatcher.Invoke(new Action(() =>
                                {
                                    history.Add(gg);
                                }));
                                lianzhuang = true;
                                endSection = true;
                            }
                            else if (yamaPos == yamaLast - 14)
                            {
                                lianzhuang = Liuju();
                                endSection = true;
                            }
                            else
                            {
                                // 轮到下一个风玩家
                                curWind += 1;
                                if (curWind > 3) curWind = 0;
                                callback = null;
                            }
                        }

                    }

                }
                else if (callback.GetType() == typeof(JiuMessage))
                {
                    // 玩家主动选择流局
                    LiujuMessage lj = new LiujuMessage();
                    // 向所有玩家发送流局（结束）消息
                    foreach (int id in curStatus.playerWind)
                    {
                        player[id].Action("liuju", lj);
                    }
                    GameHistory gg = new GameHistory();
                    gg.wind = curStatus.GetWind();
                    gg.res = "九种九牌";
                    mw.Dispatcher.Invoke(new Action(() => {
                        history.Add(gg);
                    }));
                    endSection = true;
                }
                else if (callback.GetType() == typeof(GangMessage))
                {
                    GangMessage gmsg = (GangMessage)callback;
                    int aid = gmsg.from;
                    // 玩家自己 暗杠 加杠
                    if (gmsg.type == 2)
                    {
                        // 暗杠 只有国士可以抢

                    }
                    else if (gmsg.type == 1)
                    {
                        // 明杠 加杠 需要判定抢杠
                    }
                    else
                    {
                        // 没有抢杠 向该玩家发送 杠牌
                        // 从牌底 摸牌
                        Pai mo = yama[yamaLast];
                        //mo.stat = curStatus.playerParam[aid].zifeng + 1;
                        mo.stat = curStatus.zifeng[aid] + 1;
                        yamaLast--;
                        PaiStack[aid].Add(mo);
                        // 胡牌杠消息生成
                        MopaiMessage mmsg = allow_zimo(aid, mo);
                        // 向玩家发送摸牌
                        callback = player[gmsg.from].Action("zimo", mmsg);
                    }

                }
                else if (callback.GetType() == typeof(FuluMessage))
                {
                    // 玩家选择了副露
                }
                // 记录牌谱
            }
            // 结束整个对局
            if (curStatus.score.Min() < 0)
            {
                // 有人被飞
                endGame = true;
            }
            else if (curStatus.changfeng == 2)
            {
                // 西入时，超过30000即结束
                endGame = (curStatus.qinjia == 3 || curStatus.score.Max() >= 30000);
            }
            else if (curStatus.changfeng >= 1 && curStatus.qinjia == 3)
            {
                // 南4判定
                if(lianzhuang)
                {
                    int aid = curStatus.playerWind[0];
                    // 连庄 亲家若第一 结束
                    endGame = curStatus.score[aid] == curStatus.score.Max();
                }
                else
                {
                    endGame = curStatus.score.Max() >= 30000;
                }
            }
            //FreshUI();

            // 流庄后 重新分配风
            curStatus.NextPlayer(lianzhuang);
            
        }

        /// <summary>
        /// 给每个玩家加分
        /// </summary>
        /// <returns>按ai顺序的分</returns>
        private int[] SetFenpei(int[] fenpei)
        {
            int[] fen = new int[4];
            // 四个风
            for (int i = 0; i < 4; i++)
            {
                int aid = curStatus.playerWind[i];
                curStatus.score[aid] += fenpei[i];
                fen[aid] = fenpei[i];
            }
            return fen;
        }

        /// <summary>
        /// 单（多）个和了处理
        /// </summary>
        /// <param name="mlist_hu"></param>
        private bool HuLe( List<HuMessage> mlist_hu)
        {
            FreshUI();
            // 最终点数
            int[] final = new int[4];
            // 回送消息列队
            Dictionary<int, EndMessage> mlist_end = new Dictionary<int, EndMessage>();
            // 判定标识
            int chongjia = -1;
            bool lianzhuang = false;
            bool zimo = false;
            string serverLog = "";
            foreach (HuMessage hm in mlist_hu)
            {
                int aid = hm.from;
                chongjia = hm.rongpai.stat - 1;
                // 合计点数
                List<Pai> npstack = new List<Pai>(PaiStack[aid]);
                if (npstack.Count == 13) npstack.Add(hm.rongpai);
                PtResult ptr = new PtJudger().Judge(npstack, hm.rongpai, Fulu[aid], curStatus.getPlayerParam(aid));
                serverLog += PaiMaker.GetDisp(npstack)+"\r\n";
                serverLog += ptr.GetYaku();
                // 分数加和
                for (int i = 0; i < 4; i++) final[i] += ptr.fenpei[i];
                // 若aid的自风为东，则连庄
                if (curStatus.zifeng[aid] == 0) lianzhuang = true;
                // 消息队列
                EndMessage emsg = new EndMessage(curStatus.getPlayerParam(aid));
                emsg.res = ptr;
                emsg.xunshu = curStatus.xunshu[aid];
                if (curStatus.zifeng[aid] == chongjia)
                {
                    emsg.flag_zimo = true;
                    zimo = true;
                }
                mlist_end.Add(aid, emsg);
            }
            // 分数变动
            int[] fen = SetFenpei(final);
            // 立直棒归0
            curStatus.lizhibang = 0;
            // 记录
            GameHistory gg = new GameHistory();
            gg.zhuang = curStatus.playerWind[0];
            gg.wind = curStatus.GetWind();
            gg.res += zimo ? "自摸" : "荣和";
            gg.pt = fen;
            //for (int i = 0; i < 4; i++) gg.pt[i] += fen[i];
            // 向所有的玩家发送结束消息
            foreach (int id in curStatus.playerWind)
            {
                EndMessage emsg;
                if (mlist_end.ContainsKey(id))
                {
                    emsg = mlist_end[id];
                }
                else
                {
                    //emsg = new EndMessage(curStatus.playerParam[id]);
                    emsg = new EndMessage(curStatus.getPlayerParam(id));
                }
                emsg.flag_chong = (id == chongjia);
                //emsg.flag_lizhi = curStatus.playerParam[id].lizhi > 0;
                emsg.flag_lizhi = curStatus.lizhi[id] > 0;
                emsg.flag_fu = Fulu[id].Count > 0;
                player[id].Action("hule", emsg);
            }
            mw.Dispatcher.Invoke(new Action(() =>
            {
                history.Add(gg);
                mw.RefreshAIUI();
                mw.aiTextBlock.Text = serverLog;
            }));
            return lianzhuang;
        }

        /// <summary>
        /// 荒牌流局
        /// </summary>
        private bool Liuju()
        {
            bool lianzhuang = false;
            // 向所有玩家发送荒牌流局
            LiujuMessage lj = new LiujuMessage(3);
            int[] ting = new int[4];
            string serverLog = "";
            for (int i = 0; i < 4; i++)
            {
                int pid = curStatus.playerWind[i];
                lj.shoupai[i] = PaiMaker.GetCode(PaiStack[pid]);
                //serverLog += string.Format("{0}: {1}\r\n", i, lj.shoupai[i]);
                serverLog += string.Format("{0}: {1}\r\n", i, PaiMaker.GetDisp(PaiStack[pid]));
                ting[pid] = TingJudger.xiangting(PaiMaker.GetCount(PaiStack[pid]), Fulu[pid]) == 0 ? 1 : 0;
            }
            lianzhuang = ting[curStatus.playerWind[0]] == 1;
            // 未听牌点数更新
            if (ting.Sum() > 0 && ting.Sum() < 4)
            {
                for (int i = 0; i < 4; i++)
                {
                    lj.fenpei[i] = ting[i] == 1 ? 3000 / ting.Sum()
                           : -3000 / (4 - ting.Sum());
                    curStatus.score[i] += ting[i] == 1 ? 3000 / ting.Sum()
                           : -3000 / (4 - ting.Sum());
                }
            }
            // 向所有玩家发送流局（结束）消息
            foreach (int id in curStatus.playerWind)
            {
                player[id].Action("liuju", lj);
            }
            GameHistory gg = new GameHistory();
            gg.wind = curStatus.GetWind();
            gg.zhuang = curStatus.playerWind[0];
            gg.res = "流局";
            gg.pt = lj.fenpei;
            mw.Dispatcher.Invoke(new Action(() =>
            {
                history.Add(gg);
                mw.RefreshAIUI();
                mw.aiTextBlock.Text = serverLog;
            }));
            return lianzhuang;
        }


        /// <summary>
        /// 生成摸牌消息
        /// </summary>
        /// <param name="aid">玩家id</param>
        /// <param name="mopai">摸牌</param>
        private MopaiMessage allow_zimo(int aid, Pai mopai )
        {
            // 获取信息
            List<Pai> plist = PaiStack[aid];
            List<string> fulu = Fulu[aid];
            //PtParam param = curStatus.playerParam[aid];
            PtParam param = curStatus.getPlayerParam(aid);
            // 判定可能出现的选项
            MopaiMessage msg = new MopaiMessage(mopai);
            // 是否能自摸
            PtResult ptr = new PtJudger().Judge(plist, mopai, fulu, param);
            msg.zimo = ptr.mianzi != null;
            // 是否可以立直
            if(param.lizhi == 0 && fulu.Count == 0)
            {
                Dictionary<char, int[]> paiCount = PaiMaker.GetCount(plist);
                if (TingJudger.xiangting(paiCount, fulu) == 0)
                {
                    msg.lizhipai = TingJudger.tingpai(paiCount, fulu);
                    msg.lizhi = curStatus.score[aid] >= 1000;
                }
            }
            msg.lizhi_stat = param.lizhi > 0;

            // 九种九牌流局
            if (false)
            {
                msg.liuju = true;
            }
            // 是否允 暗/加杠
            if (false)
            {
                msg.gang = true;
            }
            return msg;
        }

        private FuluMessage allow_fulu(int aid, Pai mopai)
        {
            // 根据玩家id获取信息
            List<string> fulu = Fulu[aid];
            //PtParam param = curStatus.playerParam[aid];
            PtParam param = curStatus.getPlayerParam(aid);
            List<Pai> np = new List<Pai>(PaiStack[aid]);
            np.Add(mopai);
            PtResult ptr = new PtJudger().Judge(np, mopai, fulu, param);
            // 生成消息
            FuluMessage msg = new FuluMessage(mopai);
            if(ptr.mianzi != null)
            {
                msg.hu = true;
            }
            // 立直状态不能吃碰
            //if (curStatus.playerParam[aid].lizhi > 0) return msg;
            if (curStatus.lizhi[aid] > 0) return msg;
            if (false)
            {
                msg.pen = true;
            }
            if (false)
            {
                msg.chi = true;
            }
            return msg;
        }

        /// <summary>
        /// 刷新主窗口ui
        /// </summary>
        /// <param name="t">等待时间</param>
        private void FreshUI(int t = 100)
        {
            mw.Dispatcher.Invoke(new Action(() => {
                mw.RefreshUI();
                mw.RefreshGameUI();
                mw.RefreshAIUI();
                //if (t > 1000) mw.Pause();
            }));
            Thread.Sleep(t);
        }

        /// <summary>
        /// 分部进行
        /// </summary>
        public void Next()
        {
            return;

            //if (yamaPos > 136 - 14)
            //{
            //    return;
            //}

            //由东家开始摸牌

            //摸牌
            //yamaPoint++;
            //yama[yamaPoint].stat = 1;
            //PaiStack[curPlayer].Add(yama[yamaPoint]);
            ////操作：自摸 立直 流局
            ////切牌：TODO 手牌和牌河交由AI处理
            //int pnum = new Random().Next(PaiStack[curPlayer].Count);
            //Pai qp = PaiStack[curPlayer][pnum];
            //PaiStack[curPlayer].Remove(qp);
            //River[curPlayer].Add(qp);
            //yama[yamaPoint].stat = curPlayer + 5;

            ////整理
            //List<Pai> ss = PaiStack[curPlayer].ToList();
            //ss.Sort();
            //PaiStack[curPlayer] = new ObservableCollection<Pai>(ss);

            ////他家 吃 碰 杠 处理

            //curPlayer++;
            //if (curPlayer > 3) curPlayer = 0;

        }



    }

    
}
