using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using RoguelikeCardGame.Hubs; 

var builder = WebApplication.CreateBuilder(args);

// Přidáme SignalR pro real-time komunikaci
builder.Services.AddSignalR();

// NASTAVENÍ CORS
// Povolíme připojení odkudkoliv (z tvého lokálu i z tvého frontendu na Renderu)
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.SetIsOriginAllowed(origin => true) 
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); 
    });
});

var app = builder.Build();

// Aktivujeme naše CORS pravidla
app.UseCors();

// --- NOVÉ (KISS PRINCIP) ---
// 1. Nastaví index.html jako výchozí stránku, když někdo přijde na hlavní URL
app.UseDefaultFiles(); 

// 2. Tohle přesně OPRAVUJE tu červenou chybu v konzoli! 
// Dovoluje serveru odesílat .css, .js a .html soubory se správným MIME typem.
app.UseStaticFiles(); 
// ---------------------------

// Nasadíme náš komunikační Hub na tuto adresu
app.MapHub<GameHub>("/gamehub");

app.Run();