const connection = new signalR.HubConnectionBuilder()
    .withUrl("https://karma-the-shared-burden.onrender.com/gamehub") 
    .build();

const playerName = "Hráč " + Math.floor(Math.random() * 1000);
let currentRoomName = ""; 
let hasPlayedThisTurn = false;
let isGameOver = false; 
let gameMap = []; // Paměť pro strukturu mapy

// --- DECK A MANA SYSTÉM ---
let myHand = []; 
let myMana = 0;  

const cardDatabase = {
    "Py_50": { name: "Ohnivá koule", cost: 1, karmaShift: -5, damage: 50 },
    "Card_01": { name: "Základní útok", cost: 1, karmaShift: 0, damage: 10 },
    "Card_02": { name: "Rychlý švih", cost: 1, karmaShift: 0, damage: 15 },
    "Card_03": { name: "Léčení", cost: 2, karmaShift: 10, damage: 0 },
    "Card_04": { name: "Obrana", cost: 1, karmaShift: 5, damage: 0 },
    "Card_05": { name: "Magický úder", cost: 2, karmaShift: -2, damage: 30 },
    "Card_06": { name: "Karmický výbuch", cost: 3, karmaShift: -15, damage: 60 },
    "Card_07": { name: "Meditace", cost: 1, karmaShift: 20, damage: 0 }
};

connection.on("PlayerJoinedWaitingRoom", (player, currentCount, needed) => {
    logMessage(`Čekárna: ${player} se připojil. (${currentCount}/${needed})`);
});

// --- UPRAVENO: Přijímáme i mapu ---
connection.on("GameStarted", (roomName, initialMap) => {
    currentRoomName = roomName;
    gameMap = initialMap;
    logMessage(`🔥 Hra začala! Cesta před vámi se odhaluje.`);
    // Zatím mapu jen uložíme, vykreslí se až když server pošle "EnteredNode"
});

connection.on("ReceiveInitialState", (hand, mana) => {
    myHand = hand;
    myMana = mana;
    updateManaUI();
    renderHand(); 
});

