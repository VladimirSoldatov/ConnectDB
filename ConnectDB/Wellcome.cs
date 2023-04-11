using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ConnectDB
{
    public partial class Wellcome : Form
    {
        bool result = false;
        public Wellcome()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            result = true;
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Wellcome_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (result)
                this.DialogResult = DialogResult.OK;
            else
                this.DialogResult = DialogResult.Cancel;
        }
    }
}
