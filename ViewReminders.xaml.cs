using Npgsql;
using Org.BouncyCastle.Asn1.X509;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace Individual_project_initial
{
    public partial class ViewReminders : Page
    {
        private List<Reminder> reminderOptions = new List<Reminder>();

        public ViewReminders()
        {
            InitializeComponent();
            int userId = GetLoginOwner();

            try
            {
                using (var dbHelper = new DatabaseHelper())
                {
                    using (var connection = dbHelper.GetConnection())
                    {
                        string query = @"SELECT reminderpk, ""date"", note, userfk 
                                         FROM public.reminders 
                                         WHERE userfk = @userId";

                        using (var command = new NpgsqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@userId", userId);
                            using (var reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    Reminder reminder = new Reminder
                                    {
                                        ReminderPK = reader.GetInt32(0),
                                        Date = reader.GetDateTime(1),
                                        Note = reader.IsDBNull(2) ? null : reader.GetString(2),
                                        UserFK = reader.GetInt32(3)
                                    };
                                    reminderOptions.Add(reminder);
                                }
                            }
                        }
                    }
                }

                foreach (var reminder in reminderOptions)
                {
                    TextBlock textBlock = new TextBlock
                    {
                        Text = $"Reminder Date: {reminder.Date:g}\nNote: {reminder.Note}",
                        TextWrapping = TextWrapping.Wrap,
                        Margin = new Thickness(0, 5, 0, 5)
                    };

                    Button deleteButton = new Button
                    {
                        Content = "Delete",
                        Margin = new Thickness(5, 0, 0, 0),
                        Tag = reminder.ReminderPK
                    };
                    deleteButton.Click += DeleteButton_Click;

                    ReminderStackPanel.Children.Add(textBlock);
                    ReminderStackPanel.Children.Add(deleteButton);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading reminders: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int reminderId)
            {
                var result = MessageBox.Show("Are you sure you want to delete this reminder?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (var dbHelper = new DatabaseHelper())
                        using (var connection = dbHelper.GetConnection())
                        {
                            string deleteQuery = "DELETE FROM public.reminders WHERE reminderpk = @reminderId";
                            using (var command = new NpgsqlCommand(deleteQuery, connection))
                            {
                                command.Parameters.AddWithValue("@reminderId", reminderId);
                                command.ExecuteNonQuery();
                            }
                        }
                        MessageBox.Show("Reminder deleted successfully.", "Deleted", MessageBoxButton.OK, MessageBoxImage.Information);
                        NavigationService?.Refresh();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting reminder: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void SetReminderButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new Uri("Reminders.xaml", UriKind.Relative));
        }

        private int GetLoginOwner()
        {
            return Login.GetOwner();
        }
    }
}
