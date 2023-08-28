namespace iGrid_RichTextCells;

partial class Form1
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
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        this.fGrid = new TenTec.Windows.iGridLib.iGrid();
        ((System.ComponentModel.ISupportInitialize)(this.fGrid)).BeginInit();
        this.SuspendLayout();
        // 
        // fGrid
        // 
        this.fGrid.Dock = System.Windows.Forms.DockStyle.Fill;
        this.fGrid.Header.Height = 19;
        this.fGrid.Location = new System.Drawing.Point(0, 0);
        this.fGrid.Name = "fGrid";
        this.fGrid.Size = new System.Drawing.Size(357, 339);
        this.fGrid.TabIndex = 0;
        // 
        // Form1
        // 
        this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
        this.ClientSize = new System.Drawing.Size(357, 339);
        this.Controls.Add(this.fGrid);
        this.Name = "Form1";
        this.Text = "Form1";
        this.Load += new System.EventHandler(this.Form1_Load);
        ((System.ComponentModel.ISupportInitialize)(this.fGrid)).EndInit();
        this.ResumeLayout(false);

    }

    #endregion

    private TenTec.Windows.iGridLib.iGrid fGrid;
}