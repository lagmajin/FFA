let connection = null;
let started = false;

function ensureConnection() {
    if (!connection && typeof signalR !== 'undefined') {
        connection = new signalR.HubConnectionBuilder()
            .withUrl('/worldhub')
            .withAutomaticReconnect()
            .build();
    }
    return connection;
}

export async function start(dotnetRef) {
    try {
        const conn = ensureConnection();
        if (!conn) {
            console.warn('SignalR not available yet');
            return;
        }
        
        if (started) {
            console.log('WorldHub already started');
            return;
        }

        conn.on('WorldUpdated', (payload) => {
            if (dotnetRef) {
                try {
                    dotnetRef.invokeMethodAsync('OnWorldUpdated', JSON.stringify(payload));
                } catch (e) {
                    console.warn('OnWorldUpdated invoke failed:', e);
                }
            }
        });
        
        conn.on('KarmaUpdated', (payload) => {
            if (dotnetRef) {
                try {
                    dotnetRef.invokeMethodAsync('OnKarmaUpdated', JSON.stringify(payload));
                } catch (e) {
                    console.warn('OnKarmaUpdated invoke failed:', e);
                }
            }
        });

        if (conn.state === 'Disconnected') {
            await conn.start();
            started = true;
            console.log('WorldHub connected');
        }
    } catch (err) {
        console.error('WorldHub start error:', err);
    }
}

export async function stop() {
    try {
        if (connection && connection.state !== 'Disconnected') {
            await connection.stop();
            console.log('WorldHub disconnected');
        }
    } catch (err) {
        console.error('WorldHub stop error:', err);
    }
}
