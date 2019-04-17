using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AlphaSoul
{
    /// <summary>
    /// 发给玩家的 摸牌
    /// </summary>
    class MopaiMessage
    {
        [Newtonsoft.Json.JsonIgnore()]
        public Pai mopai;
        public string tile;

        // 立直后 不允许玩家切牌
        public bool lizhi_stat = false;

        //是否可以立直
        public bool lizhi = false;
        public List<string> lizhipai = new List<string>();

        public bool zimo = false;
        public bool liuju = false;

        // 是否能杠
        public bool gang = false;
        public List<string> gangpai = new List<string>();

        public MopaiMessage(Pai p)
        {
            mopai = p;
            tile = p.code;
        }

        public string ToJsonData()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    /// <summary>
    /// 发送给玩家流局消息
    /// </summary>
    public class LiujuMessage
    {
        /// <summary>
        /// 流局理由 0"九种" 1"四风" 2"四杠" 3"荒牌"
        /// </summary>
        public int type = 0;
        public string name;

        // 流局时的4家手牌
        public string[] shoupai = new string[4];
        // 局収支
        public int[] fenpei = new int[4];
        // 4家是否立直
        public bool[] lizhi = new bool[4];

        public bool flag_li = false;
        public bool flag_fu = false;

        private static string[] typeStr = new string[] { "九种九牌", "四风连打", "四杠散了", "荒牌流局" };

        public LiujuMessage(int a=0)
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
        public List<FuluMianzi> fulu;
        public bool hu = false;
        public int from;

        public FuluMessage(Pai p)
        {
            dapai = p;
        }
        public FuluMessage(Pai p, int from)
        {
            dapai = p;
            this.from = from;
        }
    }

    /// <summary>
    /// 发送给玩家 局（风）结束消息
    /// </summary>
    public class EndMessage
    {
        // 当前的参数
        public PtParam param;

        /// <summary>
        /// 胡牌结果(空则未胡)
        /// </summary>
        public PtResult res;

        /// <summary>
        /// 是否放铳
        /// </summary>
        public bool flag_chong;
        /// <summary>
        /// 是否自摸
        /// </summary>
        public bool flag_zimo;

        /// <summary>
        /// 是否立直
        /// </summary>
        public bool flag_lizhi;

        /// <summary>
        /// 是否副露
        /// </summary>
        public bool flag_fu;

        /// <summary>
        /// 和了巡数
        /// </summary>
        public int xunshu;

        public EndMessage(PtParam param)
        {
            this.param = param;
        }
    }

    /// <summary>
    /// 发送给玩家 半庄结束
    /// </summary>
    public class ZhongMessage
    {
        public int pt;
        public int rank;
        public int maxzhuang;

        public ZhongMessage(int a, int b, int c)
        {
            pt = a;
            rank = b;
            maxzhuang = c;
        }
    }



    /* 以下为玩家AI消息 */

    /// <summary>
    /// 玩家选择 切牌
    /// </summary>
    class QiepaiMessage
    {
        // 切出（立直）的牌
        public string qiepai;
        public int from;
        public bool lizhi = false;

        public QiepaiMessage(string a, int b)
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
    //class FuluBackMessage
    //{
    //    // 玩家选择开杠的牌
    //    public Pai pai;
    //    // 
    //    public int type;
    //    // 来自的ai编号
    //    public int from;

    //    public FuluBackMessage(Pai gp, int type, int from)
    //    {
    //        this.pai = gp;
    //        this.type = type;
    //        this.from = from;
    //    }
    //}

    class LizhiMessage
    {
        // 玩家选择立直的牌
        public Pai lipai;
        // 来自的ai编号
        public int from;

        public LizhiMessage(Pai gp, int ai)
        {
            this.lipai = gp;
            this.from = ai;
        }
    }

    /// <summary>
    /// 玩家选择碰
    /// </summary>
    //class PengMessage
    //{

    //}

    /// <summary>
    /// 玩家选择吃
    /// </summary>
    //class ChiMessage
    //{

    //}

    /// <summary>
    /// 玩家选择9种流局
    /// </summary>
    class JiuMessage
    {

    }

}
