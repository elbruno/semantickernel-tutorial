#pragma warning disable IDE0059, SKEXP0040, SKEXP0043, SKEXP0060	

using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Planning;
using Microsoft.SemanticKernel.Plugins.OpenApi;
using Microsoft.SemanticKernel.Plugins.OpenApi.Extensions;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

// Azure OpenAI keys
var config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();
var deploymentName = config["AZURE_OPENAI_MODEL-GPT3.5"];
var endpoint = config["AZURE_OPENAI_ENDPOINT"];
var apiKey = config["AZURE_OPENAI_APIKEY"];


// Create a chat completion service
var builder = Kernel.CreateBuilder();
builder.AddAzureOpenAIChatCompletion(deploymentName, endpoint, apiKey);
Kernel kernel = builder.Build();

var plugInName = "PetsSearch";
var currentAssemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
var plugInFilepath = Path.Combine(currentAssemblyDirectory, "apimanifest.json");

// specify auth callbacks for each API dependency
var apiManifestPluginParameters = new ApiManifestPluginParameters
{
    FunctionExecutionParameters = new()
    {
        { "petssearch", new OpenApiFunctionExecutionParameters(ignoreNonCompliantErrors: true) },
        { "superheroapi", new OpenApiFunctionExecutionParameters(ignoreNonCompliantErrors: true) }
    }
};

// import api manifest plugin
KernelPlugin plugin = await kernel.ImportPluginFromApiManifestAsync
    (plugInName, plugInFilepath, apiManifestPluginParameters)
    .ConfigureAwait(false);

// set goal

// execute plan
var planGoal = @"Find pets in the pets catalog that have super hero names. 
With the results of the search, show the information for each pet including the pet name, pet type, pet breed and pet age, the pet's owner information, and the super hero details that match the pet's name.
Show the result of the pets with super hero names as a indented list in plain text. 
Do not generate HTML or MARKDOWN, just text.";

// display the goal in the console, with a title GOAL
Console.WriteLine("GOAL");
Console.WriteLine(planGoal);

// create planner
var planner = new FunctionCallingStepwisePlanner(
    new FunctionCallingStepwisePlannerOptions
    {
        MaxIterations = 10,
        MaxTokens = 32000
    }
);

var result = await planner.ExecuteAsync(kernel, planGoal);

// Display the plan in the console, with a title PLAN
Console.WriteLine("PLAN");

// iterate over the steps in the plan
foreach (var step in result.ChatHistory)
{
    // add line separator
    Console.WriteLine("--------------------------------------------------");
    Console.WriteLine($"Role: {step.Role}");
    Console.WriteLine(step.ToString());
    Console.WriteLine("--------------------------------------------------");
}

// display the final answer in the console, with a title FINAL ANSWER
Console.WriteLine("FINAL ANSWER");
Console.WriteLine(result.FinalAnswer);
