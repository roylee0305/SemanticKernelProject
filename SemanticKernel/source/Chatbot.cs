using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

class MainProgram
{
    static async Task Main(string[] args)
    {
        var configFilePath = "../../../../config.txt";
        var openaiModelId = ConfigReader.ReadConfigValue(configFilePath, "OPENAI_MODEL_ID");
        var openaiApiKey = ConfigReader.ReadConfigValue(configFilePath, "OPENAI_API_KEY");

        HttpClient client = new HttpClient(new MyHttpMessageHandler());

        var kernelBuilder = Kernel.CreateBuilder().AddOpenAIChatCompletion(
            modelId: openaiModelId,
            apiKey: openaiApiKey,
            httpClient: client
            );
        var kernel = kernelBuilder.Build();

        var systemMessage = @"You are a useful ChatBot. 
        ChatBot can have a conversation with you about any topic. 
        It can give explicit instructions or say 'I don't know' if it does not have an answer.."
        ;

        var chat = kernel.GetRequiredService<IChatCompletionService>();
        var history = new ChatHistory();
        history.AddSystemMessage(systemMessage);

        Console.WriteLine("ChatBot > Hi, I am a ChatBot. (type 'bye' to exit)\n");
        while (true)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("User > ");
            var userInput = Console.ReadLine();
            Console.ResetColor();

            if (userInput == "bye" || userInput == null)
                break;

            history.AddUserMessage(userInput);

            var result = await chat.GetChatMessageContentsAsync(history);

            Console.WriteLine($"\nChatBot > {result[^1].Content}\n");

            history.AddMessage(result[^1].Role, result[^1].Content ?? string.Empty);
        }

        // Wait for the user to respond before closing.
        Console.Write("\nPress any key to close the console app...");
        Console.ReadKey();
    }
}