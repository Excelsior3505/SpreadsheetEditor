//Alex Koumandarakis
//CS 3505
//ClientConnection.cpp

//Class for storing clients' connection information and for
//receiving and sending messages to and from a client

#ifndef CLIENTCONNECTION_H
#define CLIENTCONNECTION_H

#include <string>
#include <queue>
#include <boost/asio.hpp>
#include <boost/enable_shared_from_this.hpp>
#include <boost/shared_ptr.hpp>
#include "Server.h"

class Server;

class ClientConnection
: public boost::enable_shared_from_this<ClientConnection> //This allows multiple threads to access and modify data of this object
{
 public:
  typedef boost::shared_ptr<ClientConnection> cc_ptr;
  //ID of the client
  int connectionID;
  //Queue for storing outgoing messages
  std::queue<std::string> message_queue;
  //Queue for storing incoming messages
  std::queue<std::string> incoming_message_queue;
  //The socket of the client
  boost::asio::ip::tcp::socket skt;
  //The stream buffer that incoming messages will be written into
  boost::asio::streambuf in_stream_buf;
  //The username of the client
  std::string username;
  //Reference to the server
  Server * server;

  //Constructor
  ClientConnection(boost::asio::io_service& io_serv, int ID, Server * parent);
  //Create the shared pointer object
  static cc_ptr create(boost::asio::io_service& io_serv, int ID, Server * parent);
  
  //For functionality of these methods, see ClientConnection.cpp
  void start_waiting_for_message();
  void receive_username(const boost::system::error_code& error);
  void wait_for_message();
  void receive_message_loop(const boost::system::error_code& error);
  void send(const std::string message);
  void handle_send(const boost::system::error_code & error, std::size_t size);
  boost::asio::ip::tcp::socket& get_socket();
};

#endif
