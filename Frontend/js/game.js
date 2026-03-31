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

// NOVÉ PRO KARTY
let myDrawPile = [];
let myDiscardPile = [];
let myStartingDeck = [];
let myDrawPileCount = 0;
let myDiscardPileCount = 0;

let myHp = 0;
let myMaxHp = 0;
let myBlock = 0;

let myCurrentNodeId = -1;
let currentEnemiesArray = []; 
let selectedTargetCard = null; 
let currentMapVotes = {}; 

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

// --- 2. SIGNALR UDÁLOSTI ---
connection.on("LobbyError", (msg) => { alert(msg); location.reload(); });

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
    myCurrentNodeId = -1; 
    currentMapVotes = {};
    logMessage(`🔥 Hra začala! Hlasujte pro startovní políčko na dně mapy.`);
    toggleUI("map");
    renderMap();
});

connection.on("UpdateRelics", (relicsList) => {
    const relicsContainer = document.getElementById("relics-list");
    relicsContainer.innerHTML = ""; 
    if (relicsList.length === 0) { relicsContainer.innerText = "Zatím žádné"; return; }
    
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

connection.on("UpdateTeamStats", (teamData) => { renderTeam(teamData); });

connection.on("UpdateMapVotes", (votes) => {
    currentMapVotes = votes;
    renderMap(); 
});

// --- 3. BITEVNÍ SYSTÉM ---
connection.on("ReceiveInitialState", (hand, mana, serverCards, gold, drawPile, discardPile, hp, maxHp, block, startingDeck) => {
    cardDatabase = serverCards; 
    myHand = hand; myMana = mana; myGold = gold || 0;
    
    // Uložení detailních listů místo počtů
    myDrawPile = drawPile || []; 
    myDiscardPile = discardPile || [];
    myStartingDeck = startingDeck || [];
    
    myDrawPileCount = myDrawPile.length; 
    myDiscardPileCount = myDiscardPile.length;
    
    myHp = hp || 0; myMaxHp = maxHp || 0; myBlock = block || 0;
    
    updateStatsUI();
    renderHand(); 
});

connection.on("CardPlayedLog", (player, cardId) => {
    let cardName = cardDatabase[cardId] ? cardDatabase[cardId].name : cardId;
    logMessage(`🎴 ${player} připravil kartu: ${cardName}`);
});

connection.on("PlayerReadyLog", (player, readyCount, totalPlayers) => {
    logMessage(`✅ ${player} ukončil tah. (${readyCount}/${totalPlayers})`);
});

connection.on("TurnResolved", (summary, totalDamage, newKarma, enemiesArray) => {
    logMessage(`--- TAH VYHODNOCEN ---`);
    currentEnemiesArray = enemiesArray;
    renderEnemies(enemiesArray);
    updateKarmaUI(newKarma);
});

connection.on("ReceiveNewTurnState", (updatedHand, updatedMana, updatedGold, drawPile, discardPile, hp, maxHp, block, enemiesArray) => {
    myHand = updatedHand; myMana = updatedMana; myGold = updatedGold || 0;
    
    myDrawPile = drawPile || []; 
    myDiscardPile = discardPile || [];
    myDrawPileCount = myDrawPile.length; 
    myDiscardPileCount = myDiscardPile.length;
    
    myHp = hp || 0; myMaxHp = maxHp || 0; myBlock = block || 0;
    
    turnEnded = false; 
    currentEnemiesArray = enemiesArray;
    selectedTargetCard = null; 
    
    logMessage(`🔄 Začíná tvůj nový tah! Máš 5 nových karet.`);
    
    document.getElementById("end-turn-btn").disabled = false;
    document.getElementById("end-turn-btn").style.backgroundColor = "#8e44ad";
    
    renderEnemies(enemiesArray);
    updateStatsUI();
    renderHand(); 
});

connection.on("EnteredNode", (nodeType, nodeData, enemiesArray) => {
    logMessage(`📍 Vstupujete do: ${nodeType}`);
    myCurrentNodeId = nodeData.id; 
    currentMapVotes = {}; 
    currentEnemiesArray = enemiesArray; // Uložíme i prázdné pole pro nebojová místa
    
    if (nodeType === "Encounter" || nodeType === "EliteEncounter" || nodeType === "Boss") {
        renderEnemies(enemiesArray);
        toggleUI("battle"); // Zapne bitvu
    } else {
        // Shop, Rest, Event, Treasure
        logMessage(`⏳ Návštěva: ${nodeType}. Brzy zde bude speciální UI.`);
        
        let mapNode = gameMap.find(n => n.id === nodeData.id);
        if(mapNode) mapNode.isCompleted = true;

        // I v nebojovém poli chceme vidět HUD (HP, Manu, Karty), tak zapneme "battle" UI
        // ale renderEnemies neudělá nic, protože enemiesArray je prázdné.
        renderEnemies([]); 
        toggleUI("battle"); 
        
        // Přidáme tlačítko pro návrat na mapu (protože v nebojovém poli není konec tahu)
        const btn = document.createElement("button");
        btn.innerText = "🚀 Pokračovat v cestě";
        btn.style.cssText = "padding: 15px 30px; background: #27ae60; color: white; border: none; border-radius: 5px; cursor: pointer; margin-top: 20px;";
        btn.onclick = () => {
            toggleUI("map");
            renderMap();
        };
        document.getElementById("enemies-container").appendChild(btn);
    }
});

connection.on("BattleWon", (message) => {
    logMessage(`🎉 ${message}`);
    turnEnded = false;
    currentMapVotes = {};
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

    if (damage > 0) {
        selectedTargetCard = { id: cardId, karmaShift: karmaShift, damage: damage, cost: cardCost };
        logMessage("🎯 Vyber cíl pro útok (klikni na nepřítele)!");
        renderEnemies(currentEnemiesArray); 
        return;
    } else {
        executeCardPlay(cardId, karmaShift, damage, cardCost, "");
    }
}

function executeCardPlay(cardId, karmaShift, damage, cardCost, targetEnemyId) {
    myMana -= cardCost; 
    
    const cardIndex = myHand.indexOf(cardId);
    if (cardIndex > -1) {
        myHand.splice(cardIndex, 1); 
        myDiscardPileCount++; 
    }

    const cardData = cardDatabase[cardId];
    if(cardData) {
        myBlock += (cardData.block || 0);
        myHp += (cardData.heal || 0);
        if(myHp > myMaxHp) myHp = myMaxHp;
    }
    
    selectedTargetCard = null; 
    
    updateStatsUI();
    renderHand(); 
    renderEnemies(currentEnemiesArray); 

    connection.invoke("SelectCard", currentRoomName, playerName, cardId, karmaShift, damage, targetEnemyId)
        .catch(err => console.error(err));
}

function endTurn() {
    if (turnEnded) return;
    turnEnded = true;
    selectedTargetCard = null; 
    
    document.getElementById("end-turn-btn").disabled = true;
    document.getElementById("end-turn-btn").style.backgroundColor = "gray";
    
    renderEnemies(currentEnemiesArray); 
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

connection.on("RewardClaimed", (updatedDeck) => {
    if(updatedDeck) myStartingDeck = updatedDeck; // Aktualizace balíčku
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

// --- 5. VYKRESLOVÁNÍ UI A KARET (MODAL) ---

function showModalWithCards(title, cardIds) {
    document.getElementById("card-modal-title").innerText = title;
    const container = document.getElementById("card-modal-content");
    container.innerHTML = "";
    
    if (!cardIds || cardIds.length === 0) {
        container.innerHTML = "<p style='color: white;'>Zatím prázdné...</p>";
    } else {
        cardIds.forEach(cardId => {
            const cardData = cardDatabase[cardId] || { name: cardId, cost: 1, karmaShift: 0, damage: 5, description: "" };
            const cardElement = document.createElement("div");
            
            let color = "#ecf0f1"; 
            if(cardData.karmaShift < 0) color = "#ffcccc"; 
            if(cardData.karmaShift > 0) color = "#ccffcc"; 

            cardElement.innerHTML = `<strong>${cardData.name}</strong><br><em>${cardData.cost} Many</em><br><hr style="margin:5px 0;"><small>${cardData.description}</small>`;
            cardElement.style.padding = "10px";
            cardElement.style.width = "120px";
            cardElement.style.minHeight = "150px";
            cardElement.style.border = "2px solid #34495e";
            cardElement.style.borderRadius = "8px";
            cardElement.style.backgroundColor = color;
            cardElement.style.cursor = "default";
            cardElement.style.margin = "10px";
            cardElement.style.display = "flex";
            cardElement.style.flexDirection = "column";
            cardElement.style.color = "#2c3e50";
            cardElement.style.boxShadow = "2px 2px 10px rgba(0,0,0,0.5)";
            
            container.appendChild(cardElement);
        });
    }
    
    document.getElementById("card-modal").style.display = "flex";
}

function closeCardModal() {
    document.getElementById("card-modal").style.display = "none";
}

function showStartingDeck() {
    showModalWithCards("Tvůj kompletní balíček", myStartingDeck);
}

function showDrawPile() {
    showModalWithCards("Karty k líznutí", myDrawPile);
}

function showDiscardPile() {
    showModalWithCards("Odhozené karty", myDiscardPile);
}

function renderTeam(teamData) {
    const container = document.getElementById("team-container");
    if (!container) return;
    container.innerHTML = "";

    teamData.forEach(player => {
        const isMe = player.name === playerName;
        const div = document.createElement("div");
        
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
        div.style.transition = "all 0.3s ease";
        
        if (selectedTargetCard) {
            div.style.cursor = "crosshair";
            div.style.boxShadow = "0 0 20px #e74c3c";
            div.style.transform = "scale(1.05)";
            
            div.onclick = () => {
                executeCardPlay(selectedTargetCard.id, selectedTargetCard.karmaShift, selectedTargetCard.damage, selectedTargetCard.cost, e.id);
            };
        } else {
            div.style.boxShadow = "0 4px 6px rgba(0,0,0,0.3)";
            div.style.cursor = "default";
            div.onclick = null; 
        }
        
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
        
        cardElement.onclick = () => {
            if (selectedTargetCard) {
                logMessage("Nejprve vyber cíl pro předešlou kartu!");
                return;
            }
            playCard(cardId, cardData.karmaShift, cardData.damage);
        };
        
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
-
// --- VYLEPŠENÁ MAPA S IKONAMI A HLASOVÁNÍM ---
// --- VYLEPŠENÁ MAPA S IKONAMI A HLASOVÁNÍM ---
function renderMap() {
    const mapContainer = document.getElementById("nodes-list");
    if (!mapContainer) return;
    
    mapContainer.innerHTML = "";
    mapContainer.style.position = "relative";
    mapContainer.style.display = "flex";
    mapContainer.style.flexDirection = "column-reverse"; // Cesta zdola nahoru
    mapContainer.style.gap = "50px";
    mapContainer.style.padding = "20px";

    // Zjistíme, kde stojíme a kam můžeme jít
    let currentNode = gameMap.find(n => n.id === myCurrentNodeId);
    let validNextNodeIds = currentNode ? currentNode.connectedTo : [];

    const maxFloor = Math.max(...gameMap.map(n => n.floor));
    
    for (let f = 0; f <= maxFloor; f++) {
        const floorNodes = gameMap.filter(n => n.floor === f);
        
        const row = document.createElement("div");
        row.style.display = "flex";
        row.style.justifyContent = "center";
        row.style.gap = "80px";
        row.style.zIndex = "2"; 
        
        floorNodes.forEach(node => {
            const btnWrapper = document.createElement("div");
            btnWrapper.style.position = "relative"; 

            const btn = document.createElement("button");
            btn.id = `node-${node.id}`;
            
            // --- LOGIKA IKON A TOOLTIPŮ ---
            let icon = "❓"; 
            let tooltip = "Neznámé";

            switch (node.type) {
                case "Encounter": icon = "⚔️"; tooltip = "Běžný souboj"; break;
                case "EliteEncounter": icon = "👹"; tooltip = "Elitní souboj (Nebezpečí!)"; break;
                case "Boss": icon = "👑"; tooltip = "Boss Aktu"; break;
                case "Treasure": icon = "💰"; tooltip = "Truhla s pokladem"; break;
                case "RestPlace": icon = "⛺"; tooltip = "Odpočinek (Táborák)"; break;
                case "Shop": icon = "🛒"; tooltip = "Obchodník"; break;
                case "Event": icon = "📜"; tooltip = "Náhodná událost"; break;
            }

            btn.title = tooltip; // Popisek při najetí myší
            btn.innerHTML = `<div style="font-size: 26px;">${icon}</div>`;
            btn.style.padding = "10px";
            btn.style.borderRadius = "50%";
            btn.style.border = "3px solid #2c3e50";
            btn.style.width = "65px";
            btn.style.height = "65px";
            btn.style.display = "flex";
            btn.style.justifyContent = "center";
            btn.style.alignItems = "center";
            btn.style.transition = "all 0.2s ease";
            
            // Logika klikatelnosti
            let isClickable = false;
            if (myCurrentNodeId === -1 && node.floor === 0) isClickable = true; // Start
            if (validNextNodeIds.includes(node.id) && !node.isCompleted) isClickable = true; // Další krok

            // Vizualizace hlasů spoluhráčů
            let votesForThisNode = Object.keys(currentMapVotes).filter(k => currentMapVotes[k] === node.id);
            if (votesForThisNode.length > 0) {
                let votersDiv = document.createElement("div");
                votersDiv.style.position = "absolute";
                votersDiv.style.bottom = "-28px";
                votersDiv.style.left = "50%";
                votersDiv.style.transform = "translateX(-50%)";
                votersDiv.style.fontSize = "11px";
                votersDiv.style.color = "#f1c40f";
                votersDiv.style.fontWeight = "bold";
                votersDiv.style.textShadow = "1px 1px 2px black";
                votersDiv.style.whiteSpace = "nowrap";
                votersDiv.innerText = votesForThisNode.join(", ");
                btnWrapper.appendChild(votersDiv);
                
                btn.style.boxShadow = "0 0 20px #f1c40f"; // Zlaté záření pro vybrané pole
                btn.style.borderColor = "#f1c40f";
            }

            // Nastavení barev a stavů tlačítka
            if (node.isCompleted) {
                // Již navštívené pole
                btn.style.backgroundColor = "#7f8c8d"; 
                btn.style.opacity = "0.5";
                btn.disabled = true;
            } else if (myCurrentNodeId === node.id) {
                // Aktuální pole, kde stojíme
                btn.style.backgroundColor = "#f39c12"; 
                btn.style.borderColor = "white";
                btn.style.transform = "scale(1.1)";
                btn.disabled = true;
            } else if (isClickable) {
                // Pole, na které můžeme jít (aktivní pro hlasování)
                btn.style.backgroundColor = "#2ecc71"; 
                btn.style.cursor = "pointer";
                if(votesForThisNode.length === 0) btn.style.boxShadow = "0 0 10px rgba(46, 204, 113, 0.5)";
                
                btn.onmouseover = () => {
                    btn.style.transform = "scale(1.2)";
                    btn.style.backgroundColor = "#27ae60";
                };
                btn.onmouseout = () => {
                    btn.style.transform = "scale(1)";
                    btn.style.backgroundColor = "#2ecc71";
                };
                
                btn.onclick = () => {
                    connection.invoke("VoteNextNode", currentRoomName, playerName, node.id)
                        .catch(err => console.error(err));
                };
            } else {
                // Budoucí pole, která jsou zatím nedostupná
                btn.style.backgroundColor = "#34495e";
                btn.style.opacity = "0.4";
                btn.disabled = true;
            }
            
            btnWrapper.appendChild(btn);
            row.appendChild(btnWrapper);
        });
        mapContainer.appendChild(row);
    }

    // Vykreslení čar (SVG) po načtení DOMu
    setTimeout(() => drawMapLines(mapContainer), 100);
}

function drawMapLines(container) {
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

            const x1 = (fromRect.left + fromRect.width / 2) - containerRect.left;
            const y1 = (fromRect.top + fromRect.height / 2) - containerRect.top;
            const x2 = (toRect.left + toRect.width / 2) - containerRect.left;
            const y2 = (toRect.top + toRect.height / 2) - containerRect.top;

            const line = document.createElementNS("http://www.w3.org/2000/svg", "line");
            line.setAttribute("x1", x1);
            line.setAttribute("y1", y1);
            line.setAttribute("x2", x2);
            line.setAttribute("y2", y2);
            line.setAttribute("stroke", "rgba(255, 255, 255, 0.2)");
            line.setAttribute("stroke-width", "3");

            // Zvýrazní zeleně, pokud je to naše dostupná další cesta
            if (myCurrentNodeId === node.id || (myCurrentNodeId === -1 && node.floor === 0)) {
                 line.setAttribute("stroke", "rgba(46, 204, 113, 0.6)");
                 line.setAttribute("stroke-width", "5");
            }

            svg.appendChild(line);
        });
    });

    container.appendChild(svg);
}

function drawMapLines(container) {
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

            const x1 = (fromRect.left + fromRect.width / 2) - containerRect.left;
            const y1 = (fromRect.top + fromRect.height / 2) - containerRect.top;
            const x2 = (toRect.left + toRect.width / 2) - containerRect.left;
            const y2 = (toRect.top + toRect.height / 2) - containerRect.top;

            const line = document.createElementNS("http://www.w3.org/2000/svg", "line");
            line.setAttribute("x1", x1);
            line.setAttribute("y1", y1);
            line.setAttribute("x2", x2);
            line.setAttribute("y2", y2);
            line.setAttribute("stroke", "rgba(255, 255, 255, 0.2)");
            line.setAttribute("stroke-width", "3");

            if (myCurrentNodeId === node.id || (myCurrentNodeId === -1 && node.floor === 0)) {
                 line.setAttribute("stroke", "rgba(46, 204, 113, 0.6)");
                 line.setAttribute("stroke-width", "5");
            }

            svg.appendChild(line);
        });
    });

    container.appendChild(svg);
}