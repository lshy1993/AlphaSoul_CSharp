using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlphaSoul
{
    /// <summary>
    /// 判定参数
    /// </summary>
    public class PtParam
    {
        //与对局共享 引用
        //場風 (0: 東, 1: 南, 2: 西, 3: 北)
        public int changfeng;
        // 和了者の自風
        public int zifeng;
        // ドラ表示牌
        public List<string> baopai;
        // 裏ドラ表示牌
        public List<string> libaopai;
        // 積み棒の数
        public int changbang;
        // 供託立直棒の数
        public int lizhibang;

        //状况役
        /// <summary>
        /// 0 无 1 立直 2 两立直
        /// </summary>
        public int lizhi;
        // 一発のとき true
        public bool yifa;
        // 槍槓のとき true
        public bool qianggang;
        // 嶺上開花のとき true
        public bool lingshang;
        // 1: 海底摸月, 2: 河底撈魚
        public int haidi;
        // 1: 天和, 2: 地和
        public int tianhu;

        public PtParam()
        {

        }

        public PtParam(GameStatus gs, int zi)
        {
            changfeng = gs.changfeng;
            changbang = gs.changbang;
            baopai = gs.bao;
            libaopai = gs.libao;
            changbang = gs.changbang;
            lizhibang = gs.lizhibang;
            zifeng = zi;
        }

        public PtParam(ref int zhuang, ref List<string> bao, ref List<string> libao, ref int cb, ref int lb)
        {
            changfeng = zhuang;
            baopai = bao;
            libaopai = libao;
            changbang = cb;
            lizhibang = lb;
        }

    }

    public class PtRank : IComparable
    {
        public int score;
        public int feng;
        public int aid;

        public PtRank(int score,int zifeng,int aid)
        {
            this.score = score;
            this.feng = zifeng;
            this.aid = aid;
        }

        int IComparable.CompareTo(object y)
        {
            PtRank p = (PtRank)y;
            int temp = score.CompareTo(p.score);
            if (temp > 0) return 1;
            else if (temp < 0) return -1;

            return feng.CompareTo(p.feng);
        }
    }


}
