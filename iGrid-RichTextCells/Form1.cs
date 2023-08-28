using TenTec.Windows.iGridLib;

namespace iGrid_RichTextCells;

public partial class Form1 : Form
{
    public Form1()
    {
        InitializeComponent();
    }

	/// <summary>
	/// Manages (displays and edits) the rich text cells.
	/// </summary>
	private iGRichTextManager fManager;

	/// <summary>
	/// Initializes the grid.
	/// </summary>
	private void Form1_Load(object sender, System.EventArgs e)
	{
		// Create rich text manager.
		fManager = new iGRichTextManager(fGrid);

		// Set up the selection hightlight color.
		fGrid.SelCellsBackColor = Color.FromArgb(
			(2 * SystemColors.Highlight.R + 8 * SystemColors.Window.R) / 10,
			(2 * SystemColors.Highlight.G + 8 * SystemColors.Window.G) / 10,
			(2 * SystemColors.Highlight.B + 8 * SystemColors.Window.B) / 10);

		// Set an RTF text as the default cell value.
		fGrid.DefaultCol.DefaultCellValue = @"{\rtf1\ansi\ansicpg1251\deff0\deflang1049{\fonttbl{\f0\fswiss\fcharset0 Arial;}{\f1\fswiss\fcharset204{\*\fname Arial;}Arial CYR;}}{\colortbl ;\red255\green0\blue0;}{\*\generator Msftedit 5.41.15.1507;}\viewkind4\uc1\pard\lang1033\f0\fs20 This \i is\i0  a \b rich \cf1\b0 text \cf0\ul format\ulnone .\lang1049\f1\par}";

		// Setup default column properties.
		fGrid.DefaultCol.Width = 200;

		// Add a column.
		iGCol myCol = fGrid.Cols.Add();

		// Make the manager manage the column.
		fManager.ManageCol(myCol);

		// Add rows.
		fGrid.DefaultRow.Height = 70;
		fGrid.Rows.AddRange(10);
	}
}