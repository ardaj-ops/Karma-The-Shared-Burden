using System.Collections.Generic;

namespace RoguelikeCardGame.Models
{
    public class HeroTemplate
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int MaxHp { get; set; }
        
        // Popis unikátní mechaniky pro UI
        public string MechanicDescription { get; set; } = string.Empty;
        
        // Počáteční relikvie (otazník znamená, že může být i null)
        public Relic? StartingRelic { get; set; } 

        public List<string> StartingDeck { get; set; } = new List<string>();
        public List<string> DiscoverableCards { get; set; } = new List<string>();

        // Prázdný konstruktor (často ho vyžaduje SignalR pro posílání dat přes internet)
        public HeroTemplate() { }

        public HeroTemplate(string id, string name, int maxHp, string mechanicDescription, Relic startingRelic, List<string> startingDeck, List<string> discoverableCards)
        {
            Id = id;
            Name = name;
            MaxHp = maxHp;
            MechanicDescription = mechanicDescription;
            StartingRelic = startingRelic;
            StartingDeck = startingDeck;
            DiscoverableCards = discoverableCards;
        }
    }
}