import network
import socket
import time
import ujson
from machine import Pin


# CONFIGURACIÓN 

WIFI_SSID = "FranLi"
WIFI_PASSWORD = "********"

SERVIDOR_IP = "10.154.57.206"   #  cambiar para las pruebas
SERVIDOR_PUERTO = 5000           # puerto correcto


# CONEXIÓN WIFI

def conectar_wifi():
    wlan = network.WLAN(network.STA_IF) # Raspy se conecta a la red, no crea una
    wlan.active(True)
    wlan.connect(WIFI_SSID, WIFI_PASSWORD)

    print("Conectando a WiFi", end="")
    intentos = 0
    while not wlan.isconnected() and intentos < 20:
        print(".", end="")
        time.sleep(1)
        intentos += 1

    if wlan.isconnected():
        print("\nWiFi conectado. IP local:", wlan.ifconfig()[0])
        return True
    else: #Se acaban los intentos a los 20 segs sin conectar
        print("\nNo se pudo conectar al WiFi.")
        return False


# CONEXIÓN TCP AL SERVIDOR

def conectar_servidor():
    s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)# Socket TCP normal, igual a tcpClient en c sharp por parte del server
    s.connect((SERVIDOR_IP, SERVIDOR_PUERTO))
    print("Conectado al servidor", SERVIDOR_IP, ":", SERVIDOR_PUERTO)

    mensaje_conectar = { #Avisa al serve que la conexion es de la raspy no un jugador, para no meterla en la cola de turnos
        "TipoMensaje": "Peticion",
        "Accion": "CONECTAR",
        "Datos": {"TipoCliente": "Hardware"}
    }
    enviar_mensaje(s, mensaje_conectar)
    return s

def enviar_mensaje(sock, mensaje_dict):
    # NDJSON: el mensaje va como JSON de una sola línea, terminado en \n,
    # para que el servidor sepa dónde termina un mensaje y empieza el siguiente
    texto = ujson.dumps(mensaje_dict) + "\n"
    try:
        sock.send(texto.encode("utf-8"))
        print("Enviado:", texto.strip())
    except OSError:
        print("No se pudo enviar: conexión perdida")
        raise ConexionPerdida()

class ConexionPerdida(Exception): #se lanza cuando el server cierra o se cae la red
    pass


# DISPLAYS DE 7 SEGMENTOS

segmentos_1 = {
    'a': Pin(2, Pin.OUT), 'b': Pin(3, Pin.OUT), 'c': Pin(4, Pin.OUT),
    'd': Pin(5, Pin.OUT), 'e': Pin(6, Pin.OUT), 'f': Pin(7, Pin.OUT),
    'g': Pin(8, Pin.OUT),
}

segmentos_2 = {
    'a': Pin(10, Pin.OUT), 'b': Pin(11, Pin.OUT), 'c': Pin(12, Pin.OUT),
    'd': Pin(13, Pin.OUT), 'e': Pin(14, Pin.OUT), 'f': Pin(15, Pin.OUT),
    'g': Pin(21, Pin.OUT),
}

DIGITOS = {
    1: ['b', 'c'],
    2: ['a', 'b', 'g', 'e', 'd'],
    3: ['a', 'b', 'g', 'c', 'd'],
    4: ['f', 'g', 'b', 'c'],
    5: ['a', 'f', 'g', 'c', 'd'],
    6: ['a', 'f', 'g', 'e', 'c', 'd'],
    0: ['a', 'b', 'c', 'd', 'e', 'f'],  # "00" = tarjeta aceptada
    '-': ['g'],                         # "--" = esperando tarjeta
    'E': ['a', 'd', 'e', 'f', 'g'],     # "EE" = tarjeta rechazada
}

ultimo_dado = (None, None)   # últimos valores del dado, para volver a mostrarlos
esperando_tarjeta = False    # True mientras el server espera una tarjeta (vincular o pagar)
ultimo_cambio_ms = 0         # para alternar dado / "--" sin bloquear el loop
mostrando_guiones = False

