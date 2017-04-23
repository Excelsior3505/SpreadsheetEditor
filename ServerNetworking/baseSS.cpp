#include "baseSS.h"

base_ss::base_ss_ptr base_ss::create()
{
  return base_ss_ptr(new base_ss());
}

base_ss::base_ss()
{
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
      std::set <std::string> old_dependents=dep_graph.get_dependents(key);
      std::string old_content=get_contents(key);
      dep_graph.replace_dependents(key, std::set<std::string>());
      for(int i = 0; i < content.size(); i++)
	      { 
	        char c = content[i];
	        if(c <= 'Z' && c >= 'A')
	        {
	          std::vector<char> dep;
	          dep.push_back(c);
	          i++;
	          c = content[i];
	          while(c <= '9' && c >= '0' && i < content.size())
		          {
		            dep.push_back(c);
		            i++;
		            c = content[i];
		          }
	      
	          //std::string dependancy(dep.begin(), dep.end());
	         // std::cout << "Dep: " <<  dependancy << std::endl;
	          if(dep_graph.add_dependency(key, dependancy) == 1)
		          {
		            return 1;
		          } 
	        } 

	      }
      if(spreadsheet.find(key) == spreadsheet.end())
        {
          spreadsheet.insert( std::pair<std::string, std::string> (key, ""));
        }
      spreadsheet[key] = content;
      
      return 0;
    }
  else
    {
      dep_graph.replace_dependents(key, std::set<std::string>());
    }
  if(spreadsheet.find(key) == spreadsheet.end())
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

void base_ss::rename(std::string fileName)
{
  std::string before = "../files/" + name;
  std::string after = "../files/" + fileName;
  const char* beforeArr = before.c_str();
  const char* afterArr = after.c_str();
  std::rename(beforeArr, afterArr);
  name = fileName;
}
