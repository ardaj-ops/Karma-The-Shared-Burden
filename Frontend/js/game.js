// Uprav tuto adresu podle toho, co ti vygeneroval Render!
const connection = new signalR.HubConnectionBuilder()
    .withUrl("https://karma-backend-xy12.onrender.com/gamehub") 
    .build();
const playerName = "Hráč " + Math.floor(Math.random() * 1000);
let currentRoomName = ""; 
let hasPlayedThisTurn = false;
let isGameOver = false; // Nová pojistka proti hraní po konci hry

// --- DECK A MANA SYSTÉM ---
let myHand = []; 
let myMana = 0;  

// --- LOKÁLNÍ DATABÁZE KARET ---
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

connection.on("GameStarted", (roomName) => {
    currentRoomName = roomName;
    logMessage(`🔥 Hra začala! Bojujete proti společnému Bossovi!`);
});

// Zpracování úvodního rozdání karet
connection.on("ReceiveInitialState", (hand, mana) => {
    myHand = hand;
    myMana = mana;
    
    logMessage(`🃏 Lízl sis ${hand.length} karet. Tvoje Mana: ${mana}`);
    
    updateManaUI();
    renderHand(); 
});

// Zpracování začátku nového kola
connection.on("ReceiveNewTurnState", (updatedHand, updatedMana) => {
    myHand = updatedHand;
    myMana = updatedMana;
    
    logMessage(`🔄 Začíná nové kolo! Tvoje Mana je obnovena a dostal jsi novou kartu.`);
    
    updateManaUI();
    renderHand(); 
});

connection.on("PlayerReadiedUp", (player) => {
    logMessage(`⏳ ${player} si vybral kartu a čeká na ostatní...`);
});

connection.on("TurnResolved", (summary, totalDamage, newKarma, enemyHp) => {
    logMessage(`--- TAH VYHODNOCEN ---`);
    summary.forEach(msg => logMessage(`👉 ${msg}`));
    logMessage(`💥 Tým udělil ${totalDamage} zranění! Boss má ${enemyHp} HP.`);
    
    document.getElementById("enemy-hp").innerText = enemyHp;
    document.getElementById("karma-value").innerText = newKarma;
    
    hasPlayedThisTurn = false; 
    logMessage(`Nové kolo začíná! Vyberte kartu.`);
});

// --- NOVÉ: Zpracování konce hry ---
connection.on("GameOver", (message) => {
    isGameOver = true; // Zablokujeme další hraní
    logMessage(`🏆 ${message}`);
    alert(message); // Vyskočí okno s oznámením vítězství
    
    // Vyčistíme karty z obrazovky, už je nebudeme potřebovat
    const handContainer = document.getElementById("hand-container");
    if (handContainer) {
        handContainer.innerHTML = "<h3>Hra skončila!</h3>";
    }
});

function logMessage(message) {
    const li = document.createElement("li");
    li.textContent = message;
    document.getElementById("log").prepend(li);
}

connection.start().then(() => {
    logMessage(`Připojeno. Tvoje jméno: ${playerName}. Hledám hru...`);
    connection.invoke("JoinGame", playerName);
}).catch(err => console.error(err.toString()));

// --- KONTROLA A ZAHRÁNÍ KARTY ---
function playCard(cardId, karmaShift, damage) {
    if (!currentRoomName) {
        alert("Hra ještě nezačala!");
        return;
    }
    if (isGameOver) {
        alert("Hra už skončila!");
        return;
    }
    if (hasPlayedThisTurn) {
        alert("Už jsi zahrál kartu! Počkej na ostatní.");
        return;
    }
    if (!myHand.includes(cardId)) {
        alert("Tuto kartu nemáš v ruce!");
        return;
    }

    // Zkontrolujeme cenu karty
    const cardCost = cardDatabase[cardId] ? cardDatabase[cardId].cost : 1;
    
    if (myMana < cardCost) {
        alert("Nemáš dostatek Many!");
        return;
    }

    // Vše v pořádku -> Zamykáme tah a odečítáme manu lokálně
    hasPlayedThisTurn = true; 
    myMana -= cardCost; 
    myHand = myHand.filter(id => id !== cardId); 
    
    updateManaUI();
    renderHand(); 

    logMessage(`Vybral jsi kartu a čeká se na zbytek týmu...`);
    
    connection.invoke("SelectCard", currentRoomName, playerName, cardId, karmaShift, damage)
        .catch(err => console.error(err.toString()));
}

// --- POMOCNÉ FUNKCE PRO VYKRESLENÍ ---
function updateManaUI() {
    const manaElement = document.getElementById("mana-value");
    if (manaElement) {
        manaElement.innerText = myMana;
    }
}

function renderHand() {
    const handContainer = document.getElementById("hand-container");
    if (!handContainer) return; 

    handContainer.innerHTML = ""; 

    myHand.forEach(cardId => {
        const cardData = cardDatabase[cardId] || { name: "Neznámá karta", cost: 1, karmaShift: 0, damage: 5 };

        const cardElement = document.createElement("button");
        cardElement.className = "card"; 
        
        cardElement.innerHTML = `
            <strong>${cardData.name}</strong><br>
            <em>Cena: ${cardData.cost} Many</em><br>
            Poškození: ${cardData.damage}<br>
            Karma: ${cardData.karmaShift}
        `;

        cardElement.onclick = () => {
            playCard(cardId, cardData.karmaShift, cardData.damage);
        };

        handContainer.appendChild(cardElement);
    });
}