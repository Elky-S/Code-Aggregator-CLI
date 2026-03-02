using System.CommandLine;
using System.Reflection.Metadata;


//מה מקבל הקובץ המיוצא ואיפה ממוקם
var bunndleOption = new Option<FileInfo>(
    aliases: new[] { "--output", "-o" },
    description: "File name or full path. Note: If the path contains spaces, wrap it in double quotes (e.g., \\\"C:\\\\My Folder\\\\out.txt\\\").\"");
// רשימת השפות הנתמכות והסיומות שלהן
var supportedLanguages = new Dictionary<string, string>
{
    { "c#", ".cs" },
    { "js", ".js" },
    { "python", ".py" },
    { "html", ".html" },
    { "css", ".css" },
    { "csharp",  ".sln" },
    { "Angular",".ts" },
    { "react",".jsx" },
    { "sql",".sql" },
    { "java",".java" },
    {"text",".txt" }
};
// 1. הגדרת האופציה לשפות
// השתמשתי ב-string מכיוון שהמשתמש יכול להקיש רשימת שפות או את המילה "all"
var languageOption = new Option<string[]>(
    //קיצור כתיבה
    aliases: new[] { "--language", "-l" },
    description: "List of programming languages (e.g., cs, js) or 'all' to include everything")
{
    IsRequired = true // הגדרה כאופציה חובה
};
//בדיקות שהקלט- סיומת הקובץ שהמשתמש הזין- תקין
languageOption.AddValidator(result => {
    var values = result.GetValueOrDefault<string[]>();
    if (values != null && !values.Contains("all"))
    {
        foreach (var lang in values)
        {
            if (!supportedLanguages.ContainsKey(lang.ToLower()))
            {
                //החזרת השגיאה שמונעת מהפקודה לרוץ
                result.ErrorMessage = $"Error: The language '{lang}' is not supported. " +
                                     $"Please choose from: {string.Join(", ", supportedLanguages.Keys)} or use 'all'.";
            }
        }
    }
});
//אפשרות למשתמש להכניס תיעוד לקוד
var noteOption = new Option<bool>(
    aliases: new[] { "--note", "-n" },
    description: "Whether to add a comment with the source file name and path in the bundle file");


//מיון הקבצים
var sortOption = new Option<string>(
    aliases: new[] { "--sort", "-s" },
    description: "The order of copying files: 'name' (alphabetical) or 'type' (by extension). Default is 'name'.")
{
    // זה מה שיופיע בתוך הסוגריים המשולשים ב-help
    ArgumentHelpName = "sort order"
};

// הגבלת האפשרויות 
sortOption.FromAmong("name", "type");

// קביעת ברירת מחדל  אם לא יקישו, יבחר "name"
sortOption.SetDefaultValue("name");
//הסרת שורות רווח
var removeEmptyLinesOption = new Option<bool>(
    aliases: new[] { "--remove-empty-lines", "-r" },
    description: "Remove empty lines from the source code");
//bundle-שם יוצר הקובץ בראש ה
var authorOption = new Option<string>(
    aliases: new[] { "--author", "-a" },
    description: "Name of the bundle creator to be written at the top of the file");
//command הוספה של הפקודות ל
var bundleCommand = new Command("bandle", "Merge code files to a single file");
bundleCommand.AddOption(bunndleOption);
bundleCommand.AddOption(languageOption);
bundleCommand.AddOption(noteOption);
bundleCommand.AddOption(sortOption);
bundleCommand.AddOption(removeEmptyLinesOption);
bundleCommand.AddOption(authorOption);
//יצירת פקודת השורש
var rootCommand = new RootCommand("root command for bandle");

//פונקציה שתופעל כאשר הפקודה תרוץ
//הגדרת פקודה זו באמצעות משתנה סביבה שתוכל לרוץ מכל מקום במחשב
//לשמירת השינויים publish בכל שינוי בפרויקט נעשה 

