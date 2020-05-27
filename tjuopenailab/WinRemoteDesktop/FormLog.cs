using System;
using AxMSTSCLib;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;



namespace tjuremoteAI
{
    public partial class FormLog : Form
    {
        private AxMsRdpClient7NotSafeForScripting axMsRdpc = null;
        private bool isFullScreen = false;
        private List<string> axMsRdpcArray = null;
        public FormLog()
        {
            InitializeComponent();
            axMsRdpcArray = new List<string>();
        }
        #region 全局定义
        /// <summary>
        /// 创建远程桌面连接
        /// </summary>
        /// <param name="args">参数数组 new string[] { ServerIp, UserName, Password }</param>
        private void CreateAxMsRdpClient(string[] args)
        {
            string[] ServerIps = args[0].Split(':');

            Form axMsRdpcForm = new Form();
            axMsRdpcForm.ShowIcon = false;
            //axMsRdpcForm.StartPosition = FormStartPosition.Manual;
            axMsRdpcForm.Name = string.Format("Form_{0}", ServerIps[0].Replace(".", ""));
            axMsRdpcForm.Text = string.Format("{0} ({1})", args[3], ServerIps[0]);
            
            axMsRdpcForm.Size = new Size(1024, 768);
            axMsRdpcForm.FormClosed += new FormClosedEventHandler(this.axMsRdpcForm_Closed);

            //Rectangle ScreenArea = Screen.PrimaryScreen.Bounds;
            // 给axMsRdpc取个名字
            string _axMsRdpcName = string.Format("axMsRdpc_{0}", ServerIps[0].Replace(".", ""));
            if (axMsRdpcArray.Contains(_axMsRdpcName))
            {
                Global.WinMessage("此远程已经连接，请勿重复连接！"); return;
            }
            else
            {
                axMsRdpc = new AxMsRdpClient7NotSafeForScripting();
            }
            // 添加到当前缓存
            axMsRdpcArray.Add(_axMsRdpcName);

            ((System.ComponentModel.ISupportInitialize)(axMsRdpc)).BeginInit();
            axMsRdpc.Dock = DockStyle.Fill;
            axMsRdpc.Enabled = true;

            // 绑定连接与释放事件
            axMsRdpc.OnConnecting += new EventHandler(this.axMsRdpc_OnConnecting);
            axMsRdpc.OnDisconnected += new IMsTscAxEvents_OnDisconnectedEventHandler(this.axMsRdpc_OnDisconnected);

            axMsRdpcForm.Controls.Add(axMsRdpc);
            axMsRdpcForm.WindowState = FormWindowState.Maximized;
            axMsRdpcForm.Show();
            ((System.ComponentModel.ISupportInitialize)(axMsRdpc)).EndInit();

            // RDP名字
            axMsRdpc.Name = _axMsRdpcName;
            // 服务器地址
            axMsRdpc.Server = ServerIps[0];
            // 远程登录账号
            axMsRdpc.UserName = args[1];
            // 远程端口号
            axMsRdpc.AdvancedSettings7.RDPPort = ServerIps.Length == 1 ? 3389 : Convert.ToInt32(ServerIps[1]);
            //axMsRdpc.AdvancedSettings7.ContainerHandledFullScreen = 1;
            // 自动控制屏幕显示尺寸
            //axMsRdpc.AdvancedSettings7.SmartSizing = true;
            // 启用CredSSP身份验证（有些服务器连接没有反应，需要开启这个）
            axMsRdpc.AdvancedSettings7.EnableCredSspSupport = true;
            // 远程登录密码
            axMsRdpc.AdvancedSettings7.ClearTextPassword = args[2];
            // 禁用公共模式
            //axMsRdpc.AdvancedSettings7.PublicMode = false;
            // 颜色位数 8,16,24,32
            axMsRdpc.ColorDepth = 32;
            // 开启全屏 true|flase
            axMsRdpc.FullScreen = this.isFullScreen;
            // 设置远程桌面宽度为显示器宽度
            //axMsRdpc.DesktopWidth = ScreenArea.Width;
            axMsRdpc.DesktopWidth = axMsRdpcForm.ClientRectangle.Width;
            // 设置远程桌面宽度为显示器高度
            //axMsRdpc.DesktopHeight = ScreenArea.Height;
            axMsRdpc.DesktopHeight = axMsRdpcForm.ClientRectangle.Height;
            // 远程连接
            axMsRdpc.Connect();
        }

        #endregion

        #region 远程桌面组件axMsRdpc
        // 远程桌面-连接
        private void axMsRdpc_OnConnecting(object sender, EventArgs e)
        {
            var _axMsRdp = sender as AxMsRdpClient7NotSafeForScripting;
            _axMsRdp.ConnectingText = _axMsRdp.GetStatusText(Convert.ToUInt32(_axMsRdp.Connected));
            _axMsRdp.FindForm().WindowState = FormWindowState.Normal;
        }
        // 远程桌面-连接断开
        private void axMsRdpc_OnDisconnected(object sender, IMsTscAxEvents_OnDisconnectedEvent e)
        {
            var _axMsRdp = sender as AxMsRdpClient7NotSafeForScripting;
            string disconnectedText = string.Format("远程桌面 {0} 连接已断开！", _axMsRdp.Server);
            _axMsRdp.DisconnectedText = disconnectedText;
            _axMsRdp.FindForm().Close();
            Global.WinMessage(disconnectedText, "远程连接");

        }
        #endregion

        #region 远程桌面窗体axMsRdpcForm
        // 远程桌面窗体-关闭
        private void axMsRdpcForm_Closed(object sender, FormClosedEventArgs e)
        {
            Form frm = (Form)sender;
            //MessageBox.Show(frm.Controls[0].GetType().ToString());
            foreach (Control ctrl in frm.Controls)
            {
                // 找到当前打开窗口下面的远程桌面
                if (ctrl.GetType().ToString() == "AxMSTSCLib.AxMsRdpClient7NotSafeForScripting")
                {
                    // 释放缓存
                    if (axMsRdpcArray.Contains(ctrl.Name)) axMsRdpcArray.Remove(ctrl.Name);
                    // 断开连接
                    var _axMsRdp = ctrl as AxMsRdpClient7NotSafeForScripting;
                    if (_axMsRdp.Connected != 0)
                    {
                        _axMsRdp.Disconnect();
                        _axMsRdp.Dispose();
                    }
                }
            }

            // 主动断开连接
            //TODO
        }
        #endregion
        private void buttonAbout_Click(object sender, EventArgs e)
        {
            //Server.OffLine();
            FormAbout frm = new FormAbout();
            frm.ShowDialog();
        }

        private void buttonLog_Click(object sender, EventArgs e)
        {
            if (Server.IsConnected == false)
            {
                Server.Connect("101.200.220.209", 60001);
                string msg = textBoxUserName.Text + " " + textBoxPassWord.Text;
                Server.Send(MessageType.LoginData, msg);

            }
            else
            {
                MessageBox.Show("已经连接到实验管理系统,忽略本次连接请求", "本地提示");
            }
            CreateAxMsRdpClient(new string[] {
                    Server.ip,//"112.95.161.205:10003"
                    Server.userName,
                    Server.passWord,
                    //"openailab",
                    "天津大学&开放智能机器有限公司远程人工智能实验室"
                });


            
        }

        private void buttonExit_Click(object sender, EventArgs e)
        {
            if (Server.OffLine())
            {
                this.Close();
                System.Environment.Exit(0);
            }
            else
            {
                MessageBox.Show("断开连接失败，请重试", "错误");
            }
        }
    }
}
