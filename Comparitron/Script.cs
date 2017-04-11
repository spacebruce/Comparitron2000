using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Comparitron
{
    public class Script
    {
        private List<Frame> imageData = new List<Frame>();

        public Script(string epcode, string title)
        {
            this.Epcode = epcode;
            this.Title = title;
        }

        public string Epcode { get; private set; }
        public string Title { get; private set; }
        public List<Frame> Frames { get { return this.imageData; } }
    }
}
