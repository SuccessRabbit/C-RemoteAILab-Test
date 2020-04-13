using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebSocketTest
{

   
    class ServerTest
    {
        #region 变量
        private Socket serverSocket;
        public List<Socket> clientList = new List<Socket>();
        public Dictionary<Socket, int> clientStatusDict = new Dictionary<Socket, int>();  // 储存客户端上次消息发送时间
        private const int CHECKTIME = 30000;  // 看门狗定时器定时时间 ms
        #endregion


        public ServerTest()  // 构造函数
        {
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            
        }

        /// <summary>
        /// 看门狗监测方法，监测列表客户端在线状态
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        public void CheckOnlineStatus(object source, System.Timers.ElapsedEventArgs e)
        {
            //Console.WriteLine(TimeHandler());
            CheckClientStatus();
            Console.WriteLine("list length: " + clientList.Count + "dict lenth: " + clientStatusDict.Count);
        }

        private int TimeHandler()
        {
            var t = DateTime.Now;
            return t.DayOfYear * 24 * 3600 + t.Hour * 3600 + t.Minute * 60 + t.Second;
        }

        public void Start()
        {
            serverSocket.Bind(new IPEndPoint(IPAddress.Any, 60000)); // 绑定IP地址和端口

            serverSocket.Listen(300);  // 监听端口，最大挂起数为10

            Console.WriteLine("Server Start!");
            //  使能看门狗定时器
            System.Timers.Timer watchDogTimer = new System.Timers.Timer(CHECKTIME);
            watchDogTimer.Elapsed += new System.Timers.ElapsedEventHandler(CheckOnlineStatus);
            watchDogTimer.AutoReset = true;
            watchDogTimer.Enabled = true;

            // 启动多线程

            Thread threadAccept = new Thread(Accept);
            threadAccept.IsBackground = false;
            threadAccept.Start();

            
        }

        /// <summary>
        /// 线程：服务器端接收客户端消息
        /// </summary>
        private void Accept()
        {
            try
            {
                Socket client = serverSocket.Accept();  // 接受客户端  挂起当前线程
                clientList.Add(client);  // 保存当前加入的客户端到一个列表中
                // add client to client dict
                clientStatusDict.Add(client, TimeHandler());

                Console.WriteLine(clientList.Count);
                IPEndPoint clientIP = client.RemoteEndPoint as IPEndPoint;  // 获得客户端的ip地址和端口
                Console.WriteLine(clientIP.Address + "[" + clientIP.Port + "]" + "Connected");

                Thread threadReceive = new Thread(Receive);
                threadReceive.IsBackground = false;
                threadReceive.Start(client);
                

                Accept();
            }
            catch
            {
                //TODO
                Console.WriteLine("An error occur!");
            }
        }


        private void Receive(object obj)
        {
            Socket client = obj as Socket;
            // 处理客户端发送的消息
            IPEndPoint clientIP = client.RemoteEndPoint as IPEndPoint;  // 获得客户端的ip地址和端口
            try
            {
                byte[] msg = new byte[1024];

                int msgLen = client.Receive(msg);
                var temp = Encoding.UTF8.GetString(msg, 0, msgLen);
                if (msgLen == 0) return;
                Console.WriteLine(clientIP.Address + "[" + clientIP.Port + "]" + temp);

                string[] data = UserNameAndPasswordHandler(temp);
                try  // 如果发送的内容包含空格符
                {
                    string userName = data[0];
                    string userPassword = data[1];
                    client.Send(Encoding.UTF8.GetBytes("Your username is:" + userName + "   Your password is:" + userPassword));
                    Receive(client);
                }
                catch (Exception ex)  // 如果发送的内容不包含空格符
                {
                    if(data[0] == "0")
                    {
                        if (!clientList.Contains(client))  // 如果该客户端已经被判定掉线
                        {
                            clientList.Add(client);
                            clientStatusDict.Add(client, TimeHandler());
                            Console.WriteLine("A client has reconnected!");
                        }
                        else
                            UpdateClientStatus(client, data[0]);
                    }
                    
                    if (clientList.Contains(client)) Receive(client);
                }


            }
            catch(Exception ex)
            {
                Console.WriteLine(clientIP.Address + "[" + clientIP.Port + "]" + "disconnected!");
                clientList.Remove(client);
                clientStatusDict.Remove(client);
                
                //Console.WriteLine(ex);
            }
        }


        /// <summary>
        /// 更新客户端状态
        /// </summary>
        /// <param name="client"></param>
        private void UpdateClientStatus(Socket client, string msg)
        {
            if(msg == "0")  // 客户端定时发送消息0
            {
                if (clientStatusDict.ContainsKey(client))
                {
                    clientStatusDict[client] = TimeHandler();
                }
            }
        }

        /// <summary>
        /// 检测客户端状态
        /// </summary>
        private void CheckClientStatus()
        {
            var timeNow = TimeHandler();
            List<Socket> remove = new List<Socket>();
            foreach(KeyValuePair<Socket, int> client in clientStatusDict)
            {
                if (timeNow - client.Value > 40)  // 如果客户端超过30秒没有向服务器发送消息，则判定掉线
                {
                    //clientList.Remove(client.Key);
                    //Console.WriteLine("A Client has dropped off. client list length:" + clientList.Count);
                    //clientStatusDict.Remove(client.Key);
                    //Console.WriteLine(clientStatusDict.Count);
                    remove.Add(client.Key);
                }
            }
            foreach(Socket client in remove)
            {
                if(clientStatusDict.ContainsKey(client))
                {
                    clientStatusDict.Remove(client);
                    clientList.Remove(client);
                    Console.WriteLine("A Client has dropped off. client list length:" + clientList.Count);

                    //Console.WriteLine(clientStatusDict.Count);
                }
            }
        }

        /// <summary>
        /// 对来自客户端的用户输入进行处理 数组第一个元素为用户名 第二个元素为密码
        /// </summary>
        /// <param name="content">待处理的字符串</param>
        /// <returns></returns>
        private string[] UserNameAndPasswordHandler(string content)
        {
            return content.Split(' ');
        }
    }
}
