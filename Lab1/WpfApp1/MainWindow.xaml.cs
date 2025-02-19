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

using System.IO;
using System.Xml.Linq;


namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
            public MainWindow()
            {
                InitializeComponent();

                // Створення прив'язки та приєднання обробників
                CommandBinding saveCommand = new CommandBinding(ApplicationCommands.Save, execute_Save, canExecute_Save);
                // Прив'язка команд для Open і Clear
                CommandBinding openCommand = new CommandBinding(ApplicationCommands.Open, Execute_Open, CanExecute_Open);
                CommandBinding clearCommand = new CommandBinding(ApplicationCommands.Delete, Execute_Clear, CanExecute_Clear);

                CommandBinding copyCommandBinding = new CommandBinding(ApplicationCommands.Copy, Execute_Copy, CanExecute_Copy);
                CommandBinding pasteCommand = new CommandBinding(ApplicationCommands.Paste, Execute_Paste, CanExecute_Paste);

            // Реєстрація прив'язки
            CommandBindings.Add(saveCommand);
            CommandBindings.Add(openCommand);
            CommandBindings.Add(clearCommand);
            CommandBindings.Add(copyCommandBinding);
            CommandBindings.Add(pasteCommand);
        }

            // Обробник події CanExecute для команди Save
            private void canExecute_Save(object sender, CanExecuteRoutedEventArgs e)
            {
            // Якщо в TextBox є текст, команда може бути виконана
            if (textBox.Text.Trim().Length > 0) e.CanExecute = true; else e.CanExecute = false;
        }

            // Обробник події Execute для команди Save
            private void execute_Save(object sender, ExecutedRoutedEventArgs e)
            {
            // Збереження тексту в файл
            File.WriteAllText("D:\\KPI\\3curs\\2 семестр\\декларативка\\myFile.txt", textBox.Text);
            MessageBox.Show("The file was saved!");

            }

        // Обробник для команди Open
        private void CanExecute_Open(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true; 
        }
        private void Execute_Open(object sender, ExecutedRoutedEventArgs e)
        {
            // Код для відкриття файлу
            var openFileDialog = new Microsoft.Win32.OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                textBox.Text = System.IO.File.ReadAllText(openFileDialog.FileName);
            }
        }

        // Обробник для команди Clear
        private void CanExecute_Clear(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true; // У цьому випадку можна завжди виконати команду
        }

        private void Execute_Clear(object sender, ExecutedRoutedEventArgs e)
        {
            textBox.Clear();
        }


        // Обробник для CanExecute для копіювання
        private void CanExecute_Copy(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !string.IsNullOrEmpty(textBox.SelectedText); // Копіювати можна лише якщо є виділений текст
        }

        // Обробник для виконання копіювання
        private void Execute_Copy(object sender, ExecutedRoutedEventArgs e)
        {
            textBox.Copy(); // Копіює виділений текст в буфер обміну
        }


        // Обробники для вставки
        private void CanExecute_Paste(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Clipboard.ContainsText(); // Активувати, якщо в буфері обміну є текст
        }

        private void Execute_Paste(object sender, ExecutedRoutedEventArgs e)
        {
            textBox.Paste(); // Вставляємо текст з буфера в TextBox
        }


        private void fontSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // Змінюємо розмір шрифта для TextBox відповідно до значення Slider
            textBox.FontSize = e.NewValue;
        }


        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

    }
}