def mostrar_numero(segmentos, n):
    for pin in segmentos.values():#Apaga todo primero 
        pin.value(0)
    for letra in DIGITOS.get(n, []):#Prende solo los que formen el numero
        segmentos[letra].value(1)

def apagar_display(segmentos):
    for pin in segmentos.values():
        pin.value(0)

def mostrar_par(v1, v2): # muestra un símbolo en cada display
    mostrar_numero(segmentos_1, v1)
    mostrar_numero(segmentos_2, v2)

def mostrar_ultimo_dado(): # vuelve a dejar el dado en pantalla (o apagado si no hay)
    if ultimo_dado[0] is None:
        apagar_display(segmentos_1)
        apagar_display(segmentos_2)
    else:
        mostrar_par(ultimo_dado[0], ultimo_dado[1])

def actualizar_espera_tarjeta():
    # Mientras se espera tarjeta alterna cada 700 ms entre el dado y "--",
    # así se sigue viendo el dado. No usa sleep: no bloquea el loop.
    global ultimo_cambio_ms, mostrando_guiones
    if not esperando_tarjeta:
        return
    ahora = time.ticks_ms()
    if time.ticks_diff(ahora, ultimo_cambio_ms) >= 700:
        ultimo_cambio_ms = ahora
        mostrando_guiones = not mostrando_guiones
        if mostrando_guiones:
            mostrar_par('-', '-')
        else:
            mostrar_ultimo_dado()

def mostrar_resultado_tarjeta(exito):
    # Feedback cortito después de pasar la tarjeta: "00" aceptada, "EE" rechazada
    global esperando_tarjeta
    if exito:
        esperando_tarjeta = False
        mostrar_par(0, 0)
    else:
        mostrar_par('E', 'E') # sigue esperando: puede pasar la tarjeta correcta
    time.sleep_ms(800)
    mostrar_ultimo_dado()


# RFID 
# (recordar: probar primero con _rreg(0x37) que devuelva 0x91/0x92
# antes de confiar en esta parte)
#Llavero detectado. UID: 05:FF:21:07
#Tarjeta detectada. UID: 8D:70:F6:06


from mfrc522 import MFRC522
lector = MFRC522(sck=18, mosi=19, miso=16, rst=20, cs=17)
buzzer = Pin(22, Pin.OUT)

ultimo_uid_enviado = None

def revisar_rfid(sock):
    global ultimo_uid_enviado
    (estado, tag_type) = lector.request(lector.REQIDL)
    if estado == lector.OK:
        (estado, uid_bytes) = lector.SelectTagSN()
        if estado == lector.OK:
            uid_str = ":".join("{:02X}".format(b) for b in uid_bytes)
            if uid_str != ultimo_uid_enviado:
                print("Tarjeta detectada. UID:", uid_str)
                mensaje = {
                    "TipoMensaje": "Peticion",
                    "Accion": "RFID_DETECTADO",
                    "Datos": {"UID": uid_str}
                }
                enviar_mensaje(sock, mensaje)
                ultimo_uid_enviado = uid_str
                buzzer.value(1)
                time.sleep(0.3)
                buzzer.value(0)
    else:
        ultimo_uid_enviado = None


# BOTÓN — ahora sí forma parte del protocolo real:
# dispara BOTON_PRESIONADO, el servidor decide de quién es el
# turno (ColaCircular.Actual()) y tira los dados él mismo.

boton = Pin(9, Pin.IN, Pin.PULL_DOWN)
boton_presionado_antes = False #guarda estado anterior para detectar

def revisar_boton(sock):
    global boton_presionado_antes
    presionado_ahora = boton.value() == 1
    #Solo manda el mensaje en el instante que pasa de no presionado a presionado.
    if presionado_ahora and not boton_presionado_antes:
        mensaje = {
            "TipoMensaje": "Peticion",
            "Accion": "BOTON_PRESIONADO",
            "Datos": {}
        }
        enviar_mensaje(sock, mensaje)

    boton_presionado_antes = presionado_ahora


