#include <string>
#include <boost/bind.hpp>
#include <boost/asio.hpp>
#include "SocketState.h"
#include "ClientConnection.h"

CLientConnection::ClientConnection()
{
  client = NULL;
  connectionID = -1;
}

ClientConnection::ClientConnection(SocketState s)
{
  client = s;
  connectionID = client.ID;
}

ClientConnection::ClientConnection(const ClientConnection & other)
{
  client = other.client;
  connectionID = other.connectionID;
}

void start_waiting_for_message()
{
  boost::array<char, 1024> message_received;
  boost::asio::async_read(client.socket, boost::asio::buffer(message_received), boost::bind(&chat_session::receive_message_loop, this, boost::asio::placeholders::error));
  std::string newIncoming(message_received.begin(), message_received.end());
  incoming_message_queue.push_back(newIncoming);
}

void receive_message_loop(const boost::system::error_code& error)
{
  if (!error)
    {
      boost::array<char, 1024> message_received;
      boost::asio::async_read(client.socket, boost::asio::buffer(message_received), boost::bind(&chat_session::receive_message_loop, this, boost::asio::placeholders::error));
      std::string newIncoming(message_received.begin(), message_received.end());
      incoming_message_queue.push_back(newIncoming);
    }
  else
    {
      //Handle error
    }
}

void send(const std::string message)
{
  bool write_in_progress = !message_queue.empty();
  message_queue.push_back(message);
  if (!write_in_progress)
    {
      boost::asio::async_write(client.socket, boost::asio::buffer(message_queue.front(), message_queue.front().length()), boost::bind(&ClientConnection::handle_send, this, boost::asio::placeholders::error, boost::asio::placeholders::size));
    }
}

void handle_send(const boost::system::error_code & error, std::size_t size)
{
  if (!error)
    {
      message_queue.pop_front();
      if (!message_queue.empty())
	{
	  boost::asio::async_write(client.socket, boost::asio::buffer(message_queue.front(), message_queue.front().length()), boost::bind(&ClientConnection::handle_send, this, boost::asio::placeholders::error, boost::asio::placeholders::size));
	}
    }
  else
    {
      //Handle error
    }
}
