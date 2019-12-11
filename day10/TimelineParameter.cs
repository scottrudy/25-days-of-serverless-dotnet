using Microsoft.WindowsAzure.Storage.Table;

namespace day10 {
    public class TimelineParameter : TableEntity {
        public long SinceId { get; set; }
    }
}