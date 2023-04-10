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
                    else if (comboBox2.SelectedItem.ToString() == "172.30.34.7")
                        baseName = "billing_8800";
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

            button1.PerformClick();
            SqlCommand sqlCommand = sqlConnection.CreateCommand();
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

                foreach (DictionaryEntry kvp in ex.Data)
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



            if (richTextBox1.Text.Contains("{") || richTextBox1.Text.Contains("}"))
            {
                int tmp = comboBox2.SelectedIndex;
                comboBox2.SelectedIndex = 0;
                button1.PerformClick();
                Dictionary<int, string> ASR = new Dictionary<int, string>();
                using (SqlCommand sqlCommand1 = sqlConnection.CreateCommand())
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
                    using (SqlCommand sqlCommand1 = sqlConnection.CreateCommand())
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
            SqlCommand sqlCommand = sqlConnection.CreateCommand();
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
            using (SqlCommand sqlCommand1 = sqlConnection.CreateCommand())
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
                        sqlCommand1.CommandText = $"EXEC {nameSP}\r\n\t\t@start = N'20230101',\r\n\t\t@stop = N'20230331',\r\n\t\t@domain = N'{item}'";
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

            SqlCommand sqlCommand = sqlConnection.CreateCommand();
            sqlCommand.CommandText = "" +
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
    }

}






/*sqlConnection = new SqlConnection();
                                {
                                    sqlConnection.ConnectionString = "Server=i82z0report01; Initial Catalog=webpbxReportDB; Integrated Security=SSPI;";
                                    sqlConnection.Open();
                                }

    */