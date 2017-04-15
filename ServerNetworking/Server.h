//Alex Koumandarakis
//CS 3505
//Final Project
//Server.h

#ifndef SERVER_H
#define SERVER_H

#include <string>
#include <boost/asio.hpp>
#include "ClientConnection.h"

class Server
{
 public:
  boost::asio::ip::tcp::socket server_socket;
  boost::asio::io::tcp::acceptor acceptor;
  std::vector<ClientConnection> clients;
  std::queue<std::string> received_messages;
  std::vector<int> clientID_toDocID;
  int nextID;

  Server();
  Server(const Server & other);

  void await_client();
  void send(std::string message);
  void check_for_messages();
  void processMessages();
 

 private:
  void new_client_handler(SocketState new_client, const boost::system::error_code& error);
};

#endif
