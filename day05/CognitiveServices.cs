namespace day05 {

// The C# classes that represents the JSON returned by the Detect Language API.
#region DetectLanguage
    public class DetectRequest {
        public string Text { get; set; }
    }

    public class DetectResult {
        public string Language { get; set; }
        public float Score { get; set; }
        public bool IsTranslationSupported { get; set; }
        public bool IsTransliterationSupported { get; set; }
        public AltTranslations[] Alternatives { get; set; }
    }

    public class AltTranslations {
        public string Language { get; set; }
        public float Score { get; set; }
        public bool IsTranslationSupported { get; set; }
        public bool IsTransliterationSupported { get; set; }
    }
#endregion

// The C# classes that represents the JSON returned by the Translator Text API.
#region TranslatorText
    public class TranslationRequest {
        public string Text { get; set; }
    }

    public class TranslationResult
    {
        public DetectedLanguage DetectedLanguage { get; set; }
        public TextResult SourceText { get; set; }
        public Translation[] Translations { get; set; }
    }

    public class DetectedLanguage
    {
        public string Language { get; set; }
        public float Score { get; set; }
    }

    public class TextResult
    {
        public string Text { get; set; }
        public string Script { get; set; }
    }

    public class Translation
    {
        public string Text { get; set; }
        public TextResult Transliteration { get; set; }
        public string To { get; set; }
        public Alignment Alignment { get; set; }
        public SentenceLength SentLen { get; set; }
    }

    public class Alignment
    {
        public string Proj { get; set; }
    }

    public class SentenceLength
    {
        public int[] SrcSentLen { get; set; }
        public int[] TransSentLen { get; set; }
    }
#endregion

// The C# classes that represents the JSON returned by the Translator Text API.
#region TextAnalysis
    public class AnalysisRequest {
        public RequestDocument[] Documents { get; set; }
    }

    public class AnalysisResult {
        public ResponseDocument[] Documents { get; set; }
        public object[] Errors { get; set; }
    }

    public class RequestDocument {
        public string Language { get; set; }
        public int Id { get; set; }
        public string Text { get; set; }
    }

    public class ResponseDocument {
        public int Id { get; set; }
        public double Score { get; set; }
    }

#endregion
}