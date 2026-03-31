const connection = new signalR.HubConnectionBuilder()
    .withUrl("https://karma-the-shared-burden.onrender.com/gamehub") 
    .build();

let playerName = ""; let playerClass = ""; let currentRoomName = ""; 
let isGameOver = false; let turnEnded = false; 
let gameMap = []; let cardDatabase = {}; 

let myHand = []; let myMana = 0; let myGold = 0;
let myDrawPile = []; let myDiscardPile = []; let myStartingDeck = [];
let myDrawPileCount = 0; let myDiscardPileCount = 0;
let myHp = 0; let myMaxHp = 0; let myBlock = 0;

let myCurrentNodeId = -1;
let currentEnemiesArray = []; 
let selectedTargetCard = null; 
let currentMapVotes = {}; 

// --- ULTIMÁTNÍ BEZPEČNÉ ZOBRAZOVÁNÍ ---
function showElement(id, displayType = "block") {
    let el = document.getElementById(id);
    if (el) {
        el.classList.remove("hidden");
        // Tohle přebije cokoliv, co by bránilo zobrazení
        el.style.setProperty("display", displayType, "important");
    }
}

function hideElement(id) {
    let el = document.getElementById(id);
    if (el) {
        el.classList.add("hidden");
        el.style.setProperty("display", "none", "important");
    }
}

// --- BEZPEČNÉ ZÍSKÁNÍ DAT ---
function safeGet(obj, propLower, propUpper) {
    if (!obj) return undefined;
    if (obj[propLower] !== undefined) return obj[propLower];
    if (obj[propUpper] !== undefined) return obj[propUpper];
    return undefined;
}

const getTypeString = (typeVal) => {
    if (typeof typeVal === "string") return typeVal;
    const typeMap = ["Encounter", "EliteEncounter", "RestPlace", "Shop", "Treasure", "Event", "Boss"];
    return typeMap[typeVal] || "Encounter";
};

function getCardData(cardId) {
    let isUpgraded = cardId.endsWith("+");
    let baseId = isUpgraded ? cardId.slice(0, -1) : cardId;
    let base = cardDatabase[baseId];
    if (!base) return { id: cardId, name: cardId, cost: 1, karmaShift: 0, damage: 5, description: "" };
    if (isUpgraded) {
        return {
            id: cardId, name: "⭐ " + base.name,
            damage: (base.damage > 0) ? base.damage + 3 : 0, block: (base.block > 0) ? base.block + 3 : 0, heal: (base.heal > 0) ? base.heal + 2 : 0,
            cost: Math.max(0, base.cost - 1), karmaShift: base.karmaShift, description: base.description + " (Vylepšeno)"
        };
    }
    return { ...base, id: cardId };
}

// --- LOGOVÁNÍ UDÁLOSTÍ DO HRY ---
function logMessage(msg) {
    const logEl = document.getElementById("log");
    if (!logEl) return;
    const li = document.createElement("li");
    li.innerText = msg;
    logEl.appendChild(li);
    logEl.scrollTop = logEl.scrollHeight; // Posune na konec výpisu
}

// --- LOBBY A HRA ---
function pickHero(hero) { playerClass = hero; document.getElementById("selected-hero-text").innerText = hero; }
function getCredentials() {
    playerName = document.getElementById("player-name-input").value.trim();
    currentRoomName = document.getElementById("room-name-input").value.trim();
    if (!playerName || !currentRoomName || !playerClass) { alert("Musíš vyplnit všechny údaje!"); return false; }
    return true;
}

function createLobby() { if(!getCredentials()) return; connection.start().then(() => { connection.invoke("CreateLobby", currentRoomName, playerName, playerClass).catch(err => console.error(err)); showWaitingRoom(); }); }
function joinLobby() { if(!getCredentials()) return; connection.start().then(() => { connection.invoke("JoinLobby", currentRoomName, playerName, playerClass).catch(err => console.error(err)); showWaitingRoom(); }); }
function startGame() { connection.invoke("StartGame", currentRoomName).catch(err => console.error(err)); }

function showWaitingRoom() { hideElement("lobby-screen"); showElement("waiting-screen"); document.getElementById("display-room-name").innerText = currentRoomName; }

connection.on("LobbyError", (msg) => { alert(msg); location.reload(); });
connection.on("LobbyUpdate", (players) => {
    const list = document.getElementById("lobby-players-list"); list.innerHTML = "";
    players.forEach(p => { const li = document.createElement("li"); li.innerText = "🧙‍♂️ " + p; list.appendChild(li); });
});
connection.on("YouAreHost", () => { showElement("start-game-btn", "inline-block"); hideElement("waiting-text"); });

