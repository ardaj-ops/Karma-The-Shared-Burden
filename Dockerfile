# 1. Fáze: Sestavení (Build)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
# Zkopírujeme projekt a stáhneme závislosti
COPY ["Backend/Backend.csproj", "Backend/"]
RUN dotnet restore "Backend/Backend.csproj"
# Zkopírujeme zbytek kódu a sestavíme
COPY Backend/. Backend/
RUN dotnet publish "Backend/Backend.csproj" -c Release -o /app/publish

# 2. Fáze: Spuštění
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
# Nastavíme port, který Render vyžaduje
ENV ASPNETCORE_URLS=http://+:10000 
ENTRYPOINT ["dotnet", "Backend.dll"]