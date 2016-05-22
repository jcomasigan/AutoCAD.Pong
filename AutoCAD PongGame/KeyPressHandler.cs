using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PongGame
{
    public partial class KeyPressHandler : Form
    {
        public KeyPressHandler()
        {
            InitializeComponent();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Up)
            {
                Pong.keyPress = "UP";
            }
            if (keyData == Keys.Down)
            {
                Pong.keyPress = "DOWN";
            }
            if (keyData == Keys.Escape)
            {
                Pong.keyPress = "QUIT";
            }
            return base.ProcessCmdKey(ref msg, keyData);
}

        private void KeyPressHandler_FormClosing(object sender, FormClosingEventArgs e)
        {
            Pong.keyPress = "QUIT";
        }
    }
}
