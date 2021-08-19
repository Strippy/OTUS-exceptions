using System;
using System.Globalization;

namespace ToDoText
{
    public class ToDoTask
    {
        //Любые переданные на вход в конструктор ToDoTask неправильные данные - исключительная ситуация.Ограничения на данные у него такие:
        //Date в диапазоне между 2000/01/01 и 2100/01/01.
        //Name не пустой.
        public static readonly DateTime MinDate = new DateTime(2000, 1, 1);
        public static readonly DateTime MaxDate = new DateTime(2100, 1, 1);

        public ToDoTask(string name, DateTime date)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("Не задано имя!");
            Name = name;
            if (date > MaxDate || date < MinDate)
                throw new ArgumentException($"Некорректная дата, должна быть в диапазоне между {MinDate:yyyy/MM/dd} и {MaxDate:yyyy/MM/dd}!");
            Date = date;
        }

        public static ToDoTask Parse(string input)
        {
            if (string.IsNullOrEmpty(input))
                throw new ArgumentNullException("Переданы пустые данные!");
            var split = input.Split('\t');
            if (split.Length != 2)
                throw new ArgumentException("Некорректное количество параметров!");
            if (!DateTime.TryParseExact(split[0], "dd/MM/yy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                throw new ArgumentException("Некорректная дата!");
            var name = split[1];
            return new ToDoTask(name, date);
        }

        public static bool TryParse(string input, out ToDoTask toDoTask)
        {
            bool res;
            try
            {
                toDoTask = Parse(input);
                res = true;
            }
            catch
            {
                toDoTask = null;
                res = false;
            }

            return res;
        }

        public DateTime Date { get; }
        public string Name { get; }
    }
}
