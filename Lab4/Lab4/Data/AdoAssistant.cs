using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;

namespace Lab4.Data
{
    public class AdoAssistant
    {
        private readonly string connectionString = System.Configuration.ConfigurationManager
            .ConnectionStrings["connectionStringName"].ConnectionString;

        private DataTable dt;

        public DataTable TableLoad(out SqlDataAdapter adapter)
        {
            DataTable dt = new DataTable();
            adapter = new SqlDataAdapter();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT [Id], [Назва] AS [Name], [Телефон] AS [Phone], [Адреса] AS [Address], [Сума замовлення] AS [OrderTotal] FROM [Клієнти]";
                SqlCommand command = new SqlCommand(query, connection);
                adapter.SelectCommand = command;

                try
                {
                    adapter.Fill(dt);

                    // Команди для оновлення
                    SqlCommandBuilder builder = new SqlCommandBuilder(adapter);
                    //adapter.InsertCommand = builder.GetInsertCommand();
                    adapter.UpdateCommand = builder.GetUpdateCommand();
                   // adapter.DeleteCommand = builder.GetDeleteCommand();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Помилка завантаження даних: " + ex.Message);
                }
            }

            return dt;
        }


        public void AddClient(int id, string name, string phone, string address, decimal orderTotal)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = connection.CreateCommand();
                command.CommandText = "INSERT INTO Клієнти (Id, Назва, Телефон, Адреса, [Сума замовлення]) VALUES (@Id, @Name, @Phone, @Address, @OrderTotal)";

                command.Parameters.AddWithValue("@Id", id);
                command.Parameters.AddWithValue("@Name", name);
                command.Parameters.AddWithValue("@Phone", phone);
                command.Parameters.AddWithValue("@Address", address);
                command.Parameters.AddWithValue("@OrderTotal", orderTotal);

                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                }
                catch (SqlException ex)
                {
                    MessageBox.Show("SQL Error: " + ex.Message);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Помилка підключення до БД: " + ex.Message);
                }
            }
        }


        public void DeleteClient(int id)
        {
            
            string query = "DELETE FROM [Клієнти] WHERE [Id] = @Id";

            // Створюємо об'єкт підключення
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                // Створюємо об'єкт команди для виконання запиту
                SqlCommand command = new SqlCommand(query, connection);

                // Додаємо параметр для запиту
                command.Parameters.AddWithValue("@Id", id);

                try
                {
                    connection.Open();
                    command.ExecuteNonQuery(); // Виконання запиту типу NonQuery
                    MessageBox.Show("Клієнта успішно видалено!");
                }
                catch (SqlException ex)
                {
                    MessageBox.Show("SQL Error: " + ex.Message);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Помилка при видаленні: " + ex.Message);
                }
            }
        }

        public void UpdateClient(int id, string name, string phone, string address, decimal orderTotal)
        {
            string query = "UPDATE [Клієнти] SET Назва = @Name, Телефон = @Phone, Адреса = @Address, [Сума замовлення] = @OrderTotal WHERE Id = @Id";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);

                command.Parameters.AddWithValue("@Id", id);
                command.Parameters.AddWithValue("@Name", name);
                command.Parameters.AddWithValue("@Phone", phone);
                command.Parameters.AddWithValue("@Address", address);
                command.Parameters.AddWithValue("@OrderTotal", orderTotal);

                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    MessageBox.Show("Дані клієнта успішно оновлено!");
                }
                catch (SqlException ex)
                {
                    MessageBox.Show("SQL Error: " + ex.Message);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Помилка при оновленні: " + ex.Message);
                }
            }
        }
        public void UpdateDataTable(DataTable table, SqlDataAdapter adapter)
        {
            DataTable changes = table.GetChanges();

            if (changes != null)
            {
                try
                {
                    // Оновлюємо базу даних відповідно до змін у DataTable
                    adapter.Update(changes);
                    table.AcceptChanges(); // Підтверджуємо зміни
                    MessageBox.Show("Зміни успішно збережені в базі!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка оновлення даних: {ex.Message}");
                }
            }
        }

    }
}
