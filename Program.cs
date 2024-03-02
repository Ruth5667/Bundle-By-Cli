using System.Collections;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Reflection.Metadata;
using System.Text;
using static System.Net.Mime.MediaTypeNames;
using static System.Net.WebRequestMethods;
using File = System.IO.File;
class Program
{
    private static Dictionary<string, List<string>> programmingLanguages;
    static void initProgrammingLanguages()
    {
        programmingLanguages = new Dictionary<string, List<string>>();
        programmingLanguages.Add("C#", new List<string> { "*.cs" });
        programmingLanguages.Add("Java", new List<string> { "*.java" });
        programmingLanguages.Add("Python", new List<string> { "*.py" });
        programmingLanguages.Add("C++", new List<string> { "*.cpp", "*.h" });
        programmingLanguages.Add("Html", new List<string> { "*.html","*.css" });
        programmingLanguages.Add("JavaScript", new List<string> { "*.js" });
        programmingLanguages.Add("all", new List<string> { "*.*" });
    }
    static void Main(string[] args)
    {
        initProgrammingLanguages();
         var rootCommand = new RootCommand("Root command for File Bundler CLI");
        var outputOption = new Option<FileInfo>("--output", "File path and name") { IsRequired = true, };
        outputOption.AddAlias("-o");
        var languageOption = new Option<string>("--language", "Programming language to bundle files") { IsRequired = true, };
        languageOption.AddAlias("-l");
        var author = new Option<string>("--author", "Name of the author");
        author.AddAlias("-a");
        var noteOfDescription = new Option<bool>("--note", "Add name and path of the bundle");
        noteOfDescription.AddAlias("-n");
        var removeEmptyLines = new Option<bool>("--remove", "remove empty lines from the file");
        removeEmptyLines.AddAlias("-r");
        var sort = new Option<bool>("--sort", "sort the files according to code type (deafult alphabeticOrder)");
        sort.AddAlias("-s");
        var bundleCommand = new Command("bundle", "Bundle code files to a single file");
        bundleCommand.AddOption(outputOption);
        bundleCommand.AddOption(languageOption);
        bundleCommand.AddOption(noteOfDescription);
        bundleCommand.AddOption(removeEmptyLines);
        bundleCommand.AddOption(author);
        bundleCommand.AddOption(sort);
        bundleCommand.SetHandler((output, language, note, removeEmptyLines, author, sort) =>
        {
            BundleFilesByLanguage(output.FullName, language, note, removeEmptyLines, author, sort);
        }, outputOption, languageOption, noteOfDescription, removeEmptyLines, author, sort);
        rootCommand.AddCommand(bundleCommand);
        var createRspCommand = CreateRspCommand(noteOfDescription, removeEmptyLines, author, sort);
        rootCommand.AddCommand(createRspCommand);
        rootCommand.Invoke(args);
    }
    static Command CreateRspCommand( Option<bool> noteOfDescription, Option<bool> removeEmptyLines, Option<string> author, Option<bool> sort)
    {
        var createRspCommand = new Command("create-rsp", "Create a response file for bundling code");
        var outputOption = new Option<FileInfo>("--output", "File path and name");
        var languageOption = new Option<string>("--language", "Programming language to bundle files");
        createRspCommand.AddOption(outputOption);
        createRspCommand.AddOption(languageOption);
        createRspCommand.AddOption(noteOfDescription);
        createRspCommand.AddOption(removeEmptyLines);
        createRspCommand.AddOption(author);
        createRspCommand.AddOption(sort);
        createRspCommand.SetHandler((output, language, author, remove, sort, note) =>
        {
            while(output == null)
            {
                Console.WriteLine("Enter the file path and name:");
                string filePath = Console.ReadLine();
                output = new FileInfo(filePath);
            }
            while(language == null)
            {
                Console.Write("Enter language to bundle (or 'all' to all the language): ");
                language = Console.ReadLine();
            }
            if (note == false)
            {
                Console.Write("Enter true/false if you want note: ");
                note = bool.Parse(Console.ReadLine());
            }
            if (String.IsNullOrEmpty(author))
            {
                Console.Write("Enter name of the author: ");
                author = Console.ReadLine();
            }
            if (remove == false)
            {
                Console.Write("Enter true or false to remove: ");
                remove = bool.Parse(Console.ReadLine());
            }
            if (sort == false)
            {
                Console.Write("Enter true if you want sort by type of the code: ");
                sort = bool.Parse(Console.ReadLine());
            }
            var rspContent =$"bundle " + $"--output {output}" + Environment.NewLine +
                 $"--language {language}" + Environment.NewLine +
                 $"--note {note}" + Environment.NewLine +
                 $"--remove {remove}" + Environment.NewLine +
                 $"--sort {sort}" + Environment.NewLine +
                 $"--author {author}" + Environment.NewLine;
            var rspFileName = "response.rsp";
            File.WriteAllText(rspFileName, rspContent);
            Console.WriteLine($"Response file '{rspFileName}' created successfully.");
            Console.Write("use the follow command to:");
            Console.WriteLine($" cli @response.rsp");

        }, outputOption, languageOption, author, removeEmptyLines, sort, noteOfDescription);
        return createRspCommand;
    }
    static void BundleFilesByLanguage(string outputPath, string language, bool note, bool removeEmptyLines, string author, bool sort)
    {
        string text;
        List<string> searchPatterns = GetSearchPatternsByLanguage(language);
        var files = searchPatterns
                    .SelectMany(pattern => Directory.GetFiles(Environment.CurrentDirectory, pattern))
                    .ToArray();
        if (sort)
            SortTheFileByTypeOfCode(files);
        else
            files = SortTheFileAlph(files);
        //string[] files = Directory.GetFiles(Environment.CurrentDirectory,language);
        string outputFilePath = Path.Combine(outputPath);
        List<string> allTextFiles = new List<string>();
        if (!string.IsNullOrEmpty(author))
            allTextFiles.Add($"//---------author: {author} -----------"); ;
        foreach (string file in files)
        {
            text = File.ReadAllText(file);
            if (note)
                allTextFiles.Add("//--------------" + file + "----------------");
            if (removeEmptyLines)
            {
                text = string.Join(Environment.NewLine, text.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries));
            }
            allTextFiles.Add(text);
        }
        File.WriteAllLines(outputFilePath, allTextFiles);
        Console.WriteLine($"the files of {language} language budled successfully into {outputFilePath}");
    }
    static List<string> GetSearchPatternsByLanguage(string language)
    {
        if (programmingLanguages.ContainsKey(language))
            // Retrieve the list of values for the specified key
            return programmingLanguages[language];
        else throw new Exception($"there is no {language} language supported");
    }
    static string[] SortTheFileAlph(string[] files)
    {
        return files = files.OrderBy(file => file).ToArray();
    }
    static void SortTheFileByTypeOfCode(string[] files)
    {
        Array.Sort(files, (a, b) => Path.GetExtension(a).CompareTo(Path.GetExtension(b)));
    }
}

