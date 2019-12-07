using Newtonsoft.Json;

namespace day06 {
    public class AdaptiveCard {
        [JsonProperty("$schema", Order=1)]
        public string Schema => "https://adaptivecards.io/schemas/adaptive-card.json";
        [JsonProperty("type", Order=2)]
        public string Type => nameof(AdaptiveCard);
        [JsonProperty("version", Order=3)]
        public string Version  => "1.0";
        [JsonProperty("summary", Order=4)]
        public string Summary { get; set; }
        [JsonProperty("body", Order=5)]
        public Body[] Body { get; set; }
        [JsonProperty("padding", Order=6)]
        public string Padding => "Medium";
    }

    public class Body {
        [JsonProperty("type", Order=1)]
        public string Type { get; set; }
        [JsonProperty("text", Order=2)]
        public string Text { get; set; }
        [JsonProperty("wrap", Order=3)]
        public bool Wrap { get; set; }
    }

    public class MessageCard
    {
        public MessageCard() : this(string.Empty) { }

        public MessageCard(string summary, params MessageSection[] sections)
        {
            Summary = summary;
            Sections = sections;
        }

        [JsonProperty(PropertyName = "@type", Order=1)]
        public string Type => nameof(MessageCard);
        [JsonProperty(PropertyName = "@context", Order=2)]
        public string Context => "http://schema.org/extensions";
        [JsonProperty(PropertyName = "themeColor", Order=3)]
        public string ThemeColor => "0076D7";

        [JsonProperty(PropertyName = "summary", Order=4)]
        public string Summary { get; set; }

        [JsonProperty(PropertyName = "sections", Order=5)]
        public MessageSection[] Sections { get; set; }
    } 

    public class MessageSection
    {
        [JsonProperty(PropertyName = "activityTitle", Order=1)]
        public string ActivityTitle { get; set; }
        [JsonProperty(PropertyName = "activitySubtitle", Order=2)]
        public string ActivitySubtitle { get; set; }
        [JsonProperty(PropertyName = "activityImage", Order=3)]
        public string ActivityImage { get; set; }
        [JsonProperty(PropertyName = "facts", Order=4)]
        public SectionFact[] Facts { get; set; }
        [JsonProperty(PropertyName = "markdown", Order=5)]
        public bool Markdown { get; set; }
    }

    public class SectionFact
    {
        [JsonProperty(PropertyName = "name", Order=1)]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "value", Order=2)]
        public string Value { get; set; }
    }    
}