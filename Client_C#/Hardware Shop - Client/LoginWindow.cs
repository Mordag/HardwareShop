﻿using System;
using System.Data.SQLite;
using System.Windows.Forms;

namespace Hardware_Shop_Client
{
    public partial class LoginWindow : Form
    {

        public LoginWindow()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            ClientMain.databaseController.getConnection().Close();
        }

        private void button_login_Click(object sender, EventArgs e)
        {
            loginUser();
        }

        private void loginUser()
        {
            string sql = "SELECT role, password FROM user WHERE username = '" + textBox_user.Text + "';";
            SQLiteCommand command = new SQLiteCommand(sql, ClientMain.databaseController.getConnection());
            SQLiteDataReader reader = command.ExecuteReader();

            if (reader.Read())
            {
                if ((string)reader["password"] == textBox_password.Text &&
                    (int)reader["role"] == 1)
                {
                    Hide();
                    ClientMain.searchWindow.Show();
                }
            }
            else
            {
                MessageBox.Show("Invalid input. Try again.", "Error Message");
                Console.WriteLine("User fehlt!!");
            }
            reader.Close();
        }
    }
}
