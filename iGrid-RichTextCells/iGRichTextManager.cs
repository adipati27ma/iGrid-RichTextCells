using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TenTec.Windows.iGridLib;

namespace iGrid_RichTextCells;

public class iGRichTextManager
{
    #region Public

    /// <summary>
    /// Creates a new instance of the iGRichTextManager class.
    /// </summary>
    /// <param name="grid">
    /// The grid which will be managed by the new instance.
    /// </param>
    public iGRichTextManager(iGrid grid)
    {
        #region Check the parameters

        if (grid == null)
            throw new ArgumentNullException("grid");

        #endregion

        fGrid = grid;

        #region Attach iGrid event handlers

        fGrid.CustomDrawCellForeground += new iGCustomDrawCellEventHandler(fGrid_CustomDrawCellForeground);
        fGrid.CustomDrawCellGetHeight += new iGCustomDrawCellGetHeightEventHandler(fGrid_CustomDrawCellGetHeight);
        fGrid.CustomDrawCellGetWidth += new iGCustomDrawCellGetWidthEventHandler(fGrid_CustomDrawCellGetWidth);
        fGrid.ColDividerDoubleClick += new iGColDividerDoubleClickEventHandler(fGrid_ColDividerDoubleClick);
        fGrid.RequestEdit += new iGRequestEditEventHandler(fGrid_RequestEdit);
        fGrid.QuitCustomEdit += new EventHandler(fGrid_QuitCustomEdit);

        #endregion
    }

    /// <summary>
    /// Makes the manager treat the specified column as 
    /// a rich text column.
    /// </summary>
    public void ManageCol(iGCol col)
    {
        #region Check the parameters

        if (col == null)
            throw new ArgumentNullException("col");

        #endregion

        // Adjust the column's style.
        col.CellStyle.CustomDrawFlags = iGCustomDrawFlags.Foreground;

        // Mark the column as a rich text column.
        col.Tag = cRichTextCol;
    }

    #endregion

    #region Private

    #region DrawingRichRextBox

    /// <summary>
    /// A rich text box control with an ability to send native window messages.
    /// This class is used to draw in cells.
    /// </summary>
    private class DrawingRichTextBox : RichTextBox
    {
        /// <summary>
        /// Sends the specified native message to the undelying Win32
        /// Rish Text Box.
        /// </summary>			
        public int SendMessage(int msg, IntPtr wParam, IntPtr lParam)
        {
            Message m = Message.Create(Handle, msg, wParam, lParam);
            WndProc(ref m);
            return (int)m.Result;
        }
    }

    #endregion

    #region Messages

    private const int WM_USER = 0x0400;

    private const int EM_FORMATRANGE = WM_USER + 57;

    #endregion

    #region Structures

    private struct FORMATRANGE
    {
        public IntPtr hdc, hdcTarget;

        public Rectangle rc, rcPage;

        public int cpMin, cpMax;
    }

    #endregion

    #region Consts

    private const string cRichTextCol = "RichTextCol";

    #endregion

    #region Common Fields

    /// <summary>
    /// The grid which is managed by this manager.
    /// </summary>
    private iGrid fGrid;

    #endregion

    #region Common Methods

    /// <summary>
    /// Returns the contents indents of the specified cell.
    /// </summary>
    private iGIndent GetCellContentIndent(iGCell cell)
    {
        return cell.Col.CellStyle.ContentIndent;
    }

    #endregion

    #region Drawing

    /// <summary>
    /// The rich text box used for drawing.
    /// </summary>
    private DrawingRichTextBox fDrawingRichTextBox;

