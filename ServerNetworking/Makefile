.phony: all run debug test clean

all:
	g++ -std=c++11 baseSS.cpp dpgraph.cpp ClientConnection.cpp Server.cpp RunServer.cpp -pthread /usr/local/lib/libboost_system.a /usr/local/lib/libboost_filesystem.a /usr/local/lib/libboost_serialization.a

run:
	g++ -std=c++11 -pthread baseSS.cpp dpgraph.cpp ClientConnection.cpp Server.cpp RunServer.cpp -pthread /usr/local/lib/libboost_system.a /usr/local/lib/libboost_filesystem.a /usr/local/lib/libboost_serialization.a
	./a.out

debug:
	g++ -g -std=c++11 baseSS.cpp dpgraph.cpp ClientConnection.cpp Server.cpp RunServer.cpp -pthread /usr/local/lib/libboost_system.a /usr/local/lib/libboost_filesystem.a /usr/local/lib/libboost_serialization.a

test:
	g++ -g -std=c++11 baseSS.cpp dpgraph.cpp basesstest.cpp -pthread /usr/local/lib/libboost_system.a /usr/local/lib/libboost_filesystem.a /usr/local/lib/libboost_serialization.a

clean:
	rm /../files/*.ss
	g++ -std=c++11 baseSS.cpp dpgraph.cpp ClientConnection.cpp Server.cpp RunServer.cpp -pthread /usr/local/lib/libboost_system.a /usr/local/lib/libboost_filesystem.a /usr/local/lib/libboost_serialization.a
