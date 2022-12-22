using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace ConnectDB
{
    public partial class MainForm : Form
    {
        SqlConnection sqlConnection = null;
        public MainForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {

                sqlConnection = new SqlConnection();
                {
                    sqlConnection.ConnectionString = "Server=localhost; Initial Catalog=webpbxReportDB; Integrated Security=SSPI;";
                    sqlConnection.Open();


                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (sqlConnection == null)
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
            getData(sqlCommand, dataGridView1);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (sqlConnection == null)
                button1.PerformClick();
            SqlCommand sqlCommand = sqlConnection.CreateCommand();
            sqlCommand.CommandText = "" +
                "SELECT Table_Catalog as[Имя БД], table_SCHEMA as [Имя схемы], TABLE_NAME AS[Имя таблицы] " +
                "FROM INFORMATION_SCHEMA.TABLES " +
                "WHERE table_type = 'BASE TABLE' ";
            if (!String.IsNullOrEmpty(textBox3.Text) || !String.IsNullOrEmpty(textBox4.Text))
            {
                sqlCommand.CommandText += "where ";
            }
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
        }


        private void button4_Click(object sender, EventArgs e)
        {
            if (sqlConnection == null)
                button1.PerformClick();
            SqlCommand sqlCommand = sqlConnection.CreateCommand();

            sqlCommand.CommandText = "" +
                  "SELECT Table_Catalog as[Имя БД], table_SCHEMA as [Имя схемы], TABLE_NAME AS[Имя таблицы], " +
                  "COLUMN_NAME AS[Имя столбца], " +
                  "DATA_TYPE AS[Тип данных столбца], " +
                  "IS_NULLABLE AS[Значения NULL] " +
                  "FROM INFORMATION_SCHEMA.COLUMNS ";
            if (!String.IsNullOrEmpty(textBox5.Text) || !String.IsNullOrEmpty(textBox6.Text))
            {
                sqlCommand.CommandText += "where ";
                if (!String.IsNullOrEmpty(textBox5.Text) && String.IsNullOrEmpty(textBox6.Text))
                {
                    sqlCommand.CommandText += $"Table_Name like '%{textBox5.Text}%' ";
                    sqlCommand.CommandText += " order by Table_Name";
                }
                if (!String.IsNullOrEmpty(textBox5.Text) && !String.IsNullOrEmpty(textBox6.Text))
                {
                    sqlCommand.CommandText += $"Table_Name like '%{textBox5.Text}%' and Column_Name  like '%{textBox6.Text}%'";
                    sqlCommand.CommandText += " order by Table_name, Column_Name";
                }
                if (String.IsNullOrEmpty(textBox5.Text) && !String.IsNullOrEmpty(textBox6.Text))
                {
                    sqlCommand.CommandText += $"Column_Name  like '%{textBox6.Text}%'";
                    sqlCommand.CommandText += " order by Column_Name";
                }
            }
            else
                sqlCommand.CommandText += "order by Table_Name, Column_Name";
            getData(sqlCommand, dataGridView1);
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
                        MessageBox.Show(sqlCommand.CommandText);
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
    }

}
