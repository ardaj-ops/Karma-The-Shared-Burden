using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace RoguelikeCardGame.Models
{
    public enum NodeType
    {
        Encounter, EliteEncounter, RestPlace, Shop, Treasure, Event, Boss
    }

    public class Node
    {
        public int Id { get; set; }
        public int Floor { get; set; } 
        public NodeType Type { get; set; }
        public bool IsCompleted { get; set; } = false;
        public List<int> ConnectedTo { get; set; } = new List<int>(); 
    }

    public class CardPlayData
    {
        public string CardId { get; set; } = string.Empty;
        public int KarmaShift { get; set; }
        public int Damage { get; set; }
    }

    public class GameRoom
    {
        public string RoomName { get; set; } = string.Empty;
        public List<Player> Players { get; set; } = new List<Player>();
        
        public int CurrentAct { get; set; } = 1; 
        public int CurrentKarma { get; set; } = 0; 
        
        public string EnemyName { get; set; } = "Neznámý nepřítel"; 
        public int EnemyHp { get; set; } = 100;
        public int EnemyMaxHp { get; set; } = 100; 
        
        public int TeamGold { get; set; } = 50; 
        public List<Relic> TeamRelics { get; set; } = new List<Relic>();
        
        public List<Node> Map { get; set; } = new List<Node>();
        public int CurrentNodeId { get; set; } = -1; // -1 znamená, že jsme ještě nezačali (vybíráme 0. patro)

        public ConcurrentDictionary<string, List<CardPlayData>> PlayedCardsThisTurn { get; set; } = new ConcurrentDictionary<string, List<CardPlayData>>();
        public List<string> PlayersReady { get; set; } = new List<string>();

        public GameRoom(string roomName)
        {
            RoomName = roomName;
            GenerateMap(); 
        }

        // --- NOVÉ: ROZVĚTVENÉ GENEROVÁNÍ MAPY ---
        public void GenerateMap()
        {
            Map = new List<Node>();
            int floors = 10;
            int nodesPerFloor = 3;
            int idCounter = 1;

            // 1. Vytvoření uzlů
            for (int f = 0; f < floors; f++)
            {
                int count = (f >= floors - 2) ? 1 : nodesPerFloor; // Předposlední a poslední patro má 1 uzel
                for (int i = 0; i < count; i++)
                {
                    NodeType type = NodeType.Encounter;
                    if (f == floors - 1) type = NodeType.Boss;
                    else if (f == floors - 2) type = NodeType.RestPlace;
                    else if (f > 0)
                    {
                        Random r = new Random(Guid.NewGuid().GetHashCode());
                        int roll = r.Next(100);
                        if (roll < 40) type = NodeType.Encounter;
                        else if (roll < 60) type = NodeType.Event;
                        else if (roll < 70) type = NodeType.Shop;
                        else if (roll < 85) type = NodeType.EliteEncounter;
                        else type = NodeType.Treasure;
                    }
                    Map.Add(new Node { Id = idCounter++, Floor = f, Type = type });
                }
            }

            // 2. Propojení uzlů (Čáry na mapě)
            for (int f = 0; f < floors - 1; f++)
            {
                var curr = Map.Where(n => n.Floor == f).ToList();
                var next = Map.Where(n => n.Floor == f + 1).ToList();

                if (next.Count == 1)
                {
                    foreach (var node in curr) node.ConnectedTo.Add(next[0].Id);
                }
                else
                {
                    Random r = new Random(Guid.NewGuid().GetHashCode());
                    for (int i = 0; i < curr.Count; i++)
                    {
                        curr[i].ConnectedTo.Add(next[i].Id); // Rovně
                        if (i > 0 && r.Next(2) == 0) curr[i].ConnectedTo.Add(next[i - 1].Id); // Šikmo doleva
                        if (i < next.Count - 1 && r.Next(2) == 0) curr[i].ConnectedTo.Add(next[i + 1].Id); // Šikmo doprava
                    }
                }
            }
        }
    }
}