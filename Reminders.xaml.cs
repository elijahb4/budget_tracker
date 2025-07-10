using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.TimeZoneInfo;

namespace Individual_project_initial
{
    public partial class Reminders : Page
    {
        public Reminders()
        {
            InitializeComponent();
        }
        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            if (ReminderDate.SelectedDate == null || ReminderNote.Document == null ||
                ReminderNote.Document.Blocks.Count == 0)
            {
                MessageBox.Show("All fields are required");
                return;
            }
            int owner = GetLoginOwner();
            string note = Note(ReminderNote);
            if (string.IsNullOrWhiteSpace(note))
            {
                MessageBox.Show("Note cannot be empty.");
                return;
            }
            DateTime reminder_date_input = ReminderDate.SelectedDate.Value;
            if (reminder_date_input < DateTime.Now)
            {
                MessageBox.Show("Reminder date cannot be in the past.");
                return;
            }
            DateOnly reminder_date = DateOnly.FromDateTime(reminder_date_input);
            using (var dbHelper = new DatabaseHelper())
            try
            {
                using (var connection = dbHelper.GetConnection())
                {
                    string query = "INSERT INTO reminders (userFK, date, note) VALUES (@owner, @date, @note)";
                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@owner", owner);
                        command.Parameters.AddWithValue("@date", reminder_date);
                        command.Parameters.AddWithValue("@note", note);
                        command.ExecuteNonQuery();
                    }
                }
            MessageBox.Show("Reminder Set");
            }
            catch (NpgsqlException ex) {
            MessageBox.Show("Error: " + ex.Message);
            }
        }
        string Note(RichTextBox ReminderNote)
        {
            TextRange textRange = new TextRange(
                ReminderNote.Document.ContentStart,
                ReminderNote.Document.ContentEnd
            );
            return textRange.Text;
        }
        private int GetLoginOwner()
        {
            return Login.GetOwner();
        }
    }
}