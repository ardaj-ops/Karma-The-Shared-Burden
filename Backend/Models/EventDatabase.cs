using System;
using System.Collections.Generic;

namespace RoguelikeCardGame.Models
{
    public static class EventDatabase
    {
        public static List<GameEvent> Events = new List<GameEvent>
        {
            new GameEvent {
                Id = "E_01",
                Title = "Záhadná svatyně",
                Description = "Uprostřed cesty nacházíte starou svatyni se zářícím oltářem. Z ní vyzařuje zvláštní, poklidná energie.",
                Options = new List<EventOption> {
                    new EventOption { Id = 1, Text = "Pomodlit se (+10 Max HP)" },
                    new EventOption { Id = 2, Text = "Prohledat obětiny (+50 Zlaťáků, ale -10 HP)" },
                    new EventOption { Id = 3, Text = "Odejít bez povšimnutí" }
                }
            },
            new GameEvent {
                Id = "E_02",
                Title = "Zbloudilý alchymista",
                Description = "Potkáváš podivného starce, který ti nabízí bublající zelený lektvar za trochu tvé krve.",
                Options = new List<EventOption> {
                    new EventOption { Id = 1, Text = "Vypít lektvar (-15 HP, +20 Max HP)" },
                    new EventOption { Id = 2, Text = "Odmítnout s díky" }
                }
            },
            new GameEvent {
                Id = "E_03",
                Title = "Tajemná hromada mincí",
                Description = "Na zemi uprostřed lesa leží hromada zlata. Vypadá to jako zjevná past, ale peníze se hodí...",
                Options = new List<EventOption> {
                    new EventOption { Id = 1, Text = "Vzít jen trochu (+30 Zlaťáků)" },
                    new EventOption { Id = 2, Text = "Vzít všechno (+100 Zlaťáků, ale 50% šance na -20 HP)" } 
                }
            }
        };

        public static GameEvent GetRandomEvent()
        {
            Random rng = new Random();
            return Events[rng.Next(Events.Count)];
        }

        // Tady se vyhodnotí následky podle toho, na co hráč kliknul!
        public static void ApplyEventEffect(string eventId, int optionId, Player p)
        {
            Random rng = new Random();
            
            if (eventId == "E_01") {
                if (optionId == 1) { p.MaxHp += 10; p.Hp += 10; }
                else if (optionId == 2) { p.Gold += 50; p.Hp -= 10; if (p.Hp < 1) p.Hp = 1; }
            }
            else if (eventId == "E_02") {
                if (optionId == 1) { p.Hp -= 15; p.MaxHp += 20; if (p.Hp < 1) p.Hp = 1; }
            }
            else if (eventId == "E_03") {
                if (optionId == 1) { p.Gold += 30; }
                else if (optionId == 2) { 
                    p.Gold += 100; 
                    if (rng.NextDouble() > 0.5) { p.Hp -= 20; if (p.Hp < 1) p.Hp = 1; }
                }
            }
        }
    }
}