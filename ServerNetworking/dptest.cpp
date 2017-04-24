#include "dpgraph.h"
using namespace cs3505;
int main()
{
cs3505::dpgraph x;
/*
if(x.get_size()!=0){return 1;}
if(x.has_dependents("s")!=false|x.has_dependees("s")!=false){return 2;}
x.add_dependency("s","b");

if(x.get_size()!=1){return 10;}
if(x.has_dependents("s")!=true|x.has_dependees("b")!=true){return 3;}
x.add_dependency("s","b");
if(x.add_dependency("b","s")!=1){return 4;}
x.remove_dependency("s","b");
if(x.has_dependents("s")!=false|x.has_dependees("b")!=false){return 5;}
x.add_dependency("s","b");
x.add_dependency("k","b");*/
std::set<std::string> result;/*
result=x.get_dependees("b");
if(result.size()!=2|result.find("s")==result.end()|result.find("k")==result.end()){return 6;}
result=x.get_dependents("s");
if(result.size()!=1|result.find("b")==result.end()){return 7;}
if(x.get_dependees_count("b")!=2){return 7;}
std::set<std::string> newdees;
newdees.insert("a");
newdees.insert("c");
newdees.insert("b");

x.replace_dependees("b",newdees);
result=x.get_dependees("b");
if(result.size()!=3|result.find("a")==result.end()|result.find("c")==result.end()|result.find("b")==result.end()){return 8;}
x.replace_dependents("b",newdees);
result=x.get_dependents("b");
if(result.size()!=3|result.find("a")==result.end()|result.find("c")==result.end()|result.find("b")==result.end()){return 9;}
*/
return x.add_dependency("a","a");
//return 0;
}