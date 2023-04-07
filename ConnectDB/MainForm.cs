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



            if (textBox10.Text.Contains("{") || textBox10.Text.Contains("}"))
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
                        sqlCommand1.CommandText = String.Format(textBox10.Text, AsrList.Key.ToString());
                        using (SqlDataReader sqlDataReader = sqlCommand1.ExecuteReader())
                        {
                            if (sqlDataReader.HasRows)
                            {
                                while (sqlDataReader.Read())
                                {
                                    using (StreamWriter sw = new StreamWriter("ASR\\" +AsrList.Value.Replace("<", "_").Replace(">", "_") + ".sql"))
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
            sqlCommand.CommandText = textBox10.Text;
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
            string[] list = { "524661.fp.rt.ru", "491590.fp.rt.ru", "164100.fp.rt.ru", "240969.fp.rt.ru", "740066.fp.rt.ru", "661911.fp.rt.ru", "747588.fp.rt.ru", "172338.fp.rt.ru", "840678.fp.rt.ru", "428310.fp.rt.ru", "059044.fp.rt.ru", "155684.fp.rt.ru", "449786.fp.rt.ru", "322625.fp.rt.ru", "675121.fp.rt.ru", "994268.fp.rt.ru", "609665.fp.rt.ru", "598102.fp.rt.ru", "567297.fp.rt.ru", "582617.fp.rt.ru", "104679.fp.rt.ru", "528866.fp.rt.ru", "702019.fp.rt.ru", "614619.fp.rt.ru", "457882.fp.rt.ru", "925119.fp.rt.ru", "132490.fp.rt.ru", "217911.fp.rt.ru", "013470.fp.rt.ru", "599794.fp.rt.ru", "377989.fp.rt.ru", "387101.fp.rt.ru", "532803.fp.rt.ru", "976002.fp.rt.ru", "635014.fp.rt.ru", "632938.fp.rt.ru", "938016.fp.rt.ru", "618559.fp.rt.ru", "762979.fp.rt.ru", "012513.fp.rt.ru", "207100.fp.rt.ru", "650761.fp.rt.ru", "093515.fp.rt.ru", "231493.fp.rt.ru", "077976.fp.rt.ru", "366760.fp.rt.ru", "277981.fp.rt.ru", "569146.fp.rt.ru", "586075.fp.rt.ru", "998931.fp.rt.ru", "299363.fp.rt.ru", "939225.fp.rt.ru", "626218.fp.rt.ru", "297892.fp.rt.ru", "378212.fp.rt.ru", "550308.fp.rt.ru", "075212.fp.rt.ru", "678649.fp.rt.ru", "277303.fp.rt.ru", "926284.fp.rt.ru", "230776.fp.rt.ru", "281819.fp.rt.ru", "198379.fp.rt.ru", "409234.fp.rt.ru", "615500.fp.rt.ru", "953689.fp.rt.ru", "165421.fp.rt.ru", "889454.fp.rt.ru", "061865.fp.rt.ru", "667803.fp.rt.ru", "981789.fp.rt.ru", "912106.fp.rt.ru", "588430.fp.rt.ru", "898947.fp.rt.ru", "123561.fp.rt.ru", "387014.fp.rt.ru", "357542.fp.rt.ru", "603849.fp.rt.ru", "719575.fp.rt.ru", "246338.fp.rt.ru", "712211.fp.rt.ru", "557545.fp.rt.ru", "824805.fp.rt.ru", "368029.fp.rt.ru", "775906.fp.rt.ru", "395845.fp.rt.ru", "633803.fp.rt.ru", "385605.fp.rt.ru", "165983.fp.rt.ru", "219959.fp.rt.ru", "275891.fp.rt.ru", "079637.fp.rt.ru", "814591.fp.rt.ru", "244076.fp.rt.ru", "545711.fp.rt.ru", "805791.fp.rt.ru", "800392.fp.rt.ru", "100052.fp.rt.ru", "573254.fp.rt.ru", "452626.fp.rt.ru", "620016.fp.rt.ru", "709899.fp.rt.ru", "671917.fp.rt.ru", "032995.fp.rt.ru", "718447.fp.rt.ru", "562674.fp.rt.ru", "101952.fp.rt.ru", "639929.fp.rt.ru", "988195.fp.rt.ru", "911718.fp.rt.ru", "320766.fp.rt.ru", "745334.fp.rt.ru", "748451.fp.rt.ru", "494244.fp.rt.ru", "415973.fp.rt.ru", "467526.fp.rt.ru", "736726.fp.rt.ru", "164186.fp.rt.ru", "214834.fp.rt.ru", "027438.fp.rt.ru", "095892.fp.rt.ru", "710840.fp.rt.ru", "458633.fp.rt.ru", "092893.fp.rt.ru", "641648.fp.rt.ru", "486017.fp.rt.ru", "043176.fp.rt.ru", "230165.fp.rt.ru", "497820.fp.rt.ru", "345190.fp.rt.ru", "633079.fp.rt.ru", "087200.fp.rt.ru", "022767.fp.rt.ru", "305757.fp.rt.ru", "330673.fp.rt.ru", "835853.fp.rt.ru", "737787.fp.rt.ru", "191839.fp.rt.ru", "531394.fp.rt.ru", "333347.fp.rt.ru", "733793.fp.rt.ru", "956784.fp.rt.ru", "956495.fp.rt.ru", "811191.fp.rt.ru", "185704.fp.rt.ru", "426252.fp.rt.ru", "716197.fp.rt.ru", "246680.fp.rt.ru", "509612.fp.rt.ru", "428396.fp.rt.ru", "215075.fp.rt.ru", "135028.fp.rt.ru", "086315.fp.rt.ru", "015033.fp.rt.ru", "777738.fp.rt.ru", "031494.fp.rt.ru", "210493.fp.rt.ru", "354074.fp.rt.ru", "991260.fp.rt.ru", "617257.fp.rt.ru", "847174.fp.rt.ru", "955271.fp.rt.ru", "992025.fp.rt.ru", "969476.fp.rt.ru", "825116.fp.rt.ru", "028390.fp.rt.ru", "928381.fp.rt.ru", "458117.fp.rt.ru", "616457.fp.rt.ru", "738743.fp.rt.ru", "634387.fp.rt.ru", "279632.fp.rt.ru", "529914.fp.rt.ru", "648850.fp.rt.ru", "637843.fp.rt.ru", "985949.fp.rt.ru", "463140.fp.rt.ru", "164807.fp.rt.ru", "965189.fp.rt.ru", "918172.fp.rt.ru", "282526.fp.rt.ru" };
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
                    foreach(string item in list)
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
    }


}






/*sqlConnection = new SqlConnection();
                                {
                                    sqlConnection.ConnectionString = "Server=i82z0report01; Initial Catalog=webpbxReportDB; Integrated Security=SSPI;";
                                    sqlConnection.Open();
                                }

    */