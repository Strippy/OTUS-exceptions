using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;



namespace ToDoText
{
    using System;

    class Program
    {
        const string _fileName = "todo.txt";
        const string _logFileName = "log.txt";

        //Если файл не найден при добавлении новой задачи, пользователю предлагается сначала создать новый и добавить задачу в него.
        static void AddTask(string name, DateTime date)
        {
            var task = new ToDoTask(name, date);
            var dir = Path.GetDirectoryName(_fileName);
            if (FileUtils.IsFilePathValid(_fileName) && dir != "" && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            if (!File.Exists(_fileName))
            {
                WriteLineQuestion($"Файл '{_fileName}' отсутствует, нажмите Y для создания файла и продолжения или любую другую кнопку для выхода из программы");
                var key = Console.ReadKey();
                if(key.Key == ConsoleKey.Y)
                    using (File.Create(_fileName))
                    {
                         Console.WriteLine("Файл создан");   
                    }
                else
                    return;
            }
            File.AppendAllLines(_fileName, new[] { $"{task.Date.ToString("dd/MM/yy", CultureInfo.InvariantCulture)}\t{task.Name}" });
        }

        //Если файл не найден при выводе задач (today, all), то выводиться сообщение: "Файла с задачами пока нет, сначала создайте задачу".
        //Если при выводе задач, какие-то строки из файла не получилось спарсить, то эти строки выводятся вначале с предупредительным сообщением, а лишь потом все валидные задачи.
        static ToDoTask[] GetTasks()
        {
            var dir = Path.GetDirectoryName(_fileName);
            if (FileUtils.IsFilePathValid(_fileName) && dir != "" && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            if (!File.Exists(_fileName))
                WriteLineError("Файла с задачами пока нет, сначала создайте задачу"!);
            var errors = new List<string>();
            var tasks = new List<ToDoTask>();
            var todoTxtLines = File.ReadAllLines(_fileName);
            foreach (var line in todoTxtLines)
            {
                if (ToDoTask.TryParse(line, out var toDo))
                    tasks.Add(toDo);
                else
                    errors.Add(line);
            }

            if(errors.Any())
                WriteLineWarning($"Следующие строки не получилось спарсить:");
            foreach (var error in errors)
            {
                WriteLineWarning($"\t{error}");
            }
            return tasks.ToArray();
        }


        static void ListTodayTasks()
        {
            var tasks = GetTasks();
            ShowTasks(tasks, DateTime.Today, DateTime.Today);
        }

        static void ListAllTasks()
        {
            var tasks = GetTasks();
            var minDate = tasks.Min(t => t.Date);
            var maxDate = tasks.Max(t => t.Date);
            ShowTasks(tasks, minDate, maxDate);
        }

        static void ShowTasks(ToDoTask[] tasks, DateTime from, DateTime to)
        {
            for (var date = from.Date; date < to.Date.AddDays(1); date += TimeSpan.FromDays(1))
            {
                var dateTasks = tasks.Where(t => t.Date >= date && t.Date < date.AddDays(1)).ToArray();
                if (dateTasks.Any())
                {
                    Console.WriteLine($"{date:dd/MM/yy}");
                    foreach (var task in dateTasks)
                    {
                        Console.WriteLine($"\t{task.Name}");
                    }
                }
            }
        }
        //Команда add принимает на вход название 2-м аргументом и дату 3-м аргументом.Если аргументы невалидные, то выводиться соответствующее сообщение.
        //Приложение проверяет количество аргументов для конкретной команды.
        //Если была передана неверная команда, то пользователю выводится соответствующее сообщение.
        static void Main(string[] args)
        {
            if (!args.Any())
            {
                WriteLineError($"Запуск программы без парамтеров!");
                Console.ReadKey();
                return;
            }

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            switch (args[0])
            {
                case "add":
                    try
                    {
                        //Приложение проверяет количество аргументов для конкретной команды
                        if (args.Length != 3)
                        {
                            WriteLineError("Некорркетное количетво парамтеров для команды Add");
                            Console.ReadKey();
                            return;
                        }
                        var name = args[1];
                        if(DateTime.TryParseExact(args[2], "dd/MM/yy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                            AddTask(name, date);
                        else
                        {
                            WriteLineError("Некорркетная дата для команды Add");
                            Console.ReadKey();
                            return;
                        }
                    }
                    //Команда add принимает на вход название 2-м аргументом и дату 3-м аргументом. Если аргументы невалидные, то выводиться соответствующее сообщение.
                    catch (ArgumentNullException e)
                    {
                        WriteLineError($"Некорркетный параметер для команды Add {e.Message}");
                        Console.ReadKey();
                        return;
                    }
                    catch (ArgumentException e)
                    {
                        WriteLineError($"Некорркетный параметер для команды Add {e.Message}");
                        Console.ReadKey();
                        return;
                    }
                    break;
                case "today":
                    ListTodayTasks();
                    break;
                case "all":
                    ListAllTasks();
                    break;
                //Если была передана неверная команда, то пользователю выводится соответствующее сообщение.
                default:
                    WriteLineError($"Некорркетный параметер!");
                    Console.ReadKey();
                    return;
            }
        }

        //Все необработанные исключения пойманные в глобальном обработчике пишуться в log.txt.
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var dir = Path.GetDirectoryName(_fileName);
            if (FileUtils.IsFilePathValid(_fileName) && dir != "" && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            if (!File.Exists(_logFileName))
                using (File.Create(_logFileName))
                {
                    Console.WriteLine("Файл создан");
                }
            File.AppendAllLines(_logFileName, new []{ $"{(e.ExceptionObject as Exception)?.Message} {(e.ExceptionObject as Exception)?.InnerException}" });
        }

        private static void WriteLineWarning(string errorMessage)
        {
            WriteColoredLine(errorMessage, ConsoleColor.Yellow);
        }
        private static void WriteLineError(string errorMessage)
        {
            WriteColoredLine(errorMessage, ConsoleColor.Red);
        }
        private static void WriteLineQuestion(string message)
        {
            WriteColoredLine(message, ConsoleColor.White);
        }
        private static void WriteColoredLine(string message, ConsoleColor color)
        {
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ForegroundColor = oldColor;
        }
    }
}
