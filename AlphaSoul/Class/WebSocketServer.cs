using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Fleck;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AlphaSoul
{
    public class WebPacket
    {
        public string time { get; set; }
        public string from { get; set; }
        public string text { get; set; }

        public WebPacket(string from, string text)
        {
            time = DateTime.Now.ToString();
            this.from = from;
            this.text = text;
        }
    }

    public class GameServer
    {
        private MainWindow window;
        private List<WebPacket> packs;
        public List<IWebSocketConnection> allsockets;
        private IWebSocketServer server;

        private bool zimoRes = false;
        // 返回类型
        private JObject resObj;


        public GameServer(MainWindow window)
        {
            this.window = window;
            packs = new List<WebPacket>();
            allsockets = new List<IWebSocketConnection>();
        }

        public void Start()
        {
            server = new WebSocketServer("ws://127.0.0.1:8080");
            Log(0, "Listen!");
            server.Start(socket =>
            {
                socket.OnOpen = () => {
                    int clientPort = socket.ConnectionInfo.ClientPort;
                    allsockets.Add(socket);
                    Log(0, string.Format("getClient! {0}", clientPort));
                    socket.Send("2222");
                };
                socket.OnClose = () => {
                    int clientPort = socket.ConnectionInfo.ClientPort;
                    allsockets.Remove(socket);
                    Log(clientPort, string.Format("{0}: Close!", clientPort));
                };
                socket.OnMessage = message =>
                {
                    int clientPort = socket.ConnectionInfo.ClientPort;
                    Log(clientPort, string.Format("{0}:{1}", clientPort, message));
                    try
                    {
                        resObj = JObject.Parse(message);
                    }
                    catch
                    {
                        resObj = null;
                    }
                    
                };
            });

            while (true)
            {
                window.Dispatcher.Invoke(new Action(() =>
                {
                    window.serverPage.UserNum.Text = allsockets.Count.ToString();
                }));
            }

        }

        public void InitStatus(int id, AI_Core ai)
        {
            string jsondata = JsonConvert.SerializeObject(ai);
            allsockets[id].Send(jsondata);
        }

        public JObject GetZimoRes(int id, string jsondata)
        {
            resObj = null;
            allsockets[id].Send(jsondata);
            while (resObj == null)
            {
                //等待回应
            }
            return resObj;
        }








        public void Log(int id, string msg)
        {
            packs.Add(new WebPacket(id.ToString(), msg));
            window.Dispatcher.Invoke(new Action(() =>
            {
                window.serverPage.SetBind(packs);
            }));
        }


        public void End()
        {
            foreach (var client in allsockets)
            {
                client.Close();
            }
            server.Dispose();
        }
    }
}
