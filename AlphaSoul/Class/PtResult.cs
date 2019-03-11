using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlphaSoul
{
    /// <summary>
    /// 判定结果
    /// </summary>
    public class PtResult
    {
        /// <summary>
        /// 所有可能的拆分面子
        /// </summary>
        public List<Mianzi> mianziall;

        /// <summary>
        /// 选定的最大面子
        /// </summary>
        public Mianzi mianzi;

        /// <summary>
        /// 役名 番数
        /// </summary>
        public List<Yaku> hupai;

        public List<string> bao;
        public List<string> libao;

        /// <summary>
        /// 符
        /// </summary>
        public int fu;

        /// <summary>
        /// 总番数
        /// </summary>
        public int fanshu;

        /// <summary>
        /// 役满个数
        /// </summary>
        public int yiman;

        /// <summary>
        /// 纯点数
        /// </summary>
        public int defen;

        /// <summary>
        /// 点数分配 自风顺
        /// </summary>
        public int[] fenpei;

        public PtResult()
        {
            mianziall = new List<Mianzi>();
            hupai = new List<Yaku>();
            bao = new List<string>();
            libao = new List<string>();
            fenpei = new int[4];
            fu = 0;
            yiman = 0;
            defen = 0;
        }

        public string GetYaku()
        {
            string ss = defen + ":\r\n";
            foreach(Yaku y in hupai)
            {
                ss += string.Format("{0}  {1} \r\n", y.name, y.fanshu);
            }
            return ss;
        }

    }
}
