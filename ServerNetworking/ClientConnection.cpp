#include <string>
#include <queue>
#include <boost/bind.hpp>
#include <boost/asio.hpp>
#include "ClientConnection.h"

ClientConnection::ClientConnection(boost::asio::io_service& io_serv, int ID)
  : skt(io_serv)
{
  connectionID = ID;
}

ClientConnection::cc_ptr ClientConnection::create(boost::asio::io_service& io_serv, int ID)
{
  return cc_ptr(new ClientConnection(io_serv, ID));
}

void ClientConnection::start_waiting_for_message()
{
  boost::array<char, 1024> message_received;
  boost::asio::async_read(skt, boost::asio::buffer(message_received), boost::bind(&ClientConnection::receive_message_loop, this, boost::asio::placeholders::error));
  std::string newIncoming(message_received.begin(), message_received.end());
  incoming_message_queue.push(newIncoming);
}

void ClientConnection::receive_message_loop(const boost::system::error_code& error)
{
  if (!error)
    {
      boost::array<char, 1024> message_received;
      boost::asio::async_read(skt, boost::asio::buffer(message_received), boost::bind(&ClientConnection::receive_message_loop, this, boost::asio::placeholders::error));
      std::string newIncoming(message_received.begin(), message_received.end());
      incoming_message_queue.push(newIncoming);
    }
  else
    {
      //Handle error
    }
}

void ClientConnection::send(const std::string message)
{
  bool write_in_progress = !message_queue.empty();
  message_queue.push(message);
  if (!write_in_progress)
    {
      boost::asio::async_write(skt, boost::asio::buffer(message_queue.front(), message_queue.front().length()), boost::bind(&ClientConnection::handle_send, this, boost::asio::placeholders::error, boost::asio::placeholders::bytes_transferred));
    }
}

void ClientConnection::handle_send(const boost::system::error_code & error, std::size_t bytes_transferred)
{
  if (!error)
    {
      message_queue.pop();
      if (!message_queue.empty())
	{
	  boost::asio::async_write(skt, boost::asio::buffer(message_queue.front(), message_queue.front().length()), boost::bind(&ClientConnection::handle_send, this, boost::asio::placeholders::error, boost::asio::placeholders::bytes_transferred));
	}
    }
  else
    {
      //Handle error
    }
}

boost::asio::ip::tcp::socket& ClientConnection::get_socket()
{
  return skt;
}
