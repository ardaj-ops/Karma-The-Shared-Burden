const connection = new signalR.HubConnectionBuilder()
    .withUrl("https://karma-the-shared-burden.onrender.com/gamehub") 
    .build();

let playerName = ""; 
let playerClass = ""; 
let currentRoomName = ""; 
let isGameOver = false; 
let turnEnded = false; 
let gameMap = []; 
let cardDatabase = {}; 

let myHand = []; 
let myMana = 0;  
let myGold = 0;
let myDrawPileCount = 0;
let myDiscardPileCount = 0;

// --- 1. LOBBY A PŘIPOJENÍ ---
function pickHero(hero) {
    playerClass = hero;
    document.getElementById("selected-hero-text").innerText = hero;
}

function getCredentials() {
    playerName = document.getElementById("player-name-input").value.trim();
    currentRoomName = document.getElementById("room-name-input").value.trim();
    
    if (!playerName || !currentRoomName || !playerClass) {
        alert("Musíš zadat jméno, název místnosti a vybrat si hrdinu!");
        return false;
    }
    return true;
}

function createLobby() {
    if(!getCredentials()) return;
    connection.start().then(() => {
        connection.invoke("CreateLobby", currentRoomName, playerName, playerClass).catch(err => console.error(err));
        showWaitingRoom();
    });
}

function joinLobby() {
    if(!getCredentials()) return;
    connection.start().then(() => {
        connection.invoke("JoinLobby", currentRoomName, playerName, playerClass).catch(err => console.error(err));
        showWaitingRoom();
    });
}

function startGame() {
    connection.invoke("StartGame", currentRoomName).catch(err => console.error(err));
}

function showWaitingRoom() {
    document.getElementById("lobby-screen").style.display = "none";
    document.getElementById("waiting-screen").style.display = "block";
    document.getElementById("display-room-name").innerText = currentRoomName;
}

// --- 2. SIGNALR UDÁLOSTI Z LOBBY A MAPY ---
connection.on("LobbyError", (msg) => {
    alert(msg);
    location.reload(); 
});

connection.on("LobbyUpdate", (players) => {
    const list = document.getElementById("lobby-players-list");
    list.innerHTML = "";
    players.forEach(p => {
        const li = document.createElement("li");
        li.innerText = "🧙‍♂️ " + p;
        list.appendChild(li);
    });
});

connection.on("YouAreHost", () => {
    document.getElementById("start-game-btn").style.display = "inline-block";
    document.getElementById("waiting-text").style.display = "none";
});

connection.on("GameStarted", (roomName, initialMap) => {
    document.getElementById("waiting-screen").style.display = "none";
    document.getElementById("game-screen").style.display = "block";
    
    gameMap = initialMap;
    logMessage(`🔥 Hra začala! Otevřete mapu a vyberte první místnost.`);
    toggleUI("map");
    renderMap();
});

connection.on("UpdateRelics", (relicsList) => {
    const relicsContainer = document.getElementById("relics-list");
    relicsContainer.innerHTML = ""; // Vyčistíme text
    
    if (relicsList.length === 0) {
        relicsContainer.innerText = "Zatím žádné";
        return;
    }
    
    // Projdeme všechny relikvie ze serveru
    relicsList.forEach(relic => {
        const span = document.createElement("span");
        span.innerText = `[${relic.name}] `;
        
        // Zde je to kouzlo: title vytvoří nativní tooltip při najetí myší!
        span.title = relic.description; 
        
        // Přidáme trošku stylu, aby hráč věděl, že na to má najet myší
        span.style.cursor = "help";
        span.style.borderBottom = "2px dotted #2c3e50";
        span.style.marginRight = "10px";
        
        relicsContainer.appendChild(span);
    });
});

// --- 3. BITEVNÍ SYSTÉM ---
connection.on("ReceiveInitialState", (hand, mana, serverCards, gold, drawCount, discardCount) => {
    cardDatabase = serverCards; 
    myHand = hand;
    myMana = mana;
    myGold = gold || 0;
    myDrawPileCount = drawCount || 0;
    myDiscardPileCount = discardCount || 0;
    
    updateStatsUI();
    renderHand(); 
});

