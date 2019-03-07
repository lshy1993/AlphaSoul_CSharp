using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AlphaSoul
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        //private ObservableCollection<Pai> currentYama;
        private Game gm;
        //private PtParam testParam = new PtParam();
        private Thread aithread;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //YamaInit();
        }

        private void YamaInit()
        {
            YamaCodeTextBlock.Text = PaiMaker.GetCode(gm.yama.ToList());
            //WrapStock.Children.Clear();
            //for (int i = 0; i < 136; i++)
            //{
            //    Label lb = new Label();
            //    lb.SetBinding(Label.ContentProperty, new Binding("display") { Source = gm.yama[i], Mode=BindingMode.OneWay });
            //    lb.SetBinding(Label.ForegroundProperty, new Binding("foreBrush") { Source = gm.yama[i], Mode = BindingMode.OneWay });
            //    lb.SetBinding(Label.BackgroundProperty, new Binding("backBrush") { Source = gm.yama[i], Mode = BindingMode.OneWay });
            //    WrapStock.Children.Add(lb);
            //}
            //PaiListView.ItemsSource = gm.yama;
        }

        public void RefreshUI()
        {
            //wrapstock
            WrapStock.Children.Clear();
            for (var i = 0; i < 136; i++)
            {
                Pai p = gm.yama[i];
                Label lb = new Label();
                lb.Content = p.display;
                if (p.num == 0)
                {
                    lb.Foreground = new SolidColorBrush(Colors.Red);
                }
                // 牌色区分
                if( p.stat == 9)
                {
                    lb.Background = new SolidColorBrush(Colors.Gray);
                }
                else if (p.stat == 10)
                {
                    lb.Background = new SolidColorBrush(Colors.SkyBlue);
                }
                else if (p.stat == 11)
                {
                    lb.Background = new SolidColorBrush(Colors.Pink);
                }
                else if(p.stat != 0)
                {
                    lb.Background = new SolidColorBrush(Colors.Yellow);
                }
                WrapStock.Children.Add(lb);
            }
            //listview
            PaiListView.ItemsSource = gm.yama;
        }

        public void RefreshGameUI()
        {
            //手牌堆panel
            StackPanel[] splist = new StackPanel[4] { StackPanel_D, StackPanel_R, StackPanel_U, StackPanel_L };
            //牌河堆panel
            WrapPanel[] rvlist = new WrapPanel[4] { StackPanel_DR, StackPanel_RR, StackPanel_UR, StackPanel_LR };
            //UI刷新
            for (int i = 0; i < 4; i++)
            {
                StackPanel sp = splist[i];
                sp.Children.Clear();
                List<Pai> hp = gm.PaiStack[i];
                hp.Sort();
                foreach (Pai p in hp)
                {
                    Label lb = new Label();
                    lb.Content = p.display;
                    if (p.num == 0)
                    {
                        lb.Foreground = new SolidColorBrush(Colors.Red);
                    }
                    sp.Children.Add(lb);
                }

                WrapPanel rvp = rvlist[i];
                rvp.Children.Clear();
                foreach (Pai p in gm.River[i])
                {
                    Label lb = new Label();
                    lb.Content = p.display;
                    if (p.num == 0)
                    {
                        lb.Foreground = new SolidColorBrush(Colors.Red);
                    }
                    rvp.Children.Add(lb);
                }
            }
            PaiListView.Items.Refresh();

            AIDist.Content = gm.curStatus.GetAIOrder();
            ChangDist.Content = gm.curStatus.GetWindOrder();
            BaoDist.Content = gm.curStatus.GetBao();
            LiDist.Content = gm.curStatus.GetBao(true);
        }

        public void RefreshAIUI(string text)
        {
            aiTextBlock.Text = text;
        }

        /// <summary>
        /// 刷新牌山
        /// </summary>
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            //YamaInit();
        }

        /// <summary>
        /// 开始对局
        /// </summary>
        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            gm = new Game(this);
            YamaInit();
            aithread = new Thread(gm.NewGame);
            aithread.Start();
        }

        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            aithread.Abort();
        }

        private void MenuItem_Click_3(object sender, RoutedEventArgs e)
        {
            //if(gm == null)
            //{
            //    MenuItem_Click_1(null, null);
            //}
            //gm.Next();
            //RefreshUI();
            //RefreshGameUI();
        }

        /// <summary>
        /// 自动填充
        /// </summary>
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (gm == null)
            {
                MenuItem_Click(null, null);
            }
            List<Pai> handStack = gm.PaiStack[0];
            string str = "";
            foreach(Pai p in handStack)
            {
                str += p.code;
            }
            PaiTextBox.Text = str;
        }

        //代码计算判定
        private void Ron_Click(object sender, RoutedEventArgs e)
        {
            List<Pai> handStack = new List<Pai>();
            Dictionary<char, int> typenum = new Dictionary<char, int>() { { 'm', 0 }, { 'p', 1 }, { 's', 2 }, {'z', 3 } };
            //文字框逆序成排列
            for (int i= 0; i < PaiTextBox.Text.Length / 2; i++)
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
            foreach(Yaku y in ylist)
            {
                fan += String.Format("{0} {1} \r\n", y.name, +y.fanshu);
            }
            PtLabel2.Content = fan;
        }
        
        // 手牌统计
        private Dictionary<char,int[]> CountPai(List<Pai> plist)
        {
            Dictionary<char, int[]>  paiCount = new Dictionary<char, int[]>();
            paiCount.Add('m', new int[10]);
            paiCount.Add('p', new int[10]);
            paiCount.Add('s', new int[10]);
            paiCount.Add('z', new int[8]);
            foreach (Pai p in plist)
            {
                // 红宝计算2次
                if (p.num == 0) paiCount[p.type][5] += 1;
                paiCount[p.type][p.num] += 1;
            }
            return paiCount;
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
