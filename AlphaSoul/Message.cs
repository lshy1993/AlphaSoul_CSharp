using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlphaSoul
{
    // 发给玩家的 摸牌
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


    // 玩家切牌
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

    //玩家胡牌
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

    //玩家选择杠
    class GangMessage
    {
        // 玩家选择开杠的牌
        public Pai gangpai;
        // 0暗杠 1明杠 2加杠
        public int type;

        public GangMessage(Pai gp, int type)
        {
            this.gangpai = gp;
            this.type = type;
        }
    }

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
}
