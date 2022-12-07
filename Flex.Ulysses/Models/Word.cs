using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flex.Ulysses.Models
{
    public class Word
    {
        public string title { get; set; }
        public int count { get; set; }
        public LinkedList<string> next { get; set; } = new LinkedList<string>();
    }
}