    /// <summary>
    /// Draw the foreground of RTF cells.
    /// </summary>
    private void fGrid_CustomDrawCellForeground(object sender, TenTec.Windows.iGridLib.iGCustomDrawCellEventArgs e)
    {
        #region Check whether it is a rich text cell

        iGCell myCell = fGrid.Cells[e.RowIndex, e.ColIndex];
        if (!object.ReferenceEquals(myCell.Col.Tag, cRichTextCol))
            return;

        #endregion

        #region Determine the background color

        Color myBackColor;
        if (e.Selected)
        {
            if (fGrid.Focused)
                myBackColor = fGrid.SelCellsBackColor;
            else
                myBackColor = fGrid.SelCellsBackColorNoFocus;
        }
        else
            myBackColor = myCell.EffectiveBackColor;

        #endregion

        #region Determine the text bounds

        Rectangle myBounds = e.Bounds;
        iGIndent myContentIndent = GetCellContentIndent(myCell);
        myBounds.X += myContentIndent.Left;
        myBounds.Y += myContentIndent.Top;
        myBounds.Width -= myContentIndent.Left + myContentIndent.Right;
        myBounds.Height -= myContentIndent.Top + myContentIndent.Bottom;
        if (myBounds.Width <= 0 || myBounds.Height <= 0)
            return;

        #endregion

        Render(e.Graphics, myBounds, myCell.Value as string, myBackColor);
    }

    /// <summary>
    /// Calculates the height needed to show the contents of
    /// RTF cells. The RichTextBox allows you to determine the height of 
    /// a single line text only.
    /// </summary>
    private void fGrid_CustomDrawCellGetHeight(object sender, TenTec.Windows.iGridLib.iGCustomDrawCellGetHeightEventArgs e)
    {
        #region Check whether it is a rich text cell

        iGCell myCell = fGrid.Cells[e.RowIndex, e.ColIndex];
        if (!object.ReferenceEquals(myCell.Col.Tag, cRichTextCol))
            return;

        #endregion

        iGIndent myContentIndent = GetCellContentIndent(myCell);
        using (Graphics myGraphics = fGrid.CreateGraphics())
            e.Height = MeasureHeight(myGraphics, myCell.TextBounds.Width, myCell.Value as string) + myContentIndent.Top + myContentIndent.Bottom;
    }

    /// <summary>
    /// Calculates the width needed to show the contents of
    /// RTF cells. As RichTextBox does not allow you to 
    /// calculate the width of the text displayed in it,
    /// we just return a constant value. This method is 
    /// invoked when the user double-clicks a column header
    /// divider of a custom-draw cell.
    /// </summary>
    private void fGrid_CustomDrawCellGetWidth(object sender, TenTec.Windows.iGridLib.iGCustomDrawCellGetWidthEventArgs e)
    {
        #region Check whether it is a rich text cell

        if (!object.ReferenceEquals(fGrid.Cols[e.ColIndex].Tag, cRichTextCol))
            return;

        #endregion

        e.Width = 150;
    }

    /// <summary>
    /// Prohibits the auto-width operation on the rich text columns 
    /// as the RichTextBox does not allow you to determine the width
    /// of its text.
    /// </summary>
    private void fGrid_ColDividerDoubleClick(object sender, iGColDividerDoubleClickEventArgs e)
    {
        #region Check whether it is a rich text column

        if (!object.ReferenceEquals(fGrid.Cols[e.ColIndex].Tag, cRichTextCol))
            return;

        #endregion

        e.DoDefault = false;
    }

    /// <summary>
    /// Measures the height of the specified RTF text based on the width of the 
    /// output area.
    /// </summary>
    private int MeasureHeight(Graphics g, int width, string rtfText)
    {
        Rectangle myRect = new Rectangle(0, 0, (int)(width * 1440 / g.DpiX), int.MaxValue);

        int myHeight = 0;
        IntPtr myHdc = g.GetHdc();
        try
        {
            FORMATRANGE myFormat = new FORMATRANGE();
            myFormat.hdc = myFormat.hdcTarget = myHdc;
            myFormat.rc = myFormat.rcPage = myRect;
            myFormat.cpMin = 0;
            myFormat.cpMax = -1;

            bool myRender = false;
            GetDrawingRichTextBox().Rtf = rtfText;
            unsafe
            {
                GetDrawingRichTextBox().SendMessage(EM_FORMATRANGE, (IntPtr)Convert.ToInt32(myRender), (IntPtr)(&myFormat));
            }
            GetDrawingRichTextBox().SendMessage(EM_FORMATRANGE, IntPtr.Zero, IntPtr.Zero);
            myHeight = myFormat.rc.Height;
        }
        finally
        {
            g.ReleaseHdc(myHdc);
        }

        return (int)(myHeight * g.DpiX / 1440);
    }

