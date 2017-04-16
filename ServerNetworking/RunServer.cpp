#include <iostream>
#include <string>
#include <boost/bind.hpp>
#include <boost/asio.hpp>
#include <boost/shared_ptr.hpp>
#include <boost/enable_shared_from_this.hpp>
#include "Server.h"
#include "ClientConnection.h"

//To compile:
//g++ ClientConnection.cpp Server.cpp RunServer.cpp /usr/local/lib/libboost_system.a

int main()
{
  try
    {
      boost::asio::io_service io_serv;
      boost::asio::ip::tcp::endpoint endP(boost::asio::ip::tcp::v4(), 2112);
      
      Server server(io_serv, endP);
      
      io_serv.run();
    }
  catch (std::exception& e)
    {
      std::cout << "Exception : " << e.what() << std::endl;
    }

  return 0;
}
