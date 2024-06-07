using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;

namespace Kriebbels.SpeechtToText.Console;

public class FileService(ILogger<FileService> logger) : IFileService
{

    public async Task<Result<byte[], ValidationResult>> ReadAllBytesAsync(string path)
    {
        try
        {
            return await File.ReadAllBytesAsync(path);

        }
        catch (Exception e)
        {
            logger.LogCritical(e,"Could not read all bytes from {Path}", path);
            return new ValidationResult("Could not read all bytes",e);
        }
    }

    public Result<bool, ValidationResult> Exists(string fullFilePath)
    {
        return File.Exists(fullFilePath);
    }

    public Result<string,ValidationResult> Delete(string fullFilePath)
    {
        try
        {
            File.Delete(fullFilePath);
            return fullFilePath;
        }
        catch (Exception e)
        {
            return new ValidationResult("Could not delete a file",e);
        }

    }

    public async Task<Result<string, ValidationResult>> WriteTranscriptsToFileAstnc(string filePath, string initialTranscript, string punctuatedTranscript, string finalTranscript)
    {
        var res1 = WriteAllTextAsync(filePath + "_1.whisper.txt", initialTranscript);
        var res2 = WriteAllTextAsync(filePath + "_2.punc.txt", punctuatedTranscript);
        var res3 = WriteAllTextAsync(filePath + "_3.finalTranscript.txt", finalTranscript);

        await res1;
        await res2;
        await res3;
        var results = new List<Result<string, ValidationResult>> { res1.Result, res2.Result, res3.Result };

        var successfulResults = new List<string>();
        var failedResults = new List<ValidationResult>();

        foreach (var result in results)
        {
            if (result.TryGetValue(out var value))
            {
                successfulResults.Add(value);
            }
            else if (result.TryGetError(out var error))
            {
                failedResults.Add(error);
            }
        }

        if (failedResults.Count != 0)
        {
            return Result.Failure<string, ValidationResult>(new ValidationResult(failedResults));
        }

        var joinedResults = string.Join(";", successfulResults);

        return Result.Success<string, ValidationResult>(joinedResults);
    }

    private async Task<Result<string,ValidationResult>> WriteAllTextAsync(string fullFilePath, string finalTranscript)
    {
        try
        {
            await File.WriteAllTextAsync(fullFilePath, finalTranscript);
            return fullFilePath;
        }
        catch (Exception e)
        {
            logger.LogCritical(e, "Could not write all text for {FullFilePath}", fullFilePath);
            return new ValidationResult("Could not write all text",e);
        }
        
    }
}

public record ValidationResult
{
    public List<ValidationResult>? FailedResults { get; }

    public ValidationResult(string message, Exception? exception)
    {
        Message = message;
        Exception = exception;
    }

    public ValidationResult(List<ValidationResult> failedResults):this("One or more files could not be written", null)
    {
        FailedResults = failedResults;
    }


    public string Message { get; }
    public Exception? Exception { get; }
}