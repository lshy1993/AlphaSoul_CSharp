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
using System.Windows.Shapes;

namespace AlphaSoul
{
    /// <summary>
    /// MajServer.xaml 的交互逻辑
    /// </summary>
    public partial class MajServer : UserControl
    {
        public MajServer()
        {
            InitializeComponent();
        }

        public void SetBind(List<WebPacket> ss)
        {
            PacksList.ItemsSource = new ObservableCollection<WebPacket>(ss);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            //mainThread.Abort();
        }
    }
}
