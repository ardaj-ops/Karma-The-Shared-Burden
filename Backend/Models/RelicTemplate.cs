using System;

namespace RoguelikeCardGame.Models
{
    /// <summary>
    /// Základní šablona pro všechny relikvie ve hře.
    /// </summary>
    public class Relic
    {
        // Unikátní identifikátor (např. Hero_01, Relic_055)
        public string Id { get; set; } = string.Empty;

        // Viditelný název relikvie
        public string Name { get; set; } = string.Empty;

        // Popis efektu, který se zobrazí hráči
        public string Description { get; set; } = string.Empty;

        // Prázdný konstruktor pro serializaci (SignalR)
        public Relic() { }

        // Hlavní konstruktor pro vytváření v databázi
        public Relic(string id, string name, string description)
        {
            Id = id;
            Name = name;
            Description = description;
        }
    }
}