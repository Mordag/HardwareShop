﻿using System;
using System.Data.SQLite;
using System.Windows.Forms;

namespace Hardware_Shop_Client
{
    /// <summary>
    /// Dies ist die GUI Klasse zum Tag Tool Fenster. Das Fenster dient zum Bearbeiten von Tags.
    /// </summary>
    public partial class TagToolWindow : Form
    {
        /// <summary>
        /// Interne Initialisierung der GUI Elemente des Fensters.
        /// </summary>
        public TagToolWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Diese Funktion wird ausgeführt wenn das Fenster via "X" geschlossen wird. Dies ist wichtig um alle 
        /// relevanten Threads vom Client zu beenden und gleichzeitig die Datenbank Schnittstelle zu schließen.
        /// </summary>
        /// <param name="e">Internes Argument welches weitere Infos zu diesem "Schließen" Event hält</param>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            ClientMain.searchWindow.Enabled = true;
        }

        /// <summary>
        /// Wenn der Benutzer auf den "Create" Button klickt, wird ein neuer Tag in die Datenbank geschrieben.
        /// </summary>
        /// <param name="sender">Beobachtes GUI Element des Observers</param>
        /// <param name="e">Event Parameter der GUI</param>
        private void button_create_Click(object sender, EventArgs e)
        {
            int lastID = 0;

            string sql = "SELECT id FROM tag;";
            SQLiteCommand command = new SQLiteCommand(sql, ClientMain.databaseController.getConnection());

            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
                lastID = (int)reader["id"];
            reader.Close();

            sql = "INSERT INTO tag (id,tag_name) "
                + "VALUES (" + (lastID + 1) + ", '" + textBox_create.Text + "');";
            command = new SQLiteCommand(sql, ClientMain.databaseController.getConnection());
            command.ExecuteNonQuery();

            MessageBox.Show("Tag has been created.", "Info");

            textBox_search.Text = textBox_create.Text;
            textBox_create.Text = "";
            executeSearch();
        }

        /// <summary>
        /// Wenn der Benutzer auf den "Edit" Button klickt, wird ein bestehender Tag in der Datenbank geändert auf
        /// Basis der Benutzereingabe. Der Tag, welcher bearbeitet wird, basiert auf der Auswahl von der Tabelle.
        /// </summary>
        /// <param name="sender">Beobachtes GUI Element des Observers</param>
        /// <param name="e">Event Parameter der GUI</param>
        private void button_edit_Click(object sender, EventArgs e)
        {
            int tagID = (int)dataGridView_results.SelectedRows[0].Cells[0].Value;

            string sql = "UPDATE tag " +
                "SET tag_name = '" + textBox_edit.Text + "'" +
                " WHERE id = " + tagID + ";";
            SQLiteCommand command = new SQLiteCommand(sql, ClientMain.databaseController.getConnection());
            command.ExecuteNonQuery();

            MessageBox.Show("Tag has been edited.", "Info");

            textBox_search.Text = textBox_edit.Text;
            executeSearch();
        }

        /// <summary>
        /// Wenn der Benutzer auf den "Delete" Button klickt, wird ein bestehender Tag in der Datenbank gelöscht.
        /// Der Tag, welcher gelöscht wird, basiert auf der Auswahl von der Tabelle.
        /// </summary>
        /// <param name="sender">Beobachtes GUI Element des Observers</param>
        /// <param name="e">Event Parameter der GUI</param>
        private void button_delete_Click(object sender, EventArgs e)
        {
            int tagID = (int)dataGridView_results.SelectedRows[0].Cells[0].Value, amount = 0;

            string sql = "SELECT id FROM search WHERE tag_id = " + tagID + ";";
            SQLiteCommand command = new SQLiteCommand(sql, ClientMain.databaseController.getConnection());

            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
                amount++;
            reader.Close();

            if (amount == 0)
            {
                sql = "DELETE FROM tag WHERE id = " + tagID + ";";
                command = new SQLiteCommand(sql, ClientMain.databaseController.getConnection());
                command.ExecuteNonQuery();

                MessageBox.Show("Tag has been deleted.", "Info");

                textBox_search.Text = "";
                executeSearch();
            }
            else
                MessageBox.Show("Tag cannot be deleted because of remaining items related to this tag.", "Info");
        }

        /// <summary>
        /// Wenn der Benutzer auf den "Search" Button klickt, wird die Suche nach Tags ausgeführt.
        /// </summary>
        /// <param name="sender">Beobachtes GUI Element des Observers</param>
        /// <param name="e">Event Parameter der GUI</param>
        private void button_search_Click(object sender, EventArgs e)
        {
            executeSearch();
        }

        /// <summary>
        /// Wenn der Benutzer im Suchfeld die Taste "Enter" drückt, wird die Suche ausgeführt.
        /// </summary>
        /// <param name="sender">Beobachtes GUI Element des Observers</param>
        /// <param name="e">Event Parameter der GUI</param>
        private void textBox_search_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                executeSearch();
                e.Handled = true;
            }
        }

        /// <summary>
        /// Wenn der Benutzer in der Tabelle seine Auswahl der Tags (welche er angeklickt hat) ändert,
        /// wird die Textbox angepasst, die zum ggf. angepassen des Namens verwendet wird.
        /// </summary>
        /// <param name="sender">Beobachtes GUI Element des Observers</param>
        /// <param name="e">Event Parameter der GUI</param>
        private void dataGridView_results_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView_results.SelectedRows.Count > 0)
                textBox_edit.Text = (string)dataGridView_results.SelectedRows[0].Cells[1].Value;
        }

        /// <summary>
        /// Diese Funktion kümmert sich um die Suche nach Tags. Es wird dabei nach exakten Treffern und nach ähnlichen
        /// Namen gesucht.
        /// </summary>
        private void executeSearch()
        {
            string sql;

            if (textBox_search.Text == "")
                sql = "SELECT id,tag_name FROM tag;";
            else
                sql = "SELECT id,tag_name FROM tag"
                      + " WHERE tag_name = '" + textBox_search.Text + "' "
                      + "OR tag_name LIKE '%" + textBox_search.Text + "%';";

            SQLiteCommand command = new SQLiteCommand(sql, ClientMain.databaseController.getConnection());
            SQLiteDataReader reader = command.ExecuteReader();

            dataGridView_results.Rows.Clear();
            while (reader.Read())
            {
                int amount = 0;

                string sql2 = "SELECT id FROM search WHERE tag_id = " + reader["id"] + ";";
                SQLiteCommand command2 = new SQLiteCommand(sql2, ClientMain.databaseController.getConnection());

                SQLiteDataReader reader2 = command2.ExecuteReader();
                while (reader2.Read())
                    amount++;
                reader2.Close();

                dataGridView_results.Rows.Add((int)reader["id"], (string)reader["tag_name"], amount);
            }
            reader.Close();
        }
    }
}
