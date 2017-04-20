//Alex Koumandarakis
//CS 3505
//Final Project
//Server.h

#ifndef SERVER_H
#define SERVER_H

#include <string>
#include <utility>
#include <boost/shared_ptr.hpp>
#include <boost/enable_shared_from_this.hpp>
#include <boost/asio.hpp>
#include "ClientConnection.h"

class ClientConnection;
typedef boost::shared_ptr<ClientConnection> client_ptr;

class Server
{
 public:
  boost::asio::io_service& io_serv;
  boost::asio::ip::tcp::socket server_socket;
  boost::asio::ip::tcp::acceptor acceptor;
  std::vector<client_ptr> clients;
  std::queue< std::pair< int, std::string> > received_messages;
  std::vector<int> clientID_toDocID;
  int nextID;

  Server(boost::asio::io_service& io_service_, const boost::asio::ip::tcp::endpoint& endP);
    //: io_serv(io_service_), acceptor(io_service_, endP), server_socket(io_service_);

  void await_client();
  void send(int clientID, int docID, std::string message);
  void processMessage(int clientID, std::string messageToProcess);

 private:
  void new_client_handler(client_ptr new_cc, const boost::system::error_code& error);
};

#endif
