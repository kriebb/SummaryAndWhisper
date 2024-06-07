namespace Kriebbels.SpeechtToText.Console;

public interface IAudioSegmenter
{
    AudioSegment[] FindAudibleSegments(string fileName, Silence[] silences);
}