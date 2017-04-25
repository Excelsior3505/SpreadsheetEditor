using System;
using System.Collections.Generic;
using SpreadsheetUtilities;
using System.Text.RegularExpressions;
using System.Xml;
using System.IO;
using System.Globalization;


//--------------------------------branch of PS6--------------------------------------

// Sharon Xiao(Rong Xiao)
// u0943650

// Linxi Li
// u1016104

// date of commenting: 10/30/2016

/// <summary>
///  not to modify AbstractSpreadsheet.cs (except to add comments where they are needed).
/// </summary>
namespace SS
{
    // PARAGRAPHS 2 and 3 modified for PS5.
    /// <summary>
    /// An AbstractSpreadsheet object represents the state of a simple spreadsheet.  A 
    /// spreadsheet consists of an infinite number of named cells.
    /// 
    /// A string is a cell name if and only if it consists of one or more letters,
    /// followed by one or more digits AND it satisfies the predicate IsValid.
    /// For example, "A15", "a15", "XY032", and "BC7" are cell names so long as they
    /// satisfy IsValid.  On the other hand, "Z", "X_", and "hello" are not cell names,
    /// regardless of IsValid.
    /// 
    /// Any valid incoming cell name, whether passed as a parameter or embedded in a formula,
    /// must be normalized with the Normalize method before it is used by or saved in 
    /// this spreadsheet.  For example, if Normalize is s => s.ToUpper(), then
    /// the Formula "x3+a5" should be converted to "X3+A5" before use.
    /// 
    /// A spreadsheet contains a cell corresponding to every possible cell name.  
    /// In addition to a name, each cell has a contents and a value.  The distinction is
    /// important.
    /// 
    /// The contents of a cell can be (1) a string, (2) a double, or (3) a Formula.  If the
    /// contents is an empty string, we say that the cell is empty.  (By analogy, the contents
    /// of a cell in Excel is what is displayed on the editing line when the cell is selected.)
    /// 
    /// In a new spreadsheet, the contents of every cell is the empty string.
    ///  
    /// The value of a cell can be (1) a string, (2) a double, or (3) a FormulaError.  
    /// (By analogy, the value of an Excel cell is what is displayed in that cell's position
    /// in the grid.)
    /// 
    /// If a cell's contents is a string, its value is that string.
    /// 
    /// If a cell's contents is a double, its value is that double.
    /// 
    /// If a cell's contents is a Formula, its value is either a double or a FormulaError,
    /// as reported by the Evaluate method of the Formula class.  The value of a Formula,
    /// of course, can depend on the values of variables.  The value of a variable is the 
    /// value of the spreadsheet cell it names (if that cell's value is a double) or 
    /// is undefined (otherwise).
    /// 
    /// Spreadsheets are never allowed to contain a combination of Formulas that establish
    /// a circular dependency.  A circular dependency exists when a cell depends on itself.
    /// For example, suppose that A1 contains B1*2, B1 contains C1*2, and C1 contains A1*2.
    /// A1 depends on B1, which depends on C1, which depends on A1.  That's a circular
    /// dependency.
    /// </summary>
    public class Spreadsheet : AbstractSpreadsheet
    {
        // ---------------properties, constructors, and methods for cell class--------------------

        /// <summary>
        /// Private class that build cell object for later use
        /// 
        /// A string is a cell name if and only if it consists of one or more letters,
        /// followed by one or more digits AND it satisfies the predicate IsValid.
        /// For example, "A15", "a15", "XY032", and "BC7" are cell names so long as they
        /// satisfy IsValid.  On the other hand, "Z", "X_", and "hello" are not cell names,
        /// regardless of IsValid.
        /// 
        /// Any valid incoming cell name, whether passed as a parameter or embedded in a formula,
        /// must be normalized with the Normalize method before it is used by or saved in 
        /// this spreadsheet.  For example, if Normalize is s => s.ToUpper(), then
        /// the Formula "x3+a5" should be converted to "X3+A5" before use.
        /// </summary>
        private class Cell
        {
            /// <summary>
            /// the name of the cell
            /// </summary>
            public String Name { get; private set; }

