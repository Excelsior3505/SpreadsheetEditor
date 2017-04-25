//Alex Koumandarakis
//CS 3505
//Final Project
//Server.h

//defines the Server class

#ifndef SERVER_H
#define SERVER_H

#include <string>
#include <utility>
#include <boost/shared_ptr.hpp>
#include <boost/enable_shared_from_this.hpp>
#include <boost/asio.hpp>
#include "ClientConnection.h"
#include "baseSS.h"

class ClientConnection;
typedef boost::shared_ptr<ClientConnection> client_ptr;
typedef boost::shared_ptr<base_ss> base_ss_ptr;

class Server
{
 public:
  //The io_service used for tcp communication
  boost::asio::io_service& io_serv;
  //Socket of the server
  boost::asio::ip::tcp::socket server_socket;
  //Acceptor for the server (listens for connections)
  boost::asio::ip::tcp::acceptor acceptor;
  //List of all clients connected
  std::vector<client_ptr> clients;
  //List of all messages received from clients
  std::queue< std::pair< int, std::string> > received_messages;
  //List to keep track of which clients are working on which documents
  std::vector<int> clientID_toDocID;
  //List of spreadsheets open on the server
  std::vector<base_ss_ptr> spreadsheets;
  //For storing incomplete messages from clients
  std::string partialMessage;
  //For assigning an ID to connecting clients
  int nextID;

  //Constructor
  Server(boost::asio::io_service& io_service_, const boost::asio::ip::tcp::endpoint& endP);
    //: io_serv(io_service_), acceptor(io_service_, endP), server_socket(io_service_);

  //For functionailty, see Server.cpp
  void await_client();
  void send(int clientID, int docID, std::string message);
  void processMessage(int clientID, std::string messageToProcess);

 private:
  void new_client_handler(client_ptr new_cc, const boost::system::error_code& error);
  void loadSpreadsheet(std::string file_name);
  std::vector<std::string> split_message(std::string message);
  std::string get_all_filenames();
};

#endif
