using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using RoguelikeCardGame.Hubs; // Přidáno, abychom nemuseli psát celou cestu k Hubu

var builder = WebApplication.CreateBuilder(args);

// Přidáme SignalR pro real-time komunikaci
builder.Services.AddSignalR();

// --- OPRAVENO: NASTAVENÍ CORS ---
// Povolíme připojení odkudkoliv (z tvého lokálu i z tvého frontendu na Renderu)
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.SetIsOriginAllowed(origin => true) // Tohle povolí jakoukoliv URL adresu frontendu
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Tohle je kriticky důležité pro SignalR
    });
});

var app = builder.Build();

// Aktivujeme naše CORS pravidla
app.UseCors();

// Jednoduchá zpráva pro kontrolu, že server funguje, když si rozklikneš adresu backendu
app.MapGet("/", () => "Backend pro Karma: The Shared Burden běží!");

// Nasadíme náš komunikační Hub na tuto adresu
app.MapHub<GameHub>("/gamehub");

app.Run();