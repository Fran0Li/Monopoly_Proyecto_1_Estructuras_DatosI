using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;
using MonopolyCore.Comunicacion;

namespace MonopolyCliente.Interfaz
{
    // Control que DIBUJA el tablero a partir del EstadoJuegoDto que manda el servidor.
    // No calcula reglas: solo pinta casillas, dueños, fichas y dados, y anima el recorrido
    // que el servidor ya calculó (Recorrido de JUGADOR_MOVIDO).
    //
    // Distribución: las casillas van por el borde de un cuadro de (n/4 + 1) x (n/4 + 1) celdas.
    // Con 24 casillas es 7x7. Inicio (0) abajo a la derecha y se avanza en sentido horario
    // visual (abajo → izquierda → arriba → derecha), como el Monopoly clásico.
    internal class TableroControl : Control
    {
        private const int MaxJugadores = 4;

        private EstadoJuegoDto? estado;
        private int dado1, dado2;

        // Animación de fichas: por jugador (índice = id) el recorrido pendiente y la posición que se está mostrando
        private readonly int[]?[] recorridos = new int[MaxJugadores + 1][];
        private readonly int[] pasoActual = new int[MaxJugadores + 1];
        private readonly int[] posicionAnimada = new int[MaxJugadores + 1];
        private readonly bool[] animando = new bool[MaxJugadores + 1];
        private readonly System.Windows.Forms.Timer temporizador;

        private readonly ToolTip ayuda = new ToolTip();
        private int casillaBajoMouse = -1;

        public TableroControl()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
            BackColor = Colores.Fondo;
            temporizador = new System.Windows.Forms.Timer { Interval = 170 }; // velocidad de la ficha
            temporizador.Tick += AvanzarAnimacion;
        }

        // ---------- Lo que llama FormJuego ----------

        public void MostrarEstado(EstadoJuegoDto nuevoEstado)
        {
            estado = nuevoEstado;
            if (nuevoEstado.Dado1 > 0) { dado1 = nuevoEstado.Dado1; dado2 = nuevoEstado.Dado2; }
            Invalidate();
        }

        public void MostrarDados(int valor1, int valor2)
        {
            dado1 = valor1;
            dado2 = valor2;
            Invalidate();
        }

        // Mueve la ficha casilla por casilla usando el recorrido que calculó el servidor
        public void AnimarMovimiento(int jugadorId, int[] recorrido)
        {
            if (jugadorId < 1 || jugadorId > MaxJugadores || recorrido.Length == 0) return;

            int[]? pendiente = recorridos[jugadorId];
            if (pendiente != null) // ya se estaba moviendo (ej. carta "avanza 3"): se agrega al final
            {
                int restantes = pendiente.Length - pasoActual[jugadorId];
                int[] unido = new int[restantes + recorrido.Length];
                Array.Copy(pendiente, pasoActual[jugadorId], unido, 0, restantes);
                Array.Copy(recorrido, 0, unido, restantes, recorrido.Length);
                recorridos[jugadorId] = unido;
            }
            else
            {
                recorridos[jugadorId] = recorrido;
                if (!animando[jugadorId]) posicionAnimada[jugadorId] = PosicionSegunEstado(jugadorId);
            }
            pasoActual[jugadorId] = 0;
            animando[jugadorId] = true;
            temporizador.Start();
        }

        private void AvanzarAnimacion(object? sender, EventArgs e)
        {
            bool algunoSeMueve = false;
            for (int id = 1; id <= MaxJugadores; id++)
            {
                int[]? recorrido = recorridos[id];
                if (recorrido == null)
                {
                    animando[id] = false; // terminó: se vuelve a usar la posición del estado
                    continue;
                }
                posicionAnimada[id] = recorrido[pasoActual[id]];
                pasoActual[id]++;
                if (pasoActual[id] >= recorrido.Length) recorridos[id] = null;
                algunoSeMueve = true;
            }
            if (!algunoSeMueve) temporizador.Stop();
            Invalidate();
        }

        private int PosicionSegunEstado(int jugadorId)
        {
            if (estado == null) return 0;
            foreach (JugadorDto j in estado.Jugadores)
                if (j.Id == jugadorId) return j.Posicion;
            return 0;
        }

        // ---------- Geometría ----------

        private int CantidadCasillas => estado != null && estado.Casillas.Length >= 4 ? estado.Casillas.Length : 24;

