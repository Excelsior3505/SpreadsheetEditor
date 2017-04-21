#include "baseSS.h"

base_ss::base_ss()
{

}

int base_ss::get_size()
{
  return spreadsheet.size();
}

std::string base_ss::get_contents(std::string key)
{
  return spreadsheet[key];
}

void base_ss::set_cell(std::string key, std::string content)
{
  if(spreadsheet.count(key) != 1)
    {
      spreadsheet.insert( std::pair<std::string, std::string> (key, ""));
    }
  spreadsheet[key] = content;
}
