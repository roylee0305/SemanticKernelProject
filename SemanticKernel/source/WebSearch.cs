using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Plugins.Web;
using Microsoft.SemanticKernel.Plugins.Web.Google;
#pragma warning disable SKEXP0050

class MainProgram
{
    static async Task Main(string[] args)
    {
        var configFilePath = "../../../../config.txt";
        var openaiModelId = ConfigReader.ReadConfigValue(configFilePath, "OPENAI_MODEL_ID");
        var openaiApiKey = ConfigReader.ReadConfigValue(configFilePath, "OPENAI_API_KEY");
        var googleApiKey = ConfigReader.ReadConfigValue(configFilePath, "GOOGLE_API_KEY");
        var googleSearchEngineId = ConfigReader.ReadConfigValue(configFilePath, "GOOGLE_SEARCH_ENGINE_ID");
        
        // init kernel
        HttpClient client = new HttpClient(new MyHttpMessageHandler());

        var kernelBuilder = Kernel.CreateBuilder().AddOpenAIChatCompletion(
            modelId: openaiModelId,
            apiKey: openaiApiKey,
            httpClient: client
            );
        var kernel = kernelBuilder.Build();

        // import web search plugin
        var googleConnextor = new GoogleConnector(
            apiKey: googleApiKey,
            searchEngineId: googleSearchEngineId);
        kernel.ImportPluginFromObject(new WebSearchEnginePlugin(googleConnextor), "google");

        // import plugins from prompt directory
        var pluginsDirectory = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "..", "..", "..", "..", "Plugins");
        var plugins = kernel.ImportPluginFromPromptDirectory(pluginsDirectory);

        Console.WriteLine("ChatBot > Hi, please input your question. (type 'bye' to exit)");
        while (true)
        {
            // get ueser input
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("\nUser > ");
            var userInput = Console.ReadLine();

            if (userInput == null || userInput.ToLower() == "bye")
                break;

            // conver input into executable command
            var webSearchResult = await kernel.InvokeAsync(plugins["WebSearchPlugin"], new KernelArguments
            {
                ["input"] = userInput
            });

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\nExecution > {webSearchResult}");

            // convert executable command into prompt template
            var command = webSearchResult.GetValue<string>() ?? string.Empty;
            var promptTemplateFactory = new KernelPromptTemplateFactory();
            var promptTemplate = promptTemplateFactory.Create(new PromptTemplateConfig(command));

            // get web search information
            var information = await promptTemplate.RenderAsync(kernel);

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"\nInformation > {information}");

            // summarize information
            var summarizeResult = await kernel.InvokeAsync(plugins["SummarizePlugin"], new KernelArguments
            {
                ["externalInformation"] = information,
                ["question"] = userInput
            });

            Console.ResetColor();
            Console.WriteLine($"\nChatBot > {summarizeResult}");
        }

        // wait for the user to respond before closing
        Console.ResetColor();
        Console.Write("\nPress any key to close the console app...");
        Console.ReadKey();
    }
}