        private float TamanoCelda => (Math.Min(Width, Height) - 1) / (float)(CantidadCasillas / 4 + 1);

        // Rectángulo de la casilla p en el borde del cuadro
        private RectangleF RectCasilla(int p)
        {
            int seg = CantidadCasillas / 4;
            float c = TamanoCelda;
            int col, fila;
            if (p <= seg) { col = seg - p; fila = seg; }                       // abajo, de derecha a izquierda
            else if (p <= 2 * seg) { col = 0; fila = seg - (p - seg); }        // izquierda, de abajo hacia arriba
            else if (p <= 3 * seg) { col = p - 2 * seg; fila = 0; }            // arriba, de izquierda a derecha
            else { col = seg; fila = p - 3 * seg; }                            // derecha, de arriba hacia abajo
            return new RectangleF(col * c, fila * c, c, c);
        }

        // Lado del tablero donde está la casilla (0 abajo, 1 izquierda, 2 arriba, 3 derecha, -1 esquina)
        private int LadoDe(int p)
        {
            int seg = CantidadCasillas / 4;
            if (p % seg == 0) return -1;
            return p / seg;
        }

        // ---------- Dibujo ----------

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            int n = CantidadCasillas;
            float c = TamanoCelda;
            float lado = c * (n / 4 + 1);

            using (SolidBrush fondo = new SolidBrush(Colores.FondoTablero))
                g.FillRectangle(fondo, 0, 0, lado, lado);

            for (int p = 0; p < n; p++)
            {
                CasillaDto? casilla = estado != null && p < estado.Casillas.Length ? estado.Casillas[p] : null;
                DibujarCasilla(g, p, casilla);
            }

