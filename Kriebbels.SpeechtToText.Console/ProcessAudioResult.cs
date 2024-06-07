namespace Kriebbels.SpeechtToText.Console;

public sealed record ProcessAudioResult(
    Silence[] Silences, 
    AudioSegment[] AudioSegments, 
    IEnumerable<string> TrimmedFiles);