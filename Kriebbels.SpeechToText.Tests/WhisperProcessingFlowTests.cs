using Azure.AI.OpenAI;
using Kriebbels.SpeechtToText.Console;
using Meziantou.Extensions.Logging.Xunit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Kriebbels.SpeechToText.Tests;

public class WhisperProcessingFlowTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    private readonly OpenAiClientProvider _provider;
    private ConfigurationManager _configuration;

    public WhisperProcessingFlowTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;

        _provider = new OpenAiClientProvider();
        _configuration = new ConfigurationManager();
        _configuration.AddJsonFile("appsettings.tests.json", false);
        _configuration.AddUserSecrets<Program>(false,true);
        
    }
    //https://github.com/Azure-Samples/openai/blob/main/Basic_Samples/Whisper/dotnet/csharp/Whisper_prompting_guide.ipynb
    [Fact]
    public async Task PlayBook()
    {
        // Arrange
        var fileService = new FileService(XUnitLogger.CreateLogger<FileService>(_testOutputHelper));
        ISilenceDetector silenceDetector = new SilenceDetector();
        IAudioSegmenter audioSegmenter = new AudioSegmenter();
        IAudioFileTrimmer audioTrimmer = new AudioFileTrimmer();
        var audioProcessingService = new AudioProcessingService(_provider,silenceDetector, audioSegmenter,audioTrimmer);
        var transcriptionService = new TranscriptionService(_provider, fileService,_configuration);

        var path = await UseEarningsCallWavFile(false);

        // Act
        var processAudioResult = audioProcessingService.ProcessAudioAsync(path.fullFilePath);
        if (processAudioResult.IsFailure)
            Assert.Fail(processAudioResult.Error.Message);
        var transcriptionResult = await transcriptionService.TranscribeAudioAsync(processAudioResult.Value.TrimmedFiles, path.fileName);

        // Assert
        Assert.NotNull(transcriptionResult.Value.InitialTranscript);
        Assert.NotNull(transcriptionResult.Value.PunctuatedTranscript);
        Assert.NotNull(transcriptionResult.Value.FinalTranscript);

        await fileService.WriteTranscriptsToFileAstnc(path.fullFilePath, transcriptionResult.Value.InitialTranscript,transcriptionResult.Value.PunctuatedTranscript, transcriptionResult.Value.FinalTranscript);
    }
    private static async Task<(string fileName, string fullFilePath)> UseEarningsCallWavFile(bool force)
    {
        // set download paths
        var earningsCallUrl = "https://cdn.openai.com/API/examples/data/EarningsCall.wav";

//set local save locations
        var earningsFilepath = "EarningsCall.wav"; 
        var earningsCallFilepath = "./" + earningsFilepath;

        if (File.Exists(earningsCallFilepath))
        {
            if (force)
                File.Delete(earningsCallFilepath);
            else
                return (earningsFilepath, earningsCallFilepath);


        }

// download the file
        var httpClient = new HttpClient();
        using (var stream = await httpClient.GetStreamAsync(earningsCallUrl))
        {
            using (var fileStream = new FileStream(earningsCallFilepath, FileMode.CreateNew))
            {
      
                await stream.CopyToAsync(fileStream);
            }
        }

        return (earningsFilepath,earningsCallFilepath);
    }
    
  
}