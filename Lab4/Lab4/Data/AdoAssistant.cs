using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;

namespace Lab4.Data
{
    public class AdoAssistant
    {
        // Отримуємо рядок з'єднання з файлу App.config
        private readonly string connectionString = System.Configuration.ConfigurationManager
            .ConnectionStrings["connectionStringName"].ConnectionString;

        // Об'єкт DataTable для збереження даних
        private DataTable dt;

        // Метод для завантаження даних із таблиці "Клієнти"
        public DataTable TableLoad()
        {
            if (dt != null) return dt; // Завантажимо таблицю лише один раз
                                       // Заповнюємо об'єкт таблиці даними з БД
            dt = new DataTable();

            // Створюємо об'єкт підключення
            using (SqlConnection сonnection = new SqlConnection(connectionString))
            {
                SqlCommand command = сonnection.CreateCommand(); // Створюємо об'єкт команди
                SqlDataAdapter adapter = new SqlDataAdapter(command); // Створюємо об'єкт читання

                // SQL-запит для вибору даних із таблиці "Клієнти"
                command.CommandText = "SELECT [Id], [Назва] AS [Name], [Телефон] AS [Phone], [Адреса] AS [Address], [Сума замовлення] AS [OrderTotal] FROM [Клієнти]";

                try
                {
                    // Метод сам відкриває БД і сам її закриває
                    adapter.Fill(dt);
                }
                catch (SqlException ex)
                {
                    MessageBox.Show("SQL Error: " + ex.Message); // Показуємо детальну SQL-помилку
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Помилка підключення до БД: " + ex.Message); // Інші помилки
                }
            }
            return dt;
        }

    }
}
