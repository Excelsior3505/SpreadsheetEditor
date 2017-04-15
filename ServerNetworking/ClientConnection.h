#ifndef CLIENTCONNECTION_H
#define CLIENTCONNECTION_H

#include <string>
#include <boost/asio.hpp>
#include "SocketState.h"

class ClientConnection
{
 public:
  SocketState client;
  int connectionID;
  std::queue<std::string> message_queue;
  std::queue<std::string> incoming_message_queue;

  ClientConnection();
  ClientConnection(SocketState * s);
  ClientConnection(const ClientConnection & other);
  
  void start_waiting_for_message();
  void receive_message_loop(const boost::system::error_code& error);
  void send(const std::string message);
  void handle_send(const boost::system::error_code & error, std::size_t size);
}

#endif