connection.on("CardPlayedLog", (player, cardId) => {
    let cardName = cardDatabase[cardId] ? cardDatabase[cardId].name : cardId;
    logMessage(`🎴 ${player} zahrál: ${cardName}`);
});

connection.on("PlayerReadyLog", (player, readyCount, totalPlayers) => {
    logMessage(`✅ ${player} ukončil tah. (${readyCount}/${totalPlayers})`);
});

connection.on("TurnResolved", (summary, totalDamage, newKarma, enemyHp) => {
    logMessage(`--- TAH VYHODNOCEN ---`);
    logMessage(`💥 Nepřítel utrpěl ${totalDamage} DMG! Zbývá mu ${enemyHp} HP.`);
    
    document.getElementById("enemy-hp").innerText = enemyHp;
    document.getElementById("karma-value").innerText = newKarma;
});

connection.on("ReceiveNewTurnState", (updatedHand, updatedMana, updatedGold, drawCount, discardCount, hp, maxHp, block, intentDescription) => {
    myHand = updatedHand;
    myMana = updatedMana;
    myGold = updatedGold || 0;
    myDrawPileCount = drawCount || 0;
    myDiscardPileCount = discardCount || 0;
    myHp = hp || 0;
    myMaxHp = maxHp || 0;
    myBlock = block || 0;
    
    turnEnded = false; 
    logMessage(`🔄 Začíná tvůj nový tah! Dobrány karty do 3.`);
    
    document.getElementById("end-turn-btn").disabled = false;
    document.getElementById("end-turn-btn").style.backgroundColor = "#8e44ad";
    
    // Zobrazíme novou myšlenku nepřítele pro tohle kolo
    if(document.getElementById("enemy-intent") && intentDescription) {
        document.getElementById("enemy-intent").innerText = intentDescription;
    }
    
    updateStatsUI();
    renderHand(); 
});

connection.on("EnteredNode", (nodeType, nodeData, enemyName, enemyHp, enemyMaxHp, intentDescription) => {
    logMessage(`📍 Vstupujete do: ${nodeType}`);
    
    if (nodeType === "Encounter" || nodeType === "EliteEncounter" || nodeType === "Boss") {
        document.getElementById("enemy-name").innerText = enemyName;
        document.getElementById("enemy-hp").innerText = enemyHp;
        document.getElementById("enemy-max-hp").innerText = enemyMaxHp;
        
        // Zobrazíme myšlenku nepřítele
        if(document.getElementById("enemy-intent")) {
            document.getElementById("enemy-intent").innerText = intentDescription;
        }

        toggleUI("battle");
    } else if (nodeType === "Treasure" || nodeType === "RestPlace") {
        toggleUI("map");
        renderMap();
    }
});

connection.on("ReceiveTreasure", (relicName) => {
    logMessage(`🏆 Tým získal relikvii: ${relicName}`);
    document.getElementById("relics-list").innerText += ` | ${relicName}`;
});

