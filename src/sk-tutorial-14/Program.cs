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

using Azure.AI.OpenAI;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

// Azure OpenAI keys
var config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();
var deploymentName = config["AZURE_OPENAI_MODEL"];
var endpoint = config["AZURE_OPENAI_ENDPOINT"];
var apiKey = config["AZURE_OPENAI_APIKEY"];

var aiSearchEndpoint = config["AZURE_AISEARCH_ENDPOINT"];
var aiSearchApiKey = config["AZURE_AISEARCH_APIKEY"];


// Create a chat completion service
var builder = Kernel.CreateBuilder();
builder.AddAzureOpenAIChatCompletion(deploymentName, endpoint, apiKey);

// Get the chat completion service
Kernel kernel = builder.Build();
var chat = kernel.GetRequiredService<IChatCompletionService>();

// Create a sample chat history
var history = new ChatHistory();
history.AddSystemMessage("You are a helpful assistant.");
history.AddUserMessage("What do you suggest to go hiking in a rainy day?");

var azureSearchExtensionConfiguration = new AzureSearchChatExtensionConfiguration
{
    SearchEndpoint = new Uri(aiSearchEndpoint),
    Authentication = new OnYourDataApiKeyAuthenticationOptions(aiSearchApiKey),
    IndexName = "contoso-products-with-weather-index"
};
var chatExtensionsOptions = new AzureChatExtensionsOptions { Extensions = { azureSearchExtensionConfiguration } };
var executionSettings = new OpenAIPromptExecutionSettings { AzureChatExtensionsOptions = chatExtensionsOptions };

// run the prompt
var result = await chat.GetChatMessageContentsAsync(history, executionSettings);
var content = result[^1].Content;
Console.WriteLine(content);

if (result.FirstOrDefault().InnerContent is ChatResponseMessage)
{
    var ic = result.FirstOrDefault().InnerContent as ChatResponseMessage;
    var aec = ic.AzureExtensionsContext;
    var citations = aec.Citations;
    foreach (var citation in citations)
    {
        Console.WriteLine($"Title: {citation.Title}");
        Console.WriteLine($"URL: {citation.Url}");
        Console.WriteLine($"Filepath: {citation.Filepath}");
        Console.WriteLine($"Content: {citation.Content.Substring(0, 50)}");
    }
}