connection.on("GameStarted", (roomName, initialMap) => {
    console.log("=========================================");
    console.log("🔥 GAME STARTED EVENT PŘIJAT!");
    console.log("Data mapy ze serveru:", initialMap);
    
    hideElement("waiting-screen");
    showElement("game-screen"); // Tohle MUSÍ ukázat obrazovku
    
    gameMap = initialMap || []; 
    myCurrentNodeId = -1; currentMapVotes = {};
    logMessage(`🔥 Hra začala! Hlasujte pro startovní políčko.`);
    
    toggleUI("map"); 
    renderMap();
});

connection.on("UpdateRelics", (relicsList) => {
    const relicsContainer = document.getElementById("relics-list"); relicsContainer.innerHTML = ""; 
    if (relicsList.length === 0) { relicsContainer.innerText = "Zatím žádné"; return; }
    relicsList.forEach(r => {
        const span = document.createElement("span"); span.innerText = `[${safeGet(r, 'name', 'Name')}] `;
        span.title = safeGet(r, 'description', 'Description'); span.style.cursor = "help"; span.style.borderBottom = "2px dotted #2c3e50"; span.style.marginRight = "10px";
        relicsContainer.appendChild(span);
    });
});

connection.on("UpdateTeamStats", (teamData) => { renderTeam(teamData); });
connection.on("UpdateMapVotes", (votes) => { currentMapVotes = votes; renderMap(); });

connection.on("ReceiveInitialState", (hand, mana, serverCards, gold, drawPile, discardPile, hp, maxHp, block, startingDeck) => {
    cardDatabase = serverCards; myHand = hand; myMana = mana; myGold = gold || 0;
    myDrawPile = drawPile || []; myDiscardPile = discardPile || []; myStartingDeck = startingDeck || [];
    myDrawPileCount = myDrawPile.length; myDiscardPileCount = myDiscardPile.length;
    myHp = hp || 0; myMaxHp = maxHp || 0; myBlock = block || 0;
    updateStatsUI(); renderHand(); 
});

connection.on("CardPlayedLog", (player, cardId) => { const cData = getCardData(cardId); logMessage(`🎴 ${player} připravil kartu: ${cData.name}`); });
connection.on("PlayerReadyLog", (player, readyCount, totalPlayers) => { logMessage(`✅ ${player} ukončil tah. (${readyCount}/${totalPlayers})`); });
connection.on("TurnResolved", (summary, totalDamage, newKarma, enemiesArray) => { logMessage(`--- TAH VYHODNOCEN ---`); currentEnemiesArray = enemiesArray; renderEnemies(enemiesArray); updateKarmaUI(newKarma); });

connection.on("ReceiveNewTurnState", (updatedHand, updatedMana, updatedGold, drawPile, discardPile, hp, maxHp, block, enemiesArray) => {
    myHand = updatedHand; myMana = updatedMana; myGold = updatedGold || 0;
    myDrawPile = drawPile || []; myDiscardPile = discardPile || [];
    myDrawPileCount = myDrawPile.length; myDiscardPileCount = myDiscardPile.length;
    myHp = hp || 0; myMaxHp = maxHp || 0; myBlock = block || 0;
    turnEnded = false; currentEnemiesArray = enemiesArray; selectedTargetCard = null; 
    document.getElementById("end-turn-btn").disabled = false; document.getElementById("end-turn-btn").style.backgroundColor = "#8e44ad";
    renderEnemies(enemiesArray); updateStatsUI(); renderHand(); 
});

connection.on("EnteredNode", (nodeTypeRaw, nodeData, enemiesArray) => {
    let nodeType = getTypeString(nodeTypeRaw); logMessage(`📍 Vstupujete do: ${nodeType}`);
    myCurrentNodeId = safeGet(nodeData, 'id', 'Id'); currentMapVotes = {}; currentEnemiesArray = enemiesArray; 
    let mapNode = gameMap.find(n => safeGet(n, 'id', 'Id') === myCurrentNodeId); if(mapNode) mapNode.isCompleted = true;

    if (nodeType === "Encounter" || nodeType === "EliteEncounter" || nodeType === "Boss") { renderEnemies(enemiesArray); toggleUI("battle"); } 
    else if (nodeType === "Treasure") {
        renderEnemies([]); toggleUI("battle");
        const btn = document.createElement("button"); btn.innerText = "💰 Poklad sebrán. Pokračovat na mapu";
        btn.style.cssText = "padding: 15px 30px; background: #27ae60; color: white; border: none; border-radius: 5px; cursor: pointer; margin-top: 20px;";
        btn.onclick = () => { toggleUI("map"); renderMap(); }; document.getElementById("enemies-container").appendChild(btn);
    }
});

