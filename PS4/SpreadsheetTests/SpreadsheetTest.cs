using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SS;
using System.Collections.Generic;
using SpreadsheetUtilities;
using System.IO;

namespace SpreadsheetTests
{

    // Sharon Xiao
    // u0943650

    // Linxi Li
    // u1016104

    // date of commenting: 11/3/2016

    //-------------------------------- ADDED TEST FOR PS6 -----------------------------------
    [TestClass]
    public class SpreadsheetTest
    {
        public Spreadsheet sheet;

        [TestInitialize]
        public void setup()
        {
            sheet = new Spreadsheet();
        }

        /// <summary>
        /// check GetCellContentsTest
        /// </summary>
        [TestMethod]
        public void GetCellContentsTest_ps6()
        {
            sheet.SetContentsOfCell("A1", "1.01");
            Assert.AreEqual(1.01, sheet.GetCellContents("A1"));
            sheet.SetContentsOfCell("B1", "idk");
            Assert.AreEqual("idk", sheet.GetCellContents("B1"));
            sheet.SetContentsOfCell("C1", "=D1+F1");
            Assert.AreEqual("D1+F1", sheet.GetCellContents("C1").ToString());
            sheet.SetContentsOfCell("E1", "!@#$%");
            Assert.AreEqual("!@#$%", sheet.GetCellContents("E1").ToString());

            Assert.AreEqual(4, new List<string>(sheet.GetNamesOfAllNonemptyCells()).Count);
        }
        /// <summary>
        /// check GetCellValueTest
        /// </summary>
        [TestMethod]
        public void GetCellValueTest_ps6()
        {
            sheet.SetContentsOfCell("A1", "!@#$%");
            Assert.AreEqual("!@#$%", sheet.GetCellValue("A1"));

            sheet.SetContentsOfCell("B1", "3.14");
            Assert.AreEqual(3.14, (double)sheet.GetCellValue("B1"));

            sheet.SetContentsOfCell("A1", "25.0");
            sheet.SetContentsOfCell("A2", "4.0");
            sheet.SetContentsOfCell("A3", "=A1*A2");
            Assert.AreEqual(100.0, (double)sheet.GetCellValue("A3"));
        }

        // The rest of the tests are still useful to test the spreadsheet functionality. Since the UItest
        // has problem right now, ignor the UItest part

        //-------------------------------- ADDED TEST FOR PS5 -----------------------------------
        //zero argument
        [TestMethod]
        public void ConstructorTest1()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
        }
        // three argument
        [TestMethod]
        public void ConstructorTest2()
        {
            AbstractSpreadsheet ss = new Spreadsheet(s => true, s => s, "ConstructorTest2");
        }

        //four argument
        [TestMethod]
        public void ConstructorTest3()
        {
            AbstractSpreadsheet ss = new Spreadsheet();

            ss.SetContentsOfCell("A1", "1.1");

            ss.Save("ss1.xml");

            ss = new Spreadsheet("ss1.xml", s => true, s => s, "default");
        }

        // GetSavedVersionTest
        [TestMethod]
        public void GetSavedVersionTest1()
        {
            AbstractSpreadsheet ss = new Spreadsheet();

            ss.GetSavedVersion("savetest1.xml");
        }

        //GetNamesOfAllNonemptyCellsTest1_1
        [TestMethod]
        public void GetNamesOfAllNonemptyCellsTest1_1()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            HashSet<string> set = new HashSet<string>();

            foreach (string cell in ss.GetNamesOfAllNonemptyCells())
            {
                set.Add(cell);
            }

