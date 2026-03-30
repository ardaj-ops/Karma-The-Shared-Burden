// 1. PŘIPOJENÍ K TVÉMU SERVERU NA RENDERU
const connection = new signalR.HubConnectionBuilder()
    .withUrl("https://karma-the-shared-burden.onrender.com/gamehub") 
    .build();

// 2. ZÁKLADNÍ PROMĚNNÉ
let playerName = ""; 
let playerClass = ""; 
let currentRoomName = ""; 
let hasPlayedThisTurn = false;
let isGameOver = false; 
let gameMap = []; 

// --- DECK A MANA SYSTÉM ---
let myHand = []; 
let myMana = 0;  

// Dočasná databáze karet pro front-end (aby karty měly jména a ceny)
// Až přidáš další karty, jen je sem dopíšeš
let cardDatabase = {}; // Načte se automaticky ze serveru!

// --- VÝBĚR POSTAVY (Spouští se kliknutím v HTML Lobby) ---
function selectCharacter(heroClass) {
    const nameInput = document.getElementById("player-name-input").value;
    
    // Kontrola, zda hráč vyplnil jméno
    if (!nameInput.trim()) {
        alert("Prosím, zadej nejdříve své jméno!");
        return;
    }

    playerName = nameInput;
    playerClass = heroClass;

    // Schováme Lobby a ukážeme herní obrazovku
    document.getElementById("lobby-screen").style.display = "none";
    document.getElementById("game-screen").style.display = "block";

    // AŽ TEĎ se připojíme k serveru a pošleme mu Jméno i Třídu
    connection.start().then(() => {
        logMessage(`Připojeno jako ${playerName} (${playerClass}). Čekám na další hráče...`);
        connection.invoke("JoinGame", playerName, playerClass)
            .catch(err => console.error(err.toString()));
    }).catch(err => console.error(err.toString()));
}

// --- SIGNALR UDÁLOSTI (Odpovědi ze serveru) ---

connection.on("PlayerJoinedWaitingRoom", (player, currentCount, needed) => {
    logMessage(`Čekárna: ${player} se připojil. (${currentCount}/${needed})`);
});

connection.on("GameStarted", (roomName, initialMap) => {
    currentRoomName = roomName;
    gameMap = initialMap;
    logMessage(`🔥 Hra začala! Cesta před vámi se odhaluje.`);
});

// Přidali jsme parametr 'db'
connection.on("ReceiveInitialState", (hand, mana, db) => {
    myHand = hand;
    myMana = mana;
    
    // Tady si hru naučíme celou databázi karet ze serveru!
    cardDatabase = db; 
    
    updateManaUI();
    renderHand(); 
});

connection.on("ReceiveNewTurnState", (updatedHand, updatedMana) => {
    myHand = updatedHand;
    myMana = updatedMana;
    logMessage(`🔄 Začíná nové kolo!`);
    updateManaUI();
    renderHand(); 
    hasPlayedThisTurn = false; 
});

connection.on("PlayerReadiedUp", (player) => {
    logMessage(`⏳ ${player} si vybral kartu a čeká na ostatní...`);
});

connection.on("TurnResolved", (summary, totalDamage, newKarma, enemyHp) => {
    logMessage(`--- TAH VYHODNOCEN ---`);
    summary.forEach(msg => logMessage(`👉 ${msg}`));
    logMessage(`💥 Boss utrpěl ${totalDamage} DMG! Zbývá mu ${enemyHp} HP.`);
    
    document.getElementById("enemy-hp").innerText = enemyHp;
    document.getElementById("karma-value").innerText = newKarma;
});

connection.on("BattleWon", (message) => {
    logMessage(`🎉 ${message}`);
    hasPlayedThisTurn = false;
    toggleUI("map");
    renderMap();
});

connection.on("EnteredNode", (nodeType, nodeData) => {
    logMessage(`📍 Jste v: ${nodeType}`);
    // Pokud jsme v boji, ukážeme karty. Jinak ukážeme mapu.
    if (nodeType === "Encounter" || nodeType === "EliteEncounter" || nodeType === "Boss") {
        toggleUI("battle");
    } else {
        toggleUI("map");
        renderMap();
    }
});

connection.on("GameOver", (message) => {
    isGameOver = true; 
    logMessage(`🏆 ${message}`);
    alert(message); 
    toggleUI("none"); 
});

