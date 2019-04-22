using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace AlphaSoul
{
    public class Pai : IComparable
    {
        //牌类型编号
        public int id { get; set; }
        //外观
        public string display { get; set; }
        //代码
        public string code { get; set; }
        //备用
        public int num { get; set; }
        public char type { get; set; }
        public char label { get; set; }
        //牌堆状态
        public int stat
        {
            set
            {
                statint = value;
                status = statstr[value];
            }
            get
            {
                return statint;
            }
        }
        private int statint;
        public string status { get; set; }

        static string[] statstr = new string[12] {
            "未发牌",
            "东家手牌", "南家手牌","西家手牌","北家手牌",
            "东家牌河","南家牌河","西家牌河","北家牌河",
            "王牌堆", "宝牌", "里宝牌" };

        //麻将牌外观
        static string[] paiDisp = new string[34] {
            "一","二","三","四","五","六","七","八","九",
            "①","②","③","④","⑤","⑥","⑦","⑧","⑨",
            "１","２","３","４","５","６","７","８","９",
            "東","南","西","北", //场风
            "白","發","中" //役牌
        };

        //编码
        static string[] paiCode = new string[34] {
            "1m","2m","3m","4m","5m","6m","7m","8m","9m",
            "1p","2p","3p","4p","5p","6p","7p","8p","9p",
            "1s","2s","3s","4s","5s","6s","7s","8s","9s",
            "1z","2z","3z","4z","5z","6z","7z"
        };
        //static string[] typestr = new string[4] { "m", "p", "s", "z" };

        static Dictionary<char, int> typenum = new Dictionary<char, int>() {
            { 'm', 0 }, {'p',1 }, {'s' ,2}, {'z' ,3}
        };

        public Pai(char ch, int pnum)
        {
            int p = pnum - 1 + typenum[ch] * 9;
            if(pnum == 0)
            {
                p = 5 - 1 + typenum[ch] * 9;
            }
            id = p;
            num = pnum;
            type = ch;
            code = pnum == 0? paiCode[p].Replace("5", "0") : paiCode[p];
            display = paiDisp[p];
            stat = 0;
        }

        public Pai(int p, bool hongbao = false)
        {
            id = p;
            code = hongbao ? paiCode[p].Replace("5", "0") : paiCode[p];
            num = code[0] - '0';
            type = code[1];
            display = paiDisp[p];
            stat = 0;
            //status = statstr[stat];
        }

        int IComparable.CompareTo(object y)
        {
            Pai p = (Pai)y;
            int temp = type.CompareTo(p.type);
            if (temp > 0) return 1;
            else if (temp < 0) return -1;

            return (num == 0 ? 5 : num).CompareTo(p.num == 0 ? 5 : p.num);
        }




    }


    /// <summary>
    /// 和了牌型
    /// </summary>
    public class Mianzi
    {
        public List<string> paizu;

        public Mianzi()
        {
            paizu = new List<string>();
        }

        public Mianzi(List<string> p)
        {
            paizu = p;
        }
    }

}
