using System;
using System.Text.Json;
using System.Windows.Forms;
using MonopolyCliente.Comunicacion;
using MonopolyCore.Comunicacion;

namespace MonopolyCliente
{
    /// Pantalla de conexión al servidor.
    ///
    /// Capa de presentación pura: no valida reglas de juego ni calcula nada.
    /// Solo recoge lo que el usuario escribió, se lo pasa a ClienteMonopoly,
    /// y muestra en pantalla lo que el servidor conteste.
    public partial class FormLogin : Form
    {
        // Una sola instancia para toda la sesión: acá vive la conexión TCP.
        // Más adelante, cuando exista FormJuego, se le pasa esta misma
        // instancia para que siga usando la conexión ya abierta.
        private readonly ClienteMonopoly cliente = new ClienteMonopoly();

        public FormLogin()
        {
            InitializeComponent();

            // Nos suscribe a los eventos del cliente ANTES de conectar,
            // para no perdernos la respuesta de CONECTAR.
            cliente.RespuestaRecibida += AlRecibirRespuesta;
            cliente.NotificacionRecibida += AlRecibirNotificacion;
            cliente.ErrorDeConexion += AlFallarConexion;
            cliente.Desconectado += AlDesconectarse;
        }

        /// Click en "Conectar": arma la petición y la manda. Nada más.
        /// Si el nombre está vacío o el puerto mal escrito, eso es un
        /// chequeo de formulario (no de juego), así que sí va acá.
        private async void btnConectar_Click(object sender, EventArgs e)
        {
            string nombre = txtNombre.Text.Trim();
            string ip = txtIp.Text.Trim();

            if (string.IsNullOrWhiteSpace(nombre))
            {
                MostrarEstado("Escribí un nombre antes de conectar.");
                return;
            }

            if (!int.TryParse(txtPuerto.Text.Trim(), out int puerto))
            {
                MostrarEstado("El puerto debe ser un número.");
                return;
            }

            btnConectar.Enabled = false;
            MostrarEstado("Conectando...");

            try
            {
                await cliente.ConectarAsync(ip, puerto, nombre);
                // No muestra "conectado" todavía: eso se confirma cuando
                // llegue la Respuesta de CONECTAR (ver AlRecibirRespuesta).
            }
            catch (Exception ex)
            {
                MostrarEstado($"No se pudo conectar: {ex.Message}");
                btnConectar.Enabled = true;
            }
        }

        /// Llega una Respuesta del servidor (contestación a algo que pedimos).
        private void AlRecibirRespuesta(RespuestaMensaje respuesta)
        {
            // Los eventos del socket llegan en otro hilo; WinForms solo
            // permite tocar controles desde el hilo de la interfaz.
            // Invoke() reenvía la ejecución al hilo correcto.
            if (InvokeRequired)
            {
                Invoke(() => AlRecibirRespuesta(respuesta));
                return;
            }

            if (!respuesta.Exito)
            {
                // El servidor rechazó la petición: solo lo mostramos,
                // no discutimos su decisión.
                MostrarEstado($"Error: {respuesta.Mensaje}");
                btnConectar.Enabled = true;
                return;
            }

            if (respuesta.Accion == Acciones.Conectar)
            {
                // ClienteMonopoly ya guardó el JugadorId internamente.
                MostrarEstado($"Conectado como jugador {cliente.JugadorId}. {respuesta.Mensaje}");

                // Acá, más adelante, se abrirá FormJuego pasándole
                // la instancia de "cliente" y se cerrará este formulario.
            }
        }

        /// Llega una Notificacion (el servidor avisa algo sin que lo pidiéramos).
        private void AlRecibirNotificacion(MensajeBase notificacion)
        {
            if (InvokeRequired)
            {
                Invoke(() => AlRecibirNotificacion(notificacion));
                return;
            }

            // Por ahora solo las mostramos para ver que llegan.
            // Cuando exista FormJuego, cada acción se manejará allá.
            MostrarEstado($"[{notificacion.Accion}] {notificacion.Datos?.GetRawText()}");
        }

        private void AlFallarConexion(Exception ex)
        {
            if (InvokeRequired)
            {
                Invoke(() => AlFallarConexion(ex));
                return;
            }

            MostrarEstado($"Conexión perdida: {ex.Message}");
            btnConectar.Enabled = true;
        }

        private void AlDesconectarse()
        {
            if (InvokeRequired)
            {
                Invoke(AlDesconectarse);
                return;
            }

            MostrarEstado("Desconectado del servidor.");
            btnConectar.Enabled = true;
        }

        private void MostrarEstado(string texto)
        {
            lblEstado.Text = texto;
        }

        /// Al cerrar la ventana cerramos el socket, para no dejar
        /// conexiones colgando del lado del servidor.
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            cliente.Desconectar();
            base.OnFormClosing(e);
        }

    }
}
