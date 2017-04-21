#ifndef BASESS_H
#define BASESS_H

#include <map>
#include <set>
#include <string>

class base_ss
{
 private:

  std::map <std::string, std::string> spreadsheet;

 public:

   base_ss();
 int get_size();
 std::string get_contents(std::string key);
 void set_cell(std::string key, std::string content);
};

#endif