    /// <summary>
    /// Draws the specified RTF text in the specified rectangle on the 
    /// specified graphics surface.
    /// </summary>
    private void Render(Graphics g, Rectangle rect, string rtfText, Color backColor)
    {
        using (Bitmap myBitmap = new Bitmap(rect.Width, rect.Height))
        {
            // Render the text to a temporary bitmap.
            Rectangle myRect = new Rectangle(0, 0, rect.Width, rect.Height);
            using (Graphics myGraphics = Graphics.FromImage(myBitmap))
                RenderDirect(myGraphics, myRect, rtfText, backColor);

            // Copy the image from the temporary bitmap to the output 
            // graphics.
            g.DrawImage(myBitmap, rect, myRect, GraphicsUnit.Pixel);
        }
    }

    /// <summary>
    /// Draws the specified text on the specified graphics surface 
    /// directly, without buffering.
    /// </summary>
    private void RenderDirect(Graphics g, Rectangle rect, string rtfText, Color backColor)
    {
        // Convert the rectangle from pixels to twips
        rect.X = (int)(rect.X * 1440 / g.DpiX);
        rect.Y = (int)(rect.Y * 1440 / g.DpiY);
        rect.Width = rect.X + (int)(rect.Width * 1440 / g.DpiX);
        rect.Height = rect.Y + (int)(rect.Height * 1440 / g.DpiY);

        IntPtr myHdc = g.GetHdc();
        try
        {
            FORMATRANGE myFormat = new FORMATRANGE();
            myFormat.hdc = myFormat.hdcTarget = myHdc;
            myFormat.rc = myFormat.rcPage = rect;
            myFormat.cpMin = 0;
            myFormat.cpMax = -1;

            bool myRender = true;
            GetDrawingRichTextBox().Rtf = rtfText;
            GetDrawingRichTextBox().BackColor = backColor;
            unsafe
            {
                GetDrawingRichTextBox().SendMessage(EM_FORMATRANGE, (IntPtr)Convert.ToInt32(myRender), (IntPtr)(&myFormat));
            }
            GetDrawingRichTextBox().SendMessage(EM_FORMATRANGE, IntPtr.Zero, IntPtr.Zero);
        }
        finally
        {
            g.ReleaseHdc(myHdc);
        }
    }

    /// <summary>
    /// Returns the RichTextBox control which is used by the
    /// manager to redred text.
    /// </summary>
    private DrawingRichTextBox GetDrawingRichTextBox()
    {
        if (fDrawingRichTextBox == null)
        {
            fDrawingRichTextBox = new DrawingRichTextBox();
        }
        return fDrawingRichTextBox;
    }

    #endregion

    #region Editing

    /// <summary>
    /// Stores the cell which is being currently edited.
    /// </summary>
    iGCell fCell;

    /// <summary>
    /// Determines whether to accept the value entered to 
    /// the RichTextBox control.
    /// </summary>
    bool fAccept;

    /// <summary>
    /// The RichTextBox control which is used to edit cells.
    /// </summary>
    RichTextBox fEditingRichTextBox;