// --- EVENTS & SHOP & REST ---
connection.on("EnterEvent", (eventData) => {
    toggleUI("event");
    document.getElementById("event-title").innerText = safeGet(eventData, 'title', 'Title');
    document.getElementById("event-desc").innerText = safeGet(eventData, 'desc', 'Desc');
    const optionsContainer = document.getElementById("event-options"); optionsContainer.innerHTML = "";
    let opts = safeGet(eventData, 'options', 'Options') || [];
    opts.forEach(opt => {
        const btn = document.createElement("button");
        btn.innerText = safeGet(opt, 'text', 'Text');
        btn.style.cssText = "padding: 15px 30px; font-size: 16px; background: #8e44ad; color: white; border: none; border-radius: 5px; cursor: pointer; transition: 0.2s;";
        btn.onclick = () => {
            optionsContainer.innerHTML = "<p>Vybráno! Čekáme na ostatní nebo posun...</p>";
            connection.invoke("ResolveEventOption", currentRoomName, playerName, safeGet(opt, 'id', 'Id')).catch(err => console.error(err));
        };
        optionsContainer.appendChild(btn);
    });
});

connection.on("EventResolved", (newGold, newHp, newMaxHp) => { myGold = newGold; myHp = newHp; myMaxHp = newMaxHp; updateStatsUI(); toggleUI("map"); renderMap(); });

connection.on("EnterShop", (shopCards, shopRelics, removeCost) => {
    toggleUI("shop");
    document.getElementById("shop-gold").innerText = myGold;
    document.getElementById("shop-remove-cost").innerText = removeCost;
    
    const cardsContainer = document.getElementById("shop-cards"); cardsContainer.innerHTML = "";
    shopCards.forEach(c => {
        const btn = document.createElement("div");
        let id = safeGet(c, 'id', 'Id'); let name = safeGet(c, 'name', 'Name'); let desc = safeGet(c, 'desc', 'Desc'); let price = safeGet(c, 'price', 'Price');
        btn.innerHTML = `<strong>${name}</strong><br><small>${desc}</small><br><button style="margin-top:10px; background:#f1c40f; border:none; padding:5px; cursor:pointer; color:black;">Koupit za ${price}</button>`;
        btn.style.cssText = "padding: 10px; border: 2px solid #f1c40f; border-radius: 5px; width: 140px; background: #2c3e50;";
        btn.querySelector("button").onclick = () => buyShopItem(id, "Card", price);
        cardsContainer.appendChild(btn);
    });

    const relicsContainer = document.getElementById("shop-relics"); relicsContainer.innerHTML = "";
    shopRelics.forEach(r => {
        const btn = document.createElement("div");
        let id = safeGet(r, 'id', 'Id'); let name = safeGet(r, 'name', 'Name'); let desc = safeGet(r, 'desc', 'Desc'); let price = safeGet(r, 'price', 'Price');
        btn.innerHTML = `<strong>🏆 ${name}</strong><br><small>${desc}</small><br><button style="margin-top:10px; background:#f1c40f; border:none; padding:5px; cursor:pointer; color:black;">Koupit za ${price}</button>`;
        btn.style.cssText = "padding: 10px; border: 2px solid #f1c40f; border-radius: 5px; width: 180px; background: #2c3e50;";
        btn.querySelector("button").onclick = () => buyShopItem(id, "Relic", price);
        relicsContainer.appendChild(btn);
    });

    const removeContainer = document.getElementById("shop-remove-deck"); removeContainer.innerHTML = "";
    myStartingDeck.forEach(cardId => {
        const cData = getCardData(cardId);
        const btn = document.createElement("button"); btn.innerText = cData.name;
        btn.style.cssText = "padding: 5px 10px; background: #e74c3c; color: white; border: none; border-radius: 3px; cursor: pointer;";
        btn.onclick = () => removeCardFromDeck(cardId, removeCost);
        removeContainer.appendChild(btn);
    });
});

function buyShopItem(itemId, type, price) { if (myGold < price) { alert("Nemáš dost zlaťáků!"); return; } connection.invoke("BuyShopItem", currentRoomName, playerName, itemId, type, price).catch(err => console.error(err)); }
function removeCardFromDeck(cardId, price) { if (myGold < price) { alert("Nemáš dost zlaťáků!"); return; } connection.invoke("RemoveCardFromDeck", currentRoomName, playerName, cardId, price).catch(err => console.error(err)); }

