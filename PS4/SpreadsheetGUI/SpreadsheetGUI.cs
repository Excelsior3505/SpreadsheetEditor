using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using SS;

namespace SpreadsheetGUI
{
    // Sharon Xiao
    // u0943650

    // Linxi Li
    // u1016104

    public class SSContextSingleton : ApplicationContext
    {
        //crouching constant...
        /// <summary>
        /// how many forms are allowed
        /// </summary>
        public const int MAX_FORMS = 20;
        /// <summary>
        /// How many forms are active.
        /// </summary>
        public int formCount = 0;
        ///<summary>
        ///this singleton regulates spreadsheet reproduction
        ///crouching singleton
        ///</summary>
        public static SSContextSingleton MyContext;
        //hidden constructor
        private SSContextSingleton() { }
        /// <summary>
        /// makes new context exactly once
        /// </summary>
        /// <returns> a new contextsingleton if there isn't one already.</returns>
        public static SSContextSingleton getContext()
        {
            if (MyContext == null)
            {
                MyContext = new SSContextSingleton();
            }
            return MyContext;
        }
        /// <summary>
        /// Runs the given form
        /// </summary>
        /// <param name="form"> The appcontext can launch anything of type Form</param>
        public void RunForm(Form form)
        {
            // increment formcount
            formCount++;

            // When this form closes, we want to find out
            //using a lambda to make a small event handler. If decrementing the
            //formcount leaves 0, we can close the entire program.
            form.FormClosed += (o, e) => { if (--formCount <= 0) ExitThread(); };
            // Run the form
            form.Show();
        }
    }

    static class SpreadsheetGUI
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
