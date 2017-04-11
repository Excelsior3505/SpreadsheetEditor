
README.TXT
---------------------------PS6---------------------------------------------

 Design decisions: speciel feture of arrow key moving to select cell.
 External code resources: spreadsheetPanal provided by CS3500 University of Utah
 Implementation notes: more imformation in Guide
 Problems: Saving problems

 Author information: 
	Sharon Xiao
	u0943650

    Linxi Li
    u1016104

	Skeletion:
	CS3500 The University of Utah

	Date of Commenting: 4th Nov 2016


----------------------------PS5--------------------------------------------
1. Your name and the date associated with your comments
Sharon Xiao

u0943650

date associated with your comments: 10/7/2016

2. Your initial design thoughts about the project
(how you are going to set things up/code the project)
My initial design thoughts is that my spreadsheet should be a dictonary typr,
and the keyValuePair should be name and cell, so I need a class for creating 
cell object. cell object should be made by a cell name and a cell contents (should
be double numbers, string text, or formula object). Then it should be okay to 
implement GetCellContents and GetNamesOfAllNonemptyCells and GetDirectDependents.
Then implement the rest. I think the isInvalid helper function is important for 
most of the implemtation, therefore, I create a private helper function to check
the imput validation.

  up date: implemented more functions as SetContentsOfCell(),GetCellValue()
            Save(), GetSavedVersion() and so on.


3. A comment on which versions of the PS2/PS3 libraries you are building against
PS2: initial version 1.0
PS3: initial version 2.0


4. Any notes you want the graders to be aware of when evaluating your work
Thank you for checking my homework!