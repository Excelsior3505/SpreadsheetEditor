//Alex Koumandarakis
//CS 3505
//ClientConnection.cpp

//Class for storing clients' connection information and for
//receiving and sending messages to and from a client

#include <string>
#include <queue>
#include <iostream>
#include <sstream>
#include <iterator>
#include <boost/bind.hpp>
#include <boost/asio.hpp>
#include "ClientConnection.h"

//Constructor (called by create)
ClientConnection::ClientConnection(boost::asio::io_service& io_serv, int ID, Server * parent)
  : skt(io_serv)
{
  connectionID = ID;
  server = parent;
}

//Creates a client connection shared pointer
ClientConnection::cc_ptr ClientConnection::create(boost::asio::io_service& io_serv, int ID, Server * parent)
{
  return cc_ptr(new ClientConnection(io_serv, ID, parent));
}

//Starts the client connection waiting for a message
//Callback: receive_username
void ClientConnection::start_waiting_for_message()
{
  boost::asio::async_read_until(skt, in_stream_buf, '\n', boost::bind(&ClientConnection::receive_username, shared_from_this(), boost::asio::placeholders::error));
}

//First message from client should be their username
//Receives that username and sends info up to server to store
void ClientConnection::receive_username(const boost::system::error_code& error)
{
  if (!error)
    {
      std::string newIncoming;
      std::istream is (&in_stream_buf);
      std::getline(is, newIncoming);

      newIncoming = "10\t" + newIncoming;

      if (!is.eof())
	{
	  newIncoming = newIncoming + '\n';
	}
      
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

//Continues waiting for any type of message
void ClientConnection::wait_for_message()
{
   boost::asio::async_read_until(skt, in_stream_buf, '\n', boost::bind(&ClientConnection::receive_message_loop, shared_from_this(), boost::asio::placeholders::error));
}

//Handles receiving a message
void ClientConnection::receive_message_loop(const boost::system::error_code& error)
{
  if (!error)
    {
      //Generate string from stream buffer, send string to server to be processed
      std::string newIncoming;
      std::istream is (&in_stream_buf);
      std::getline(is, newIncoming);

      if (!is.eof())
	{
	  newIncoming = newIncoming + '\n';
	}
      
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

//Send message to client
void ClientConnection::send(const std::string message)
{
  //If a write is in progress, don't send message, just add it to queue
  bool write_in_progress = !message_queue.empty();
  message_queue.push(message);
  if (!write_in_progress)
    {
      boost::asio::async_write(skt, boost::asio::buffer(message_queue.front(), message_queue.front().length()), boost::bind(&ClientConnection::handle_send, shared_from_this(), boost::asio::placeholders::error, boost::asio::placeholders::bytes_transferred));
    }
}

//After sending a message, continue sending messages stored in the message queue until it is empty
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

//Returns the socket of the client
boost::asio::ip::tcp::socket& ClientConnection::get_socket()
{
  return skt;
}
