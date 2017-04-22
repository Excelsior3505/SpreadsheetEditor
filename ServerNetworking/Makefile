.phony: all run

all:
	g++ -std=c++11 baseSS.cpp dpgraph.cpp ClientConnection.cpp Server.cpp RunServer.cpp -pthread /usr/local/lib/libboost_system.a /usr/local/lib/libboost_filesystem.a /usr/local/lib/libboost_serialization.a

run:
	g++ -std=c++11 ClientConnection.cpp Server.cpp RunServer.cpp -pthread /usr/local/lib/libboost_system.a /usr/local/lib/libboost_filesystem.a
	./a.out
