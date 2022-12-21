using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
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

                    MessageBox.Show("Connected");
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
            try
            {
                using (SqlDataReader sqlDataReader = sqlCommand.ExecuteReader())
                {
                    dataGridView1.Rows.Clear();
                    dataGridView1.Columns.Clear();

                    List<string> data = new List<string>();

                    for (int i = 0; i < sqlDataReader.FieldCount; i++)
                        dataGridView1.Columns.Add(sqlDataReader.GetName(i), sqlDataReader.GetName(i));


                    if (sqlDataReader.HasRows)
                    {
                        while (sqlDataReader.Read())
                        {
                            for (int i = 0; i < sqlDataReader.FieldCount; i++)
                            {
                                data.Add(sqlDataReader.GetValue(i).ToString());
                            }
                            dataGridView1.Rows.Add(data.ToArray());
                            data.Clear();
                        }
                    }
                    dataGridView1.AutoResizeColumns();
                    dataGridView1.Width = 0;
                    for (int i = 0; i < dataGridView1.Columns.Count; i++)
                    {

                        dataGridView1.Columns[i].Width = dataGridView1.Columns[0].Width;
                        dataGridView1.Width += dataGridView1.Columns[0].Width;

                    }

                    if (this.Width > 453)
                        this.Width = dataGridView1.Width + 100;
                    else
                        this.Width = 463;

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " \n " + ex.HResult + " \n " + ex.Source + " \n " + ex.StackTrace + " \n " + ex.HelpLink + " \n " + ex.HelpLink);
            }
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
            try
            {
                using (SqlDataReader sqlDataReader = sqlCommand.ExecuteReader())
                {
                    dataGridView1.Rows.Clear();
                    dataGridView1.Columns.Clear();

                    List<string> data = new List<string>();

                    for (int i = 0; i < sqlDataReader.FieldCount; i++)
                        dataGridView1.Columns.Add(sqlDataReader.GetName(i), sqlDataReader.GetName(i));


                    if (sqlDataReader.HasRows)
                    {
                        while (sqlDataReader.Read())
                        {
                            for (int i = 0; i < sqlDataReader.FieldCount; i++)
                            {
                                data.Add(sqlDataReader.GetValue(i).ToString());
                            }
                            dataGridView1.Rows.Add(data.ToArray());
                            data.Clear();
                        }
                    }
                    dataGridView1.AutoResizeColumns();
                    dataGridView1.Width = 0;
                    for (int i = 0; i < dataGridView1.Columns.Count; i++)
                    {


                        dataGridView1.Width += dataGridView1.Columns[i].Width + 10;

                    }

                    if (this.Width > 500)
                        this.Width = dataGridView1.Width + 100;
                    else
                        this.Width = 500;

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " \n " + ex.HResult + " \n " + ex.Source + " \n " + ex.StackTrace + " \n " + ex.HelpLink + " \n " + ex.HelpLink);
            }
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
            try
            {
                using (SqlDataReader sqlDataReader = sqlCommand.ExecuteReader())
                {
                    dataGridView1.Rows.Clear();
                    dataGridView1.Columns.Clear();

                    List<string> data = new List<string>();

                    for (int i = 0; i < sqlDataReader.FieldCount; i++)
                        dataGridView1.Columns.Add(sqlDataReader.GetName(i), sqlDataReader.GetName(i));


                    if (sqlDataReader.HasRows)
                    {
                        while (sqlDataReader.Read())
                        {
                            for (int i = 0; i < sqlDataReader.FieldCount; i++)
                            {
                                data.Add(sqlDataReader.GetValue(i).ToString());
                            }
                            dataGridView1.Rows.Add(data.ToArray());
                            data.Clear();
                        }
                    }
                    dataGridView1.AutoResizeColumns();
                    dataGridView1.Width = 0;
                    for (int i = 0; i < dataGridView1.Columns.Count; i++)
                    {


                        dataGridView1.Width += dataGridView1.Columns[i].Width + 10;

                    }

                    if (dataGridView1.Width > 500)
                        this.Width = dataGridView1.Width + 100;
                    else
                        this.Width = 500;
                    Size resolution = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Size;

                    dataGridView1.Width = this.Width;





                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " \n " + ex.HResult + " \n " + ex.Source + " \n " + ex.StackTrace + " \n " + ex.HelpLink + " \n " + ex.HelpLink);
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

                try
                {
                    using (SqlDataReader sqlDataReader = sqlCommand.ExecuteReader())
                    {
                        dataGridView2.Rows.Clear();
                        dataGridView2.Columns.Clear();

                        List<string> data = new List<string>();

                        for (int i = 0; i < sqlDataReader.FieldCount; i++)
                            dataGridView2.Columns.Add(sqlDataReader.GetName(i), sqlDataReader.GetName(i));


                        if (sqlDataReader.HasRows)
                        {
                            while (sqlDataReader.Read())
                            {
                                data.Clear();
                                for (int i = 0; i < sqlDataReader.FieldCount; i++)
                                {
                                    if(sqlDataReader.GetValue(i) !=null)
                                    data.Add(sqlDataReader.GetValue(i).ToString());
                                }
                                dataGridView2.Rows.Add(data.ToArray());
                                
                            }
                        }
                        dataGridView2.AutoResizeColumns();
                        dataGridView2.Width = 0;
                        for (int i = 0; i < dataGridView1.Columns.Count; i++)
                        {
                            dataGridView2.Width += dataGridView2.Columns[i].Width + 10;
                        }

                        if (dataGridView2.Width > 500)
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
            else
                MessageBox.Show(dataGridView2.Columns[dataGridView2.CurrentCell.ColumnIndex].HeaderText);
        }

    }
}
