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
    location.reload(); // Vrátí ho zpět
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
    
    // Necháme hráče v klidu koukat na mapu, nespouštíme hned bitvu!
    toggleUI("map");
    renderMap();
});

// --- 3. BITEVNÍ SYSTÉM ---
connection.on("ReceiveInitialState", (hand, mana, serverCards) => {
    cardDatabase = serverCards; 
    myHand = hand;
    myMana = mana;
    updateManaUI();
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

connection.on("ReceiveNewTurnState", (updatedHand, updatedMana) => {
    myHand = updatedHand;
    myMana = updatedMana;
    turnEnded = false; // Můžeme zase hrát
    logMessage(`🔄 Začíná tvůj nový tah!`);
    
    document.getElementById("end-turn-btn").disabled = false;
    document.getElementById("end-turn-btn").style.backgroundColor = "#8e44ad";
    
    updateManaUI();
    renderHand(); 
});

connection.on("EnteredNode", (nodeType, nodeData) => {
    logMessage(`📍 Vstupujete do: ${nodeType}`);
    if (nodeType === "Encounter" || nodeType === "EliteEncounter" || nodeType === "Boss") {
        toggleUI("battle");
    } else if (nodeType === "Treasure" || nodeType === "RestPlace") {
        toggleUI("map");
        renderMap();
    }
});

connection.on("ReceiveTreasure", (relicName) => {
    logMessage(`🏆 Tým získal relikvii: ${relicName}`);
    document.getElementById("relics-list").innerText += ` [${relicName}] `;
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

    // Odečteme manu a kartu odstraníme z ruky u nás
    myMana -= cardCost; 
    myHand = myHand.filter(id => id !== cardId); 
    
    updateManaUI();
    renderHand(); 

    // Pošleme na server, ale server zatím NEVYHODNOCUJE TAH!
    connection.invoke("SelectCard", currentRoomName, playerName, cardId, karmaShift, damage)
        .catch(err => console.error(err));
}

function endTurn() {
    if (turnEnded) return;
    turnEnded = true;
    
    // Vizuálně vypneme tlačítko
    document.getElementById("end-turn-btn").disabled = true;
    document.getElementById("end-turn-btn").style.backgroundColor = "gray";
    
    connection.invoke("PlayerReady", currentRoomName, playerName)
        .catch(err => console.error(err));
}

// --- 5. VYKRESLOVÁNÍ UI ---
function updateManaUI() {
    const manaElement = document.getElementById("mana-value");
    if (manaElement) manaElement.innerText = myMana;
}

function renderHand() {
    const handContainer = document.getElementById("hand-container");
    if (!handContainer) return; 

    handContainer.innerHTML = ""; 

    myHand.forEach(cardId => {
        const cardData = cardDatabase[cardId] || { name: cardId, cost: 1, karmaShift: 0, damage: 5, description: "" };
        const cardElement = document.createElement("button");
        cardElement.className = "card"; 
        
        let color = "#ecf0f1"; // Základní šedá
        if(cardData.karmaShift < 0) color = "#ffcccc"; // Červená pro temnotu
        if(cardData.karmaShift > 0) color = "#ccffcc"; // Zelená pro světlo

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

function renderMap() {
    const mapContainer = document.getElementById("nodes-list");
    if (!mapContainer) return;
    
    mapContainer.innerHTML = "";
    
    gameMap.forEach(node => {
        const btn = document.createElement("button");
        btn.innerText = `Patro ${node.floor}: ${node.type}`;
        btn.style.padding = "15px";
        btn.style.borderRadius = "5px";
        btn.style.border = "none";
        btn.style.color = "white";
        btn.style.fontWeight = "bold";
        
        if (node.isCompleted) {
            btn.style.backgroundColor = "gray"; 
            btn.disabled = true;
        } else {
            btn.style.backgroundColor = "#2ecc71"; 
            btn.style.cursor = "pointer";
            btn.onclick = () => {
                connection.invoke("MoveToNextNode", currentRoomName, node.id).catch(err => console.error(err));
            };
        }
        
        mapContainer.appendChild(btn);
    });
}