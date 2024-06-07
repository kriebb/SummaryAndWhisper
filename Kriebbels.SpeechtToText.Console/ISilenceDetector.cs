namespace Kriebbels.SpeechtToText.Console;

public interface ISilenceDetector
{
    Silence[] FindSilences(string fileName, double silenceThreshold = -40);
}