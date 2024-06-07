using System.Net;
using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Configuration;

namespace Kriebbels.SpeechtToText.Console;

public class OpenAiClientProvider
{

    
    public OpenAIClient Create(IConfiguration configuration)
    {


        HttpClient.DefaultProxy = new WebProxy("127.0.0.1:8888");

        string azureOpenAiKey = configuration["OPEN_AI_KEY"]!;

        // Your endpoint should look like the following https://YOUR_OPEN_AI_RESOURCE_NAME.openai.azure.com/
        string azureOpenAiEndpoint = configuration["OPEN_AI_ENDPOINT"]!;

        // Enter the deployment name you chose when you deployed the model.
        WhisperDeployment = configuration["whisper_deployment_name"]!;

        GptDeployment =  configuration["gpt_deployment_name"]!;

        SilenceThreshold = int.Parse(configuration["silence_threshold"]);
        FormatAndCorrectionsPrompt = configuration["FormatPunctuationsCorrectionsPrompt"];
        FinalizeCorrectionsPrompt = configuration[nameof(FinalizeCorrectionsPrompt)];
        return new OpenAIClient(new Uri(azureOpenAiEndpoint), new AzureKeyCredential(azureOpenAiKey));
    }


    public int SilenceThreshold { get; set; }

    public string GptDeployment { get; set; }

    public string WhisperDeployment { get; set; }
    public string FinalizeCorrectionsPrompt { get; set; }
    public string FormatAndCorrectionsPrompt { get; set; }
}