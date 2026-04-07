// ==========================================
// SIGNALR PŘIPOJENÍ
// ==========================================
const connection = new signalR.HubConnectionBuilder()
    .withUrl("https://karma-the-shared-burden.onrender.com/gamehub") 
    .withAutomaticReconnect()
    .build();

// --- LOGOVÁNÍ VÝPADKŮ PŘIPOJENÍ ---
connection.onreconnecting(error => {
    console.warn("Spojení se serverem bylo přerušeno. Pokouším se znovu připojit...", error);
    logMessage("⚠️ Ztráta spojení. Hra se zkouší znovu připojit...");
});

connection.onreconnected(connectionId => {
    console.log("Znovu připojeno!");
    logMessage("✅ Spojení bylo úspěšně obnoveno!");
});

connection.onclose(error => {
    console.error("Spojení bylo trvale ukončeno.", error);
    logMessage("❌ Spojení se serverem bylo trvale ztraceno. Bude potřeba obnovit stránku (F5).");
});

async function startConnectionIfNotStarted() {
    if (connection.state === signalR.HubConnectionState.Disconnected) {
        await connection.start();
    } else if (connection.state === signalR.HubConnectionState.Connecting || connection.state === signalR.HubConnectionState.Reconnecting) {
        throw new Error("Připojování k serveru probíhá, vydržte chvíli...");
    }
}

async function createLobby() { 
    if(!getCredentials()) return; 
    try {
        await startConnectionIfNotStarted();
        await connection.invoke("CreateLobby", currentRoomName, playerName, playerClass);
        showWaitingRoom();
    } catch (err) {
        console.warn(err.message); 
    }
}

async function joinLobby() { 
    if(!getCredentials()) return; 
    try {
        await startConnectionIfNotStarted();
        await connection.invoke("JoinLobby", currentRoomName, playerName, playerClass);
        showWaitingRoom();
    } catch (err) {
        console.warn(err.message);
    }
}

function startGame() { 
    connection.invoke("StartGame", currentRoomName).catch(err => console.error(err)); 
}

// ==========================================
// ZACHYTÁVÁNÍ ZPRÁV ZE SERVERU
// ==========================================

connection.on("LobbyError", (msg) => { alert(msg); location.reload(); });
connection.on("LobbyUpdate", (players) => {
    const list = document.getElementById("lobby-players-list"); list.innerHTML = "";
    players.forEach(p => { 
        const li = document.createElement("li"); 
        let pName = safeGet(p, 'name', 'Name') || p; 
        let pClass = safeGet(p, 'heroClass', 'HeroClass');
        li.innerHTML = `🧙‍♂️ <strong>${pName}</strong> ${pClass ? `<span style="color: #f1c40f; font-size: 16px;">(${pClass})</span>` : ""}`; 
        list.appendChild(li); 
    });
});

connection.on("YouAreHost", () => { 
    showElement("start-game-btn", "inline-block"); 
    hideElement("waiting-text"); 
});

connection.on("GameStarted", (roomName, initialMap) => {
    hideElement("waiting-screen");
    showElement("game-screen"); 
    gameMap = initialMap || []; 
    myCurrentNodeId = -1; currentMapVotes = {};
    logMessage(`🔥 Hra začala! Hlasujte pro startovní políčko.`);
    toggleUI("map"); 
    renderMap();
});

// --- PŘIDÁNO ZPĚT: RELIKVIE ---
connection.on("UpdateRelics", (relicsList) => { 
    updateRelicsUI(relicsList); 
});

connection.on("UpdateTeamStats", (teamData) => { currentTeamData = teamData; });
connection.on("UpdateMapVotes", (votes) => { currentMapVotes = votes; renderMap(); });

connection.on("ReceiveInitialState", (hand, mana, serverCards, gold, drawPile, discardPile, hp, maxHp, block, startingDeck) => {
    cardDatabase = serverCards; myHand = hand; myMana = mana; myGold = gold || 0;
    myDrawPile = drawPile || []; myDiscardPile = discardPile || []; myStartingDeck = startingDeck || [];
    myDrawPileCount = myDrawPile.length; myDiscardPileCount = myDiscardPile.length;
    myHp = hp || 0; myMaxHp = maxHp || 0; myBlock = block || 0;
    updateStatsUI(); renderHand(); 
});

