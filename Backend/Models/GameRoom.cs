using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace RoguelikeCardGame.Models
{
    public enum NodeType { Encounter, EliteEncounter, RestPlace, Shop, Treasure, Event, Boss }

    // --- Třída ActiveEnemy pro real-time správu ve 3D ---
    public class ActiveEnemy
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public string TemplateName { get; set; } = string.Empty; 
        public int Hp { get; set; }
        public int MaxHp { get; set; }
        public EnemyAction CurrentAction { get; set; } = new EnemyAction();

        public Dictionary<string, int> Effects { get; set; } = new Dictionary<string, int>();

        // --- 3D FYZIKA A REAL-TIME ---
        public float X { get; set; } = 0f;
        public float Y { get; set; } = 0f;
        public float Z { get; set; } = 0f;
        
        // Za jak dlouho monstrum zaútočí (v milisekundách)
        public float AttackCooldown { get; set; } = 3000f; // Standardně každé 3 vteřiny
        public float CurrentCooldown { get; set; } = 3000f;
        public float Speed { get; set; } = 2.0f; // Rychlost pohybu v Three.js

        public void AddEffect(string type, int amount)
        {
            if (Effects.ContainsKey(type)) Effects[type] += amount;
            else Effects[type] = amount;
        }
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
        public string TargetEnemyId { get; set; } = string.Empty; 
    }

    public class GameRoom
    {
        public string RoomName { get; set; } = string.Empty;
        public List<Player> Players { get; set; } = new List<Player>();
        
        // Místnost nyní sama spravuje nepřátele pro real-time výpočty
        public List<ActiveEnemy> ActiveEnemies { get; set; } = new List<ActiveEnemy>();
        
        public int CurrentAct { get; set; } = 1; 
        public int CurrentKarma { get; set; } = 0; 
        
        public int TeamGold { get; set; } = 50; 
        public List<Relic> TeamRelics { get; set; } = new List<Relic>();
        
        public List<Node> Map { get; set; } = new List<Node>();
        public int CurrentNodeId { get; set; } = -1; 

        public ConcurrentDictionary<string, int> MapVotes { get; set; } = new ConcurrentDictionary<string, int>();
        public ConcurrentDictionary<string, List<CardPlayData>> PlayedCardsThisTurn { get; set; } = new ConcurrentDictionary<string, List<CardPlayData>>();
        public List<string> PlayersReady { get; set; } = new List<string>();

        // --- REAL-TIME SMYČKA ---
        private System.Timers.Timer? _battleTimer; // Nullable varování opraveno přidáním '?'
        private int _tickRateMs = 100; // Server tiká 10x za sekundu
        private int _manaTickAccumulator = 0;

        // Události, na které se GameHub napojí, aby odeslal data klientům (Three.js)
        public event Action<GameRoom>? OnTickUpdate;
        public event Action<GameRoom, ActiveEnemy>? OnEnemyAttack;

        public GameRoom(string roomName)
        {
            RoomName = roomName;
            GenerateMap(); 
        }

        // ==========================================
        // REAL-TIME BATTLE LOGIKA
        // ==========================================
        
        public void StartBattle()
        {
            if (_battleTimer != null) return;
            
            _battleTimer = new System.Timers.Timer(_tickRateMs);
            _battleTimer.Elapsed += (sender, e) => BattleTick();
            _battleTimer.Start();
        }

        public void StopBattle()
        {
            if (_battleTimer != null)
            {
                _battleTimer.Stop();
                _battleTimer.Dispose();
                _battleTimer = null;
            }
        }

        private void BattleTick()
        {
            bool requireSync = false;

            // 1. GENERACE MANY (Každou 1 sekundu)
            _manaTickAccumulator += _tickRateMs;
            if (_manaTickAccumulator >= 1000) 
            {
                _manaTickAccumulator = 0;
                foreach(var p in Players)
                {
                    if (p.Mana < p.MaxMana) 
                    { 
                        p.Mana++; 
                        requireSync = true; 
                    }
                }
            }

            // 2. AI NEPŘÁTEL A COOLDOWNY
            foreach(var enemy in ActiveEnemies)
            {
                if (enemy.Hp <= 0) continue;

                enemy.CurrentCooldown -= _tickRateMs;
                if (enemy.CurrentCooldown <= 0)
                {
                    // Monstrum vystřelí útok!
                    enemy.CurrentCooldown = enemy.AttackCooldown; 
                    OnEnemyAttack?.Invoke(this, enemy); // Signalizuje GameHubu, ať to pošle do Three.js
                }
            }

            // 3. BROADCAST (Odeslání nového stavu, pokud se něco důležitého změnilo)
            if (requireSync)
            {
                OnTickUpdate?.Invoke(this);
            }
        }

        // ==========================================
        // 3D GENERACE ARÉNY A SPAWNOVÁNÍ
        // ==========================================
        public void Initialize3DArena()
        {
            Random rng = new Random();

            // 1. Nastavíme hráče blízko středu arény
            float offset = 0f;
            foreach (var player in Players)
            {
                player.X = offset;
                player.Y = 0f;
                player.Z = 0f;
                offset += 2.0f; // Rozestup mezi hráči, pokud jich je víc
            }

            // 2. Rozmístíme nepřátele v kruhu kolem hráčů (10-20 jednotek daleko)
            foreach (var enemy in ActiveEnemies)
            {
                double angle = rng.NextDouble() * Math.PI * 2;
                double radius = rng.NextDouble() * 10 + 10;

                enemy.X = (float)(Math.Cos(angle) * radius);
                enemy.Z = (float)(Math.Sin(angle) * radius);
                enemy.Y = 0f; 
                
                // Mírný rozptyl v prvním útoku, ať nezaútočí všichni najednou
                enemy.CurrentCooldown = enemy.AttackCooldown + rng.Next(-500, 500);
            }
        }

        // ==========================================
        // GENEROVÁNÍ MAPY (Slay the Spire styl stromu)
        // ==========================================
        public void GenerateMap()
        {
            Map = new List<Node>();
            int floors = 10;
            int nodesPerFloor = 3;
            int idCounter = 1;

            for (int f = 0; f < floors; f++)
            {
                int count = (f >= floors - 2) ? 1 : nodesPerFloor; 
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
                        curr[i].ConnectedTo.Add(next[i].Id);
                        if (i > 0 && r.Next(2) == 0) curr[i].ConnectedTo.Add(next[i - 1].Id); 
                        if (i < next.Count - 1 && r.Next(2) == 0) curr[i].ConnectedTo.Add(next[i + 1].Id); 
                    }
                }
            }
        }
    }
}