using MonopolyCore;
using MonopolyServidor.Comunicacion;

// Configuración de la partida (ajustable para la demo).
// minJugadores: 4 para la defensa; 2 sirve para probar con menos compus.
Juego juego = new Juego(
    saldoInicial: 1500,
    premioPorInicio: 200,
    maxTurnos: 20,
    minJugadores: 4);

ServidorTcp servidor = new ServidorTcp(5000, juego);

await servidor.IniciarAsync();

// CodigosError se movió a MonopolyCore.Comunicacion (Protocolo.cs) porque Juego también los usa.