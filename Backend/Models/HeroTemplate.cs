using System.Collections.Generic;

namespace RoguelikeCardGame.Models
{
    public class HeroTemplate
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int MaxHp { get; set; }
        
        // NOVÉ: Popis unikátní mechaniky pro UI
        public string MechanicDescription { get; set; } 
        
        // NOVÉ: Počáteční relikvie
        public Relic StartingRelic { get; set; } 

        public List<string> StartingDeck { get; set; } 
        public List<string> DiscoverableCards { get; set; }

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