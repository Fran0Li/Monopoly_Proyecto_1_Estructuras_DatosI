from machine import Pin
import random
import time

# --- Dado 1 ---
segmentos_1 = {
    'a': Pin(2, Pin.OUT), 'b': Pin(3, Pin.OUT), 'c': Pin(4, Pin.OUT),
    'd': Pin(5, Pin.OUT), 'e': Pin(6, Pin.OUT), 'f': Pin(7, Pin.OUT),
    'g': Pin(8, Pin.OUT),
}

# --- Dado 2 ---
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

boton = Pin(9, Pin.IN, Pin.PULL_DOWN)

print("Presiona el boton para tirar los dos dados...")
apagar_display(segmentos_1)
apagar_display(segmentos_2)

while True:
    if boton.value() == 1:
        valor1 = random.randint(1, 6)
        valor2 = random.randint(1, 6)

        mostrar_numero(segmentos_1, valor1)
        mostrar_numero(segmentos_2, valor2)

        print("Dado 1:", valor1, "| Dado 2:", valor2)

        while boton.value() == 1:
            time.sleep_ms(50)

        time.sleep(2)
        apagar_display(segmentos_1)
        apagar_display(segmentos_2)

    time.sleep_ms(50)