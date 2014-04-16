using System;

namespace superProject
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            
            
	


        System.Threading.Thread.CurrentThread.CurrentCulture = 
                System.Globalization.CultureInfo.CreateSpecificCulture("en-US");

            using (Game1 game = new Game1())
            {
                game.Run();
            }
        }
    }
#endif
}

