using System;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MonopolyCliente.Comunicacion;
using MonopolyCliente.Interfaz;
using MonopolyCore.Comunicacion;

namespace MonopolyCliente
{
    /// Pantalla principal de la partida (una ventana por jugador).
    ///
    /// Capa de presentación pura: los botones solo MANDAN peticiones al servidor y la pantalla
    /// se redibuja con lo que el servidor contesta. Toda la información viene de:
    ///   - ESTADO_ACTUALIZADO / CONSULTAR_ESTADO → EstadoJuegoDto completo (tablero, jugadores, turno)
    ///   - Notificaciones de eventos → registro de la partida y animaciones
    /// Habilitar o no un botón solo refleja lo que el servidor ya dijo (turno, dados lanzados, compra
    /// disponible...). Si igual llega una petición inválida, el servidor la rechaza.
    public partial class FormJuego : Form
    {
        private readonly ClienteMonopoly cliente;
        private readonly string nombreJugador;
        private readonly string servidor;

        private EstadoJuegoDto? estado;     // último estado recibido del servidor
        private FormHistorial? historial;   // ventana de historial abierta (si hay)
        private bool finMostrado;           // para mostrar el ganador una sola vez
        private bool conectado = true;
        private Font? fuenteNegrita;        // fuente en negrita del registro

        public FormJuego(ClienteMonopoly cliente, string nombreJugador, string servidor)
        {
            InitializeComponent();
            this.cliente = cliente;
            this.nombreJugador = nombreJugador;
            this.servidor = servidor;

            cliente.RespuestaRecibida += AlRecibirRespuesta;
            cliente.NotificacionRecibida += AlRecibirNotificacion;
            cliente.ErrorDeConexion += AlFallarConexion;
            cliente.Desconectado += AlDesconectarse;

            Text = $"Monopoly TEC - {nombreJugador} (Jugador {MiId})";
            lblConexion.Text = $"Conectado a {servidor} como {nombreJugador} (Jugador {MiId})";
            ActualizarBotones();
        }

        private int MiId => cliente.JugadorId ?? 0;

        // Al abrir, se pide el estado completo por si se perdió alguna notificación
        protected override async void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            AjustarAPantalla();
            AgregarLog($"Conectado como {nombreJugador} (Jugador {MiId}).", Colores.AzulTec);
            await Enviar(Acciones.ConsultarEstado);
        }

        // Si la pantalla es más chica que la ventana (laptops con escala 125-150%), se reduce todo
        private void AjustarAPantalla()
        {
            Rectangle area = Screen.FromControl(this).WorkingArea;
            float factor = Math.Min(1f, Math.Min((area.Width - 20f) / Width, (area.Height - 20f) / Height));
            if (factor >= 0.99f) return;

            Scale(new SizeF(factor, factor)); // posiciones y tamaños
            foreach (Control c in Controls)   // letras
                c.Font = new Font(c.Font.FontFamily, c.Font.Size * factor, c.Font.Style);
            CenterToScreen();
        }

        // ---------- Botones: solo envían la petición ----------

        private async void btnIniciar_Click(object sender, EventArgs e)
        {
            btnIniciar.Enabled = false;
            await Enviar(Acciones.IniciarJuego);
        }

        private async void btnTirar_Click(object sender, EventArgs e)
        {
            btnTirar.Enabled = false; // evita doble clic mientras llega la respuesta
            await Enviar(Acciones.TirarDados);
        }

        private async void btnComprar_Click(object sender, EventArgs e)
        {
            btnComprar.Enabled = false;
            await Enviar(Acciones.ComprarPropiedad);
        }

        private async void btnNoComprar_Click(object sender, EventArgs e)
        {
            btnNoComprar.Enabled = false;
            await Enviar(Acciones.NoComprar);
        }

        private async void btnTerminar_Click(object sender, EventArgs e)
        {
            btnTerminar.Enabled = false;
            await Enviar(Acciones.TerminarTurno);
        }

