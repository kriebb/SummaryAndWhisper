using System.Text;
using System.Text.RegularExpressions;
using Azure.AI.OpenAI;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Configuration;

namespace Kriebbels.SpeechtToText.Console;

public sealed record TranscriptionResult(string InitialTranscript, string PunctuatedTranscript, string FinalTranscript);
public partial class TranscriptionService(OpenAiClientProvider provider, IFileService fileService, IConfiguration configuration):ITranscriptionService
{
    private const string NonAsciiCharactersPattern = @"[^\u0000-\u007F]+";


    public async Task<Result<TranscriptionResult,ValidationResult>> TranscribeAudioAsync(IEnumerable<string> trimmedFiles, string fileName)
    {
        var initialTranscript = await GetInitialTranscript(trimmedFiles, fileName);
        if (initialTranscript.IsFailure)
            return initialTranscript.Error;
        var punctuatedTranscript = await GetPunctuatedTranscript(initialTranscript.Value);
        var finalTranscript = await GetFinalTranscript(punctuatedTranscript);

        return new TranscriptionResult(initialTranscript.Value, punctuatedTranscript, finalTranscript);
    }

    private async Task<Result<string,ValidationResult>> GetInitialTranscript(IEnumerable<string> trimmedFiles, string fileName)
    {
        var transcript = new StringBuilder();

        var client = provider.Create(configuration);
        foreach(var trimmedFile in trimmedFiles)
        {
            var audioFile = await fileService.ReadAllBytesAsync(trimmedFile);
            if (audioFile.IsFailure)
                return audioFile.Error;
            var response = await client.GetAudioTranscriptionAsync(new AudioTranscriptionOptions(
                deploymentName: provider.WhisperDeployment,
                audioData:BinaryData.FromBytes(audioFile.Value))
            {
                Filename = fileName
            });
            transcript.AppendLine(response.Value.Text);
        }

        return NonAsciiCharactersPatternRegEx().Replace(transcript.ToString(), string.Empty);
    }

    private async Task<string> GetTranscript(string prompt, string transcript)
    {
        var client = provider.Create(configuration);
        var response = await client.GetChatCompletionsAsync(new ChatCompletionsOptions
        {
            Messages =
            {
                new ChatRequestSystemMessage(prompt),
                new ChatRequestUserMessage(transcript)
            },
            Temperature = 0.0f,
            DeploymentName = provider.GptDeployment
        });

        return response.Value.Choices[0].Message.Content;
    }

    private async Task<string> GetPunctuatedTranscript(string initialTranscript)
    {
        return await GetTranscript(provider.FormatAndCorrectionsPrompt, initialTranscript);
    }

    private async Task<string> GetFinalTranscript(string punctuatedTranscript)
    {
        return await GetTranscript(provider.FinalizeCorrectionsPrompt, punctuatedTranscript);
    }

    [GeneratedRegex(NonAsciiCharactersPattern)]
    private static partial Regex NonAsciiCharactersPatternRegEx();
}

public interface ITranscriptionService
{
    Task<Result<TranscriptionResult, ValidationResult>> TranscribeAudioAsync(IEnumerable<string> trimmedFiles,
        string fileName);

}