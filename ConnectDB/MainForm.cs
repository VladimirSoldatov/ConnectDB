using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Security;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace ConnectDB
{
    public partial class MainForm : Form
    {
        SqlConnection sqlConnection = null;
        List<string> stored = new List<string>();

        public MainForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (var item = new Impersonator("vladimir.soldatov", "VATS", "@Altavista1963"))
            {

                sqlConnection = new SqlConnection();
                {
                    string baseName;
                    if (comboBox2.SelectedItem.ToString() == "i82z0report01.vats.local")
                        baseName = "webpbxReportDB";
                    else
                        baseName = "billing_federal";

                    sqlConnection.ConnectionString = $"Server={comboBox2.SelectedItem.ToString()}; Initial Catalog={baseName}; Integrated Security=SSPI;";

                    sqlConnection.Open();

                }

            }



        }


        private void button2_Click(object sender, EventArgs e)
        {


            button1.PerformClick();

            SqlCommand sqlCommand = sqlConnection.CreateCommand();
            sqlCommand.CommandText = "" +
                "select name, object_definition(object_id) " +
                "from sys.procedures ";
            if (!String.IsNullOrEmpty(textBox1.Text) || !String.IsNullOrEmpty(textBox2.Text))
            {
                sqlCommand.CommandText += "where ";
            }
            if (!String.IsNullOrEmpty(textBox1.Text) && String.IsNullOrEmpty(textBox2.Text))
            {
                sqlCommand.CommandText += $"name={textBox1.Text} ";
            }
            if (!String.IsNullOrEmpty(textBox1.Text) && !String.IsNullOrEmpty(textBox2.Text))
            {
                sqlCommand.CommandText += $"name={textBox1.Text} and object_definition(object_id)  like '{textBox2.Text}'";
            }
            if (String.IsNullOrEmpty(textBox1.Text) && !String.IsNullOrEmpty(textBox2.Text))
            {
                sqlCommand.CommandText += $"object_definition(object_id)  like '%{textBox2.Text}%'";
            }
            MessageBox.Show(sqlCommand.CommandText);
            string distributive = String.Empty;
            string curPath = String.Empty;
            getData(sqlCommand, dataGridView1);
            string path = String.Empty;
            if (comboBox2.SelectedItem.ToString() == "i82z0report01.vats.local")
                path = "WebRS";
            else
                path = "Billing";
            distributive = $"{Environment.GetEnvironmentVariable("SystemDrive")}{Environment.GetEnvironmentVariable("HOMEPATH")}\\Desktop\\storedproceduresrs";
            curPath = distributive + "\\" + path;
            if (!Directory.Exists(distributive))
                Directory.CreateDirectory(distributive);
            if (!Directory.Exists(curPath))
                Directory.CreateDirectory(curPath);


            foreach (DataGridViewRow item in dataGridView1.Rows)
            {
                if (!String.IsNullOrEmpty(item.Cells[0].FormattedValue.ToString()))
                {
                    using (StreamWriter sw = new StreamWriter(curPath + "\\" + item.Cells[0].FormattedValue.ToString() + ".xml"))
                    {
                        sw.WriteLine(item.Cells[1].FormattedValue.ToString());
                        string text = item.Cells[0].FormattedValue.ToString();
                        int count = stored.Count(u => u == path + "\\" + text);
                        if (count == 0)
                        {
                            stored.Add(path + "\\" + item.Cells[0].FormattedValue.ToString());
                        }
                    }

                }
            }

        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (sqlConnection == null)
                button1.PerformClick();
            SqlCommand sqlCommand = sqlConnection.CreateCommand();
            sqlCommand.CommandText = "" +
                "SELECT Tables.Table_Catalog as[Имя БД], tables.table_SCHEMA as [Имя схемы], tables.TABLE_NAME AS[Имя таблицы] " +
                "FROM INFORMATION_SCHEMA.TABLES " +
                 "where TABLE_TYPE = 'Base Table'  ";

            if (!String.IsNullOrEmpty(textBox3.Text) && String.IsNullOrEmpty(textBox4.Text))
            {
                sqlCommand.CommandText += $"[Имя таблицы]={textBox1.Text} ";
            }
            if (!String.IsNullOrEmpty(textBox3.Text) && !String.IsNullOrEmpty(textBox4.Text))
            {
                sqlCommand.CommandText += $"[Имя таблицы]={textBox1.Text} and [Имя схемы]  like '{textBox4.Text}'";
            }
            if (String.IsNullOrEmpty(textBox3.Text) && !String.IsNullOrEmpty(textBox4.Text))
            {
                sqlCommand.CommandText += $"[Имя схемы]  like '%{textBox4.Text}%'";
            }
            sqlCommand.CommandText += " order by [Имя схемы], [Имя таблицы]";
            MessageBox.Show(sqlCommand.CommandText);
            getData(sqlCommand, dataGridView1);
            foreach (DataGridViewRow item in dataGridView1.Rows)
            {
                item.Cells[1].ToString();
                break;
            }
        }


        private void button4_Click(object sender, EventArgs e)
        {
            if (sqlConnection == null)
                button1.PerformClick();
            SqlCommand sqlCommand = sqlConnection.CreateCommand();

            try
            {
                sqlCommand.CommandText = "" +
                    "SELECT " +
                    "Tables.Table_Catalog as[Имя БД] " +
                     ", tables.TABLE_NAME AS[Имя таблицы] " +
                     ", Columns.COLUMN_NAME AS[Имя столбца] " +
                     ", Columns.DATA_TYPE AS[Тип данных столбца] " +
                    "FROM INFORMATION_SCHEMA.TABLES Tables " +
                    "left join INFORMATION_SCHEMA.COLUMNS Columns " +
                    "ON " +
                    "Columns.TABLE_NAME = Tables.TABLE_NAME " +
                    "and Columns.TABLE_SCHEMA = Tables.TABLE_SCHEMA " +
                    "and Columns.TABLE_CATALOG = Tables.TABLE_CATALOG " +
                    "where Tables.TABLE_TYPE = 'Base Table'  " +
                    "";


                if (!String.IsNullOrEmpty(textBox5.Text) || !String.IsNullOrEmpty(textBox6.Text))
                {

                    if (!String.IsNullOrEmpty(textBox5.Text) && String.IsNullOrEmpty(textBox6.Text))
                    {
                        sqlCommand.CommandText += $" and Tables.Table_Name like '%{textBox5.Text}%' ";
                        sqlCommand.CommandText += " order by Tables.Table_Name";
                    }
                    if (!String.IsNullOrEmpty(textBox5.Text) && !String.IsNullOrEmpty(textBox6.Text))
                    {
                        sqlCommand.CommandText += $"and Tables.Table_Name like '%{textBox5.Text}%' and Columns.Column_Name  like '%{textBox6.Text}%'";
                        sqlCommand.CommandText += " order by Tables.Table_name, Columns.Column_Name ";
                    }
                    if (String.IsNullOrEmpty(textBox5.Text) && !String.IsNullOrEmpty(textBox6.Text))
                    {
                        sqlCommand.CommandText += $" and Columns.Column_Name  like '%{textBox6.Text}%'";
                        sqlCommand.CommandText += " order by Columns.Column_Name";
                    }
                }
                else
                    sqlCommand.CommandText += "order by Tables.Table_Name, Columns.Column_Name";
                getData(sqlCommand, dataGridView1);
            }
            catch
            {
                MessageBox.Show(sqlCommand.CommandText);
            }
        }

        private void dataGridView1_DoubleClick(object sender, EventArgs e)
        {
            if (dataGridView1.Columns[dataGridView1.CurrentCell.ColumnIndex].HeaderText == "Имя таблицы")
            {
                if (sqlConnection == null)
                    button1.PerformClick();
                SqlCommand sqlCommand = sqlConnection.CreateCommand();

                sqlCommand.CommandText = "" +
                      $"SELECT TOP 10 * " +
                      $"FROM [{dataGridView1.CurrentRow.Cells[0].Value}].[{dataGridView1.CurrentRow.Cells[1].Value}].[{dataGridView1.CurrentCell.Value}]";

                getData(sqlCommand, dataGridView2);
            }
            if (dataGridView1.Columns[dataGridView1.CurrentCell.ColumnIndex].HeaderText == "Название")
            {
                try
                {
                    if (String.IsNullOrEmpty(textBox7.Text))
                        textBox7.Text = dataGridView1.CurrentCell.Value.ToString();
                    sqlConnection = new SqlConnection();
                    {
                        sqlConnection.ConnectionString = $"Server=localhost; Initial Catalog={textBox7.Text}; Integrated Security=SSPI;";
                        sqlConnection.Open();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

            }
        }



        private void button5_Click(object sender, EventArgs e)
        {
            try
            {

                sqlConnection = new SqlConnection();
                {
                    sqlConnection.ConnectionString = "Server=localhost; Initial Catalog=master; Integrated Security=SSPI;";
                    sqlConnection.Open();
                    using (SqlCommand sqlCommand = sqlConnection.CreateCommand())
                    {
                        sqlCommand.CommandText = "" +
                            "SELECT " +
                            "database_id AS[ID]" +
                          ", name AS[Название]" +
                          ", CONVERT(CHAR(10), create_date, 104) AS[Дата создания]" +
                          ", state_desc AS[Статус]" +
                          ", compatibility_level AS[Уровень совместимости]" +
                          ", recovery_model_desc AS[Модель восстановления] " +
                          "  FROM " +
                          "sys.databases";
                        getData(sqlCommand, dataGridView1);
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }


        }
        private void getData(SqlCommand sqlCommand, object temp)
        {

            try
            {
                using (SqlDataReader sqlDataReader = sqlCommand.ExecuteReader())
                {

                    (temp as DataGridView).Rows.Clear();
                    (temp as DataGridView).Columns.Clear();

                    List<string> data = new List<string>();

                    for (int i = 0; i < sqlDataReader.FieldCount; i++)
                        (temp as DataGridView).Columns.Add(sqlDataReader.GetName(i), sqlDataReader.GetName(i));


                    if (sqlDataReader.HasRows)
                    {
                        while (sqlDataReader.Read())
                        {
                            data.Clear();
                            for (int i = 0; i < sqlDataReader.FieldCount; i++)
                            {
                                if (sqlDataReader.GetValue(i) != null)
                                    data.Add(sqlDataReader.GetValue(i).ToString());

                            }

                            (temp as DataGridView).Rows.Add(data.ToArray());

                        }
                    }
                   (temp as DataGridView).AutoResizeColumns();
                    (temp as DataGridView).Width = 0;
                    for (int i = 0; i < dataGridView1.Columns.Count; i++)
                    {
                        (temp as DataGridView).Width += (temp as DataGridView).Columns[i].Width + 10;
                    }

                    if ((temp as DataGridView).Width > 500)
                        this.Width = dataGridView2.Width + 100;
                    else
                        this.Width = 500;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " \n " + ex.HResult + " \n " + ex.Source + " \n " + ex.StackTrace + " \n " + ex.HelpLink + " \n " + ex.Data.ToString());

                foreach (KeyValuePair<string, string> kvp in ex.Data)
                {
                    MessageBox.Show(kvp.Key + "  " + kvp.Value);
                }


            }
        }

        private void dataGridView2_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {


            if (dataGridView1.Columns[dataGridView2.CurrentCell.ColumnIndex].HeaderText == "Имя таблицы")
            {
                if (sqlConnection == null)
                    button1.PerformClick();
                SqlCommand sqlCommand = sqlConnection.CreateCommand();

                sqlCommand.CommandText = "" +
                      $"SELECT TOP 10 * " +
                      $"FROM [{dataGridView2.CurrentRow.Cells[0].Value}].[{dataGridView2.CurrentRow.Cells[1].Value}].[{dataGridView2.CurrentCell.Value}]";

                getData(sqlCommand, (DataGridView)sender);
            }

        }

        private void button6_Click(object sender, EventArgs e)
        {
            try
            {
                if (sqlConnection != null)
                    sqlConnection.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " \n " + ex.HResult + " \n " + ex.Source + " \n " + ex.StackTrace + " \n " + ex.HelpLink + " \n " + ex.Data.ToString());

                foreach (KeyValuePair<string, string> kvp in ex.Data)
                {
                    MessageBox.Show(kvp.Key + "  " + kvp.Value);
                }
            }
        }


        public static Process Elevated(string process, string args, string username, string password, string workingDirectory)
        {
            if (process == null || process.Length == 0) throw new ArgumentNullException("process");

            process = Path.GetFullPath(process);
            string domain = null;
            if (username != null)
                username = GetUsername(username, out domain);
            ProcessStartInfo info = new ProcessStartInfo();
            info.UseShellExecute = false;
            info.Arguments = args;
            info.WorkingDirectory = workingDirectory ?? Path.GetDirectoryName(process);
            info.FileName = process;
            info.Verb = "runas";
            info.UserName = username;
            info.Domain = domain;
            info.LoadUserProfile = true;
            if (password != null)
            {
                SecureString ss = new SecureString();
                foreach (char c in password)
                    ss.AppendChar(c);
                info.Password = ss;
            }

            return Process.Start(info);
        }

        private static string GetUsername(string username, out string domain)
        {
            SplitUserName(username, out username, out domain);

            if (domain == null && username.IndexOf('@') < 0)
                domain = Environment.GetEnvironmentVariable("USERDOMAIN");
            return username;
        }

        private static void SplitUserName(string username1, out string username2, out string domain)
        {
            var auth = username1.Split('\\');
            username2 = auth[1];
            domain = auth[0];
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            comboBox2.SelectedIndex = 0;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!Directory.Exists($"{Environment.GetEnvironmentVariable("SystemDrive")}{Environment.GetEnvironmentVariable("HOMEPATH")}\\Desktop\\storedproceduresrs"))
                Directory.CreateDirectory($"{Environment.GetEnvironmentVariable("SystemDrive")}{Environment.GetEnvironmentVariable("HOMEPATH")}\\Desktop\\storedproceduresrs");
            using (StreamWriter sw = new StreamWriter
                (
                $"{Environment.GetEnvironmentVariable("SystemDrive")}{Environment.GetEnvironmentVariable("HOMEPATH")}\\Desktop\\storedproceduresrs" + "\\" + "list.txt")
                )
            {
                foreach (string item in stored)
                    sw.WriteLine(item);

            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            button1.PerformClick();

            SqlCommand sqlCommand = sqlConnection.CreateCommand();
            sqlCommand.CommandText = textBox10.Text;
            MessageBox.Show(sqlCommand.CommandText);

            getData(sqlCommand, dataGridView1);



            foreach (DataGridViewRow item in dataGridView1.Rows)
            {

                using (StreamWriter sw = new StreamWriter("result.sql"))
                {
                    sw.WriteLine(item.Cells[0].FormattedValue.ToString());

                }

            }
        }

    }





}






/*sqlConnection = new SqlConnection();
                                {
                                    sqlConnection.ConnectionString = "Server=i82z0report01; Initial Catalog=webpbxReportDB; Integrated Security=SSPI;";
                                    sqlConnection.Open();
                                }

    */