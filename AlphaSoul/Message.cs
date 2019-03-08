using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlphaSoul
{
    /// <summary>
    /// 发给玩家的 摸牌
    /// </summary>
    class MopaiMessage
    {
        public Pai mopai;

        // 如果立直后 不允许玩家切牌
        public bool lizhi_stat = false;

        public bool lizhi = false;
        public bool zimo = false;
        public bool liuju = false;

        // 是否能杠
        public bool gang = false;
        public List<string> gangpai = new List<string>();

        public MopaiMessage(Pai p)
        {
            mopai = p;
        }
    }

    /// <summary>
    /// 发送给玩家流局消息
    /// </summary>
    class LiujuMessage
    {
        // 流局理由 "九种" "四风" "四杠" "荒牌"
        public int type = 0;
        public string name;

        // 流局时的手牌
        public string[] shoupai = new string[4];
        // 局収支
        public int[] fenpei = new int[4];

        static string[] typeStr = new string[] { "九种九牌", "四风连打", "四杠散了", "荒牌流局" };

        public LiujuMessage(int a)
        {
            type = a;
            name = typeStr[type];
        }

        public LiujuMessage(List<Pai> hand)
        {
            type = 0;
            name = typeStr[type];
            shoupai[0] = PaiMaker.GetCode(hand);
        }
    }

    /// <summary>
    /// 发送给玩家允许副露
    /// </summary>
    class FuluMessage
    {
        public Pai dapai;
        public bool hu = false;
        public bool chi = false;
        public bool pen = false;
        public bool gang = false;

        public FuluMessage(Pai p)
        {
            dapai = p;
        }
    }

    /// <summary>
    /// 发送给玩家 局（风）结束消息
    /// </summary>
    class EndMessage
    {

    }



    /* 以下为玩家消息 */

    /// <summary>
    /// 玩家选择 切牌
    /// </summary>
    class QiepaiMessage
    {
        // 切出的牌
        public Pai qiepai;
        public int from;

        public QiepaiMessage(Pai a, int b)
        {
            qiepai = a;
            from = b;
        }
    }

    /// <summary>
    /// 玩家选择 胡牌
    /// </summary>
    class HuMessage
    {
        public Pai rongpai;
        public int from;
        //public PtResult res;

        public HuMessage(Pai p, int id)
        {
            rongpai = p;
            from = id;
        }
    }

    /// <summary>
    /// 玩家选择 杠牌
    /// </summary>
    class GangMessage
    {
        // 玩家选择开杠的牌
        public Pai gangpai;
        // 0暗杠 1明杠 2加杠
        public int type;
        // 来自的ai编号
        public int from;

        public GangMessage(Pai gp, int type)
        {
            this.gangpai = gp;
            this.type = type;
        }
    }

    /// <summary>
    /// 玩家选择碰
    /// </summary>
    class PengMessage
    {

    }

    /// <summary>
    /// 玩家选择吃
    /// </summary>
    class ChiMessage
    {

    }

}
