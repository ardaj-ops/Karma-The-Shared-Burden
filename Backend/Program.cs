using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using RoguelikeCardGame.Hubs;
using System;

var builder = WebApplication.CreateBuilder(args);

var port = Environment.GetEnvironmentVariable("PORT") ?? "10000";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
// =======================================================

// Přidáme SignalR (toto automaticky registruje i IHubContext)
builder.Services.AddSignalR();

// Nastavení CORS - Povolí tvému frontendu mluvit s backendem, i když mají jiné URL
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.SetIsOriginAllowed(origin => true) 
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Nutné pro SignalR handshake
    });
});

var app = builder.Build();

// POŘADÍ JE KRITICKÉ:
app.UseCors(); // 1. Nejdříve vyřešíme povolení přístupu

app.UseDefaultFiles(); // 2. Pak zkusíme najít index.html
app.UseStaticFiles();  // 3. Pak povolíme odesílání souborů (js, css)

// 4. Nakonec namapujeme Hub
app.MapHub<GameHub>("/gamehub");

app.Run();