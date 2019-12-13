using Microsoft.WindowsAzure.Storage.Table;

namespace day12 {
    public class CacheEntity : TableEntity {
        public string Content { get; set; }
        public string GithubEtag { get; set; }
    }
}
