from mfrc522 import MFRC522
import time

lector = MFRC522(sck=18, mosi=19, miso=16, rst=20, cs=17)

print("Circuito RFID listo. Acerca la tarjeta o el llavero...")
print("(Ctrl+C para detener)\n")

ultimo_uid = None

while True:
    (estado, tag_type) = lector.request(lector.REQIDL)

    if estado == lector.OK:
        (estado, uid_bytes) = lector.SelectTagSN()

        if estado == lector.OK:
            uid_str = ":".join("{:02X}".format(b) for b in uid_bytes)

            if uid_str != ultimo_uid:
                print("Tarjeta detectada. UID:", uid_str)
                ultimo_uid = uid_str
        else:
            print("Error leyendo la tarjeta, acerca de nuevo")
    else:
        ultimo_uid = None

    time.sleep_ms(200)