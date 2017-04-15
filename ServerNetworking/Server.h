//Alex Koumandarakis
//CS 3505
//Final Project
//Server.h

#ifndef SERVER_H
#define SERVER_H

#include <string>
#include <boost/asio.hpp>

class Server
{
 public:
  boost::asio::ip::tcp::socket server_socket;
  boost::asio::io::tcp::acceptor acceptor;
  std::vector<SocketState> clients;

  Server();
  Server(const Server & other);

  void await_client();
  void send(SocketState receiver, std::string message);
  void wait_for_message(const boost::system::error_code&);
 

 private:
  void new_client_handler(SocketState new_client, const boost::system::error_code& error);
  void send_handle(const boost::system::error_code&, std::size_t bytes_transferred);
};

#endif
