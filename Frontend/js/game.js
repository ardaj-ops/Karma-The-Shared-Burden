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

// NOVÉ: Přijímání stavů všech hráčů z týmu
connection.on("UpdateTeamStats", (teamData) => {
    renderTeam(teamData);
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

connection.on("TurnResolved", (summary, totalDamage, newKarma, enemiesArray) => {
    logMessage(`--- TAH VYHODNOCEN ---`);
    logMessage(`💥 Uštědřili jste ${totalDamage} plošného DMG!`);
    
    renderEnemies(enemiesArray);
    updateKarmaUI(newKarma);
});

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

// --- SYSTÉM ODMĚN ---
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

// NOVÉ: Vykreslení celého týmu
function renderTeam(teamData) {
    const container = document.getElementById("team-container");
    if (!container) return;
    container.innerHTML = "";

    teamData.forEach(player => {
        const isMe = player.name === playerName;
        const div = document.createElement("div");
        
        // Zelená pro tebe, modrá pro ostatní spoluhráče
        div.style.background = isMe ? "#27ae60" : "#2980b9";
        div.style.color = "white";
        div.style.padding = "8px 15px";
        div.style.borderRadius = "5px";
        div.style.boxShadow = "0 2px 4px rgba(0,0,0,0.3)";
        div.style.textAlign = "center";
        div.style.minWidth = "120px";

        div.innerHTML = `
            <div style="font-weight: bold; font-size: 16px; border-bottom: 1px solid rgba(255,255,255,0.3); margin-bottom: 5px;">${player.name}</div>
            <div style="font-size: 14px;">❤️ ${player.hp} / ${player.maxHp}</div>
            <div style="font-size: 14px; color: #ecf0f1;">🛡️ Blok: ${player.block}</div>
        `;
        container.appendChild(div);
    });
}

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

// Globální proměnná pro aktuální polohu (pro UI mapy)
let myCurrentNodeId = -1;

connection.on("GameStarted", (roomName, initialMap) => {
    document.getElementById("waiting-screen").style.display = "none";
    document.getElementById("game-screen").style.display = "block";
    
    gameMap = initialMap;
    myCurrentNodeId = -1; // Reset při nové hře
    logMessage(`🔥 Hra začala! Vyberte si startovní políčko na dně mapy.`);
    toggleUI("map");
    renderMap();
});

connection.on("EnteredNode", (nodeType, nodeData, enemiesArray) => {
    logMessage(`📍 Vstupujete do: ${nodeType}`);
    myCurrentNodeId = nodeData.id; // Uložíme aktuální pozici
    
    if (nodeType === "Encounter" || nodeType === "EliteEncounter" || nodeType === "Boss") {
        renderEnemies(enemiesArray);
        toggleUI("battle");
    } else {
        // Shop, RestPlace, Event, Treasure
        logMessage(`⏳ Návštěva: ${nodeType}. Brzy zde bude UI. Zatím pokračujeme.`);
        // Označíme lokálně jako completed
        let mapNode = gameMap.find(n => n.id === nodeData.id);
        if(mapNode) mapNode.isCompleted = true;
        
        toggleUI("map");
        renderMap();
    }
});
// --- VYLEPŠENÁ MAPA (Stromová struktura s čarami) ---
function renderMap() {
    const mapContainer = document.getElementById("nodes-list");
    if (!mapContainer) return;
    
    mapContainer.innerHTML = "";
    mapContainer.style.position = "relative";
    mapContainer.style.display = "flex";
    mapContainer.style.flexDirection = "column-reverse"; // Odspodu nahoru
    mapContainer.style.gap = "40px";
    mapContainer.style.padding = "20px";

    // 1. Zjistíme, v jakém uzlu stojíme
    let currentNode = gameMap.find(n => n.id === myCurrentNodeId);
    let validNextNodeIds = currentNode ? currentNode.connectedTo : [];

    // 2. Seskupíme uzly podle pater (Floors)
    const maxFloor = Math.max(...gameMap.map(n => n.floor));
    
    for (let f = 0; f <= maxFloor; f++) {
        const floorNodes = gameMap.filter(n => n.floor === f);
        
        const row = document.createElement("div");
        row.style.display = "flex";
        row.style.justifyContent = "center";
        row.style.gap = "60px";
        row.style.zIndex = "2"; // Nad čarami
        
        floorNodes.forEach(node => {
            const btn = document.createElement("button");
            btn.id = `node-${node.id}`;
            
            let icon = "❓";
            if (node.type === "Encounter") icon = "⚔️";
            else if (node.type === "EliteEncounter") icon = "👹";
            else if (node.type === "Boss") icon = "👑";
            else if (node.type === "Treasure") icon = "💰";
            else if (node.type === "RestPlace") icon = "⛺";
            else if (node.type === "Shop") icon = "🛒";
            else if (node.type === "Event") icon = "📜";

            btn.innerHTML = `<div style="font-size: 24px;">${icon}</div>`;
            btn.style.padding = "10px";
            btn.style.borderRadius = "50%";
            btn.style.border = "3px solid #2c3e50";
            btn.style.width = "60px";
            btn.style.height = "60px";
            btn.style.display = "flex";
            btn.style.justifyContent = "center";
            btn.style.alignItems = "center";
            btn.style.position = "relative";
            btn.style.transition = "all 0.2s";
            
            // Logika povolení kliknutí (Jen aktuálně napojená patra nebo start)
            let isClickable = false;
            if (myCurrentNodeId === -1 && node.floor === 0) isClickable = true; // První volba
            if (validNextNodeIds.includes(node.id) && !node.isCompleted) isClickable = true; // Další krok

            if (node.isCompleted || myCurrentNodeId === node.id) {
                // Prošlé nebo aktuální
                btn.style.backgroundColor = (myCurrentNodeId === node.id) ? "#f39c12" : "#7f8c8d"; 
                btn.style.borderColor = "#95a5a6";
                btn.disabled = true;
            } else if (isClickable) {
                // Dostupné další kroky
                btn.style.backgroundColor = "#2ecc71"; 
                btn.style.cursor = "pointer";
                btn.style.boxShadow = "0 0 15px rgba(46, 204, 113, 0.8)";
                btn.onmouseover = () => btn.style.transform = "scale(1.2)";
                btn.onmouseout = () => btn.style.transform = "scale(1)";
                btn.onclick = () => {
                    connection.invoke("MoveToNextNode", currentRoomName, node.id).catch(err => console.error(err));
                };
            } else {
                // Vzdálená budoucnost
                btn.style.backgroundColor = "#34495e";
                btn.style.opacity = "0.5";
                btn.disabled = true;
            }
            
            row.appendChild(btn);
        });
        mapContainer.appendChild(row);
    }

    // 3. Vykreslení spojovacích čar (SVG) po vložení do DOMu
    setTimeout(() => drawMapLines(mapContainer), 100);
}

function drawMapLines(container) {
    // Odstraní staré plátno, pokud existuje
    let oldSvg = document.getElementById("map-svg");
    if (oldSvg) oldSvg.remove();

    const svg = document.createElementNS("http://www.w3.org/2000/svg", "svg");
    svg.id = "map-svg";
    svg.style.position = "absolute";
    svg.style.top = "0";
    svg.style.left = "0";
    svg.style.width = "100%";
    svg.style.height = "100%";
    svg.style.zIndex = "1";
    svg.style.pointerEvents = "none";

    const containerRect = container.getBoundingClientRect();

    gameMap.forEach(node => {
        const fromEl = document.getElementById(`node-${node.id}`);
        if (!fromEl) return;
        const fromRect = fromEl.getBoundingClientRect();

        node.connectedTo.forEach(targetId => {
            const toEl = document.getElementById(`node-${targetId}`);
            if (!toEl) return;
            const toRect = toEl.getBoundingClientRect();

            // Výpočet pozic relativně ke kontejneru
            const x1 = (fromRect.left + fromRect.width / 2) - containerRect.left;
            const y1 = (fromRect.top + fromRect.height / 2) - containerRect.top;
            const x2 = (toRect.left + toRect.width / 2) - containerRect.left;
            const y2 = (toRect.top + toRect.height / 2) - containerRect.top;

            const line = document.createElementNS("http://www.w3.org/2000/svg", "line");
            line.setAttribute("x1", x1);
            line.setAttribute("y1", y1);
            line.setAttribute("x2", x2);
            line.setAttribute("y2", y2);
            line.setAttribute("stroke", "rgba(255, 255, 255, 0.3)");
            line.setAttribute("stroke-width", "3");

            // Zvýrazníme čáru, pokud je to naše dostupná cesta
            if (myCurrentNodeId === node.id || (myCurrentNodeId === -1 && node.floor === 0)) {
                 line.setAttribute("stroke", "rgba(46, 204, 113, 0.6)");
                 line.setAttribute("stroke-width", "5");
            }

            svg.appendChild(line);
        });
    });

    container.appendChild(svg);
}