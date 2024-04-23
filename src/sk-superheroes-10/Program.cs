//    Copyright (c) 2024
//    Author      : Bruno Capuano
//    Change Log  :
//    - Sample console application to use OpenAI and Semantic Kernel
//
//    The MIT License (MIT)
//
//    Permission is hereby granted, free of charge, to any person obtaining a copy
//    of this software and associated documentation files (the "Software"), to deal
//    in the Software without restriction, including without limitation the rights
//    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//    copies of the Software, and to permit persons to whom the Software is
//    furnished to do so, subject to the following conditions:
//
//    The above copyright notice and this permission notice shall be included in
//    all copies or substantial portions of the Software.
//
//    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//    THE SOFTWARE.

using Keys;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Planning.Handlebars;
using System.Text.Json;

// Create a chat completion service
var config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();
IKernelBuilder builder = Kernel.CreateBuilder();
builder.AddAzureOpenAIChatCompletion(
    config["AZURE_OPENAI_MODEL-GPT3.5"],
    config["AZURE_OPENAI_ENDPOINT"],
    config["AZURE_OPENAI_APIKEY"]);
Kernel kernel = builder.Build();

// Load Plugins
string pluginsDirectoryPath = Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\..\", "plugins");
KernelPlugin pluginFunctions = kernel.ImportPluginFromPromptDirectory(pluginsDirectoryPath);

// tell a super hero joke
KernelArguments variables = new KernelArguments()
{
    ["input"] = "Tell me a April's fools joke",
    ["hero"] = "Batman"
};
FunctionResult result = await kernel.InvokeAsync(pluginFunctions["Joke"], variables);

ConsoleHelper.PromptAndResponse(variables, result.GetValue<string>());

// tell a super hero story
variables = new KernelArguments()
{
    ["input"] = "Tell me a Spring Story",
    ["hero"] = "Superman"
};

ConsoleHelper.PromptAndResponse(variables, result.GetValue<string>());

// create an out of office
variables = new KernelArguments()
{
    ["input"] = "Create an OOF for Christmas",
    ["hero"] = "Hulk"
};

result = await kernel.InvokeAsync(pluginFunctions["OOF"], variables);
ConsoleHelper.PromptAndResponse(variables, result.GetValue<string>());

// get a super hero info
HeroInfo heroInfo = new HeroInfo(SuperHero.ApiKey);
builder.Plugins.AddFromObject(heroInfo, "HeroInfo");
kernel = builder.Build();

// get the alter ego of an hero using native functions
variables = new KernelArguments()
{
    ["input"] = "Ironman"
};

string? heroResult = await kernel.InvokeAsync<string>("HeroInfo", "GetAlterEgo", variables);
ConsoleHelper.PromptAndResponse(variables, heroResult);

// Create planner
#pragma warning disable SKEXP0060
HandlebarsPlanner planner = new HandlebarsPlanner();

string ask = "I would like you to tell me a joke about Batman, and with that joke, create an out-of-office message using the joke.";
HandlebarsPlan originalPlan = await planner.CreatePlanAsync(kernel, ask);

Console.WriteLine("Original plan:\n");
Console.WriteLine(JsonSerializer.Serialize(originalPlan, new JsonSerializerOptions { WriteIndented = true }));

// executing the plan
#pragma warning disable SKEXP0060
string originalPlanResult = await originalPlan.InvokeAsync(kernel, new KernelArguments());

Console.WriteLine("Original Plan results:\n");
Console.WriteLine(originalPlanResult.ToString());