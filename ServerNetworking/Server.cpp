//Alex Koumandarakis
//CS 3505
//Final Project
//Server.cpp

#include <iostream>
#include <string>
#include <boost/bind.hpp>
//#include <boost/shared_ptr.hpp>
//#include <boost/enable_shared_from_this.hpp>
#include <boost/asio.hpp>
#include "Server.h"
#include "SocketState.h"

using boost::asio::ip::tcp;


//Default constructor for the server
Server::Server()
{
  //Create io_service
  boost::asio::io_service io_serv;

  //initialize the object that will listen and accept tcp connections
  acceptor(io_serv, tcp::enpoint(tcp::v4(), 2112));

  //Begin awaiting clients to connect
  await_client()
}

//Copy constructor
Server::Server(const Server & other)
{
  server_socket = other.server_socket;
  acceptor = other.acceptor;
  clients = other.clients;
}

void Server::await_client()
{
  //Create new socket state to place any new connections
  SocketState new_client_SS = SocketState::create(acceptor.get_io_service());

  //Start asyncronously accepting new clients
  //Bind new_client_handler to be called when new client connects
  acceptor.async_accept(newClientSS->socket, boost::bind(&Server::new_client_handler, this, new_client_SS, boost::asio::placeholders::error));
}

void Server::new_client_handler(SocketState new_client, const boost::system::error_code& error)
{
  //If no error ocurred, add the new client to the list of clients
  if (!error)
    {
      //NEW CLIENT
      clients.push_back(new_client);
    }

  //Continue waiting for another client
  await_client();
}

void Server::send(SocketState receiver, std::string message)
{
  //Asyncronously writes a message to a specified reciever's socket
  boost::asio::async_write(receiver->socket, boost::asio::buffer(message), boost::bind(&Server::send_handler, this, boost::asio::placeholders::error, boost::asio::placeholders::bytes_transferred));
} 

//Used to handle the send event (doesn't need to do anything but exist apparently?)
void Server::send_handle(const boost::system::error_code&, std::size_t bytes_transferred)
{
}

//Asynchronously receive a message 
void Server::wait_for_message(const boost::system::error_code& error)
{
  boost::array<char, 1024> message_received;
  
  boost::asio::async_read_until(server_socket, boost::asio::buffer(message_received), "\n", boost::bind(&Server::wait_for_message, this, boost::asio::placeholders::error));

  process_message(message_received);
}

void process_message (boost::array<char, 1024> message)
{
  //TODO: implement method so that the various different messages are handled
}
