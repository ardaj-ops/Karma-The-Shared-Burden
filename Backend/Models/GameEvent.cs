using System.Collections.Generic;

namespace RoguelikeCardGame.Models
{
    public class GameEvent
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<EventOption> Options { get; set; } = new List<EventOption>();
    }

    public class EventOption
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
    }
}