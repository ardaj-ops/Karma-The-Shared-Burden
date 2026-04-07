using System;
using System.Collections.Generic;

namespace RoguelikeCardGame.Models
{
    public class Player
    {
        public string ConnectionId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        
        // --- HRDINA A ŽIVOTY ---
        public string HeroClass { get; set; } = string.Empty;
        public int Hp { get; set; }           // Aktuální životy
        public int MaxHp { get; set; }        // Maximální životy
        public int Block { get; set; } = 0;   // Štíty (Blok)

        // --- MANA SYSTÉM ---
        public int Mana { get; set; }
        public int MaxMana { get; set; } = 3;

        // --- EKONOMIKA ---
        public int Gold { get; set; } = 50;

        // --- 3D FYZIKA A POZICE ---
        public float X { get; set; } = 0f;
        public float Y { get; set; } = 0f;
        public float Z { get; set; } = 0f;

        // --- DECK SYSTÉM ---
        public List<string> StartingDeck { get; set; } = new List<string>();
        public List<string> DrawPile { get; set; } = new List<string>();
        public List<string> Hand { get; set; } = new List<string>();
        public List<string> DiscardPile { get; set; } = new List<string>();

        // --- NOVÉ: SYSTÉM EFEKTŮ A KOMBA (Důležité pro RelicManager) ---
        
        // Slovník pro ukládání buffů a debuffů (např. "Strength", "Dexterity", "Poison")
        public Dictionary<string, int> Effects { get; set; } = new Dictionary<string, int>();

        // Počítadlo karet zahraných v tomto tahu (pro relikvie)
        public int CardsPlayedThisTurn { get; set; } = 0;

        // Prázdný konstruktor (Důležité pro SignalR/Serializaci)
        public Player() { }

        // Hlavní konstruktor
        public Player(string connectionId, string name)
        {
            ConnectionId = connectionId;
            Name = name;
        }

        // --- FUNKCE PRO HRU ---

        // Metoda pro přidání efektu (řeší chybu CS1061)
        public void AddEffect(string type, int amount)
        {
            if (Effects.ContainsKey(type))
            {
                Effects[type] += amount;
            }
            else
            {
                Effects[type] = amount;
            }
        }

        // Připraví hru: nakopíruje balíček, vyčistí ruku, nastaví manu a životy
        public void InitializeGame()
        {
            DrawPile = new List<string>(StartingDeck); 
            Hand.Clear();
            DiscardPile.Clear();
            Effects.Clear();         // Vyčistit efekty na začátku hry
            CardsPlayedThisTurn = 0; // Resetovat počítadlo komb
            
            Mana = MaxMana; 
            Hp = MaxHp;
            
            // Resetování pozice na začátku hry
            X = 0f;
            Y = 0f;
            Z = 0f;
            
            ShuffleDeck();
        }

        // Zamíchá dobírací balíček (Fisher-Yates algoritmus)
        public void ShuffleDeck()
        {
            Random rng = new Random();
            int n = DrawPile.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                string value = DrawPile[k];
                DrawPile[k] = DrawPile[n];
                DrawPile[n] = value;
            }
        }

        // Lízne zadaný počet karet do ruky
        public void DrawCards(int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (DrawPile.Count == 0)
                {
                    if (DiscardPile.Count > 0)
                    {
                        DrawPile = new List<string>(DiscardPile);
                        DiscardPile.Clear();
                        ShuffleDeck();
                    }
                    else
                    {
                        break; 
                    }
                }

                string drawnCard = DrawPile[0];
                DrawPile.RemoveAt(0);
                Hand.Add(drawnCard);
            }
        }
    }
}