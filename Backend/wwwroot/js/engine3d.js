// ==========================================
// 3D PROMĚNNÉ A ENGINE
// ==========================================
let scene, camera, renderer;
let players3D = {}; 
let enemies3D = {}; 
let arenaObjects = []; // Zdi a překážky
let is3DActive = false; 
let myPosition = { x: 0, y: 0, z: 0 };

const keys = { w: false, a: false, s: false, d: false };
const mouse = new THREE.Vector2(0, 0); 
const raycaster = new THREE.Raycaster();
let targetedEnemyId = null; 

// --- POMOCNÁ FUNKCE: Vytvoření textové jmenovky ve 3D ---
function createLabel(text, color) {
    const canvas = document.createElement('canvas');
    canvas.width = 256; canvas.height = 128;
    const ctx = canvas.getContext('2d');
    ctx.font = 'bold 28px "Segoe UI", sans-serif';
    ctx.textAlign = 'center';
    ctx.fillStyle = color;
    ctx.shadowColor = "black";
    ctx.shadowBlur = 5;
    ctx.lineWidth = 4;
    ctx.strokeText(text, 128, 64);
    ctx.fillText(text, 128, 64);
    
    const texture = new THREE.CanvasTexture(canvas);
    const spriteMat = new THREE.SpriteMaterial({ map: texture, depthTest: false });
    const sprite = new THREE.Sprite(spriteMat);
    sprite.scale.set(6, 3, 1);
    return sprite;
}

function init3DScene() {
    if (is3DActive) return; 
    is3DActive = true;

    scene = new THREE.Scene();
    
    camera = new THREE.PerspectiveCamera(75, window.innerWidth / window.innerHeight, 0.1, 1000);
    
    const canvas = document.getElementById("game-canvas");
    if (!renderer) {
        renderer = new THREE.WebGLRenderer({ canvas: canvas, antialias: true });
        renderer.setSize(window.innerWidth, window.innerHeight);
    }
    
    scene.add(new THREE.AmbientLight(0xffffff, 0.5)); 
    const directionalLight = new THREE.DirectionalLight(0xffffff, 0.7);
    directionalLight.position.set(20, 30, 10);
    scene.add(directionalLight);

    generateArena();

    document.addEventListener("keydown", onKeyDown);
    document.addEventListener("keyup", onKeyUp);
    document.addEventListener("mousemove", onMouseMove);

    canvas.addEventListener("mousedown", (event) => {
        if(is3DActive && document.pointerLockElement !== canvas) {
            canvas.requestPointerLock();
            return;
        }

        if (is3DActive && event.button === 0 && document.pointerLockElement === canvas) {
            if (typeof connection !== 'undefined') {
                connection.invoke("RechargeManaClick", currentRoomName, playerName).catch(err => console.error(err));
            }
            
            const manaEl = document.getElementById("mana-value");
            if (manaEl) {
                manaEl.style.transform = "scale(1.5)";
                setTimeout(() => manaEl.style.transform = "scale(1)", 150);
            }
        }
    });

    myPosition = { x: 0, y: 0, z: 0 };
    camera.position.set(0, 1.6, 0);

    animate3D();
}

