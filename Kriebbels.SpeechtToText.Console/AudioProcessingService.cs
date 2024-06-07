using CSharpFunctionalExtensions;

namespace Kriebbels.SpeechtToText.Console;

public class AudioProcessingService(
    OpenAiClientProvider provider,
    ISilenceDetector silenceDetector,
    IAudioSegmenter audioSegmenter,
    IAudioFileTrimmer audioTrimmer):IAudioProcessingService
{
    public Result<ProcessAudioResult, ValidationResult> ProcessAudioAsync(string filePath)
    {
        try
        {
            var silences = silenceDetector.FindSilences(filePath, provider.SilenceThreshold);
            var audioSegments = audioSegmenter.FindAudibleSegments(filePath, silences);
            var trimmedFiles = audioTrimmer.FindTrimmedFiles(audioSegments, filePath);

            var result = new ProcessAudioResult(silences, audioSegments, trimmedFiles);

            return result;
        }
        catch (Exception ex)
        {
            return new ValidationResult("Could not process audio", ex);
        }
    }


}