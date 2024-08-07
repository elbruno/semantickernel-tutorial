﻿//    Copyright (c) 2024
//    Author      : Bruno Capuano
//    Change Log  :
//    - Sample console application to show how to use native functions with Semantic Kernel
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

using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Planning.Handlebars;
using System.Text.Json;

// Azure OpenAI keys
var config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();
var deploymentName = config["AZURE_OPENAI_MODEL"];
var endpoint = config["AZURE_OPENAI_ENDPOINT"];
var apiKey = config["AZURE_OPENAI_APIKEY"];

// Create a chat completion service
var builder = Kernel.CreateBuilder();
//builder.AddAzureOpenAIChatCompletion(deploymentName, endpoint, apiKey);
builder.AddOpenAIChatCompletion(
    modelId: "phi3",
    endpoint: new Uri("http://localhost:11434"),
    apiKey: "apikey");

// add the hero info native functions
var heroInfo = new HeroInfo(config["SUPERHERO_APIKEY"]);
builder.Plugins.AddFromObject(heroInfo, "HeroInfo");
Kernel kernel = builder.Build();

// Create planner
#pragma warning disable SKEXP0060
var planner = new HandlebarsPlanner();

var ask = "Find out who is Batman, and show a response with the super hero name and the alter ego name. Use emojis and a list for the response.";
var originalPlan = await planner.CreatePlanAsync(kernel, ask);

Console.WriteLine("Original plan:\n");
Console.WriteLine(JsonSerializer.Serialize(originalPlan, new JsonSerializerOptions { WriteIndented = true }));

// executing the plan
#pragma warning disable SKEXP0060
var originalPlanResult = await originalPlan.InvokeAsync(kernel, new KernelArguments());

Console.WriteLine("Original Plan results:\n");
Console.WriteLine(originalPlanResult.ToString());