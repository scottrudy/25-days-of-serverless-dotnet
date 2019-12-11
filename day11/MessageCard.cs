using Newtonsoft.Json;

namespace day11 {

    // https://docs.microsoft.com/en-us/microsoftteams/platform/webhooks-and-connectors/how-to/connectors-using
    public class MessageCard {
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

//        [JsonProperty(PropertyName = "potentialAction", Order=6)]
//        public ActionCard[] PotentialAction { get; set; }
    } 

    public class MessageSection {
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

    public class SectionFact {
        [JsonProperty(PropertyName = "name", Order=1)]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "value", Order=2)]
        public string Value { get; set; }
    }

    public class ActionCard {
        [JsonProperty(PropertyName = "@type", Order=1)]
        public string Type => nameof(ActionCard);
        [JsonProperty(PropertyName = "name", Order=2)]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "inputs", Order=3)]
        public ActionCardInput[] Inputs { get; set; }
        [JsonProperty(PropertyName = "actions", Order=4)]
        public ActionCardAction[] Actions { get; set; }
    }

    public class ActionCardInput {
        [JsonProperty(PropertyName = "@type", Order=1)]
        public string Type { get; set; }
        [JsonProperty(PropertyName = "id", Order=2)]
        public string Id { get; set; }
        [JsonProperty(PropertyName = "title", Order=3)]
        public string Title { get; set; }
        [JsonProperty(PropertyName = "IsMultiSelect", Order=4)]
        public bool IsMultiSelect { get; set; }
        [JsonProperty(PropertyName = "choices", Order=1)]
        public ActionCardInputChoice[] Choices { get; set; }
    }

     public class ActionCardInputChoice {
        [JsonProperty(PropertyName = "display", Order=1)]
        public string Display { get; set; }
        [JsonProperty(PropertyName = "value", Order=2)]
        public string Value { get; set; }
    }

   public class ActionCardAction {
        [JsonProperty(PropertyName = "@type", Order=1)]
        public string Type { get; set; }
        [JsonProperty(PropertyName = "name", Order=2)]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "target", Order=3)]
        public string Target { get; set; }
    }    
}