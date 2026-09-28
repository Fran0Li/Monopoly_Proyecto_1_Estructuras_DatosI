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
        label1 = new Label();
        SuspendLayout();
        // 
        // lblNombre
        // 
        lblNombre.AutoSize = true;
        lblNombre.Location = new Point(345, 65);
        lblNombre.Name = "lblNombre";
        lblNombre.Size = new Size(71, 20);
        lblNombre.TabIndex = 0;
        lblNombre.Text = "Nombre :";
        // 
        // txtNombre
        // 
        txtNombre.Location = new Point(317, 101);
        txtNombre.Name = "txtNombre";
        txtNombre.Size = new Size(125, 27);
        txtNombre.TabIndex = 1;
        // 
        // IblIp
        // 
        IblIp.AutoSize = true;
        IblIp.Location = new Point(347, 161);
        IblIp.Name = "IblIp";
        IblIp.Size = new Size(81, 20);
        IblIp.TabIndex = 2;
        IblIp.Text = "IP servidor:";
        // 
        // txtIp
        // 
        txtIp.Location = new Point(317, 205);
        txtIp.Name = "txtIp";
        txtIp.Size = new Size(125, 27);
        txtIp.TabIndex = 3;
        txtIp.Text = "127.0.0.1";
        // 
        // lblPuerto
        // 
        lblPuerto.AutoSize = true;
        lblPuerto.Location = new Point(357, 263);
        lblPuerto.Name = "lblPuerto";
        lblPuerto.Size = new Size(59, 20);
        lblPuerto.TabIndex = 4;
        lblPuerto.Text = "Puerto :";
        // 
        // txtPuerto
        // 
        txtPuerto.Location = new Point(317, 309);
        txtPuerto.Name = "txtPuerto";
        txtPuerto.Size = new Size(125, 27);
        txtPuerto.TabIndex = 5;
        txtPuerto.Text = "5000";
        // 
        // btnConectar
        // 
        btnConectar.Font = new Font("Bahnschrift SemiBold", 13.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
        btnConectar.Location = new Point(308, 373);
        btnConectar.Name = "btnConectar";
        btnConectar.Size = new Size(143, 61);
        btnConectar.TabIndex = 6;
        btnConectar.Text = "Conectar";
        btnConectar.UseVisualStyleBackColor = true;
        btnConectar.Click += btnConectar_Click;
        // 
        // lblEstado
        // 
        lblEstado.AutoSize = true;
        lblEstado.Location = new Point(357, 396);
        lblEstado.Name = "lblEstado";
        lblEstado.Size = new Size(0, 20);
        lblEstado.TabIndex = 7;
        // 
        // label1
        // 
        label1.AutoSize = true;
        label1.Font = new Font("Bahnschrift SemiBold", 18F, FontStyle.Bold, GraphicsUnit.Point, 0);
        label1.Location = new Point(170, 9);
        label1.Name = "label1";
        label1.Size = new Size(457, 36);
        label1.TabIndex = 8;
        label1.Text = "Conectese al servidor para jugar";
        // 
        // FormLogin
        // 
        AutoScaleDimensions = new SizeF(8F, 20F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = SystemColors.ActiveCaption;
        ClientSize = new Size(800, 451);
        Controls.Add(label1);
        Controls.Add(lblEstado);
        Controls.Add(btnConectar);
        Controls.Add(txtPuerto);
        Controls.Add(lblPuerto);
        Controls.Add(txtIp);
        Controls.Add(IblIp);
        Controls.Add(txtNombre);
        Controls.Add(lblNombre);
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
    private Label label1;
}
