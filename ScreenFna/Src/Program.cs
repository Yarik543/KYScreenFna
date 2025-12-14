using Microsoft.Xna.Framework;
using ScreenSaverFna;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScreenFna.Src
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (var game = new ScreenSaverGame())
            {
                game.Run();
            }
        }
    }
}
