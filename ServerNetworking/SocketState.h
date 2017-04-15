#ifndef SOCKETSTATE_H
#define SOCKETSTATE_H

#include <string>
#include <boost/asio.hpp>

class SocketState
{
 public:
  boost::asio::ip::tcp::socket socket;
  byte messageBuffer[1024];
  bool SocketConnected;
  int ID

 public:
  SocketState(boost::asio::ip::tcp::socket s, int id);
  SocketState(const SocketState & other);
};

#endif
