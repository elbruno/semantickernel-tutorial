using Microsoft.SemanticKernel;
using System;

public class ConsoleHelper
{
    public static void PromptAndResponse(KernelArguments args, string message) { 
        Prompt(args);
        Response(message);
    }

    public static void Prompt(KernelArguments args)
    {
        string message = "";

        // concatenate args into the message
        foreach (KeyValuePair<string, object?> arg in args)
        {
            message += $"{arg.Key} = {arg.Value}\n";
        }
        var title = " Prompt Arguments                     ";
        Prompt(message, title);
    }

    public static void Prompt(string message, string title = " Question                             ")
    {
        Console.BackgroundColor = ConsoleColor.White;
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine("======================================");
        Console.WriteLine(title);
        Console.WriteLine("======================================");
        Console.ResetColor();
        Console.WriteLine();

        // iterate throught the message lines, complete each line with spaces to the right up to 30 characters and print the line
        foreach (string line in message.Split('\n'))
        {
            Console.WriteLine(line.PadRight(30));
        }
        Console.ResetColor();
        Console.WriteLine();
    }

    public static void Response(string message)
    {        
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Response:");
        Console.ResetColor();
        Console.WriteLine(message);
        Console.WriteLine();
    }

    public static void Warning(string message)
    {
        Console.BackgroundColor = ConsoleColor.Red;
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine(message);
        Console.ResetColor();
    }
}
