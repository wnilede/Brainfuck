using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brainfuck
{
    public class BrainfuckException : Exception
    {
        public BrainfuckException(string message) : base(message) { }
    }
}