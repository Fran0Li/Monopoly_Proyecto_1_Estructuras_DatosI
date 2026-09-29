# Monopoly Distribuido — Estructuras Lineales

> Proyecto 1 — Algoritmos y Estructuras de Datos I | Instituto Tecnológico de Costa Rica | II Semestre 2026

![C#](https://img.shields.io/badge/C%23-.NET%2010-512BD4?style=flat&logo=dotnet)
![Windows Forms](https://img.shields.io/badge/GUI-Windows%20Forms-0078D4?style=flat)
![TCP](https://img.shields.io/badge/Comunicación-TCP-00599C?style=flat)
![MSTest](https://img.shields.io/badge/Pruebas-MSTest-68217A?style=flat)
![MicroPython](https://img.shields.io/badge/Hardware-MicroPython-2B2728?style=flat&logo=micropython)
![Proyecto](https://img.shields.io/badge/Proyecto-ITCR-B5121B?style=flat)

---

## Descripción

**Monopoly Distribuido** es una implementación simplificada de Monopoly Electrónico desarrollada principalmente en C# bajo una arquitectura cliente-servidor.

El proyecto tiene como objetivo aplicar estructuras de datos lineales implementadas manualmente dentro de una aplicación distribuida. La partida es administrada de forma centralizada por un servidor, mientras varios clientes se conectan mediante sockets TCP para realizar acciones y recibir actualizaciones del estado del juego.

El sistema integra además un módulo físico programado en MicroPython que funciona como dado electrónico y lector RFID. Las tarjetas RFID permiten identificar a los jugadores durante determinadas operaciones económicas, mientras que el saldo oficial y todas las reglas de la partida se mantienen exclusivamente en el servidor.

La temática visual utilizada para el tablero está inspirada en el Instituto Tecnológico de Costa Rica.

---

## Arquitectura del Sistema

El proyecto se divide en cuatro componentes principales:

```text
┌─────────────────────────────┐
│       MonopolyCliente       │
│       Windows Forms         │
└──────────────┬──────────────┘
               │
               │ TCP / JSON
               │
               ▼
┌─────────────────────────────┐
│      MonopolyServidor       │
│        Servidor TCP         │
└──────────────┬──────────────┘
               │
               ▼
┌─────────────────────────────┐
│        MonopolyCore         │
│                             │
│ Juego · Banco · Tablero     │
│ Jugadores · Transacciones   │
│ Estructuras de Datos        │
└──────────────┬──────────────┘
               │
               │ TCP
               ▼
┌─────────────────────────────┐
│       Módulo Hardware       │
│ RFID · Dados · Botón físico │
│       MicroPython           │
└─────────────────────────────┘
```

### Servidor

`MonopolyServidor` administra las conexiones TCP de los jugadores y del hardware.

Sus principales responsabilidades son:

- recibir las peticiones de los clientes;
- identificar el origen de cada mensaje;
- validar que la conexión corresponda al jugador indicado;
- delegar las acciones al núcleo del juego;
- enviar respuestas al cliente correspondiente;
- distribuir eventos y actualizaciones del estado;
- manejar las desconexiones;
- mantener la comunicación con el módulo físico.

Las reglas propias del Monopoly no se implementan directamente en el servidor. La lógica central de la partida se encuentra en la clase `Juego`.

### Núcleo del Juego

`MonopolyCore` contiene la lógica y el estado oficial de la partida.

La clase `Juego` funciona como orquestador central y coordina:

- jugadores;
- turnos;
- movimiento;
- dados;
- tablero;
- compra de propiedades;
- pagos;
- Banco;
- tarjetas RFID;
- cartas de evento;
- eliminación de jugadores;
- condiciones de finalización;
- historial de transacciones.

### Cliente

`MonopolyCliente` es una aplicación Windows Forms utilizada por cada jugador.

La interfaz gráfica únicamente solicita acciones al servidor y representa el estado recibido. El cliente no modifica directamente elementos del estado oficial como:

- saldo;
- posición;
- propiedades;
- turno;
- dados;
- transacciones.

Toda acción debe ser validada y ejecutada por el servidor y por la lógica central del juego.

### Hardware

El módulo físico se comunica con el servidor mediante TCP y utiliza MicroPython.

El sistema integra:

- lector RFID RC522;
- tarjetas o llaveros RFID;
- dos displays de 7 segmentos;
- botón físico para lanzar los dados;
- buzzer para retroalimentación;
- conexión Wi-Fi.

---

## Estructuras de Datos

Las principales estructuras utilizadas en el proyecto fueron implementadas manualmente.

No se utilizan directamente `List`, `LinkedList`, `Queue`, `Stack`, `PriorityQueue` o estructuras equivalentes para sustituir las estructuras evaluadas en el curso.

| Estructura | Uso principal |
|---|---|
| `ListaCircularDoble<T>` | Representación del tablero y recorrido circular de casillas |
| `ColaCircular<T>` | Administración del orden y rotación de turnos |
| `ListaDoblementeEnlazada<T>` | Historial de transacciones y otras colecciones internas |
| `NodoDoble<T>` | Nodo utilizado por las estructuras enlazadas |

### Lista Circular Doblemente Enlazada

El tablero utiliza una lista circular doblemente enlazada.

Cada nodo representa una casilla y mantiene referencias al nodo anterior y al siguiente, permitiendo desplazamientos en ambas direcciones y múltiples vueltas completas sobre el tablero.

El tablero está compuesto por un mínimo de 24 casillas.

```text
┌─────────┐     ┌─────────┐     ┌─────────┐
│ Casilla │ ⇄   │ Casilla │ ⇄   │ Casilla │
└────┬────┘     └─────────┘     └────┬────┘
     ↑                               │
     └───────────────────────────────┘
```

### Cola Circular

Los turnos se administran mediante una cola circular.

La estructura mantiene al jugador que posee el turno actual y permite avanzar continuamente entre todos los participantes.

```text
Jugador A → Jugador B → Jugador C → Jugador D
    ↑                                     ↓
    └─────────────────────────────────────┘
```

Cuando un jugador termina su turno, la cola avanza automáticamente al siguiente.

Si un jugador queda eliminado o abandona la partida, se elimina de la cola sin romper su circularidad.

### Lista Doblemente Enlazada

El Banco utiliza una lista doblemente enlazada para almacenar el historial de transacciones.

La estructura permite:

- agregar transacciones;
- recorrer desde la transacción más antigua;
- recorrer desde la más reciente;
- buscar por jugador;
- buscar por tipo;
- consultar el historial completo.

---

## Funcionamiento General de la Partida

El flujo general del sistema es:

```text
Jugador
   │
   ▼
Cliente Windows Forms
   │
   │ Petición TCP / JSON
   ▼
ServidorTcp
   │
   ▼
Juego
   │
   ├── Validaciones
   ├── Turnos
   ├── Tablero
   ├── Banco
   ├── Dados
   └── Transacciones
   │
   ▼
ResultadoAccion
   │
   ▼
ServidorTcp
   │
   ├── Respuesta al jugador
   └── Notificación a los demás clientes
```

El servidor mantiene en todo momento la fuente oficial del estado de la partida.

---

## Funcionalidades Principales

### Administración de la Partida

- Registro de jugadores.
- Inicio de la partida por parte del organizador.
- Orden de turnos aleatorio.
- Administración mediante cola circular.
- Lanzamiento de dos dados.
- Movimiento nodo por nodo sobre el tablero.
- Compra de propiedades.
- Pago de alquiler.
- Casillas especiales.
- Cartas de evento.
- Cárcel y pérdida de turnos.
- Premio por pasar por Inicio.
- Eliminación de jugadores.
- Cálculo de patrimonio.
- Límite configurable de turnos.
- Determinación automática del ganador.

### Banco

El Banco centraliza las operaciones económicas del sistema.

Permite:

- pagos de jugador al Banco;
- pagos del Banco a un jugador;
- pagos entre jugadores;
- validación de saldo;
- registro automático de transacciones;
- consulta del historial;
- búsqueda de transacciones por jugador;
- búsqueda de transacciones por tipo;
- exportación del historial a un archivo TXT.

### RFID

El sistema permite vincular una tarjeta RFID con cada jugador.

El flujo general es:

```text
Jugador debe realizar un pago
          │
          ▼
Servidor solicita RFID
          │
          ▼
Jugador acerca tarjeta
          │
          ▼
Hardware lee UID
          │
          ▼
Servidor identifica jugador
          │
          ▼
Juego valida operación
          │
          ▼
Banco realiza el pago
          │
          ▼
Se registra la transacción
```

El saldo no se almacena en la tarjeta RFID. La tarjeta únicamente funciona como mecanismo de identificación.

### Hardware

El hardware permite:

- vincular tarjetas RFID;
- identificar jugadores;
- confirmar pagos mediante RFID;
- mostrar los valores de los dos dados;
- lanzar los dados mediante un botón físico;
- ofrecer retroalimentación mediante displays y buzzer.

---

## Validaciones

La lógica central impide realizar operaciones inválidas.

| Situación | Resultado |
|---|---|
| Jugar fuera de turno | Rechazado |
| Lanzar los dados múltiples veces en el mismo turno | Rechazado |
| Comprar antes de lanzar los dados | Rechazado |
| Comprar sin saldo suficiente | Rechazado |
| Comprar una propiedad que ya tiene propietario | Rechazado |
| Terminar el turno con un pago RFID pendiente | Rechazado |
| Realizar acciones después de ser eliminado | Rechazado |
| Reconectarse después de abandonar una partida activa | Rechazado |
| Modificar directamente el estado desde el cliente | No permitido |

---

## Desconexión de Jugadores

Durante una partida activa, una desconexión se considera un abandono.

Cuando un jugador pierde su conexión:

1. se marca como inactivo;
2. libera sus propiedades;
3. se elimina de la cola circular de turnos;
4. se cancelan las operaciones pendientes relacionadas con el jugador;
5. si poseía el turno actual, el juego continúa con el siguiente participante;
6. el jugador no puede volver a conectarse a esa misma partida.

Si después de una desconexión queda únicamente un jugador activo, la partida finaliza automáticamente.

Antes de iniciar la partida, un jugador registrado puede reconectarse utilizando su identificador.

---

## Fin de la Partida

La partida puede finalizar de dos formas.

### Único Jugador Activo

Si todos los demás jugadores fueron eliminados o abandonaron la partida, el jugador restante es declarado ganador.

### Límite Máximo de Turnos

La partida posee una cantidad máxima configurable de turnos.

Cuando se alcanza este límite, gana el jugador activo que posea el mayor patrimonio.

El patrimonio se calcula mediante:

```text
Patrimonio = Saldo + Valor de las propiedades
```

Al finalizar también se genera un ranking de los jugadores según su patrimonio.

El sistema administra una partida por ejecución del servidor. Para iniciar una partida completamente nueva se reinician el servidor y los clientes, garantizando que todas las estructuras y estados comiencen limpios.

---

## Protocolo Cliente-Servidor

La comunicación utiliza sockets TCP y mensajes JSON.

Cada mensaje contiene información como:

```text
TipoMensaje
Accion
JugadorId
Datos
Timestamp
```

### Acciones Principales

| Acción | Descripción |
|---|---|
| `CONECTAR` | Registra un jugador o dispositivo de hardware |
| `INICIAR_JUEGO` | Inicia la partida |
| `TIRAR_DADOS` | Solicita el lanzamiento de dados |
| `COMPRAR_PROPIEDAD` | Solicita la compra de una propiedad |
| `NO_COMPRAR` | Rechaza una compra disponible |
| `TERMINAR_TURNO` | Finaliza el turno actual |
| `CONSULTAR_ESTADO` | Obtiene el estado completo de la partida |
| `CONSULTAR_TRANSACCIONES` | Consulta el historial económico |
| `VINCULAR_RFID` | Solicita vincular una tarjeta |
| `RFID_DETECTADO` | Envía un UID desde el hardware |
| `BOTON_PRESIONADO` | Informa una pulsación del botón físico |
| `EXPORTAR_TRANSACCIONES` | Genera el archivo TXT del historial |

### Notificaciones

El servidor puede distribuir eventos como:

```text
JUGADOR_CONECTADO
JUEGO_INICIADO
ESTADO_ACTUALIZADO
JUGADOR_MOVIDO
COMPRA_DISPONIBLE
PROPIEDAD_COMPRADA
PAGO_PENDIENTE
PAGO_REALIZADO
CARTA_TOMADA
TURNO_CAMBIADO
TURNO_PERDIDO
JUGADOR_ELIMINADO
FIN_JUEGO
MENSAJE_JUEGO
```

---

## Historial de Transacciones

Toda operación económica relevante genera una transacción.

Cada transacción almacena información como:

```text
Identificador
Fecha y hora
Número de turno
Tipo
Jugador origen
Jugador destino
Monto
Descripción
```

Entre los tipos de transacción se encuentran:

- compra de propiedad;
- pago de alquiler;
- pago al Banco;
- pago entre jugadores;
- ganancia por evento;
- pérdida por evento;
- premio por pasar por Inicio.

El historial puede exportarse en formato TXT para su consulta posterior.

---

## Estructura del Repositorio

```text
Monopoly_Proyecto_1_Estructuras_DatosI/
│
├── MonopolyCore/
│   │
│   ├── Comunicacion/
│   │   ├── DatosHardware.cs
│   │   ├── DatosJuego.cs
│   │   └── Protocolo.cs
│   │
│   ├── Estructuras/
│   │   ├── ColaCircular.cs
│   │   ├── DoublyLinkedList.cs
│   │   ├── ListaCircularDoble.cs
│   │   └── NodoDoble.cs
│   │
│   ├── Juego/
│   │   ├── ConfiguracionPartida.cs
│   │   ├── Juego.cs
│   │   └── ResultadoAccion.cs
│   │
│   ├── Modelos/
│   │   ├── Banco.cs
│   │   ├── CartaEvento.cs
│   │   ├── Casilla.cs
│   │   ├── CasillaEvento.cs
│   │   ├── CasillaEspecial.cs
│   │   ├── ClaseJugador.cs
│   │   ├── Dado.cs
│   │   ├── Enums.cs
│   │   ├── Propiedad.cs
│   │   ├── Tablero.cs
│   │   └── Transaccion.cs
│   │
│   └── MonopolyCore.csproj
│
├── MonopolyServidor/
│   │
│   ├── Comunicacion/
│   │   └── ServidorTcp.cs
│   │
│   ├── Program.cs
│   └── MonopolyServidor.csproj
│
├── MonopolyCliente/
│   │
│   ├── Comunicacion/
│   │   └── ClienteMonopoly.cs
│   │
│   ├── Interfaz/
│   │   ├── Colores.cs
│   │   ├── DatosMensaje.cs
│   │   └── TableroControl.cs
│   │
│   ├── Imagenes/
│   ├── FormLogin.cs
│   ├── FormJuego.cs
│   ├── FormHistorial.cs
│   └── MonopolyCliente.csproj
│
├── MonopolyCore.Tests/
│   ├── BancoTests.cs
│   ├── ColaCircularTests.cs
│   ├── DadoTests.cs
│   ├── NodoDobleTests.cs
│   ├── ProtocoloTests.cs
│   └── MonopolyCore.Tests.csproj
│
├── hardware/
│   ├── hardware_monopoly.py
│   ├── Prueba_aislada_RFID.py
│   ├── Prueba_aisada_ambosdados.py
│   └── Prueba dado 1.py
│
├── docs/
│   ├── Documentacion_Hardware_conexion/
│   └── protocolo/
│
├── MonopolyProject.slnx
├── .gitignore
└── README.md
```

---

## Requisitos

### Software

- .NET 10 SDK.
- Windows para ejecutar el cliente Windows Forms.
- Git.
- Visual Studio, Visual Studio Code o un entorno compatible con .NET.
- Conexión de red entre las computadoras que participarán en la partida.

### Hardware

Para ejecutar la integración física completa se utilizan:

- Raspberry Pi Pico W o dispositivo compatible;
- lector RFID RC522;
- tarjetas o llaveros RFID;
- dos displays de 7 segmentos;
- botón pulsador;
- buzzer;
- resistencias;
- cables y protoboard;
- red Wi-Fi disponible.

---

## Compilación

Desde la raíz del repositorio:

```bash
dotnet restore
dotnet build MonopolyProject.slnx
```

Para ejecutar las pruebas:

```bash
dotnet test
```

Una compilación correcta debe finalizar sin errores antes de integrar nuevos cambios en la rama principal.

---

## Ejecución

### 1. Iniciar el Servidor

Desde la raíz del proyecto:

```bash
dotnet run --project MonopolyServidor
```

El servidor utiliza por defecto el puerto:

```text
5000
```

La configuración principal de la partida se encuentra en:

```text
MonopolyServidor/Program.cs
```

Allí pueden modificarse parámetros como:

```csharp
saldoInicial
premioPorInicio
maxTurnos
minJugadores
```

Para la demostración oficial debe utilizarse la cantidad de jugadores indicada por la consigna del proyecto.

---

### 2. Iniciar los Clientes

En cada computadora Windows:

```bash
dotnet run --project MonopolyCliente
```

La pantalla de conexión solicita:

```text
Nombre del jugador
Dirección IP del servidor
Puerto
```

El puerto utilizado por defecto es:

```text
5000
```

Todos los clientes deben poder comunicarse mediante red con la computadora donde se ejecuta `MonopolyServidor`.

---

### 3. Configurar el Hardware

Antes de ejecutar el programa del hardware deben configurarse los datos de red en:

```text
hardware/hardware_monopoly.py
```

Utilizando valores locales:

```python
WIFI_SSID = "NOMBRE_RED"
WIFI_PASSWORD = "CONTRASENA_RED"

SERVIDOR_IP = "IP_DEL_SERVIDOR"
SERVIDOR_PUERTO = 5000
```

No se recomienda almacenar credenciales reales de redes Wi-Fi dentro del repositorio.

El dispositivo también debe disponer del controlador correspondiente para el lector RFID RC522.

---

## Pruebas

El proyecto utiliza MSTest para verificar componentes importantes del sistema.

Actualmente se incluyen pruebas relacionadas con:

- Banco;
- pagos;
- validaciones de saldo;
- historial de transacciones;
- exportación de transacciones;
- cola circular;
- avance de turnos;
- eliminación de elementos de la cola;
- dado;
- nodos;
- protocolo de comunicación.

Para ejecutar todas las pruebas:

```bash
dotnet test
```

Además de las pruebas unitarias, el sistema puede validarse mediante pruebas manuales ejecutando múltiples clientes simultáneamente.

---

## Documentación

La carpeta `docs/` contiene documentación complementaria del proyecto.

```text
docs/
├── Documentacion_Hardware_conexion/
└── protocolo/
```

Incluye documentación relacionada con:

- conexión del hardware;
- integración del RFID;
- protocolo TCP;
- comunicación entre los diferentes componentes del sistema.

---

## Ramas del Proyecto

| Rama | Propósito |
|---|---|
| `main` | Versión integrada y estable del proyecto |
| `feature/servidor-A` | Servidor, Banco, validaciones y turnos |
| `Persona-B` | Tablero, casillas y estructuras relacionadas |
| `Persona-C` | Jugadores, propiedades y transacciones |
| `featur/Cliente_D` | Desarrollo del cliente |
| `feature/GUI_cliente` | Interfaz gráfica del cliente |
| `feature/Hardware-RFID_D` | Integración RFID y hardware |
| `feature/Juego_Orquestador` | Integración de la lógica central de `Juego` |
| `micro_códigos-D` | Pruebas y desarrollo relacionado con MicroPython |

La rama `main` representa la versión integrada utilizada para las pruebas finales y la entrega.

---

## Principios de Diseño

El proyecto mantiene una separación clara de responsabilidades.

```text
Cliente
   │
   │ Solicita
   ▼
ServidorTcp
   │
   │ Delega
   ▼
Juego
   │
   ├── Banco
   ├── Tablero
   ├── ColaCircular
   ├── Dados
   └── Jugadores
```

### Cliente

Se encarga únicamente de la presentación y comunicación.

### Servidor

Se encarga del transporte de mensajes y administración de conexiones.

### Juego

Es la fuente central de verdad y contiene las reglas de la partida.

### Banco

Centraliza las operaciones económicas y el historial.

### Estructuras

Administran los datos utilizando implementaciones desarrolladas específicamente para el curso.

---

## Programación Orientada a Objetos

El proyecto aplica conceptos de programación orientada a objetos como:

- encapsulamiento;
- composición;
- herencia;
- polimorfismo;
- separación de responsabilidades.

Las diferentes casillas derivan de una clase base común y ejecutan comportamientos particulares al recibir un jugador.

Entre ellas se encuentran:

```text
Casilla
├── Propiedad
├── CasillaEvento
└── CasillaEspecial
```

Esto permite que el tablero trabaje con diferentes tipos de casillas manteniendo una interfaz común.

---

## Objetivos Académicos Aplicados

El proyecto fue desarrollado como parte del curso **Algoritmos y Estructuras de Datos I** y se enfoca principalmente en:

- implementación manual de estructuras de datos lineales;
- manejo de nodos;
- listas doblemente enlazadas;
- listas circulares;
- colas circulares;
- recorrido de estructuras;
- eliminación e inserción de elementos;
- programación orientada a objetos;
- herencia y polimorfismo;
- arquitectura cliente-servidor;
- comunicación mediante sockets TCP;
- administración de múltiples jugadores;
- sincronización de estado;
- pruebas unitarias;
- integración entre software y hardware.

---

## Equipo de Desarrollo

| Integrante | Responsabilidad principal |
|---|---|
| Esteban Sánchez | Servidor, Banco, validaciones y administración de turnos |
| Francisco | Cliente, protocolo e integración de hardware |
| Julián | Tablero, casillas, herencia y polimorfismo |
| Andrew | Jugadores, propiedades, transacciones e historial |

---

## Curso

**Instituto Tecnológico de Costa Rica**  
Escuela de Ingeniería en Computación  
Algoritmos y Estructuras de Datos I  
Proyecto 1  
II Semestre 2026

---

## Estado del Proyecto

La rama `main` contiene la versión integrada del sistema.

El proyecto incluye:

```text
Servidor TCP
Cliente Windows Forms
Lógica central del Monopoly
Banco
Tablero circular
Cola circular de turnos
Historial de transacciones
Sistema de propiedades
Cartas de evento
RFID
Dados electrónicos
Interfaz gráfica
Pruebas unitarias
Documentación técnica
Integración con hardware
```

El desarrollo prioriza la aplicación correcta de estructuras de datos lineales dentro de una arquitectura distribuida, manteniendo el estado oficial de la partida centralizado en el servidor.
