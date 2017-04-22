#include <string>
#include <queue>
#include <iostream>
#include <sstream>
#include <iterator>
#include <boost/bind.hpp>
#include <boost/asio.hpp>
#include "ClientConnection.h"

//char message_received[1024];

ClientConnection::ClientConnection(boost::asio::io_service& io_serv, int ID, Server * parent)
  : skt(io_serv)
{
  connectionID = ID;
  server = parent;
}

ClientConnection::cc_ptr ClientConnection::create(boost::asio::io_service& io_serv, int ID, Server * parent)
{
  return cc_ptr(new ClientConnection(io_serv, ID, parent));
}

void ClientConnection::start_waiting_for_message()
{
  boost::asio::async_read_until(skt, in_stream_buf, '\n', boost::bind(&ClientConnection::receive_username, shared_from_this(), boost::asio::placeholders::error));
}

void ClientConnection::receive_username(const boost::system::error_code& error)
{
  if (!error)
    {
      std::string newIncoming;
      std::istream is (&in_stream_buf);
      std::getline(is, newIncoming);

      newIncoming = "10\t" + newIncoming;
      
      incoming_message_queue.push(newIncoming);

      server->processMessage(connectionID, newIncoming);

      //in_stream_buf.consume(in_stream_buf.size()+1);
      wait_for_message();
    }
  else
    {
      //std::cout << "Error: ";
      incoming_message_queue.push("Error");
    }
}

void ClientConnection::wait_for_message()
{
   boost::asio::async_read_until(skt, in_stream_buf, '\n', boost::bind(&ClientConnection::receive_message_loop, shared_from_this(), boost::asio::placeholders::error));
}

void ClientConnection::receive_message_loop(const boost::system::error_code& error)
{
  if (!error)
    {
      std::string newIncoming;
      std::istream is (&in_stream_buf);
      std::getline(is, newIncoming);
      
      incoming_message_queue.push(newIncoming);

      server->processMessage(connectionID, newIncoming);
     
      wait_for_message();
    }
  else
    {
      //std::cout << "Error: ";
      incoming_message_queue.push("Error");
    }
}

void ClientConnection::send(const std::string message)
{
  bool write_in_progress = !message_queue.empty();
  message_queue.push(message);
  if (!write_in_progress)
    {
      boost::asio::async_write(skt, boost::asio::buffer(message_queue.front(), message_queue.front().length()), boost::bind(&ClientConnection::handle_send, shared_from_this(), boost::asio::placeholders::error, boost::asio::placeholders::bytes_transferred));
    }
}

void ClientConnection::handle_send(const boost::system::error_code & error, std::size_t bytes_transferred)
{
  if (!error)
    {
      message_queue.pop();
      if (!message_queue.empty())
	{
	  boost::asio::async_write(skt, boost::asio::buffer(message_queue.front(), message_queue.front().length()), boost::bind(&ClientConnection::handle_send, shared_from_this(), boost::asio::placeholders::error, boost::asio::placeholders::bytes_transferred));
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
