#pragma warning disable IDE0059, SKEXP0040, SKEXP0043, SKEXP0060	

using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Planning;
using Microsoft.SemanticKernel.Plugins.OpenApi;
using Microsoft.SemanticKernel.Plugins.OpenApi.Extensions;
using System.Reflection;

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
        { "elbruno.petssearch", new OpenApiFunctionExecutionParameters(ignoreNonCompliantErrors: true) }
    }
};

// import api manifest plugin
KernelPlugin plugin = await kernel.ImportPluginFromApiManifestAsync
    (plugInName, plugInFilepath, apiManifestPluginParameters)
    .ConfigureAwait(false);

// set goal



// create planner
var planner = new FunctionCallingStepwisePlanner(
    new FunctionCallingStepwisePlannerOptions
    {
        MaxIterations = 10,
        MaxTokens = 32000
    }
);

// execute plan
var goal = @"List all the pets names and owners in El Bruno Pets Store";
var result = await planner.ExecuteAsync(kernel, goal);

// output result
Console.WriteLine(result.FinalAnswer);
