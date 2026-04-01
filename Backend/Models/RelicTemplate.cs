using System;

namespace RoguelikeCardGame.Models
{
    public class Relic
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public Relic() { }

        public Relic(string id, string name, string description)
        {
            Id = id;
            Name = name;
            Description = description;
        }
    }
}