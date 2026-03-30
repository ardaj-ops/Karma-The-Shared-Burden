using System;
using System.Collections.Generic;

namespace RoguelikeCardGame.Models
{
    public class Player
    {
        public string ConnectionId { get; set; } 
        public string Name { get; set; }

        // --- MANA SYSTÉM ---
        // Vlastnosti (Properties) pro sledování aktuální a maximální many
        public int Mana { get; set; }
        public int MaxMana { get; set; } = 3;

        // --- DECK SYSTÉM ---
        // Seznamy pro držení ID karet (např. "Py_50")
        public List<string> StartingDeck { get; set; } = new List<string>();
        public List<string> DrawPile { get; set; } = new List<string>();
        public List<string> Hand { get; set; } = new List<string>();
        public List<string> DiscardPile { get; set; } = new List<string>();

        // Tvůj původní konstruktor (volá se při vytvoření nového hráče)
        public Player(string connectionId, string name)
        {
            ConnectionId = connectionId;
            Name = name;
        }

        // --- FUNKCE PRO HRU ---

        // 1. Připraví hru: nakopíruje balíček, vyčistí ruku, nastaví manu a zamíchá
        public void InitializeGame()
        {
            DrawPile = new List<string>(StartingDeck); // Vytvoří kopii startovního balíčku
            Hand.Clear();
            DiscardPile.Clear();
            Mana = MaxMana; 
            ShuffleDeck();
        }

        // 2. Zamíchá dobírací balíček (používá Fisher-Yates algoritmus)
        public void ShuffleDeck()
        {
            Random rng = new Random();
            int n = DrawPile.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                // Prohození karet
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
                // Pokud nám došly karty v dobíracím balíčku, otočíme odhazovací
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
                        // Pokud nemáme karty ani v odhazovacím, přestaneme lízat
                        break; 
                    }
                }

                // Vezmeme první kartu shora, dáme ji do ruky a smažeme z balíčku
                string drawnCard = DrawPile[0];
                DrawPile.RemoveAt(0);
                Hand.Add(drawnCard);
            }
        }
    }
}