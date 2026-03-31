using System;
using System.Collections.Generic;
using System.Linq;

namespace RoguelikeCardGame.Models
{
    // Třída reprezentující položku v obchodě (kartu nebo relikvii)
    public class ShopItem
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Desc { get; set; } = string.Empty; 
        public int Price { get; set; }
        public int Cost { get; set; } // Pouze pro karty (Mana cost)
    }

    // Třída držící kompletní nabídku pro jednoho hráče
    public class PlayerShop
    {
        public List<ShopItem> Cards { get; set; } = new List<ShopItem>();
        public List<ShopItem> Relics { get; set; } = new List<ShopItem>();
        public int RemoveCost { get; set; } = 50;
    }

    public static class ShopManager
    {
        // Vygeneruje unikátní obchod pro konkrétního hráče
        public static PlayerShop GenerateShopForPlayer(Player player)
        {
            // Guid.NewGuid() zaručí, že každý hráč dostane opravdu jiný seed pro náhodu
            Random rng = new Random(Guid.NewGuid().GetHashCode());
            var shop = new PlayerShop();

            // 1. Výběr 4 náhodných karet 
            // (Zde můžeš v budoucnu přidat podmínku "Where(c => c.Class == player.HeroClass)")
            var allCards = CardDatabase.Cards.Values.ToList();
            var selectedCards = allCards.OrderBy(x => rng.Next()).Take(4).ToList();
            
            foreach (var c in selectedCards)
            {
                shop.Cards.Add(new ShopItem 
                { 
                    Id = c.Id, 
                    Name = c.Name, 
                    Desc = c.Description, 
                    Cost = c.Cost,
                    Price = rng.Next(40, 80) // Náhodná cena karty
                });
            }

            // 2. Výběr 2 náhodných relikvií
            var selectedRelics = RelicDatabase.LootRelics.OrderBy(x => rng.Next()).Take(2).ToList();
            foreach (var r in selectedRelics)
            {
                shop.Relics.Add(new ShopItem 
                { 
                    Id = r.Id, 
                    Name = r.Name, 
                    Desc = r.Description, 
                    Price = rng.Next(120, 200) // Náhodná cena relikvie
                });
            }

            // 3. Cena za odstranění karty
            // (Zde by se v budoucnu dalo nastavit, že se cena zvyšuje s každým smazáním)
            shop.RemoveCost = 50; 

            return shop;
        }
    }
}