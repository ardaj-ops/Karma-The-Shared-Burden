using System;
using System.Collections.Generic;

namespace RoguelikeCardGame.Models
{
    public class Player
    {
        public string ConnectionId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        
        // --- HRDINA A ŽIVOTY ---
        // --- HRDINA A ŽIVOTY ---
public string HeroClass { get; set; } = string.Empty;
public int Hp { get; set; }           // Aktuální životy
public int MaxHp { get; set; }        // Maximální životy
public int Block { get; set; } = 0;   // NOVÉ: Štíty (Blok)
        // --- MANA SYSTÉM ---
        public int Mana { get; set; }
        public int MaxMana { get; set; } = 3;

        // --- NOVÉ: EKONOMIKA ---
        public int Gold { get; set; } = 50;   // Každý hráč začne s 50 zlaťáky

        // --- DECK SYSTÉM ---
        public List<string> StartingDeck { get; set; } = new List<string>();
        public List<string> DrawPile { get; set; } = new List<string>();
        public List<string> Hand { get; set; } = new List<string>();
        public List<string> DiscardPile { get; set; } = new List<string>();

        // Prázdný konstruktor (Důležité pro SignalR/Serializaci)
        public Player() { }

        // Hlavní konstruktor
        public Player(string connectionId, string name)
        {
            ConnectionId = connectionId;
            Name = name;
        }

        // --- FUNKCE PRO HRU ---

        // 1. Připraví hru: nakopíruje balíček, vyčistí ruku, nastaví manu a životy
        public void InitializeGame()
        {
            DrawPile = new List<string>(StartingDeck); 
            Hand.Clear();
            DiscardPile.Clear();
            
            Mana = MaxMana; 
            Hp = MaxHp; // Na startu hry (v první místnosti) má hráč plné životy
            
            ShuffleDeck();
        }

        // 2. Zamíchá dobírací balíček (Fisher-Yates algoritmus)
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

        // 3. Lízne zadaný počet karet do ruky
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
                        break; // Už nejsou žádné karty k líznutí
                    }
                }

                string drawnCard = DrawPile[0];
                DrawPile.RemoveAt(0);
                Hand.Add(drawnCard);
            }
        }
    }
}