            /// <summary>
            /// the type of the cell(string/ double/ formula)
            /// </summary>
            private object _contents;

            // initial the contents, if the content is a formula, evalue it and set to the content
            public object Contents
            {
                get { return _contents; }
                set
                {
                    _value = value;
                    try
                    {
                        if (value is Formula)
                        {
                            _value = (_value as Formula).Evaluate(Lookup);
                        }
                    }
                   
                    finally
                    {
                                            _contents = value;
                    }
                }
            }

            // holds the value of the cell
            private object _value;

            // initialize the value
            public object Value
            {
                get { return _value; }
                private set { _value = value; }
            }

            // the lookup delegates
            public Func<string, double> Lookup { get; private set; }

            /// <summary>
            /// Cell class constructor, crates a cell that contains three properties
            /// </summary>
            /// <param name="Name"> the name of the cell</param>
            /// <param name="Contents"> the contents of the cell(string/ double/ formula)</param>
            /// <param name="Lookup">the lookup delegate</param>
            public Cell(string _name, object _contents, Func<string, double> _lookup)
            {
                Name = _name;

                Lookup = _lookup;
                Contents = _contents;

            }
            public Cell(string _name, object _contents)
            {
                Name = _name;

                Lookup = null;
                Contents = _contents;

            }
        }

        // -----------properties, constructors, and methods for spreadsheet class--------------

        /// <summary>
        /// initial DependencyGraph and named myDG
        /// (stands for my dependency graph)
        /// </summary>
        private DependencyGraph myDG;

        /// <summary>
        /// initial my spreadsheet type with dictionary and named mySS 
        /// (stands for my spread sheet)
        /// </summary>
        private Dictionary<string, Cell> mySS;

        // ADDED FOR PS5
        /// <summary>
        /// True if this spreadsheet has been modified since it was created or saved                  
        /// (whichever happened most recently); false otherwise.
        /// </summary>
        public override bool Changed
        { get; protected set; }

        /// <summary>
        /// zero-argument constructor should create an empty spreadsheet that imposes no 
        /// extra validity conditions, normalizes every cell name to itself, 
        /// and has version "default".
        /// </summary>
        public Spreadsheet() : base(s => true, s => s, "default")
        {
            Changed = false;
            myDG = new DependencyGraph();
            mySS = new Dictionary<string, Cell>();
        }

        /// <summary>
        /// A three-argument constructor to the Spreadsheet class. Just like the
        /// zero-argument constructor, it should create an empty spreadsheet. However, 
        /// it should allow the user to provide a validity delegate (first parameter), 
        /// a normalization delegate (second parameter), and a version (third parameter).
        /// </summary>
        /// <param name="Isvalid">the delegate of validation</param>
        /// <param name="Normalize">the delegate of normalization</param>
        /// <param name="Version">the version of the spreadsheet</param>
        public Spreadsheet(Func<string, bool> Isvalid, Func<string, string> Normalize,
            string Version) : base(Isvalid, Normalize, Version)
        {
            Changed = false;
            mySS = new Dictionary<string, Cell>();
            myDG = new DependencyGraph();
        }


        /// <summary>
        /// A four-argument constructor to the Spreadsheet class. It should 
        /// allow the user to provide a string representing a path to a file (first parameter), 
        /// a validity delegate (second parameter), a normalization delegate (third parameter), 
        /// and a version (fourth parameter). It should read a saved spreadsheet from a file 
        /// (see the Save method) and use it to construct a new spreadsheet. 
        /// The new spreadsheet should use the provided validity delegate, normalization delegate, 
        /// and version.
        /// </summary>
        /// <param name="FilePath">the target path of the file</param>
        /// <param name="Isvalid">the delegate of validation</param>
        /// <param name="Normalize">the delegate of normalization</param>
        /// <param name="Version">the version of the spreadsheet</param>
        public Spreadsheet(string FilePath, Func<string, bool> Isvalid, Func<string, string> Normalize,
            string Version) : base(Isvalid, Normalize, Version)
        {
            Changed = false;
            mySS = new Dictionary<string, Cell>();
            myDG = new DependencyGraph();

            // inplement filepath initial
            // if the version that user put in is not equal to the vertion tracked 
            // through the filepath, throw SpreadsheetReadWriteException with information says
            // "version not match"
            if (Version != GetSavedVersion(FilePath))
            {
                throw new SpreadsheetReadWriteException("version not match");
            }
            // otherwise, using ReadFile function to read through the FilePath xml file.
            ReadFile(FilePath);
        }


