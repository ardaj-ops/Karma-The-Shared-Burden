// --- OPRAVENÉ PŘIPOJENÍ: Automatické znovupřipojení při výpadku ---
const connection = new signalR.HubConnectionBuilder()
    .withUrl("https://karma-the-shared-burden.onrender.com/gamehub") 
    .withAutomaticReconnect() // Tohle zachrání hru před spadnutím při mikro-výpadku internetu
    .build();

// --- ZÁKLADNÍ PROMĚNNÉ ---
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

// ========================================================
// 3D PROMĚNNÉ (THREE.JS)
// ========================================================
let scene, camera, renderer;
let players3D = {}; // Objekt pro ukládání modelů hráčů
let enemies3D = {}; // Objekt pro ukládání modelů monster
let is3DActive = false; // Je zrovna bojový režim?
let myPosition = { x: 0, y: 0, z: 0 };

// Ovládání a zaměřování
const keys = { w: false, a: false, s: false, d: false };
const mouse = new THREE.Vector2(0, 0); // Vždy střed obrazovky
const raycaster = new THREE.Raycaster();
let targetedEnemyId = null; // ID monstra, na které zrovna koukáme

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

// --- NAČTENÍ JMÉNA Z PAMĚTI PROHLÍŽEČE ---
document.addEventListener("DOMContentLoaded", () => {
    const ulozenoJmeno = localStorage.getItem("karmaPlayerName");
    if (ulozenoJmeno) {
        document.getElementById("player-name-input").value = ulozenoJmeno;
    }
});

