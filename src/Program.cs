using System.Globalization;
using MoneyBurned.Dotnet.Lib;
using MoneyBurned.Dotnet.Lib.Data;
using TRoschinsky.Common;

namespace MoneyBurned.Dotnet.Cli;

/// <summary>
/// A static single class application, generating a CLI for money burned
/// </summary>
internal class Program
{
    private static bool resComplete = false;
    private static bool nicePrint = false;
    private static bool directRun = false;
    private static int redraws = 0;
    private static readonly List<Resource> resources = [];
    private static Job? job;
    private static string? jobName;

    /// <summary>
    /// Main method that processes the arguments and executes the most important application methods in the correct order
    /// </summary>
    /// <param name="args">Parameters separated by spaces; see PrintManual() method for more information</param>
    private static void Main(string[] args)
    {
        try
        {
            if (args != null && args.Length > 0)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    switch (args[i].Trim().ToLower())
                    {
                        case "-j":
                        case "--job-name":
                            jobName = args[i + 1];
                            break;
                        case "-r":
                        case "--resources":
                            ReadResources(args[i + 1]);
                            break;
                        case "-n":
                        case "--nice":
                            nicePrint = true;
                            break;
                        case "-d":
                        case "--disable-direct-run":
                            directRun = true;
                            break;
                        case "-c":
                        case "--cost-types":
                            PrintCostTypes();
                            break;
                        case "-?":
                        case "-h":
                        case "--help":
                            PrintManual();
                            break;
                    }
                }

                // if resources are added already, switch to direct mode, except 'disable-direct-run' was selected
                if (resources.Count > 0) { directRun = !directRun; }
            }

