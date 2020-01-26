using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BUS_DAA_SIGMA
{
    class UI
    {
        private static readonly int _screenW;
        private static readonly int _screenH;

        private static int          _cursorPosH;
        private static readonly int _commanderH;

        static UI()
        {
            _screenW = Console.WindowWidth;
            _screenH = Console.WindowHeight;
            _commanderH = (int)Math.Floor(_screenH * 0.8);
        }

        public static void ClearCurrentConsoleLine()
        {
            int currentLineCursor = Console.CursorTop;
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, currentLineCursor);
        }

        public static void Print(string message)
        {
            int messageLinesNumber = message.Split('\n').Length;
            if (_cursorPosH + messageLinesNumber < _commanderH)
                Console.SetCursorPosition(0, _cursorPosH);
            else
            {
                Console.Clear();
                _cursorPosH = 0;
                Console.SetCursorPosition(0, _cursorPosH);
            }

            _cursorPosH += messageLinesNumber;
            Console.Write(message);
            DrawUI();
        }

        public static void DrawUI()
        {
            Console.SetCursorPosition(0, _commanderH);
            for (int i = 0; i < _screenW; i++)
            {
                Console.Write("-");
            }

            for (int i = _commanderH + 1; i < _commanderH + 4; i++)
            {
                Console.SetCursorPosition(0, i);
                ClearCurrentConsoleLine();
            }

            Console.SetCursorPosition(0, _commanderH + 1);
            Console.Write(">>");
        }
    }
}
