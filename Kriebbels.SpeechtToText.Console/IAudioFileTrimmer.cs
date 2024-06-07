namespace Kriebbels.SpeechtToText.Console;

public interface IAudioFileTrimmer
{
    IEnumerable<string> FindTrimmedFiles(IEnumerable<AudioSegment> audioSegments, string filePath);
}