        /// <summary>
        /// a private helper function to find the path of a file
        /// </summary>
        /// <param name="filename">the name of the file</param>
        private void ReadFile(string filename)
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            {
                // set IgnoreComments, IgnoreProcessingInstructions, and IgnoreWhitespace to truw
                settings.IgnoreComments = true;
                settings.IgnoreProcessingInstructions = true;
                settings.IgnoreWhitespace = true;
            }
            // implement xmlreader by using "using", create a xml reader for later use
            using (XmlReader reader = XmlReader.Create(filename, settings))
            {
                // create an empty string of cell name
                string name = "";
                // create an empty string of cell content
                string cont = "";
                // while reading

                while (reader.Read())
                {
                    // if the reader reads startelement, switch to the case if match the conditon
                    if (reader.IsStartElement())
                    {
                        // swithc in different contitions
                        switch (reader.Name)
                        {
                            // if the reader node is "spreadsheet", make the version eqaul 
                            // the "version" and break
                            case "spreadsheet":
                                Version = reader["version"];
                                break;

                            // if the reader node if "cell", reader reads the current element 
                            // and return to name and cont
                            case "cell":
                                reader.Read();
                                name = reader.ReadElementContentAsString();
                                cont = reader.ReadElementContentAsString();
                                //use the returned name and content to SetContentsOfCell
                                // by using the method.
                                SetContentsOfCell(name, cont);
                                break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// a private helper function that test if the input is
        /// a valid string for the cell name
        /// </summary>
        /// <param name="key">input, which is the cell name</param>
        /// <returns></returns>
        private bool IsValidps4(string key)
        {
            //return true if the input matches the regex, otherwise return false
            if (Regex.IsMatch(key, @"[a-zA-Z]+\d+"))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// If name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, returns the contents (as opposed to the value) of the named cell.  The return
        /// value should be either a string, a double, or a Formula.
        public override object GetCellContents(string name)
        {
            Normalize(name);
            Regex check = new Regex(@"^[a-zA-Z_](?: [a-zA-Z_]|\d)*");
            // if the name is null or the name is not valid, throw exception
            if (name == null || check.IsMatch(name = Normalize(name)) == false)
            {
                throw new InvalidNameException();
            }

            // if myss does not contain the key, return an empty string because blank cell should be "";
            if (!mySS.ContainsKey(name))
            {
                return "";
            }

            // otherwise, look up the name in myss dictionary, return corresponding value
            return mySS[name].Contents;
        }

        /// <summary>
        /// Enumerates the names of all the non-empty cells in the spreadsheet.
        /// </summary>
        public override IEnumerable<string> GetNamesOfAllNonemptyCells()
        {
            // foreach key value pair in mySS
            foreach (KeyValuePair<string, Cell> kvp in mySS)
            {
                // if the tyoe is not an empty string, yeild return the key(name)
                object empStr = "";
                if (kvp.Value.Contents != empStr)
                {
                    yield return kvp.Key;
                }
            }
        }


        /// <summary>
        /// the lookup delegate wrapped function that returns a double basicalli check if 
        /// the input name's corresponding content is a double or not.
        /// If is, return the parsed double, if not, throw ArgumentException that indicate 
        /// value is not a double.
        /// </summary>
        /// <param name="name">the cell name</param>
        /// <returns></returns>
        private double my_Lookup(string name)
        {
            Normalize(name);
            // if spreadsheet contains the name key, create an object that's equal to the 
            // contents of the cell.

            if (mySS.ContainsKey(name))
            {
                object getValue = mySS[name].Value;

                //if the parsed content is a double, return the double
                if (getValue is double)
                {
                    return (double)getValue;
                }
                
            }
            if (!mySS.ContainsKey(name))
            {
                 if(Regex.IsMatch(name, @"^([a-zA-Z]){1}([1-9]\d{0,1})$"))
                 {
                return 0;
                }

            }
            throw new InvalidNameException();
        }


        /// <summary>
        /// If the formula parameter is null, throws an ArgumentNullException.
        /// 
        /// Otherwise, if name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, if changing the contents of the named cell to be the formula would cause a 
        /// circular dependency, throws a CircularException.  (No change is made to the spreadsheet.)
        /// 
        /// Otherwise, the contents of the named cell becomes formula.  The method returns a
        /// Set consisting of name plus the names of all other cells whose value depends,
        /// directly or indirectly, on the named cell.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// set {A1, B1, C1} is returned.
        /// </summary>
        protected override ISet<string> SetCellContents(string name, Formula formula)
        {

            Normalize(name);
            // if changing the contents of the named cell to be the formula would cause a 
            // circular dependency, throws a CircularException.  (No change is made to the spreadsheet.)
            IEnumerable<string> dents = myDG.GetDependents(name);
            myDG.ReplaceDependents(name, new HashSet<string>());
            string x = formula.ToString();
            x = x.ToUpper();
            Formula newformula = new Formula(x);
            formula = newformula;
            foreach (string var in formula.GetVariables())
                
            {
                Normalize(var);
                myDG.AddDependency(name, var);
                
            }

            // if the ss has had the cell name, replace
            if (mySS.ContainsKey(name))
            {
                Normalize(name);
                                mySS[name].Contents = newformula;
            }

            // otherwise, add this name,cell key value pair to ss
            if (!mySS.ContainsKey(name))
            {
                Normalize(name);
                Cell theCell = new Cell(name, formula, my_Lookup);
                mySS.Add(name, theCell);
            }

            // iterate through the ss by GetCellsToRecalculate and make sure there is no 
            // CircularException
            foreach (string s in GetCellsToRecalculate(name))
            {
                Normalize(name);
                mySS[s].Contents = mySS[s].Contents;
            }

            // the ss has been changed, set changed to true.
            Changed = true;

            // iterate through the ss by GetCellsToRecalculate and make sure there is no CircularException, 
            // and The names are enumerated in the order in which the calculations should be done. 
            // create a new set to equal to the ordered set
            HashSet<string> set = new HashSet<string>(GetCellsToRecalculate(name));
            
            // add the name to the set and return
            set.Add(name);
            return set;
        }


        /// <summary>
        /// If text is null, throws an ArgumentNullException.
        /// 
        /// Otherwise, if name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, the contents of the named cell becomes text.  The method returns a
        /// set consisting of name plus the names of all other cells whose value depends, 
        /// directly or indirectly, on the named cell.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// set {A1, B1, C1} is returned.
        /// </summary>>(Get
        protected override ISet<string> SetCellContents(string name, string text)
        {

            Normalize(name);
            // Otherwise, the contents of the named cell becomes text.  The method returns a set consisting of 
            // name plus the names of all other cells whose value depends, directly or indirectly, 
            // on the named cell.

            // if my spreadsheet has had the cell name, replace the contents with the text
            if (mySS.ContainsKey(name))
            {
                mySS[name].Contents = text;
            }

            // if my spreadsheet does not contain name, add the cell that's from the user input
            if (!mySS.ContainsKey(name))
            {
                Cell theCell = new Cell(name, text, my_Lookup);
                mySS.Add(name, theCell);
            }

            // iterate through the ss by GetCellsToRecalculate and make sure there is no CircularException, 
            // and The names are enumerated in the order in which the calculations should be done. 
            myDG.ReplaceDependents(name, new HashSet<string>());

            foreach (string s in GetCellsToRecalculate(name))
            {
                mySS[s].Contents = mySS[s].Contents;
            }

            // the ss has been changed, set changed to true.
            Changed = true;
            // create a new set that will be return which use the 

            // GetDependees() method from dependencyGraph dll
            HashSet<string> set = new HashSet<string>(GetCellsToRecalculate(name));

            // add name and return the set
            set.Add(name);
            return set;
        }


        /// <summary>
        /// If name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, the contents of the named cell becomes number.  The method returns a
        /// set consisting of name plus the names of all other cells whose value depends, 
        /// directly or indirectly, on the named cell.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// set {A1, B1, C1} is returned.
        /// </summary>
        protected override ISet<string> SetCellContents(string name, double number)
        {


            Normalize(name);
            // Otherwise, the contents of the named cell becomes number.  The method returns a
            // set consisting of name plus the names of all other cells whose value depends, 
            // directly or indirectly, on the named cell.

            // if my spreadsheet has had the cell name, replace the contents with the number
            if (mySS.ContainsKey(name))
            {
                mySS[name].Contents = number;
            }

            // if my spreadsheet does not contain name, add the cell that's from the user input
            if (!mySS.ContainsKey(name))
            {
                Cell theCell = new Cell(name, number, my_Lookup);
                mySS.Add(name, theCell);
            }
            // the ss has been changed, set changed to true.
            Changed = true;

            // iterate through the ss by GetCellsToRecalculate and make sure there is no CircularException, 
            // and The names are enumerated in the order in which the calculations should be done. 
            myDG.ReplaceDependents(name, new HashSet<string>());
            foreach (string s in GetCellsToRecalculate(name))
            {
                mySS[s].Contents = mySS[s].Contents;
            }

            // create a new set that will be return which use the 
            // GetDependees() method from dependencyGraph dll
            HashSet<string> set = new HashSet<string>(GetCellsToRecalculate(name));

            // add name and return the set
            set.Add(name);
            return set;
        }


        /// <summary>
        /// If name is null, throws an ArgumentNullException.
        /// 
        /// Otherwise, if name isn't a valid cell name, throws an InvalidNameException.
        /// 
        /// Otherwise, returns an enumeration, without duplicates, of the names of all cells whose
        /// values depend directly on the value of the named cell.  In other words, returns
        /// an enumeration, without duplicates, of the names of all cells that contain
        /// formulas containing name.
        /// 
        /// For example, suppose that
        /// A1 contains 3
        /// B1 contains the formula A1 * A1
        /// C1 contains the formula B1 + A1
        /// D1 contains the formula B1 - C1
        /// The direct dependents of A1 are B1 and C1
        /// </summary>
        protected override IEnumerable<string> GetDirectDependents(string name)
        {
            Normalize(name);
            // If name is null, throws an ArgumentNullException.
            if (name == null)
            {
                throw new ArgumentNullException();
            }

            // Otherwise, if name isn't a valid cell name, throws an InvalidNameException.
            else if (IsValidps4(name) == false)
            {
                throw new InvalidNameException();
            }

            // Otherwise, returns an enumeration, without duplicates, of the names of all cells whose
            // values depend directly on the value of the named cell.  In other words, returns
            // an enumeration, without duplicates, of the names of all cells that contain
            // formulas containing name.
            return myDG.GetDependees(name);
        }


        // ADDED FOR PS5
        /// <summary>
        /// Returns the version information of the spreadsheet saved in the named file.
        /// If there are any problems opening, reading, or closing the file, the method
        /// should throw a SpreadsheetReadWriteException with an explanatory message.
        /// </summary>
        public override string GetSavedVersion(string filename)
        {
            // try to use XmlReader. Create an XmlReader with the filename by using "using". 
            try
            {
                // make XmlReaderSettings to be able to read formalized xml file
                XmlReaderSettings settings = new XmlReaderSettings();
                {
                    // set IgnoreComments, IgnoreProcessingInstructions, and IgnoreWhitespace to truw
                    settings.IgnoreComments = true;
                    settings.IgnoreProcessingInstructions = true;
                    settings.IgnoreWhitespace = true;
                }
                using (XmlReader xmlReader = XmlReader.Create(filename, settings))
                {

                    while (xmlReader.Read())
                    {
                        if (xmlReader.IsStartElement())
                        {
                            switch (xmlReader.Name)
                            {
                                // if the current node is "spreadsheet", read "version" and return the 
                                // corresponding version string in the xml file.
                                case "spreadsheet":
                                    return xmlReader["version"];

                                // defualt case: throw a SpreadsheetReadWrite Exception says
                                default:
                                    throw new SpreadsheetReadWriteException
                                        ("cannot get the version of the spreadsheet");
                            }
                        }
                    }
                }
                throw new Exception();
            }
            // catch other exceptions
            catch (Exception exception)
            { 
                // otherwise, in other exception case, throw SpreadsheetReadWriteException
                // with message "unknow error"
                throw new SpreadsheetReadWriteException("unknow error"+ exception.Message);
            }
        }


        // ADDED FOR PS5
        /// <summary>
        /// Writes the contents of this spreadsheet to the named file using an XML format.
        /// The XML elements should be structured as follows:
        /// 
        /// <spreadsheet version="version information goes here">
        /// 
        /// <cell>
        /// <name>
        /// cell name goes here
        /// </name>
        /// <contents>
        /// cell contents goes here
        /// </contents>    
        /// </cell>
        /// 
        /// </spreadsheet>
        /// 
        /// There should be one cell element for each non-empty cell in the spreadsheet.  
        /// If the cell contains a string, it should be written as the contents.  
        /// If the cell contains a double d, d.ToString() should be written as the contents.  
        /// If the cell contains a Formula f, f.ToString() with "=" prepended should be written as the contents.
        /// 
        /// If there are any problems opening, writing, or closing the file, the method should throw a
        /// SpreadsheetReadWriteException with an explanatory message.
        /// </summary>
        public override void Save(string filename)
        {
            try
            {
                // make XmlReaderSettings to be able to write formalized xml file
                XmlWriterSettings settings = new XmlWriterSettings()
                {
                    // indent must be set to true to set IndentChars and NewLineOnAttributes
                    Indent = true,
                    // set the indent char to be ""
                    IndentChars = "",
                    NewLineOnAttributes = true
                };
                // using xmlWriter, crate an xmlWriter with the filename and the settings
                using (XmlWriter xmlWriter = XmlWriter.Create(filename, settings))
                {
                    // start write
                    xmlWriter.WriteStartDocument();
                    // WriteStartElement spreadsheet
                    xmlWriter.WriteStartElement("spreadsheet");
                    // WriteAttributeString with the version user inputed
                    xmlWriter.WriteAttributeString("version", Version);

                    // write cell name, and cell content
                    foreach (Cell cell in mySS.Values)
                    {
                        // write elements.
                        xmlWriter.WriteStartElement("cell");
                        xmlWriter.WriteElementString("name", cell.Name);
                        xmlWriter.WriteElementString("contents", Write(cell.Contents));
                        // end writing cell name and contents
                        xmlWriter.WriteEndElement();
                    }
                    // end writing spreadsheet
                    xmlWriter.WriteEndElement();
                    // end writing the xml file
                    xmlWriter.WriteEndDocument();
                    // releases all resources used by the current instance of the XmlWriter class.
                    xmlWriter.Dispose();
                }
            }
            // If there are any problems opening, writing, or closing the file, the method should throw a
            // SpreadsheetReadWriteException with an explanatory message.
            catch { throw new SpreadsheetReadWriteException("cannot wirte to the file"); }
        }

        /// <summary>
        /// An private helper function for xml writer to write the xml fileThere 
        /// should be one cell element for each non-empty cell in the spreadsheet.  
        /// If the cell contains a string, it should be written as the contents.  
        /// If the cell contains a double d, d.ToString() should be written as the contents.  
        /// If the cell contains a Formula f, f.ToString() with "=" prepended should
        /// be written as the contents.
        /// </summary>
        /// <param name="cont">content of the cell</param>
        /// <returns></returns>
        private string Write(object cont)
        {
            // If the cell contains a string, it should be written as the contents.  
            if (cont is string)
                return cont as string;

            // If the cell contains a double d, d.ToString() should be written as the contents.
            if (cont is double)
                return cont.ToString();

            // If the cell contains a Formula f, f.ToString() with "=" prepended should
            if (cont is Formula)
                return "=" + cont.ToString();

            // otherwise, throw SpreadsheetReadWriteException with message "can not write cell"
            throw new SpreadsheetReadWriteException("can not write cell");
        }

        // ADDED FOR PS5
        /// <summary>
        /// If name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, returns the value (as opposed to the contents) of the named cell.  The return
        /// value should be either a string, a double, or a SpreadsheetUtilities.FormulaError.
        /// </summary>
        public override object GetCellValue(string name)
        {
            Normalize(name);
            //If name is null or invalid, throws an InvalidNameException.
            if (name == null || IsValidps4(name) == false || !IsValid(Normalize(name)))
            {
                throw new InvalidNameException();
            }

            // Otherwise, returns the value (as opposed to the contents) of the named cell.  The return
            // value should be either a string, a double, or a SpreadsheetUtilities.FormulaError.
            if (!mySS.ContainsKey(name))
            {
                // if the my ss does not contain the cell name, creat an empty cell content and return it
                string str = "";
                return str;
            }

            // else, return the corresponding cell content of the cell
            return mySS[name].Value;

        }


        // ADDED FOR PS5
        /// <summary>
        /// If content is null, throws an ArgumentNullException.
        /// 
        /// Otherwise, if name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, if content parses as a double, the contents of the named
        /// cell becomes that double.
        /// 
        /// Otherwise, if content begins with the character '=', an attempt is made
        /// to parse the remainder of content into a Formula f using the Formula
        /// constructor.  There are then three possibilities:
        /// 
        ///   (1) If the remainder of content cannot be parsed into a Formula, a 
        ///       SpreadsheetUtilities.FormulaFormatException is thrown.
        ///       
        ///   (2) Otherwise, if changing the contents of the named cell to be f
        ///       would cause a circular dependency, a CircularException is thrown.
        ///       
        ///   (3) Otherwise, the contents of the named cell becomes f.
        /// 
        /// Otherwise, the contents of the named cell becomes content.
        /// 
        /// If an exception is not thrown, the method returns a set consisting of
        /// name plus the names of all other cells whose value depends, directly
        /// or indirectly, on the named cell.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// set {A1, B1, C1} is returned.
        public override ISet<string> SetContentsOfCell(string name, string content)
        {
            Normalize(name);
            // If content is null, throws an ArgumentNullException.
            if (content == null)
                throw new ArgumentNullException();

            // Otherwise, if name is null or invalid, throws an InvalidNameException.
            if (name == null || !IsValidps4(name = Normalize(name)))
                throw new InvalidNameException();

            // Otherwise, if content parses as a double, the contents of the named
            // cell becomes that double.
            if (IsValid(name))
            {
                // if the content is an empty string, assign the set a new hashset with
                // make sure no CircularException, if yes, throw CircularException
                if (content == "")
                {
                    myDG.ReplaceDependents(name, new HashSet<string>());
                    return new HashSet<string>(GetCellsToRecalculate(name));
                }
                // Otherwise, if content begins with the character '=', an attempt is made
                // to parse the remainder of content into a Formula f using the Formula
                // constructor.  There are then three possibilities:
                if (content[0] == '=')
                {
                    Formula f = new Formula(content.Substring(1), Normalize, IsValid);
                    return SetCellContents(name, f);
                }
                double theDouble;
                // if the content is a double, assign the set a new hashset which using SetCellContents,
                // with parameter name and the parsed duble value
                try
                {
                    NumberStyles styles = NumberStyles.AllowExponent |
                        NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint;
                    theDouble = Double.Parse(content, styles);

                    return SetCellContents(name, theDouble);
                }
                //(3) Otherwise, the contents of the named cell becomes f.
                catch
                {
                    return SetCellContents(name, content);
                }
            }
            // otherwise, throw InvalidNameException
            throw new InvalidNameException();
        }
    }
}

