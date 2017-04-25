//Tristan Willis
//CS 3505
//baseSS.cpp

//Stores spreadsheet information on the server

#include <boost/filesystem.hpp>
#include "baseSS.h"

//Creates the shared pointer for the spreadsheet object
base_ss::base_ss_ptr base_ss::create()
{
  return base_ss_ptr(new base_ss());
}

//Constructor
base_ss::base_ss()
{
}

//Destructor
base_ss::~base_ss()
{

}

//Returns the size of the spreadsheet (number of cells)
int base_ss::get_size()
{
  return spreadsheet.size();
}

//Returns the contents of the specified cell
std::string base_ss::get_contents(std::string key)
{
  return spreadsheet[key];
}

//Sets the contents of the specified cell to the given content
//Returns 1 if circular dependancy found, 0 if content is okay
int base_ss::set_cell(std::string key, std::string content)
{
  if(content == "")
    {
      //If clearing cell, clear dependants of cell
      dep_graph.replace_dependents(key, std::set<std::string>());
    }

  //If content is a formula
  if(content[0] == '=')
    {
      //Get the old content and dependants
      boost::to_upper(content);
      std::set <std::string> old_dependents=dep_graph.get_dependents(key);
      std::string old_content=get_contents(key);
      dep_graph.replace_dependents(key, std::set<std::string>());
      
      //Get the cells in the formula
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
		  //Add these cell names to the dependancy graph, return 1 if circular dependancy found
	         if(dep_graph.add_dependency(key, dependancy) == 1)
		        {
		          return 1;
		       } 
	        } 

	      }
      //If cell cannot be found
      if(spreadsheet.find(key) == spreadsheet.end())
        {
          spreadsheet.insert( std::pair<std::string, std::string> (key, ""));
        }
      //Add new contents, and check dependancies
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

//Check the dependancies of the given cell
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

//Used for iterating through dependancies of a cell
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

//Takes a filename, loads the data stored in that file into a base_ss object
void base_ss::loadSS(std::string fileName)
{
  //Make sure the fileName ends in .ss
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
  
  //If it exists, load it
  if(boost::filesystem::exists("../files/" + fileName))
    {
      std::ifstream loading(fileName);
      {
	boost::archive::text_iarchive in(loading);
	in >> spreadsheet;
      }
    }
}

//Saves the spreadsheet to the given fileName
void base_ss::saveSS(std::string saveName)
{
  //Make sure filename ends in .ss
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
  
  //Save the file
  std::ofstream saving(saveName);
  {
    boost::archive::text_oarchive out(saving);
    out << spreadsheet;
  }
}

//Change the file name of the spreadsheet
void base_ss::rename(std::string fileName)
{
  std::string before = "../files/" + name;
  std::string after = "../files/" + fileName;
  const char* beforeArr = before.c_str();
  const char* afterArr = after.c_str();
  std::rename(beforeArr, afterArr);
  name = fileName;
}
