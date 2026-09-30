using MonopolyCore.Estructuras;
using MonopolyCore.Modelos;

namespace MonopolyCore
{
    // Datos del tablero y del mazo.
    // Regla: la posición de cada casilla debe coincidir con el orden en que se agrega (0, 1, 2...).
    public static class ConfiguracionPartida
    {
        public static Tablero CrearTableroPorDefecto()
        {
            Tablero t = new Tablero();
            int p = 0;

            t.AgregarCasilla(new CasillaEspecial(p + 1, "Inicio de Semestre", p++, TipoCasillaEspecial.Inicio));
            t.AgregarCasilla(new Propiedad(p + 1, "Soda Institucional", p++, 60, 12));
            t.AgregarCasilla(new CasillaEvento(p + 1, "Vida TEC", p++));
            t.AgregarCasilla(new Propiedad(p + 1, "El Pretil", p++, 60, 12));
            t.AgregarCasilla(new CasillaEspecial(p + 1, "Pago de Matrícula", p++, TipoCasillaEspecial.Impuesto, 100));
            t.AgregarCasilla(new Propiedad(p + 1, "Residencias", p++, 100, 20));
            t.AgregarCasilla(new CasillaEspecial(p + 1, "Curso Repetido", p++, TipoCasillaEspecial.Carcel));
            t.AgregarCasilla(new Propiedad(p + 1, "Gymtec", p++, 120, 24));
            t.AgregarCasilla(new CasillaEvento(p + 1, "Vida TEC", p++));
            t.AgregarCasilla(new Propiedad(p + 1, "Piscina", p++, 140, 28));
            t.AgregarCasilla(new Propiedad(p + 1, "El Lago", p++, 160, 32));
            t.AgregarCasilla(new Propiedad(p + 1, "Centro de las Artes", p++, 160, 32));
            t.AgregarCasilla(new CasillaEspecial(p + 1, "Parqueo del TEC", p++, TipoCasillaEspecial.ParqueoGratis));
            t.AgregarCasilla(new Propiedad(p + 1, "Edificio D3", p++, 180, 36));
            t.AgregarCasilla(new CasillaEvento(p + 1, "Vida TEC", p++));
            t.AgregarCasilla(new Propiedad(p + 1, "Edificio F2", p++, 200, 40));
            t.AgregarCasilla(new Propiedad(p + 1, "LAIMI corte laser", p++, 220, 44));
            t.AgregarCasilla(new Propiedad(p + 1, "Escuela de Matemática", p++, 240, 48));
            t.AgregarCasilla(new CasillaEspecial(p + 1, "¡Reprobaste!", p++, TipoCasillaEspecial.IrACarcel));
            t.AgregarCasilla(new Propiedad(p + 1, "Biblioteca José Figueres", p++, 260, 52));
            t.AgregarCasilla(new Propiedad(p + 1, "Learning Commons", p++, 280, 56));
            t.AgregarCasilla(new CasillaEvento(p + 1, "Vida TEC", p++));
            t.AgregarCasilla(new CasillaEspecial(p + 1, "Pago de Las copias", p++, TipoCasillaEspecial.Impuesto, 150));
            t.AgregarCasilla(new Propiedad(p + 1, "Sala de fuerza", p++, 350, 70));

            return t; //24 casillas
        }

        public static ListaCircularDoble<CartaEvento> CrearMazoPorDefecto()
        {
            ListaCircularDoble<CartaEvento> mazo = new ListaCircularDoble<CartaEvento>();
            CartaEvento[] cartas =
            {
                new CartaEvento(1, "Ganaste un hackathon: recibe 150", TipoEfectoEvento.GanarDinero, 150),
                new CartaEvento(2, "Perdiste el carné: paga 80 por la reposición", TipoEfectoEvento.PerderDinero, 80),
                new CartaEvento(3, "Te hicieron ride al campus: avanza 3 casillas", TipoEfectoEvento.Moverse, 3),
                new CartaEvento(4, "FULL presa!: pierdes un turno", TipoEfectoEvento.PerderTurno, 1),
                new CartaEvento(5, "Te cayó la beca: recibe 100", TipoEfectoEvento.GanarDinero, 100),
                new CartaEvento(6, "Se te olvidó la laptop: retrocede 2 casillas", TipoEfectoEvento.Retroceder, 2),
                new CartaEvento(7, "Comienzo del semestre: ve a Inicio", TipoEfectoEvento.IrACasilla, 0),
                new CartaEvento(8, "Se te quemó la Raspy:( : paga 120", TipoEfectoEvento.PerderDinero, 120),
                new CartaEvento(9, "Te atraparon copiando: ve a Curso Repetido", TipoEfectoEvento.IrACarcel, 0),
                new CartaEvento(10, "Te cancelaron una clase: ve al Parqueo del TEC", TipoEfectoEvento.IrACasilla, 12),
                new CartaEvento(11, "Es tu cumpleaños: cada jugador te paga 20", TipoEfectoEvento.CobrarATodos, 20),
                new CartaEvento(12, "Invitás a todos a la soda: pagás 10 a cada jugador", TipoEfectoEvento.PagarATodos, 10),
            };

            //Se barajan una vez al crear la partida (Fisher-Yates sobre el arreglo)
            for (int i = cartas.Length - 1; i > 0; i--)
            {
                int j = Random.Shared.Next(i + 1);
                (cartas[i], cartas[j]) = (cartas[j], cartas[i]);
            }
            for (int i = 0; i < cartas.Length; i++)
            {
                mazo.AgregarAlFinal(cartas[i]);
            }
            return mazo;
        }
    }
}