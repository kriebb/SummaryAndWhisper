using CSharpFunctionalExtensions;

namespace Kriebbels.SpeechtToText.Console;

public interface IAudioTranscriptionService
{
    Task<Result<AudioTranscriptionResult,ValidationResult>> TranscribeAudioAsync(IEnumerable<string> trimmedFiles);
}