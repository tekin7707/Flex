using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flex.Ulysses.Models
{
    public class Word
    {
        public int count { get; set; }
        public ConcurrentDictionary<string,int> Next { get; set; }= new ConcurrentDictionary<string, int>();
    }
}