    /// <summary>
    /// Starts editing of rich text cells.
    /// </summary>
    private void fGrid_RequestEdit(object sender, TenTec.Windows.iGridLib.iGRequestEditEventArgs e)
    {
        #region Check whether it is a rich text cell

        fCell = fGrid.Cells[e.RowIndex, e.ColIndex];
        if (!object.ReferenceEquals(fCell.Col.Tag, cRichTextCol))
            return;

        #endregion

        // Make the cell entirely visible.
        fCell.EnsureVisible();

        // Determine the coordinates of the RichTextBox control.
        Rectangle myBounds = fCell.TextBounds;
        myBounds.Inflate(1, 0);
        if (myBounds.Width <= 0 || myBounds.Height <= 0)
            return;

        // This code is required if you can have cells which
        // are higher or wider than the entire cells area.
        Rectangle myCellsArea = fGrid.CellsAreaBounds;
        if (myBounds.Right > myCellsArea.Right)
            myBounds.Width -= myBounds.Right - myCellsArea.Right;
        if (myBounds.Bottom > myCellsArea.Bottom)
            myBounds.Height -= myBounds.Bottom - myCellsArea.Bottom;

        // Create the RichTextBox control used for 
        // editing if it has not been created yet.
        if (fEditingRichTextBox == null)
        {
            fEditingRichTextBox = new RichTextBox();
            fEditingRichTextBox.BorderStyle = BorderStyle.None;
            fEditingRichTextBox.ScrollBars = RichTextBoxScrollBars.None;
            fEditingRichTextBox.Leave += new EventHandler(fEditingRichTextBox_Leave);
            fEditingRichTextBox.KeyDown += new KeyEventHandler(fEditingRichTextBox_KeyDown);
        }

        // Put the RichTextBox control to the grid.
        fEditingRichTextBox.Visible = false;
        fEditingRichTextBox.Parent = fGrid;

        // Set up the RichTextBox control's coordinates.
        fEditingRichTextBox.Bounds = myBounds;

        // Put the value from the current cell into 
        // the RichTextBox control.
        fEditingRichTextBox.Rtf = fCell.Value as string;

        // Set up the RichTextBox control's background color.s
        fEditingRichTextBox.BackColor = fCell.EffectiveBackColor;

        // Display our editor and move the focus to it.
        fEditingRichTextBox.Visible = true;
        fEditingRichTextBox.Focus();

        fAccept = true;

        // Lock operations with mouse button down to support the editor value validation.
        // If you set the MouseDownLocked property to true iGrid will not perform 
        // any action (column width changing, sorting, cur cell and selection changing) 
        // when user presses a mouse button in the cells area or column header
        fGrid.MouseDownLocked = true;

        // As the grid marks the selected cells with different colors when it is 
        // focused and when isn't, make it draw as if it was focused. 
        fGrid.DrawAsFocused = true;

        e.DoDefault = false;
    }

    /// <summary>
    /// Processes the Enter and Esc keys to commit and cancel editing.
    /// </summary>
    private void fEditingRichTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        switch (e.KeyCode)
        {
            case Keys.Enter:
                if ((e.Modifiers & Keys.Shift) == Keys.Shift)
                    return;
                fGrid.Focus();
                e.Handled = true;
                break;
            case Keys.Escape:
                fAccept = false;
                fGrid.Focus();
                e.Handled = true;
                break;
        }
    }

    /// <summary>
    /// Commits or cancels the entered value and hides the RichTextBox control.
    /// </summary>
    private void fEditingRichTextBox_Leave(object sender, EventArgs e)
    {
        if (fEditingRichTextBox.Visible)
        {
            // Finish editing.
            fEditingRichTextBox.Visible = false;
            if (fAccept)
                fCell.Value = fEditingRichTextBox.Rtf;
            fGrid.MouseDownLocked = false;
            fGrid.DrawAsFocused = false;
        }
    }

    /// <summary>
    /// Hides the RichTextBox control.
    /// </summary>
    private void fGrid_QuitCustomEdit(object sender, System.EventArgs e)
    {
        fGrid.Focus();
    }

    #endregion

    #endregion
}
