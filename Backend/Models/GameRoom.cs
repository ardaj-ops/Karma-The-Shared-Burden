using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace RoguelikeCardGame.Models
{
    // Typy místností na mapě
    public enum NodeType
    {
        Encounter,        // Běžný souboj
        EliteEncounter,   // Těžký souboj (lepší odměny)
        RestPlace,        // Odpočinek (Heal / Upgrade)
        Shop,             // Obchod s kartami a relikviemi
        Treasure,         // Truhla s pokladem
        Event,            // Náhodná událost
        Boss              // Finální souboj
    }

    // Definice jednoho bodu na mapě
    public class MapNode
    {
        public int Id { get; set; }
        public int Floor { get; set; } // Patro (výška na mapě)
        public NodeType Type { get; set; }
        public bool IsCompleted { get; set; } = false;
        // ID uzlů, do kterých lze z tohoto místa jít
        public List<int> ConnectedTo { get; set; } = new List<int>(); 
    }

   public class CardPlayData
{
    // Přidali jsme = string.Empty;
    public string CardId { get; set; } = string.Empty;
    public int KarmaShift { get; set; }
    public int Damage { get; set; }
}

    public class GameRoom
    {
        public string RoomName { get; set; }
        public List<Player> Players { get; set; } = new List<Player>();
        public int CurrentKarma { get; set; } = 0;
        // --- PARAMETRY AKTUÁLNÍHO BOJE ---
public string EnemyName { get; set; } = "Neznámý nepřítel"; // NOVÉ: Jméno
public int EnemyHp { get; set; } = 100;
public int CurrentAct { get; set; } = 1; // Pamatuje si, v jakém jsme Aktu
public int EnemyMaxHp { get; set; } = 100;                  // NOVÉ: Max HP
public int CurrentKarma { get; set; } = 0; 
        
        // --- EKONOMIKA A POSTUP ---
        public int TeamGold { get; set; } = 50; // Společná pokladna
        public List<Relic> TeamRelics { get; set; } = new List<Relic>();
        
        // Mapa a aktuální pozice
        public List<MapNode> Map { get; set; } = new List<MapNode>();
        public int CurrentNodeId { get; set; } = 0;

        // Změněno na List<CardPlayData>, aby hráč mohl zahrát více karet za kolo
public ConcurrentDictionary<string, List<CardPlayData>> PlayedCardsThisTurn { get; set; } = new ConcurrentDictionary<string, List<CardPlayData>>();
// Množina hráčů, kteří už klikli na "Ukončit tah"
public HashSet<string> PlayersReady { get; set; } = new HashSet<string>();

        public GameRoom(string roomName)
        {
            RoomName = roomName;
            GenerateMap(); // Při vytvoření místnosti vygenerujeme mapu
        }

        // --- LOGIKA GENEROVÁNÍ MAPY ---
        // Vytvoříme jednoduchou cestu: Souboj -> Event -> Obchod -> Poklad -> Boss
        public void GenerateMap()
        {
            Map = new List<MapNode>
            {
                new MapNode { Id = 0, Floor = 0, Type = NodeType.Encounter },
                new MapNode { Id = 1, Floor = 1, Type = NodeType.Event },
                new MapNode { Id = 2, Floor = 2, Type = NodeType.Shop },
                new MapNode { Id = 3, Floor = 3, Type = NodeType.EliteEncounter },
                new MapNode { Id = 4, Floor = 4, Type = NodeType.Treasure },
                new MapNode { Id = 5, Floor = 5, Type = NodeType.RestPlace },
                new MapNode { Id = 6, Floor = 6, Type = NodeType.Boss }
            };

            // Propojení uzlů lineárně (0 -> 1 -> 2...)
            for (int i = 0; i < Map.Count - 1; i++)
            {
                Map[i].ConnectedTo.Add(Map[i + 1].Id);
            }
        }

        // Metoda pro posun do další místnosti
        public MapNode? GetCurrentNode()
        {
            return Map.FirstOrDefault(n => n.Id == CurrentNodeId);
        }
    }
}