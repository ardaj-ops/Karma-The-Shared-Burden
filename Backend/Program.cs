using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Přidáme SignalR pro real-time komunikaci
builder.Services.AddSignalR();

// Povolíme připojení z našeho lokálního frontendu (Live Server běží na portu 5500)
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://127.0.0.1:5500", "http://localhost:5500") 
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

app.UseCors();
// Nasadíme náš komunikační Hub na tuto adresu
app.MapHub<RoguelikeCardGame.Hubs.GameHub>("/gamehub");

app.Run();