// ==========================================
// 3D PROMĚNNÉ
// ==========================================
let scene, camera, renderer;
let players3D = {}; 
let enemies3D = {}; 
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
    scene.background = new THREE.Color(0x1e272e);
    scene.fog = new THREE.Fog(0x1e272e, 10, 50);

    camera = new THREE.PerspectiveCamera(75, window.innerWidth / window.innerHeight, 0.1, 1000);
    
    const canvas = document.getElementById("game-canvas");
    if (!renderer) {
        renderer = new THREE.WebGLRenderer({ canvas: canvas, antialias: true });
        renderer.setSize(window.innerWidth, window.innerHeight);
    }
    
    scene.add(new THREE.AmbientLight(0xffffff, 0.4));
    const directionalLight = new THREE.DirectionalLight(0xffffff, 0.6);
    directionalLight.position.set(10, 20, 10);
    scene.add(directionalLight);

    scene.add(new THREE.GridHelper(100, 100, 0x8e44ad, 0x2c3e50));

    document.addEventListener("keydown", onKeyDown);
    document.addEventListener("keyup", onKeyUp);
    document.addEventListener("mousemove", onMouseMove);

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

// --- Klávesové zkratky (1, 2, 3...) i s Českou klávesnicí (+, ě, š...) ---
document.addEventListener("keydown", (event) => {
    if (!is3DActive) return;
    
    let num = parseInt(event.key);
    
    // Podpora pro českou horní řadu kláves
    const czechKeys = { '+': 1, 'ě': 2, 'š': 3, 'č': 4, 'ř': 5, 'ž': 6, 'ý': 7, 'á': 8, 'í': 9, 'é': 0 };
    if (czechKeys[event.key.toLowerCase()]) {
        num = czechKeys[event.key.toLowerCase()];
    }

    if (!isNaN(num) && num >= 1 && num <= myHand.length) {
        const cardId = myHand[num - 1]; 
        const cData = getCardData(cardId);
        playCard(cardId, cData.karmaShift, cData.damage);
    }
});

function animate3D() {
    if (!is3DActive) return;
    requestAnimationFrame(animate3D);

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

        if (moved && typeof connection !== 'undefined') {
            connection.invoke("MovePlayer", currentRoomName, playerName, myPosition.x, 0, myPosition.z).catch(err=>{});
        }
    }

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