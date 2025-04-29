using System.Windows.Input;

namespace FortuneTeller.Commands 
{
    public static class DataCommands
    {
        // Оголошення статичних властивостей RoutedCommand
        public static RoutedCommand Undo { get; private set; }
        public static RoutedCommand New { get; private set; }
        public static RoutedCommand Replace { get; private set; } // Редагувати
        public static RoutedCommand Save { get; private set; }
        public static RoutedCommand Find { get; private set; }
        public static RoutedCommand Delete { get; private set; }

        // Статичний конструктор для ініціалізації команд та комбінацій клавіш
        static DataCommands()
        {
            // Скасувати (Ctrl+Z)
            InputGestureCollection undoGestures = new InputGestureCollection();
            undoGestures.Add(new KeyGesture(Key.Q, ModifierKeys.Control, "Ctrl+Z"));
            Undo = new RoutedCommand("Undo", typeof(DataCommands), undoGestures);

            // Створити (Ctrl+N)
            InputGestureCollection newGestures = new InputGestureCollection();
            newGestures.Add(new KeyGesture(Key.N, ModifierKeys.Control, "Ctrl+N"));
            New = new RoutedCommand("New", typeof(DataCommands), newGestures);

            // Редагувати (F2 або Ctrl+E - оберіть один варіант, тут F2)
            InputGestureCollection replaceGestures = new InputGestureCollection();
            replaceGestures.Add(new KeyGesture(Key.F2, ModifierKeys.None, "F2"));
            // Або: replaceGestures.Add(new KeyGesture(Key.E, ModifierKeys.Control, "Ctrl+E"));
            Replace = new RoutedCommand("Replace", typeof(DataCommands), replaceGestures);

            // Зберегти (Ctrl+S)
            InputGestureCollection saveGestures = new InputGestureCollection();
            saveGestures.Add(new KeyGesture(Key.S, ModifierKeys.Control, "Ctrl+S"));
            Save = new RoutedCommand("Save", typeof(DataCommands), saveGestures);

            // Знайти (Ctrl+F)
            InputGestureCollection findGestures = new InputGestureCollection();
            findGestures.Add(new KeyGesture(Key.F, ModifierKeys.Control, "Ctrl+F"));
            Find = new RoutedCommand("Find", typeof(DataCommands), findGestures);

            // Видалити (Delete)
            InputGestureCollection deleteGestures = new InputGestureCollection();
            deleteGestures.Add(new KeyGesture(Key.D, ModifierKeys.Control, "Ctrl+D"));
            Delete = new RoutedCommand("Delete", typeof(DataCommands), deleteGestures);
        }
    }
}