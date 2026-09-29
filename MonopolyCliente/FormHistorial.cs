using System;
using System.Windows.Forms;
using MonopolyCliente.Comunicacion;
using MonopolyCliente.Interfaz;
using MonopolyCore.Comunicacion;
using MonopolyCore.Modelos;

namespace MonopolyCliente
{
    /// Muestra el historial de transacciones que guarda el servidor (CONSULTAR_TRANSACCIONES).
    /// Los filtros (jugador, tipo, orden) se mandan al servidor; la ventana solo pinta la tabla.
    public partial class FormHistorial : Form
    {
        private readonly ClienteMonopoly cliente;

        // Opción de un ComboBox: texto visible + valor que se manda al servidor
        private class Opcion
        {
            public string Texto { get; }
            public object? Valor { get; }
            public Opcion(string texto, object? valor) { Texto = texto; Valor = valor; }
            public override string ToString() => Texto;
        }

        public FormHistorial(ClienteMonopoly cliente, JugadorDto[] jugadores)
        {
            InitializeComponent();
            this.cliente = cliente;

            cmbJugador.Items.Add(new Opcion("Todos", null));
            foreach (JugadorDto j in jugadores) cmbJugador.Items.Add(new Opcion(j.Nombre, j.Id));
            cmbJugador.SelectedIndex = 0;

            cmbTipo.Items.Add(new Opcion("Todos", null));
            foreach (string tipo in Enum.GetNames(typeof(TipoTransaccion))) cmbTipo.Items.Add(new Opcion(NombreTipo(tipo), tipo));
            cmbTipo.SelectedIndex = 0;

            cliente.RespuestaRecibida += AlRecibirRespuesta;
        }

        protected override async void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            await Consultar();
        }

        private async void btnConsultar_Click(object sender, EventArgs e)
        {
            await Consultar();
        }

        private async System.Threading.Tasks.Task Consultar()
        {
            object? filtroJugador = (cmbJugador.SelectedItem as Opcion)?.Valor;
            object? filtroTipo = (cmbTipo.SelectedItem as Opcion)?.Valor;
            lblTotal.Text = "Consultando...";
            try
            {
                await cliente.EnviarPeticionAsync(Acciones.ConsultarTransacciones, new
                {
                    FiltroJugadorId = filtroJugador,
                    Tipo = filtroTipo,
                    DesdeInicio = !chkRecientes.Checked
                });
            }
            catch (Exception ex)
            {
                lblTotal.Text = $"No se pudo consultar: {ex.Message}";
            }
        }

        private void AlRecibirRespuesta(RespuestaMensaje respuesta)
        {
            if (respuesta.Accion != Acciones.ConsultarTransacciones) return;
            if (IsDisposed || Disposing) return;
            if (InvokeRequired)
            {
                if (IsHandleCreated) BeginInvoke(new Action(() => AlRecibirRespuesta(respuesta)));
                return;
            }

            if (!respuesta.Exito)
            {
                lblTotal.Text = $"Error: {respuesta.Mensaje}";
                return;
            }

            TransaccionesRespuesta? datos = DatosMensaje.Leer<TransaccionesRespuesta>(respuesta.Datos);
            TransaccionDto[] transacciones = datos?.Transacciones ?? new TransaccionDto[0];

            dgvTransacciones.Rows.Clear();
            foreach (TransaccionDto t in transacciones)
            {
                dgvTransacciones.Rows.Add(t.Id, t.NumeroTurno, t.FechaHora.ToString("HH:mm:ss"), NombreTipo(t.Tipo),
                                          t.Origen, t.Destino, $"${t.Monto}", t.Descripcion);
            }
            lblTotal.Text = $"{transacciones.Length} transacciones";
        }

        // Nombre legible del tipo (solo presentación)
        private static string NombreTipo(string tipo)
        {
            switch (tipo)
            {
                case "CompraPropiedad": return "Compra de propiedad";
                case "PagoAlBanco": return "Pago al banco";
                case "PagoAlquiler": return "Pago de alquiler";
                case "PagoEntreJugadores": return "Pago entre jugadores";
                case "GananciaEvento": return "Ganancia por evento";
                case "PerdidaEvento": return "Pérdida por evento";
                case "PremioPorPasarInicio": return "Premio por pasar Inicio";
                default: return tipo;
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            cliente.RespuestaRecibida -= AlRecibirRespuesta;
            base.OnFormClosed(e);
        }
    }
}
