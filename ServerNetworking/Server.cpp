//Alex Koumandarakis
//CS 3505
//Final Project
//Server.cpp

#include <iostream>
#include <string>
#include <utility>
#include <boost/bind.hpp>
#include <boost/asio.hpp>
#include <boost/shared_ptr.hpp>
#include <boost/enable_shared_from_this.hpp>
#include <boost/filesystem.hpp>
#include <thread>
#include "Server.h"
#include "ClientConnection.h"

using boost::asio::ip::tcp;

typedef boost::shared_ptr<ClientConnection> client_ptr;

//Default constructor for the server
Server::Server(boost::asio::io_service& io_service_, const boost::asio::ip::tcp::endpoint& endP)
  : io_serv(io_service_), acceptor(io_service_, endP), server_socket(io_service_)
{
  nextID = 0;

  await_client();
}

void Server::await_client()
{
  std::cout << "Awaiting new client" << std::endl;

  //Create new socket to place any new connections
  ClientConnection::cc_ptr new_cc = ClientConnection::create(io_serv, nextID, this); 
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
      std::stringstream ss;
      ss << new_cc->connectionID;
      std::string clientID = ss.str();
      new_cc->send(clientID);
      new_cc->start_waiting_for_message();
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

//Process all received messages
void Server::processMessage(int clientID, std::string messageToProcess)
{
  //TODO: process the various kinds of messages
  std::cout << "Server received message: " << std::endl;
  std::cout << messageToProcess << std::endl;
  std::cout <<"From client: " << clientID << std::endl;

  received_messages.push(std::pair<int, std::string>(clientID, messageToProcess));

  std::char opCode = messageToProcess.at(0);
  std::string fileName = "";
  int docID = -1;

  if (messageToProcess == "Error")
    {
      clients[clientID] = NULL;
      return;
    }
  
  switch (opCode)
    {
    case '0':    //File List
      //Send list of filenames (from ../files/ folder)
      break;

    case '1':    //New
      //Extract file name from message
      //Check if name exists
      if(boost::filesystem::exists("../files/" + fileName))
	{
          //If it does, send list of filenames (from ../files/ folder)
	}
      else
	{
          //If it does not, send new docID
	  docID = spreadsheets.size();
	  spreadsheets.push_back(baseSS());
	}
      break;

    case '2':    //Open
      //Extract file name from message
      //Check if name exists
      if(boost::filesystem::exists("../files/" + fileName))
	{
	  int loc = 0;
	  if(spreadsheets.size() > 0)
	    {
	      bool found = false;
	      for(std::vector<base_ss>::iterator it = spreadsheets.begin(); it != spreadsheets.end(); it++)
		{
		  if(it->name == fileName)
		    {
		      docID = loc;
		      found = true;
		      break;
		    }
		  loc++;
		}
		if(!found)
		  {
		    docID = spreadsheets.size();
		    loadSpreadsheet("../files/" + fileName);
		  }
	    }
	  else
	    {
	      docID = 0;
	      loadSpreadsheet("../files/" + fileName);
	    }
          //If it does, send docID
	}
      else
	{
          //If it does not, send list of filenames (from ../files/ folder)
	}
      break;

    case '3':    //Edit
      //Extract docID, cell, and new contents from message
      docID = 0; // message docID
      std::string cell = "A1"; // message cell
      std::string content = "Hi" // message content
      //Check for circular dependancy
      //If no error:
      {
      //    send valid update message to client
      //    store previous value of cell in undo list
      //    update contents of cell
	spreadsheets[docID].set_cell(cell, content);
      //    send update to all clients working on docID
      }
      //If error:
      {
      //    send invalid edit message to client
      }
      break;

    case '4':    //Undo
      //Extract docID from message
      //If there are changes to undo:
      //    store current value of cell in redo list
      //    change contents of cell to last contents in undo list
      //    send update to all clients working on docID
      break;

    case '5':    //Redo
      //Extract docID from message
      //If there are changes to redo:
      //    Store current contents of cell in undo list
      //    Change contents of cell to last contents in redo list
      //    Send update to all clients working on docID
      break;

    case '6':    //Save
      //Save the current state of the document the client is working on
      break;

    case '7':    //Rename
      //Extract new filename from message
      //If new filename is already in use on server:
      //    send packet with opcode 9 to client indicating invalid name
      //If new filename is valid:
      //    send packet with opcode 8 to client indicating rename accepted
      //    send packet with opcode 6 to all clients working on doc with to indicate rename occurred
      //    change name of document
      break;
    }
}

void Server::loadSpreadsheet(std::string file_name)
{

}


