// ==========================================
// 3D PROMĚNNÉ
// ==========================================
let scene, camera, renderer;
let players3D = {}; 
let enemies3D = {}; 
let arenaObjects = []; // NOVÉ: Pole pro ukládání překážek a stěn arény
let is3DActive = false; 
let myPosition = { x: 0, y: 0, z: 0 };

const keys = { w: false, a: false, s: false, d: false };
const mouse = new THREE.Vector2(0, 0); 
const raycaster = new THREE.Raycaster();
let targetedEnemyId = null; 

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
    
    // Světla
    scene.add(new THREE.AmbientLight(0xffffff, 0.4));
    const directionalLight = new THREE.DirectionalLight(0xffffff, 0.6);
    directionalLight.position.set(10, 20, 10);
    scene.add(directionalLight);

    // Vykreslení náhodné arény
    generateArena();

    document.addEventListener("keydown", onKeyDown);
    document.addEventListener("keyup", onKeyUp);
    document.addEventListener("mousemove", onMouseMove);

    canvas.addEventListener("click", () => {
        if(is3DActive) canvas.requestPointerLock();
    });

    // Reset pozice při vstupu
    myPosition = { x: 0, y: 0, z: 0 };
    camera.position.set(0, 1.6, 0);

    animate3D();
}

// --- NOVÉ: GENERÁTOR ARÉNY ---
function generateArena() {
    // 1. Úklid staré arény
    arenaObjects.forEach(obj => scene.remove(obj));
    arenaObjects = [];

    // 2. Náhodná barva mlhy a oblohy pro atmosféru
    const atmospheres = [0x1e272e, 0x2c3e50, 0x192a56, 0x2c2c54, 0x222f3e];
    const bgColor = atmospheres[Math.floor(Math.random() * atmospheres.length)];
    scene.background = new THREE.Color(bgColor);
    scene.fog = new THREE.Fog(bgColor, 10, 60); // Mlha, která pohlcuje okraje arény

    // 3. Temná pevná podlaha
    const floorGeo = new THREE.PlaneGeometry(100, 100);
    const floorMat = new THREE.MeshLambertMaterial({ color: 0x111111 });
    const floor = new THREE.Mesh(floorGeo, floorMat);
    floor.rotation.x = -Math.PI / 2;
    scene.add(floor);
    
    // Zachováme mřížku pro Cyber-Fantasy pocit (těsně nad podlahou)
    const grid = new THREE.GridHelper(100, 100, 0x8e44ad, 0x34495e);
    grid.position.y = 0.01; 
    scene.add(grid);

    // 4. Náhodné překážky (Kameny, Sloupy)
    const geoBox = new THREE.BoxGeometry(2, 6, 2);
    const geoCyl = new THREE.CylinderGeometry(1.5, 1.5, 8, 8);
    const matObs = new THREE.MeshLambertMaterial({ color: 0x555555 }); // Šedý kámen

    const numObstacles = 12 + Math.floor(Math.random() * 15); // 12 až 27 překážek
    
    for(let i = 0; i < numObstacles; i++) {
        const isBox = Math.random() > 0.5;
        const mesh = new THREE.Mesh(isBox ? geoBox : geoCyl, matObs);
        
        // Náhodná pozice
        let ox = (Math.random() - 0.5) * 80;
        let oz = (Math.random() - 0.5) * 80;
        
        // Bezpečná zóna pro spawn hráčů uprostřed (nesmí tu být překážka)
        if (Math.abs(ox) < 8 && Math.abs(oz) < 8) {
            ox += 10 * Math.sign(ox || 1);
            oz += 10 * Math.sign(oz || 1);
        }

        mesh.position.set(ox, isBox ? 3 : 4, oz);
        mesh.rotation.y = Math.random() * Math.PI;
        
        mesh.userData = { isObstacle: true }; // Značka pro kolize
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

// --- NOVÉ: KOLIZNÍ SYSTÉM ---
function checkCollision(nx, nz) {
    // 1. Zastaví tě na okraji mapy (Zdi arény)
    if (nx > 45 || nx < -45 || nz > 45 || nz < -45) return true;
    
    // 2. Zastaví tě, pokud narazíš do sloupu/překážky
    for (let obj of arenaObjects) {
        if (obj.userData.isObstacle) {
            let dx = nx - obj.position.x;
            let dz = nz - obj.position.z;
            let distance = Math.sqrt(dx * dx + dz * dz);
            if (distance < 2.0) return true; // 2 metry hitbox sloupu
        }
    }
    return false;
}

function animate3D() {
    if (!is3DActive) return;
    requestAnimationFrame(animate3D);

    // POHYB A KOLIZE
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
            // Hráč se může pohnout jen tehdy, pokud na nové souřadnici NENÍ překážka
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

    // Zaměřování Raycasterem (Zahrnuje monstra, překážkám se vyhýbá ve výběru)
    raycaster.setFromCamera(mouse, camera);
    const intersects = raycaster.intersectObjects(scene.children);
    
    targetedEnemyId = null;
    const targetInfoEl = document.getElementById("target-info");
    if(targetInfoEl) targetInfoEl.innerText = "";
    
    for (let i = 0; i < intersects.length; i++) {
        const obj = intersects[i].object;
        
        // Zastavení zaměřovače, pokud se díváme do sloupu (překážka brání ve výhledu na monstrum)
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

    enemiesData.forEach(eData => {
        let eHp = safeGet(eData, 'hp', 'Hp');
        let eId = safeGet(eData, 'id', 'Id');
        let eName = safeGet(eData, 'name', 'Name');
        
        if (eHp <= 0) {
            if (enemies3D[eId]) { scene.remove(enemies3D[eId]); delete enemies3D[eId]; }
            return;
        }

        if (!enemies3D[eId]) {
            const geometry = new THREE.BoxGeometry(1.5, 2.5, 1.5);
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
        mesh.position.y = 1.25; // Úprava výšky
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
}

function spawn3DHitEffect(x, y, z, damage) {
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
}