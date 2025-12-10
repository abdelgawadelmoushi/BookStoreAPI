namespace BookStoreAPI.Models
{
    public class PersonInfo
    {
       public int Id { get; set; }
        public string Name { get; set; }=string.Empty;
        public string? IMG { get; set; }
           public int Age { get; set; }
      public List<string>? Skills { get; set; } = [];

    }
}
