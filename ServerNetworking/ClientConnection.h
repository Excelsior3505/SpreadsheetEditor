#ifndef CLIENTCONNECTION_H
#define CLIENTCONNECTION_H

#include <string>
#include <queue>
#include <boost/asio.hpp>
#include <boost/enable_shared_from_this.hpp>
#include <boost/shared_ptr.hpp>

class ClientConnection
: public boost::enable_shared_from_this<ClientConnection>
{
 public:
  typedef boost::shared_ptr<ClientConnection> cc_ptr;

  int connectionID;
  boost::asio::streambuf in_stream_buf;
  std::queue<std::string> message_queue;
  std::queue<std::string> incoming_message_queue;
  boost::asio::ip::tcp::socket skt;
  boost::asio::streambuf in_stream_buf;

  ClientConnection(boost::asio::io_service& io_serv, int ID);
  
  static cc_ptr create(boost::asio::io_service& io_serv, int ID);
  void start_waiting_for_message();
  void receive_message_loop(const boost::system::error_code& error);
  void send(const std::string message);
  void handle_send(const boost::system::error_code & error, std::size_t size);
  boost::asio::ip::tcp::socket& get_socket();
};

#endif