            Assert.AreEqual(0, set.Count);
        }

        //FormulaTest1
        [TestMethod]
        public void FormulaTest1()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            List<string> a = new List<string>();
            List<string> b = new List<string>();

            a.Add("A1");
            a.Add("B1");
            a.Add("A3");

            ss.SetContentsOfCell("A1", "=A2 - B1");
            ss.SetContentsOfCell("B1", "=A3 + A2");

            foreach (string s in ss.SetContentsOfCell("A3", "=9+C1"))
            {
                Assert.IsTrue(a.Contains(s));
                b.Add(s);
                Console.WriteLine(s);
            }

            foreach (string s in a)
            {

                Assert.IsTrue(b.Contains(s));
            }
        }

        // FormulaTest2
        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void FormulaTest2()
        {
            {
                AbstractSpreadsheet ss = new Spreadsheet();

                ss.SetContentsOfCell("3.14X", "=A1 + A2");
            }
        }
        //FormulaTest3
        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void FormulaTest3()
        {
            {
                AbstractSpreadsheet ss = new Spreadsheet();

                ss.SetContentsOfCell(null, "=A1 + A2");
            }
        }

        //FormulaTest4
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FormulaTest4()
        {
            {
                AbstractSpreadsheet ss = new Spreadsheet();

                ss.SetContentsOfCell("A1", null);
            }
        }

        //FormulaTest5
        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void FormulaTest5()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            ss.GetCellContents("");
        }

        //FormulaTest6
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FormulaTest6()
        {
            AbstractSpreadsheet s = new Spreadsheet();

            s.SetContentsOfCell("A1", null);
        }

        //GetCellValueTest
        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void GetCellValueTest()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            string name = null;
            ss.GetCellValue(name);
        }

        //GetCellValueTest2
        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void GetCellValueTest2()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            string name = "$%";
            ss.GetCellValue(name);
        }
        //SaveTest1
        [TestMethod]
        public void SaveTest1()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            ss.Save("savetest1.xml");
        }

        //saveTest2
        [TestMethod]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void saveTest2()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            ss.Save("f:\\address\\test.xml");
        }


        //GetSavedVersionTest
        [TestMethod]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void GetSavedVersionTest()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            { 
                string ver= ss.GetSavedVersion("savetest1.xml"+"0");
            }
        }

        //GetSavedVersionTest2
        [TestMethod]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void GetSavedVersionTest2()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            {
                
                ss.GetSavedVersion("savetest2.xml" );
            }
        }

        //GetSavedVersionTest3
        [TestMethod]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void GetSavedVersionTest3()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            {

                ss.GetSavedVersion("savetest3.xml");
            }
        }


        //------------------------ TEST FROM PS4 THAT CAN BE USED---------------------------
        //--------------All SetContents() HAS BEEN CHANGED TO SetContentsOfCell()----------------


        //----------------------test for GetCellContents()-------------------------

        /// <summary>
        ///  This tests the GetCellContents method and see if it throw when
        ///  give the name a null input
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void InvalidNameTest()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            ss.GetCellContents(null);
        }

        /// <summary>
        ///  This tests the GetCellContents method and see if it throw when
        ///  give the name a invalid input
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void InvalidNameTest2()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            ss.GetCellContents("%");
        }

        /// <summary>
        /// GetCellContentsTest that test if it return "" with a not existed cell name
        /// </summary>
        [TestMethod]
        public void GetCellContentsTest()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            object right = "";
            Assert.IsTrue(ss.GetCellContents("haha") == right);
        }



        //----------------------test for GetNamesOfAllNonemptyCells()-------------------------

        /// <summary>
        /// test GetNamesOfAllNonemptyCells(). can not movenext if there is no cell in the dictionary
        /// </summary>
        [TestMethod]
        public void GetNamesOfAllNonemptyCellsTest()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            Assert.IsFalse(ss.GetNamesOfAllNonemptyCells().GetEnumerator().MoveNext());
        }

        /// <summary>
        /// test GetNamesOfAllNonemptyCells(). can not movenext if the cell is empty
        /// </summary>
        [TestMethod]
        public void GetNamesOfAllNonemptyCellsTest2()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            ss.SetContentsOfCell("A2", "");
            Assert.IsFalse(ss.GetNamesOfAllNonemptyCells().GetEnumerator().MoveNext());
        }

        /// <summary>
        /// test GetNamesOfAllNonemptyCells(). assert should be true because there is cell inside
        /// </summary>
        [TestMethod]
        public void GetNamesOfAllNonemptyCellsTest3()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            ss.SetContentsOfCell("A2", "77");
            HashSet<string> test = new HashSet<string>() { "A2" };
            Assert.IsTrue(new HashSet<string>(ss.GetNamesOfAllNonemptyCells()).SetEquals(test));
        }


        //----------------------test for SetContentsOfCell(name, text)-------------------------
        /// <summary>
        /// test if the content is given by a null, should throw ArgumentNullException
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SetContentsOfCellTest()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            string text = null;
            ss.SetContentsOfCell("haha", text);
        }

        /// <summary>
        /// test if the name is given by a invalid string, should throw InvalidNameException
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void SetContentsOfCellTest2()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            ss.SetContentsOfCell("&", "haha");
        }

        /// <summary>
        /// test if the name is given by a null string, should throw InvalidNameException
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void SetContentsOfCellTest2_1()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            ss.SetContentsOfCell(null, "haha");
        }

        /// <summary>
        /// if my spreadsheet has had the cell name, the ss should replace the contents at the name
        /// </summary>
        [TestMethod]
        public void SetContentsOfCellTest3()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            ss.SetContentsOfCell("A2", "yo");
            ss.SetContentsOfCell("A2", "ha");
            object content = "ha";
            Assert.IsTrue(ss.GetCellContents("A2") == content);
        }


        /// <summary>
        /// if my spreadsheet does not have the cell name, 
        /// the ss should add the contents at the cell name
        /// </summary>
        [TestMethod]
        public void SetContentsOfCellTest4()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            ss.SetContentsOfCell("A2", "yo");

            object content = "yo";
            Assert.IsTrue(ss.GetCellContents("A2") == content);
        }

        //----------------------test for SetContentsOfCell(name, number)-------------------------

        /// <summary>
        /// test if the name is given by a null, should throw InvalidNameException
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void SetContentsOfCellTest5()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            string name = null;
            ss.SetContentsOfCell(name, "3.14");
        }

        /// <summary>
        /// test if the name is given by a invalid string, should throw InvalidNameException
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void SetContentsOfCellTest6()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            ss.SetContentsOfCell("&", "3.14");
        }

        /// <summary>
        /// if my spreadsheet has had the cell name, the ss should replace the contents at the name
        /// </summary>
        [TestMethod]
        public void SetContentsOfCellTest7()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            ss.SetContentsOfCell("A2", "3.14");
            ss.SetContentsOfCell("A2", "1");
            double content = 1;
            object temp = ss.GetCellContents("A2");
            double output;
            double.TryParse(temp.ToString(), out output);

            Assert.IsTrue(output == content);
        }


        /// <summary>
        /// if my spreadsheet does not have the cell name, 
        /// the ss should add the contents at the cell name
        /// </summary>
        [TestMethod]
        public void SetContentsOfCellTest8()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            ss.SetContentsOfCell("A2", "3.14");
            Assert.AreEqual(ss.GetCellContents("A2"), 3.14);
        }


        //----------------------test for SetContentsOfCell(name, formula)-------------------------

        /// <summary>
        /// test if the content is given by a null, should throw ArgumentNullException
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SetContentsOfCellTest9()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            ss.SetContentsOfCell("haha", null);
        }

        /// <summary>
        /// test if the name is given by a invalid string, should throw InvalidNameException
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void SetContentsOfCellTest10()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            ss.SetContentsOfCell("&", "A1+A2");
        }

        /// <summary>
        /// test if the name is given by a null string, should throw InvalidNameException
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void SetContentsOfCellTest11()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            ss.SetContentsOfCell(null, "A1+A2");
        }

        //----------------------test for GetDirectDependents-------------------------

        /// <summary>
        /// test for testing GetDirectDependents because it's hard to check by outside using
        /// </summary>
        [TestMethod]
        public void GetDirectDependentsTest()
        {
            PrivateObject test = new PrivateObject(new Spreadsheet());
            test.Invoke("GetDirectDependents", new object[] { "X1" });
        }

        /// <summary>
        /// test for testing GetDirectDependents of null name
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetDirectDependentsTest2()
        {
            PrivateObject test = new PrivateObject(new Spreadsheet());
            test.Invoke("GetDirectDependents", new Object[] { null });
        }

        /// <summary>
        /// test for testing GetDirectDependents of invalid name
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void GetDirectDependentsTest3()
        {
            PrivateObject test = new PrivateObject(new Spreadsheet());
            test.Invoke("GetDirectDependents", new Object[] { "#%$" });
        }

    }
}
