// Pokud má tvůj GameHub jiný namespace (např. KarmaGame.Hubs), 
// uprav tento using podle sebe, nebo ho smaž, pokud VS nenahlásí chybu.
using Backend.Hubs; 

var builder = WebApplication.CreateBuilder(args);

// 1. PŘIDÁNÍ SIGNALR (Pro herní komunikaci)
builder.Services.AddSignalR();

// 2. NASTAVENÍ CORS (Aby tě prohlížeče neblokovaly při lokálním testování)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .SetIsOriginAllowed((host) => true) // Povolí připojení odkudkoliv
              .AllowCredentials();
    });
});

var app = builder.Build();

// 3. POUŽITÍ CORS
app.UseCors("AllowAll");

// 4. SERVÍROVÁNÍ FRONTENDU (Tohle pošle index.html ze složky wwwroot)
app.UseDefaultFiles(); // Zajistí, že při zadání adresy webu se automaticky načte index.html
app.UseStaticFiles();  // Povolí načítání CSS, JS a obrázků z wwwroot

// 5. NAMAPOVÁNÍ HERNÍHO HUBU
app.MapHub<GameHub>("/gamehub");

// 6. SPUŠTĚNÍ SERVERU
app.Run();