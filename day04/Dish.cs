using Microsoft.WindowsAzure.Storage.Table;

namespace day04 {
    public class Dish : TableEntity {
        public string Email { get; set; }
        public string Description { get; set; }
    }
}