connection.on("ShopPurchaseSuccess", (newGold, newDeck) => {
    myGold = newGold; myStartingDeck = newDeck;
    document.getElementById("shop-gold").innerText = myGold;
    logMessage("Nákup v obchodě proběhl úspěšně!"); updateStatsUI();
    const removeContainer = document.getElementById("shop-remove-deck"); removeContainer.innerHTML = "";
    myStartingDeck.forEach(cId => {
        const cData = getCardData(cId);
        const btn = document.createElement("button"); btn.innerText = cData.name;
        btn.style.cssText = "padding: 5px 10px; background: #e74c3c; color: white; border: none; border-radius: 3px; cursor: pointer;";
        btn.onclick = () => removeCardFromDeck(cId, 50); 
        removeContainer.appendChild(btn);
    });
});

function leaveShop() { toggleUI("map"); renderMap(); }

connection.on("EnterRestPlace", () => { toggleUI("rest"); });
function chooseRestHeal() { connection.invoke("RestPlaceAction", currentRoomName, playerName, "heal", "").catch(err => console.error(err)); }
function openUpgradeModal() {
    const container = document.getElementById("upgrade-modal-content"); container.innerHTML = "";
    let upgradableCards = myStartingDeck.filter(c => !c.endsWith("+"));
    if (upgradableCards.length === 0) { container.innerHTML = "<p style='color: white;'>Nemáš žádné karty k vylepšení.</p>"; } 
    else {
        upgradableCards.forEach(cardId => {
            const cData = getCardData(cardId); const btn = document.createElement("div");
            let color = "#ecf0f1"; if(cData.karmaShift < 0) color = "#ffcccc"; if(cData.karmaShift > 0) color = "#ccffcc"; 
            btn.innerHTML = `<strong>${cData.name}</strong><br><em>${cData.cost} Many</em><br><hr style="margin:5px 0;"><small>${cData.description}</small>`;
            btn.style.cssText = `padding: 10px; width: 120px; border: 2px solid #e67e22; border-radius: 8px; background: ${color}; color: #2c3e50; cursor: pointer; transition: transform 0.2s;`;
            btn.onclick = () => chooseRestUpgrade(cardId);
            container.appendChild(btn);
        });
    }
    showElement("upgrade-modal", "flex");
}
function closeUpgradeModal() { hideElement("upgrade-modal"); }
function chooseRestUpgrade(cardId) { closeUpgradeModal(); connection.invoke("RestPlaceAction", currentRoomName, playerName, "upgrade", cardId).catch(err => console.error(err)); }

connection.on("RestActionCompleted", (newHp, newDeck) => { myHp = newHp; myStartingDeck = newDeck; updateStatsUI(); logMessage("Odpočinek u táboráku úspěšně dokončen."); toggleUI("map"); renderMap(); });

// --- BOJ A HRANÍ KARET ---
function playCard(cardId, karmaShift, damage) {
    if (isGameOver) return; 
    if (turnEnded) { alert("Už jsi ukončil tah!"); return; }
    const cardData = getCardData(cardId);
    if (myMana < cardData.cost) { alert("Nemáš dostatek Many!"); return; }

    if (cardData.damage > 0) {
        selectedTargetCard = { id: cardId, karmaShift: karmaShift, damage: cardData.damage, cost: cardData.cost };
        logMessage("🎯 Vyber cíl pro útok (klikni na nepřítele)!");
        renderEnemies(currentEnemiesArray); 
        return;
    } else { executeCardPlay(cardId, karmaShift, 0, cardData.cost, ""); }
}

function executeCardPlay(cardId, karmaShift, damage, cardCost, targetEnemyId) {
    myMana -= cardCost; 
    const cardIndex = myHand.indexOf(cardId);
    if (cardIndex > -1) { myHand.splice(cardIndex, 1); myDiscardPileCount++; }
    const cData = getCardData(cardId);
    myBlock += (cData.block || 0); myHp += (cData.heal || 0); if(myHp > myMaxHp) myHp = myMaxHp;
    selectedTargetCard = null; 
    updateStatsUI(); renderHand(); renderEnemies(currentEnemiesArray); 
    connection.invoke("SelectCard", currentRoomName, playerName, cardId, karmaShift, damage, targetEnemyId).catch(err => console.error(err));
}

function endTurn() {
    if (turnEnded) return; turnEnded = true; selectedTargetCard = null; 
    document.getElementById("end-turn-btn").disabled = true; document.getElementById("end-turn-btn").style.backgroundColor = "gray";
    renderEnemies(currentEnemiesArray); 
    connection.invoke("PlayerReady", currentRoomName, playerName).catch(err => console.error(err));
}

