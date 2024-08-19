using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel;

var builder = Kernel.CreateBuilder();
builder.AddOpenAIChatCompletion(
        modelId: "llama3.1:8b",
        endpoint: new Uri("http://localhost:11434/"),
        apiKey: "apikey");
    
var heroInfo = new HeroInfo("10d9fe4acd0db9670f88108fa42cb221");
builder.Plugins.AddFromObject(heroInfo, "HeroInfo");

Kernel kernel = builder.Build();    

IChatCompletionService chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();


OpenAIPromptExecutionSettings executionSettings = new() { ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions };

// generate plan
//ChatHistory chatHistory = [];
//chatHistory.AddUserMessage("Check current UTC time and return current weather in Boston city.");
//await chatCompletionService.GetChatMessageContentAsync(chatHistory, executionSettings, kernel);
//ChatHistory generatedPlan = chatHistory;

// execute plan
FunctionResult result = await kernel.InvokePromptAsync("Check the alter ego of Batman", new(executionSettings));
string planResult = result.ToString();

Console.WriteLine(planResult);