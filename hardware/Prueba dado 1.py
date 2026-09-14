from machine import Pin
import random
import time

# --- Display de 7 segmentos (mismo cableado de antes) ---
segmentos = {
    'a': Pin(2, Pin.OUT),
    'b': Pin(3, Pin.OUT),
    'c': Pin(4, Pin.OUT),
    'd': Pin(5, Pin.OUT),
    'e': Pin(6, Pin.OUT),
    'f': Pin(7, Pin.OUT),
    'g': Pin(8, Pin.OUT),
}

DIGITOS = {  
    1: ['b', 'c'],
    2: ['a', 'b', 'g', 'e', 'd'],
    3: ['a', 'b', 'g', 'c', 'd'],
    4: ['f', 'g', 'b', 'c'],
    5: ['a', 'f', 'g', 'c', 'd'],
    6: ['a', 'f', 'g', 'e', 'c', 'd'],
}

def mostrar_numero(n):
    for pin in segmentos.values():
        pin.value(0)
    for letra in DIGITOS.get(n, []):
        segmentos[letra].value(1)

def apagar_display():
    for pin in segmentos.values():
        pin.value(0)

#Botón 
boton = Pin(9, Pin.IN, Pin.PULL_DOWN)

print("Presiona el boton para tirar el dado...")

apagar_display()

while True:
    if boton.value() == 1:
        valor = random.randint(1, 6)
        mostrar_numero(valor)
        print("Dado:", valor)

        # Espera a que suelten el boton, para no tirar 50 veces
        # mientras lo tienen presionado (rebote / hold)
        while boton.value() == 1:
            time.sleep_ms(50)

        time.sleep(2)          # deja el número visible un rato
        apagar_display()

    time.sleep_ms(50)