//    Copyright (c) 2024
//    Author      : Bruno Capuano
//    Change Log  :
//    - Sample console application to show how to use semantic memory with Semantic Kernel
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

using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.Memory;


// Create a kernel and an embedding generation service
var builder = Kernel.CreateBuilder();
builder.AddLocalTextEmbeddingGeneration();
var kernel = builder.Build();
var embeddingGenerator = kernel.Services.GetRequiredService<ITextEmbeddingGenerationService>();

// create a memory with the embedding generation service and a volatile memory store
var memoryBuilder = new MemoryBuilder();
memoryBuilder.WithTextEmbeddingGeneration(embeddingGenerator);
memoryBuilder.WithMemoryStore(new VolatileMemoryStore());
var memory = memoryBuilder.Build();



// add facts to the collection
const string MemoryCollectionName = "fanFacts";

await memory.SaveInformationAsync(MemoryCollectionName, id: "info1", text: "Gisela's favourite super hero is Batman");
await memory.SaveInformationAsync(MemoryCollectionName, id: "info2", text: "The last super hero movie watched by Gisela was Guardians of the Galaxy Vol 3");
await memory.SaveInformationAsync(MemoryCollectionName, id: "info3", text: "Bruno's favourite super hero is Invincible");
await memory.SaveInformationAsync(MemoryCollectionName, id: "info4", text: "The last super hero movie watched by Bruno was Aquaman II");
await memory.SaveInformationAsync(MemoryCollectionName, id: "info5", text: "Bruno don't like the super hero movie: Eternals");

// ask questions and show the response
var questions = new[]
{
    "what is Bruno's favourite super hero?",
    "what was the last movie watched by Gisela?",
    "Which is the prefered super hero for Gisela?",
    "Did Bruno watched a super hero movie in the past, which was the last one?"
};

foreach (var q in questions)
{
    var response = await memory.SearchAsync(MemoryCollectionName, q).FirstOrDefaultAsync();
    Console.WriteLine(q + " " + response?.Metadata.Text);
}
