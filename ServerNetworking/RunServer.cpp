//Alex Koumandarakis
//CS 3505
//RunServer.cpp

//Holds the main function that creates and starts the server

#include <iostream>
#include <string>
//#include <boost/bind.hpp>
#include <boost/asio.hpp>
#include <boost/shared_ptr.hpp>
#include <boost/enable_shared_from_this.hpp>
#include "Server.h"
#include "ClientConnection.h"

//To compile: use makefile
//                 (make - compile server)
//                 (make run - compile and run the server)
//                 (make test - compile the test files)
//                 (make debug - compiles the server with debugging symbols)
//                 (make clean - complies the server and erases all .ss files)

int main()
{
  try
    {
      //Create the io_service and endpoint for the server and all tcp communications (port 2112)
      boost::asio::io_service io_serv;
      boost::asio::ip::tcp::endpoint endP(boost::asio::ip::tcp::v4(), 2112);
      
      //Create the server
      Server server(io_serv, endP);
      
      //Start the io_service
      io_serv.run();
    }
  catch (std::exception& e)
    {
      std::cout << "Exception : " << e.what() << std::endl;
    }

  return 0;
}