            DibujarCentro(g, c, lado);
            DibujarFichas(g);
        }

        // Área útil de la casilla (sin la franja de color de las propiedades)
        private RectangleF AreaInterior(int p, CasillaDto? casilla)
        {
            RectangleF r = RectCasilla(p);
            if (casilla == null || casilla.Tipo != "Propiedad") return r;
            float grosor = r.Height * 0.2f;
            switch (LadoDe(p))
            {
                case 0: return new RectangleF(r.X, r.Y + grosor, r.Width, r.Height - grosor);
                case 1: return new RectangleF(r.X, r.Y, r.Width - grosor, r.Height);
                case 2: return new RectangleF(r.X, r.Y, r.Width, r.Height - grosor);
                default: return new RectangleF(r.X + grosor, r.Y, r.Width - grosor, r.Height);
            }
        }

        private void DibujarCasilla(Graphics g, int p, CasillaDto? casilla)
        {
            RectangleF r = RectCasilla(p);
            RectangleF texto = AreaInterior(p, casilla);

            Color fondo = Color.White;
            if (casilla != null && casilla.Tipo == "Especial") fondo = Colores.Especial(casilla.Subtipo);
            if (casilla != null && casilla.Tipo == "Evento") fondo = Color.FromArgb(255, 248, 225);
            using (SolidBrush b = new SolidBrush(fondo)) g.FillRectangle(b, r);

            // Franja de color hacia el centro del tablero (solo propiedades)
            if (casilla != null && casilla.Tipo == "Propiedad")
            {
                float grosor = r.Height * 0.2f;
                RectangleF franja;
                switch (LadoDe(p))
                {
                    case 0: franja = new RectangleF(r.X, r.Y, r.Width, grosor); break;
                    case 1: franja = new RectangleF(r.Right - grosor, r.Y, grosor, r.Height); break;
                    case 2: franja = new RectangleF(r.X, r.Bottom - grosor, r.Width, grosor); break;
                    default: franja = new RectangleF(r.X, r.Y, grosor, r.Height); break;
                }
                using (SolidBrush b = new SolidBrush(Colores.GrupoPropiedad(casilla.Precio))) g.FillRectangle(b, franja);
            }

            // Borde normal
            using (Pen borde = new Pen(Color.FromArgb(90, 90, 90), 1f)) g.DrawRectangle(borde, r.X, r.Y, r.Width, r.Height);

            if (casilla == null) return;

            // Nombre y valor
            float tamFuente = Math.Max(6f, TamanoCelda / 12.5f);
            using (Font nombre = new Font("Segoe UI", tamFuente, FontStyle.Bold))
            using (Font detalle = new Font("Segoe UI", tamFuente * 0.95f))
            using (StringFormat centrado = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Near, Trimming = StringTrimming.EllipsisWord })
            {
                RectangleF areaNombre = new RectangleF(texto.X + 2, texto.Y + 2, texto.Width - 4, texto.Height * 0.46f);
                g.DrawString(casilla.Nombre, nombre, Brushes.Black, areaNombre, centrado);

                string valor = "";
                if (casilla.Tipo == "Propiedad") valor = $"${casilla.Precio}";
                else if (casilla.Subtipo == "Impuesto") valor = $"Paga ${casilla.Monto}";
                else if (casilla.Tipo == "Evento") valor = "Carta";
                if (valor.Length > 0)
                {
                    RectangleF areaValor = new RectangleF(texto.X + 2, texto.Y + texto.Height * 0.47f, texto.Width - 4, texto.Height * 0.2f);
                    g.DrawString(valor, detalle, Brushes.DimGray, areaValor, centrado);
                }
            }

            // Dueño: borde grueso del color del jugador
            if (casilla.PropietarioId.HasValue)
            {
                using (Pen dueno = new Pen(Colores.Jugador(casilla.PropietarioId.Value), 4f))
                    g.DrawRectangle(dueno, r.X + 2, r.Y + 2, r.Width - 4, r.Height - 4);
            }

            // Propiedad que el jugador en turno puede comprar: borde punteado dorado
            if (estado != null && estado.CompraPendientePosicion == p)
            {
                using (Pen compra = new Pen(Color.Goldenrod, 3f) { DashStyle = DashStyle.Dash })
                    g.DrawRectangle(compra, r.X + 5, r.Y + 5, r.Width - 10, r.Height - 10);
            }
        }

        private void DibujarCentro(Graphics g, float c, float lado)
        {
            RectangleF centro = new RectangleF(c, c, lado - 2 * c, lado - 2 * c);

            using (Font titulo = new Font("Segoe UI", Math.Max(14f, c / 3.2f), FontStyle.Bold))
            using (Font subtitulo = new Font("Segoe UI", Math.Max(8f, c / 9f), FontStyle.Italic))
            using (SolidBrush azul = new SolidBrush(Colores.AzulTec))
            using (StringFormat centrado = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                RectangleF rTitulo = new RectangleF(centro.X, centro.Y + centro.Height * 0.12f, centro.Width, centro.Height * 0.16f);
                g.DrawString("MONOPOLY TEC", titulo, azul, rTitulo, centrado);
                RectangleF rSub = new RectangleF(centro.X, rTitulo.Bottom, centro.Width, centro.Height * 0.07f);
                g.DrawString("Edición Instituto Tecnológico de Costa Rica", subtitulo, Brushes.DimGray, rSub, centrado);

                // Dados
                float tamDado = c * 0.75f;
                float yDados = centro.Y + centro.Height * 0.42f;
                float xCentro = centro.X + centro.Width / 2;
                DibujarDado(g, new RectangleF(xCentro - tamDado - c * 0.12f, yDados, tamDado, tamDado), dado1);
                DibujarDado(g, new RectangleF(xCentro + c * 0.12f, yDados, tamDado, tamDado), dado2);

                // Estado de la partida
                using (Font info = new Font("Segoe UI", Math.Max(9f, c / 7.5f), FontStyle.Bold))
                {
                    RectangleF rInfo = new RectangleF(centro.X, yDados + tamDado + c * 0.2f, centro.Width, centro.Height * 0.12f);
                    g.DrawString(TextoCentral(), info, azul, rInfo, centrado);
                }
            }
        }

        private string TextoCentral()
        {
            if (estado == null) return "Conectando...";
            if (estado.Estado == "Esperando") return $"Esperando jugadores ({estado.Jugadores.Length}/4)";
            if (estado.Estado == "Finalizado")
            {
                string ganador = NombreDe(estado.GanadorId);
                return ganador.Length > 0 ? $"¡Ganó {ganador}!" : "Partida finalizada";
            }
            return $"Turno {estado.NumeroTurno}: juega {NombreDe(estado.JugadorEnTurnoId)}";
        }

        private string NombreDe(int? jugadorId)
        {
            if (estado == null || !jugadorId.HasValue) return "";
            foreach (JugadorDto j in estado.Jugadores)
                if (j.Id == jugadorId.Value) return j.Nombre;
            return "";
        }

        private static void DibujarDado(Graphics g, RectangleF r, int valor)
        {
            using (GraphicsPath forma = Redondeado(r, r.Width * 0.18f))
            {
                g.FillPath(Brushes.White, forma);
                using (Pen borde = new Pen(Colores.AzulTec, 2f)) g.DrawPath(borde, forma);
            }
            if (valor < 1 || valor > 6) return;

            float d = r.Width * 0.18f; // diámetro de cada punto
            float izq = r.X + r.Width * 0.25f, cen = r.X + r.Width * 0.5f, der = r.X + r.Width * 0.75f;
            float arr = r.Y + r.Height * 0.25f, med = r.Y + r.Height * 0.5f, aba = r.Y + r.Height * 0.75f;

            void Punto(float x, float y) => g.FillEllipse(Brushes.Black, x - d / 2, y - d / 2, d, d);

            if (valor == 1 || valor == 3 || valor == 5) Punto(cen, med);
            if (valor >= 2) { Punto(izq, arr); Punto(der, aba); }
            if (valor >= 4) { Punto(der, arr); Punto(izq, aba); }
            if (valor == 6) { Punto(izq, med); Punto(der, med); }
        }

        private static GraphicsPath Redondeado(RectangleF r, float radio)
        {
            GraphicsPath path = new GraphicsPath();
            float d = radio * 2;
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        private void DibujarFichas(Graphics g)
        {
            if (estado == null || estado.Estado == "Esperando") return;

            int[] ocupadas = new int[CantidadCasillas]; // cuántas fichas ya se dibujaron en cada casilla
            float diametro = TamanoCelda * 0.19f;

            using (Font numero = new Font("Segoe UI", Math.Max(7f, diametro / 2.4f), FontStyle.Bold))
            using (StringFormat centrado = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                foreach (JugadorDto j in estado.Jugadores)
                {
                    if (!j.Activo) continue;
                    int pos = j.Id >= 1 && j.Id <= MaxJugadores && animando[j.Id] ? posicionAnimada[j.Id] : j.Posicion;
                    if (pos < 0 || pos >= CantidadCasillas) continue;

                    CasillaDto? casilla = pos < estado.Casillas.Length ? estado.Casillas[pos] : null;
                    RectangleF area = AreaInterior(pos, casilla);
                    int k = ocupadas[pos]++ % 4;
                    // Hasta 4 fichas en una fila en la parte baja de la casilla (debajo del precio)
                    float separacion = (area.Width - 4 * diametro) / 5f;
                    float x = area.X + separacion + k * (diametro + separacion);
                    float y = area.Bottom - diametro - area.Height * 0.08f;
                    RectangleF ficha = new RectangleF(x, y, diametro, diametro);

                    using (SolidBrush relleno = new SolidBrush(Colores.Jugador(j.Id))) g.FillEllipse(relleno, ficha);
                    using (Pen borde = new Pen(Color.White, 2f)) g.DrawEllipse(borde, ficha);
                    g.DrawString(j.Id.ToString(), numero, Brushes.White, ficha, centrado);
                }
            }
        }

        // ---------- Información al pasar el mouse ----------

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            int encontrada = -1;
            for (int p = 0; p < CantidadCasillas; p++)
            {
                if (RectCasilla(p).Contains(e.X, e.Y)) { encontrada = p; break; }
            }
            if (encontrada == casillaBajoMouse) return;
            casillaBajoMouse = encontrada;
            ayuda.SetToolTip(this, encontrada >= 0 ? DescribirCasilla(encontrada) : "");
        }

        private string DescribirCasilla(int p)
        {
            if (estado == null || p >= estado.Casillas.Length) return "";
            CasillaDto c = estado.Casillas[p];
            string texto = $"{c.Nombre} (casilla {c.Posicion})";
            if (c.Tipo == "Propiedad")
            {
                texto += $"\nPrecio: ${c.Precio}\nAlquiler: ${c.Alquiler}";
                texto += c.PropietarioId.HasValue ? $"\nDueño: {NombreDe(c.PropietarioId)}" : "\nDisponible";
            }
            else if (c.Tipo == "Evento") texto += "\nToma una carta Vida TEC";
            else if (c.Subtipo == "Impuesto") texto += $"\nPaga ${c.Monto} al banco";

            foreach (JugadorDto j in estado.Jugadores)
                if (j.Activo && j.Posicion == p) texto += $"\nAquí está: {j.Nombre}";
            return texto;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                temporizador.Dispose();
                ayuda.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
