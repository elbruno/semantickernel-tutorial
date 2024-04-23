using System.ComponentModel;
using Microsoft.SemanticKernel;


namespace sk_tutorial_10;

public class EmailPlugin
{
    [KernelFunction, Description("Sends an email to a recipient.")]
    public async Task SendEmailAsync(
        Kernel kernel,
        [Description("Semicolon delimitated list of emails of the recipients")] string recipientEmails,
        string subject,
        string body
    )
    {
        Console.WriteLine("Email infotmation");

        Console.WriteLine($"Recipient Emails: {recipientEmails}");
        Console.WriteLine($"Subject: {subject}");
        Console.WriteLine($"Body: {body}");
        Console.WriteLine("Email sent!");
    }
}