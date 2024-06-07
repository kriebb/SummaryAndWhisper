using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;

namespace Kriebbels.SpeechtToText.Console;

public class ConsoleApp(
    IAudioRecordingService audioRecordingService, 
    IAudioProcessingService audioProcessingService, 
    IAudioTranscriptionService audioTranscriptionService,
    IFileService fileService,
    IServiceProvider serviceProvider,
    ILogger<ConsoleApp> logger)

{
    private string? _fileName;
    private AudioFile? _path;
    public IServiceProvider ServiceProvider => serviceProvider;


    public Task StartAsync()
    {
        _fileName = Guid.NewGuid().ToString();

        _path = new AudioFile( FileName : $"{_fileName}.wav", FullPath : $".\\{_fileName}.wav");
        
        return audioRecordingService.StartRecordAudioAsync(_path);
    }
    
    public async Task StopAsync()
    {
        if (_path == null) return;
    
        audioRecordingService.StopRecordAudio();
        await audioProcessingService.ProcessAudioAsync(_path.FullPath)
            .Bind(async processedAudio =>
            {
                return await audioTranscriptionService.TranscribeAudioAsync(processedAudio.TrimmedFiles)
                    .Bind(async transcription =>
                    {
                        var fileResult = await fileService.WriteTranscriptsToFileAstnc(_path.FullPath,
                            transcription.InitialTranscript,
                            transcription.PunctuatedTranscript, transcription.FinalTranscript);
                        return fileResult;
                    });
            })
            .TapError(ex =>
            {
                // Handle the AggregateException here
                if(ex.FailedResults != null)
                {
                    foreach (var failedResult in ex.FailedResults)
                    {
                        logger.LogCritical(failedResult.Exception, "An error occurred: {Message}", failedResult.Message);
                    }
                }
                logger.LogCritical(ex.Exception, "An error occurred: {Message}", ex.Message);
            });
    }
}