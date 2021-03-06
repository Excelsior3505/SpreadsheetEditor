//Alex Koumandarakis and Tristan Willis
//CS 3505
//Final Project
//Server.cpp

//Implements the server for the spreadsheet editor

//CHAT_SERVER.cpp from boost library examples was used as a 
//starting point for implementing the server
//http://www.boost.org/doc/libs/1_53_0/doc/html/boost_asio/example/chat/chat_server.cpp

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
#include <chrono>
#include "Server.h"
#include "ClientConnection.h"
#include "baseSS.h"

using boost::asio::ip::tcp;

typedef boost::shared_ptr<ClientConnection> client_ptr;
typedef boost::shared_ptr<base_ss> base_ss_ptr;

//Default constructor for the server
Server::Server(boost::asio::io_service& io_service_, const boost::asio::ip::tcp::endpoint& endP)
  : io_serv(io_service_), acceptor(io_service_, endP), server_socket(io_service_)
{
  nextID = 0;

  await_client();
}

//Starts the server waiting for a client to connect
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

//Asynchronously handles a new client connecting
void Server::new_client_handler(client_ptr new_cc, const boost::system::error_code& error)
{
  std::cout << "New client connected, ID: " << nextID-1 << std::endl;

  //If no error ocurred, add the new client to the list of clients
  if (!error)
    {
      //NEW CLIENT
      //Assign ID, assign docID (default -1), add to clients list
      //Send client their ID
      //Start waiting for messages from client
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

//Sends a message to either a client, or to all clients working on a document
void Server::send(int clientID, int docID, std::string message)
{
  std::cout << "Sending: " << message << std::endl;
  
  //If -1 is only given for clientID, send to all clients working on specified doc
  if (clientID == -1)
    {
      for (int i = 0; i < clients.size(); i++)
	{
	  if (clientID_toDocID[i] == docID && clients[i] != NULL)
	    {
	      //std::cout << "To client " << i << std::endl;
	      clients[i]->send(message);
	    }
	}
    }
  //If a specific client is given and no doc is given, send only to that client
  else if (clientID > -1 && docID == -1)
    {
      if (clients[clientID] != NULL)
	{
	  //std::cout << "To client " << clientID << std::endl;
	  clients[clientID]->send(message);
	}
    }
} 

//Processes a received message (called from ClientConnection)
void Server::processMessage(int clientID, std::string messageToProcess)
{
  //Make sure to store any partial messages
  if (messageToProcess[messageToProcess.length()-1] != '\n')
    {
      std::cout << "Partial Message Received: " << messageToProcess << std::endl;
      partialMessage = partialMessage + messageToProcess;
      return;
    }

  //If there is a partial message stored, and the next message received has newline, complete message
  if (!partialMessage.empty() && messageToProcess[messageToProcess.length()-1] == '\n')
    {
      messageToProcess = partialMessage + messageToProcess;
      std::cout << "Full message received: " << messageToProcess << std::endl;
      partialMessage.erase();
    }

  //Process the various kinds of messages
  std::cout << "Server received message: " << std::endl;
  std::cout << messageToProcess;
  std::cout <<"From client: " << clientID << std::endl;

  received_messages.push(std::pair<int, std::string>(clientID, messageToProcess));

  //If client connection returns an error, stop storing that client
  if (messageToProcess == "Error")
    {
      clients[clientID] = NULL;
      clientID_toDocID[clientID] = -1;
      return;
    }

  //Split the message into parts
  std::vector<std::string> data = split_message(messageToProcess);
  
  //Extract the opCode of the message
  std::istringstream is1(data[0]);
  int opCode;
  is1 >> opCode;

  switch (opCode)
    {
    case 0:    //File List
      {
	//Send list of filenames (from ../files/ folder)
	std::cout << "Sending filenames to client " << clientID << std::endl;
	send(clientID, -1, get_all_filenames());
	break;
      }

    case 1:    //New
      {
	//Extract file name from message
	std::string fileName = data[1];
	
	//Make sure name ends in .ss
	std::string hasSS = "";
	size_t pos = fileName.find_last_of(".");
	if(pos != std::string::npos)
	  {
	    hasSS = fileName.substr(pos);
	    hasSS.erase(std::remove(hasSS.begin(), hasSS.end(), ' '), hasSS.end());
	    if(hasSS != ".ss")
	      fileName = fileName + ".ss";
	  }
	else
	  fileName = fileName + ".ss";
	
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
	    
	    //create new spreadsheet object
	    base_ss::base_ss_ptr newSS = base_ss::create();
	    newSS->name = fileName;
	    spreadsheets.push_back(newSS);
	    
	    //Send valid open message to client
	    std::string docIDSend = std::to_string(docID);
	    std::string newFileMessage = "1\t" + docIDSend + "\n"; 
	    send(clientID, -1, newFileMessage);

	  }
	break;
      }

    case 2:    //Open
      {
	//Extract file name from message
	std::string fileName = data[1];
	std::cout << fileName << std::endl;
	
	//Make sure file name ends in .ss
	std::string hasSS = "";
	size_t pos = fileName.find_last_of(".");
	if(pos != std::string::npos)
	  {
	    hasSS = fileName.substr(pos);
	    std::cout << "hasSS: " << hasSS << std::endl;
	    hasSS.erase(std::remove(hasSS.begin(), hasSS.end(), ' '), hasSS.end());
	    if(hasSS != ".ss")
	      fileName = fileName + ".ss";
	  }
	else
	  fileName = fileName + ".ss";

	//Check if name exists
	if(boost::filesystem::exists("../files/" + fileName))
	  {
	    int loc = 0;
	    std::cout << "Num Spreadsheets open: " << spreadsheets.size() << std::endl;
	    //If there are spreadsheets open...
	    if(spreadsheets.size() > 0)
	      {
		//See if the file requested is already open on the server
		bool found = false;
		for(std::vector<base_ss_ptr>::iterator it = spreadsheets.begin(); it != spreadsheets.end(); it++)
		  {
		    if((*it)->name == fileName)
		      {
			//If it was already open
			std::cout << "Document was already open" << std::endl;
			int docID = loc;
			found = true;
			clientID_toDocID[clientID] = docID;
			
			//Send valid open to client with document's ID
			std::string validOpen  = "2\t" + std::to_string(docID) + "\n";
			send(clientID, -1, validOpen);
			
			//Iterate through spreadsheet and send all cell data to client
			std::map<std::string, std::string>::iterator it;
			for (it = spreadsheets[docID]->spreadsheet.begin(); it != spreadsheets[docID]->spreadsheet.end(); ++it)
			  {
			    std::string cell = it->first;
			    std::string content = it->second;
			    
			    if (it->first != "Version:")
			      {
				//std::this_thread::sleep_for(std::chrono::milliseconds(1000));
				std::string update  = "3\t" + std::to_string(docID) + "\t" + cell + "\t" + content + "\n";
				std::thread sendThread(&Server::send, this, clientID, -1, update);
				sendThread.join();
			      }	
			  }
			break;
		      }
		    loc++;
		  }
		if(!found)
		  {
		    //If the document was not open on the server
		    std::cout << "Document not open" << std::endl;
		    int docID = spreadsheets.size();
		    base_ss::base_ss_ptr newSS = base_ss::create(); 
		    
		    //Load spreadsheet and Send valid open to client with document's ID
		    spreadsheets.push_back(newSS);
		    spreadsheets.back()->loadSS("../files/" + fileName);
		    spreadsheets.back()->name = fileName;
		    std::string validOpen  = "2\t" + std::to_string(docID) + "\n";
		    send(clientID, -1, validOpen);

		    //Send cell data to client
		    std::map<std::string, std::string>::iterator it;
		    for (it = spreadsheets.back()->spreadsheet.begin(); it != spreadsheets.back()->spreadsheet.end(); ++it)
		      {
			std::string cell = it->first;
			std::string content = it->second;
		
			if (it->first != "Version:")
			  {
			    //std::this_thread::sleep_for(std::chrono::milliseconds(1000));
			    std::string update  = "3\t" + std::to_string(docID) + "\t" + cell + "\t" + content + "\n";
			    std::thread sendThread(&Server::send, this, clientID, -1, update);
			    sendThread.join(); 
			  }	
		      }
		    clientID_toDocID[clientID] = docID;
		  }
	      }
	    else
	      {
		//If no spreadsheets are open on the client, load first spreadsheet and send valid open to client
		int docID = 0;
		base_ss::base_ss_ptr newSS = base_ss::create(); 
		spreadsheets.push_back(newSS);

		spreadsheets.back()->loadSS("../files/" + fileName);
		spreadsheets.back()->name = fileName;
		std::string validOpen  = "2\t" + std::to_string(docID) + "\n";
		send(clientID, -1, validOpen);
		
		//Send cell data to client
		std::map<std::string, std::string>::iterator it;
		for (it = spreadsheets.back()->spreadsheet.begin(); it != spreadsheets.back()->spreadsheet.end(); ++it)
		  {
		    std::string cell = it->first;
		    std::string content = it->second;
		    
		    if (it->first != "Version:")
		      {
			//std::this_thread::sleep_for(std::chrono::milliseconds(1000));
			std::string update  = "3\t" + std::to_string(docID) + "\t" + cell + "\t" + content + "\n";
			std::thread sendThread(&Server::send, this, clientID, -1, update);
			sendThread.join();
		      }
		  }
		
		clientID_toDocID[clientID] = docID;
	      }
	  }
	else
	  {
	    //If file name does not exist, send list of filenames (from ../files/ folder)
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
	    
	    //Do not accept edits with invalid docIDs
	    if (docID < 0 || docID >= spreadsheets.size())
	      {
		break;
	      }

	    std::string docIDSend = data[1];
	    std::string cell = data[2];
	    std::string content = data[3];

	    std::string oldCellContent = spreadsheets[docID]->get_contents(cell);
	    if (oldCellContent.empty())
	      {
		oldCellContent = " ";
	      }

	    std::cout << "Cell: " << cell << std::endl;
	    std::cout << "content: " << content << std::endl;
	    
	    //update_cell method for base_ss:
	    //          check for circular dependancy (return num indicating if one was found)
	    //          store old value in undo list	
	    //          update value of cell
	    //          clear redo list
	    int ret = spreadsheets[docID]->set_cell(cell, content);
	    if (ret > 0) //ERROR: circular dependancy
	      {
		//Send invalid edit message to client who sent edit
		std::string invalidEdit = "5\t" + docIDSend + "\n";
		send(clientID, -1, invalidEdit);
	      }
	    else //Edit is valid
	      {
		//Save the document every time a valid edit is made
		std::string name = "../files/" + spreadsheets[docID]->name;
		spreadsheets[docID]->saveSS(name);

		//Send valid edit to client who sent edit, send edit to all clients on document
		std::string validEdit = "4\t" + docIDSend + "\n";
		std::string cellUpdate = "3\t" + docIDSend + "\t" + cell + "\t" + content + "\n";
		spreadsheets[docID]->undo.push_back(std::pair<std::string, std::string>(cell, oldCellContent));
		spreadsheets[docID]->redo.clear();
		send(clientID, -1, validEdit);
		send(-1, docID, cellUpdate);
	      }
	  }
	else if (data.size() > 1)
	  {
	    //If invalid message received but still sent docID, send invalid edit 
	    std::string docIDSend = data[1];
	    std::string invalidEdit = "5\t" + docIDSend + "\n";
	    send(clientID, -1, invalidEdit);
	  }
	break;
      }

    case 4:    //Undo
      {
	//If there are changes to undo:
	//    store current value of cell in redo list
	//    change contents of cell to last contents in undo list
	//    send update to all clients working on docID

	//Extract docID from message
	std::string docIDSend = data[1];
	std::istringstream is(data[1]);
	int docID;
	is >> docID;
	
	//Do not accept edits with incorrect docID
	if (docID < 0 || docID >= spreadsheets.size())
	  {
	    break;
	  }

	if (!spreadsheets[docID]->undo.empty())
	  {
	    std::pair<std::string, std::string> lastEdit = spreadsheets[docID]->undo.back();
	    spreadsheets[docID]->undo.pop_back();

	    std::string cell = lastEdit.first;
	    std::string content = lastEdit.second;
	    std::string oldCellContent = spreadsheets[docID]->get_contents(cell);
	    spreadsheets[docID]->redo.push_back(std::pair<std::string, std::string>(cell, oldCellContent));
	    
	    //Perform edit using old values of cell
	    int ret = spreadsheets[docID]->set_cell(cell, content);
	    if (ret > 0) //ERROR
	      {
		std::string invalidEdit = "5\t" + docIDSend + "\n";
		send(clientID, -1, invalidEdit);
	      }
	    else //Edit is valid
	      {
		std::string name = "../files/" + spreadsheets[docID]->name;
		spreadsheets[docID]->saveSS(name);
		
		std::string validEdit = "4\t" + docIDSend + "\n";
		std::string cellUpdate = "3\t" + docIDSend + "\t" + cell + "\t" + content + "\n";
		send(clientID, -1, validEdit);
		send(-1, docID, cellUpdate);
	      }
	  }
	break;
      }

    case 5:    //Redo
      { 
	//If there are changes to redo:
	//    Store current contents of cell in undo list
	//    Change contents of cell to last contents in redo list
	//    Send update to all clients working on docID

	//Extract docID from message
	std::string docIDSend = data[1];
	std::istringstream iss1(data[1]);
	int docID;
	iss1 >> docID;
	
	//Do not accept edits with invalid docID
	if (docID < 0 || docID >= spreadsheets.size())
	  {
	    break;
	  }


	if (!spreadsheets[docID]->redo.empty())
	  {
	    std::pair<std::string, std::string> lastEdit = spreadsheets[docID]->redo.back();
	    spreadsheets[docID]->redo.pop_back();
	    
	    std::string cell = lastEdit.first;
	    std::string content = lastEdit.second;
	    spreadsheets[docID]->undo.push_back(std::pair<std::string, std::string>(cell, content));
	    
	    //Perform edit with values stored in redo list
	    int ret = spreadsheets[docID]->set_cell(cell, content);
	    if (ret > 0) //ERROR
	      {
		std::string invalidEdit = "5\t" + docIDSend + "\n";
		send(clientID, -1, invalidEdit);
	      }
	    else //Edit is valid
	      {
		std::string name = "../files/" + spreadsheets[docID]->name;
		spreadsheets[docID]->saveSS(name);

		std::string validEdit = "4\t" + docIDSend + "\n";
		std::string cellUpdate = "3\t" + docIDSend + "\t" + cell + "\t" + content + "\n";
		send(clientID, -1, validEdit);
		send(-1, docID, cellUpdate);
	      }
	  }
	break;
      }

    case 6:    //Save
      {
	int docID = clientID_toDocID[clientID];
	if (docID >= 0 && docID < spreadsheets.size())
	  {
	    //Save the current state of the document the client is working on
	    std::string name = "../files/" + spreadsheets[docID]->name;
	    spreadsheets[docID]->saveSS(name);
	  }
	break;
      }

    case 7:    //Rename
      {
	if (data.size() < 3)
	  {
	    break;
	  }

	//Extract new filename from message
	std::string fileName = data[2];
	std::string hasSS = "";
	
	//Make sure is has .ss at end
	size_t pos = fileName.find_last_of(".");
	if(pos != std::string::npos)
	  {
	    hasSS = fileName.substr(pos);
	    hasSS.erase(std::remove(hasSS.begin(), hasSS.end(), ' '), hasSS.end());
	    if(hasSS == ".ss")
	      fileName = fileName.substr(0,pos);
	  }
	
        std::istringstream iss2(data[1]);
	int docID;
	iss2 >> docID;

	if (docID < 0)
	  {
	    break;
	  }

	//Get old file name of the document
	std::string oldFileName = "../files/" + spreadsheets[docID]->name;
	//Make sure it ends in .ss
        hasSS = "";
	pos = oldFileName.find_last_of(".");
	if(pos != std::string::npos)
	  {
	    hasSS = oldFileName.substr(pos);
	    std::cout << "hasSS: " << hasSS << std::endl;
	    hasSS.erase(std::remove(hasSS.begin(), hasSS.end(), ' '), hasSS.end());
	    if(hasSS != ".ss")
	      oldFileName = oldFileName + ".ss";
	  }
	else
	  oldFileName = oldFileName + ".ss";
        
	//If new filename is already in use on server:
        if(boost::filesystem::exists("../files/" + fileName))
	  {
	    //send packet with opcode 9 to client indicating invalid name
	    std::string invalidName = "9\t" + std::to_string(docID) + "\n";
	    send(clientID, -1, invalidName); 
	    break;
	  }

	//If new filename is valid:
	//    send packet with opcode 8 to client indicating rename accepted
	//    send packet with opcode 6 to all clients working on doc with to indicate rename occurred
	//    remove document from files
	//    save document with new name
	boost::filesystem::remove(oldFileName);
	spreadsheets[docID]->saveSS("../files/" + fileName);

	std::string validName = "8\t" + std::to_string(docID) + "\n";
	std::string rename = "6\t" + std::to_string(docID) + "\t" + fileName + "\n";
	send(clientID, -1, validName);
	send(-1, docID, rename);
	spreadsheets[docID]->rename(fileName);
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

	//Remove client from list
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

  //Removes newline from last piece of data
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


