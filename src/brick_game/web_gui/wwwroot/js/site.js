const apiUrl = 'http://localhost:5109/api/game/';
const canvas = document.getElementById('gameCanvas');
const ctx = canvas.getContext('2d');
const cellSize = 20;

let gameStarted = false; 

async function startGame(gameId) {
    console.log('Starting game with ID:', gameId);

    let gameName;
    switch (gameId) {
        case 1:
            gameName = 'Snake Game';
            break;
        case 2:
            gameName = 'Tetris Game';
            break;
        case 3:
            gameName = 'Race Game';
            break;
        default:
            gameName = 'Unknown Game';
            break;
    }

    try {
        const response = await fetch(apiUrl + 'start', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(gameId),
        });

        if (!response.ok) {
            throw new Error(`Error starting game: ${response.statusText}`);
        }

        console.log('Game started successfully');
        gameStarted = true;
        document.getElementById('state-output').textContent = `Started ${gameName}`;
    } catch (error) {
        console.error(error);
        document.getElementById('state-output').textContent = `Error starting game: ${error.message}`;
    }
}

async function updateGameState() {
    if (!gameStarted) return;

    console.log('Fetching game state from:', apiUrl + 'state');
    try {
        const response = await fetch(apiUrl + 'state');
        console.log('Response Status:', response.status);

        if (!response.ok) {
            const responseBody = await response.text();
            console.log('Response Body:', responseBody);
            throw new Error(`Failed to fetch game state: ${response.status} ${response.statusText}`);
        }

        const state = await response.json();
        console.log('Game state:', state);
        renderGame(state);

    } catch (error) {
        console.error('Error fetching game state:', error);
        document.getElementById('state-output').textContent = 'Error fetching game state: ' + error.message;
    }
}

function renderGame(state) {
    if (!canvas || !ctx) {
        console.error('Canvas or context is not available');
        return;
    }

    ctx.clearRect(0, 0, canvas.width, canvas.height);
    for (let row = 0; row < state.field.length; row++) {
        for (let col = 0; col < state.field[row].length; col++) {
            const value = state.field[row][col];
            if (value > 0) {
                ctx.fillStyle = value === 9 ? 'blue' : 'red'; // Пример цвета для разных значений
                ctx.fillRect(col * cellSize, 20 + row * cellSize, cellSize, cellSize);
            }
            ctx.strokeRect(col * cellSize, 20 + row * cellSize, cellSize, cellSize); // Рисуем границы клеток
        }
    }

    ctx.font = '20px Arial';
    ctx.fillStyle = 'black';
    ctx.fillText(`Score: ${state.score}`, 250, 40);
    ctx.fillText(`High Score: ${state.highScore}`, 250, 70);
    ctx.fillText(`Level: ${state.level}`, 250, 100);
}

window.addEventListener('keydown', (event) => {
    const actionType = mapKeyToAction(event.key);
    if (actionType) {
        postAction(actionType);
    }
});

async function postAction(actionType) {
    try {
        const response = await fetch(apiUrl + 'actions', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(actionType)
        });

        if (!response.ok) {
            const responseBody = await response.text();
            throw new Error(`Failed to post action: ${response.status} ${response.statusText}. Response: ${responseBody}`);
        }

        await updateGameState();
    } catch (error) {
        console.error('Error posting action:', error);
        document.getElementById('state-output').textContent = 'Error posting action: ' + error.message;
    }
}

function mapKeyToAction(key) {
    switch (key) {
        case 'ArrowUp': return 'Up';
        case 'ArrowDown': return 'Down';
        case 'ArrowLeft': return 'Left';
        case 'ArrowRight': return 'Right';
        case ' ': return 'Action';
        case 'Enter': return 'Start';
        default: return 'Nothing';
    }
}

const updateInterval = 100;
setInterval(updateGameState, updateInterval);
