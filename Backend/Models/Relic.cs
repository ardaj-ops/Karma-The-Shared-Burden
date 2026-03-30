namespace RoguelikeCardGame.Models
{
    public class Relic
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public Relic(string id, string name, string description)
        {
            Id = id;
            Name = name;
            Description = description;
        }
    }
}