const connection = new signalR.HubConnectionBuilder()
    .withUrl('/worldhub')
    .withAutomaticReconnect()
    .build();

export function start(dotnetRef) {
    connection.on('WorldUpdated', (payload) => {
        if (dotnetRef) dotnetRef.invokeMethodAsync('OnWorldUpdated', JSON.stringify(payload));
    });
    connection.on('KarmaUpdated', (payload) => {
        if (dotnetRef) dotnetRef.invokeMethodAsync('OnKarmaUpdated', JSON.stringify(payload));
    });
    connection.start().catch(err => console.error(err.toString()));
}

export function stop() {
    connection.stop();
}
