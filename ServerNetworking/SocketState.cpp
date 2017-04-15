#include <string>
#include <boost/asio.hpp>
#include "SocketState.h"

SocketState::SocketState(boost::asio::ip::tcp::socket s, int id)
{
  socket = s;
  ID = id;
  SocketConnected = false;
}

SocketState::SocketState(const SocketState & other)
{
  socket = other.socket;
  ID = other.ID;
  SocketConnected = other.SocketConnected;
  messageBuffer = other.messageBuffer;
}
