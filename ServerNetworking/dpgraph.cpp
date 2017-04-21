#include "dpgraph.h"
dpgraph::dpgraph()
{

 
}



int dpgraph::get_size()
{
    int size=0;
    std::map <std::string, std:: set <std::string> > ::iterator it;
    for (it=keys.begin();it!=keys.end();it++)
    {
        size+=it->second.size();

    } 
    return size;
}
int dpgraph::get_dependees_count(std::string key)
{
    int count=0;
    std::map <std::string, std:: set <std::string> > ::iterator it;
   
    for (it=keys.begin();it!=keys.end();it++)
    {
        if(it->second.find(key)!=it->second.end())
        {
            count++;
        }

    } 
    return count;
}
bool dpgraph::has_dependents(std::string key)
{
    std::map <std::string, std:: set <std::string> > ::iterator it;
   
    for (it=keys.begin();it!=keys.end();it++)
    {
        if(it->first==key&&it->second.size()!=0)
        {
            return true;
        }

    } 
    return false;
}
bool dpgraph::has_dependees(std::string key)
{
    if(get_dependees_count(key)==0)
    {
        return false;
    }
    return true;
}
std::set <std::string> dpgraph::get_dependents (std::string key)
{
    std::set<std::string> result;
    if(keys.find(key)==keys.end())
    {
        return result;
    }
    return keys[key];

   
    
}
std::set <std::string> dpgraph::get_dependees (std::string key)
{
    std::set<std::string> result;
    std::map <std::string, std:: set <std::string> > ::iterator it;
   
    for (it=keys.begin();it!=keys.end();it++)
    {
        if(it->second.find(key)!=it->second.end())
        {
            result.insert(it->first);
        }

    } 
    return result;
}
int dpgraph::add_dependency(std::string s, std::string t)
{
    if(keys.find(t)!=keys.end()&&keys[t].find(s)!=keys[t].end())
    {
        return 1; //circular depency check
    }
    if(keys.find(s)!=keys.end()&&keys[s].find(t)==keys[s].end())
    {
        keys[s].insert(t);
        return 0;
    }
    if(keys.find(s)!=keys.end()&&keys[s].find(t)!=keys[s].end())
    {
        return 0;
    }
    else
    {
        std::set<std::string> value;
        value.insert(t);
        keys[s]=value;
        return 0;
    }
}
void dpgraph::remove_dependency(std::string s, std::string t)
{
if(keys.find(s)!=keys.end()&&keys[s].find(t)!=keys[s].end())
    {
        keys[s].erase(t);
    } 
    
}
void dpgraph::replace_dependents(std::string s, std::set <std::string> newdependents)
{
if(keys.find(s)!=keys.end())
    {
        keys[s].clear();
        keys[s]=newdependents;
    } 

}

void dpgraph::replace_dependees(std::string s, std::set <std::string> newdependees)
{  
    std::map <std::string, std:: set <std::string> > ::iterator it;
   
    for (it=keys.begin();it!=keys.end();it++)
    {
        if(it->second.find(s)!=it->second.end())
        {
            it->second.erase(s);
        }
    }
     std:: set <std::string> ::iterator it2;
     for(it2=newdependees.begin();it2!=newdependees.end();it2++)
     {
         if(keys.find(*it2)!=keys.end()&&keys[*it2].find(s)!=keys[*it2].end())
         {
             keys[*it2].insert(s);
         }
         if (keys.find(*it2)==keys.end())
         {
             add_dependency(*it2,s);
         }
     }

}
