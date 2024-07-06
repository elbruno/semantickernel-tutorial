using HandlebarsDotNet.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

// create kernel
var config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();
var deploymentName = config["AZURE_OPENAI_MODEL"];
var endpoint = config["AZURE_OPENAI_ENDPOINT"];
var apiKey = config["AZURE_OPENAI_APIKEY"];
var superHeroApiKey = config["SUPERHERO_APIKEY"];

var builder = Kernel.CreateBuilder();
//builder.AddAzureOpenAIChatCompletion(deploymentName, endpoint, apiKey);
builder.AddOpenAIChatCompletion(
    modelId: "phi3",
    endpoint: new Uri("http://localhost:11434"),
    apiKey: "apikey");

// add the hero info native functions
var heroInfo = new HeroInfo(superHeroApiKey);
builder.Plugins.AddFromObject(heroInfo, "HeroInfo");
Kernel kernel = builder.Build();

// create chat
var chat = kernel.GetRequiredService<IChatCompletionService>();
var history = new ChatHistory();
history.AddSystemMessage("You are a usefull assistant. You always reply with a short and funny message. If you don't know an answer, you say 'I don't know that.'");

// run chat
while (true)
{
    Console.Write("Q: ");
    var userQ = Console.ReadLine();
    if (string.IsNullOrEmpty(userQ))
    {
        break;
    }
    history.AddUserMessage(userQ);

    Console.Write($"Phi3: ");

    OpenAIPromptExecutionSettings settings = new()
    {
        ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
        Temperature = 1
    };

    var response = "";
    var result = chat.GetStreamingChatMessageContentsAsync(history, settings, kernel);
    await foreach (var message in result)
    {
        Console.Write(message.Content);
        response += message.Content;
    }

    history.AddAssistantMessage(response);
    Console.WriteLine("");
}
