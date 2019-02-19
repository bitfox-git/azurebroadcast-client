using Bitfox.AzureBroadcast;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DemoApp
{
    public partial class Form1 : Form
    {

        BroadcastClient<string> client;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            client = new BroadcastClient<string>(Properties.Settings.Default.AzureFunctionUrl, Properties.Settings.Default.AzureFunctionCode);

            client.onMessage = (msg, info) =>
            {
                BeginInvoke((MethodInvoker)(() => {
                    textBox2.Text += $"{msg} from {info.fromUser} for group: {info.toGroupName} {Environment.NewLine} ";
                }));
            };
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
             client.Start();
        }

        private void btnJoin_Click(object sender, EventArgs e)
        {
            client.JoinGroup("samplegroupname");
        }

        private void btnLeave_Click(object sender, EventArgs e)
        {
            client.LeaveGroup("samplegroupname");
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            client.Send(textBox1.Text);
        }

        private void btnSendGroup_Click(object sender, EventArgs e)
        {
            client.SendToGroup(textBox1.Text, "samplegroupname");
        }

        private void cbFilter_CheckedChanged(object sender, EventArgs e)
        {
            client.FilterOwnMessages = cbFilter.Checked;
        }
    }
}
