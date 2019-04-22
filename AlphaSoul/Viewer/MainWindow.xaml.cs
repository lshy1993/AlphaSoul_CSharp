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
        private Game gm;
        private GameServer mainServer;
        private Thread aithread, mainThread;
        string[] windStr = new string[4] { "东", "南", "西", "北" };

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //YamaInit();
            mainServer = new GameServer(this);
            gm = new Game(this, mainServer);
        }

        private void YamaInit()
        {
            YamaCodeTextBlock.Text = PaiMaker.GetCode(gm.yama.ToList());
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
            Label[] lblist = new Label[4] { Info_D, Info_R, Info_U, Info_L };
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
                
                //string feng = windStr[gm.curStatus.playerParam[i].zifeng];
                string feng = windStr[gm.curStatus.zifeng[i]];
                string fen = gm.curStatus.score[i].ToString();
                //string li = gm.curStatus.playerParam[i].lizhi > 0 ? "立直" : "";
                string li = gm.curStatus.lizhi[i] > 0 ? "立直" : "";
                lblist[i].Content = string.Format("AI_{0} {1} {2} {3}", i, feng, fen, li);
            }
            PaiListView.Items.Refresh();

            PtDist.Content = gm.curStatus.GetPtList();
            //ChangDist.Content = gm.curStatus.GetWindOrder();
            ChangDist.Content = string.Format("场风：{0} 场：{1} 立：{2}", windStr[gm.curStatus.changfeng], gm.curStatus.changbang, gm.curStatus.lizhibang);
            BaoDist.Content = gm.curStatus.GetBao();
            LiDist.Content = gm.curStatus.GetBao(true);

        }

        public void RefreshAIUI()
        {
            PaiListView.ItemsSource = gm.yama;
            //aiTextBlock.Text = text;
            WindListView.ItemsSource = gm.history;

            aiStaticGrid1.DataContext = null;
            aiStaticGrid1.DataContext = gm.getStatic();
            aiStaticGrid2.DataContext = null;
            aiStaticGrid2.DataContext = gm.getStatic(1);
            aiStaticGrid3.DataContext = null;
            aiStaticGrid3.DataContext = gm.getStatic(2);
            aiStaticGrid4.DataContext = null;
            aiStaticGrid4.DataContext = gm.getStatic(3);

        }

        public void Pause()
        {
            aithread.Suspend();
        }

        /// <summary>
        /// 打开工具窗口
        /// </summary>
        private void Tool_Click(object sender, RoutedEventArgs e)
        {
            //YamaInit();
        }

        /// <summary>
        /// 开始对局
        /// </summary>
        private void GameStart_Click(object sender, RoutedEventArgs e)
        {
            if (mainServer.allsockets.Count() == 0)
            {
                mainServer.Log(0, "ai未连接！");
                //return;
            };
            YamaInit();
            //aithread = new Thread(gm.LoopGame);
            aithread = new Thread(gm.NewGame);
            aithread.Start();
        }

        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            if (aithread == null) return;
            aithread.Abort();
        }

        private void MenuItem_Click_3(object sender, RoutedEventArgs e)
        {
            //aithread.Resume();
            aithread = new Thread(gm.LoopGame);
            aithread.Start();
        }

        private void ServerListen_Click(object sender, RoutedEventArgs e)
        {
            if (mainThread != null) return;
            mainThread = new Thread(mainServer.Start);
            mainThread.Start();
        }

        private void ServerStop_Click(object sender, RoutedEventArgs e)
        {
            if (mainThread == null) return;
            mainServer.End();
            mainServer.Log(0, "Abort!");
            mainThread.Abort();
        }

    }
}