// --- ODMĚNY PO BOJI ---
connection.on("ShowRewardScreen", (cardChoices, relicChoice, goldReward, newGoldAmount) => {
    logMessage(`🎉 Výhra!`); toggleUI("reward"); currentRelicReward = relicChoice; myGold = newGoldAmount; updateStatsUI();
    document.getElementById("reward-gold").innerText = goldReward;

    const relicContainer = document.getElementById("relic-reward-container");
    if (relicChoice) {
        relicContainer.style.display = "block";
        document.getElementById("reward-relic-name").innerText = safeGet(relicChoice, 'name', 'Name');
        document.getElementById("reward-relic-desc").innerText = safeGet(relicChoice, 'description', 'Description');
    } else { relicContainer.style.display = "none"; }

    const cardContainer = document.getElementById("card-reward-container"); cardContainer.innerHTML = "";
    cardChoices.forEach(card => {
        const btn = document.createElement("button");
        let id = safeGet(card, 'id', 'Id'); let karma = safeGet(card, 'karmaShift', 'KarmaShift');
        let color = "#ecf0f1"; if(karma < 0) color = "#ffcccc"; if(karma > 0) color = "#ccffcc"; 
        btn.innerHTML = `<strong>${safeGet(card, 'name', 'Name')}</strong><br><em>${safeGet(card, 'cost', 'Cost')} Many</em><br><hr style="margin:5px 0;"><small>${safeGet(card, 'description', 'Description')}</small>`;
        btn.style.cssText = `padding: 15px; width: 160px; border: 3px solid #34495e; border-radius: 8px; background: ${color}; cursor: pointer;`;
        btn.onclick = () => {
            let relId = currentRelicReward ? safeGet(currentRelicReward, 'id', 'Id') : "";
            let relName = currentRelicReward ? safeGet(currentRelicReward, 'name', 'Name') : "";
            let relDesc = currentRelicReward ? safeGet(currentRelicReward, 'description', 'Description') : "";
            connection.invoke("ClaimReward", currentRoomName, playerName, id, relId, relName, relDesc).catch(err => console.error(err));
        };
        cardContainer.appendChild(btn);
    });
});

connection.on("RewardClaimed", (updatedDeck) => { if(updatedDeck) myStartingDeck = updatedDeck; toggleUI("map"); renderMap(); });
function skipReward() {
    let relId = currentRelicReward ? safeGet(currentRelicReward, 'id', 'Id') : "";
    let relName = currentRelicReward ? safeGet(currentRelicReward, 'name', 'Name') : "";
    let relDesc = currentRelicReward ? safeGet(currentRelicReward, 'description', 'Description') : "";
    connection.invoke("ClaimReward", currentRoomName, playerName, "", relId, relName, relDesc).catch(err => console.error(err));
}

// --- VYKRESLOVÁNÍ UI ---
function showModalWithCards(title, cardIds) {
    document.getElementById("card-modal-title").innerText = title;
    const container = document.getElementById("card-modal-content"); container.innerHTML = "";
    if (!cardIds || cardIds.length === 0) { container.innerHTML = "<p style='color: white;'>Zatím prázdné...</p>"; } 
    else {
        cardIds.forEach(cardId => {
            const cData = getCardData(cardId); const cardElement = document.createElement("div");
            let color = "#ecf0f1"; if(cData.karmaShift < 0) color = "#ffcccc"; if(cData.karmaShift > 0) color = "#ccffcc"; 
            cardElement.innerHTML = `<strong>${cData.name}</strong><br><em>${cData.cost} Many</em><br><hr style="margin:5px 0;"><small>${cData.description}</small>`;
            cardElement.style.cssText = `padding: 10px; width: 120px; min-height: 150px; border: 2px solid #34495e; border-radius: 8px; background: ${color}; margin: 10px; display: flex; flex-direction: column; color: #2c3e50; box-shadow: 2px 2px 10px rgba(0,0,0,0.5);`;
            container.appendChild(cardElement);
        });
    }
    showElement("card-modal", "flex");
}
function closeCardModal() { hideElement("card-modal"); }
function showStartingDeck() { showModalWithCards("Tvůj kompletní balíček", myStartingDeck); }
function showDrawPile() { showModalWithCards("Karty k líznutí", myDrawPile); }
function showDiscardPile() { showModalWithCards("Odhozené karty", myDiscardPile); }

