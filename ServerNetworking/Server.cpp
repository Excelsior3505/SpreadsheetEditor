//Alex Koumandarakis
//CS 3505
//Final Project
//Server.cpp

#include <iostream>
#include <string>
#include <boost/bind.hpp>
#include <boost/asio.hpp>
#include <boost/shared_ptr.hpp>
#include <boost/enable_shared_from_this.hpp>
#include "Server.h"
#include "ClientConnection.h"

using boost::asio::ip::tcp;

typedef boost::shared_ptr<ClientConnection> client_ptr;

//Default constructor for the server
Server::Server(boost::asio::io_service& io_service_, const boost::asio::ip::tcp::endpoint& endP)
  : io_serv(io_service_), acceptor(io_service_, endP), server_socket(io_service_)
{
  nextID = 0;
  
  //Begin awaiting clients to connect
  await_client();
}

void Server::await_client()
{
  std::cout << "Awaiting new client" << std::endl;

  //Create new socket to place any new connections
  ClientConnection::cc_ptr new_cc = ClientConnection::create(io_serv, nextID); 
  nextID++;

  //Start asyncronously accepting new clients
  //Bind new_client_handler to be called when new client connects
  acceptor.async_accept(new_cc->get_socket(), boost::bind(&Server::new_client_handler, this, new_cc, boost::asio::placeholders::error));
}

void Server::new_client_handler(client_ptr new_cc, const boost::system::error_code& error)
{
  std::cout << "New client connected, ID: " << nextID-1 << std::endl;

  //If no error ocurred, add the new client to the list of clients
  if (!error)
    {
      //NEW CLIENT
      clients.push_back(new_cc);
      clientID_toDocID.push_back(-1);
      new_cc->send("Hello");
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
	  clients[i]->send(message);
	}
    }
  //If -1 is only given for clientID, send to all clients working on specified doc
  else if (clientID == -1 && docID > -1)
    {
      for (int i = 0; i < clients.size(); i++)
	{
	  if (clientID_toDocID[i] == docID)
	    {
	      clients[i]->send(message);
	    }
	}
    }
  //If a specific client is given and no doc is given, send only to that client
  else if (clientID > -1 && docID == -1)
    {
      clients[clientID]->send(message);
    }
} 

//Check the client connections for incoming messages
void Server::check_for_messages()
{
  for (int i = 0; i < clients.size(); i++)
    {
      if (!clients[i]->incoming_message_queue.empty())
	{
	  received_messages.push(clients[i]->incoming_message_queue.front());
	  clients[i]->incoming_message_queue.pop();
	}
    }

  processMessages();
  check_for_messages();
}

//Process all received messages
void Server::processMessages()
{
  while (!received_messages.empty())
    {
      std::string messageToProcess = received_messages.front();
      received_messages.pop();

      //TODO: process the various kinds of messages
      std::cout << messageToProcess << std::endl;
    }
}
