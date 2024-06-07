using CSharpFunctionalExtensions;

namespace Kriebbels.SpeechtToText.Console;

public interface IFileService
{
    Task<Result<byte[], ValidationResult>> ReadAllBytesAsync(string path);
    Result<bool,ValidationResult> Exists(string fullFilePath);
    Result<string,ValidationResult> Delete(string fullFilePath);
    Task<Result<string, ValidationResult>> WriteTranscriptsToFileAstnc(string pathFullFilePath, string valueInitialTranscript, string valuePunctuatedTranscript, string valueFinalTranscript);
}