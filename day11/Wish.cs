using Microsoft.WindowsAzure.Storage.Table;

namespace day11 {
    public class Wish : TableEntity {
        public string Description { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string WishType { get; set; }
    }
}
