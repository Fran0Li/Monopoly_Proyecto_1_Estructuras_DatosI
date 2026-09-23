namespace MonopolyCliente;

partial class FormLogin
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
        lblNombre = new Label();
        txtNombre = new TextBox();
        IblIp = new Label();
        txtIp = new TextBox();
        lblPuerto = new Label();
        txtPuerto = new TextBox();
        btnConectar = new Button();
        lblEstado = new Label();
        SuspendLayout();
        // 
        // lblNombre
        // 
        lblNombre.AutoSize = true;
        lblNombre.Location = new Point(312, 45);
        lblNombre.Name = "lblNombre";
        lblNombre.Size = new Size(57, 15);
        lblNombre.TabIndex = 0;
        lblNombre.Text = "Nombre :";
        // 
        // txtNombre
        // 
        txtNombre.Location = new Point(277, 76);
        txtNombre.Margin = new Padding(3, 2, 3, 2);
        txtNombre.Name = "txtNombre";
        txtNombre.Size = new Size(110, 23);
        txtNombre.TabIndex = 1;
        // 
        // IblIp
        // 
        IblIp.AutoSize = true;
        IblIp.Location = new Point(304, 121);
        IblIp.Name = "IblIp";
        IblIp.Size = new Size(65, 15);
        IblIp.TabIndex = 2;
        IblIp.Text = "IP servidor:";
        // 
        // txtIp
        // 
        txtIp.Location = new Point(277, 154);
        txtIp.Margin = new Padding(3, 2, 3, 2);
        txtIp.Name = "txtIp";
        txtIp.Size = new Size(110, 23);
        txtIp.TabIndex = 3;
        txtIp.Text = "127.0.0.1";
        // 
        // lblPuerto
        // 
        lblPuerto.AutoSize = true;
        lblPuerto.Location = new Point(312, 197);
        lblPuerto.Name = "lblPuerto";
        lblPuerto.Size = new Size(48, 15);
        lblPuerto.TabIndex = 4;
        lblPuerto.Text = "Puerto :";
        // 
        // txtPuerto
        // 
        txtPuerto.Location = new Point(277, 232);
        txtPuerto.Margin = new Padding(3, 2, 3, 2);
        txtPuerto.Name = "txtPuerto";
        txtPuerto.Size = new Size(110, 23);
        txtPuerto.TabIndex = 5;
        txtPuerto.Text = "5000";
        // 
        // btnConectar
        // 
        btnConectar.Location = new Point(287, 273);
        btnConectar.Margin = new Padding(3, 2, 3, 2);
        btnConectar.Name = "btnConectar";
        btnConectar.Size = new Size(82, 22);
        btnConectar.TabIndex = 6;
        btnConectar.Text = "Conectar";
        btnConectar.UseVisualStyleBackColor = true;
        btnConectar.Click += btnConectar_Click;
        // 
        // lblEstado
        // 
        lblEstado.AutoSize = true;
        lblEstado.Location = new Point(312, 297);
        lblEstado.Name = "lblEstado";
        lblEstado.Size = new Size(0, 15);
        lblEstado.TabIndex = 7;
        // 
        // FormLogin
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(700, 338);
        Controls.Add(lblEstado);
        Controls.Add(btnConectar);
        Controls.Add(txtPuerto);
        Controls.Add(lblPuerto);
        Controls.Add(txtIp);
        Controls.Add(IblIp);
        Controls.Add(txtNombre);
        Controls.Add(lblNombre);
        Margin = new Padding(3, 2, 3, 2);
        Name = "FormLogin";
        Text = "Monopoly-Conexión";
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private Label lblNombre;
    private TextBox txtNombre;
    private Label IblIp;
    private TextBox txtIp;
    private Label lblPuerto;
    private TextBox txtPuerto;
    private Button btnConectar;
    private Label lblEstado;
}
