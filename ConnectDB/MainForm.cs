using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Security;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Net;
using System.Reflection;
using System.Data.Common;
using System.Diagnostics.Eventing.Reader;


namespace ConnectDB
{
    public partial class MainForm : Form
    {
        DbConnection sqlConnection = null;
        List<string> stored = new List<string>();
        string name = String.Empty;
        string password = string.Empty;

        public MainForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string[] auth = DataBuffer.userName.Split('\\');
            string user_domain, user_name;
            if (auth.Count() == 2)
            {
                user_name = auth[1];
                user_domain = auth[0];
            }
            else
                return;
            using (var item = new Impersonator(user_name,user_domain, DataBuffer.userPassword))
            {
                List<string> dtBases = new List<string>();
                try
                {
                    if (comboBox3.Text == "MS SQL")
                    {
                        sqlConnection = new SqlConnection();
                        Dictionary<string, string> ip_pool = new Dictionary<string, string>();
                        ip_pool.Add("i82z0report01.vats.local", "10.243.32.196");
                        ip_pool.Add("p0a8i82z0db01.vats.local", "10.243.32.164");
                        ip_pool.Add("p0a8i82z0db02.vats.local", "10.243.32.165");
                        ip_pool.Add("p0a8i82z1bps01.vats.local", "10.243.32.230");
                        ip_pool.Add("p0a8i82z1bps02.vats.local", "10.243.32.230");
                        ip_pool.Add("p0a8i82z2bps01.vats.local", "10.243.33.54");
                        ip_pool.Add("p0a8i82z2bps02.vats.local", "10.243.33.54");

                        {
                            string baseName = "master";
                            string host = String.Empty;
                            if (Regex.IsMatch(comboBox1.SelectedItem.ToString(), @"^((25[0-5]|(2[0-4]|1\d|[1-9]|)\d)(\.(?!$)|$)){4}$"))
                            {
                                //Network.PTRLookup(comboBox1.SelectedItem.ToString());
                                //IPAddress address = IPAddress.Parse(comboBox1.SelectedItem.ToString());
                                //host = Dns.GetHostByName(address.ToString()).HostName;

                                host = ip_pool.Keys.Where(s => ip_pool[s] == comboBox1.SelectedItem.ToString()).FirstOrDefault();
                            }
                            else
                                host = comboBox1.SelectedItem.ToString();


                            sqlConnection.ConnectionString = $"Server={host}; Initial Catalog={baseName}; Integrated Security=SSPI;";
                            sqlConnection.Open();
                            SqlCommand sqlCommand = (sqlConnection as SqlConnection).CreateCommand();
                            sqlCommand.CommandText = "" +
                                                    "SELECT " +
                                                    "dbid, name " +
                                                    "FROM " +
                                                    "dbo.sysdatabases";

                            using (SqlDataReader sqlDataReader = sqlCommand.ExecuteReader())
                            {
                                if (sqlDataReader.HasRows)
                                {
                                    while (sqlDataReader.Read())
                                    {
                                        dtBases.Add(sqlDataReader.GetString(1));
                                    }

                                }
                            }
                            if (dtBases.Contains(comboBox2.Text))
                            {
                                sqlCommand.CommandText = $"use {comboBox2.Text}";
                                sqlCommand.ExecuteNonQuery();
                            }

                        }
                    }
                    else 
                    {
                    }
    
                }
                catch (Exception ex)
                {

                    MessageBox.Show(ex.Message);
                    sqlConnection.Close();
                    OnLoad(e);
                }

            }



        }