function generateArena() {
    arenaObjects.forEach(obj => scene.remove(obj));
    arenaObjects = [];

    const bgColor = 0x1e272e;
    scene.background = new THREE.Color(bgColor);
    scene.fog = new THREE.Fog(bgColor, 10, 70); 

    const floorGeo = new THREE.PlaneGeometry(100, 100);
    const floorMat = new THREE.MeshLambertMaterial({ color: 0x111111 });
    const floor = new THREE.Mesh(floorGeo, floorMat);
    floor.rotation.x = -Math.PI / 2;
    scene.add(floor);
    
    const grid = new THREE.GridHelper(90, 90, 0x8e44ad, 0x34495e);
    grid.position.y = 0.01; 
    scene.add(grid);

    const wallMat = new THREE.MeshBasicMaterial({ color: 0x8e44ad, wireframe: true, transparent: true, opacity: 0.3 });
    const wallGeo = new THREE.PlaneGeometry(90, 10);
    
    const wallN = new THREE.Mesh(wallGeo, wallMat); wallN.position.set(0, 5, -45); scene.add(wallN); arenaObjects.push(wallN);
    const wallS = new THREE.Mesh(wallGeo, wallMat); wallS.position.set(0, 5, 45); wallS.rotation.y = Math.PI; scene.add(wallS); arenaObjects.push(wallS);
    const wallE = new THREE.Mesh(wallGeo, wallMat); wallE.position.set(45, 5, 0); wallE.rotation.y = -Math.PI/2; scene.add(wallE); arenaObjects.push(wallE);
    const wallW = new THREE.Mesh(wallGeo, wallMat); wallW.position.set(-45, 5, 0); wallW.rotation.y = Math.PI/2; scene.add(wallW); arenaObjects.push(wallW);

    const geoBox = new THREE.BoxGeometry(2.5, 8, 2.5);
    const geoCyl = new THREE.CylinderGeometry(1.5, 1.5, 10, 8);
    const matObs = new THREE.MeshLambertMaterial({ color: 0x555555 }); 

    const numObstacles = 15 + Math.floor(Math.random() * 10); 
    
    for(let i = 0; i < numObstacles; i++) {
        const isBox = Math.random() > 0.5;
        const mesh = new THREE.Mesh(isBox ? geoBox : geoCyl, matObs);
        
        let ox = (Math.random() - 0.5) * 80;
        let oz = (Math.random() - 0.5) * 80;
        
        if (Math.abs(ox) < 10 && Math.abs(oz) < 10) {
            ox += 12 * Math.sign(ox || 1);
            oz += 12 * Math.sign(oz || 1);
        }

        mesh.position.set(ox, isBox ? 4 : 5, oz);
        mesh.rotation.y = Math.random() * Math.PI;
        
        mesh.userData = { isObstacle: true }; 
        scene.add(mesh);
        arenaObjects.push(mesh);
    }
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

document.addEventListener("keydown", (event) => {
    if (!is3DActive) return;
    let num = parseInt(event.key);
    const czechKeys = { '+': 1, 'ě': 2, 'š': 3, 'č': 4, 'ř': 5, 'ž': 6, 'ý': 7, 'á': 8, 'í': 9, 'é': 0 };
    if (czechKeys[event.key.toLowerCase()]) num = czechKeys[event.key.toLowerCase()];

    if (!isNaN(num) && num >= 1 && num <= myHand.length) {
        const cardId = myHand[num - 1]; 
        const cData = getCardData(cardId);
        playCard(cardId, cData.karmaShift, cData.damage);
    }
});

function checkCollision(nx, nz) {
    if (nx > 44 || nx < -44 || nz > 44 || nz < -44) return true; // Zdi
    
    // 1. Kolize se sloupy a překážkami
    for (let obj of arenaObjects) {
        if (obj.userData && obj.userData.isObstacle) {
            let dx = nx - obj.position.x;
            let dz = nz - obj.position.z;
            let distance = Math.sqrt(dx * dx + dz * dz);
            if (distance < 2.5) return true; 
        }
    }

    // 2. OPRAVA: Kolize s monstry (už do nich nevlezeš)
    for (let id in enemies3D) {
        let enemy = enemies3D[id];
        let dx = nx - enemy.position.x;
        let dz = nz - enemy.position.z;
        let distance = Math.sqrt(dx * dx + dz * dz);
        if (distance < 2.0) return true; // Poloměr krystalu je cca 1.5, takže 2.0 tě udrží bezpečně před ním
    }

    return false;
}

function animate3D() {
    if (!is3DActive) return;
    requestAnimationFrame(animate3D);

    if (document.pointerLockElement === document.getElementById("game-canvas")) {
        const speed = 0.2;
        let moved = false;
        
        let newX = myPosition.x;
        let newZ = myPosition.z;

        const direction = new THREE.Vector3();
        camera.getWorldDirection(direction);
        direction.y = 0; direction.normalize();

        const right = new THREE.Vector3().crossVectors(camera.up, direction).normalize();

        if (keys.w) { newX += direction.x * speed; newZ += direction.z * speed; moved = true; }
        if (keys.s) { newX -= direction.x * speed; newZ -= direction.z * speed; moved = true; }
        if (keys.a) { newX += right.x * speed; newZ += right.z * speed; moved = true; }
        if (keys.d) { newX -= right.x * speed; newZ -= right.z * speed; moved = true; }

        if (moved) {
            if (!checkCollision(newX, newZ)) {
                myPosition.x = newX;
                myPosition.z = newZ;
                camera.position.set(myPosition.x, 1.6, myPosition.z); 
                
                if (typeof connection !== 'undefined') {
                    connection.invoke("MovePlayer", currentRoomName, playerName, myPosition.x, 0, myPosition.z).catch(err=>{});
                }
            }
        }
    }

    raycaster.setFromCamera(mouse, camera);
    const intersects = raycaster.intersectObjects(scene.children, true); 
    
    targetedEnemyId = null;
    const targetInfoEl = document.getElementById("target-info");
    if(targetInfoEl) targetInfoEl.innerText = "";
    
    for (let i = 0; i < intersects.length; i++) {
        let obj = intersects[i].object;
        while (obj.parent && obj.parent.type !== "Scene") {
            if (obj.userData.isEnemy || obj.userData.isObstacle) break;
            obj = obj.parent;
        }

        if (obj.userData && obj.userData.isObstacle) break; 

        if (obj.userData && obj.userData.isEnemy && obj.userData.hp > 0) {
            targetedEnemyId = obj.userData.id;
            if(targetInfoEl) targetInfoEl.innerText = `🎯 Cíl: ${obj.userData.name} (${obj.userData.hp} HP)`;
            break;
        }
    }

    renderer.render(scene, camera);
}

function update3DEntities(playersData, enemiesData) {
    if (!is3DActive) return;

    const incomingEnemyIds = enemiesData.map(e => safeGet(e, 'id', 'Id'));
    for (let id in enemies3D) {
        if (!incomingEnemyIds.includes(id)) {
            scene.remove(enemies3D[id]); 
            delete enemies3D[id];        
        }
    }

    enemiesData.forEach(eData => {
        let eHp = safeGet(eData, 'hp', 'Hp');
        let eId = safeGet(eData, 'id', 'Id');
        let eName = safeGet(eData, 'name', 'Name');

        if (!enemies3D[eId]) {
            const geometry = new THREE.ConeGeometry(1.5, 3.5, 4);
            const material = new THREE.MeshLambertMaterial({ color: 0xe74c3c });
            const mesh = new THREE.Mesh(geometry, material);
            
            const label = createLabel(`👹 ${eName}`, "#ff7675");
            label.position.y = 3; 
            mesh.add(label); 

            mesh.userData = { isEnemy: true, id: eId, name: eName }; 
            scene.add(mesh);
            enemies3D[eId] = mesh;
        }
        
        let mesh = enemies3D[eId];
        mesh.userData.hp = eHp;
        mesh.position.x += (safeGet(eData, 'x', 'X') - mesh.position.x) * 0.1;
        mesh.position.z += (safeGet(eData, 'z', 'Z') - mesh.position.z) * 0.1;
        mesh.position.y = 1.75; 
    });

    playersData.forEach(pData => {
        let pName = safeGet(pData, 'name', 'Name');
        if (pName === playerName) return; 
        
        let pHp = safeGet(pData, 'hp', 'Hp');
        if (pHp <= 0) {
            if (players3D[pName]) { scene.remove(players3D[pName]); delete players3D[pName]; }
            return;
        }

        if (!players3D[pName]) {
            const geometry = new THREE.SphereGeometry(1, 16, 16);
            const material = new THREE.MeshLambertMaterial({ color: 0x3498db });
            const mesh = new THREE.Mesh(geometry, material);

            const label = createLabel(`🛡️ ${pName}`, "#74b9ff");
            label.position.y = 2.5; 
            mesh.add(label);

            scene.add(mesh);
            players3D[pName] = mesh;
        }

        let mesh = players3D[pName];
        mesh.position.x += (safeGet(pData, 'x', 'X') - mesh.position.x) * 0.1;
        mesh.position.z += (safeGet(pData, 'z', 'Z') - mesh.position.z) * 0.1;
        mesh.position.y = 1;
    });
}

function spawn3DHitEffect(x, y, z, damage) {
    if (!is3DActive) return;

    // OPRAVA: Kontrola, koho přesně ten útok zasáhl
    let distanceToMe = Math.sqrt(Math.pow(x - myPosition.x, 2) + Math.pow(z - myPosition.z, 2));

    // Pokud je to blíž než 1 jednotka k tvé kameře, dostal jsi zásah ty!
    if (distanceToMe < 1.0) {
        // Vytvoření dynamického zčervenání obrazovky (Vignette) pomocí CSS
        let flashOverlay = document.getElementById("damage-flash-overlay");
        if (!flashOverlay) {
            flashOverlay = document.createElement("div");
            flashOverlay.id = "damage-flash-overlay";
            // Stín svítící z okrajů obrazovky dovnitř
            flashOverlay.style.cssText = "position: fixed; top: 0; left: 0; width: 100vw; height: 100vh; pointer-events: none; z-index: 50; box-shadow: inset 0 0 0px rgba(231, 76, 60, 0); transition: box-shadow 0.1s ease-out;";
            document.body.appendChild(flashOverlay);
        }
        
        // Okamžitě okraje zčervenají
        setTimeout(() => { flashOverlay.style.boxShadow = "inset 0 0 150px rgba(231, 76, 60, 0.9)"; }, 10);
        // A plynule vyblednou zpět do průhledna
        setTimeout(() => { flashOverlay.style.boxShadow = "inset 0 0 0px rgba(231, 76, 60, 0)"; }, 250);
        
        return; // Nechceme, aby se nám do obličeje kreslila žlutá koule
    }

    // Pokud útok zasáhl monstrum nebo někoho jiného, vykreslíme 3D žlutý výbuch
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
}