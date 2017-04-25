#include <boost/filesystem.hpp>
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
	      
	          std::string dependancy(dep.begin(), dep.end());
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
      if(check_dependency(key)==1)
      {
        dep_graph.replace_dependents(key,old_dependents);
        spreadsheet[key]=old_content;
        return 1;
      }
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

int base_ss::check_dependency(std::string key)
{
 
  std::set<std::string> visited;
  int code;
  if (visited.find(key)==visited.end())
  {
    code+=visit(key,key,visited);
  }
  return code;
}
int base_ss::visit(std::string start, std::string name, std::set<std::string>visited)
{
  visited.insert(name);
  std::set<std::string> dependees=dep_graph.get_dependees(name);
  std::set<std::string> ::iterator it;
  for(it=dependees.begin(); it!=dependees.end();it++)
  {
    if(*it==start)
    {
      return 1;
    }
    if(visited.find(*it)==visited.end())
    {
    visit(start,*it,visited);
    }
  }
return 0;
}

void base_ss::loadSS(std::string fileName)
{
  
  std::string hasSS = "";
  size_t pos = fileName.find_last_of(".");
  if(pos != std::string::npos)
    {
      hasSS = fileName.substr(pos);
      std::cout << "hasSS: " << hasSS << std::endl;
      hasSS.erase(std::remove(hasSS.begin(), hasSS.end(), ' '), hasSS.end());
      if(hasSS != ".ss")
	fileName = fileName + ".ss";
    }
  else
    fileName = fileName + ".ss";
  
  if(boost::filesystem::exists("../files/" + fileName))
    {
      std::ifstream loading(fileName);
      {
	boost::archive::text_iarchive in(loading);
	in >> spreadsheet;
      }
    }
}

void base_ss::saveSS(std::string saveName)
{
  std::string hasSS = "";
  size_t pos = saveName.find_last_of(".");
  if(pos != std::string::npos)
    {
      hasSS = saveName.substr(pos);
      std::cout << "hasSS: " << hasSS << std::endl;
      hasSS.erase(std::remove(hasSS.begin(), hasSS.end(), ' '), hasSS.end());
      if(hasSS != ".ss")
	saveName = saveName + ".ss";
    }
  else
    saveName = saveName + ".ss";
  
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
