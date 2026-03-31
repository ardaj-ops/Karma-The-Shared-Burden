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

let myHp = 0;
let myMaxHp = 0;
let myBlock = 0;

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
    relicsContainer.innerHTML = ""; 
    
    if (relicsList.length === 0) {
        relicsContainer.innerText = "Zatím žádné";
        return;
    }
    
    relicsList.forEach(relic => {
        const span = document.createElement("span");
        span.innerText = `[${relic.name}] `;
        span.title = relic.description; 
        span.style.cursor = "help";
        span.style.borderBottom = "2px dotted #2c3e50";
        span.style.marginRight = "10px";
        relicsContainer.appendChild(span);
    });
});

// --- 3. BITEVNÍ SYSTÉM (Více nepřátel) ---
connection.on("ReceiveInitialState", (hand, mana, serverCards, gold, drawCount, discardCount, hp, maxHp, block) => {
    cardDatabase = serverCards; 
    myHand = hand;
    myMana = mana;
    myGold = gold || 0;
    myDrawPileCount = drawCount || 0;
    myDiscardPileCount = discardCount || 0;
    myHp = hp || 0;
    myMaxHp = maxHp || 0;
    myBlock = block || 0;
    
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

// UPRAVENO PRO SKUPINU NEPŘÁTEL
connection.on("TurnResolved", (summary, totalDamage, newKarma, enemiesArray) => {
    logMessage(`--- TAH VYHODNOCEN ---`);
    logMessage(`💥 Uštědřili jste ${totalDamage} plošného DMG!`);
    
    renderEnemies(enemiesArray);
    updateKarmaUI(newKarma);
});

// UPRAVENO PRO SKUPINU NEPŘÁTEL
connection.on("ReceiveNewTurnState", (updatedHand, updatedMana, updatedGold, drawCount, discardCount, hp, maxHp, block, enemiesArray) => {
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
    
    renderEnemies(enemiesArray);
    updateStatsUI();
    renderHand(); 
});

// UPRAVENO PRO SKUPINU NEPŘÁTEL
connection.on("EnteredNode", (nodeType, nodeData, enemiesArray) => {
    logMessage(`📍 Vstupujete do: ${nodeType}`);
    
    if (nodeType === "Encounter" || nodeType === "EliteEncounter" || nodeType === "Boss") {
        renderEnemies(enemiesArray);
        toggleUI("battle");
    } else if (nodeType === "Treasure" || nodeType === "RestPlace") {
        toggleUI("map");
        renderMap();
    }
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
    
    const cardData = cardDatabase[cardId];
    const cardCost = cardData ? cardData.cost : 1;
    if (myMana < cardCost) { alert("Nemáš dostatek Many!"); return; }

    myMana -= cardCost; 
    
    const cardIndex = myHand.indexOf(cardId);
    if (cardIndex > -1) {
        myHand.splice(cardIndex, 1); 
        myDiscardPileCount++; 
    }

    if(cardData) {
        myBlock += (cardData.block || 0);
        myHp += (cardData.heal || 0);
        if(myHp > myMaxHp) myHp = myMaxHp;
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

// --- NOVÉ: SYSTÉM ODMĚN ---
let currentRelicReward = null; 

connection.on("ShowRewardScreen", (cardChoices, relicChoice) => {
    logMessage(`🎉 Výhra! Vyber si odměnu.`);
    toggleUI("reward");

    currentRelicReward = relicChoice;

    const relicContainer = document.getElementById("relic-reward-container");
    if (relicChoice) {
        relicContainer.style.display = "block";
        document.getElementById("reward-relic-name").innerText = relicChoice.name;
        document.getElementById("reward-relic-desc").innerText = relicChoice.description;
    } else {
        relicContainer.style.display = "none";
    }

    const cardContainer = document.getElementById("card-reward-container");
    cardContainer.innerHTML = "";

    cardChoices.forEach(card => {
        const btn = document.createElement("button");
        let color = "#ecf0f1"; 
        if(card.karmaShift < 0) color = "#ffcccc"; 
        if(card.karmaShift > 0) color = "#ccffcc"; 

        btn.innerHTML = `<strong>${card.name}</strong><br><em>${card.cost} Many</em><br><hr style="margin:5px 0;"><small>${card.description}</small>`;
        btn.style.padding = "15px";
        btn.style.width = "160px";
        btn.style.border = "3px solid #34495e";
        btn.style.borderRadius = "8px";
        btn.style.backgroundColor = color;
        btn.style.cursor = "pointer";
        btn.style.transition = "transform 0.2s";
        
        btn.onmouseover = () => btn.style.transform = "scale(1.05)";
        btn.onmouseout = () => btn.style.transform = "scale(1)";

        btn.onclick = () => {
            let relId = currentRelicReward ? currentRelicReward.id : "";
            let relName = currentRelicReward ? currentRelicReward.name : "";
            let relDesc = currentRelicReward ? currentRelicReward.description : "";
            
            connection.invoke("ClaimReward", currentRoomName, playerName, card.id, relId, relName, relDesc)
                .catch(err => console.error(err));
        };
        
        cardContainer.appendChild(btn);
    });
});

connection.on("RewardClaimed", () => {
    toggleUI("map");
    renderMap();
});

function skipReward() {
    let relId = currentRelicReward ? currentRelicReward.id : "";
    let relName = currentRelicReward ? currentRelicReward.name : "";
    let relDesc = currentRelicReward ? currentRelicReward.description : "";
    
    connection.invoke("ClaimReward", currentRoomName, playerName, "", relId, relName, relDesc)
        .catch(err => console.error(err));
}

// --- 5. VYKRESLOVÁNÍ UI ---
function renderEnemies(enemiesArray) {
    const container = document.getElementById("enemies-container");
    if (!container) return;
    container.innerHTML = "";
    
    if (!enemiesArray || enemiesArray.length === 0) return;

    enemiesArray.forEach(e => {
        if (e.hp <= 0) return; 
        
        const div = document.createElement("div");
        div.style.background = "#c0392b";
        div.style.padding = "10px";
        div.style.borderRadius = "5px";
        div.style.color = "white";
        div.style.textAlign = "center";
        div.style.minWidth = "160px";
        div.style.boxShadow = "0 4px 6px rgba(0,0,0,0.3)";
        
        div.innerHTML = `
            <h3 style="margin: 0 0 5px 0; font-size: 18px;">${e.name}</h3>
            <div style="font-weight: bold;">${e.hp} / ${e.maxHp} HP</div>
            <div style="margin-top: 8px; font-size: 13px; background: rgba(0,0,0,0.4); padding: 5px; border-radius: 3px; color: #f1c40f;">
                ${e.currentAction.intentDescription}
            </div>
        `;
        container.appendChild(div);
    });
}

function updateStatsUI() {
    const manaElement = document.getElementById("mana-value");
    if (manaElement) manaElement.innerText = myMana;

    const goldElement = document.getElementById("gold-value");
    if (goldElement) goldElement.innerText = myGold;

    const deckElement = document.getElementById("deck-count");
    if (deckElement) deckElement.innerText = `🎴 V balíčku: ${myDrawPileCount}`;

    const discardElement = document.getElementById("discard-count");
    if (discardElement) discardElement.innerText = `🗑️ Odhozeno: ${myDiscardPileCount}`;

    const hpElement = document.getElementById("hero-hp");
    if (hpElement) hpElement.innerText = myHp;

    const maxHpElement = document.getElementById("hero-max-hp");
    if (maxHpElement) maxHpElement.innerText = myMaxHp;

    const blockElement = document.getElementById("hero-block");
    if (blockElement) blockElement.innerText = myBlock;
}

function updateKarmaUI(karma) {
    const karmaValEl = document.getElementById("karma-value");
    if(karmaValEl) karmaValEl.innerText = karma;

    let statusText = " (Rovnováha ⚖️)";
    let statusColor = "white";
    
    if (karma <= -10) { statusText = " (Hluboká Temnota 🌑)"; statusColor = "#e74c3c"; }
    else if (karma < 0) { statusText = " (Příklon ke stínu 🌘)"; statusColor = "#e67e22"; }
    else if (karma >= 10) { statusText = " (Čisté Světlo ☀️)"; statusColor = "#f1c40f"; }
    else if (karma > 0) { statusText = " (Příklon ke světlu 🌤️)"; statusColor = "#f39c12"; }
    
    const statusEl = document.getElementById("karma-status");
    if(statusEl) {
        statusEl.innerText = statusText;
        statusEl.style.color = statusColor;
    }
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
    const rewardScreen = document.getElementById("reward-screen");
    
    if(battleHUD) battleHUD.style.display = "none";
    if(handWrapper) handWrapper.style.display = "none";
    if(mapUI) mapUI.style.display = "none";
    if(rewardScreen) rewardScreen.style.display = "none";

    if (state === "battle") {
        if(battleHUD) battleHUD.style.display = "block";
        if(handWrapper) handWrapper.style.display = "block";
    } else if (state === "map") {
        if(mapUI) mapUI.style.display = "block";
    } else if (state === "reward") {
        if(rewardScreen) rewardScreen.style.display = "block";
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