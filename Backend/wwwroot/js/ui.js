// ==========================================
// ZOBRAZOVÁNÍ PRVKŮ
// ==========================================
function showElement(id, displayType = "block") {
    let el = document.getElementById(id);
    if (el) { el.classList.remove("hidden"); el.style.setProperty("display", displayType, "important"); }
}

function hideElement(id) {
    let el = document.getElementById(id);
    if (el) { el.classList.add("hidden"); el.style.setProperty("display", "none", "important"); }
}

function logMessage(msg) {
    const logEl = document.getElementById("log");
    if (!logEl) return;
    const li = document.createElement("li");
    li.innerText = msg;
    logEl.appendChild(li);
    logEl.scrollTop = logEl.scrollHeight; 
}

function showWaitingRoom() { 
    hideElement("lobby-screen"); 
    showElement("waiting-screen"); 
    document.getElementById("display-room-name").innerText = currentRoomName; 
}

function toggleUI(state) {
    showElement("game-screen");

    ["map-container", "reward-screen", "shop-screen", "event-screen", "rest-screen"].forEach(hideElement);
    
    if (state === "battle") {
        showElement("ui-layer"); 
    } else if (state === "map") { 
        hideElement("ui-layer");
        showElement("map-container"); 
    } 
    else if (state === "shop") { showElement("shop-screen"); } 
    else if (state === "event") { showElement("event-screen"); } 
    else if (state === "rest") { showElement("rest-screen"); }
    else if (state === "reward") { showElement("reward-screen"); }
}

// ==========================================
// VYKRESLOVÁNÍ STATISTIK A RUKY
// ==========================================
function updateStatsUI() {
    if(document.getElementById("mana-value")) document.getElementById("mana-value").innerText = myMana;
    if(document.getElementById("gold-value")) document.getElementById("gold-value").innerText = myGold;
    if(document.getElementById("deck-count")) document.getElementById("deck-count").innerText = `🎴 V balíčku: ${myDrawPileCount}`;
    if(document.getElementById("discard-count")) document.getElementById("discard-count").innerText = `🗑️ Odhozeno: ${myDiscardPileCount}`;
    if(document.getElementById("hero-hp")) document.getElementById("hero-hp").innerText = myHp;
    if(document.getElementById("hero-max-hp")) document.getElementById("hero-max-hp").innerText = myMaxHp;
    if(document.getElementById("hero-block")) document.getElementById("hero-block").innerText = myBlock;
}

function renderHand() {
    const handContainer = document.getElementById("hand-container"); if (!handContainer) return; handContainer.innerHTML = ""; 
    myHand.forEach(cardId => {
        const cData = getCardData(cardId); const cardElement = document.createElement("div"); cardElement.className = "card interactive-ui"; 
        let color = "#ecf0f1"; if(cData.karmaShift < 0) color = "#ffcccc"; if(cData.karmaShift > 0) color = "#ccffcc"; 
        
        cardElement.innerHTML = `<div class="card-cost">${cData.cost}</div><strong>${cData.name}</strong>`;
        cardElement.title = `Efekt: ${cData.description}`;
        
        cardElement.onclick = () => { playCard(cardId, cData.karmaShift, cData.damage); };
        cardElement.style.cssText = `background: ${color}; color: #2c3e50;`;
        
        handContainer.appendChild(cardElement);
    });
}

// --- PŘIDÁNO: Vykreslování relikvií ---
function updateRelicsUI(relicsList) {
    const relicsContainer = document.getElementById("relics-list"); 
    if (!relicsContainer) return;
    relicsContainer.innerHTML = ""; 
    if (!relicsList || relicsList.length === 0) { 
        relicsContainer.innerText = "Zatím žádné"; 
        return; 
    }
    relicsList.forEach(r => {
        const span = document.createElement("span"); 
        span.innerText = `[${safeGet(r, 'name', 'Name')}] `;
        span.title = safeGet(r, 'description', 'Description'); 
        span.style.cursor = "help"; 
        span.style.borderBottom = "2px dotted #2c3e50"; 
        span.style.marginRight = "10px";
        relicsContainer.appendChild(span);
    });
}

