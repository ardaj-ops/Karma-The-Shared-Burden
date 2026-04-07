// ==========================================
// GLOBÁLNÍ STAVOVÉ PROMĚNNÉ
// ==========================================
let playerName = ""; let playerClass = ""; let currentRoomName = ""; 
let isGameOver = false; 
let gameMap = []; let cardDatabase = {}; 

let myHand = []; let myMana = 0; let myGold = 0;
let myDrawPile = []; let myDiscardPile = []; let myStartingDeck = [];
let myDrawPileCount = 0; let myDiscardPileCount = 0;
let myHp = 0; let myMaxHp = 0; let myBlock = 0;

let myCurrentNodeId = -1;
let currentEnemiesArray = []; 
let currentTeamData = []; 
let currentMapVotes = {}; 
let currentRelicReward = null;

// ==========================================
// POMOCNÉ FUNKCE A DATA
// ==========================================
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
    let card = cardDatabase[cardId];
    if (card) return { ...card, id: cardId };
    
    let isUpgraded = cardId.endsWith("+");
    let baseId = isUpgraded ? cardId.slice(0, -1) : cardId;
    let base = cardDatabase[baseId];
    
    if (!base) return { id: cardId, name: cardId, cost: 1, karmaShift: 0, damage: 5, description: "" };
    
    if (isUpgraded) {
        return {
            id: cardId, name: "⭐ " + base.name,
            damage: (base.damage > 0) ? base.damage + 3 : 0, block: (base.block > 0) ? base.block + 3 : 0, heal: (base.heal > 0) ? base.heal + 2 : 0,
            cost: Math.max(0, base.cost - 1), karmaShift: base.karmaShift, description: base.description + " (Automatické vylepšení)"
        };
    }
    return { ...base, id: cardId };
}

function getCredentials() {
    playerName = document.getElementById("player-name-input").value.trim();
    currentRoomName = document.getElementById("room-name-input").value.trim();
    if (!playerName || !currentRoomName || !playerClass) { alert("Musíš vyplnit všechny údaje (Jméno, Místnost i Hrdinu)!"); return false; }
    localStorage.setItem("karmaPlayerName", playerName);
    return true;
}

// ==========================================
// HERNÍ AKCE HRÁČE
// ==========================================
function pickHero(hero) { 
    playerClass = hero; 
    document.getElementById("selected-hero-text").innerText = hero; 
    document.querySelectorAll(".hero-btn").forEach(btn => {
        btn.style.boxShadow = "none";
        btn.style.border = "2px solid transparent";
        if (btn.innerHTML.includes(hero)) {
            btn.style.boxShadow = "0 0 15px #f1c40f";
            btn.style.border = "2px solid #f1c40f";
        }
    });
}

function playCard(cardId, karmaShift, damage) {
    if (isGameOver) return; 
    
    const cardData = getCardData(cardId);
    if (myMana < cardData.cost) { logMessage("⚡ Málo many!"); return; }

    if (cardData.damage > 0 && !targetedEnemyId) {
        logMessage("🎯 Musíš se dívat na nepřítele (zaměřovací kříž)!");
        return;
    }

    let targetId = cardData.damage > 0 ? targetedEnemyId : "";
    let targetAlly = ""; 
    
    try {
        connection.invoke("CastCard", currentRoomName, playerName, cardId, karmaShift, targetId, targetAlly);
    } catch (err) {
        console.error(err);
    }
}

function buyShopItem(itemId, type, price) { 
    if (myGold < price) { alert("Nemáš dost zlaťáků!"); return; } 
    connection.invoke("BuyShopItem", currentRoomName, playerName, itemId, type, price).catch(err => console.error(err)); 
}

function removeCardFromDeck(cardId, price) { 
    if (myGold < price) { alert("Nemáš dost zlaťáků!"); return; } 
    connection.invoke("RemoveCardFromDeck", currentRoomName, playerName, cardId, price).catch(err => console.error(err)); 
}

function chooseRestHeal() { 
    connection.invoke("RestPlaceAction", currentRoomName, playerName, "heal", "").catch(err => console.error(err)); 
}

function chooseRestUpgrade(cardId) { 
    closeUpgradeModal(); 
    connection.invoke("RestPlaceAction", currentRoomName, playerName, "upgrade", cardId).catch(err => console.error(err)); 
}

function skipReward() {
    let relId = currentRelicReward ? safeGet(currentRelicReward, 'id', 'Id') : "";
    let relName = currentRelicReward ? safeGet(currentRelicReward, 'name', 'Name') : "";
    let relDesc = currentRelicReward ? safeGet(currentRelicReward, 'description', 'Description') : "";
    connection.invoke("ClaimReward", currentRoomName, playerName, "", relId, relName, relDesc).catch(err => console.error(err));
}