using System.Drawing;

namespace MonopolyCliente.Interfaz
{
    // Paleta de la interfaz (temática TEC). Solo es presentación: cambiar un color aquí lo cambia en todo el cliente.
    internal static class Colores
    {
        public static readonly Color AzulTec = Color.FromArgb(0, 48, 95);        // azul principal
        public static readonly Color AzulClaro = Color.FromArgb(0, 102, 170);    // acentos
        public static readonly Color Fondo = Color.FromArgb(237, 241, 246);      // fondo de ventanas
        public static readonly Color FondoTablero = Color.FromArgb(221, 234, 222); // centro del tablero
        public static readonly Color Deshabilitado = Color.FromArgb(170, 184, 200); // botones apagados
        public static readonly Color Error = Color.FromArgb(178, 34, 34);
        public static readonly Color Exito = Color.FromArgb(30, 120, 60);
        public static readonly Color Aviso = Color.FromArgb(255, 236, 179);      // fondo de avisos
        public static readonly Color AvisoPago = Color.FromArgb(255, 205, 150);  // fondo cuando hay que pasar tarjeta

        // Color de cada jugador (ficha, dueño de propiedades, lista)
        public static Color Jugador(int jugadorId)
        {
            switch (jugadorId)
            {
                case 1: return Color.FromArgb(214, 48, 49);   // rojo
                case 2: return Color.FromArgb(9, 132, 227);   // azul
                case 3: return Color.FromArgb(0, 163, 90);    // verde
                case 4: return Color.FromArgb(232, 130, 0);   // naranja
                default: return Color.Gray;
            }
        }

        // Franja de color de las propiedades según su precio (solo para agruparlas visualmente)
        public static Color GrupoPropiedad(int precio)
        {
            if (precio <= 60) return Color.FromArgb(139, 90, 43);     // café
            if (precio <= 120) return Color.FromArgb(135, 206, 235);  // celeste
            if (precio <= 160) return Color.FromArgb(214, 51, 132);   // rosado
            if (precio <= 200) return Color.FromArgb(247, 148, 29);   // naranja
            if (precio <= 240) return Color.FromArgb(220, 20, 60);    // rojo
            if (precio <= 280) return Color.FromArgb(255, 215, 0);    // amarillo
            return Color.FromArgb(0, 70, 140);                        // azul oscuro (la más cara)
        }

        // Fondo de las casillas especiales de las esquinas
        public static Color Especial(string subtipo)
        {
            switch (subtipo)
            {
                case "Inicio": return Color.FromArgb(200, 240, 200);
                case "Carcel": return Color.FromArgb(255, 224, 178);
                case "ParqueoGratis": return Color.FromArgb(197, 225, 245);
                case "IrACarcel": return Color.FromArgb(255, 205, 210);
                case "Impuesto": return Color.FromArgb(236, 236, 236);
                default: return Color.White;
            }
        }
    }
}
