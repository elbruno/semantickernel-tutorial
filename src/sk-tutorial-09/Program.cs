// 01 - usings
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

// 02 - create kernel
var config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();

var builder = Kernel.CreateBuilder();
builder.AddAzureOpenAIChatCompletion(
    config["AZURE_OPENAI_MODEL-GPT3.5"],
    config["AZURE_OPENAI_ENDPOINT"],
    config["AZURE_OPENAI_APIKEY"]);
var kernel = builder.Build();

// 14 - define prompt execution settings
var settings = new OpenAIPromptExecutionSettings
{
    MaxTokens = 5,
    Temperature = 1
};
var kernelArguments = new KernelArguments(settings);

// 03 - invoke a simple prompt to the chat service
string prompt = "Write a joke about kittens";
var response = await kernel.InvokePromptAsync(prompt, kernelArguments);
Console.WriteLine(response.GetValue<string>());