//יקבל פונקציה בתור פרמטר SetHandler
//של שם ומיקום ושפה- סיומת קובץ option וגם את 
bundleCommand.SetHandler(async (output, language, note, sort, removeEmptyLines, author) =>
{
    try
    {
        // --- שלב 1: זיהוי המיקום הנוכחי ושליפת כל הקבצים ---
        // Directory.GetCurrentDirectory() מחזיר את הנתיב שבו המשתמש נמצא בטרמינל
        var currentDirectory = Directory.GetCurrentDirectory();

        // (שליפת שמות כל הקבצים שנמצאים בתיקייה הזו (ללא תיקיות משנה
        var allFiles = Directory.GetFiles(currentDirectory);

        // --- שלב 2: סינון הקבצים לפי בחירת המשתמש ---
        var filteredFiles = allFiles.Where(file =>
        {
            // אם המשתמש הקיש "all"
            if (language.Contains("all"))
            {
                return true;
            }

            // חילוץ הסיומת של הקובץ הנוכחי (למשל .cs) והפיכה לאותיות קטנות למניעת טעויות
            var extension = Path.GetExtension(file).ToLower();

            // בדיקה האם לפחות אחת מהשפות שהמשתמש ביקש קיימת במילון שלנו והסיומת שלה מתאימה
            return language.Any(lang =>
                supportedLanguages.ContainsKey(lang.ToLower()) &&
                supportedLanguages[lang.ToLower()] == extension);
        }).ToList(); // המרה לרשימה כדי שנוכל לבדוק כמה קבצים נמצאו

        // --- שלב 3: בדיקת תוצאות הסינון ---
        if (filteredFiles.Count == 0)
        {
            // אם אחרי הסינון לא נשאר אף קובץ, אין טעם להמשיך ליצירת ה-output
            Console.WriteLine("No files found that match your criteria.");
            return;
        }

        // --- שלב 4: טיפול בקובץ הפלט (Output) ---
        //  בודק האם צריך ניתוב או חסר שם הקובץ שרוצים ליצור 
        if (output == null)
        {
            Console.WriteLine("Error: Please provide a file name or a full path using --output.");
            return;
        }
        // --- שלב המיון  ---
        // אנחנו בודקים מה המשתמש בחר באופציה 'sort'
        if (sort == "type")
        {
            // קודם ממיין לפי סיומת (css לפני html) 
            // ואז בתוך כל סיומת - לפי שם הקובץ (A.css לפני B.css)
            filteredFiles = filteredFiles
                .OrderBy(f => Path.GetExtension(f))
                .ThenBy(f => Path.GetFileName(f))
                .ToList();
        }
        else
        {
            // ברירת מחדל: ממיינים לפי שם הקובץ (א"ב)
            filteredFiles = filteredFiles.OrderBy(f => Path.GetFileName(f)).ToList();
        }
        // שלב החיבור: 
        // השורה הזו מנסה ליצור את הקובץ. 
        // אם הנתיב מלא ותקין - הוא ייוצר שם.
        // אם ניתן רק שם - הוא ייוצר בתיקייה הנוכחית 
        using (var writer = new StreamWriter(output.FullName))
            {
            // האם המשתמש הזין שם מחבר?
            if (!string.IsNullOrWhiteSpace(author))
            {
                //כתיבת השם בראש הקובץ
                writer.WriteLine($"// Author: {author}");
                writer.WriteLine(); // שורה ריקה ליופי אחרי הכותרת
            }
            foreach (var file in filteredFiles)
            {
                // בדיקה: האם המשתמש הקיש --note?
                if (note)
                {
                    // א. חישוב הניתוב היחסי (ביחס לתיקייה הנוכחית)
                    var relativePath = Path.GetRelativePath(currentDirectory, file);

                    // ב. כתיבת ההערה לתוך הקובץ (למשל עם // בתחילת השורה)
                    writer.WriteLine($"// Source file: {relativePath}");
                }
                var lines = File.ReadAllLines(file);

                foreach (var line in lines)
                {
                    // אם המשתמש סימן "להסיר שורות ריקות" והשורה הנוכחית אכן ריקה או רק עם רווחים
                    if (removeEmptyLines && string.IsNullOrWhiteSpace(line))
                    {
                        continue; // דלג לשורה הבאה, אל תכתוב אותה ל-writer
                    }

                    writer.WriteLine(line);
                }
                // הוספת שורה ריקה להפרדה יפה
                writer.WriteLine();

            }

        }
            Console.WriteLine($"Output path validated: {output.FullName}");
        
    }

        // --- שלב 5: ניהול שגיאות (Catch) ---
        // תפיסת שגיאה במקרה שהנתיב שהמשתמש הזין לא קיים (תיקייה שלא קיימת)
        catch (DirectoryNotFoundException)
    {
        Console.WriteLine("Error: The directory path is invalid. Check if the folders exist.");
    }
    // תפיסת שגיאה במקרה שאין הרשאות לכתוב במיקום שנבחר
    catch (UnauthorizedAccessException)
    {
        Console.WriteLine("Error: No permission to write to this location.");
    }
    // תפיסת כל שגיאה אחרת (קובץ בשימוש, דיסק מלא וכו')
    catch (Exception ex)
    {
        Console.WriteLine($"An unexpected error occurred: {ex.Message}");
    }

}, bunndleOption, languageOption, noteOption, sortOption, removeEmptyLinesOption, authorOption);

