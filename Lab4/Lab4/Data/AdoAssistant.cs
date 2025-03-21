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

        public DataTable TableLoad()
        {
            if (dt != null) return dt;
            dt = new DataTable();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = connection.CreateCommand();
                SqlDataAdapter adapter = new SqlDataAdapter(command);

                command.CommandText = "SELECT [Id], [Назва] AS [Name], [Телефон] AS [Phone], [Адреса] AS [Address], [Сума замовлення] AS [OrderTotal] FROM [Клієнти]";

                try
                {
                    adapter.Fill(dt);
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
            // SQL-запит для видалення клієнта за його ID
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

    }
}
