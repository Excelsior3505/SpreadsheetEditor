using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SS;
using System.Text.RegularExpressions;
using SpreadsheetUtilities;
using ClientNetworking;
using System.Net.Sockets;

namespace SpreadsheetGUI
{
    // Sharon Xiao
    // u0943650

    // Linxi Li
    // u1016104
    public partial class Form1 : Form
    {
        // Spreadsheet object we are going to use for each cell in the spreadsheet panel
        Spreadsheet sheet;

        /// <summary>
        /// varaiant shows if user is highlighting a cell or the panel
        /// </summary>
        private bool IsPanleFocused;

        // The name displaying on the title of the form
        private string docName = "sheet" + SSContextSingleton.getContext().formCount;

        // properties and variables

        private Func<string, bool> Validator;
        private Func<string, string> Normalizer;
        private string version = "default";
        private string filePath;
        // the colume and the row of the spreadsheet panel
        private int col;
        private int row;


        /// <summary>
        /// Form1 constructor without paramiter
        /// </summary>
        public Form1()
        {
            //the Validator with a lambda expression
            Validator = s => Regex.IsMatch(s, @"[A-Z][1-9][0-9]?");

            //the Normalizer with a lambda expression
            Normalizer = s => s.ToUpper();

            // the spreadsheet that will be used in this class
            sheet = new Spreadsheet(Validator, Normalizer, version);

            // initialize all conponents
            InitializeComponent();

            // the name of the documents(show on title)
            this.Text = "SpreadSheet" + " - " + docName;

            // show the defult cooddinate of stating cell
            Coordi_textBox.Text = "A1";
        }

        /// <summary>
        /// Form1 constructor with paramiter filepath
        /// </summary>
        /// <param name="p">filepath</param>
        public Form1(string path)
        {
            // the path of the file
            filePath = path;

            // see above 
            Validator = s => Regex.IsMatch(s, @"[A-Z][1-9][0-9]?");
            Normalizer = s => s.ToUpper();
            sheet = new Spreadsheet(filePath, Validator, Normalizer, version);
            InitializeComponent();
            this.Text = "SpreadSheet" + "- " + docName;
            Display();
        }


        /// <summary>
        /// If click the newToolStripMenuItem, do the following reaction
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // if the sheet has not been changed, hide this form and create a new one
            if (!(sheet.Changed))
            {
                this.Hide();
                SSContextSingleton.getContext().RunForm(new Form1());
            }
            else
            {
                // warning the user if they want to save first before creating
                DialogResult result = MessageBox.Show("Do you want to save this spreadsheet before creating a new spreadsheet?",
                    "Message", MessageBoxButtons.YesNoCancel);
                // if yes, save the file
                if (result == DialogResult.Yes)
                {
                    if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                    {
                        try
                        {
                            filePath = saveFileDialog1.FileName;
                            backgroundWorker1.RunWorkerAsync();
                            MessageBox.Show("Saved.");

                            getFileName();
                            this.Text = docName;
                        }
                        catch (SpreadsheetReadWriteException)
                        {
                            MessageBox.Show("Unable to save.");
                        }
                        this.Hide();
                        SSContextSingleton.getContext().RunForm(new Form1());
                    }
                }
                // if no, hide the form and creat a new one
                if (result == DialogResult.No)
                {
                    this.Hide();
                    SSContextSingleton.getContext().RunForm(new Form1());

                }
                // if cancle, do nothing
                if (result == DialogResult.Cancel)
                {
                    return;
                }
            }
        }

        /// <summary>
        /// If click the openToolStripMenuItem, do the following reaction
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // if the sheet has not been changed, directely open the target sheet
            if (!(sheet.Changed))
            {
                this.Hide();
                OpenFileDialog OpenFileDialog1 = new OpenFileDialog();
                OpenFileDialog1.Filter = "Spreadsheet files|*.sprd|All Files|*.*";
                OpenFileDialog1.Title = "Open Saved Spreadsheet";
                open(OpenFileDialog1);
            }
            