//הוספת הפקודה שניצור לשורש
rootCommand.AddCommand(bundleCommand);

//שבה המשתמש יוכל להזין את כל הפקודות create-rsp חדש command הגדרת 
//בצורה של שאלה תשובה ואח"כ הכל יארז לתוך פקודה שבה הכתיבת פקודה (עם @) כל הלמעלה ירוץ

//הגדרה
var rspCommand = new Command("create-rsp", "Create a response file for the bundle command");

rspCommand.SetHandler(() =>
{
    // משתנה שיצבור את כל חלקי הפקודה
    var commandLines = new List<string>();
    commandLines.Add("bandle"); // תמיד מתחילים בשם הפקודה

    // שאלת ה-Output ---שדה חובה---
    Console.WriteLine("--- Creating Response File ---");
    Console.Write("Enter output file name/path: ");
    var output = Console.ReadLine();
    while (string.IsNullOrWhiteSpace(output))
    {
        Console.Write("Output path is mandatory. Please enter a value: ");
        output = Console.ReadLine();
    }
    commandLines.Add($"--output \"{output}\"");

    // שאלת השפות
    Console.Write("Enter languages (e.g., 'cs, js, python' or 'all' to include everything): ");
    var langs = Console.ReadLine();
    // ודואגת שזה לא יהיה ריק
    while (string.IsNullOrWhiteSpace(langs))
    {
        Console.Write("This field is required. Enter languages or 'all': ");
        langs = Console.ReadLine();
    }
    // פתרון הבעיה: מחליפים פסיקים ברווחים ומפצלים למערך
    var languagesArray = langs.Replace(",", " ").Split(' ', StringSplitOptions.RemoveEmptyEntries);

    // מוסיפים כל שפה בנפרד לפקודה
    foreach (var lang in languagesArray)
    {
        commandLines.Add("--language");
        commandLines.Add(lang);
    }

    // שאלת ה-Note (כן/לא)
    Console.Write("Add source file comments? (y/n): ");
    if (Console.ReadLine()?.ToLower() == "y")
    {
        commandLines.Add("--note");
    }

    // שאלת ה-Sort ---אופציונלי---
    Console.Write("Sort by 'name' or 'type'? (press Enter for default 'name'): ");
    var sort = Console.ReadLine();
    if (!string.IsNullOrWhiteSpace(sort))
    {
        commandLines.Add($"--sort {sort}");
    }
    // שאלת ה-Author
    Console.Write("Enter author name (optional): ");
    var author = Console.ReadLine();
    if (!string.IsNullOrWhiteSpace(author)) commandLines.Add($"--author \"{author}\"");

    // שאלת הסרת שורות ריקות
    Console.Write("Remove empty lines? (y/n): ");
    if (Console.ReadLine()?.ToLower() == "y") commandLines.Add("--remove-empty-lines");

    // שמירה לקובץ
    try
    {
        // מחברים את כל הרשימה למחרוזת אחת עם רווחים
        string finalCommand = string.Join(" ", commandLines);
        File.WriteAllText("bundle.rsp", finalCommand);

        Console.WriteLine("\nSuccess! 'bundle.rsp' has been created.");
        Console.WriteLine("To use it, run: dotnet run -- @bundle.rsp");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
    }
    //כאשר יתבצע setHendler זה 
    //תתבצע בשורת הפקודה פעולה זו  dotnet run -- @bundle.rsp: 
});

//לשורש rspCommand הוספת הפקודה  
rootCommand.AddCommand(rspCommand);

//הוא לא זוהה - InvokeAsync ניתקלתי בשגיאה ב
//דרך שורת הפקודה לגרסה מתקדמת שזיההתה את הפונקציה system.commandLine לא זוהה, בסוף החלפתי גרסה ל  

//bandle לפני שמפעילה את פקודת ה args פונקציה זו מנתחת את הנתונים שהתקבלו ב  
await rootCommand.InvokeAsync(args);

//סדר הפעולות:
//publish לאחר ששמרנו את קובץ הפרויקט הרצתי את ה
//נכנסתי לתקיה שנוצרה העתקתי משם את הניתוב והדבקתי במשתני סביבה
//cli create-rsp :פתחתי תקייה סתם מהמחשב המכילה קבצי תכנות וההפעלתי בשורת הפקודה שלה את הפקודה
//...ואז המשתמש מכניס את כל הדרישות שלו לפעולה כגון שם כותרת רווח בין השורות מיון
//שמבצעת את החיבור עם כל ההגדרות שהגדרנו cli @bundle.rsp משלה ניתן להריץ בה את הפקודה הקצרה bundle  לאחר שהתקייה זו נוצר קובץ   


