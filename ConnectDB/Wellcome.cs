using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
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
            if (!String.IsNullOrEmpty(textBox2.Text))
            {
                result = true;
                this.Close();
            }
            else MessageBox.Show("Пароль пуст или не введен");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Wellcome_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (result)
            {
                this.DialogResult = DialogResult.OK;
                DataBuffer.userName = textBox1.Text;
                DataBuffer.userPassword = textBox2.Text;
            }
            else
                this.DialogResult = DialogResult.Cancel;
        }

        private void Wellcome_Load(object sender, EventArgs e)
        {
            string path = String.Empty;
            path = Environment.GetEnvironmentVariable("USERPROFILE");
            path += "\\Desktop\\MyName.txt";
            if(File.Exists(path))
                using (StreamReader sr = new StreamReader(path))
                {
                    textBox1.Text += sr.ReadLine();
                }
            button1.Focus();
        }
        

    }
}
