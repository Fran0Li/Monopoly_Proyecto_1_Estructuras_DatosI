namespace MonopolyCliente;

partial class FormHistorial
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
        lblJugador = new Label();
        cmbJugador = new ComboBox();
        lblTipo = new Label();
        cmbTipo = new ComboBox();
        chkRecientes = new CheckBox();
        btnConsultar = new Button();
        dgvTransacciones = new DataGridView();
        colId = new DataGridViewTextBoxColumn();
        colTurno = new DataGridViewTextBoxColumn();
        colHora = new DataGridViewTextBoxColumn();
        colTipo = new DataGridViewTextBoxColumn();
        colOrigen = new DataGridViewTextBoxColumn();
        colDestino = new DataGridViewTextBoxColumn();
        colMonto = new DataGridViewTextBoxColumn();
        colDescripcion = new DataGridViewTextBoxColumn();
        lblTotal = new Label();
        ((System.ComponentModel.ISupportInitialize)dgvTransacciones).BeginInit();
        SuspendLayout();
        //
        // lblJugador
        //
        lblJugador.AutoSize = true;
        lblJugador.Location = new Point(12, 17);
        lblJugador.Name = "lblJugador";
        lblJugador.Size = new Size(51, 15);
        lblJugador.TabIndex = 0;
        lblJugador.Text = "Jugador:";
        //
        // cmbJugador
        //
        cmbJugador.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbJugador.Location = new Point(70, 13);
        cmbJugador.Name = "cmbJugador";
        cmbJugador.Size = new Size(160, 23);
        cmbJugador.TabIndex = 1;
        //
        // lblTipo
        //
        lblTipo.AutoSize = true;
        lblTipo.Location = new Point(248, 17);
        lblTipo.Name = "lblTipo";
        lblTipo.Size = new Size(34, 15);
        lblTipo.TabIndex = 2;
        lblTipo.Text = "Tipo:";
        //
        // cmbTipo
        //
        cmbTipo.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbTipo.Location = new Point(288, 13);
        cmbTipo.Name = "cmbTipo";
        cmbTipo.Size = new Size(190, 23);
        cmbTipo.TabIndex = 3;
        //
        // chkRecientes
        //
        chkRecientes.AutoSize = true;
        chkRecientes.Location = new Point(496, 15);
        chkRecientes.Name = "chkRecientes";
        chkRecientes.Size = new Size(160, 19);
        chkRecientes.TabIndex = 4;
        chkRecientes.Text = "Más recientes primero";
        chkRecientes.UseVisualStyleBackColor = true;
        //
        // btnConsultar
        //
        btnConsultar.BackColor = Color.FromArgb(0, 48, 95);
        btnConsultar.FlatStyle = FlatStyle.Flat;
        btnConsultar.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        btnConsultar.ForeColor = Color.White;
        btnConsultar.Location = new Point(780, 9);
        btnConsultar.Name = "btnConsultar";
        btnConsultar.Size = new Size(108, 30);
        btnConsultar.TabIndex = 5;
        btnConsultar.Text = "Consultar";
        btnConsultar.UseVisualStyleBackColor = false;
        btnConsultar.Click += btnConsultar_Click;
        //
        // dgvTransacciones
        //
        dgvTransacciones.AllowUserToAddRows = false;
        dgvTransacciones.AllowUserToDeleteRows = false;
        dgvTransacciones.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        dgvTransacciones.BackgroundColor = Color.White;
        dgvTransacciones.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        dgvTransacciones.Columns.AddRange(new DataGridViewColumn[] { colId, colTurno, colHora, colTipo, colOrigen, colDestino, colMonto, colDescripcion });
        dgvTransacciones.Location = new Point(12, 48);
        dgvTransacciones.Name = "dgvTransacciones";
        dgvTransacciones.ReadOnly = true;
        dgvTransacciones.RowHeadersVisible = false;
        dgvTransacciones.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvTransacciones.Size = new Size(876, 440);
        dgvTransacciones.TabIndex = 6;
        //
        // colId
        //
        colId.HeaderText = "N°";
        colId.Name = "colId";
        colId.ReadOnly = true;
        colId.Width = 45;
        //
        // colTurno
        //
        colTurno.HeaderText = "Turno";
        colTurno.Name = "colTurno";
        colTurno.ReadOnly = true;
        colTurno.Width = 50;
        //
        // colHora
        //
        colHora.HeaderText = "Hora";
        colHora.Name = "colHora";
        colHora.ReadOnly = true;
        colHora.Width = 70;
        //
        // colTipo
        //
        colTipo.HeaderText = "Tipo";
        colTipo.Name = "colTipo";
        colTipo.ReadOnly = true;
        colTipo.Width = 150;
        //
        // colOrigen
        //
        colOrigen.HeaderText = "Origen";
        colOrigen.Name = "colOrigen";
        colOrigen.ReadOnly = true;
        colOrigen.Width = 100;
        //
        // colDestino
        //
        colDestino.HeaderText = "Destino";
        colDestino.Name = "colDestino";
        colDestino.ReadOnly = true;
        colDestino.Width = 100;
        //
        // colMonto
        //
        colMonto.HeaderText = "Monto";
        colMonto.Name = "colMonto";
        colMonto.ReadOnly = true;
        colMonto.Width = 70;
        //
        // colDescripcion
        //
        colDescripcion.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        colDescripcion.HeaderText = "Descripción";
        colDescripcion.Name = "colDescripcion";
        colDescripcion.ReadOnly = true;
        //
        // lblTotal
        //
        lblTotal.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        lblTotal.AutoSize = true;
        lblTotal.Location = new Point(12, 496);
        lblTotal.Name = "lblTotal";
        lblTotal.Size = new Size(0, 15);
        lblTotal.TabIndex = 7;
        //
        // FormHistorial
        //
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.FromArgb(237, 241, 246);
        ClientSize = new Size(900, 520);
        Controls.Add(lblTotal);
        Controls.Add(dgvTransacciones);
        Controls.Add(btnConsultar);
        Controls.Add(chkRecientes);
        Controls.Add(cmbTipo);
        Controls.Add(lblTipo);
        Controls.Add(cmbJugador);
        Controls.Add(lblJugador);
        MinimumSize = new Size(700, 300);
        Name = "FormHistorial";
        StartPosition = FormStartPosition.CenterParent;
        Text = "Historial de transacciones";
        ((System.ComponentModel.ISupportInitialize)dgvTransacciones).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private Label lblJugador;
    private ComboBox cmbJugador;
    private Label lblTipo;
    private ComboBox cmbTipo;
    private CheckBox chkRecientes;
    private Button btnConsultar;
    private DataGridView dgvTransacciones;
    private DataGridViewTextBoxColumn colId;
    private DataGridViewTextBoxColumn colTurno;
    private DataGridViewTextBoxColumn colHora;
    private DataGridViewTextBoxColumn colTipo;
    private DataGridViewTextBoxColumn colOrigen;
    private DataGridViewTextBoxColumn colDestino;
    private DataGridViewTextBoxColumn colMonto;
    private DataGridViewTextBoxColumn colDescripcion;
    private Label lblTotal;
}