function logMessage(message) {
    const li = document.createElement("li");
    li.textContent = message;
    document.getElementById("log").prepend(li);
}

// --- KONTROLA A ZAHRÁNÍ KARTY ---
function playCard(cardId, karmaShift, damage) {
    if (!currentRoomName) { alert("Hra nezačala!"); return; }
    if (isGameOver) { alert("Hra skončila!"); return; }
    if (hasPlayedThisTurn) { alert("Už jsi hrál v tomto kole!"); return; }
    if (!myHand.includes(cardId)) { alert("Kartu nemáš v ruce!"); return; }

    const cardCost = cardDatabase[cardId] ? cardDatabase[cardId].cost : 1;
    if (myMana < cardCost) { alert("Málo Many!"); return; }

    hasPlayedThisTurn = true; 
    myMana -= cardCost; 
    myHand = myHand.filter(id => id !== cardId); 
    
    updateManaUI();
    renderHand(); 

    logMessage(`Karta zahrána, čekám na ostatní hráče...`);
    connection.invoke("SelectCard", currentRoomName, playerName, cardId, karmaShift, damage)
        .catch(err => console.error(err.toString()));
}

// --- POMOCNÉ FUNKCE PRO VYKRESLOVÁNÍ (UI) ---
function updateManaUI() {
    const manaElement = document.getElementById("mana-value");
    if (manaElement) manaElement.innerText = myMana;
}

function renderHand() {
    const handContainer = document.getElementById("hand-container");
    if (!handContainer) return; 

    handContainer.innerHTML = ""; 

    myHand.forEach(cardId => {
        // Pokud kartu nemáme v databázi výše, dáme jí provizorní staty
        const cardData = cardDatabase[cardId] || { name: cardId, cost: 1, karmaShift: 0, damage: 5 };
        
        const cardElement = document.createElement("button");
        cardElement.className = "card"; 
        cardElement.innerHTML = `<strong>${cardData.name}</strong><br><em>${cardData.cost} Many</em><br>Dmg: ${cardData.damage}<br>Karma: ${cardData.karmaShift}`;
        cardElement.onclick = () => playCard(cardId, cardData.karmaShift, cardData.damage);
        
        // Jednoduchý vizuál karty
        cardElement.style.padding = "10px";
        cardElement.style.margin = "5px";
        cardElement.style.border = "2px solid #34495e";
        cardElement.style.borderRadius = "8px";
        cardElement.style.backgroundColor = "#ecf0f1";
        cardElement.style.cursor = "pointer";

        handContainer.appendChild(cardElement);
    });
}

function toggleUI(state) {
    const battleHUD = document.getElementById("battle-hud");
    const handWrapper = document.getElementById("hand-wrapper");
    const mapUI = document.getElementById("map-container"); 
    
    if (state === "battle") {
        if(battleHUD) battleHUD.style.display = "block";
        if(handWrapper) handWrapper.style.display = "block";
        if(mapUI) mapUI.style.display = "none";
    } else if (state === "map") {
        if(battleHUD) battleHUD.style.display = "none";
        if(handWrapper) handWrapper.style.display = "none";
        if(mapUI) mapUI.style.display = "block";
    } else {
        if(battleHUD) battleHUD.style.display = "none";
        if(handWrapper) handWrapper.style.display = "none";
        if(mapUI) mapUI.style.display = "none";
    }
}

function renderMap() {
    const mapContainer = document.getElementById("nodes-list");
    if (!mapContainer) return;
    
    mapContainer.innerHTML = "";
    
    gameMap.forEach(node => {
        const btn = document.createElement("button");
        btn.innerText = `Patro ${node.floor}: ${node.type}`;
        btn.style.padding = "15px";
        btn.style.margin = "5px";
        btn.style.borderRadius = "5px";
        btn.style.border = "none";
        btn.style.color = "white";
        btn.style.fontWeight = "bold";
        
        if (node.isCompleted) {
            btn.style.backgroundColor = "gray"; // Šedá - už projito
            btn.disabled = true;
        } else {
            btn.style.backgroundColor = "#2ecc71"; // Zelená - lze vstoupit
            btn.style.cursor = "pointer";
            btn.onclick = () => {
                logMessage("Pokouším se přejít do další místnosti...");
                connection.invoke("MoveToNextNode", currentRoomName, node.id)
                    .catch(err => console.error(err));
            };
        }
        
        mapContainer.appendChild(btn);
    });
}