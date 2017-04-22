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
#define BOOST_NO_CXX11_SCOPED_ENUMS
#include <boost/filesystem.hpp>
#undef BOOST_NO_CXX11_SCOPED_ENUMS
#include <thread>
#include "Server.h"
#include "ClientConnection.h"
#include "baseSS.h"

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
  std::cout << "Sending: " << message << std::endl;
  //If -1 given, send to all clients
  // if (clientID == -1 && docID == -1)
  //{
  //  for (int i = 0; i < clients.size(); i++)
  //	{
  //	  clients[i]->send(message);
  //	}
  //}
  //If -1 is only given for clientID, send to all clients working on specified doc
  if (clientID == -1)
    {
      for (int i = 0; i < clients.size(); i++)
	{
	  if (clientID_toDocID[i] == docID && clients[i] != NULL)
	    {
	      std::cout << "To client " << i << std::endl;
	      clients[i]->send(message);
	    }
	}
    }
  //If a specific client is given and no doc is given, send only to that client
  else if (clientID > -1 && docID == -1)
    {
      if (clients[clientID] != NULL)
	{
	  std::cout << "To client " << clientID << std::endl;
	  clients[clientID]->send(message);
	}
    }
} 

//Process all received messages
void Server::processMessage(int clientID, std::string messageToProcess)
{

  if (messageToProcess[messageToProcess.length()-1] != '\n')
    {
      std::cout << "Partial Message Received: " << messageToProcess << std::endl;
      partialMessage = partialMessage + messageToProcess;
      return;
    }

  if (!partialMessage.empty() && messageToProcess[messageToProcess.length()-1] == '\n')
    {
      messageToProcess = partialMessage + messageToProcess;
      std::cout << "Full message received: " << messageToProcess << std::endl;
      partialMessage.erase();
    }

  //TODO: process the various kinds of messages
  std::cout << "Server received message: " << std::endl;
  std::cout << messageToProcess;
  std::cout <<"From client: " << clientID << std::endl;

  received_messages.push(std::pair<int, std::string>(clientID, messageToProcess));

  if (messageToProcess == "Error")
    {
      clients[clientID] = NULL;
      clientID_toDocID[clientID] = -1;
      return;
    }

  std::vector<std::string> data = split_message(messageToProcess);
  std::istringstream is1(data[0]);
  int opCode;
  is1 >> opCode;

  switch (opCode)
    {
    case 0:    //File List
      {
	std::cout << "Sending filenames to client " << clientID << std::endl;
	//Send list of filenames (from ../files/ folder)
	send(clientID, -1, get_all_filenames());
	break;
      }

    case 1:    //New
      {
	//Extract file name from message
	std::string fileName = data[1];
	
	//Check if name exists
	if(boost::filesystem::exists("../files/" + fileName))
	  {
	    //If it does, send list of filenames (from ../files/ folder)
	    send(clientID, -1, get_all_filenames());
	  }
	else
	  {
	    //If it does not, send new docID
	    int docID = spreadsheets.size();
	    clientID_toDocID[clientID] = docID;
	    //TODO: create new spreadsheet object
	  }
	break;
      }

    case 2:    //Open
      {
	//Extract file name from message
	std::string fileName = data[1];
	
	//Check if name exists
	if(boost::filesystem::exists("../files/" + fileName))
	  {
	    int loc = 0;
	    if(spreadsheets.size() > 0)
	      {
		bool found = false;
		for(std::vector<base_ss*>::iterator it = spreadsheets.begin(); it != spreadsheets.end(); it++)
		  {
		    if((*it)->name == fileName)
		      {
			int docID = loc;
			found = true;
			break;
		      }
		    loc++;
		  }
		if(!found)
		  {
		    int docID = spreadsheets.size();
		    base_ss newSS = base_ss();
		    spreadsheets.push_back(&newSS);
		    spreadsheets.back()->loadSS("../files/" + fileName);
		  }
	      }
	    else
	      {
		int docID = 0;
		base_ss newSS = base_ss();
		spreadsheets.push_back(&newSS);
		spreadsheets.back()->loadSS("../files/" + fileName);
	      }
	    //If it does, send docID
	  }
	else
	  {
	    //If it does not, send list of filenames (from ../files/ folder)
	    send(clientID, -1, get_all_filenames());
	  }

	break;
      }

    case 3:    //Edit
      {
	if (data.size() == 4)
	  {
	    //Extract docID, cell, and new contents from message
	    std::istringstream iss(data[1]);
	    int docID;
	    iss >> docID;
	    std::string docIDSend = data[1];
	    std::string cell = data[2];
	    std::string content = data[3];
	    
	    //TODO: create update_cell method for base_ss
	    //      method should:
	    //          check for circular dependancy (return num indicating if one was found)
	    //          store old value in undo list	
	    //          update value of cell
	    //          clear redo list
	    int ret = 0;//spreadsheets[docID]->set_cell(cell, content);
	    if (ret > 0) //ERROR: circular dependancy
	      {
		std::string invalidEdit = "5\t" + docIDSend + "\n";
		send(clientID, -1, invalidEdit);
	      }
	    else //Edit is valid
	      {
		std::string validEdit = "4\t" + docIDSend + "\n";
		std::string cellUpdate = "3\t" + docIDSend + "\t" + cell + "\t" + content + "\n";
		send(clientID, -1, validEdit);
		send(-1, docID, cellUpdate);
	      }
	  }
	else if (data.size() > 1)
	  {
	    std::string docIDSend = data[1];
	    std::string invalidEdit = "5\t" + docIDSend + "\n";
	    send(clientID, -1, invalidEdit);
	  }
	break;
      }

    case 4:    //Undo
      {
	/*
	//If there are changes to undo:
	//    store current value of cell in redo list
	//    change contents of cell to last contents in undo list
	//    send update to all clients working on docID

	//Extract docID from message
	std::string docIDSend = data[1];
	std::istringstream is(data[1]);
	int docID;
	is >> docID;
	
	if (!spreadsheets[docID]->undo.empty())
	  {
	    std::pair<std::string, std::string> lastEdit = spreadsheets[docID]->undo.back();
	    spreadsheets[docID]->undo.pop_back();

	    std::string cell = lastEdit.first();
	    std::string contents = lastEdit.second();
	    
	    int ret = 0;// spreadsheets[docID]->set_cell(cell, contents);
	    if (ret > 0) //ERROR
	      {
		std::string invalidEdit = "5\t" + docIDSend + "\n";
		send(clientID, -1, invalidEdit);
	      }
	    else //Edit is valid
	      {
		std::string validEdit = "4\t" + docIDSend + "\n";
		std::string cellUpdate = "3\t" + docIDSend + "\t" + cell + "\t" + content + "\n";
		send(clientID, -1, validEdit);
		send(-1, docID, cellUpdate);
	      }
	      } */
	break;
      }

    case 5:    //Redo
      { /*
	//If there are changes to redo:
	//    Store current contents of cell in undo list
	//    Change contents of cell to last contents in redo list
	//    Send update to all clients working on docID

	//Extract docID from message
	std::string docIDSend = data[1];
	std::istringstream iss1(data[1]);
	int docID;
	iss1 >> docID;
	
	if (!spreadsheets[docID]->redo.empty())
	  {
	    std::pair<std::string, std::string> lastEdit = spreadsheets[docID]->redo.back();
	    spreadsheets[docID]->redo.pop_back();
	    
	    std::string cell = lastEdit.first();
	    std::string contents = lastEdit.second();
	    
	    int ret = 1;//spreadsheets[docID]->set_cell(cell, contents);
	    if (ret > 0) //ERROR
	      {
		std::string invalidEdit = "5\t" + docIDSend + "\n";
		send(clientID, -1, invalidEdit);
	      }
	    else //Edit is valid
	      {
		std::string validEdit = "4\t" + docIDSend + "\n";
		std::string cellUpdate = "3\t" + docIDSend + "\t" + cell + "\t" + content + "\n";
		send(clientID, -1, validEdit);
		send(-1, docID, cellUpdate);
	      }
	      }*/
	break;
      }

    case 6:    //Save
      {
	int docID = clientID_toDocID[clientID];
	
	//Save the current state of the document the client is working on
	break;
      }

    case 7:    //Rename
      {
	//Extract new filename from message
	std::string filename = data[1];
	int docID = clientID_toDocID[clientID];
	
	//If new filename is already in use on server:
	//    send packet with opcode 9 to client indicating invalid name
	//If new filename is valid:
	//    send packet with opcode 8 to client indicating rename accepted
	//    send packet with opcode 6 to all clients working on doc with to indicate rename occurred
	//    change name of document
	break;
      }
    case 8: //Edit Location
      {
	if (data.size() == 3)
	  {
	    //Extract docID and cellName from message
	    std::string docIDSend = data[1];
	    std::string clientIDSend = std::to_string(clientID);
	    
	    std::istringstream iss1(data[1]);
	    int docID;
	    iss1 >> docID;
	    std::string cellName = data[2];
	    
	    std::string userN = clients[clientID]->username;
	    
	    //Let other users working on document docID what cell clientID is editing
	    std::string editLocation = "A\t" + docIDSend + "\t" + cellName + "\t" + clientIDSend + "\t" + userN + "\n";
	    send(-1, docID, editLocation);
	  }
	break;
      }
    case 9:    //Close document
      {
	//Extract docId from message
	std::cout << "Client " << clientID << " has disconnected." << std::endl;

        std::istringstream iss1(data[1]);
	int docID;
	iss1 >> docID;
	std::string docIDSend = data[1];

	std::string clientIDSend = std::to_string(clientID);
	std::string userN = clients[clientID]->username;

	clients[clientID] = NULL;
	clientID_toDocID[clientID] = -1;

	//send edit location with cell name -1 to all other users on document docID to indicate user has left
	std::string userLeftMessage = "A\t" + docIDSend + "\t-1\t" + clientIDSend + "\t" + userN + "\n"; 
	send(-1, docID, userLeftMessage);
	break;
      }
    case 10:    //Received username
      {
	std::string userN = data[1];
	clients[clientID]->username = userN;
	break;
      }
    }
  std::cout << std::endl << std::endl;
}

//Extracts data from message string sent from client
std::vector<std::string> Server::split_message(std::string message)
{
  std::stringstream msg(message);
  std::string data;
  std::vector<std::string> dataList;

  while (std::getline(msg, data, '\t'))
    {
      dataList.push_back(data);
    } 

  std::string lastString = dataList.back();
  dataList.pop_back();
  
  if (lastString.back() == '\n')
    {
      lastString.pop_back();
    }

  dataList.push_back(lastString);
  return dataList;
}

//returns all the filenames from the files directory
std::string Server::get_all_filenames()
{
  std::string allFileNames = "0\t";

  boost::filesystem::path p("../files/");

  boost::filesystem::directory_iterator end_it;

  for(boost::filesystem::directory_iterator it(p); it != end_it; ++it)
    {
      if (boost::filesystem::is_regular_file(it->path()))
	{
	  std::string fileName = it->path().string();
	  fileName = fileName.substr(9); 
	  allFileNames = allFileNames + fileName + "\t";
	}
    }

  allFileNames = allFileNames + "\n";
  return allFileNames;
}


