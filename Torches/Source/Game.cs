﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using System.Security.Cryptography;
using System.Runtime.InteropServices;

namespace Torches
{
    static class Game
    {
        private static bool running = false;

        private static World world;

        private static List<ECS.ISystem> systems;


        // Disable selection in console (https://stackoverflow.com/questions/13656846/how-to-programmatic-disable-c-sharp-console-applications-quick-edit-mode)
        const int STD_INPUT_HANDLE = -10;
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetStdHandle(int nStdHandle);
        [DllImport("kernel32.dll")]
        static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);
        [DllImport("kernel32.dll")]
        static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);
        ///////////////////////////////////////////////////////////////////////

        public static void Start()
        {
            running = true;

            // Disable selection in console (https://stackoverflow.com/questions/13656846/how-to-programmatic-disable-c-sharp-console-applications-quick-edit-mode)
            const uint ENABLE_QUICK_EDIT = 0x0040;
            IntPtr consoleHandle = GetStdHandle(STD_INPUT_HANDLE);
            uint consoleMode;
            GetConsoleMode(consoleHandle, out consoleMode);
            consoleMode &= ~ENABLE_QUICK_EDIT;
            SetConsoleMode(consoleHandle, consoleMode);
            /////////////////////////////////////////////////////////////

            Renderer.PrintUI();

            world = new World();

            systems = new List<ECS.ISystem>();
            systems.Add(new ECS.MoveSystem());
            systems.Add(new ECS.EnemySystem());
            systems.Add(new ECS.TribesmanSystem());

            while (running)
            {
                string command = InputCommand();
                string[] segments = command.Split(' ');

                if(segments.First() == "quit" || segments.First() == "exit")
                {
                    running = false;
                }
                else if(segments.First() == "help" || segments.First() == "h")
                {
                    Help.OpenHelpMenu(segments);
                }
                // Turn on cheats
                else if(segments.First() == "cheat")
                {
                    if(segments.Length >= 2)
                    {
                        SHA256 sha256 = SHA256.Create();

                        // Check if password is correct, without storing it in code.
                        if(BitConverter.ToString(
                            sha256.ComputeHash(
                                Encoding.UTF8.GetBytes(segments[1]))) == 
                                // Hash value of the password.
                                "83-AF-00-23-EA-54-C5-97-F6-C0-47-1E-F2-80-0A-C6-EC-19-0E-92-22-FC-62-19-4B-6B-7E-9E-32-AD-8A-ED")
                        {
                            Renderer.PrintGameOutput("Access Granted ;)");
                        }
                        else
                        {
                            Renderer.PrintGameOutputColoured("`RINTRUDER DETECTED... EXITING");
                            Thread.Sleep(3000);
                            Stop();
                        }
                    }
                    
                }
                // If the update returns false, the command has not been handled
                else if(!Update(segments))
                {
                    Renderer.PrintGameOutput("Error");
                }
            }
        }
        
        private static bool Update(string[] segments)
        {
            foreach(ECS.ISystem s in systems)
            {
                if (s.Update(segments, ref world))
                {
                    world.Update();
                    return true;
                }
            }

            world.Update();
            return false;
        }

        public static string InputCommand()
        {
            Console.SetCursorPosition(Constants.TextInputX, Constants.TextInputY);
            string command = Console.ReadLine();
            Console.SetCursorPosition(Constants.TextInputX, Constants.TextInputY);
            Console.WriteLine(new string(' ', 100));
            Console.SetCursorPosition(Constants.TextInputX, Constants.TextInputY);
            return command;
        }

        public static void Stop()
        {
            running = false;
        }
    }
}
