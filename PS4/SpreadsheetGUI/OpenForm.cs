using ClientNetworking;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SpreadsheetGUI
{
    public partial class OpenForm : Form
    {
        // Needs a reference to the socket for sending messages
        public Socket clientSocket;
        // Needs a reference to the filelist of the clients
        public List<string> files;
        int Opcode = -1;
        string DocID;

        /// <summary>
        /// Opens a form that displays the available files to the user,
        /// takes the socket to send across and the filelist
        /// </summary>
        /// <param name="fileList"></param>
        /// <param name="client"></param>
        public OpenForm(List<string> fileList, Socket client, int opCode, string docid)
        {
            clientSocket = client;
            files = fileList;
            InitializeComponent();
            Opcode = opCode;
            DocID = docid;
            fileNameBox.Text = "(filename)";
            if (fileList.Count > 0)
            {
                //fileListBox.Enabled = false;
                foreach (string file in fileList)
                {
                    fileListBox.Items.Add(file);
                }
            }
            fileListBox.Click += new EventHandler(fileListBox_Click);
            fileListBox.DoubleClick += new EventHandler(fileListBox_DoubleClick);
        }

        /// <summary>
        /// Allow user to enter name
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void fileNameBox_TextChanged(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// Send a request to the server
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            string name;
            if (fileNameBox.Text.Length < 1)
            {
                MessageBox.Show("Please enter a filename");
                return;
            }
            if (files.Contains(fileNameBox.Text))
            {
                name = fileNameBox.Text;
                if (name.Length > 2)
                {
                    if (name.Substring(name.Length - 3, 3) == ".ss")
                    {
                        name = name.Substring(0, name.Length - 3);
                        if (name.Length > 0)
                        {
                            SpreadsheetNetworking.Send(clientSocket, name, Opcode);
                            Close();
                        }
                        else
                        {
                            MessageBox.Show("Please enter a valid filename");
                        }
                    }
                    else
                    {
                        SpreadsheetNetworking.Send(clientSocket, name, Opcode);
                        Close();
                    }
                }
                //SpreadsheetNetworking.Send(clientSocket, DocID, 6);
                //SpreadsheetNetworking.Send(clientSocket, fileNameBox.Text, Opcode);
            }
            else if (Opcode == 7)
            {
                name = fileNameBox.Text;
                if (name.Length > 2)
                {
                    if (name.Substring(name.Length - 3, 3) == ".ss")
                    {
                        name = name.Substring(0, name.Length - 3);
                        if (name.Length > 0)
                        {
                            SpreadsheetNetworking.Send(clientSocket, DocID + "\t" + name, Opcode);
                            SpreadsheetNetworking.Send(clientSocket, DocID, 6);
                            Close();
                        }
                        else
                        {
                            MessageBox.Show("Please enter a valid filename");
                        }
                    }
                    else
                    {
                        SpreadsheetNetworking.Send(clientSocket, DocID + "\t" + name, Opcode);
                        SpreadsheetNetworking.Send(clientSocket, DocID, 6);
                        Close();
                    }
                }
            }
            else
            {
                //SpreadsheetNetworking.Send(clientSocket, DocID, 6);
                name = fileNameBox.Text;
                if (name.Length > 2)
                {
                    if (name.Substring(name.Length - 3, 3) == ".ss")
                    {
                        name = name.Substring(0, name.Length - 3);
                        if (name.Length > 0)
                        {
                            SpreadsheetNetworking.Send(clientSocket, name, Opcode);
                            Close();
                        }
                        else
                        {
                            MessageBox.Show("Please enter a valid filename");
                        }
                    }
                    else
                    {
                        SpreadsheetNetworking.Send(clientSocket, name, Opcode);
                        Close();
                    }
                }
            }
        }


        /// <summary>
        /// Fills text box with the filename
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void fileListBox_Click(object sender, EventArgs e)
        {
            if (fileListBox.SelectedItem != null)
            {
                fileNameBox.Text = fileListBox.SelectedItem.ToString();
            }
        }

        /// <summary>
        /// Sends a request to the server
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void fileListBox_DoubleClick(object sender, EventArgs e)
        {
            string name;
            if (fileListBox.SelectedItem != null)
            {
                fileNameBox.Text = fileListBox.SelectedItem.ToString();
                //SpreadsheetNetworking.Send(clientSocket, DocID, 6);
                name = fileNameBox.Text;
                if (name.Length > 2)
                {
                    if (name.Substring(name.Length - 3, 3) == ".ss")
                    {
                        name = name.Substring(0, name.Length - 3);
                        if (name.Length > 0)
                        {
                            SpreadsheetNetworking.Send(clientSocket, name, Opcode);
                            Close();
                        }
                        else
                        {
                            MessageBox.Show("Please enter a valid filename");
                        }
                    }
                    else
                    {
                        SpreadsheetNetworking.Send(clientSocket, name, Opcode);
                        Close();
                    }
                }
            }
        }
    }
}