connection.on("ReceiveNewTurnState", (updatedHand, updatedMana, updatedGold, drawPile, discardPile, hp, maxHp, block, enemiesArray) => {
    myHand = updatedHand; myMana = updatedMana; myGold = updatedGold || 0;
    myDrawPile = drawPile || []; myDiscardPile = discardPile || [];
    myDrawPileCount = myDrawPile.length; myDiscardPileCount = myDiscardPile.length;
    myHp = hp || 0; myMaxHp = maxHp || 0; myBlock = block || 0;
    currentEnemiesArray = enemiesArray;
    updateStatsUI(); renderHand(); 
});

connection.on("EnteredNode", (nodeTypeRaw, nodeData, enemiesArray) => {
    let nodeType = getTypeString(nodeTypeRaw); logMessage(`📍 Vstupujete do: ${nodeType}`);
    myCurrentNodeId = safeGet(nodeData, 'id', 'Id'); currentMapVotes = {}; currentEnemiesArray = enemiesArray; 
    let mapNode = gameMap.find(n => safeGet(n, 'id', 'Id') === myCurrentNodeId); if(mapNode) mapNode.isCompleted = true;
    
    if (nodeType === "Encounter" || nodeType === "EliteEncounter" || nodeType === "Boss") { 
        toggleUI("battle");
        init3DScene(); // Start 3D grafiky
    } else {
        stop3DScene(); // Zastavení 3D grafiky
        toggleUI(nodeType.toLowerCase()); 
        if(nodeType === "Shop" || nodeType === "Event" || nodeType === "RestPlace" || nodeType === "Treasure") {
            hideElement("map-container");
        }
    }
});

// 3D KOMUNIKACE
connection.on("Update3DState", (playersData, enemiesData) => { update3DEntities(playersData, enemiesData); });
connection.on("SpawnHitEffect", (x, y, z, damage) => { spawn3DHitEffect(x, y, z, damage); });

// KARTY A BOJ
connection.on("CardPlayedLog", (player, cardId) => { const cData = getCardData(cardId); logMessage(`🎴 ${player} seslal: ${cData.name}`); });
connection.on("TurnResolved", (summary, totalDamage, newKarma, enemiesArray) => { summary.forEach(s => logMessage(s)); });

// MÍSTNOSTI
connection.on("EnterShop", (shopCards, shopRelics, removeCost) => { renderShopUI(shopCards, shopRelics, removeCost); });
connection.on("ShopPurchaseSuccess", (newGold, newDeck) => {
    myGold = newGold; myStartingDeck = newDeck;
    document.getElementById("shop-gold").innerText = myGold;
    logMessage("Nákup v obchodě proběhl úspěšně!"); updateStatsUI();
    renderShopRemoveDeck();
});

connection.on("EnterEvent", (eventData) => { renderEventUI(eventData); });
connection.on("EventResolved", (newGold, newHp, newMaxHp) => { myGold = newGold; myHp = newHp; myMaxHp = newMaxHp; updateStatsUI(); toggleUI("map"); renderMap(); });

connection.on("EnterRestPlace", () => { toggleUI("rest"); });
connection.on("RestActionCompleted", (newHp, newDeck) => { myHp = newHp; myStartingDeck = newDeck; updateStatsUI(); logMessage("Odpočinek úspěšně dokončen."); toggleUI("map"); renderMap(); });

connection.on("ShowRewardScreen", (cardChoices, relicChoice, goldReward, newGoldAmount) => {
    currentRelicReward = relicChoice; myGold = newGoldAmount; updateStatsUI();
    renderRewardScreenUI(cardChoices, relicChoice, goldReward);
});
connection.on("RewardClaimed", (updatedDeck) => { if(updatedDeck) myStartingDeck = updatedDeck; toggleUI("map"); renderMap(); });