# MANEJO DE MENSAJES ENTRANTES (buffer NDJSON)

buffer_entrada = b"" # acumula bytes hasta encontrar un /n completo

def revisar_mensajes_servidor(sock):
    global buffer_entrada
    sock.settimeout(0.05)  # no bloquear el loop principal
    try:
        datos = sock.recv(1024)
        if not datos: # recv vacío = el servidor cerró la conexión
            raise ConexionPerdida()
        if datos:
            buffer_entrada += datos
            # Puede llegar más de un mensaje pegado en un solo recv(),
            # por eso el while: procesa todos los \n completos que haya
            while b"\n" in buffer_entrada:
                linea, buffer_entrada = buffer_entrada.split(b"\n", 1)
                if linea.strip():
                    procesar_mensaje(linea)
    except OSError:
        pass  # no llegó nada nuevo (timeout), normal

def procesar_mensaje(linea_bytes):
    try:
        texto = linea_bytes.decode("utf-8")
        #Quita el BOM (/uefeff) que c# agrega al inicio de los mensajes
        if texto and texto[0] == "\ufeff":
            texto = texto[1:]
        mensaje = ujson.loads(texto)
    except ValueError:
        print("Mensaje mal formado, se ignora:", linea_bytes)
        return

    global ultimo_dado, esperando_tarjeta
    accion = mensaje.get("Accion")
    tipo = mensaje.get("TipoMensaje")

    if accion == "MOSTRAR_DADO": # Raspy solo muestra, el numero lo genera el server 
        datos = mensaje.get("Datos") or {}
        valor1 = datos.get("Valor1")
        valor2 = datos.get("Valor2")
        print("Servidor pidió mostrar dado:", valor1, valor2)
        ultimo_dado = (valor1, valor2)
        esperando_tarjeta = False # tirada nueva: se limpia cualquier espera vieja (ej. compra cancelada)
        mostrar_par(valor1, valor2)

    elif accion == "ESPERAR_RFID":
        # Llega en dos casos: un jugador quiere VINCULAR su tarjeta, o
        # un jugador DEBE PAGAR (compra, alquiler, impuesto, carta).
        # La Pico no necesita saber cuál: solo avisa con "--" y sigue
        # leyendo tarjetas; el servidor decide qué hacer con el UID.
        print("Servidor espera una tarjeta (vincular o pagar), jugador:", mensaje.get("JugadorId"))
        esperando_tarjeta = True

    elif tipo == "Respuesta" and accion == "RFID_DETECTADO":
        # El servidor contesta si aceptó la tarjeta (Exito) y por qué (Mensaje)
        exito = mensaje.get("Exito", False)
        print("Tarjeta", "aceptada:" if exito else "rechazada:", mensaje.get("Mensaje"))
        if esperando_tarjeta:
            mostrar_resultado_tarjeta(exito)

    elif tipo == "Respuesta" and accion == "CONECTAR":
        print("Servidor:", mensaje.get("Mensaje"))

    else:
        print("Mensaje recibido, acción no manejada:", accion)


# PROGRAMA PRINCIPAL

def main():
    global esperando_tarjeta, buffer_entrada
    if not conectar_wifi():
        return 

    apagar_display(segmentos_1)
    apagar_display(segmentos_2)

    while True: # si se cae la conexión, vuelve a intentar cada 3 s
        try:
            sock = conectar_servidor()
        except (OSError, ConexionPerdida):
            print("Servidor no disponible, reintentando en 3 s...")
            time.sleep(3)
            continue

        buffer_entrada = b""
        esperando_tarjeta = False
        print("Listo. Esperando mensajes del servidor...")

        try:
            while True:
                revisar_mensajes_servidor(sock)
                revisar_boton(sock)
                revisar_rfid(sock)
                actualizar_espera_tarjeta()

                time.sleep_ms(100)
        except ConexionPerdida:
            print("Conexión con el servidor perdida. Reconectando...")
            try:
                sock.close()
            except OSError:
                pass
            time.sleep(3)

main()