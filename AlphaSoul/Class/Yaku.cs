using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlphaSoul
{
    /// <summary>
    /// 役种
    /// </summary>
    public class Yaku
    {
        /// <summary>
        /// 役名
        /// </summary>
        public string name;

        /// <summary>
        /// 番数，役满为负
        /// </summary>
        public int fanshu;

        /// <summary>
        /// 包家
        /// </summary>
        public char baojia;

        public Yaku(string a, int b)
        {
            name = a;
            fanshu = b;
            baojia = '0';
        }

        public Yaku(string a, int b, char c)
        {
            name = a;
            fanshu = b;
            baojia = c;
        }
    }
}