        private async void btnVincular_Click(object sender, EventArgs e)
        {
            await Enviar(Acciones.VincularRfid);
        }

        private async void btnExportar_Click(object sender, EventArgs e)
        {
            await Enviar(Acciones.ExportarTransacciones);
        }

        private void btnHistorial_Click(object sender, EventArgs e)
        {
            if (historial == null || historial.IsDisposed)
            {
                historial = new FormHistorial(cliente, estado?.Jugadores ?? new JugadorDto[0]);
                historial.Show(this);
            }
            else
            {
                historial.Activate();
            }
        }

        private async Task Enviar(string accion, object? datos = null)
        {
            try
            {
                await cliente.EnviarPeticionAsync(accion, datos);
            }
            catch (Exception ex)
            {
                MostrarMensaje($"No se pudo enviar la acción: {ex.Message}", Colores.Error);
                ActualizarBotones();
            }
        }

        // ---------- Mensajes del servidor ----------

        // Respuesta a una petición de ESTE jugador
        private void AlRecibirRespuesta(RespuestaMensaje respuesta)
        {
            if (!EnHiloDeInterfaz(() => AlRecibirRespuesta(respuesta))) return;

            if (respuesta.Accion == Acciones.ConsultarTransacciones) return; // la maneja FormHistorial

            if (!respuesta.Exito)
            {
                // El servidor rechazó la acción: solo se muestra el motivo
                MostrarMensaje(respuesta.Mensaje, Colores.Error);
                AgregarLog($"No permitido: {respuesta.Mensaje}", Colores.Error);
                ActualizarBotones();
                return;
            }

            switch (respuesta.Accion)
            {
                case Acciones.ConsultarEstado:
                    EstadoJuegoDto? nuevo = DatosMensaje.Leer<EstadoJuegoDto>(respuesta.Datos);
                    if (nuevo != null) AplicarEstado(nuevo);
                    break;

                case Acciones.VincularRfid:
                    MostrarMensaje("Acerque su tarjeta al lector RFID...", Colores.AzulClaro);
                    break;

                case Acciones.ExportarTransacciones:
                    MostrarMensaje("Historial exportado.", Colores.Exito);
                    MessageBox.Show(this, respuesta.Mensaje + "\n\n(El archivo queda en la computadora del servidor.)",
                        "Exportar historial", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;

                default:
                    if (!string.IsNullOrWhiteSpace(respuesta.Mensaje)) MostrarMensaje(respuesta.Mensaje, Colores.Exito);
                    break;
            }
        }

        // Avisos que el servidor manda a TODOS después de cada acción importante
        private void AlRecibirNotificacion(MensajeBase notificacion)
        {
            if (!EnHiloDeInterfaz(() => AlRecibirNotificacion(notificacion))) return;

            string texto = DatosMensaje.Texto(notificacion);

            switch (notificacion.Accion)
            {
                case Acciones.EstadoActualizado:
                    // Aquí Datos es directamente el EstadoJuegoDto
                    EstadoJuegoDto? nuevo = DatosMensaje.Leer<EstadoJuegoDto>(notificacion.Datos);
                    if (nuevo != null) AplicarEstado(nuevo);
                    return;

                case Acciones.TirarDados:
                    DatosDados? dados = DatosMensaje.Leer<DatosDados>(DatosMensaje.Detalle(notificacion));
                    if (dados != null) tablero.MostrarDados(dados.Valor1, dados.Valor2);
                    AgregarLog(texto, Colores.AzulTec);
                    return;

                case Acciones.JugadorMovido:
                    MovimientoDetalle? mov = DatosMensaje.Leer<MovimientoDetalle>(DatosMensaje.Detalle(notificacion));
                    if (mov != null) tablero.AnimarMovimiento(mov.JugadorId, mov.Recorrido);
                    AgregarLog(texto, Color.DimGray);
                    return;

                case Acciones.FinJuego:
                    AgregarLog(texto, Colores.AzulTec, negrita: true);
                    FinJuegoDetalle? fin = DatosMensaje.Leer<FinJuegoDetalle>(DatosMensaje.Detalle(notificacion));
                    if (fin != null && !finMostrado)
                    {
                        finMostrado = true;
                        BeginInvoke(new Action(() => MostrarFin(fin))); // después de pintar el estado final
                    }
                    return;

                case Acciones.RfidVinculado:
                    AgregarLog(texto, Colores.Exito);
                    if (notificacion.JugadorId == MiId) MostrarMensaje("Tu tarjeta quedó vinculada.", Colores.Exito);
                    return;

                case Acciones.TurnoCambiado:
                    AgregarLog(texto, Colores.AzulTec, negrita: true);
                    return;

                case Acciones.JugadorEliminado:
                    AgregarLog(texto, Colores.Error, negrita: true);
                    return;

                case Acciones.PagoPendiente:
                    AgregarLog(texto, Color.DarkOrange);
                    return;

                case Acciones.PagoRealizado:
                case Acciones.PropiedadComprada:
                    AgregarLog(texto, Colores.Exito);
                    return;

                case Acciones.CartaTomada:
                    AgregarLog(texto, Color.Purple, negrita: true);
                    return;

                default:
                    if (texto.Length > 0) AgregarLog(texto, Color.Black);
                    return;
            }
        }

        // ---------- Pintar el estado ----------

        private void AplicarEstado(EstadoJuegoDto nuevo)
        {
            estado = nuevo;
            tablero.MostrarEstado(nuevo);
            ActualizarTurno();
            ActualizarJugadores();
            ActualizarAviso();
            ActualizarBotones();
        }

        private void ActualizarTurno()
        {
            if (estado == null) return;
            if (estado.Estado == "Esperando")
                lblTurno.Text = $"Esperando jugadores ({estado.Jugadores.Length}/4)";
            else if (estado.Estado == "Finalizado")
                lblTurno.Text = $"Partida finalizada - ganó {NombreDe(estado.GanadorId)}";
            else
                lblTurno.Text = EsMiTurno()
                    ? $"Turno {estado.NumeroTurno}/{estado.MaxTurnos}: ¡es tu turno!"
                    : $"Turno {estado.NumeroTurno}/{estado.MaxTurnos}: juega {NombreDe(estado.JugadorEnTurnoId)}";
        }

        private void ActualizarJugadores()
        {
            if (estado == null) return;
            lvJugadores.BeginUpdate();
            lvJugadores.Items.Clear();
            foreach (JugadorDto j in estado.Jugadores)
            {
                ListViewItem fila = new ListViewItem("■") { UseItemStyleForSubItems = false };
                fila.ForeColor = Colores.Jugador(j.Id);
                fila.SubItems.Add(j.Nombre + (j.Id == MiId ? " (vos)" : ""));
                fila.SubItems.Add($"${j.Saldo}");
                fila.SubItems.Add(j.Propiedades.Length.ToString());
                fila.SubItems.Add($"${j.Patrimonio}");
                fila.SubItems.Add(EstadoDe(j));

                bool enTurno = estado.Estado == "EnCurso" && estado.JugadorEnTurnoId == j.Id;
                Color fondo = enTurno ? Color.FromArgb(255, 243, 205) : Color.White;
                for (int i = 0; i < fila.SubItems.Count; i++)
                {
                    fila.SubItems[i].BackColor = fondo;
                    if (i > 0) fila.SubItems[i].ForeColor = j.Activo || estado.Estado == "Esperando" ? Color.Black : Color.Gray;
                }
                lvJugadores.Items.Add(fila);
            }
            lvJugadores.EndUpdate();
        }

        private string EstadoDe(JugadorDto j)
        {
            if (estado == null) return "";
            string tarjeta = j.TieneTarjeta ? " · tarjeta ✓" : "";
            if (estado.Estado == "Esperando") return "Listo" + tarjeta;
            if (!j.Activo) return "Eliminado";
            if (estado.GanadorId == j.Id) return "Ganador";
            if (estado.JugadorEnTurnoId == j.Id && estado.Estado == "EnCurso") return "En turno" + tarjeta;
            if (j.TurnosPorPerder > 0) return $"Pierde {j.TurnosPorPerder} turno(s)";
            return "Activo" + tarjeta;
        }

        // Cartel superior: le dice al jugador qué tiene que hacer ahora (según lo que mandó el servidor)
        private void ActualizarAviso()
        {
            if (estado == null) { lblAviso.Visible = false; return; }

            string aviso = "";
            Color fondo = Colores.Aviso;

            if (estado.Estado == "Esperando")
            {
                aviso = MiId == 1 ? "Sos el organizador: iniciá la partida cuando estén todos conectados."
                                  : "Esperando a que el organizador inicie la partida...";
            }
            else if (estado.Estado == "Finalizado")
            {
                aviso = $"Fin de la partida. Ganador: {NombreDe(estado.GanadorId)}";
            }
            else if (estado.PagoPendiente != null)
            {
                PagoPendienteDto pago = estado.PagoPendiente;
                fondo = Colores.AvisoPago;
                aviso = pago.JugadorId == MiId
                    ? $"Acerque su tarjeta RFID para pagar ${pago.Monto} ({pago.Descripcion})"
                    : $"Esperando que {NombreDe(pago.JugadorId)} pase su tarjeta (${pago.Monto})";
            }
            else if (EsMiTurno() && estado.CompraPendientePosicion.HasValue)
            {
                CasillaDto? casilla = CasillaEn(estado.CompraPendientePosicion.Value);
                aviso = casilla != null
                    ? $"Podés comprar {casilla.Nombre} por ${casilla.Precio} (alquiler ${casilla.Alquiler})"
                    : "Podés comprar esta propiedad";
            }
            else if (EsMiTurno())
            {
                aviso = estado.DadosLanzados ? "Cuando estés listo, terminá tu turno."
                                             : "¡Es tu turno! Tirá los dados (botón o botón físico).";
            }

            lblAviso.Text = aviso;
            lblAviso.BackColor = fondo;
            lblAviso.Visible = aviso.Length > 0;
        }

        // Activa solo los botones que tienen sentido según el estado que mandó el servidor
        private void ActualizarBotones()
        {
            bool enCurso = conectado && estado?.Estado == "EnCurso";
            bool miTurno = enCurso && EsMiTurno();
            bool hayPago = estado?.PagoPendiente != null;
            bool dados = estado?.DadosLanzados ?? false;
            bool compra = estado?.CompraPendientePosicion.HasValue ?? false;
            bool compraConTarjeta = hayPago && estado!.PagoPendiente!.EsCompra && estado.PagoPendiente.JugadorId == MiId;
            JugadorDto? yo = Yo();

            Habilitar(btnIniciar, conectado && estado?.Estado == "Esperando" && MiId == 1, Colores.AzulTec);
            Habilitar(btnTirar, miTurno && !dados && !hayPago, Colores.AzulTec);
            Habilitar(btnComprar, miTurno && compra && !hayPago, Colores.AzulTec);
            Habilitar(btnNoComprar, miTurno && (compra || compraConTarjeta), Colores.AzulTec);
            Habilitar(btnTerminar, miTurno && dados && !hayPago, Colores.AzulTec);
            Habilitar(btnVincular, conectado && estado?.Estado != "Finalizado" && !(yo?.TieneTarjeta ?? false), Colores.AzulClaro);
            Habilitar(btnHistorial, conectado, Colores.AzulClaro);
            Habilitar(btnExportar, conectado && estado != null && estado.Estado != "Esperando", Colores.AzulClaro);
            btnVincular.Text = yo?.TieneTarjeta == true ? "Tarjeta vinculada ✓" : "Vincular tarjeta RFID";
        }

        private static void Habilitar(Button boton, bool activo, Color color)
        {
            boton.Enabled = activo;
            boton.BackColor = activo ? color : Colores.Deshabilitado;
        }

        private void MostrarFin(FinJuegoDetalle fin)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Ganador: {fin.Ganador ?? "nadie"}");
            sb.AppendLine($"Motivo: {fin.Motivo}");
            sb.AppendLine();
            sb.AppendLine("Ranking por patrimonio:");
            for (int i = 0; i < fin.Ranking.Length; i++)
            {
                JugadorDto j = fin.Ranking[i];
                sb.AppendLine($"  {i + 1}. {j.Nombre}: ${j.Patrimonio} (saldo ${j.Saldo}, {j.Propiedades.Length} propiedades){(j.Activo ? "" : " - eliminado")}");
            }
            if (!string.IsNullOrEmpty(fin.ArchivoTransacciones))
            {
                sb.AppendLine();
                sb.AppendLine("Historial exportado en el servidor:");
                sb.AppendLine(fin.ArchivoTransacciones);
            }
            MessageBox.Show(this, sb.ToString(), "¡Fin de la partida!", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // ---------- Utilidades de presentación ----------

        private bool EsMiTurno() => estado != null && estado.Estado == "EnCurso" && estado.JugadorEnTurnoId == MiId;

        private JugadorDto? Yo()
        {
            if (estado == null) return null;
            foreach (JugadorDto j in estado.Jugadores) if (j.Id == MiId) return j;
            return null;
        }

        private string NombreDe(int? jugadorId)
        {
            if (estado == null || !jugadorId.HasValue) return "-";
            foreach (JugadorDto j in estado.Jugadores) if (j.Id == jugadorId.Value) return j.Nombre;
            return "-";
        }

        private CasillaDto? CasillaEn(int posicion)
        {
            if (estado == null) return null;
            foreach (CasillaDto c in estado.Casillas) if (c.Posicion == posicion) return c;
            return null;
        }

        private void MostrarMensaje(string texto, Color color)
        {
            lblMensaje.Text = texto;
            lblMensaje.ForeColor = color;
        }

        private void AgregarLog(string texto, Color color, bool negrita = false)
        {
            if (string.IsNullOrWhiteSpace(texto)) return;
            rtbLog.SelectionStart = rtbLog.TextLength;
            rtbLog.SelectionLength = 0;
            rtbLog.SelectionColor = color;
            fuenteNegrita ??= new Font(rtbLog.Font, FontStyle.Bold); // se crea una sola vez
            rtbLog.SelectionFont = negrita ? fuenteNegrita : rtbLog.Font;
            rtbLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {texto}{Environment.NewLine}");
            rtbLog.ScrollToCaret();
        }

        // Los eventos del socket llegan en otro hilo: se pasan al hilo de la interfaz.
        // BeginInvoke mantiene el orden de llegada y no bloquea la lectura del socket.
        private bool EnHiloDeInterfaz(Action accion)
        {
            if (IsDisposed || Disposing) return false;
            if (InvokeRequired)
            {
                if (IsHandleCreated) BeginInvoke(accion);
                return false;
            }
            return true;
        }

        private void AlFallarConexion(Exception ex)
        {
            if (!EnHiloDeInterfaz(() => AlFallarConexion(ex))) return;
            AgregarLog($"Error de conexión: {ex.Message}", Colores.Error, negrita: true);
        }

        private void AlDesconectarse()
        {
            if (!EnHiloDeInterfaz(AlDesconectarse)) return;
            conectado = false;
            lblConexion.Text = "Desconectado del servidor.";
            lblConexion.ForeColor = Colores.Error;
            MostrarMensaje("Se perdió la conexión con el servidor.", Colores.Error);
            AgregarLog("Se perdió la conexión con el servidor.", Colores.Error, negrita: true);
            ActualizarBotones();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            cliente.RespuestaRecibida -= AlRecibirRespuesta;
            cliente.NotificacionRecibida -= AlRecibirNotificacion;
            cliente.ErrorDeConexion -= AlFallarConexion;
            cliente.Desconectado -= AlDesconectarse;
            cliente.Desconectar();
            base.OnFormClosing(e);
        }
    }
}