            if (!directRun)
            {
                DrawScreen();
            }
            RunJob();
        }
        catch (IndexOutOfRangeException ex)
        {
            Console.WriteLine("Something seems to be wrong with your command line parameters. Please check carefully: {0}", ex.Message);
            Environment.Exit(2);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Whoops, something went not as expected: {0}.", ex.Message);
            Environment.Exit(1);
        }
    }

    /// <summary>
    /// Parser for a string based list of resources, that will be added as resources for processing
    /// </summary>
    /// <param name="resourceString"></param>
    private static void ReadResources(string resourceString)
    {
        try
        {
            string[] resourceStringArray = resourceString.Split([';', '+'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (resourceStringArray != null && resourceStringArray.Length > 0)
            {
                for (int i = 0; i < resourceStringArray.Length; i++)
                {
                    string[] resource = resourceStringArray[i].Split(":", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    if (resource.Length == 2)
                    {
                        resources.Add(new Resource(resource[0], new Cost(resource[1])));
                    }
                    else
                    {
                        resources.Add(new Resource("Generic", new Cost(resource[0])));
                    }
                }
            }
        }
        catch (System.Exception ex)
        {
            Console.WriteLine("Resources from command line input are not valid because of {0}. Good bye.", ex.Message);
        }
    }

    /// <summary>
    /// Starts the processing of the time/money recording
    /// </summary>
    public static void RunJob()
    {
        if (resources == null || resources.Count == 0)
        {
            Console.WriteLine("Without any resources, there is nothing to calculate. Good bye.");
            return;
        }

        Console.CancelKeyPress += (sender, e) =>
        {
            e.Cancel = true;
            job!.EndRecording();
            Console.WriteLine();
            return;
        };

        var posTop = Console.CursorTop;
        var posLeft = Console.CursorLeft;
        AsciiText fancyFont;

        job = new Job([.. resources]);
        job.Name = string.IsNullOrWhiteSpace(jobName) ? $"Job_{DateTime.Now:yyMMdd_HHmmss}" : jobName;
        if (!directRun)
        {
            fancyFont = new AsciiText(1, 0);
            Console.Write("Press Return to start or Ctrl+C to abort...");
            Console.Read();
        }
        else
        {
            fancyFont = new AsciiText();
        }

        job.StartRecording();
        if (nicePrint) { Console.SetCursorPosition(posLeft, posTop); }
        Console.WriteLine("Recording - press Return or Ctrl+C to stop recording...    ");
        int i = 1;
        int lastKey = 0;
        do
        {
            if (directRun)
            {
                fancyFont.TextInput = $"{job.ElapsedCost:C2}";
                CenterText($"{fancyFont}");
                Thread.Sleep(1000);
            }
            else if (nicePrint)
            {
                char pm = (char)177;
                Console.SetCursorPosition(posLeft, posTop + 1);
                if (i % 2 == 0) { fancyFont.TextInput = $" + {job.ElapsedCost:C2}"; }
                else { fancyFont.TextInput = $" {pm} {job.ElapsedCost:C2}"; }
                Console.WriteLine($"{fancyFont}");
                Thread.Sleep(500);
            }
            else
            {
                Console.Write("{0:C2}.. ", job.ElapsedCost);
                Thread.Sleep(1000);
            }
            if (Console.KeyAvailable) { lastKey = Console.Read(); }
            i++;
        } while (lastKey != 13 && !(job.EndTime != DateTime.MaxValue));

        job.EndRecording();
        SummarizeJob();
    }

    #region UI support

    /// <summary>
    /// Drawing the main screen of the applications user interface
    /// </summary>
    private static void DrawScreen()
    {
        if (nicePrint) { Console.Clear(); }
        PrintLogo();
        Console.WriteLine("--------------------------------------------------------------------");
        ListResources();
        redraws++;
    }

    /// <summary>
    /// Prints all enlisted resources and handles the interactive dialog to add additional resources
    /// </summary>
    private static void ListResources()
    {
        if (nicePrint || redraws == 0)
        {
            Console.WriteLine("Resources...");
            if (resources != null && resources.Count > 0)
            {
                foreach (Resource resource in resources)
                {
                    Console.WriteLine($"  - {resource}");
                }
            }
            else
            {
                Console.WriteLine("  (No resources entered... If you need help, use the command line option -h.)");
            }
            Console.WriteLine();
        }

        while (!resComplete)
        {
            Console.Write("Want to add a resource? [Y/n]: ");
            string? yesNo = Console.ReadLine();
            Console.WriteLine();
            if (yesNo != null && (yesNo.ToLower() == "y" || yesNo == String.Empty))
            {
                try
                {
                    Console.Write("Enter a resource name: ");
                    string? resName = Console.ReadLine();
                    Console.Write("Enter a resource cost: ");
                    string? resCost = Console.ReadLine();
                    Console.WriteLine();
                    if (resName != null && resCost != null)
                    {
                        resources?.Add(new Resource(String.IsNullOrWhiteSpace(resName) ? "Generic" : resName, new Cost(resCost)));
                        if (!nicePrint) { Console.WriteLine("{0} was added...", resources?.Last()); }
                    }
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine("Resource couldn't be added, because of: {0}", ex.Message);
                    Thread.Sleep(2000);
                }
            }
            else if (yesNo != null && yesNo.ToLower() == "n")
            {
                resComplete = true;
            }
            if (nicePrint) { DrawScreen(); }
        }
    }

    /// <summary>
    /// Summarize job recording
    /// </summary>
    private static void SummarizeJob()
    {
        if (nicePrint || directRun) { Console.Clear(); }
        if (nicePrint) { PrintLogo(); }
        if (!directRun) { Console.WriteLine("--------------------------------------------------------------------"); }
        if (job != null) { Console.WriteLine(job); }
    }

    /// <summary>
    /// Prints full screen centered text to console
    /// </summary>
    /// <param name="centerText">Text to display; can be multiple lines</param>
    /// <param name="leftOffset">Left offset</param>
    /// <param name="topOffset">Top offset</param>
    private static void CenterText(string centerText, int leftOffset = 0, int topOffset = 0)
    {
        if (string.IsNullOrEmpty(centerText))
        {
            return;
        }

        int middleWidth;
        int middleHeight;

        try
        {
            centerText = centerText.ReplaceLineEndings();
            string[] lines = centerText.Split([Environment.NewLine], StringSplitOptions.None);
            int horizontalMiddle = 0;
            foreach (string line in lines)
            {
                horizontalMiddle = line.Length > horizontalMiddle ? Convert.ToInt32(line.Length / 2) : horizontalMiddle;
            }

            middleWidth = Convert.ToInt32(Console.WindowWidth / 2) - horizontalMiddle + leftOffset;
            middleHeight = Convert.ToInt32(Console.WindowHeight / 2) + topOffset - Convert.ToInt32(lines.Length / 2);
            Console.Clear();
            for (int i = 0; i < lines.Length; i++)
            {
                Console.SetCursorPosition(middleWidth, middleHeight);
                Console.Write(lines[i]);
                middleHeight++;
            }
        }
        catch (ArgumentOutOfRangeException)
        {
            middleWidth = Convert.ToInt32(Console.WindowWidth / 2) - 13;
            middleHeight = Convert.ToInt32(Console.WindowHeight / 2) - 1;
            Console.SetCursorPosition(middleWidth, middleHeight);
            Console.Write("# !Output Area to small! #");
            Console.SetCursorPosition(middleWidth, middleHeight + 1);
            Console.Write("# Please enlarge console #");
        }
        catch (Exception ex)
        {
            Console.Write($"Writing output failed with {ex.GetType().Name}: {ex.Message}");
        }
    }

    /// <summary>
    /// Prints the applications logo
    /// </summary>
    private static void PrintLogo()
    {
        if (nicePrint || redraws == 0)
        {
            string logo = @"   __  ___                       ___                           __
  /  |/  /___   ___  ___  __ __ / _ ) __ __ ____ ___  ___  ___/ /
 / /|_/ // _ \ / _ \/ -_)/ // // _  |/ // // __// _ \/ -_)/ _  / 
/_/  /_/ \___//_//_/\__/ \_, //____/ \_,_//_/  /_//_/\__/ \_,_/  
                        /___/  https://github.com/Money-Burned";
            Console.WriteLine(logo);
        }
    }

    /// <summary>
    /// Prints the manual, explaining the usage of the CLI application
    /// </summary>
    private static void PrintManual()
    {
        PrintLogo();
        string helpInfo = @"
Description:
  Is a simple tool that makes the loss of time and money through knowledge of the resources used visible.
  For more information visit https://github.com/Money-Burned/mb-dotnet-console

Usage:
  MoneyBurned.Cli [options]

Options:
  -n <name>, --job-name <name>     Give it a descriptive name if you wish - it's just for convenience.
  -r <resource string>,            Starts the tool including a set of resources, given as string. 
  --resources <resource string>    A resource string is separated by a semicolon or plus sign for 
                                   cost. If you need to assign names, use a colon as an additional 
                                   each resource separator before the cost value. 
                                   (e. g. for 3 resources: ""24,99;Manager:89;11"")
                                   You are allowed to use common interval types to specify costs  
                                   scoped not only to hourly bases (e. g. MD = man days, d = days). 
                                   **BE AWARE** If the resources have been processed successfully and 
                                   you have at least one resource defined, the job will start immediately 
                                   in full-screen mode with centered output!
  -c, --cost-types                 Lists all available cost interval types and a few sample resource strings.
  -d, --disable-direct-run         Prevent immediately jobe execution, even if resources are configured
                                   to have the opportunity to add more resources in interactive mode.
  -n, --nice                       Enables a more interactive and nice looking user experience.
  -?, -h, --help                   Show help and usage information.
";
        Console.WriteLine(helpInfo);
        Console.WriteLine("Current Culture:                   {0} (UI: {1})", CultureInfo.CurrentCulture, CultureInfo.CurrentUICulture);
        Environment.Exit(0);
    }

    private static void PrintCostTypes()
    {
        PrintLogo();
        string costTypesInfo = @"
To deviate from the standard time scope (hours) for a cost, use a slash, 'per' or 'à' after 
the value. Then use the codes for the respective interval type.
        
Standard Cost Types:
    - Minute: m, min
    - Day: d, day, Tag
    - Week: w, wk, Week, Woche
    - Month: mth, month, mo, mon, Monat
    - Year: y, yr, Year, j, Jahr

Labor-based Cost Types:
    - Work Day: MD (man day), PD, PT, Arbeitstag [default: 8h/d]
    - Work Week: ww, wwk, MW, PW, AW  [default: 8h * 5d /w]
    - Work Month: wmth, workmonth, MM, PM, Arbeitsmonat [8h * 5d * 4.35w / m]
    - Work Year: wy, MY, PY, AJ, PJ [8h * 5d * 4.35w * 12 / y]

To decide whether it is a resource based on labor-related or total costs, consider 
whether or not the resource would be available to you for the 24-hour period per day.

Example:
    MoneyBurned.Cli -r ""Consultant:1100 per MD; Rental:92/d; Dev:63500/wy; Junior-Dev:55200/wy; Co-Working-Space:35""
       - Resource Consultant at $137,50/h
       - Resource Rental at $3,83/h
       - Resource Dev at $30,41/h
       - Resource Junior-Dev at $26,44/h
       - Resource Co-Working-Space at $35,00/h
    ";
        Console.WriteLine(costTypesInfo);
        Environment.Exit(0);
    }

    #endregion
}
