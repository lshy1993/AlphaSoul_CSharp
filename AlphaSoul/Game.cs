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
        //总牌山
        public ObservableCollection<Pai> yama = new ObservableCollection<Pai>();
        //玩家手牌
        //public Dictionary<char, int[]>[] PaiCount = new Dictionary<char, int[]>[4];
        public List<Pai>[] PaiStack = new List<Pai>[4];
        //牌河
        public ObservableCollection<Pai>[] River = new ObservableCollection<Pai>[4];
        //鸣牌区域
        public ObservableCollection<Pai>[] Fulu = new ObservableCollection<Pai>[4];

        // 牌山位置指示
        private int yamaPoint, yamaLast;
        // 玩家位置
        private int curPlayer = 0;
        // 终局指示
        private bool endGame;

        //当前对局信息
        public GameStatus curStatus;

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

        /// <summary>
        /// 初始化开始一局
        /// </summary>
        private void NewWind()
        {
            // 随机生成牌山
            yama = PaiMaker.ShufflePai();
            // 指向牌顶
            yamaPoint = -1;
            // 指向杠牌
            yamaLast = 135;

            // 翻开第一张宝牌
            curStatus.bao = new List<string>();
            curStatus.libao = new List<string>();
            curStatus.bao.Add(yama[122].code);
            curStatus.libao.Add(yama[123].code);
            yama[122].stat = 10;
            yama[123].stat = 11;
            // 下一个宝牌位置
            curStatus.baopos = 124;

            // 玩家手牌清空
            for (int p = 0; p < 4; p++)
            {
                PaiStack[p].Clear();
                River[p].Clear();
                Fulu[p].Clear();
            }
            //摸4轮牌
            for (int t = 0; t < 4; t++)
            {
                //4个玩家依次
                foreach (int p in curStatus.playerWind)
                {
                    if (t < 3)
                    {
                        //前3轮 每轮连续摸4张
                        for (int i = 0; i < 4; i++)
                        {
                            yamaPoint++;
                            yama[yamaPoint].stat = p + 1;
                            Pai mpai = yama[yamaPoint];
                            PaiStack[p].Add(mpai);
                        }
                    }
                    else
                    {
                        //第四轮
                        yamaPoint++;
                        yama[yamaPoint].stat = p + 1;
                        Pai mpai = yama[yamaPoint];
                        PaiStack[p].Add(mpai);
                    }
                    // ui显示
                    FreshUI();

                }
            }

        }

        //private ObservableCollection<Pai> GetPaiStack(int d)
        //{
        //    List<Pai> PaiStack = new List<Pai>();
        //    foreach(KeyValuePair<char,int[]> kv in PaiCount[d])
        //    {
        //        // 麻将类型
        //        char ch = kv.Key;
        //        for(int n = 1; n < kv.Value.Length; n++)
        //        {
        //            for(int i = 0; i < kv.Value[n]; i++)
        //            {
        //                if (n == 5 && i<kv.Value[0] )
        //                {
        //                    PaiStack.Add(new Pai(ch, 0));
        //                }
        //                else
        //                {
        //                    PaiStack.Add(new Pai(ch, n));
        //                }
                        
        //            }
        //        }
        //    }
        //    PaiStack.Sort();
        //    return new ObservableCollection<Pai>(PaiStack);
        //}

        //private void GivePai(int curPlayer, Pai dapai)
        //{
        //    if (dapai.num == 0) PaiCount[curPlayer][dapai.type][5] += 1;
        //    PaiCount[curPlayer][dapai.type][dapai.num] += 1;
        //}

        //private void RemovePai(int curPlayer, Pai dapai)
        //{
        //    if (dapai.num == 0) PaiCount[curPlayer][dapai.type][5] -= 1;
        //    PaiCount[curPlayer][dapai.type][dapai.num] -= 1;
        //}

        private void PlayWind()
        {
            // 发送初始化信息给player
            foreach (int playerid in curStatus.playerWind)
            {
                player[playerid].InitStatus(curStatus);
                //player[playerid].InitPai(PaiCount[playerid]);
                player[playerid].InitStack(PaiStack[playerid]);
            }
            // 自动牌局
            curPlayer = 0;
            bool endSection = false;
            object callback = null;// new QiepaiMessage(null, 0);
            while (!endSection)
            {
                // ui显示
                FreshUI();
                // 处理player的回应
                if (callback == null)
                {
                    int playerid = curStatus.playerWind[curPlayer];
                    // 从牌顶 摸牌
                    yamaPoint++;
                    Pai mo = yama[yamaPoint];
                    yama[yamaPoint].stat = playerid + 1;
                    // 手牌+1
                    PaiStack[curPlayer].Add(mo);
                    // 通知生成（有暗杠 胡牌 流局）
                    MopaiMessage mmsg = allow_zimo(PaiStack[playerid], mo, curStatus.playerParam[playerid]);
                    // 向玩家发送消息
                    callback = player[curPlayer].Action("zimo", mmsg);
                    
                }
                else if (callback.GetType() == typeof(HuMessage))
                {
                    // 玩家胡牌 （展示手牌）
                    HuMessage hmsg = (HuMessage)callback;
                    // 计算点数
                    int pwind = curStatus.playerWind[hmsg.from];
                    PtResult ptr = new PtJudger().Judge(PaiStack[hmsg.from], hmsg.rongpai, curStatus.playerParam[hmsg.from]);

                    // 向所有的玩家发送胡了分数（结束）消息
                    foreach (int id in curStatus.playerWind)
                    {
                        player[id].Action("hule", new object());
                    }
                    endSection = true;
                }
                else if (callback.GetType() == typeof(QiepaiMessage))
                {
                    // 玩家切牌
                    QiepaiMessage qm = (QiepaiMessage)callback;
                    curPlayer = qm.from;
                    Pai dapai = qm.qiepai;
                    PaiStack[curPlayer].Remove(dapai);
                    // 如果有玩家可以副露 则发送消息
                    foreach (int id in curStatus.playerWind)
                    {
                        if (id != curPlayer)
                        {
                            FuluMessage fmsg = allow_fulu(PaiStack[id], dapai, curStatus.playerParam[id]);
                            object subcallback = player[id].Action("fulu", fmsg);
                        }
                        // 判定优先级 hu 碰优先于吃
                        
                    }
                    

                    // 否则进入牌河
                    River[curPlayer].Add(dapai);
                    dapai.stat = curPlayer + 4;
                    // 轮到下一个玩家/荒牌流局
                    if (yamaPoint == yamaLast - 14)
                    {
                        LiujuMessage lj = new LiujuMessage(1);
                        for(int i = 0; i < 4; i++)
                        {
                            int pid = curStatus.playerWind[i];
                            lj.shoupai[i] = PaiMaker.GetCode(PaiStack[pid]);
                        }
                        callback = lj;
                    }
                    else
                    {
                        curPlayer += 1;
                        if (curPlayer > 3) curPlayer = 0;
                        callback = null;
                    }

                }
                else if (callback.GetType() == typeof(LiujuMessage))
                {
                    // 玩家选择流局

                    //向所有玩家发送流局（结束）消息
                    foreach (int id in curStatus.playerWind)
                    {
                        player[id].Action("liuju", null);
                    }
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
                        // 没有抢杠 发送 杠牌
                        int playerid = curStatus.playerWind[curPlayer];
                        // 从牌底 摸牌
                        Pai mo = yama[yamaLast];
                        mo.stat = playerid + 1;
                        yamaLast--;
                        //消息生成
                        MopaiMessage mmsg = allow_zimo(PaiStack[playerid], mo, curStatus.playerParam[playerid]);
                        // 计算是否有暗杠 胡牌 流局

                        // 向玩家发送摸牌
                        callback = player[curPlayer].Action("zimo", mmsg);
                        
                    }

                }
                else if (callback.GetType() == typeof(FuluMessage))
                {
                    // 玩家选择了副露
                }
                // 记录牌谱
            }
            //向所有玩家发送单局结果
            foreach (int id in curStatus.playerWind)
            {
                player[id].Action("jieguo", new object());
            }
            //连庄判定
            bool lianzhuang = true;
            // 以下变更设置
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
            // 是否允开杠
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
                mw.RefreshAIUI(String.Format("P: {0}", curPlayer));
            }));
            Thread.Sleep(t);
        }



        /// <summary>
        /// 分部进行
        /// </summary>
        public void Next()
        {
            return;

            if (yamaPoint > 136 - 14)
            {
                return;
            }

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
