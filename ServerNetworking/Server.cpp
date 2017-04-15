//Alex Koumandarakis
//CS 3505
//Final Project
//Server.cpp

#include <iostream>
#include <string>
#include <boost/bind.hpp>
//#include <boost/shared_ptr.hpp>
//#include <boost/enable_shared_from_this.hpp>
#include <boost/asio.hpp>
#include <thread>
#include "Server.h"
#include "SocketState.h"

using boost::asio::ip::tcp;


//Default constructor for the server
Server::Server()
{
  //Create io_service
  boost::asio::io_service io_serv;

  //initialize the object that will listen and accept tcp connections
  acceptor(io_serv, tcp::enpoint(tcp::v4(), 2112));

  //Begin awaiting clients to connect
  await_client();
  
  nextId = 0;
}

//Copy constructor
Server::Server(const Server & other)
{
  server_socket = other.server_socket;
  acceptor = other.acceptor;
  clients = other.clients;
}

void Server::await_client()
{
  //Create new socket state to place any new connections
  SocketState new_client_SS(tcp::socket(acceptor.get_io_service()), nextID);
  nextID++;

  //Start asyncronously accepting new clients
  //Bind new_client_handler to be called when new client connects
  acceptor.async_accept(new_Client_SS->socket, boost::bind(&Server::new_client_handler, this, new_client_SS, boost::asio::placeholders::error));
}

void Server::new_client_handler(SocketState new_client_SS, const boost::system::error_code& error)
{
  //If no error ocurred, add the new client to the list of clients
  if (!error)
    {
      //NEW CLIENT
      new_client_SS.SocketConnected = true;
      ClientConnection new_cc(new_client_SS);
      clients.push_back(new_cc);
      clientID_toDocID.push_back(-1);
    }

  //Continue waiting for another client
  await_client();
}

void Server::send(int clientID, int docID, std::string message)
{
  //If -1 given, send to all clients
  if (clientID == -1 && docID == -1)
    {
      for (int i = 0; i < clients.size(); i++)
	{
	  clients[i].send(message);
	}
    }
  //If -1 is only given for clientID, send to all clients working on specified doc
  else if (clientID == -1 && docID > -1)
    {
      for (int i = 0; i < clients.size(); i++)
	{
	  if (clientID_toDocID[i] == docID)
	    {
	      clients[i].send(message);
	    }
	}
    }
  //If a specific client is given and no doc is given, send only to that client
  else if (clientID > -1 && docID == -1)
    {
      clients[clientID].send(message);
    }
} 

//Check the client connections for incoming messages
void Server::check_for_messages()
{
  for (int i = 0; i < clients.size(); i++)
    {
      if (!clients[i].incoming_message_queue.empty())
	{
	  received_messages.push_back(clients[i].incoming_message_queue.front());
	  clients[i].incoming_message_queue.pop_front();
	}
    }

  processMessages();
  check_for_messages();
}

//Process all received messages
void processMessages()
{
  while (!received_messages.empty())
    {
      std::string messageToProcess = received_messaged.front();
      received_messages.pop_front();

      //TODO: process the various kinds of messages
    }
}
