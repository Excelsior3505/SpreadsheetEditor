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
using System.Net;
using System.Diagnostics;


/* TODO:
 *      - Save sending and receiving
 *      - Open/New Receiving functionality
 *      - Rename Sending/Receiving
 *      - Pretty much all the Receiving functionality
 *  
 * */


namespace SpreadsheetGUI
{
    // Original code by
    // Sharon Xiao
    // u0943650

    // Linxi Li
    // u1016104


    /* Repurposed by Charlie Clausen, u0972939
     * */
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

        /*
         * Network Defaults
         * 
         * 
         * */
        private Socket ClientSocket;
        public int ID;
        public const int DefaultPort = 2112;
        private bool CurrentlyConnected = false;
        private string DocID = "-1";
        string FileName = "";
        string UserName = "";
        private List<string> AvailableFiles = new List<string>();
        //Timer timer = new Timer()
        private static object thisLock = new object();


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
            if (CurrentlyConnected && ClientSocket.Connected)
            {
                try
                {
                    SpreadsheetNetworking.Send(ClientSocket, "", 0);
                }
                catch (Exception)
                {
                    MessageBox.Show("You do not appear to be connected to the server");
                    AllowReconnect();
                }
            }
            else
            {
                MessageBox.Show("You do not appear to be connected to the server");
                AllowReconnect();
            }
            System.Threading.Thread.Sleep(350);
            OpenForm fileForm = new OpenForm(AvailableFiles, ClientSocket, 1, DocID);
            fileForm.Show();
        }

        /// <summary>
        /// If click the openToolStripMenuItem, do the following reaction
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (CurrentlyConnected && ClientSocket.Connected)
            {
                try
                {
                    SpreadsheetNetworking.Send(ClientSocket, "", 0);
                }
                catch (Exception)
                {
                    MessageBox.Show("You do not appear to be connected to the server");
                    AllowReconnect();
                }
            }
            else
            {
                MessageBox.Show("You do not appear to be connected to the server");
                AllowReconnect();
            }

            System.Threading.Thread.Sleep(2000);
            if (AvailableFiles.Count > 0)
            {
                OpenForm fileForm = new OpenForm(AvailableFiles, ClientSocket, 2, DocID);
                fileForm.Show();
            }
            else
            {
                MessageBox.Show("No available files");
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
            if (CurrentlyConnected && ClientSocket.Connected)
            {
                try
                {
                    SpreadsheetNetworking.Send(ClientSocket, DocID, 6);
                }
                catch (Exception)
                {
                    MessageBox.Show("You appear to have lost connection with the server");
                }
            }

        }



        /// <summary>
        ///  The Save As menu botton to save the sheet to a targeting location with a name.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (CurrentlyConnected && ClientSocket.Connected)
            {
                try
                {
                    SpreadsheetNetworking.Send(ClientSocket, DocID, 6);
                }
                catch (Exception)
                {
                    MessageBox.Show("You appear to have lost connection with the server");
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
            // if there is no change in the sheet, close the sheet directely, send server the proper code
            saveToolStripMenuItem_Click(sender, e);
            SendCloseToServer();
            Close();

        }


        /// <summary>
        /// Used to send the closing message to the server
        /// </summary>
        private void SendCloseToServer()
        {
            if (CurrentlyConnected && ClientSocket.Connected)
            {
                try
                {
                    SpreadsheetNetworking.Send(ClientSocket, DocID, 9);
                }
                catch (Exception e)
                {
                    MessageBox.Show("You appear to have lost connection with the server");
                }
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
        /// press enter to send an edit request to the server
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EnterBotton_Click(object sender, EventArgs e)
        {
            // calls the Updatecell function
            //Updatecell();
            if (CurrentlyConnected && ClientSocket.Connected)
            {
                try
                {
                    // Create the packet of form "3\DocID\tCellName\tnewContents\n
                    string message = DocID + "\t" + ColRowtoString(col, row) + "\t" + userInput.Text;
                    SpreadsheetNetworking.Send(ClientSocket, message, 3);
                }
                catch (Exception)
                {
                    MessageBox.Show("Could not update cell: " + ColRowtoString(col, row));
                    AllowReconnect();
                }
            }
            else
            {
                ipBox.Enabled = true;
                usernameBox.Enabled = true;
                CurrentlyConnected = false;
            }
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
                if (CurrentlyConnected && ClientSocket.Connected)
                {
                    try
                    {
                        // Create the packet of form "3\DocID\tCellName\tnewContents\n
                        string message = DocID + "\t" + ColRowtoString(col, row) + "\t" + userInput.Text;
                        SpreadsheetNetworking.Send(ClientSocket, message, 3);
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Could not update cell: " + ColRowtoString(col, row));
                        AllowReconnect();
                    }
                }
                else
                {
                    ipBox.Enabled = true;
                    usernameBox.Enabled = true;
                    CurrentlyConnected = false;
                }
                //Updatecell();
            }
        }



        /// <summary>
        /// the funtion that react when the sheet's selected cell is changed, sends message to server
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

            // Sends a message to let the server know it is editing
            SendCellNameToServer();


            //need another version of printable value that does contents.
            spreadsheetPanel1.GetValue(col, row, out value);
            Value_textBox.Text = value;
            IsPanleFocused = true;
        }

        private void SendCellNameToServer()
        {
            if (CurrentlyConnected && ClientSocket.Connected)
            {
                try
                {
                    string message = DocID + "\t" + ColRowtoString(col, row);
                    SpreadsheetNetworking.Send(ClientSocket, message, 8);
                }
                catch (Exception)
                {
                    MessageBox.Show("Could not send request to the server, please attempt to reconnect");
                    AllowReconnect();
                }
            }
            else
            {
                ipBox.Enabled = true;
                usernameBox.Enabled = true;
                CurrentlyConnected = false;
            }
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
        /*
 * SPREADSHEET NETWORK EVENTS
 * 
 * 
 * */



        /*
         * This holds the username to be sent to the server
         * */
        private void usernameBox_TextChanged(object sender, EventArgs e)
        {
            
        }

        /*
         * This takes the ip address to be connected to
         * */
        private void ipBox_TextChanged(object sender, EventArgs e)
        {
            
        }


        /*
         * Attempts an asyncronous connection attempt using the IP and username given in the 
         * ipBox and userName text boxes
         * */
        private void connectButton_Click(object sender, EventArgs e)
        {
            // Client already is connected to server, clicking again will open another instance on the server
            
            if (CurrentlyConnected)
            {               
                return;
            }
            
            // Simple checks for input, could be improved upon later
            // Could also be a popup
            bool connect = true;
            int timer = 0;
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
            else if (usernameBox.Text.Length >= 25)
            {
                usernameBox.Text = "Username too long";
                connect = false;
            }
            // Attempt to create a socket with the server
            if (connect)
            {
                    try
                    {
                        ipBox.Enabled = false;
                        usernameBox.Enabled = false;
                        // Make a socket object to represent the connection, uses the IP passed in and default port
                        ClientSocket = SpreadsheetNetworking.ConnectToServer(ipBox.Text, DefaultPort, Startup);
                    }
                    catch (Exception)
                    {
                        if (!ClientSocket.Connected)
                        {
                            MessageBox.Show("Failed to connect to server, please check IP and try again");
                            AllowReconnect();
                        }
                }
            }
        }


        /// <summary>
        /// Re-enables all functionality in case of network drop
        /// </summary>
        private void AllowReconnect()
        {
            ipBox.Enabled = true;
            usernameBox.Enabled = true;
            CurrentlyConnected = false;
        }



        /// <summary>
        /// Startup callback when first connecting to the server
        /// </summary>
        /// <param name="state"></param>
        private void Startup(SpreadsheetNetworking.SocketState state)
        {
            if (!state.socket.Connected)
            {
                ipBox.Enabled = true;
                usernameBox.Enabled = true;
                CurrentlyConnected = false;
                MessageBox.Show("Connection with server has failed.  Please check the IP address and try again");
                return;
            }
            CurrentlyConnected = true;
            // Break up the message ending at the terminator
            string clientID = state.sb.ToString();
            // This needs to be tested, I don't know if the startup ID message will use the terminator
            string[] wholeMessage = Regex.Split(clientID, @"(?<=[\n])");
            // Extract the ID
            foreach(string s in wholeMessage)
            {
                if (s.Length == 0)
                {
                    continue;
                }
                if (Int32.TryParse(s, out ID))
                {
                    state.ID = ID;
                    state.sb.Remove(0, s.Length);
                    break;
                }
                // In case no terminator was received
                if (s[s.Length - 1] != '\n')
                {
                    break;
                }
                state.sb.Remove(0, s.Length);
            }

            // Set the callback to start looking for messages of any kind
            state.EventProcessor = ReceiveDataFromServer;
            if (state.ID != -1)
            {
                try
                {
                    MessageBox.Show("Successfully connected, your ID is: " + state.ID);
                    // Don't know what format code to send, so sending -1
                    SpreadsheetNetworking.Send(state.socket, usernameBox.Text, -1);
                }
                catch(SocketException e)
                {
                    MessageBox.Show("There was an error sending to the server");
                    MessageBox.Show(e.Message.ToString());
                    MessageBox.Show("Is the socket connected?  " + state.socket.Connected);
                }
                // Starts an asyncronous listen for data coming from the server
                SpreadsheetNetworking.GetData(state);
            }
            else
            {
                MessageBox.Show("Could not receive ID from server");
            }
        }


        /// <summary>
        /// Callback function that will be called whenever data is received from the server
        /// </summary>
        /// <param name="state"></param>
        private void ReceiveDataFromServer(SpreadsheetNetworking.SocketState state)
        {
            // Get the data
            string data;
            lock (state.sb)
            {
                data = state.sb.ToString();
                //MessageBox.Show("Data: " + data);
                state.sb.Remove(0, data.Length);
                // Split at newline
            }
            // Split messages at the newline and then break that up by tabs

            string[] splitOne = Regex.Split(data, @"(?<=[\n])");
            //MessageBox.Show("Message length: " + splitOne.Length.ToString());   
            //System.Threading.Thread.Sleep(100); 
            foreach (string message in splitOne)
            {
                if (message == "\n" || message.Length < 1 || message == "\t")
                {
                    continue;
                }
                string[] splitData = message.Split('\t');
                //MessageBox.Show(message);
                System.Threading.Thread.Sleep(25);
                    //splitData[splitData.Length - 1] = splitData[splitData.Length - 1].Substring(0, splitData.Length - 1);
                    /*
                    lock (state.sb)
                    {
                        // Prevent buffer overflow
                        state.sb.Remove(0, splitOne[i].Length);
                    }
                    */
                    // Check the op-codes
                switch (splitData[0])
                {
                    case "0":
                        ReceiveFileNames(splitData);
                        break;
                    case "1":
                        ReceiveNewID(splitData);
                        break;
                    case "2":
                        ReceiveValidOpen(splitData, state);
                        break;
                    case "3":
                        try
                        {
                            int c, r;
                            NametoCoor(out c, out r, splitData[2]);
                            string cellname = splitData[2];
                            string cellvalue = splitData[3].Substring(0, splitData[3].Length - 1);
                            ISet<string> list = sheet.SetContentsOfCell(cellname, cellvalue);
                            ISet<string> copy = list;

                                //ReceiveCellUpdate(splitData, state);
                            Debug.WriteLine("Setting: " + cellname + "  " + cellvalue);
                                // get the value of the named cell
                            string value = sheet.GetCellValue(cellname).ToString();
                                // see if it's string or  double or fomula 
                            string content = PrintableContents(sheet.GetCellContents(cellname));

                                // set the cell value to the coordinate
                                //lock (spreadsheetPanel1)
                                //{
                            spreadsheetPanel1.SetValue(c, r, value);
                                //}

                            for (int i = 0; i < copy.Count; i++)
                            {
                                string s = copy.ElementAt(i);
                                int co, ro;
                                    // convert the cell name to the coordinat
                                NametoCoor(out co, out ro, s);
                                    // set the cell value as the specific cell value
                                    //lock (spreadsheetPanel1)
                                    //{
                               spreadsheetPanel1.SetValue(co, ro, sheet.GetCellValue(s).ToString());
                                    //}
                            }
                                //this.spreadsheetPanel1.Select();
                                IsPanleFocused = true;
                                //Value_textBox.Text = value;
                                //for each string in the list,
                            }
                            catch (Exception s)
                            {
                            MessageBox.Show(s.Message);
                              //  MessageBox.Show("This is an invalid cell name");
                            }
                            break;
                        case "4":
                            ReceiveValidEdit(splitData);
                            break;
                        case "5":
                            ReceiveInvalidEdit(splitData);
                            break;
                        case "6":
                            ReceieveRename(splitData);
                            break;
                        case "7":
                            ReceiveSave(splitData);
                            break;
                        case "8":
                            ReceiveValidRename(splitData);
                            break;
                        case "9":
                            ReceiveInvalidRename(splitData);
                            break;
                        case "A":
                            ReceiveEditLocation(splitData);
                            break;
                        default:
                            break;
                    }
                }
            SpreadsheetNetworking.GetData(state);
        }

        private void ReceiveEditLocation(string[] splitData)
        {
            string data = "";

            //MessageBox.Show("Received packet: " + data);
        }


        /// <summary>
        /// Received a packet that the rename attempt was unsuccessful
        /// </summary>
        /// <param name="splitData"></param>
        private void ReceiveInvalidRename(string[] splitData)
        {
            //DocID = GetDocID(splitData);
            MessageBox.Show("The spreadsheet rename attempt was unsuccessful");
        }


        /// <summary>
        /// Received a packet that the rename attempt was successful
        /// </summary>
        /// <param name="splitData"></param>
        private void ReceiveValidRename(string[] splitData)
        {
            //DocID = GetDocID(splitData);
            if (DocID != "-1")
            {
                MessageBox.Show("Spreadsheet successfully renamed");
            }
        }


        /// <summary>
        /// Confirmation that the spreadsheet was saved
        /// </summary>
        /// <param name="splitData"></param>
        private void ReceiveSave(string[] splitData)
        {
            DocID = GetDocID(splitData);
            MessageBox.Show("Saved!");
        }


        /// <summary>
        /// When a rename is valid, rename the file
        /// </summary>
        /// <param name="splitData"></param>
        private void ReceieveRename(string[] splitData)
        {
            
        }


        /// <summary>
        /// Edit caused an error (circular dependency)
        /// </summary>
        /// <param name="splitData"></param>
        private void ReceiveInvalidEdit(string[] splitData)
        {
            //DocID = GetDocID(splitData);
            MessageBox.Show("This caused an error on the server, please change the value");
        }


        /// <summary>
        /// Valid edit
        /// </summary>
        /// <param name="splitData"></param>
        private void ReceiveValidEdit(string[] splitData)
        {
            //DocID = GetDocID(splitData);
        }


        /// <summary>
        /// Updates the actual value of the cell
        /// </summary>
        /// <param name="splitData"></param>
        private void ReceiveCellUpdate(string[] splitData, SpreadsheetNetworking.SocketState state)
        {
            Debug.WriteLine("Adding: " + splitData[2] + "  " + splitData[3]);
            sheet.SetContentsOfCell(splitData[2], splitData[3]);
            Updatecell();
            Display();
            //SpreadsheetNetworking.GetData(state);
        }


        /// <summary>
        /// Receive the DocID from the server when opening an existing spreadsheet
        /// </summary>
        /// <param name="splitData"></param>
        private void ReceiveValidOpen(string[] splitData, SpreadsheetNetworking.SocketState state)
        {
            DocID = splitData[1];
            DocID = DocID.Substring(0, DocID.Length - 1);
            // Add code to clear spreadsheet
            //ClearSpreadsheet();
            //System.Threading.Thread.Sleep(5000);
            //spreadsheetPanel1.Clear();
            ClearSpreadsheet();
            System.Threading.Thread.Sleep(100);
        }


        /// <summary>
        /// Receive the DocID from the server when opening up a new spreadsheet
        /// </summary>
        /// <param name="splitData"></param>
        private void ReceiveNewID(string[] splitData)
        {
            DocID = splitData[1];
            DocID = DocID.Substring(0, DocID.Length - 1);
            // Add code to clear spreadsheet
            ClearSpreadsheet();
            //spreadsheetPanel1.Clear();
            System.Threading.Thread.Sleep(100);
        }


        /// <summary>
        /// Helper function to extract DocID from a packet
        /// </summary>
        /// <param name="splitData"></param>
        /// <returns></returns>
        private string GetDocID(string[] splitData)
        {
            string id = "";
            id = splitData[1];
            id = DocID.Substring(0, DocID.Length - 1);
            return id;
        }

        /// <summary>
        /// Takes the split up packet data and save the data to the AvailableFiles list
        /// </summary>
        /// <param name="splitData"></param>
        private void ReceiveFileNames(string[] splitData)
        {
            int temp;
            lock (AvailableFiles)
            {
                foreach (string s in splitData)
                {
                    if (!Int32.TryParse(s, out temp) && s != "\n" && s != "   " && AvailableFiles.Contains(s) == false)
                    {
                        AvailableFiles.Add(s);
                    }
                }
            }
        }


        /// <summary>
        /// Sends an undo request to the server when clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (CurrentlyConnected && ClientSocket.Connected)
            {
                try
                {
                    SpreadsheetNetworking.Send(ClientSocket, DocID, 4);
                }
                catch(Exception)
                {
                    MessageBox.Show("Failed to send undo request to server, check connection and try again");
                }
            }
        }


        /// <summary>
        /// Sends a Redo request to the server
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (CurrentlyConnected && ClientSocket.Connected)
            {
                try
                {
                    SpreadsheetNetworking.Send(ClientSocket, DocID, 5);
                }
                catch (Exception)
                {
                    MessageBox.Show("Failed to send undo request to server, check connection and try again");
                }
            }
        }

        /// <summary>
        /// Send a rename request to the server
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void renameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (DocID != "-1" && ClientSocket.Connected)
            {
                
                try
                {
                    OpenForm fileForm = new OpenForm(AvailableFiles, ClientSocket, 7, DocID);
                    fileForm.Show();
                }
                catch (Exception)
                {
                    MessageBox.Show("Please reconnect to server");
                }
            }
            else
            {
                MessageBox.Show("You must first open a document");
            }
        }


        /// <summary>
        /// Clears all cells in the spreadsheet
        /// </summary>
        private void ClearSpreadsheet()
        {
            IEnumerable<string> cells = sheet.GetNamesOfAllNonemptyCells();
            IEnumerable<string> copy = cells;
            foreach (string s in copy)
            {
                sheet.SetContentsOfCell(s, "");
                NametoCoor(out col, out row, s);
                spreadsheetPanel1.SetValue(col, row, "");
                IsPanleFocused = true;
            }
            sheet = new Spreadsheet();
            
           
        }
    }
}