function renderTeam(teamData) {
    const container = document.getElementById("team-container"); if (!container) return; container.innerHTML = "";
    teamData.forEach(player => {
        const name = safeGet(player, 'name', 'Name');
        const isMe = name === playerName; const div = document.createElement("div");
        div.style.cssText = `background: ${isMe ? "#27ae60" : "#2980b9"}; color: white; padding: 8px 15px; border-radius: 5px; box-shadow: 0 2px 4px rgba(0,0,0,0.3); text-align: center; min-width: 120px;`;
        div.innerHTML = `<div style="font-weight: bold; border-bottom: 1px solid rgba(255,255,255,0.3); margin-bottom: 5px;">${name}</div>
            <div style="font-size: 14px;">❤️ ${safeGet(player, 'hp', 'Hp')} / ${safeGet(player, 'maxHp', 'MaxHp')}</div>
            <div style="font-size: 14px;">🛡️ Blok: ${safeGet(player, 'block', 'Block')}</div>`;
        container.appendChild(div);
    });
}

function renderEnemies(enemiesArray) {
    const container = document.getElementById("enemies-container"); if (!container) return; container.innerHTML = "";
    if (!enemiesArray || enemiesArray.length === 0) return;

    enemiesArray.forEach(e => {
        let hp = safeGet(e, 'hp', 'Hp'); if (hp <= 0) return; 
        let id = safeGet(e, 'id', 'Id'); let name = safeGet(e, 'name', 'Name'); let maxHp = safeGet(e, 'maxHp', 'MaxHp');
        let action = safeGet(e, 'currentAction', 'CurrentAction');
        let actionDesc = action ? safeGet(action, 'intentDescription', 'IntentDescription') : "";

        const div = document.createElement("div");
        div.style.cssText = "background: #c0392b; padding: 10px; border-radius: 5px; color: white; text-align: center; min-width: 160px; transition: all 0.3s ease;";
        
        if (selectedTargetCard) {
            div.style.cursor = "crosshair"; div.style.boxShadow = "0 0 20px #e74c3c"; div.style.transform = "scale(1.05)";
            div.onclick = () => executeCardPlay(selectedTargetCard.id, selectedTargetCard.karmaShift, selectedTargetCard.damage, selectedTargetCard.cost, id);
        } else {
            div.style.boxShadow = "0 4px 6px rgba(0,0,0,0.3)"; div.style.cursor = "default"; div.onclick = null; 
        }
        
        div.innerHTML = `<h3 style="margin: 0 0 5px 0; font-size: 18px;">${name}</h3>
            <div style="font-weight: bold;">${hp} / ${maxHp} HP</div>
            <div style="margin-top: 8px; font-size: 13px; background: rgba(0,0,0,0.4); padding: 5px; border-radius: 3px; color: #f1c40f;">${actionDesc}</div>`;
        container.appendChild(div);
    });
}

function updateStatsUI() {
    if(document.getElementById("mana-value")) document.getElementById("mana-value").innerText = myMana;
    if(document.getElementById("gold-value")) document.getElementById("gold-value").innerText = myGold;
    if(document.getElementById("deck-count")) document.getElementById("deck-count").innerText = `🎴 V balíčku: ${myDrawPileCount}`;
    if(document.getElementById("discard-count")) document.getElementById("discard-count").innerText = `🗑️ Odhozeno: ${myDiscardPileCount}`;
    if(document.getElementById("hero-hp")) document.getElementById("hero-hp").innerText = myHp;
    if(document.getElementById("hero-max-hp")) document.getElementById("hero-max-hp").innerText = myMaxHp;
    if(document.getElementById("hero-block")) document.getElementById("hero-block").innerText = myBlock;
}

function updateKarmaUI(karma) {
    const karmaValEl = document.getElementById("karma-value"); if(karmaValEl) karmaValEl.innerText = karma;
    let statusText = " (Rovnováha ⚖️)"; let statusColor = "white";
    if (karma <= -10) { statusText = " (Hluboká Temnota 🌑)"; statusColor = "#e74c3c"; }
    else if (karma < 0) { statusText = " (Příklon ke stínu 🌘)"; statusColor = "#e67e22"; }
    else if (karma >= 10) { statusText = " (Čisté Světlo ☀️)"; statusColor = "#f1c40f"; }
    else if (karma > 0) { statusText = " (Příklon ke světlu 🌤️)"; statusColor = "#f39c12"; }
    const statusEl = document.getElementById("karma-status"); if(statusEl) { statusEl.innerText = statusText; statusEl.style.color = statusColor; }
}

