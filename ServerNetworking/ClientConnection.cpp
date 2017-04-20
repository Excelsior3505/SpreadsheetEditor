#include <string>
#include <queue>
#include <iostream>
#include <sstream>
#include <iterator>
#include <boost/bind.hpp>
#include <boost/asio.hpp>
#include "ClientConnection.h"

char message_received[10];

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
  boost::asio::async_read(skt, boost::asio::buffer(message_received, 10), boost::bind(&ClientConnection::receive_message_loop, shared_from_this(), boost::asio::placeholders::error));
 
  //std::string newIncoming((std::istreambuf_iterator<char>(&message_received)), std::istreambuf_iterator<char>());

  //std::string newIncoming(message_received);

  //std::cout << newIncoming << std::endl;
  //incoming_message_queue.push(newIncoming);
}

void ClientConnection::receive_message_loop(const boost::system::error_code& error)
{
  std::cout << "Callback" << std::endl;
  if (!error)
    {
      for (int i=0; i<10; i++)
	{
	  std::cout << message_received[i] << std::endl;
	}
      boost::asio::streambuf message_received;
      boost::asio::async_read_until(skt, message_received, '\n', boost::bind(&ClientConnection::receive_message_loop, shared_from_this(), boost::asio::placeholders::error));
      //std::string newIncoming((std::istreambuf_iterator<char>(&message_received)), std::istreambuf_iterator<char>());
      std::string newIncoming(boost::asio::buffers_begin(message_received.data()), boost::asio::buffers_begin(message_received.data())+message_received.size());

      std::cout << newIncoming << std::endl;
      incoming_message_queue.push(newIncoming);
    }
  else
    {
      std::cout << "Error" << std::endl;
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