// --- BEZPEČNÉ ZOBRAZOVÁNÍ A SKRÝVÁNÍ ---
function showElement(id, displayType = "block") {
    let el = document.getElementById(id);
    if (el) {
        el.classList.remove("hidden");
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

// --- DYNAMICKÉ VYLEPŠENÍ KARET ---
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

function logMessage(msg) {
    const logEl = document.getElementById("log");
    if (!logEl) return;
    const li = document.createElement("li");
    li.innerText = msg;
    logEl.appendChild(li);
    logEl.scrollTop = logEl.scrollHeight; 
}

// --- LOBBY A HRA ---
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

function getCredentials() {
    playerName = document.getElementById("player-name-input").value.trim();
    currentRoomName = document.getElementById("room-name-input").value.trim();
    if (!playerName || !currentRoomName || !playerClass) { alert("Musíš vyplnit všechny údaje (Jméno, Místnost i Hrdinu)!"); return false; }
    localStorage.setItem("karmaPlayerName", playerName);
    return true;
}

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

function startGame() { connection.invoke("StartGame", currentRoomName).catch(err => console.error(err)); }

function showWaitingRoom() { hideElement("lobby-screen"); showElement("waiting-screen"); document.getElementById("display-room-name").innerText = currentRoomName; }

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
connection.on("YouAreHost", () => { showElement("start-game-btn", "inline-block"); hideElement("waiting-text"); });

// --- GAME START A AKTUALIZACE STAVŮ ---
connection.on("GameStarted", (roomName, initialMap) => {
    hideElement("waiting-screen");
    showElement("game-screen"); 
    gameMap = initialMap || []; 
    myCurrentNodeId = -1; currentMapVotes = {};
    logMessage(`🔥 Hra začala! Hlasujte pro startovní políčko.`);
    toggleUI("map"); 
    renderMap();
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
        init3DScene(); // Zapnutí 3D pro souboj
    } else {
        stop3DScene(); // Vypnutí 3D mimo souboj
        toggleUI(nodeType.toLowerCase()); 
        if(nodeType === "Shop" || nodeType === "Event" || nodeType === "RestPlace" || nodeType === "Treasure") {
            hideElement("map-container");
        }
    }
});

// ========================================================
// THREE.JS - 3D ENGINE A HERNÍ SMYČKA
// ========================================================

function init3DScene() {
    if (is3DActive) return; 
    is3DActive = true;

    // 1. Založení scény
    scene = new THREE.Scene();
    scene.background = new THREE.Color(0x1e272e);
    scene.fog = new THREE.Fog(0x1e272e, 10, 50);

    // 2. Kamera
    camera = new THREE.PerspectiveCamera(75, window.innerWidth / window.innerHeight, 0.1, 1000);
    
    // 3. Renderer
    const canvas = document.getElementById("game-canvas");
    if (!renderer) {
        renderer = new THREE.WebGLRenderer({ canvas: canvas, antialias: true });
        renderer.setSize(window.innerWidth, window.innerHeight);
    }
    
    // 4. Světla
    const ambientLight = new THREE.AmbientLight(0xffffff, 0.4);
    scene.add(ambientLight);
    const directionalLight = new THREE.DirectionalLight(0xffffff, 0.6);
    directionalLight.position.set(10, 20, 10);
    scene.add(directionalLight);

    // 5. Podlaha arény (Mřížka pro orientaci)
    const gridHelper = new THREE.GridHelper(100, 100, 0x8e44ad, 0x2c3e50);
    scene.add(gridHelper);

    // 6. Události kláves a myši
    document.addEventListener("keydown", onKeyDown);
    document.addEventListener("keyup", onKeyUp);
    document.addEventListener("mousemove", onMouseMove);

    // 7. Zamykání myši pro FPS pohled
    canvas.addEventListener("click", () => {
        if(is3DActive) canvas.requestPointerLock();
    });

    animate3D();
}

function stop3DScene() {
    is3DActive = false;
    document.removeEventListener("keydown", onKeyDown);
    document.removeEventListener("keyup", onKeyUp);
    document.removeEventListener("mousemove", onMouseMove);
    if(document.pointerLockElement) document.exitPointerLock();
}

let pitch = 0; let yaw = 0;
function onMouseMove(event) {
    if (!is3DActive || document.pointerLockElement !== document.getElementById("game-canvas")) return;
    
    yaw -= event.movementX * 0.002;
    pitch -= event.movementY * 0.002;
    pitch = Math.max(-Math.PI/2 + 0.1, Math.min(Math.PI/2 - 0.1, pitch)); 
    
    camera.rotation.set(pitch, yaw, 0, 'YXZ');
}

function onKeyDown(event) { if(keys.hasOwnProperty(event.key.toLowerCase())) keys[event.key.toLowerCase()] = true; }
function onKeyUp(event) { if(keys.hasOwnProperty(event.key.toLowerCase())) keys[event.key.toLowerCase()] = false; }

// --- SMYČKA 3D SCÉNY ---
function animate3D() {
    if (!is3DActive) return;
    requestAnimationFrame(animate3D);

    // Pohyb
    if (document.pointerLockElement === document.getElementById("game-canvas")) {
        const speed = 0.2;
        let moved = false;
        const direction = new THREE.Vector3();
        camera.getWorldDirection(direction);
        direction.y = 0; direction.normalize();

        const right = new THREE.Vector3().crossVectors(camera.up, direction).normalize();

        if (keys.w) { myPosition.x += direction.x * speed; myPosition.z += direction.z * speed; moved = true; }
        if (keys.s) { myPosition.x -= direction.x * speed; myPosition.z -= direction.z * speed; moved = true; }
        if (keys.a) { myPosition.x += right.x * speed; myPosition.z += right.z * speed; moved = true; }
        if (keys.d) { myPosition.x -= right.x * speed; myPosition.z -= right.z * speed; moved = true; }

        camera.position.set(myPosition.x, 1.6, myPosition.z); 

        // Odeslat pozici na server
        if (moved) connection.invoke("MovePlayer", currentRoomName, playerName, myPosition.x, 0, myPosition.z).catch(err=>{});
    }

    // Zaměřování pomocí Raycasteru (hledání objektů před očima)
    raycaster.setFromCamera(mouse, camera);
    const intersects = raycaster.intersectObjects(scene.children);
    
    targetedEnemyId = null;
    const targetInfoEl = document.getElementById("target-info");
    if(targetInfoEl) targetInfoEl.innerText = "";
    
    for (let i = 0; i < intersects.length; i++) {
        const obj = intersects[i].object;
        if (obj.userData && obj.userData.isEnemy && obj.userData.hp > 0) {
            targetedEnemyId = obj.userData.id;
            if(targetInfoEl) targetInfoEl.innerText = `🎯 Cíl: ${obj.userData.name} (${obj.userData.hp} HP)`;
            break;
        }
    }

    renderer.render(scene, camera);
}

// --- PŘÍJEM 3D DAT ZE SERVERU ---
connection.on("Update3DState", (playersData, enemiesData) => {
    if (!is3DActive) return;

    // Vykreslení nepřátel
    enemiesData.forEach(eData => {
        let eHp = safeGet(eData, 'hp', 'Hp');
        let eId = safeGet(eData, 'id', 'Id');
        let eName = safeGet(eData, 'name', 'Name');
        
        if (eHp <= 0) {
            if (enemies3D[eId]) { scene.remove(enemies3D[eId]); delete enemies3D[eId]; }
            return;
        }

        if (!enemies3D[eId]) {
            const geometry = new THREE.BoxGeometry(1, 2, 1);
            const material = new THREE.MeshLambertMaterial({ color: 0xe74c3c });
            const mesh = new THREE.Mesh(geometry, material);
            mesh.userData = { isEnemy: true, id: eId, name: eName }; 
            scene.add(mesh);
            enemies3D[eId] = mesh;
        }
        
        let mesh = enemies3D[eId];
        mesh.userData.hp = eHp;
        mesh.position.x += (safeGet(eData, 'x', 'X') - mesh.position.x) * 0.1;
        mesh.position.z += (safeGet(eData, 'z', 'Z') - mesh.position.z) * 0.1;
        mesh.position.y = 1;
    });

    // Vykreslení ostatních hráčů v týmu
    playersData.forEach(pData => {
        let pName = safeGet(pData, 'name', 'Name');
        if (pName === playerName) return; // Sebe sama nekreslíme
        
        let pHp = safeGet(pData, 'hp', 'Hp');
        if (pHp <= 0) {
            if (players3D[pName]) { scene.remove(players3D[pName]); delete players3D[pName]; }
            return;
        }

        if (!players3D[pName]) {
            const geometry = new THREE.SphereGeometry(0.8, 16, 16);
            const material = new THREE.MeshLambertMaterial({ color: 0x3498db });
            const mesh = new THREE.Mesh(geometry, material);
            scene.add(mesh);
            players3D[pName] = mesh;
        }

        let mesh = players3D[pName];
        mesh.position.x += (safeGet(pData, 'x', 'X') - mesh.position.x) * 0.1;
        mesh.position.z += (safeGet(pData, 'z', 'Z') - mesh.position.z) * 0.1;
        mesh.position.y = 1;
    });
});

connection.on("SpawnHitEffect", (x, y, z, damage) => {
    if (!is3DActive) return;
    const geom = new THREE.SphereGeometry(1, 8, 8);
    const mat = new THREE.MeshBasicMaterial({ color: 0xf1c40f, transparent: true, opacity: 0.8 });
    const flash = new THREE.Mesh(geom, mat);
    flash.position.set(x, 1.5, z);
    scene.add(flash);

    let scale = 1;
    const fadeOut = setInterval(() => {
        scale += 0.2;
        flash.scale.set(scale, scale, scale);
        flash.material.opacity -= 0.1;
        if (flash.material.opacity <= 0) {
            scene.remove(flash);
            clearInterval(fadeOut);
        }
    }, 50);
});

// ========================================================
// HRANÍ KARET V REÁLNÉM ČASE
// ========================================================
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

// ========================================================
// UDÁLOSTI Z BOJE
// ========================================================
connection.on("CardPlayedLog", (player, cardId) => { const cData = getCardData(cardId); logMessage(`🎴 ${player} seslal: ${cData.name}`); });
connection.on("TurnResolved", (summary, totalDamage, newKarma, enemiesArray) => { 
    summary.forEach(s => logMessage(s));
});

// ========================================================
// UI A VYKRESLOVÁNÍ
// ========================================================
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

function toggleUI(state) {
    showElement("game-screen");

    ["map-container", "reward-screen", "shop-screen", "event-screen", "rest-screen"].forEach(hideElement);
    
    if (state === "battle") {
        showElement("ui-layer"); // TOTO OPRAVUJE CHYBĚJÍCÍ BOJ!
    } else if (state === "map") { 
        hideElement("ui-layer");
        showElement("map-container"); 
    } 
    else if (state === "shop") { showElement("shop-screen"); } 
    else if (state === "event") { showElement("event-screen"); } 
    else if (state === "rest") { showElement("rest-screen"); }
    else if (state === "reward") { showElement("reward-screen"); }
}

// ========================================================
// OBCHOD (SHOP)
// ========================================================
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

// ========================================================
// UDÁLOSTI (EVENTY)
// ========================================================
connection.on("EnterEvent", (eventData) => {
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
});

connection.on("EventResolved", (newGold, newHp, newMaxHp) => { myGold = newGold; myHp = newHp; myMaxHp = newMaxHp; updateStatsUI(); toggleUI("map"); renderMap(); });

// ========================================================
// TÁBORÁK (REST PLACE)
// ========================================================
connection.on("EnterRestPlace", () => { toggleUI("rest"); });

function chooseRestHeal() { connection.invoke("RestPlaceAction", currentRoomName, playerName, "heal", "").catch(err => console.error(err)); }

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
function chooseRestUpgrade(cardId) { closeUpgradeModal(); connection.invoke("RestPlaceAction", currentRoomName, playerName, "upgrade", cardId).catch(err => console.error(err)); }
connection.on("RestActionCompleted", (newHp, newDeck) => { myHp = newHp; myStartingDeck = newDeck; updateStatsUI(); logMessage("Odpočinek u táboráku úspěšně dokončen."); toggleUI("map"); renderMap(); });

// ========================================================
// ODMĚNY (REWARDS)
// ========================================================
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

// ========================================================
// MODÁLNÍ OKNA PRO KARTY
// ========================================================
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

// ========================================================
// VYKRESLOVÁNÍ MAPY A UZLŮ
// ========================================================
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
    
    // ZDE JE OPRAVA PRO ČÁRY (Spouštíme bez zbytečného parametru)
    setTimeout(() => drawMapLines(), 100);
}

// ZDE JE OPRAVENÁ FUNKCE PRO KRESLENÍ ČAR
function drawMapLines() {
    // Čáry nyní cílíme do hlavního okna map-container, aby se nerozhodily souřadnice
    const container = document.getElementById("map-container"); 
    if (!container) return;

    let oldSvg = document.getElementById("map-svg"); if (oldSvg) oldSvg.remove();
    const svg = document.createElementNS("http://www.w3.org/2000/svg", "svg");
    svg.id = "map-svg"; 
    // SVG teď překryje celé černé okno s mapou
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

            // Výpočet souřadnic relativně k hlavnímu oknu mapy
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