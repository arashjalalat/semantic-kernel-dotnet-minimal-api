using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using OpenAIMinimalApi.Models;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapPost("plugins/{pluginName}/invoke/{functionName}", async (HttpContext context, Query query, string pluginName, string functionName) =>
        {
            try
            {
                // Prepare variables
                var headers = context.Request.Headers;
                var deploymentName = headers["x-sk-web-app-deployment"]; //e.g. gpt-35-turbo
                var endpoint = headers["x-sk-web-app-endpoint"];
                var modelId = "gpt-3.5-turbo";
                var apiKey = headers["x-sk-web-app-key"];
                if (string.IsNullOrEmpty(modelId) || string.IsNullOrEmpty(apiKey))
                {
                    throw new Exception("Missing required headers");
                }

                IKernelBuilder builder = Kernel.CreateBuilder();
                var pluginDirectory = "Plugins";

                builder.AddOpenAIChatCompletion(modelId, apiKey!).Build();
                //builder.AddAzureOpenAIChatCompletion(deploymentName!, endpoint!, apiKey!);

                Kernel kernel = builder.Build();
                var storyPluginDirectoryPath = Path.Combine(System.IO.Directory.GetCurrentDirectory(), pluginDirectory, pluginName);

                // Load the StoryPlugin from the Plugins Directory
                var storyPluginFunctions = kernel.ImportPluginFromPromptDirectory(storyPluginDirectoryPath);
                
                // Enable auto invocation of kernel functions
                OpenAIPromptExecutionSettings settings = new()
                {
                    ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
                };

                // Run the Function called StoryGen
                var result = await kernel.InvokeAsync(storyPluginFunctions[functionName], new() { ["input"] = query.Value });
                
                Console.WriteLine(result.ToString());

                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                };
                
                // Return the Json response to the client
                return Results.Json(JsonSerializer.Deserialize<Response>(result.ToString(), options));
            }
            catch (Exception ex)
            {
                return Results.Text(ex.Message );
            }
        });

app.Run();