function renderHand() {
    const handContainer = document.getElementById("hand-container"); if (!handContainer) return; handContainer.innerHTML = ""; 
    myHand.forEach(cardId => {
        const cData = getCardData(cardId); const cardElement = document.createElement("button"); cardElement.className = "card"; 
        let color = "#ecf0f1"; if(cData.karmaShift < 0) color = "#ffcccc"; if(cData.karmaShift > 0) color = "#ccffcc"; 
        cardElement.innerHTML = `<strong>${cData.name}</strong><br><em>${cData.cost} Many</em><br><hr style="margin:5px 0;"><small>${cData.description}</small>`;
        cardElement.onclick = () => { if (selectedTargetCard) { logMessage("Nejprve vyber cíl pro předešlou kartu!"); return; } playCard(cardId, cData.karmaShift, cData.damage); };
        cardElement.style.cssText = `padding: 10px; width: 140px; border: 2px solid #34495e; border-radius: 8px; background: ${color}; cursor: pointer;`;
        handContainer.appendChild(cardElement);
    });
}

function toggleUI(state) {
    console.log(`🎛️ Přepínám UI na stav: ${state}`);
    
    // Nejprve se ujistíme, že herní obrazovka je stoprocentně vidět
    showElement("game-screen");

    ["battle-hud", "hand-wrapper", "map-container", "reward-screen", "shop-screen", "event-screen", "rest-screen"].forEach(hideElement);
    
    if (state === "battle") { showElement("battle-hud"); showElement("hand-wrapper"); } 
    else if (state === "map") { showElement("map-container"); } 
    else if (state === "reward") { showElement("reward-screen"); } 
    else if (state === "shop") { showElement("shop-screen"); } 
    else if (state === "event") { showElement("event-screen"); } 
    else if (state === "rest") { showElement("rest-screen"); }
}

// --- SILNĚ VYLEPŠENÁ MAPA (S DETAILNÍM LOGOVÁNÍM) ---
function renderMap() {
    console.log("🗺️ Funkce renderMap() spuštěna!");
    
    const mapContainer = document.getElementById("nodes-list"); 
    if (!mapContainer) { console.error("❌ CHYBA: Kontejner nodes-list nebyl v HTML nalezen!"); return; }

    mapContainer.innerHTML = ""; 
    mapContainer.style.position = "relative"; 
    mapContainer.style.display = "flex";
    mapContainer.style.flexDirection = "column-reverse"; 
    mapContainer.style.gap = "50px"; 
    mapContainer.style.padding = "20px";

    if (!gameMap || gameMap.length === 0) { console.warn("⚠️ gameMap je prázdná!"); return; }

    let currentNode = gameMap.find(n => safeGet(n, 'id', 'Id') === myCurrentNodeId);
    let validNextNodeIds = currentNode ? (safeGet(currentNode, 'connectedTo', 'ConnectedTo') || []) : [];

    // Bezpečný výpočet max patra
    let floorValues = gameMap.map(n => {
        let f = safeGet(n, 'floor', 'Floor');
        return (typeof f === "number") ? f : 0;
    });
    const maxFloor = floorValues.length > 0 ? Math.max(...floorValues) : 0;
    console.log(`⬆️ Max patro je: ${maxFloor}`);

    let vykreslenoUzlu = 0;

    for (let f = 0; f <= maxFloor; f++) {
        const floorNodes = gameMap.filter(n => {
            let nf = safeGet(n, 'floor', 'Floor');
            return (typeof nf === "number" ? nf : 0) === f;
        });
        
        if (floorNodes.length === 0) continue;

        const row = document.createElement("div");
        row.style.display = "flex"; row.style.justifyContent = "center"; row.style.gap = "80px"; row.style.zIndex = "2"; 
        
        floorNodes.forEach(node => {
            vykreslenoUzlu++;
            const btnWrapper = document.createElement("div"); btnWrapper.style.position = "relative"; 
            const btn = document.createElement("button"); 
            let nodeId = safeGet(node, 'id', 'Id'); let nodeFloor = safeGet(node, 'floor', 'Floor'); let nodeType = getTypeString(safeGet(node, 'type', 'Type')); let isCompleted = safeGet(node, 'isCompleted', 'IsCompleted');
            btn.id = `node-${nodeId}`;
            
            let icon = "❓"; let tooltip = "Neznámé";
            switch (nodeType) {
                case "Encounter": icon = "⚔️"; tooltip = "Běžný souboj"; break;
                case "EliteEncounter": icon = "👹"; tooltip = "Elitní souboj (Nebezpečí!)"; break;
                case "Boss": icon = "👑"; tooltip = "Boss Aktu"; break;
                case "Treasure": icon = "💰"; tooltip = "Truhla s pokladem"; break;
                case "RestPlace": icon = "⛺"; tooltip = "Odpočinek (Táborák)"; break;
                case "Shop": icon = "🛒"; tooltip = "Obchodník"; break;
                case "Event": icon = "📜"; tooltip = "Náhodná událost"; break;
            }

            btn.title = tooltip; btn.innerHTML = `<div style="font-size: 26px;">${icon}</div>`;
            btn.style.cssText = "padding: 10px; border-radius: 50%; border: 3px solid #2c3e50; width: 65px; height: 65px; display: flex; justify-content: center; align-items: center; transition: all 0.2s ease;";
            
            let isClickable = false;
            if (myCurrentNodeId === -1 && nodeFloor === 0) isClickable = true; 
            if (validNextNodeIds.includes(nodeId) && !isCompleted) isClickable = true; 

            let votesForThisNode = Object.keys(currentMapVotes).filter(k => currentMapVotes[k] === nodeId);
            if (votesForThisNode.length > 0) {
                let votersDiv = document.createElement("div");
                votersDiv.style.cssText = "position: absolute; bottom: -28px; left: 50%; transform: translateX(-50%); font-size: 11px; color: #f1c40f; font-weight: bold; text-shadow: 1px 1px 2px black; white-space: nowrap;";
                votersDiv.innerText = votesForThisNode.join(", ");
                btnWrapper.appendChild(votersDiv);
                btn.style.boxShadow = "0 0 20px #f1c40f"; btn.style.borderColor = "#f1c40f";
            }

            if (isCompleted) { btn.style.backgroundColor = "#7f8c8d"; btn.style.opacity = "0.5"; btn.disabled = true; } 
            else if (myCurrentNodeId === nodeId) { btn.style.backgroundColor = "#f39c12"; btn.style.borderColor = "white"; btn.style.transform = "scale(1.1)"; btn.disabled = true; } 
            else if (isClickable) {
                btn.style.backgroundColor = "#2ecc71"; btn.style.cursor = "pointer";
                if(votesForThisNode.length === 0) btn.style.boxShadow = "0 0 10px rgba(46, 204, 113, 0.5)";
                btn.onmouseover = () => { btn.style.transform = "scale(1.2)"; btn.style.backgroundColor = "#27ae60"; };
                btn.onmouseout = () => { btn.style.transform = "scale(1)"; btn.style.backgroundColor = "#2ecc71"; };
                btn.onclick = () => { connection.invoke("VoteNextNode", currentRoomName, playerName, nodeId).catch(err => console.error(err)); };
            } else {
                btn.style.backgroundColor = "#34495e"; btn.style.opacity = "0.4"; btn.disabled = true;
            }
            
            btnWrapper.appendChild(btn); row.appendChild(btnWrapper);
        });
        mapContainer.appendChild(row);
    }
    
    console.log(`✅ Mapování dokončeno. Do HTML bylo vloženo ${vykreslenoUzlu} tlačítek.`);
    setTimeout(() => drawMapLines(mapContainer), 100);
}

