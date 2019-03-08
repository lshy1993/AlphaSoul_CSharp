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
        public ObservableCollection<Pai>[] Fulu = new ObservableCollection<Pai>[4];

        // 牌山位置指示
        private int yamaPos, yamaLast;
        private int baoPos;
        // 风位置
        private int curWind = 0;
        // 终局指示
        private bool endGame;

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
                Fulu[p] = new ObservableCollection<Pai>();
            }
            // 四个AI
            player = new AI_Core[4] { new AI_Core(0), new AI_Core(1), new AI_Core(2), new AI_Core(3) };
        }

        // 游戏开始
        public void NewGame()
        {
            endGame = false;
            // 游戏参数初始化
            curStatus = new GameStatus();
            while (!endGame)
            {
                // 开始小局
                NewWind();
                // 开始对局
                PlayWind();
                // 休息
                Thread.Sleep(500);
            }
            //发送终局消息
            foreach (int id in curStatus.playerWind)
            {
                player[id].Action("zhongju", new object());
            }
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
                    // ui显示
                    FreshUI();

                }
            }

        }

        private void PlayWind()
        {
            // 发送初始化信息给player
            foreach (int playerid in curStatus.playerWind)
            {
                player[playerid].InitStatus(curStatus);
                //player[playerid].InitPai(PaiCount[playerid]);
                player[playerid].InitStack(PaiStack[playerid]);
            }
            // 自动牌局 东风起手
            curWind = 0;
            bool endSection = false;
            bool lianzhuang = true;
            object callback = null;
            // 主循环
            while (!endSection)
            {
                // ui显示
                FreshUI();
                // 处理player的回应
                if (callback == null)
                {
                    int playerid = curStatus.playerWind[curWind];
                    // 从牌顶 摸牌
                    yamaPos++;
                    Pai mo = yama[yamaPos];
                    yama[yamaPos].stat = curWind + 1;
                    // 手牌+1
                    PaiStack[playerid].Add(mo);
                    // 通知生成（有暗杠 胡牌 流局）
                    MopaiMessage mmsg = allow_zimo(PaiStack[playerid], mo, curStatus.playerParam[playerid]);
                    // 向玩家发送消息
                    callback = player[playerid].Action("zimo", mmsg);
                    
                }
                else if (callback.GetType() == typeof(HuMessage))
                {
                    // 玩家胡牌（自摸）
                    HuMessage hmsg = (HuMessage)callback;
                    // 计算点数
                    PtResult ptr = new PtJudger().Judge(PaiStack[hmsg.from], hmsg.rongpai, curStatus.playerParam[hmsg.from]);

                    // 向所有的玩家发送结束消息
                    foreach (int id in curStatus.playerWind)
                    {
                        EndMessage emsg = new EndMessage(curStatus.playerParam[id]);
                        if (id == hmsg.from) emsg.res = ptr;
                        player[id].Action("hule", emsg);
                    }
                    GameHistory gg = new GameHistory();
                    gg.wind = curStatus.GetWind();
                    gg.res = "自摸";
                    gg.pt = ptr.fenpei;
                    mw.Dispatcher.Invoke(new Action(() => {
                        history.Add(gg);
                    }));
                    endSection = true;
                }
                else if (callback.GetType() == typeof(QiepaiMessage))
                {
                    // 玩家切牌
                    QiepaiMessage qm = (QiepaiMessage)callback;
                    int playerid = qm.from;
                    Pai dapai = qm.qiepai;
                    PaiStack[playerid].Remove(dapai);
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
                            FuluMessage fmsg = allow_fulu(PaiStack[id], dapai, curStatus.playerParam[id]);
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
                        GameHistory gg = new GameHistory();
                        gg.wind = curStatus.GetWind();
                        gg.res = "放铳";
                        // TODO 多人放铳在这里处理
                        foreach (HuMessage hm in mlist_hu)
                        {
                            int a = hm.from;
                            // 计算点数
                            PtResult ptr = new PtJudger().Judge(PaiStack[a], hm.rongpai, curStatus.playerParam[a]);
                            for (int i = 0; i < 4; i++) gg.pt[i] += ptr.fenpei[i];
                        }
                        // 向所有的玩家发送结束消息
                        foreach (int id in curStatus.playerWind)
                        {
                            EndMessage emsg = new EndMessage(curStatus.playerParam[id]);
                            player[id].Action("hule", emsg);
                        }
                        mw.Dispatcher.Invoke(new Action(() => {
                            history.Add(gg);
                        }));
                        endSection = true;
                    }
                    else if(mlist_gang.Count == 1)
                    {
                        // 杠
                    }
                    else if(mlist_peng.Count == 1)
                    {
                        // 碰
                    }
                    else if (mlist_chi.Count == 1)
                    {
                        // 吃
                    }
                    else
                    {
                        // 无人回应 则弃牌进入牌河
                        River[playerid].Add(dapai);
                        dapai.stat = curStatus.playerParam[playerid].zifeng + 4;
                        // 判定
                        if (yamaPos == yamaLast - 14)
                        {
                            // 向所有玩家发送荒牌流局
                            LiujuMessage lj = new LiujuMessage(3);
                            for (int i = 0; i < 4; i++)
                            {
                                int pid = curStatus.playerWind[i];
                                lj.shoupai[i] = PaiMaker.GetCode(PaiStack[pid]);
                            }
                            // 向所有玩家发送流局（结束）消息
                            foreach (int id in curStatus.playerWind)
                            {
                                player[id].Action("liuju", lj);
                            }
                            GameHistory gg = new GameHistory();
                            gg.wind = curStatus.GetWind();
                            gg.res = "流局";
                            gg.pt = lj.fenpei;
                            mw.Dispatcher.Invoke(new Action(() => {
                                history.Add(gg);
                            }));
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
                        int pwind = curStatus.playerParam[gmsg.from].zifeng;
                        // 从牌底 摸牌
                        Pai mo = yama[yamaLast];
                        mo.stat = pwind + 1;
                        yamaLast--;
                        // 胡牌杠消息生成
                        MopaiMessage mmsg = allow_zimo(PaiStack[gmsg.from], mo, curStatus.playerParam[gmsg.from]);
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

            // 连庄判定以下变更设置
            if (!lianzhuang)
            {
                // 流庄后 重新分配风
                curStatus.NextPlayer();
            }
            // 结束整个对局
            if (curStatus.score.Min() < 0)
            {
                // 有人被飞
                endGame = true;
            }
            else if (curStatus.changfeng > 2)
            {
                // 西入
                endGame = (curStatus.jushu == 4 || curStatus.score.Max() >= 30000);
            }
            else if (curStatus.changfeng == 2)
            {
                // 南4 结束
                endGame = (curStatus.jushu == 4 && curStatus.score.Max() >= 30000);
            }
        }


        private MopaiMessage allow_zimo(List<Pai> plist, Pai mopai, PtParam param)
        {
            //  统计
            //Dictionary<char, int[]> hand = PaiMaker.GetCount(plist);
            // 判定可能出现的选项
            MopaiMessage msg = new MopaiMessage(mopai);
            // 是否能自摸
            PtResult ptr = new PtJudger().Judge(plist, mopai, param);
            if (ptr.mianzi != null)
            {
                msg.zimo = true;
                //msg.res = ptr;
            }
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

        private FuluMessage allow_fulu(List<Pai> plist, Pai mopai, PtParam param)
        {
            FuluMessage msg = new FuluMessage(mopai);
            PtResult ptr = new PtJudger().Judge(plist, mopai, param);
            if(ptr.mianzi != null)
            {
                msg.hu = true;
            }
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
        private void FreshUI(int t = 10)
        {
            mw.Dispatcher.Invoke(new Action(() => {
                mw.RefreshUI();
                mw.RefreshGameUI();
                mw.RefreshAIUI(String.Format("P: {0}", curWind));
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
