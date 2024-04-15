//    Copyright (c) 2024
//    Author      : Bruno Capuano
//    Change Log  :
//    - Sample console application to show how to use semantic memory with Semantic Kernel using documents and files
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
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Connectors.AzureAISearch;
using System.Net.Http;
using HtmlAgilityPack;
using Azure.AI.OpenAI;
using Humanizer;
using System.Diagnostics;

// Azure OpenAI keys
string deploymentName = AzureOpenAI.DeploymentName;
string endpoint = AzureOpenAI.Endpoint;
string apiKey = AzureOpenAI.ApiKey;


// Create a chat completion service
IKernelBuilder builder = Kernel.CreateBuilder();
builder.AddAzureOpenAIChatCompletion(deploymentName, endpoint, apiKey);

// create the memory builder
#pragma warning disable SKEXP0000, SKEXP0003, SKEXP0011, SKEXP0020, SKEXP0052
MemoryBuilder memoryBuilder = new MemoryBuilder();

memoryBuilder.
    WithAzureOpenAITextEmbeddingGeneration(
        AzureOpenAI.EmbeddingsModel,
        AzureOpenAI.Endpoint,
        AzureOpenAI.ApiKey)
    .WithMemoryStore(new AzureAISearchMemoryStore(AzureAISearch.Endpoint, AzureAISearch.ApiKey));

ISemanticTextMemory memory = memoryBuilder.Build();

Dictionary<string, string> gitHubFiles = new Dictionary<string, string>
{
    ["https://github.com/microsoft/semantic-kernel/blob/main/README.md"]
        = "Installation, getting started, and how to contribute",
    ["https://docs.github.com/en/account-and-profile/managing-subscriptions-and-notifications-on-github/setting-up-notifications/configuring-notifications"] = "Configuring notifications"

    //["https://github.com/microsoft/semantic-kernel/blob/main/dotnet/notebooks/02-running-prompts-from-file.ipynb"] = "Jupyter notebook describing how to pass prompts from a file to a semantic plugin or function"
    //    ,
    //["https://github.com/microsoft/semantic-kernel/blob/main/dotnet/notebooks//00-getting-started.ipynb"] = "Jupyter notebook describing how to get started with the Semantic Kernel",
    //["https://github.com/microsoft/semantic-kernel/tree/main/samples/plugins/ChatPlugin/ChatGPT"] = "Sample demonstrating how to create a chat plugin interfacing with ChatGPT",
    //["https://github.com/microsoft/semantic-kernel/blob/main/dotnet/src/SemanticKernel/Memory/VolatileMemoryStore.cs"] = "C# class that defines a volatile embedding store"
};

foreach (KeyValuePair<string, string> entry in gitHubFiles)
{
    // read the string content from the webpage
    HttpClient client = new HttpClient();
    string url = entry.Key;
    string htmlContent  = await client.GetStringAsync(url);

    var htmlDocument = new HtmlDocument();
    htmlDocument.LoadHtml(htmlContent);

    string content = htmlDocument.DocumentNode.InnerText.Trim();

    // remove all duplicates and empty lines in content
    content = string.Join("\n", content.Split('\n').Distinct().Where(x => !string.IsNullOrWhiteSpace(x)));


    // split the content in chunks of 6000 characters
    // to avoid the 8000 characters limit in the text field
    // of the memory store
    int chunkSize = 6000;
    List<string> chunks = new List<string>();
    for (int i = 0; i < content.Length; i += chunkSize)
    {
        string chunk = content.Substring(i, Math.Min(chunkSize, content.Length - i));
        string key = entry.Key + "-" + i;
        //await memory.SaveReferenceAsync(
        //    collection: "GitHubFiles",
        //    externalSourceName: "GitHub",
        //    externalId: key,
        //    description: entry.Value,
        //    text: chunk);

        await memory.SaveInformationAsync(collection: "GitHubFiles",
            text: chunk, id: key, description: entry.Value);

    }

}

// ask questions and show the response
List<string> questions = new List<string>()
{
    "What are the steps to create a new Semantic Kernel app in C#?",
    "How to configure notifications on GitHub?",
    "How do you prepare an apple pie?"
};

foreach (string q in questions)
{
    var response = await memory.SearchAsync("GitHubFiles", q, withEmbeddings: true, limit: 1).FirstOrDefaultAsync();

    

    Console.WriteLine(q + " >> ID:" + response?.Metadata.Id + " - Description: " + response?.Metadata.Description + " - Relevance: " + response.Relevance + " - Is Reference:" + response?.Metadata.IsReference);
    Console.WriteLine();
}
