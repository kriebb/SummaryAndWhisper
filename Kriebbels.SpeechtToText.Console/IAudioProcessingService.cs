using CSharpFunctionalExtensions;

namespace Kriebbels.SpeechtToText.Console;

public interface IAudioProcessingService
{
    Result<ProcessAudioResult,ValidationResult> ProcessAudioAsync(string filePath);
}