connection.on("BattleWon", (message) => {
    logMessage(`🎉 ${message}`);
    turnEnded = false;
    toggleUI("map");
    renderMap();
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

// --- 4. AKCE HRÁČE ---
function playCard(cardId, karmaShift, damage) {
    if (isGameOver) return; 
    if (turnEnded) { alert("Už jsi ukončil tah!"); return; }
    
    const cardCost = cardDatabase[cardId] ? cardDatabase[cardId].cost : 1;
    if (myMana < cardCost) { alert("Nemáš dostatek Many!"); return; }

    myMana -= cardCost; 
    
    // Odstranění z ruky a přesun do odhazovacího balíčku
    const cardIndex = myHand.indexOf(cardId);
    if (cardIndex > -1) {
        myHand.splice(cardIndex, 1); 
        myDiscardPileCount++; 
    }
    
    updateStatsUI();
    renderHand(); 

    connection.invoke("SelectCard", currentRoomName, playerName, cardId, karmaShift, damage)
        .catch(err => console.error(err));
}

function endTurn() {
    if (turnEnded) return;
    turnEnded = true;
    
    document.getElementById("end-turn-btn").disabled = true;
    document.getElementById("end-turn-btn").style.backgroundColor = "gray";
    
    connection.invoke("PlayerReady", currentRoomName, playerName)
        .catch(err => console.error(err));
}

// --- 5. VYKRESLOVÁNÍ UI ---
function updateStatsUI() {
    const manaElement = document.getElementById("mana-value");
    if (manaElement) manaElement.innerText = myMana;

    const goldElement = document.getElementById("gold-value");
    if (goldElement) goldElement.innerText = myGold;

    const deckElement = document.getElementById("deck-count");
    if (deckElement) deckElement.innerText = `🎴 V balíčku: ${myDrawPileCount}`;

    const discardElement = document.getElementById("discard-count");
    if (discardElement) discardElement.innerText = `🗑️ Odhozeno: ${myDiscardPileCount}`;
}

function renderHand() {
    const handContainer = document.getElementById("hand-container");
    if (!handContainer) return; 

    handContainer.innerHTML = ""; 

    myHand.forEach(cardId => {
        const cardData = cardDatabase[cardId] || { name: cardId, cost: 1, karmaShift: 0, damage: 5, description: "" };
        const cardElement = document.createElement("button");
        cardElement.className = "card"; 
        
        let color = "#ecf0f1"; 
        if(cardData.karmaShift < 0) color = "#ffcccc"; 
        if(cardData.karmaShift > 0) color = "#ccffcc"; 

        cardElement.innerHTML = `<strong>${cardData.name}</strong><br><em>${cardData.cost} Many</em><br><hr style="margin:5px 0;"><small>${cardData.description}</small>`;
        cardElement.onclick = () => playCard(cardId, cardData.karmaShift, cardData.damage);
        
        cardElement.style.padding = "10px";
        cardElement.style.width = "140px";
        cardElement.style.border = "2px solid #34495e";
        cardElement.style.borderRadius = "8px";
        cardElement.style.backgroundColor = color;
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

// --- VYLEPŠENÁ MAPA ---
function renderMap() {
    const mapContainer = document.getElementById("nodes-list");
    if (!mapContainer) return;
    
    mapContainer.innerHTML = "";
    
    gameMap.forEach((node, index) => {
        if (index > 0) {
            const arrow = document.createElement("div");
            arrow.innerText = "➔";
            arrow.style.color = "white";
            arrow.style.fontSize = "24px";
            arrow.style.alignSelf = "center";
            mapContainer.appendChild(arrow);
        }

        const btn = document.createElement("button");
        
        let icon = "❓";
        if (node.type === "Encounter") icon = "⚔️";
        else if (node.type === "EliteEncounter") icon = "👹";
        else if (node.type === "Boss") icon = "👑";
        else if (node.type === "Treasure") icon = "💰";
        else if (node.type === "RestPlace") icon = "⛺";

        btn.innerHTML = `<div style="font-size: 24px;">${icon}</div><div style="font-size: 12px; margin-top: 5px;">Patro ${node.floor}</div>`;
        btn.style.padding = "10px";
        btn.style.borderRadius = "50%";
        btn.style.border = "3px solid #2c3e50";
        btn.style.color = "white";
        btn.style.width = "80px";
        btn.style.height = "80px";
        btn.style.display = "flex";
        btn.style.flexDirection = "column";
        btn.style.alignItems = "center";
        btn.style.justifyContent = "center";
        btn.style.transition = "transform 0.2s";
        
        if (node.isCompleted) {
            btn.style.backgroundColor = "#7f8c8d"; 
            btn.style.borderColor = "#95a5a6";
            btn.disabled = true;
        } else {
            btn.style.backgroundColor = "#2ecc71"; 
            btn.style.cursor = "pointer";
            btn.style.boxShadow = "0 0 10px rgba(46, 204, 113, 0.5)";
            btn.onmouseover = () => btn.style.transform = "scale(1.1)";
            btn.onmouseout = () => btn.style.transform = "scale(1)";
            btn.onclick = () => {
                connection.invoke("MoveToNextNode", currentRoomName, node.id).catch(err => console.error(err));
            };
        }
        
        mapContainer.appendChild(btn);
    });
}