function drawMapLines(container) {
    let oldSvg = document.getElementById("map-svg"); if (oldSvg) oldSvg.remove();
    const svg = document.createElementNS("http://www.w3.org/2000/svg", "svg");
    svg.id = "map-svg"; svg.style.cssText = "position: absolute; top: 0; left: 0; width: 100%; height: 100%; z-index: 1; pointer-events: none;";
    const containerRect = container.getBoundingClientRect();

    gameMap.forEach(node => {
        let nodeId = safeGet(node, 'id', 'Id');
        const fromEl = document.getElementById(`node-${nodeId}`); if (!fromEl) return;
        const fromRect = fromEl.getBoundingClientRect();

        let connectedTo = safeGet(node, 'connectedTo', 'ConnectedTo') || [];
        connectedTo.forEach(targetId => {
            const toEl = document.getElementById(`node-${targetId}`); if (!toEl) return;
            const toRect = toEl.getBoundingClientRect();

            const x1 = (fromRect.left + fromRect.width / 2) - containerRect.left; const y1 = (fromRect.top + fromRect.height / 2) - containerRect.top;
            const x2 = (toRect.left + toRect.width / 2) - containerRect.left; const y2 = (toRect.top + toRect.height / 2) - containerRect.top;

            const line = document.createElementNS("http://www.w3.org/2000/svg", "line");
            line.setAttribute("x1", x1); line.setAttribute("y1", y1); line.setAttribute("x2", x2); line.setAttribute("y2", y2);
            line.setAttribute("stroke", "rgba(255, 255, 255, 0.2)"); line.setAttribute("stroke-width", "3");

            if (myCurrentNodeId === nodeId || (myCurrentNodeId === -1 && safeGet(node, 'floor', 'Floor') === 0)) {
                 line.setAttribute("stroke", "rgba(46, 204, 113, 0.6)"); line.setAttribute("stroke-width", "5");
            }
            svg.appendChild(line);
        });
    });
    container.appendChild(svg);
}