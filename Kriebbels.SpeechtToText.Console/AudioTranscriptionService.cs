using System.Text;
using System.Text.RegularExpressions;
using Azure.AI.OpenAI;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;

namespace Kriebbels.SpeechtToText.Console;

public class AudioTranscriptionService : IAudioTranscriptionService
{
    private readonly OpenAiClientProvider _provider;
    private readonly OpenAIClient _client;
    private readonly ILogger<AudioTranscriptionService> _logger;
    private readonly IFileService _fileService;

    public AudioTranscriptionService(OpenAiClientProvider provider, OpenAIClient client, ILogger<AudioTranscriptionService> logger, IFileService fileService)
    {
        _provider = provider;
        _client = client;
        _logger = logger;
        _fileService = fileService;
    }

    public async Task<Result<AudioTranscriptionResult,ValidationResult>> TranscribeAudioAsync(IEnumerable<string> trimmedFiles)
    {
        try
        {
            var transcript = new StringBuilder();

            foreach (var trimmedFile in trimmedFiles)
            {
                var audioFile = await _fileService.ReadAllBytesAsync(trimmedFile);
                var response = await _client.GetAudioTranscriptionAsync(new AudioTranscriptionOptions(
                    deploymentName: _provider.WhisperDeployment,
                    audioData: BinaryData.FromBytes(audioFile.Value))
                {
                    Filename = trimmedFile
                });
                transcript.AppendLine(response.Value.Text);
            }
            

            var asciiText = Regex.Replace(transcript.ToString(), @"[^\u0000-\u007F]+", string.Empty);
            _logger.LogInformation("Whisper Result: {AsciiText}",asciiText);

            // FormatAndCorrections
            var punctuationResponse = await _client.GetChatCompletionsAsync(new ChatCompletionsOptions
            {
                Messages =
                {
                    new ChatRequestSystemMessage(_provider.FormatAndCorrectionsPrompt),
                    new ChatRequestUserMessage(asciiText)
                },
                Temperature = 0.0f,
                DeploymentName = _provider.GptDeployment
            });

            var punctuatedTranscript = punctuationResponse.Value.Choices[0].Message.Content;
            _logger.LogInformation("Added format with punctation: {PunctuatedTranscript}",punctuatedTranscript);

            // finalize
            var productAssistantResponse = await _client.GetChatCompletionsAsync(new ChatCompletionsOptions
            {
                Messages =
                {
                    new ChatRequestSystemMessage(_provider.FinalizeCorrectionsPrompt),
                    new ChatRequestUserMessage(punctuatedTranscript)
                },
                Temperature = 0.0f,
                DeploymentName = _provider.GptDeployment
            });

            var finalTranscript = productAssistantResponse.Value.Choices[0].Message.Content;
            _logger.LogInformation("Productize the result: {FinalTranscript}", finalTranscript);

            return new AudioTranscriptionResult(asciiText, punctuatedTranscript, finalTranscript);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Transcription failed");
            return new ValidationResult("Could not transcribe", ex);
        }
    }
}