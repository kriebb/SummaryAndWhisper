namespace Kriebbels.SpeechtToText.Console;

public sealed record AudioTranscriptionResult(string InitialTranscript, string PunctuatedTranscript, string FinalTranscript);