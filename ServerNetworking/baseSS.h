#ifndef BASESS_H
#define BASESS_H

#include <map>
#include <vector>
#include <algorithm>
#include <set>
#include <string>
#include <fstream>
#include <boost/serialization/map.hpp>
#include <boost/archive/text_iarchive.hpp>
#include <boost/archive/text_oarchive.hpp>
#include <boost/algorithm/string.hpp>
#include <boost/enable_shared_from_this.hpp>
#include <boost/shared_ptr.hpp>
#include "dpgraph.h"

class base_ss
: public boost::enable_shared_from_this<base_ss>
{
 public:
typedef boost::shared_ptr<base_ss> base_ss_ptr;

std::map <std::string, std::string> spreadsheet;
static base_ss_ptr create();
base_ss();
 ~base_ss();
 int get_size();
 std::string get_contents(std::string key);
 int set_cell(std::string key, std::string content);
 void loadSS(std::string fileName);
 void saveSS(std::string saveName);
 void rename(std::string fileName);
 std::string name;
 std::vector< std::pair< std::string, std::string> > undo;
 std::vector< std::pair< std::string, std::string> > redo;
 cs3505::dpgraph dep_graph;
 int check_dependency(std::string key);
 int visit(std::string start, std::string name, std::set<std::string>visited);
};

#endif
