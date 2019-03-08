using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlphaSoul
{
    public class GameHistory
    {
        public string wind { get; set; }
        public string res { get; set; }
        public int[] pt { get; set; }

        public GameHistory()
        {
            pt = new int[4];
        }
    }
}
