import network
import socket
import time
import ujson
from machine import Pin


# CONFIGURACIÓN 

WIFI_SSID = "NOMBRE_DEL_HOTSPOT"
WIFI_PASSWORD = "CONTRASEÑA_HOTSPOT"

SERVIDOR_IP = "192.168.1.100"   #  cambia esto cada vez que reinicien el hotspot
SERVIDOR_PUERTO = 5000           # confirmar puerto real con Persona A


# CONEXIÓN WIFI

def conectar_wifi():
    wlan = network.WLAN(network.STA_IF)
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
    else:
        print("\nNo se pudo conectar al WiFi.")
        return False


# CONEXIÓN TCP AL SERVIDOR

def conectar_servidor():
    s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    s.connect((SERVIDOR_IP, SERVIDOR_PUERTO))
    print("Conectado al servidor", SERVIDOR_IP, ":", SERVIDOR_PUERTO)

    mensaje_conectar = {
        "TipoMensaje": "Peticion",
        "Accion": "CONECTAR",
        "Datos": {"TipoCliente": "Hardware"}
    }
    enviar_mensaje(s, mensaje_conectar)
    return s

def enviar_mensaje(sock, mensaje_dict):
    texto = ujson.dumps(mensaje_dict) + "\n"
    sock.send(texto.encode("utf-8"))
    print("Enviado:", texto.strip())


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
}

def mostrar_numero(segmentos, n):
    for pin in segmentos.values():
        pin.value(0)
    for letra in DIGITOS.get(n, []):
        segmentos[letra].value(1)

def apagar_display(segmentos):
    for pin in segmentos.values():
        pin.value(0)

def parpadear_espera_rfid():
    # Feedback visual simple mientras el servidor espera que acerquemos
    # una tarjeta para vincular (Accion: ESPERAR_RFID). No bloquea el
    # loop principal por mucho tiempo -- solo un parpadeo cortito.
    for _ in range(3):
        mostrar_numero(segmentos_1, 1)
        mostrar_numero(segmentos_2, 1)
        time.sleep_ms(150)
        apagar_display(segmentos_1)
        apagar_display(segmentos_2)
        time.sleep_ms(150)


# RFID — DESCOMENTAR CUANDO EL RC522 ESTÉ RESOLDADO Y FUNCIONANDO
# (recordar: probar primero con _rreg(0x37) que devuelva 0x91/0x92
# antes de confiar en esta parte)

# from mfrc522 import MFRC522
# lector = MFRC522(sck=18, mosi=19, miso=16, rst=20, cs=17)
#
# ultimo_uid_enviado = None
#
# def revisar_rfid(sock):
#     global ultimo_uid_enviado
#     (estado, tag_type) = lector.request(lector.REQIDL)
#     if estado == lector.OK:
#         (estado, uid_bytes) = lector.SelectTagSN()
#         if estado == lector.OK:
#             uid_str = ":".join("{:02X}".format(b) for b in uid_bytes)
#             if uid_str != ultimo_uid_enviado:
#                 print("Tarjeta detectada. UID:", uid_str)
#                 mensaje = {
#                     "TipoMensaje": "Peticion",
#                     "Accion": "RFID_DETECTADO",
#                     "Datos": {"UID": uid_str}
#                 }
#                 enviar_mensaje(sock, mensaje)
#                 ultimo_uid_enviado = uid_str
#     else:
#         ultimo_uid_enviado = None


# BOTÓN — ahora sí forma parte del protocolo real:
# dispara BOTON_PRESIONADO, el servidor decide de quién es el
# turno (ColaCircular.Actual()) y tira los dados él mismo.

boton = Pin(9, Pin.IN, Pin.PULL_DOWN)
boton_presionado_antes = False

def revisar_boton(sock):
    global boton_presionado_antes
    presionado_ahora = boton.value() == 1

    if presionado_ahora and not boton_presionado_antes:
        mensaje = {
            "TipoMensaje": "Peticion",
            "Accion": "BOTON_PRESIONADO",
            "Datos": {}
        }
        enviar_mensaje(sock, mensaje)

    boton_presionado_antes = presionado_ahora


# MANEJO DE MENSAJES ENTRANTES (buffer NDJSON)

buffer_entrada = b""

def revisar_mensajes_servidor(sock):
    global buffer_entrada
    sock.settimeout(0.05)  # no bloquear el loop principal
    try:
        datos = sock.recv(1024)
        if datos:
            buffer_entrada += datos
            while b"\n" in buffer_entrada:
                linea, buffer_entrada = buffer_entrada.split(b"\n", 1)
                if linea.strip():
                    procesar_mensaje(linea)
    except OSError:
        pass  # no llegó nada nuevo, normal

def procesar_mensaje(linea_bytes):
    try:
        mensaje = ujson.loads(linea_bytes.decode("utf-8"))
    except ValueError:
        print("Mensaje mal formado, se ignora:", linea_bytes)
        return

    accion = mensaje.get("Accion")

    if accion == "MOSTRAR_DADO":
        datos = mensaje.get("Datos", {})
        valor1 = datos.get("Valor1")
        valor2 = datos.get("Valor2")
        print("Servidor pidió mostrar dado:", valor1, valor2)
        mostrar_numero(segmentos_1, valor1)
        mostrar_numero(segmentos_2, valor2)

    elif accion == "ESPERAR_RFID":
        print("Servidor pidió esperar tarjeta para vincular...")
        parpadear_espera_rfid()
        # A partir de aquí, la próxima lectura de RFID_DETECTADO
        # que mandemos, el servidor la va a interpretar como la
        # vinculación pendiente -- la Pico no necesita saber nada
        # de esto, solo sigue leyendo tarjetas como siempre.

    else:
        print("Mensaje recibido, acción no manejada:", accion)


# PROGRAMA PRINCIPAL

def main():
    if not conectar_wifi():
        return

    sock = conectar_servidor()
    apagar_display(segmentos_1)
    apagar_display(segmentos_2)

    print("Listo. Esperando mensajes del servidor...")

    while True:
        revisar_mensajes_servidor(sock)
        revisar_boton(sock)
        # revisar_rfid(sock)   # descomentar cuando el RC522 esté listo

        time.sleep_ms(100)

main()