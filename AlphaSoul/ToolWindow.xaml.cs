using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AlphaSoul
{
    /// <summary>
    /// ToolWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ToolWindow : Window
    {
        public Game gm;

        public ToolWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 自动填充
        /// </summary>
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            List<Pai> handStack = gm.PaiStack[0];
            string str = "";
            foreach (Pai p in handStack)
            {
                str += p.code;
            }
            PaiTextBox.Text = str;
        }

        //代码计算判定
        private void Ron_Click(object sender, RoutedEventArgs e)
        {
            List<Pai> handStack = new List<Pai>();
            Dictionary<char, int> typenum = new Dictionary<char, int>() { { 'm', 0 }, { 'p', 1 }, { 's', 2 }, { 'z', 3 } };
            //文字框逆序成排列
            for (int i = 0; i < PaiTextBox.Text.Length / 2; i++)
            {
                int num = PaiTextBox.Text[i * 2] - '0';
                num = (num == 0) ? 5 : num;
                int type = typenum[PaiTextBox.Text[i * 2 + 1]];
                handStack.Add(new Pai(type * 9 + num - 1, num == 0));
            }
            //胡牌处理
            int hunm = MoTextBox.Text[0] - '0';
            hunm = hunm == 0 ? 5 : hunm;
            int htype = typenum[MoTextBox.Text[1]];
            Pai mp = new Pai(htype * 9 + hunm - 1, hunm == 0);
            //UI
            SetPaiDisp(handStack, mp);
            //胡了形式判定

            PtResult ptr = new PtJudger().Judge(handStack, mp, null);
            List<Mianzi> huliaopaixing = ptr.mianziall;
            string str = "";
            foreach (Mianzi mz in huliaopaixing)
            {
                if (mz == null) continue;
                str += (str == "") ? "[" : "\r\n[";
                foreach (string ss in mz.paizu)
                {
                    str += ss;
                }
                str += "]";
            }
            FanLabel.Content = str;
            if (ptr.mianzi == null) return;
            string cmz = "";
            foreach (string ss in ptr.mianzi.paizu)
            {

                cmz += ss;
            }
            PtLabel.Content = cmz;
            List<Yaku> ylist = ptr.hupai;
            string fan = "";
            foreach (Yaku y in ylist)
            {
                fan += String.Format("{0} {1} \r\n", y.name, +y.fanshu);
            }
            PtLabel2.Content = fan;
        }

        private void SetPaiDisp(List<Pai> handStack, Pai mp)
        {
            int[] paiCount = new int[34];
            string dispStr = "";
            List<Pai> allstack = new List<Pai>(handStack);
            allstack.Add(mp);
            foreach (Pai p in allstack)
            {
                int num = p.id % 34;
                paiCount[num] += 1;
                dispStr += p.display + " ";
            }
            DispLabel.Content = dispStr;
            string codeStr = "";
            foreach (int i in paiCount)
            {
                codeStr += i;
            }
            RonCodeLabel.Content = codeStr;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //testParam.zhuangfeng = ((ComboBox)sender).SelectedIndex;
        }

        private void ComboBox_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            //testParam.menfeng = ((ComboBox)sender).SelectedIndex;
        }

        private void ComboBox_SelectionChanged_2(object sender, SelectionChangedEventArgs e)
        {
            //testParam.lizhi = ((ComboBox)sender).SelectedIndex;
        }

        private void ComboBox_SelectionChanged_3(object sender, SelectionChangedEventArgs e)
        {
            //testParam.zi = ((ComboBox)sender).SelectedIndex;
        }

    }
}
