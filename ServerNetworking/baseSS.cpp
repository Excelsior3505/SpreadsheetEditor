#include "baseSS.h"

base_ss::base_ss()
{
  set_cell("Version:", "1.0");
}

base_ss::~base_ss()
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

int base_ss::set_cell(std::string key, std::string content)
{
  if(content == "")
    {
      dep_graph.replace_dependents(key, std::set<std::string>());
    }
  if(content[0] == '=')
    {
      boost::to_upper(content);
      for(std::string::iterator i = content.begin(); i != content.end(); i++)
	{
	  if(*i <= 'Z' && *i >= 'A')
	    {
	      std::string dep = "" + *i;
	      std::string::iterator t = i;
	      t++;
	      while(*t <= '9' && *t >= '0')
		{
		  dep += *t;
		}
	      if(dep_graph.add_dependency(key, dep) == 1)
		{
		  return 1;
		}
	    }
	}
    }
  else
    {
      dep_graph.replace_dependents(key, std::set<std::string>());
    }
  if(spreadsheet.count(key) != 1)
    {
      spreadsheet.insert( std::pair<std::string, std::string> (key, ""));
    }
  spreadsheet[key] = content;
  return 0;
}

void base_ss::loadSS(std::string fileName)
{
  std::ifstream loading(fileName);
  {
    boost::archive::text_iarchive in(loading);
    in >> spreadsheet;
  }
}

void base_ss::saveSS(std::string saveName)
{
  std::ofstream saving(saveName);
  {
    boost::archive::text_oarchive out(saving);
    out << spreadsheet;
  }
}
