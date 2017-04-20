#include <string>
#include <queue>
#include <iostream>
#include <sstream>
#include <iterator>
#include <boost/bind.hpp>
#include <boost/asio.hpp>
#include "ClientConnection.h"

//char message_received[1024];

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
  //boost::asio::async_read(skt, boost::asio::buffer(message_received, 1), boost::bind(&ClientConnection::receive_message_loop, shared_from_this(), boost::asio::placeholders::error));

  boost::asio::async_read_until(skt, in_stream_buf, '\n', boost::bind(&ClientConnection::receive_message_loop, shared_from_this(), boost::asio::placeholders::error));
  //std::string newIncoming((std::istreambuf_iterator<char>(&message_received)), std::istreambuf_iterator<char>());
}

void ClientConnection::receive_message_loop(const boost::system::error_code& error)
{
  if (!error)
    {
      std::string newIncoming((std::istreambuf_iterator<char>(&in_stream_buf)), std::istreambuf_iterator<char>());
      std::cout << newIncoming << std::endl;
      incoming_message_queue.push(newIncoming);
   
      //std::cout << "First Char: " << message_received[0] << std::endl; 

      //for (int i = 0; i < 1024; i++)
      //{
      //  std::cout << message_received[i];
      //}

      //boost::asio::streambuf message_received;
      //boost::asio::async_read_until(skt, message_received, '\n', boost::bind(&ClientConnection::receive_message_loop, shared_from_this(), boost::asio::placeholders::error));

      boost::asio::async_read_until(skt, in_stream_buf, '\n', boost::bind(&ClientConnection::receive_message_loop, shared_from_this(), boost::asio::placeholders::error));
      

      //std::string newIncoming(boost::asio::buffers_begin(message_received.data()), boost::asio::buffers_begin(message_received.data())+message_received.size());
    }
  else
    {
      std::cout << "Error: ";
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
