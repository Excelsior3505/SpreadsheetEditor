#ifndef DPGRAPF_H
#define DPGRAPF_H

#include <set>
#include <map>
#include <utility>
#include <string>
class dpgraph
{
    private:

std::map <std::string, std:: set <std::string> > keys;

public:
dpgraph();
int get_size();
int get_dependees_count(std::string key);
bool has_dependents(std::string key);
bool has_dependees(std::string key);
std::set <std::string> get_dependents (std::string key);
std::set <std::string> get_dependees (std::string key);
int add_dependency(std::string s, std::string t);
void remove_dependency(std::string s, std::string t);
void replace_dependents(std::string s, std::set <std::string> newdependents);

void replace_dependees(std::string s, std::set <std::string> newdependees);

};
#endif