        private void button2_Click(object sender, EventArgs e)
        {



            button1.PerformClick();
            if (comboBox3.SelectedText == "MS SQL")
            {
                SqlCommand sqlCommand = (sqlConnection as SqlConnection).CreateCommand();
                sqlCommand.CommandText = "" +
                    "WITH CTE_common (name, definition) " +
                    "as (select schema_name(schema_id) + '.' + name, object_definition(object_id) " +
                    "from sys.procedures " +
                    "union " +
                    "SELECT s.name + '.' + o.name, m.definition " +
                    "  FROM sys.sql_modules m " +
                    "INNER JOIN sys.objects o " +
                    "        ON m.object_id = o.object_id " +
                    "inner join sys.schemas s on s.schema_id = o.schema_id " +
                    "WHERE o.type_desc like '%function%') " +
                    "SELECT * from CTE_common ";
                if (!String.IsNullOrEmpty(textBox1.Text) || !String.IsNullOrEmpty(textBox2.Text))
                {
                    sqlCommand.CommandText += "where ";
                }
                if (!String.IsNullOrEmpty(textBox1.Text) && String.IsNullOrEmpty(textBox2.Text))
                {
                    sqlCommand.CommandText += $"name like '%{textBox1.Text}%' ";
                }
                if (!String.IsNullOrEmpty(textBox1.Text) && !String.IsNullOrEmpty(textBox2.Text))
                {
                    sqlCommand.CommandText += $"name like '%{textBox1.Text}%' and definition like '%{textBox2.Text}%'";
                }
                if (String.IsNullOrEmpty(textBox1.Text) && !String.IsNullOrEmpty(textBox2.Text))
                {
                    sqlCommand.CommandText += $"definition  like '%{textBox2.Text}%'";
                }

                string distributive = String.Empty;
                string curPath = String.Empty;
                getData(sqlCommand, dataGridView1);
                string path = String.Empty;
                if (comboBox1.SelectedItem.ToString() == "i82z0report01.vats.local")
                    path = "WebRS";
                else if (comboBox1.SelectedItem.ToString() == "p0a8i82z1bps01.vats.local" || comboBox1.SelectedItem.ToString() == "p0a8i82z1bps02.vats.local" || comboBox1.SelectedItem.ToString() == "10.243.32.230")
                    path = "Billing_1";
                else if (comboBox1.SelectedItem.ToString() == "p0a8i82z2bps01.vats.local" || comboBox1.SelectedItem.ToString() == "p0a8i82z2bps02.vats.local" || comboBox1.SelectedItem.ToString() == "10.243.33.54")
                    path = "Billing_2";
                else if (comboBox1.SelectedItem.ToString() == "172.30.34.8" || comboBox1.SelectedItem.ToString() == "172.30.34.7")
                    path = "8_800";
                else
                    path = "Other";

                distributive = $"{Environment.GetEnvironmentVariable("SystemDrive")}{Environment.GetEnvironmentVariable("HOMEPATH")}\\Desktop\\storedproceduresrs";
                curPath = distributive + "\\" + path;

                if (!Directory.Exists(distributive))
                    Directory.CreateDirectory(distributive);
                if (Directory.Exists(curPath))
                    Directory.Delete(curPath, true);
                if (!Directory.Exists(curPath))
                    Directory.CreateDirectory(curPath);

                if (MessageBox.Show("Открыть папку?", "Диалоговое окно", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    Process.Start("explorer.exe", curPath);
                }
                string list_files = String.Empty;
                foreach (DataGridViewRow item in dataGridView1.Rows)
                {
                    if (!String.IsNullOrEmpty(item.Cells[0].FormattedValue.ToString()))
                    {
                        using (StreamWriter sw = new StreamWriter(curPath + "\\" + item.Cells[0].FormattedValue.ToString() + ".sql"))
                        {
                            sw.WriteLine(item.Cells[1].FormattedValue.ToString());
                            string text = item.Cells[0].FormattedValue.ToString();
                            int count = stored.Count(u => u == path + "\\" + text);
                            if (count == 0)
                            {
                                stored.Add(path + "\\" + item.Cells[0].FormattedValue.ToString());
                            }
                        }
                        list_files += item.Cells[0].FormattedValue.ToString() + Environment.NewLine;
                    }
                }
                using (StreamWriter sw = new StreamWriter(Environment.GetEnvironmentVariable("USERPROFILE") + "\\Desktop" + "\\" + "list_files.txt"))
                {
                    sw.WriteLine(list_files);
                }
            }
        
            else
            {

            }
            
        }

        private void button3_Click(object sender, EventArgs e)
        {

            button1.PerformClick();
            SqlCommand sqlCommand = (sqlConnection as SqlConnection).CreateCommand();
            sqlCommand.CommandText = "" +
                "SELECT Tables.Table_Catalog as [Имя БД], tables.table_SCHEMA as [Имя схемы], tables.TABLE_NAME AS [Имя таблицы] " +
                "FROM INFORMATION_SCHEMA.TABLES " +
                 "where TABLE_TYPE = 'Base Table'  ";

            if (!String.IsNullOrEmpty(textBox3.Text) && String.IsNullOrEmpty(textBox4.Text))
            {
                sqlCommand.CommandText += $" and TABLE_NAME='{textBox3.Text}' ";
            }
            if (!String.IsNullOrEmpty(textBox3.Text) && !String.IsNullOrEmpty(textBox4.Text))
            {
                sqlCommand.CommandText += $"and TABLE_NAME='{textBox3.Text}' and table_SCHEMA  like '{textBox4.Text}' ";
            }
            if (String.IsNullOrEmpty(textBox3.Text) && !String.IsNullOrEmpty(textBox4.Text))
            {
                sqlCommand.CommandText += $"and table_SCHEMA  like '%{textBox4.Text}%' ";
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
            button1.PerformClick();
            SqlCommand sqlCommand = (sqlConnection as SqlConnection).CreateCommand();

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
                SqlCommand sqlCommand = (sqlConnection as SqlConnection).CreateCommand();

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
            button1.PerformClick();
            try
            {

                /*              sqlConnection = new SqlConnection();
                              {
                                  sqlConnection.ConnectionString = $"Server={comboBox1.SelectedItem.ToString()}; Initial Catalog={comboBox2.SelectedItem.ToString()}; Integrated Security=SSPI;";
                                  sqlConnection.Open();*/
                using (SqlCommand sqlCommand = (sqlConnection as SqlConnection).CreateCommand())
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
                /*
                foreach (DictionaryEntry kvp in ex.Data)
                {
                    MessageBox.Show(kvp.Key + "  " + kvp.Value);
                }
                */

            }
        }

        private void dataGridView2_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {


            if (dataGridView1.Columns[dataGridView2.CurrentCell.ColumnIndex].HeaderText == "Имя таблицы")
            {
                if (sqlConnection == null)
                    button1.PerformClick();
                SqlCommand sqlCommand = (sqlConnection as SqlConnection).CreateCommand();

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

                foreach (DictionaryEntry kvp in ex.Data)
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
            Wellcome wellcome = new Wellcome();
            this.Hide();
            if (wellcome.ShowDialog() == DialogResult.OK)
            {
                this.Show();
            }
            else
                this.Close();
            comboBox1.SelectedIndex = 0;
            this.dateTimePicker1.Value = DateTime.Now.AddDays(-DateTime.Now.Day + 1).AddMonths(-3);
            textBox12.Text = Environment.GetEnvironmentVariable("USERPROFILE") + "\\DESKTOP";
            this.dateTimePicker2.Value = DateTime.Now.AddDays(-DateTime.Now.Day);
            comboBox1_SelectedIndexChanged(sender, e);

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
            if (thread != null)
                thread.Abort();
        }



        private void button7_Click(object sender, EventArgs e)
        {



            if (richTextBox1.Text.Contains("{") || richTextBox1.Text.Contains("}"))
            {
                int tmp = comboBox2.SelectedIndex;
                comboBox2.SelectedIndex = 0;
                button1.PerformClick();
                Dictionary<int, string> ASR = new Dictionary<int, string>();
                using (SqlCommand sqlCommand1 = (sqlConnection as SqlConnection).CreateCommand())
                {
                    sqlCommand1.CommandText = "EXEC Rep.DEFIR_FileTypeC_RS";
                    using (SqlDataReader sqlDataReader = sqlCommand1.ExecuteReader())
                    {
                        if (sqlDataReader.HasRows)
                        {
                            while (sqlDataReader.Read())
                            {
                                ASR.Add(sqlDataReader.GetInt16(0), sqlDataReader.GetString(1));
                            }
                        }
                    }
                }
                comboBox2.SelectedIndex = 1;
                button1.PerformClick();
                foreach (var AsrList in ASR)
                {
                    using (SqlCommand sqlCommand1 = (sqlConnection as SqlConnection).CreateCommand())
                    {
                        sqlCommand1.CommandText = String.Format(richTextBox1.Text, AsrList.Key.ToString());
                        using (SqlDataReader sqlDataReader = sqlCommand1.ExecuteReader())
                        {
                            if (sqlDataReader.HasRows)
                            {
                                while (sqlDataReader.Read())
                                {
                                    using (StreamWriter sw = new StreamWriter("ASR\\" + AsrList.Value.Replace("<", "_").Replace(">", "_") + ".sql"))
                                        if (sqlDataReader.GetValue(0) != DBNull.Value)
                                            sw.WriteLine(sqlDataReader.GetString(0));
                                }
                            }
                        }
                    }

                }

                return;
            }
            button1.PerformClick();
            SqlCommand sqlCommand = (sqlConnection as SqlConnection).CreateCommand();
            sqlCommand.CommandText = richTextBox1.Text;
            // MessageBox.Show(sqlCommand.CommandText);

            getData(sqlCommand, dataGridView1);



            foreach (DataGridViewRow item in dataGridView1.Rows)
            {

                using (StreamWriter sw = new StreamWriter("result.sql"))
                {
                    sw.WriteLine(item.Cells[0].FormattedValue.ToString());

                }

            }



        }

        private void button8_Click(object sender, EventArgs e)
        {
            button1.PerformClick();
            string[] list = richTextBox1.Text.Split(',');
            using (SqlCommand sqlCommand1 = (sqlConnection as SqlConnection).CreateCommand())
            {
                int count = 0;
                string nameSP = String.Empty;
                if (textBox9.Text.Contains("800"))
                {

                    nameSP = "Rep.DEFIR_Report_Charges_8800_RS";
                }
                else
                    nameSP = "Rep.DEFIR_Report_Charges_history_RS_slow";
                using (StreamWriter sw = new StreamWriter("detail.csv"))
                {
                    foreach (string item in list)
                    {
                        sqlCommand1.CommandText = $"EXEC {nameSP}\r\n\t\t@start = N'{dateTimePicker1.Value.ToString("yyyyMMdd")}',\r\n\t\t@stop = N'{dateTimePicker2.Value.ToString("yyyyMMdd")}',\r\n\t\t@domain = N'{item}'";
                        using (SqlDataReader sqlDataReader = sqlCommand1.ExecuteReader())
                        {
                            //int count2 = 0;
                            if (sqlDataReader.HasRows)
                            {
                                if (count == 0)
                                {
                                    for (int i = 0; i < sqlDataReader.FieldCount; i++)
                                    {
                                        sw.Write(sqlDataReader.GetName(i) + ";");
                                    }
                                    sw.Write(Environment.NewLine);
                                    count++;
                                }
                                while (sqlDataReader.Read())
                                {
                                    for (int i = 0; i < sqlDataReader.FieldCount; i++)
                                    {
                                        sw.Write(sqlDataReader.GetValue(i).ToString() + ";");
                                    }
                                    sw.Write(Environment.NewLine);
                                    count++;
                                }
                            }
                            /*if (count2 ==0)
                            {
                                for (int i = 0; i < sqlDataReader.FieldCount; i++)
                                {
                                    if (i == 3)
                                        sw.Write(item);
                                    sw.Write(";");
                                }
                                sw.Write(Environment.NewLine);
                            }
                            */
                        }

                    }
                }
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            button1.PerformClick();

            SqlCommand sqlCommand = (sqlConnection as SqlConnection).CreateCommand();
            sqlCommand.CommandText = ""+
                "SELECT CONCAT(TABLE_SCHEMA,'.',TABLE_NAME)" +
                "from " +
                "INFORMATION_SCHEMA.TABLES " +
                "where " +
                "TABLE_TYPE = 'BASE TABLE'";
            getData(sqlCommand, dataGridView2);
            string tableName = String.Empty;
            foreach (DataGridViewRow dataGridViewRow in dataGridView2.Rows)
            {
                tableName = dataGridViewRow.Cells[0].Value.ToString();
                sqlCommand.CommandText = "" +
                        "DECLARE       " +
                        "@object_name SYSNAME     " +
                        ", @object_id INT     " +
                        ", @SQL NVARCHAR(MAX) " +
                        "SELECT " +
                        "      @object_name = '' + OBJECT_SCHEMA_NAME(o.[object_id]) + '.' + OBJECT_NAME([object_id]) +'' " +
                        "    , @object_id = [object_id] " +
                        $"FROM(SELECT[object_id] = OBJECT_ID('{tableName}', 'U')) o                                      " +
                        "" +
                        "SELECT @SQL = 'CREATE TABLE ' + @object_name + CHAR(13) + '(' + CHAR(13) + STUFF((" +
                        "  SELECT CHAR(13) + '    , ' + c.name + ' ' + " +
                        "      CASE WHEN c.is_computed = 1 " +
                        "          THEN 'AS ' + OBJECT_DEFINITION(c.[object_id], c.column_id) " +
                        "          ELSE " +
                        "              CASE WHEN c.system_type_id != c.user_type_id " +
                        "                  THEN '' + SCHEMA_NAME(tp.[schema_id]) + '.' + tp.name + '' " +
                        "                  ELSE '' + UPPER(tp.name) + '' " +
                        "              END + " +
                        "              CASE " +
                        "                  WHEN tp.name IN('varchar', 'char', 'varbinary', 'binary') " +
                        "                      THEN '(' + CASE WHEN c.max_length = -1 " +
                        "                                      THEN 'MAX' " +
                        "                                      ELSE CAST(c.max_length AS VARCHAR(5)) " +
                        "                                  END + ')' " +
                        "                  WHEN tp.name IN('nvarchar', 'nchar') " +
                        "                      THEN '(' + CASE WHEN c.max_length = -1 " +
                        "                                      THEN 'MAX' " +
                        "                    ELSE CAST(c.max_length / 2 AS VARCHAR(5)) " +
                        "                                  END + ')' " +
                        "                  WHEN tp.name IN('datetime2', 'time2', 'datetimeoffset') " +
                        "                      THEN '(' + CAST(c.scale AS VARCHAR(5)) + ')' " +
                        "                  WHEN tp.name = 'decimal' " +
                        "                      THEN '(' + CAST(c.[precision] AS VARCHAR(5)) + ',' + CAST(c.scale AS VARCHAR(5)) + ')' " +
                        "                  ELSE '' " +
                        "              END + " +
              /*"              CASE WHEN c.collation_name IS NOT NULL AND c.system_type_id = c.user_type_id " +
              "                  THEN ' COLLATE ' + c.collation_name " +
              "                  ELSE '' " +
              "              END + " +
              "              CASE WHEN c.is_nullable = 1 " +
              "                  THEN ' NULL' " +
              "                  ELSE ' NOT NULL' " +
              "              END + " +
              "+ " +
             "               CASE WHEN c.default_object_id != 0 " +
             "                  THEN ' CONSTRAINT ' + OBJECT_NAME(c.default_object_id) + '' + " +
             "                       ' DEFAULT ' + OBJECT_DEFINITION(c.default_object_id) " +
             "                  ELSE '' " +
             "              END " +
             "                 CASE WHEN cc.[object_id] IS NOT NULL " +
             "                  THEN ' CONSTRAINT ' + cc.name + ' CHECK ' + cc.[definition] " +
             "                  ELSE '' " +
             "              END + " +
             "              CASE WHEN c.is_identity = 1 " +
             "                  THEN ' IDENTITY(' + CAST(IDENTITYPROPERTY(c.[object_id], 'SeedValue') AS VARCHAR(5)) + ',' + " +
             "                                  CAST(IDENTITYPROPERTY(c.[object_id], 'IncrementValue') AS VARCHAR(5)) + ')' " +
             "                  ELSE '' " +
             "              END  + " + 
         */  "     '' END " +
                        "  FROM sys.columns c WITH(NOLOCK) " +
                        "  JOIN sys.types tp WITH(NOLOCK) ON c.user_type_id = tp.user_type_id " +
                        "  LEFT JOIN sys.check_constraints cc WITH(NOLOCK) ON c.[object_id] = cc.parent_object_id AND cc.parent_column_id = c.column_id " +
                        "  WHERE c.[object_id] = @object_id " +
                        "  ORDER BY c.column_id " +
                        "  FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 7, '      ') +" +
                       /* "  ISNULL((SELECT ' " +
                        "  , CONSTRAINT' + i.name + ' PRIMARY KEY ' + " +
                        "  CASE WHEN i.index_id = 1 " +
                        "      THEN 'CLUSTERED' " +
                        "      ELSE 'NONCLUSTERED' " +
                        "  END + ' (' + ( " +
                        " SELECT STUFF(CAST(( " +
                        "     SELECT ', ' + COL_NAME(ic.[object_id], ic.column_id) + '' + " +
                        "              CASE WHEN ic.is_descending_key = 1 " +
                        "                  THEN ' DESC' " +
                        "                  ELSE '' " +
                        "              END " +
                        "      FROM sys.index_columns ic WITH(NOLOCK) " +
                        "      WHERE i.[object_id] = ic.[object_id] " +
                        "          AND i.index_id = ic.index_id " +
                        "      FOR XML PATH(N''), TYPE) AS NVARCHAR(MAX)), 1, 2, '')) +')' " +
                        "    FROM sys.indexes i WITH(NOLOCK) " +
                        "   WHERE i.[object_id] = @object_id " +
                        "AND i.is_primary_key = 1), '') + CHAR(13) + ');
                       */ "'      )'" +
                        " select  @SQL";
                getData(sqlCommand, dataGridView1);
                richTextBox1.Text = dataGridView1.Rows[0].Cells[0].Value.ToString();
                richTextBox1.Text = richTextBox1.Text.Replace("     ", Environment.NewLine);
                if (!Directory.Exists("SQL_files"))
                    Directory.CreateDirectory("SQL_files");
                using (StreamWriter sw = new StreamWriter("SQL_Files\\" + tableName + ".sql"))
                {
                    sw.WriteLine(richTextBox1.Text);
                }
            }
        }

        private void button10_Click(object sender, EventArgs e)
        {
            string curDir = Environment.CurrentDirectory + "\\" + "PS_sql";
            string oldDir = Environment.CurrentDirectory + "\\" + "SQL_files";
            if (!String.IsNullOrEmpty(textBox12.Text) && !String.IsNullOrEmpty(richTextBox1.Text))
            {
                curDir = textBox12.Text;
                string text = richTextBox1.Text;
                text = text.Replace("[", "").Replace("]", "").Replace("N'", "'").Replace("SET IDENTITY", "").Replace("INSERT", ";" +"?" +"INSERT INTO")
                    .Replace("\n", " ").Replace("\r","").Replace("\t", "").Replace(" ,", ",").Replace("  ", " ").Replace("GO", " ")
                    .Replace("( ","(").Replace(" )", ")").Replace(" ,", ",").Replace("_;", ";").Replace(" ;", ";").Replace("DATETIMEOFFSET", "timestamptz(6)");

                var listText = text.Split('?');
                using (StreamWriter sw = new StreamWriter($"{textBox12.Text}\\current.sql"))
                {
                   foreach(string item in listText)
                    {
                        if (item.Contains("USE")|| item.Contains("USE")|| item.Contains("ON") || item.Contains("OFF") || item.Contains("DELETE"))
                            continue;
                        sw.WriteLine(item);
                    }
                }
                    return;
            }
            if (!Directory.Exists(curDir))
                Directory.CreateDirectory(curDir);
            foreach(string file in Directory.EnumerateFiles($"{Directory.GetCurrentDirectory()}" + "\\" + "SQL_files"))
            {
                string text = String.Empty;
       
                using (StreamReader sr = new StreamReader(file))
                {
                    text = sr.ReadToEnd();
                }
                Regex regex;
                string fileName;
                PosgreSQL posgreSQL = new PosgreSQL();
                foreach(KeyValuePair<string, string> item in posgreSQL.sqlTypes)
                {
                    if (item.Key.Contains("(") && !item.Value.Contains("("))
                    {
                        regex = new Regex($"({item.Key.Substring(0, item.Key.IndexOf('('))})\\([0-9]+\\)", RegexOptions.IgnoreCase);
                        bool ext = regex.IsMatch(text);
                        text = Regex.Replace(text, regex.ToString(), item.Value);
                        regex = new Regex($"({item.Key.Substring(0, item.Key.IndexOf('('))})\\([A-Z]+\\)", RegexOptions.IgnoreCase);
                        text = Regex.Replace(text, regex.ToString(), item.Value);

                    }
                    else
                    {
                        if (text.Contains("DATETIMEOFFSET"))
                        {
                            regex = new Regex("(DATETIMEOFFSET)\\([0-9+]\\)", RegexOptions.IgnoreCase);
                            var type = Regex.Match(text, regex.ToString());
                            regex = new Regex("[0-9]+");
                            var number = Regex.Match(type.Value, regex.ToString());
                            regex = new Regex($"({item.Key.Substring(0, item.Key.IndexOf('('))})\\([0-9]+\\)", RegexOptions.IgnoreCase);
                            text = Regex.Replace(text, regex.ToString(), $"TIMESTAMP({number.Value}) WITH TIME ZONE");
                        }
                        else
                        
                        regex = new Regex($"({item.Key})");
                    }
                        text = Regex.Replace(text, regex.ToString(), item.Value, RegexOptions.IgnoreCase);
                        text = Regex.Replace(text, "\n", string.Empty);
                        text = Regex.Replace(text, "  ", " ");
                        text = Regex.Replace(text, "\t", string.Empty);
                }
                text = text.Replace("N'", "'");
                regex = new Regex("[A-Za-z_.0-9]+.(sql)");
                fileName = regex.Match(file).ToString();
                using (StreamWriter sw = new StreamWriter(curDir + "\\" + fileName))
                {
                    sw.Write(text);
                }
            }


        }

        private void textBox10_TextChanged(object sender, EventArgs e)
        {
            if(String.IsNullOrWhiteSpace(((TextBox)sender).Text))
            {
                textBox11.Text = String.Empty;
            }
            PosgreSQL posgreSQL = new PosgreSQL();
            if(posgreSQL.sqlTypes.Count(u => u.Key.Equals(((TextBox)sender).Text)) == 0)
            textBox11.Text = posgreSQL.sqlTypes.FirstOrDefault(u => u.Key.Contains(((TextBox)sender).Text)).Value;
            else
                textBox11.Text = posgreSQL.sqlTypes.FirstOrDefault(u => u.Key.Equals(((TextBox)sender).Text)).Value;

        }

        private void button11_Click(object sender, EventArgs e)
        {
            button1.PerformClick();
            for (int i = 0; i<comboBox1.Items.Count; i++)
            {
                if(comboBox1.Items[i].ToString() == "i82z0report01.vats.local")
                {
                    comboBox1.SelectedItem = comboBox1.Items[i];
                }
            }
            for (int i = 0; i < comboBox2.Items.Count; i++)
            {
                if (comboBox2.Items[i].ToString() == "ReportServer")
                {
                    comboBox2.SelectedItem = comboBox2.Items[i];
                }
            }
       
            
            int num = 0;
            using (SqlCommand sqlCommand1 = (sqlConnection as SqlConnection).CreateCommand())
            {
                sqlCommand1.CommandText = "WITH ItemContentBinaries AS\r\n(\r\nSELECT\r\nPath\r\n,CONVERT(varbinary(max),Content) AS Content\r\nFROM ReportServer.dbo.Catalog\r\nWHERE Type IN (2,5,7,8)\r\nand content is not null\r\nand path not Like ('%Data Sources%')\r\nand path not in ('')\r\n),\r\n--The second CTE strips off the BOM if it exists…\r\nItemContentNoBOM AS\r\n(\r\nSELECT\r\nPath\r\n,CASE\r\nWHEN LEFT(Content,3) = 0xEFBBBF\r\nTHEN CONVERT(varchar(max),SUBSTRING(Content,4,LEN(Content)))\r\nELSE\r\nContent\r\nEND AS Content\r\nFROM ItemContentBinaries\r\n)\r\n--–The outer query gets the content in its varbinary, varchar and xml representations…\r\nSELECT\r\nReplace(Path, '/', '\\') AS Path\r\n,CONVERT(varbinary(max),Content) AS ReportData --–varbinary\r\n\r\nFROM ItemContentNoBOM";
                SqlDataReader sqlDataReader = sqlCommand1.ExecuteReader();
                string mainDirectory = Environment.GetEnvironmentVariable("USERPROFILE") + "\\DESKTOP\\ReportServer";
                 if (!Directory.Exists(mainDirectory))
                    Directory.CreateDirectory(Environment.GetEnvironmentVariable("USERPROFILE") + "\\DESKTOP\\ReportServer");
                while (sqlDataReader.Read())
                {
                    string[] pathMassive = sqlDataReader.GetString(0).Split('\\');
                    ArraySegment<string> myPath = new ArraySegment<string>(pathMassive, 1, pathMassive.Length-1);
                    string pathDirectory = String.Join("\\", myPath);
                    string parrentDir = mainDirectory + "\\" + pathDirectory.Substring(0, pathDirectory.LastIndexOf("\\"));
                    if (!Directory.Exists(parrentDir))
                        Directory.CreateDirectory(parrentDir);
                    //Console.WriteLine((++num).ToString() + Directory.GetCurrentDirectory() + sqlDataReader.GetString(0) + ".rdl");
                    using (StreamWriter streamWriter = new StreamWriter(mainDirectory + "\\" + pathDirectory + ".rdl"))
                        streamWriter.WriteLine(Encoding.UTF8.GetString((byte[])sqlDataReader.GetValue(1)));
                    num++;
                }
                MessageBox.Show(num.ToString());
            }
        }


        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            button1.PerformClick();
            SqlCommand sqlCommand = (sqlConnection as SqlConnection).CreateCommand();
            sqlCommand.CommandText = "" +
            "SELECT " +
            "dbid, name " +
            "FROM " +
            "master.dbo.sysdatabases";
            using (SqlDataReader sqlDataReader = sqlCommand.ExecuteReader())
            {
                comboBox2.Items.Clear();
                if (sqlDataReader.HasRows)
                {
                    while (sqlDataReader.Read())
                    {
                       // if(sqlDataReader.GetInt16(0)>4)
                        comboBox2.Items.Add(sqlDataReader.GetSqlString(1));
                    }
      
                    comboBox2.SelectedIndex = 0;

                }
            }
        }
        Thread thread;
        private void button13_Click(object sender, EventArgs e)
        {
            richTextBox1.Text = String.Empty;

            thread = new Thread(new ThreadStart(() =>
               {
                   int i = 1;
                   int j = 1;
                   File.Delete($"{textBox12.Text}\\current.sql");
                   using (StreamWriter sw = new StreamWriter($"{textBox12.Text}\\current.sql"))
                   {
             
                       string text = String.Empty;
                       IEnumerable<string> lines = File.ReadLines(textBox12.Text + "\\script.sql");
                       decimal count = lines.Count();
                 
                       foreach (string line in lines)
                       {
                           
                           i++;

                           label10.Text = $"{(i / count * 100).ToString("0.00")}%";
                           text = line.Replace("[", "").Replace("]", "").Replace("N'", "'").Replace("INSERT", "INSERT INTO")
                           .Replace("\t", "").Replace(" ,", ",").Replace("  ", " ")
                           .Replace("( ", "(").Replace(" )", ")").Replace(" ,", ",").Replace("_;", ";").Replace(" ;", ";").Replace("DateTimeOffset", "timestamptz(6)");


                           {
                               if (text.Contains("USE") || text == "" || text.Contains("GO") || text.Contains("ON") || text.Contains("OFF") || text.Contains("DELETE") || text.Contains("SET"))
                                   continue;
                               sw.WriteLine(text);
                               j++;
                           }


                       }
                   }
                   richTextBox1.Text = $"Исходных строк {i}, ";
               }
                
                ));
            thread.Start();
        }

        private void button14_Click(object sender, EventArgs e)
        {
            if(thread != null)
            thread.Abort();
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            comboBox1.Items.Clear();
            if (comboBox1.Text == "MS SQL")
            {
                comboBox1.Items.AddRange(new object[] {
            "i82z0report01.vats.local",
            "p0a8i82z1bps01.vats.local",
            "p0a8i82z2bps02.vats.local",
            "172.30.34.7",
            "172.30.34.8",
            "p0a8i82z0db02.vats.local",
            "p0a8i82z1bps02.vats.local",
            "10.243.32.230",
            "10.243.33.54",
            "p0a8i82z0lst01.vats.local"});
            }
            else
            {
                comboBox1.Items.AddRange(new object[] {
            "10.243.32.246",
            "10.243.33.70",
            "10.243.208.114"   });

            }
        }
    }

}


