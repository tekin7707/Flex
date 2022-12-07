using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flex.Ulysses.Helpers
{
    public static class ExceptionWords
    {
        public static Dictionary<string, (string, string)> exceptions = new Dictionary<string, (string, string)>();
        public static string[] remove { get; set; } = { "","— I —", "— II —", "— III —" };
        public static void Fill()
        {
            if(exceptions.Count == 0)
            {

                exceptions.Add("i’ll", ("i", "will"));
                exceptions.Add("we’ll", ("we", "will"));
                exceptions.Add("you’ll", ("you", "will"));
                exceptions.Add("he’ll", ("he", "will"));

                exceptions.Add("i’m", ("i", "am"));
                exceptions.Add("you’re", ("you", "are"));
                exceptions.Add("we’re", ("we", "are"));
                exceptions.Add("he’s", ("he", "is"));
                exceptions.Add("it’s", ("it", "is"));
                exceptions.Add("can’t", ("can", "not"));
                exceptions.Add("don’t", ("do", "not"));
                exceptions.Add("won’t", ("will", "not"));
                exceptions.Add("that’s", ("that", "is"));
                exceptions.Add("what’s", ("what", "is"));
                exceptions.Add("where’s", ("where", "is"));

                exceptions.Add("hasn’t", ("has", "not"));
                exceptions.Add("isn’t", ("is", "not"));
                exceptions.Add("there’s", ("there", "is"));
                exceptions.Add("doesn’t", ("does", "not"));
                exceptions.Add("didn’t", ("did", "not"));
                exceptions.Add("wouldn’t", ("would", "not"));
                exceptions.Add("hadn’t", ("had", "not"));
                exceptions.Add("couldn’t", ("could", "not"));
                exceptions.Add("weren’t", ("were", "not"));
                
            }
        }
    }
}
