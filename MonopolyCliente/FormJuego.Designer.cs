namespace MonopolyCliente;

partial class FormJuego
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        tablero = new MonopolyCliente.Interfaz.TableroControl();
        lblTurno = new Label();
        lblAviso = new Label();
        lvJugadores = new ListView();
        colColor = new ColumnHeader();
        colNombre = new ColumnHeader();
        colSaldo = new ColumnHeader();
        colPropiedades = new ColumnHeader();
        colPatrimonio = new ColumnHeader();
        colEstado = new ColumnHeader();
        btnIniciar = new Button();
        btnTirar = new Button();
        btnComprar = new Button();
        btnNoComprar = new Button();
        btnTerminar = new Button();
        btnVincular = new Button();
        btnHistorial = new Button();
        btnExportar = new Button();
        lblMensaje = new Label();
        lblLog = new Label();
        rtbLog = new RichTextBox();
        lblConexion = new Label();
        SuspendLayout();
        //
        // tablero
        //
        tablero.Location = new Point(10, 10);
        tablero.Name = "tablero";
        tablero.Size = new Size(600, 600);
        tablero.TabIndex = 0;
        //
        // lblTurno
        //
        lblTurno.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
        lblTurno.ForeColor = Color.FromArgb(0, 48, 95);
        lblTurno.Location = new Point(622, 10);
        lblTurno.Name = "lblTurno";
        lblTurno.Size = new Size(510, 30);
        lblTurno.TabIndex = 1;
        lblTurno.Text = "Conectando...";
        lblTurno.TextAlign = ContentAlignment.MiddleLeft;
        //
        // lblAviso
        //
        lblAviso.BackColor = Color.FromArgb(255, 236, 179);
        lblAviso.BorderStyle = BorderStyle.FixedSingle;
        lblAviso.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        lblAviso.Location = new Point(622, 44);
        lblAviso.Name = "lblAviso";
        lblAviso.Size = new Size(510, 42);
        lblAviso.TabIndex = 2;
        lblAviso.TextAlign = ContentAlignment.MiddleCenter;
        lblAviso.Visible = false;
        //
        // lvJugadores
        //
        lvJugadores.Columns.AddRange(new ColumnHeader[] { colColor, colNombre, colSaldo, colPropiedades, colPatrimonio, colEstado });
        lvJugadores.Font = new Font("Segoe UI", 10F);
        lvJugadores.FullRowSelect = true;
        lvJugadores.HeaderStyle = ColumnHeaderStyle.Nonclickable;
        lvJugadores.Location = new Point(622, 90);
        lvJugadores.MultiSelect = false;
        lvJugadores.Name = "lvJugadores";
        lvJugadores.Size = new Size(510, 110);
        lvJugadores.TabIndex = 3;
        lvJugadores.UseCompatibleStateImageBehavior = false;
        lvJugadores.View = View.Details;
        //
        // colColor
        //
        colColor.Text = "";
        colColor.Width = 26;
        //
        // colNombre
        //
        colNombre.Text = "Jugador";
        colNombre.Width = 130;
        //
        // colSaldo
        //
        colSaldo.Text = "Saldo";
        colSaldo.Width = 72;
        //
        // colPropiedades
        //
        colPropiedades.Text = "Prop.";
        colPropiedades.Width = 48;
        //
        // colPatrimonio
        //
        colPatrimonio.Text = "Patrimonio";
        colPatrimonio.Width = 88;
        //
        // colEstado
        //
        colEstado.Text = "Estado";
        colEstado.Width = 138;
        //
        // btnIniciar
        //
        btnIniciar.BackColor = Color.FromArgb(0, 48, 95);
        btnIniciar.FlatStyle = FlatStyle.Flat;
        btnIniciar.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        btnIniciar.ForeColor = Color.White;
        btnIniciar.Location = new Point(622, 206);
        btnIniciar.Name = "btnIniciar";
        btnIniciar.Size = new Size(98, 38);
        btnIniciar.TabIndex = 4;
        btnIniciar.Text = "Iniciar";
        btnIniciar.UseVisualStyleBackColor = false;
        btnIniciar.Click += btnIniciar_Click;
        //
        // btnTirar
        //
        btnTirar.BackColor = Color.FromArgb(0, 48, 95);
        btnTirar.FlatStyle = FlatStyle.Flat;
        btnTirar.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        btnTirar.ForeColor = Color.White;
        btnTirar.Location = new Point(725, 206);
        btnTirar.Name = "btnTirar";
        btnTirar.Size = new Size(98, 38);
        btnTirar.TabIndex = 5;
        btnTirar.Text = "Tirar dados";
        btnTirar.UseVisualStyleBackColor = false;
        btnTirar.Click += btnTirar_Click;
        //
        // btnComprar
        //
        btnComprar.BackColor = Color.FromArgb(0, 48, 95);
        btnComprar.FlatStyle = FlatStyle.Flat;
        btnComprar.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        btnComprar.ForeColor = Color.White;
        btnComprar.Location = new Point(828, 206);
        btnComprar.Name = "btnComprar";
        btnComprar.Size = new Size(98, 38);
        btnComprar.TabIndex = 6;
        btnComprar.Text = "Comprar";
        btnComprar.UseVisualStyleBackColor = false;
        btnComprar.Click += btnComprar_Click;
        //
        // btnNoComprar
        //
        btnNoComprar.BackColor = Color.FromArgb(0, 48, 95);
        btnNoComprar.FlatStyle = FlatStyle.Flat;
        btnNoComprar.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        btnNoComprar.ForeColor = Color.White;
        btnNoComprar.Location = new Point(931, 206);
        btnNoComprar.Name = "btnNoComprar";
        btnNoComprar.Size = new Size(98, 38);
        btnNoComprar.TabIndex = 7;
        btnNoComprar.Text = "No comprar";
        btnNoComprar.UseVisualStyleBackColor = false;
        btnNoComprar.Click += btnNoComprar_Click;
        //
        // btnTerminar
        //
        btnTerminar.BackColor = Color.FromArgb(0, 48, 95);
        btnTerminar.FlatStyle = FlatStyle.Flat;
        btnTerminar.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        btnTerminar.ForeColor = Color.White;
        btnTerminar.Location = new Point(1034, 206);
        btnTerminar.Name = "btnTerminar";
        btnTerminar.Size = new Size(98, 38);
        btnTerminar.TabIndex = 8;
        btnTerminar.Text = "Terminar turno";
        btnTerminar.UseVisualStyleBackColor = false;
        btnTerminar.Click += btnTerminar_Click;
        //
        // btnVincular
        //
        btnVincular.BackColor = Color.FromArgb(0, 102, 170);
        btnVincular.FlatStyle = FlatStyle.Flat;
        btnVincular.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        btnVincular.ForeColor = Color.White;
        btnVincular.Location = new Point(622, 250);
        btnVincular.Name = "btnVincular";
        btnVincular.Size = new Size(166, 30);
        btnVincular.TabIndex = 9;
        btnVincular.Text = "Vincular tarjeta RFID";
        btnVincular.UseVisualStyleBackColor = false;
        btnVincular.Click += btnVincular_Click;
        //
        // btnHistorial
        //
        btnHistorial.BackColor = Color.FromArgb(0, 102, 170);
        btnHistorial.FlatStyle = FlatStyle.Flat;
        btnHistorial.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        btnHistorial.ForeColor = Color.White;
        btnHistorial.Location = new Point(793, 250);
        btnHistorial.Name = "btnHistorial";
        btnHistorial.Size = new Size(166, 30);
        btnHistorial.TabIndex = 10;
        btnHistorial.Text = "Historial";
        btnHistorial.UseVisualStyleBackColor = false;
        btnHistorial.Click += btnHistorial_Click;
        //
        // btnExportar
        //
        btnExportar.BackColor = Color.FromArgb(0, 102, 170);
        btnExportar.FlatStyle = FlatStyle.Flat;
        btnExportar.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        btnExportar.ForeColor = Color.White;
        btnExportar.Location = new Point(964, 250);
        btnExportar.Name = "btnExportar";
        btnExportar.Size = new Size(168, 30);
        btnExportar.TabIndex = 11;
        btnExportar.Text = "Exportar TXT";
        btnExportar.UseVisualStyleBackColor = false;
        btnExportar.Click += btnExportar_Click;
        //
        // lblMensaje
        //
        lblMensaje.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
        lblMensaje.Location = new Point(622, 284);
        lblMensaje.Name = "lblMensaje";
        lblMensaje.Size = new Size(510, 20);
        lblMensaje.TabIndex = 12;
        lblMensaje.TextAlign = ContentAlignment.MiddleLeft;
        //
        // lblLog
        //
        lblLog.AutoSize = true;
        lblLog.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        lblLog.ForeColor = Color.FromArgb(0, 48, 95);
        lblLog.Location = new Point(622, 308);
        lblLog.Name = "lblLog";
        lblLog.Size = new Size(138, 15);
        lblLog.TabIndex = 13;
        lblLog.Text = "Registro de la partida";
        //
        // rtbLog
        //
        rtbLog.BackColor = Color.White;
        rtbLog.Font = new Font("Segoe UI", 9F);
        rtbLog.Location = new Point(622, 326);
        rtbLog.Name = "rtbLog";
        rtbLog.ReadOnly = true;
        rtbLog.Size = new Size(510, 266);
        rtbLog.TabIndex = 14;
        rtbLog.Text = "";
        //
        // lblConexion
        //
        lblConexion.Font = new Font("Segoe UI", 8.25F);
        lblConexion.ForeColor = Color.DimGray;
        lblConexion.Location = new Point(622, 596);
        lblConexion.Name = "lblConexion";
        lblConexion.Size = new Size(510, 18);
        lblConexion.TabIndex = 15;
        lblConexion.TextAlign = ContentAlignment.MiddleLeft;
        //
        // FormJuego
        //
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.FromArgb(237, 241, 246);
        ClientSize = new Size(1142, 620);
        Controls.Add(lblConexion);
        Controls.Add(rtbLog);
        Controls.Add(lblLog);
        Controls.Add(lblMensaje);
        Controls.Add(btnExportar);
        Controls.Add(btnHistorial);
        Controls.Add(btnVincular);
        Controls.Add(btnTerminar);
        Controls.Add(btnNoComprar);
        Controls.Add(btnComprar);
        Controls.Add(btnTirar);
        Controls.Add(btnIniciar);
        Controls.Add(lvJugadores);
        Controls.Add(lblAviso);
        Controls.Add(lblTurno);
        Controls.Add(tablero);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        Name = "FormJuego";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Monopoly TEC";
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private MonopolyCliente.Interfaz.TableroControl tablero;
    private Label lblTurno;
    private Label lblAviso;
    private ListView lvJugadores;
    private ColumnHeader colColor;
    private ColumnHeader colNombre;
    private ColumnHeader colSaldo;
    private ColumnHeader colPropiedades;
    private ColumnHeader colPatrimonio;
    private ColumnHeader colEstado;
    private Button btnIniciar;
    private Button btnTirar;
    private Button btnComprar;
    private Button btnNoComprar;
    private Button btnTerminar;
    private Button btnVincular;
    private Button btnHistorial;
    private Button btnExportar;
    private Label lblMensaje;
    private Label lblLog;
    private RichTextBox rtbLog;
    private Label lblConexion;
}
