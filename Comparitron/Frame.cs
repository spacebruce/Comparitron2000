using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Comparitron
{
    public class Frame
    {
        public Frame(int number, string text)
        {
            this.Number = number;
            this.Text = text;
        }

        public int Number { get; private set; }
        public string Text { get; private set; }
    }
}
