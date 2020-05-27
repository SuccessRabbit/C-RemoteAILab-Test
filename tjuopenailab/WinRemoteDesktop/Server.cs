using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace tjuremoteAI
{
    public enum MessageType
    {
        None = 0,
        StartCountTime = 1,
        StopCountTime = 2,
        LoginData = 3
    }
    public static class Server
    {
        private static Socket server;
        private static bool isConnected;  // 是否连接到云服务器
        private static bool isLogin;      // 是否正确输入用户名和密码且成功登录
        public static string ip = "0.0.0.0";  // 从云服务器获取的远程桌面ip地址
        public static string userName = "openailab"; // 从云服务器获取的远程桌面用户名
        public static string passWord = "openailab";  // 从云服务器获取的远程桌面密码
        public static bool IsConnected
        {
            get
            {
                return isConnected;
            }
        }
        public static bool IsLogin
        {   get
            {
                return isLogin;
            }    
        }

        /// <summary>
        /// 连接云端访问接口服务器
        /// </summary>
        /// <param name="ip">云端服务器ip地址</param>
        /// <param name="port">云端服务器端口号</param>
        /// <returns></returns>
        public static bool Connect(string ip, int port)
        {
            isConnected = false;
            isLogin = false;
            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                server.Connect(ip, port);
                isConnected = true;
                return true;
            }
            catch(Exception ex)
            {
                isConnected = false;
                return false;
            }
        }
        /// <summary>
        /// 向服务端发送消息
        /// </summary>
        /// <param name="msgType">消息类型，枚举</param>
        /// <param name="msg">消息体</param>
        /// <returns></returns>
        public static bool Send(MessageType msgType, string msg)
        {
            string messageType = ((int)msgType).ToString();
            byte[] c = new byte[1024];
            c = Encoding.UTF8.GetBytes(messageType + msg);

            try
            {
                server.Send(c);
                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
        }
        /// <summary>
        /// 从服务器断开连接
        /// </summary>
        public static bool OffLine()
        {
            try
            {
                server.Close();
                isConnected = false;
                isLogin = false;
                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
        }

    }

    

}
