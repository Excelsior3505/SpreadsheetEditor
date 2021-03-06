﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using static ClientNetworking.SpreadsheetNetworking;

namespace ClientNetworking
{
    public delegate void CallbackFunction(SocketState state);


    public class SpreadsheetNetworking
    {

        // Default port as stated by the specs
        static int DEFAULT_PORT = 2112;

        /// <summary>
        /// This class holds all the necessary state to handle a client connection
        /// Note that all of its fields are public because we are using it like a "struct"
        /// It is a simple collection of fields
        /// </summary>
        public class SocketState
        {

            public Socket socket;
            public int ID;

            // Delegate callback function can be changed outside this class
            public CallbackFunction EventProcessor;

            // Delegate callback function can be changed outside this class, it will be called when a socked closed.
            public CallbackFunction DisconnectedProcessor;

            // This is the buffer where we will receive message data from the client
            public byte[] messageBuffer = new byte[1024];

            // This is a larger (growable) buffer, in case a single receive does not contain the full message.
            public StringBuilder sb = new StringBuilder();

            // Used to tell if a connection fails, default is true so we only worry about it when no socket can be made
            public bool SocketConnected;

            // Keep a static field to keep track of the total number of connected clients
            private static int nextID = 0;

            /// <summary>
            /// The ID will be automaticly assigned.
            /// </summary>
            /// <param name="s"></param>
            public SocketState(Socket s) : this(s, nextID++)
            {

            }

            /// <summary>
            /// Create a SocketState with socket and ID.
            /// </summary>
            /// <param name="s"></param>
            /// <param name="id"></param>
            public SocketState(Socket s, int id)
            {
                socket = s;
                ID = id;
                SocketConnected = true;
            }
        }


        /// <summary>
        /// Start attempting to connect to the server
        /// </summary>
        /// <param name="host_name"> server to connect to </param>
        /// <returns></returns>
        public static Socket ConnectToServer(string hostName, int port, CallbackFunction connectedCallback)
        {
            
            // Connect to a remote device.
            try
            {
                // Establish the remote endpoint for the socket.
                IPHostEntry ipHostInfo;
                IPAddress ipAddress = IPAddress.None;

                // Determine if the server address is a URL or an IP
                try
                {
                    ipHostInfo = Dns.GetHostEntry(hostName);
                    bool foundIPV4 = false;
                    foreach (IPAddress addr in ipHostInfo.AddressList)
                        if (addr.AddressFamily != AddressFamily.InterNetworkV6)
                        {
                            foundIPV4 = true;
                            ipAddress = addr;
                            break;
                        }
                    // Didn't find any IPV4 addresses
                    if (!foundIPV4)
                    {
                        Debug.WriteLine("Could not find IP");
                        return null;
                    }
                }
                catch (Exception e1)
                {
                    // see if host name is actually an ipaddress, i.e., 155.99.123.456
                    ipAddress = IPAddress.Parse(hostName);
                }


            // Create a TCP/IP socket.
                Socket theServer = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                theServer.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);
                SocketState serverState = new SocketState(theServer, -1);

                // store the action for when first conectting to the server.
                serverState.EventProcessor = connectedCallback;

                serverState.socket.BeginConnect(ipAddress, port, ConnectedToServer, serverState);
                //serverState.socket.Connect(ipAddress, 2112);
                return theServer;
            }
            catch (Exception e)
            {
                return null;
            }
            
        }


        /// <summary>
        /// This function is "called" by the operating system when the remote site acknowledges connect request
        /// </summary>
        /// <param name="ar"></param>
        public static void ConnectedToServer(IAsyncResult ar)
        {
            SocketState state = (SocketState)ar.AsyncState;
            try
            {
                // Complete the connection.
                state.socket.EndConnect(ar);
            }
            catch (Exception e)
            {
                Debug.WriteLine("Failed to end connection");
                // Connection failed with the server, or server is not running
                state.SocketConnected = false;
                return;
            }
            finally
            {
                // Run the last delegate event, so the application does not just crash when connection failed
                //state.EventProcessor(state);
            }

            // Connection successful, begin recieving some data
            state.socket.BeginReceive(state.messageBuffer, 0, state.messageBuffer.Length, SocketFlags.None, ReceiveCallback, state);
        }



        /// <summary>
        /// Convert the string to a byte array, append a NewLine, and send over the socket.
        /// NOTE:  This only appends the first \t char, any other seperators must be added
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="data">string to be sent.</param>
        /// <param name="formatCode"> integer identifier for packet type</param>
        public static void Send(Socket socket, String data, int formatCode)
        {
            // Don't send empty strings over the socket
            if (data.Equals("\n") || data.Equals(""))
            {
                return;
            }
            byte[] message = Encoding.UTF8.GetBytes(formatCode.ToString() + "\t" + data + "\n");

            socket.BeginSend(message, 0, message.Length, SocketFlags.None, SendCallback, socket);
        }


        /// <summary>
        /// This starts an event loop to continuously listen for messages from the server.
        /// </summary>
        /// <param name="state">The state representing the server connection</param>
        public static void GetData(SocketState state)
        {
            // Start listening for a message
            // When a message arrives, handle it on a new thread with ReceiveCallback
            state.socket.BeginReceive(state.messageBuffer, 0, state.messageBuffer.Length, SocketFlags.None, ReceiveCallback, state);

        }


        /// <summary>
        /// Normal callback used to continually recieve and process incoming data from the server
        /// </summary>
        /// <param name="ar"></param>
        public static void ReceiveCallback(IAsyncResult ar)
        {
            SocketState state = (SocketState)ar.AsyncState;
            Debug.WriteLine("State is now: " + state == null);
            // Try to stop recieving data, if fails call the provided
            // disconnect event delegate
            try
            {
                int bytesRead = state.socket.EndReceive(ar);
                if (bytesRead > 0)
                {
                    // Convert the bytes into a string
                    string theMessage = Encoding.UTF8.GetString(state.messageBuffer, 0, bytesRead);

                    // Append the received data to the growable buffer.
                    // It may be an incomplete message, so we need to start building it up piece by piece
                    lock (state.sb)
                    {
                        state.sb.Append(theMessage);
                    }
                    // This calls the delegate held in the SocketState class
                    //state.EventProcessor(state);


                    Debug.WriteLine(state.sb.ToString());
                }
            }
            catch (Exception)
            {
                // Don't just break, let the server handle disconnected clients
                //state.DisconnectedProcessor(state);
            }
        }


        /// <summary>
        /// This function "assists" the Send function.
        /// </summary>
        /// <param name="ar"></param>
        public static void SendCallback(IAsyncResult ar)
        {
            // Just end the message send
            Socket socket = (Socket)ar.AsyncState;
            socket.EndSend(ar);
        }
    }
}
