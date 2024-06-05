using System.CommandLine;
using System.Text;

namespace prj
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Define command line options
            var bundleOption = new Option<FileInfo>("--output", "file path and name");
            bundleOption.AddAlias("--otp");
            var bundleOptionList = new Option<string>("--language", "list of languages") { IsRequired = true };
            bundleOptionList.AddAlias("--lng");
            var bundleNote = new Option<bool>("--note", "show the note of the file");
            bundleNote.AddAlias("--nt");
            var bundleSort = new Option<bool>("--sort", "Sort by code");
            bundleSort.AddAlias("--srt");
            var bundleLineEmpy = new Option<bool>("--remove-empty-lines", "delete empty lines");
            bundleLineEmpy.AddAlias("--rel");
            var bundleNameY = new Option<string>("--author", "enter your name");
            bundleNameY.AddAlias("--n");

            // Define commands
            var bundleCommand = new Command("bundle", "bundle code files into a single file");
            var rsp = new Command("create-rsp", "create a new rsp file");
            var rootCommand = new RootCommand("root command for file bundler CLI");

            // Add options to commands
            bundleCommand.AddOption(bundleOption);
            bundleCommand.AddOption(bundleOptionList);
            bundleCommand.AddOption(bundleSort);
            bundleCommand.AddOption(bundleNote);
            bundleCommand.AddOption(bundleLineEmpy);
            bundleCommand.AddOption(bundleNameY);

            // Handler for creating an rsp file
            rsp.SetHandler(() =>
            {
                // Prompt user for inputs
                string nameRsp, otp, lng, nt, srt, rel, n;
                Console.WriteLine("enter name for file rsp");
                nameRsp = Console.ReadLine();
                Console.WriteLine("enter name for your new file");
                otp = Console.ReadLine();
                while (string.IsNullOrEmpty(otp))
                {
                    Console.WriteLine("you didn't enter otp:");
                    otp = Console.ReadLine();
                }
                Console.WriteLine("enter language you want (if you want many, enter with ',')");
                lng = Console.ReadLine();
                while (string.IsNullOrEmpty(lng))
                {
                    Console.WriteLine("is required - enter the language:");
                    lng = Console.ReadLine();
                }
                Console.WriteLine("you want note (MKOR KODE) enter true or false (or nothing :) )");
                nt = Console.ReadLine();
                Console.WriteLine("you want to sort by source code enter true or false (or nothing :) )");
                srt = Console.ReadLine();
                Console.WriteLine("you want to remove empty lines enter true or false (or nothing :) )");
                rel = Console.ReadLine();
                Console.WriteLine("enter your author name if you want...");
                n = Console.ReadLine();

                try
                {
                    // Create the rsp file and write the gathered options
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), nameRsp + ".rsp");
                    using (File.Create(filePath)) { }
                    var mell = new StringBuilder();
                    mell.AppendLine($"bundle --otp {otp}");
                    mell.AppendLine($"--lng {lng}");
                    if (!string.IsNullOrEmpty(nt)) mell.AppendLine($"--nt {nt}");
                    if (!string.IsNullOrEmpty(srt)) mell.AppendLine($"--srt {srt}");
                    if (!string.IsNullOrEmpty(rel)) mell.AppendLine($"--rel {rel}");
                    if (!string.IsNullOrEmpty(n)) mell.AppendLine($"--n {n}");
                    File.WriteAllText(filePath, mell.ToString());
                }
                catch (System.IO.DirectoryNotFoundException)
                {
                    Console.WriteLine("file directory is invalid");
                }
            });

            // Handler for the bundle command
            bundleCommand.SetHandler((output, language, note, sort, rel, n) =>
            {
                string newFile = Path.IsPathRooted(output.FullName) ? output.FullName : Path.GetDirectoryName(output.FullName);
                try
                {
                    using (FileStream fs = File.Create(newFile))
                    {
                        Console.WriteLine("File was created");
                    }
                }
                catch (IOException ex)
                {
                    Console.WriteLine($"Error creating file: {ex.Message}");
                }
                catch (UnauthorizedAccessException ex)
                {
                    Console.WriteLine($"Access denied: {ex.Message}");
                }

                // Gather and process files based on options
                string[] languages = language.Split(',');
                string folder = Directory.GetCurrentDirectory();
                string[] files = Directory.GetFiles(folder);
                string copy = "";

                if (!string.IsNullOrEmpty(n))
                {
                    copy += n;
                }

                // Sort files if specified
                if (sort)
                {
                    ProcessFiles(languages, files, note, ref copy);
                }
                else
                {
                    Array.Sort(files);
                    ProcessFiles(languages, files, note, ref copy);
                }

                // Write the processed content to the new file
                System.IO.File.WriteAllText(newFile, copy);

                // Remove empty lines if specified
                if (rel)
                {
                    string[] lines = File.ReadAllLines(newFile);
                    lines = lines.Where(line => !string.IsNullOrEmpty(line.Trim())).ToArray();
                    File.WriteAllLines(newFile, lines);
                }
            }, bundleOption, bundleOptionList, bundleNote, bundleSort, bundleLineEmpy, bundleNameY);

            // Add commands to root command
            rootCommand.AddCommand(bundleCommand);
            rootCommand.AddCommand(rsp);

            // Invoke root command
            rootCommand.InvokeAsync(args);
        }

        // Process files based on language and note options
        static void ProcessFiles(string[] languages, string[] files, bool note, ref string copy)
        {
            foreach (var lang in languages)
            {
                string lanGood = $".{lang.Trim()}";
                copy += $"\n\n\nthe language is {lanGood}\n";
                foreach (var file in files)
                {
                    if (Path.GetExtension(file) == lanGood)
                    {
                        if (note) copy += $"\n\n/*the note is {file}*/\n\n";
                        copy += $"\n\nthe file is {Path.GetFileName(file)}\n\n";
                        copy += System.IO.File.ReadAllText(file);
                    }
                }
            }
        }
    }
}