// ==========================================
// MODÁLNÍ OKNA (Balíček, Vylepšování)
// ==========================================
function showModalWithCards(title, cardIds) {
    document.getElementById("card-modal-title").innerText = title;
    const container = document.getElementById("card-modal-content"); if(!container) return; container.innerHTML = "";
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

function openUpgradeModal() {
    const container = document.getElementById("upgrade-modal-content"); 
    if(!container) return; container.innerHTML = "";
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

// ==========================================
// RENDER SPECIFICKÝCH OKEN (Shop, Event)
// ==========================================
function leaveShop() { toggleUI("map"); renderMap(); }

function renderShopUI(shopCards, shopRelics, removeCost) {
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

    renderShopRemoveDeck(removeCost);
}

function renderShopRemoveDeck(removeCost) {
    const removeContainer = document.getElementById("shop-remove-deck"); 
    if(!removeContainer) return; removeContainer.innerHTML = "";
    myStartingDeck.forEach(cardId => {
        const cData = getCardData(cardId);
        const btn = document.createElement("button"); btn.innerText = cData.name;
        btn.style.cssText = "padding: 5px 10px; background: #e74c3c; color: white; border: none; border-radius: 3px; cursor: pointer;";
        btn.onclick = () => removeCardFromDeck(cardId, removeCost || 50);
        removeContainer.appendChild(btn);
    });
}

function renderEventUI(eventData) {
    toggleUI("event");
    document.getElementById("event-title").innerText = safeGet(eventData, 'title', 'Title');
    document.getElementById("event-desc").innerText = safeGet(eventData, 'desc', 'Desc');
    const optionsContainer = document.getElementById("event-options"); optionsContainer.innerHTML = "";
    
    let opts = safeGet(eventData, 'options', 'Options') || [];
    let eventId = safeGet(eventData, 'id', 'Id'); 
    
    opts.forEach(opt => {
        const btn = document.createElement("button");
        btn.innerText = safeGet(opt, 'text', 'Text');
        btn.style.cssText = "padding: 15px 30px; font-size: 16px; background: #8e44ad; color: white; border: none; border-radius: 5px; cursor: pointer; transition: 0.2s;";
        btn.onclick = () => {
            optionsContainer.innerHTML = "<p>Vybráno! Čekáme na server...</p>";
            connection.invoke("ResolveEventOption", currentRoomName, playerName, eventId, safeGet(opt, 'id', 'Id')).catch(err => console.error(err));
        };
        optionsContainer.appendChild(btn);
    });
}

function renderRewardScreenUI(cardChoices, relicChoice, goldReward) {
    logMessage(`🎉 Výhra!`); toggleUI("reward"); 
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
}

// ==========================================
// VYKRESLOVÁNÍ MAPY A UZLŮ
// ==========================================
function renderMap() {
    const mapContainer = document.getElementById("nodes-list"); if (!mapContainer) return;
    mapContainer.innerHTML = ""; mapContainer.style.position = "relative"; mapContainer.style.display = "flex";
    mapContainer.style.flexDirection = "column-reverse"; mapContainer.style.gap = "50px"; mapContainer.style.padding = "20px";

    const legend = document.createElement("div");
    legend.style.cssText = "position: absolute; right: 20px; top: 20px; background: rgba(44, 62, 80, 0.9); padding: 15px; border-radius: 8px; border: 2px solid #f1c40f; color: white; font-size: 14px; z-index: 10; box-shadow: 0 4px 10px rgba(0,0,0,0.5); text-align: left;";
    legend.innerHTML = `
        <h4 style="margin: 0 0 10px 0; text-align: center; color: #f1c40f; border-bottom: 1px solid #f1c40f; padding-bottom: 5px;">Legenda</h4>
        <div style="margin-bottom: 5px;">⚔️ Běžný souboj</div>
        <div style="margin-bottom: 5px;">👹 Elitní souboj</div>
        <div style="margin-bottom: 5px;">👑 Boss</div>
        <div style="margin-bottom: 5px;">💰 Poklad</div>
        <div style="margin-bottom: 5px;">⛺ Táborák</div>
        <div style="margin-bottom: 5px;">🛒 Obchod</div>
        <div>📜 Událost</div>
    `;
    mapContainer.appendChild(legend);

    if (!gameMap || gameMap.length === 0) return;

    let currentNode = gameMap.find(n => safeGet(n, 'id', 'Id') === myCurrentNodeId);
    let validNextNodeIds = currentNode ? (safeGet(currentNode, 'connectedTo', 'ConnectedTo') || []) : [];

    let floorValues = gameMap.map(n => { let f = safeGet(n, 'floor', 'Floor'); return (typeof f === "number") ? f : 0; });
    const maxFloor = floorValues.length > 0 ? Math.max(...floorValues) : 0;

    for (let f = 0; f <= maxFloor; f++) {
        const floorNodes = gameMap.filter(n => { let nf = safeGet(n, 'floor', 'Floor'); return (typeof nf === "number" ? nf : 0) === f; });
        if (floorNodes.length === 0) continue;

        const row = document.createElement("div");
        row.style.display = "flex"; row.style.justifyContent = "center"; row.style.gap = "80px"; row.style.zIndex = "2"; 
        
        floorNodes.forEach(node => {
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
                btn.style.backgroundColor = "#34495e"; btn.style.opacity = "0.8"; btn.disabled = true;
            }
            
            btnWrapper.appendChild(btn); row.appendChild(btnWrapper);
        });
        mapContainer.appendChild(row);
    }
    
    // OPRAVA: Zpoždění 500ms kvůli CSS animaci zmenšení/zvětšení okna
    setTimeout(() => drawMapLines(), 500);
}

function drawMapLines() {
    const container = document.getElementById("map-container"); 
    if (!container) return;

    let oldSvg = document.getElementById("map-svg"); if (oldSvg) oldSvg.remove();
    const svg = document.createElementNS("http://www.w3.org/2000/svg", "svg");
    svg.id = "map-svg"; 
    svg.style.cssText = "position: absolute; top: 0; left: 0; width: 100%; height: 100%; z-index: 1; pointer-events: none;";
    
    const containerRect = container.getBoundingClientRect();

    gameMap.forEach(node => {
        let nodeId = safeGet(node, 'id', 'Id');
        const fromEl = document.getElementById(`node-${nodeId}`); if (!fromEl) return;
        const fromRect = fromEl.getBoundingClientRect();

        let connectedTo = safeGet(node, 'connectedTo', 'ConnectedTo') || [];
        connectedTo.forEach(targetId => {
            const toEl = document.getElementById(`node-${targetId}`); if (!toEl) return;
            const toRect = toEl.getBoundingClientRect();

            const x1 = (fromRect.left + fromRect.width / 2) - containerRect.left; 
            const y1 = (fromRect.top + fromRect.height / 2) - containerRect.top;
            const x2 = (toRect.left + toRect.width / 2) - containerRect.left; 
            const y2 = (toRect.top + toRect.height / 2) - containerRect.top;

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