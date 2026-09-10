using MonopolyServidor.Comunicacion;

ServidorTcp servidor = new ServidorTcp(5000);

await servidor.IniciarAsync();

