using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace zad2_apbd
{
    public class DataProcessingApp
    {
        public static async Task Main(string[] args)
        {
            //Wyjątek, który obsługuje nieodpowiedną ilość argumentów wejściowych programu (wartość inna niż 4)
            if (args.Length != 4)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(args),
                    "Specified argument's value is out range valid values, is not equal to 4");
            }
            
            //ARGUMENTY (IN) WEJŚCIOWE PROGRAMU
            //Ścieżka do pliku z danymi wejściowymi
            var csvFilePath = args[0];
            //Ścieżka do folderu z danymi wyjściowymi
            var resultPath = args[1];
            //Ścieżka do pliku z logam
            var logFilePath = args[2];
            //Format pliku z danymi wyjściowymi
            var formatOfResultFile = args[3];
            
            //Wyjątek, który obsługuje błędnie podaną ścieżkę do pliku z danymi wejściowymi
            if (!File.Exists(csvFilePath))
            {
                throw new FileNotFoundException("Specified file does not exist", csvFilePath);
            }
            
            //Wyjątek, który obsługuje błędnie podaną ścieżkę do folderu z danymi wyjściowymi
            if (!Directory.Exists(resultPath))
            {
                throw new DirectoryNotFoundException("Specified directory does not exist");
            }
            
            //Wyjątek, który obsługuje błędnie podaną ścieżkę do pliku z logami
            if (!File.Exists(logFilePath))
            {
                throw new FileNotFoundException("Specified file does not exist", logFilePath);
            }
            
            //Wyjątek, który obsługuje błędnie podany format pliku z danymi wyjściowymi, czyli inny niż "json"
            if (formatOfResultFile != "json")
            {
                throw new InvalidOperationException("Specified format is not valid");
            }
            
            //Każdy wiersz reprezentuje pojedynczego studenta
            //Wczytywanie danych z pliku csv po linijce
            var lines = File.ReadLines(csvFilePath);
            
            //Lista studentów
            var students = new List<Student>();
            
            //Zapis logów błędów do pliku
            var errorsLogs = File.AppendText(logFilePath);
            errorsLogs.AutoFlush = true;
            
            //tworzenie nowego słownika
            var map = new Dictionary<string, int>();
            
            foreach(var line in lines)
            { 
                //Każda kolumna jest oddzielona znakiem ","
                var separatedComma = line.Split(",");
            
                //Każdy student powinien być opisywany przez 9 kolumn        
                if (separatedComma.Length != 9)
                {
                    //Zapis logów błędów do pliku
                    Console.WriteLine(line);
                    errorsLogs.WriteLine($"Wiersz nie posiada odpowiedniej ilości kolumn: {line}.");
                    continue; //wiersz nie zostanie dodany do listy studentów
                }
                
                //Jeśli jeden wiersz z danymi posiada w kolumnie pustą wartość -
                //traktujemy taką wartość jako brakującą.
                //W takim wypadku nie dodajemy studenta do zbioru wynikowego i zapisujemy następującą do pliku logów
                if (separatedComma.Any(line =>
                        line.Trim() == string.Empty))
                {
                    //Zapis logów błędów do pliku
                    errorsLogs.WriteLine($"Wiersz nie może posiadać pustych kolumn: {line}");
                    continue; //wiersz nie zostanie dodany do listy studentów
                }
                
                var studies = new Studies()
                {
                    Name = separatedComma[2],
                    Mode = separatedComma[3]
                };
                
                var student = new Student
                {
                    Fname = separatedComma[0],
                    Lname = separatedComma[1],
                    Studies = studies,   
                    IndexNumber = "s" + separatedComma[4],
                    Birthdate = DateTime.Parse(separatedComma[5]).ToString("dd.MM.yyyy"),
                    Email = separatedComma[6],
                    MothersName = separatedComma[7],
                    FathersName = separatedComma[8],
                };
                
                //Logowanie duplikatów imienia, nazwiska i numeru indeksu
                if (students.Any(s =>
                        student.Fname == s.Fname &&
                        student.Lname == s.Lname &&
                        student.IndexNumber == s.IndexNumber))
                {
                   errorsLogs.WriteLine($"Duplikat: {line}");
                   continue; //dulikat nie zostanie dodawany ponownie do listy studentów
                }
                
                //Zapis info o ilości studentów na danym kierunku co pliku
                map[studies.Name] = map.ContainsKey(studies.Name) ? ++map[studies.Name] : 1;
                            
                //dodać studenta do listy studentów
                students.Add(student);
            }
            
            var json = JsonSerializer.Serialize(
                new {
                    Uczelnia = new Uczelnia()
                    {
                        CreatedAt = DateTime.Now.ToString("dd.MM.yyyy"),
                        Author = "Katarzyna Kowalska",
                        Students = students,
                        ActiveStudies = map.Select(kvp => new ActiveStudies()
                        {
                            Name = kvp.Key,
                            NumberOfStudents = kvp.Value
                        })
                    }
                },
                
            new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }
        );  
            
            File.WriteAllText(Path.Combine(resultPath, "uczelnia." + formatOfResultFile), json);
        }
    }
}