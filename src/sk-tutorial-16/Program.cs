//    Copyright (c) 2024
//    Author      : Bruno Capuano
//    Change Log  :
//    - Sample console application to use Azure OpenAI and Semantic Kernel
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

#pragma warning disable SKEXP0003, SKEXP0011, SKEXP0052

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.KernelMemory;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.Plugins.Memory;

// Azure OpenAI keys
var config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();
var deploymentName = config["AZURE_OPENAI_MODEL"];
var endpoint = config["AZURE_OPENAI_ENDPOINT"];
var apiKey = config["AZURE_OPENAI_APIKEY"];


// Create a chat completion service
var builder = Kernel.CreateBuilder();
builder.AddAzureOpenAIChatCompletion(deploymentName, endpoint, apiKey);
builder.AddLocalTextEmbeddingGeneration();
Kernel kernel = builder.Build();

var question = "What is Bruno's favourite super hero?";

var response = await kernel.InvokePromptAsync(question);
Console.WriteLine($"{question}: {response.GetValue<string>()}");

// display a message that sais press a key to continue
Console.WriteLine("Press a key to continue...");
Console.ReadLine();

//////////////////////////////////////////////////////////////////////////////////////////////////////
///
// get the embeddings generator service
var embeddingGenerator = kernel.Services.GetRequiredService<ITextEmbeddingGenerationService>();

SemanticTextMemory memory = new(new VolatileMemoryStore(), embeddingGenerator);

// add facts to the collection
const string MemoryCollectionName = "fanFacts";

await memory.SaveInformationAsync(MemoryCollectionName, id: "info1", text: "Gisela's favourite super hero is Batman");
await memory.SaveInformationAsync(MemoryCollectionName, id: "info2", text: "The last super hero movie watched by Gisela was Guardians of the Galaxy Vol 3");
await memory.SaveInformationAsync(MemoryCollectionName, id: "info3", text: "Bruno's favourite super hero is Invincible");
await memory.SaveInformationAsync(MemoryCollectionName, id: "info4", text: "The last super hero movie watched by Bruno was Aquaman II");
await memory.SaveInformationAsync(MemoryCollectionName, id: "info5", text: "Bruno don't like the super hero movie: Eternals");

TextMemoryPlugin memoryPlugin = new(memory);

// Import the text memory plugin into the Kernel.
kernel.ImportPluginFromObject(memoryPlugin);

OpenAIPromptExecutionSettings settings = new()
{
    ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
};


var prompt = @"
    Question: {{$input}}
    Answer the question using the memory content: {{Recall}}
";

KernelArguments arguments = new KernelArguments(settings)
{
    { "input", question },
    { "collection", MemoryCollectionName }

};

response = await kernel.InvokePromptAsync(prompt, arguments);

// run the prompt
Console.WriteLine($"{question}: {response.GetValue<string>()}");