connection.on("ReceiveNewTurnState", (updatedHand, updatedMana) => {
    myHand = updatedHand;
    myMana = updatedMana;
    logMessage(`🔄 Začíná nové kolo!`);
    updateManaUI();
    renderHand(); 
    hasPlayedThisTurn = false; // Odemkneme hráče
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

// --- NOVÉ: Zpracování výhry v boji a přesun na mapu ---
connection.on("BattleWon", (message) => {
    logMessage(`🎉 ${message}`);
    hasPlayedThisTurn = false;
    
    // Schováme UI pro boj (karty) a ukážeme mapu
    toggleUI("map");
    renderMap();
});

// --- NOVÉ: Zpracování vstupu do místnosti ---
connection.on("EnteredNode", (nodeType, nodeData) => {
    logMessage(`📍 Jste v: ${nodeType}`);
    
    if (nodeType === "Encounter" || nodeType === "EliteEncounter" || nodeType === "Boss") {
        // Pokud je to boj, zapneme karty
        toggleUI("battle");
    } else {
        // Jinak zapneme obrazovku s mapou/eventem
        toggleUI("map");
        // Vykreslíme i to, v jakém uzlu jsme a kam můžeme dál
        renderMap();
    }
});

connection.on("GameOver", (message) => {
    isGameOver = true; 
    logMessage(`🏆 ${message}`);
    alert(message); 
    toggleUI("none"); // Schová vše
});

function logMessage(message) {
    const li = document.createElement("li");
    li.textContent = message;
    document.getElementById("log").prepend(li);
}

connection.start().then(() => {
    logMessage(`Připojeno. Jméno: ${playerName}. Hledám hru...`);
    connection.invoke("JoinGame", playerName);
}).catch(err => console.error(err.toString()));

function playCard(cardId, karmaShift, damage) {
    if (!currentRoomName) { alert("Hra nezačala!"); return; }
    if (isGameOver) { alert("Hra skončila!"); return; }
    if (hasPlayedThisTurn) { alert("Už jsi hrál!"); return; }
    if (!myHand.includes(cardId)) { alert("Kartu nemáš v ruce!"); return; }

    const cardCost = cardDatabase[cardId] ? cardDatabase[cardId].cost : 1;
    if (myMana < cardCost) { alert("Málo Many!"); return; }

    hasPlayedThisTurn = true; 
    myMana -= cardCost; 
    myHand = myHand.filter(id => id !== cardId); 
    
    updateManaUI();
    renderHand(); 

    logMessage(`Karta zahrána, čekám...`);
    connection.invoke("SelectCard", currentRoomName, playerName, cardId, karmaShift, damage)
        .catch(err => console.error(err.toString()));
}

// --- POMOCNÉ FUNKCE PRO UI ---
function updateManaUI() {
    const manaElement = document.getElementById("mana-value");
    if (manaElement) manaElement.innerText = myMana;
}

function renderHand() {
    const handContainer = document.getElementById("hand-container");
    if (!handContainer) return; 

    handContainer.innerHTML = ""; 

    myHand.forEach(cardId => {
        const cardData = cardDatabase[cardId] || { name: "Neznámá", cost: 1, karmaShift: 0, damage: 5 };
        const cardElement = document.createElement("button");
        cardElement.className = "card"; 
        cardElement.innerHTML = `<strong>${cardData.name}</strong><br><em>${cardData.cost} Many</em><br>Dmg: ${cardData.damage}<br>Karma: ${cardData.karmaShift}`;
        cardElement.onclick = () => playCard(cardId, cardData.karmaShift, cardData.damage);
        handContainer.appendChild(cardElement);
    });
}

// --- NOVÉ: Správa zobrazení ---
function toggleUI(state) {
    // Máme dva hlavní stavy: "battle" (boj) a "map" (cestování)
    const battleUI = document.getElementById("hand-container");
    const mapUI = document.getElementById("map-container"); // Toto musíš přidat do index.html
    
    if (!mapUI) return; // Pokud ještě nemáš mapu v HTML, přeskoč

    if (state === "battle") {
        battleUI.style.display = "flex";
        mapUI.style.display = "none";
    } else if (state === "map") {
        battleUI.style.display = "none";
        mapUI.style.display = "block";
    } else {
        battleUI.style.display = "none";
        mapUI.style.display = "none";
    }
}

// --- NOVÉ: Vykreslení mapy ---
function renderMap() {
    const mapContainer = document.getElementById("map-container");
    if (!mapContainer) return;
    
    // Vyčistíme a připravíme
    mapContainer.innerHTML = "<h3>Mapa</h3>";
    
    // Najdeme uzel, ve kterém se zrovna nacházíme (není ideální to hledat jen podle isCompleted, ale pro lineární mapu to stačí)
    // Lepší by bylo, kdyby server posílal ID aktuálního uzlu v události "EnteredNode"
    // Pro teď zobrazíme vše a zvýrazníme, kam se dá jít
    
    gameMap.forEach(node => {
        const btn = document.createElement("button");
        btn.innerText = `Patro ${node.floor}: ${node.type}`;
        btn.style.margin = "5px";
        
        // Zvýraznění uzlů - šedé = už tam nejde jít, zelené = lze tam jít
        if (node.isCompleted) {
            btn.style.backgroundColor = "gray";
            btn.disabled = true;
        } else {
            btn.style.backgroundColor = "#2ecc71";
            btn.onclick = () => {
                // Zavoláme server, ať nás posune
                logMessage("Pokouším se přejít dál...");
                connection.invoke("MoveToNextNode", currentRoomName, node.id)
                    .catch(err => console.error(err));
            };
        }
        
        mapContainer.appendChild(btn);
    });
}