            else
            {
                // warning the user if they want save the sheet at the first place
                DialogResult result = MessageBox.Show("Do you want to save this spreadsheet before opening other spreadsheets?",
                    "Message", MessageBoxButtons.YesNoCancel);
                // if yes, save execute
                if (result == DialogResult.Yes)
                {
                    if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                    {
                        try
                        {
                            filePath = saveFileDialog1.FileName;
                            backgroundWorker1.RunWorkerAsync();
                            MessageBox.Show("Successfully saved.", "Message");
                            getFileName();
                            this.Text = docName;
                        }
                        catch (SpreadsheetReadWriteException)
                        {
                            MessageBox.Show("Unable to save.", "Message");
                        }
                        OpenFileDialog OpenFileDialog1 = new OpenFileDialog();
                        OpenFileDialog1.Filter = "Spreadsheet files|*.sprd|All Files|*.*";
                        OpenFileDialog1.Title = "Open Saved Spreadsheet";
                        open(OpenFileDialog1);
                    }
                }
                // if no, hide the sheet and open a target one
                if (result == DialogResult.No)
                {

                    OpenFileDialog OpenFileDialog1 = new OpenFileDialog();
                    OpenFileDialog1.Filter = "Spreadsheet files|*.sprd|All Files|*.*";
                    OpenFileDialog1.Title = "Open Saved Spreadsheet";
                    open(OpenFileDialog1);
                }
                // if cancel, do nothing
                if (result == DialogResult.Cancel)
                {
                    return;
                }
            }
        }

        /// <summary>
        /// private helper function to help open the file
        /// </summary>
        /// <param name="OpenFileDialog"></param>
        private void open(OpenFileDialog OpenFileDialog)
        {
            // if the user hit ok
            if (OpenFileDialog.ShowDialog() == DialogResult.OK)
            {
                // set the file path to the OpenFileDialog.FileName
                filePath = OpenFileDialog.FileName;

                // change the extension of the file
                System.IO.Path.ChangeExtension(filePath, ".xml");

                // clear teh spreadsheetpanel and hide
                spreadsheetPanel1.Clear();
                this.Hide();

                // create a new form1 instance
                Form1 newform = new Form1(filePath);
                try
                {
                    // run the form and setvalues to the cells
                    SSContextSingleton.getContext().RunForm(newform);
                    Display();
                }
                catch (SpreadsheetReadWriteException)
                {
                    // show a messagebox if the file cannot be opened
                    MessageBox.Show("Unable to open the file.","Message");
                }

            }
        }
        /// <summary>
        /// If click the saveToolStripMenuItem, do the following reaction
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // filePath is null, excute saveAsToolStripMenuItem_Click function
            if (filePath == null)
            {
                saveAsToolStripMenuItem_Click(sender, e);
            }
            // the sheet has been changed, save the sheet directly
            if (sheet.Changed)
            {
                sheet.Save(docName);
                MessageBox.Show("Successfully saved.", "Message");
            }
            // if the sheet hasn't been changed. do nothing
            if (!sheet.Changed)
            {
                return;
            }
        }

        /// <summary>
        ///  The Save As menu botton to save the sheet to a targeting location with a name.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // if the user hit ok
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    // set the file path to saveFileDialog1.FileName
                    filePath = saveFileDialog1.FileName;

                    // save file by using the backgroundWorker1
                    backgroundWorker1.RunWorkerAsync();

                    // show message after saveing
                    MessageBox.Show("Successfully saved.", "Message");

                    // display the new name
                    getFileName();
                    this.Text = docName;
                }
                // return message shows Unable to save if SpreadsheetReadWriteException catched
                catch (SpreadsheetReadWriteException)
                {
                    MessageBox.Show("Unable to save.", "Message");
                }
            }
        }
        /// <summary>
        /// backgroundWorker1_DoWork helper to save the file.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            lock (send)
            {
                try { sheet.Save(filePath); }
                catch (SpreadsheetReadWriteException ex)
                {
                    MessageBox.Show("Could not save " + docName + "\n" + ex.Message);
                }
            }
        }
        // the readonly object that will be used in the save as method
        private readonly object send = new object();

        /// <summary>
        /// Excuting closing the program when the close botton is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // if there is no change in the sheet, close the sheet directely
            if (!(sheet.Changed))
                Close();
            else
            {   
                // warning the user with message if dont save before closing
                DialogResult result = MessageBox.Show("There are unsaved changes." +
                    " Save your changes before closing?", "Message",
                    MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);

                // if yes, sabe the file
                if (result == DialogResult.Yes)
                {
                    saveToolStripMenuItem.PerformClick();
                }
                // if no, close the file
                if (result == DialogResult.No)
                {
                    Close();
                }
                // if cancle, do nothing
                if (result == DialogResult.Cancel)
                    return;
            }
        }

        /// <summary>
        /// set and dispaly the value to the sheet
        /// </summary>
        private void Display()
        {
            int RowDisplay;
            int ColDisplay;
            // foreach of the cell in the sheet
            foreach (string cellName in sheet.GetNamesOfAllNonemptyCells())
            {
                // convert to the cooresponding coordinate
                NametoCoor(out ColDisplay, out RowDisplay, cellName);
                // the set and dispaly the value on the sheet
                spreadsheetPanel1.SetValue(ColDisplay, RowDisplay, PrintableValue(sheet.GetCellValue(cellName)).ToString());
            }
        }

        /// <summary>
        /// change cell name to coordinate
        /// </summary>
        /// <param name="c">colum</param>
        /// <param name="r">row</param>
        /// <param name="cellname"></param>
        private void NametoCoor(out int c, out int r, string cellname)
        {
            c = (int)cellname[0] - 65;
            int.TryParse(cellname.Substring(1), out r);
            r--;
        }

        /// <summary>
        /// returns a string that's converted from the colum and the row of the cell
        /// </summary>
        /// <param name="c">colum</param>
        /// <param name="r">row</param>
        /// <returns></returns>
        private string ColRowtoString(int c, int r)
        {
            return new string((char)(c + 65), 1) + (r + 1).ToString();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            IsPanleFocused = false;
        }


        /// <summary>
        /// press enter to change value
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EnterBotton_Click(object sender, EventArgs e)
        {
            // calls the Updatecell function
            Updatecell();
        }
        /// <summary>
        /// update the cells with thier value
        /// </summary>
        private void Updatecell()
        {
            try
            {
                ISet<string> list = sheet.SetContentsOfCell(ColRowtoString(col, row), userInput.Text);
                // get the value of the named cell
                string value = sheet.GetCellValue(ColRowtoString(col, row)).ToString();
                // see if it's string or  double or fomula 
                string content = PrintableContents(sheet.GetCellContents(ColRowtoString(col, row)));
                // set the cell value to the coordinate
                spreadsheetPanel1.SetValue(col, row, value);
                this.spreadsheetPanel1.Select();
                IsPanleFocused = true;
                Value_textBox.Text = value;
                //for each string in the list,
                foreach (string s in list)
                {
                    // convert the cell name to the coordinat
                    NametoCoor(out col, out row, s);
                    // set the cell value as the specific cell value
                    spreadsheetPanel1.SetValue(col, row, sheet.GetCellValue(s).ToString());
                }
            }
            catch (CircularException)
            {
                //showing message if there is Circular dependency occurs
                MessageBox.Show("Circular dependency.", "Message"); return;
            }

            catch
            {
                Value_textBox.Text = "Eval Error"; return;
            }

        }

        /// <summary>
        /// this function helps to selecte sheet cell by using arrowkeys on the ketboard
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="keyData"></param>
        /// <returns></returns>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // if the panel is focused
            if (IsPanleFocused)
            {
                // switch in below different cases
                switch (keyData)
                {
                    // if the pressed key is down, row + 1, and update to spreadsheetPanel1
                    case Keys.Down:
                        {
                            spreadsheetPanel1.SetSelection(col, ++row);
                            this.spreadsheetPanel1_SelectionChanged(spreadsheetPanel1);
                            break;
                        }
                    // if the pressed key is up, row + 1, and update to spreadsheetPanel1
                    case Keys.Up:
                        {
                            spreadsheetPanel1.SetSelection(col, --row);
                            this.spreadsheetPanel1_SelectionChanged(spreadsheetPanel1);
                            break;
                        }
                    // if the pressed key is left, colum - 1, and update to spreadsheetPanel1
                    case Keys.Left:
                        {
                            spreadsheetPanel1.SetSelection(--col, row);
                            this.spreadsheetPanel1_SelectionChanged(spreadsheetPanel1);
                            break;
                        }
                    // if the pressed key is right, colum + 1, and update to spreadsheetPanel1
                    case Keys.Right:
                        {
                            spreadsheetPanel1.SetSelection(++col, row);
                            this.spreadsheetPanel1_SelectionChanged(spreadsheetPanel1);
                            break;
                        }
                    // defaukt case
                    default:
                        IsPanleFocused = false;
                        break;
                }
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }



        /// <summary>
        /// returns a string. It looks up to the cell value and convert to string
        /// </summary>
        /// <param name="CellValue"></param>
        /// <returns></returns>
        private string PrintableValue(object CellValue)
        {
            if (CellValue is string)
            {
                return CellValue as string;
            }
            else if (CellValue is double)
            {
                return "" + CellValue;
            }
            else
            {
                FormulaError toReturn = (FormulaError)CellValue;
                return toReturn.Reason;
            }

        }
        /// <summary>
        /// Checks if the contens is printable. If yes, return the contents
        /// </summary>
        /// <param name="CellContents"></param>
        /// <returns></returns>
        private string PrintableContents(object CellContents)
        {
            if (CellContents is string)
            {
                return CellContents as string;
            }
            else if (CellContents is double)
            {
                return "" + CellContents;
            }
            else
                return "=" + CellContents.ToString();

        }


        /// <summary>
        /// when the guife botton is hit, a message box is showing and stated the idea of this application
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void guideToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                "General Guide: \nSelecting one cell, type in the formula textbox with '=' mark, hit enterkey" +
                "\nor clicking the enter botton to set a cellcontent with its cellname and cellvalue showing on the textbox.\n\n" +
                "Special Features: \nPressing arrow key and move selection of a cell in the spreadsheet.\n\n" +
                "Condition checker: \nIf saving the opening file when creat a new file/open an existing file.",
                "Help"
            );
        }

        /// <summary>
        /// reads filename without extentision
        /// </summary>
        private void getFileName()
        {
            char[] path = filePath.ToCharArray();
            int i = path.Length - 1;
            char currLetter = path[i];
            while (currLetter != '/' && currLetter != '\\')
            {
                docName = docName + currLetter;
                currLetter = path[--i];
            }
            docName.Reverse();

        }

       /// <summary>
       /// a message box will show up when clicking the about botton in the menu strip
       /// </summary>
       /// <param name="sender"></param>
       /// <param name="e"></param>
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Spread Sheet Written By Linxi Li and Rong Xiao.\nLast Updated 11/2/2016" +
                "\nVersion ps6\nSpreadsheerPanel provided by CS3500 UofU", "About"

            );
        }

        
        /// <summary>
        /// if the return key is pressed, the cell in the sheet also get updated
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void userInput_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                Updatecell();
            }
        }

        /// <summary>
        /// the funtion that react when the sheet's shelected cell is changed
        /// </summary>
        /// <param name="sender"></param>
        private void spreadsheetPanel1_SelectionChanged(SpreadsheetPanel sender)
        {
            string value;
            // Get the selection's colum and row
            spreadsheetPanel1.GetSelection(out col, out row);
            // show the coordination on the coordi textbox
            Coordi_textBox.Text = this.ColRowtoString(col, row);
            // show the contents in the inputbox when selecting one of the changed cell
            userInput.Text = this.PrintableContents(sheet.GetCellContents(Coordi_textBox.Text));

            //need another version of printable value that does contents.
            spreadsheetPanel1.GetValue(col, row, out value);
            Value_textBox.Text = value;
            IsPanleFocused = true;
        }


        private void progressBar1_Click(object sender, EventArgs e)
        {
            IsPanleFocused = false;
        }

        private void CancelBotton_Click(object sender, EventArgs e)
        {

        }

        private void Coordination_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void coordi_Label_Click(object sender, EventArgs e)
        {
            IsPanleFocused = false;
        }

        private void spreadsheetPanel1_Load(object sender, EventArgs e)
        {

        }


        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IsPanleFocused = false;
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            IsPanleFocused = false;
        }

        private void ValueLable_Click(object sender, EventArgs e)
        {
            IsPanleFocused = false;
        }

        private void label2_Click(object sender, EventArgs e)
        {
            IsPanleFocused = false;
        }

        private void Coordination_Click_1(object sender, EventArgs e)
        {
            IsPanleFocused = false;
        }

        private void fileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IsPanleFocused = false;
        }

        private void EnterLable_Click(object sender, EventArgs e)
        {
            IsPanleFocused = false;
        }

        private void userInput_Click(object sender, EventArgs e)
        {
            IsPanleFocused = false;
        }

        private void usernameBox_TextChanged(object sender, EventArgs e)
        {

        }

        private void ipBox_TextChanged(object sender, EventArgs e)
        {

        }

        private void connectButton_Click(object sender, EventArgs e)
        {
            bool connect = false;
            if (ipBox.Text.Length < 5)
            {
                ipBox.Text = "Invalid IP address";
                connect = false;
            }
            if (usernameBox.Text.Length < 1)
            {
                usernameBox.Text = "Username cannot be blank";
                connect = false;
            }
            connect = true;
            if (connect)
            {
                Socket s = SpreadsheetNetworking.ConnectToServer(ipBox.Text, 2112, null);
                //SpreadsheetNetworking.Send(s, "Hello, world", 1);
            }
            //MessageBox.Show(ipBox.Text);
        }


        /*
         * SPREADSHEET NETWORK EVENTS
         * 
         * 
         * */

    }
}
