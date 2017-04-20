.phony: all run

all:
	g++ -std=c++11 ClientConnection.cpp Server.cpp RunServer.cpp -pthread /usr/local/lib/libboost_system.a

run:
	g++ -std=c++11 ClientConnection.cpp Server.cpp RunServer.cpp -pthread /usr/local/lib/libboost_system.a
	./a.out
