using Flex.Ulysses.Models;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Ulysses
{
    internal class Program
    {
        public static LinkedList<string> MergeLinkedList(LinkedList<string> a, LinkedList<string> b)
        {
            var node = b.First;
            while (node != null)
            {
                a.AddLast(node.Value);
                node = node.Next;
            }
            return a;
        }

        public static (Dictionary<string, Word>, string last) SeperateWords(StringBuilder sb, string priorLastWord)
        {
            DateTime start = DateTime.Now;
            Dictionary<string, Word> map = new Dictionary<string, Word>();
            var str = sb.ToString().Replace("'", "||");


            Regex r = new Regex("(?:[^a-z_ ]|(?<=['\"])s)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
            string[] items = r.Replace(str, " ").Replace("||", "'").Replace("|", "").ToLower().Split(' ').Where(x => x != "").ToArray();

            if (!string.IsNullOrEmpty(priorLastWord))
            {
                var firstword = str.Substring(0, 1);
                bool isFirstCharacterUpper = Regex.IsMatch(str.Substring(0, 1), @"^[A-Z]+$");
                if (!isFirstCharacterUpper)
                {
                    items[0] = priorLastWord + items[0];
                }
            }

            bool isWaste = Regex.IsMatch(str.Substring(str.Length - 1, 1), @"^[a-z]+$");
            for (int i = 0; i < (isWaste ? items.Length - 1 : items.Length); i++)
            {
                if (map.ContainsKey(items[i]))
                {
                    map[items[i]].count++;
                    if (i < str.Length - 1)
                        map[items[i]].next.AddLast(items[i + 1]);
                }
                else
                {
                    map.Add(items[i], new Word { count = 1, next = new LinkedList<string>() });
                    if (i < str.Length - 1)
                        map[items[i]].next.AddLast(items[i + 1]);
                }
            }
            //return list and last word
            return (map.OrderByDescending(x => x.Value.count).ToDictionary(a => a.Key, b => b.Value), isWaste ? items[items.Length - 1] : "");
        }

        public static void Next(ConcurrentDictionary<string, Word> map)
        {
            DateTime start = DateTime.Now;
            ConcurrentDictionary<string, int> mapNext = new ConcurrentDictionary<string, int>();
            var mapList = map.OrderByDescending(x => x.Value.count).Take(20).ToDictionary(a => a.Key, b => b.Value);

            Parallel.ForEach(mapList, new ParallelOptions { MaxDegreeOfParallelism = 10 }, (item) =>
            {
                LinkedListNode<string>? node = item.Value.next.First;
                while (node != null)
                {
                    mapNext.AddOrUpdate(node.Value, 1, (key, v) => v + 1);
                    node = node.Next;
                }
            });

            var nextList = mapNext
                .OrderByDescending(c => c.Value)
                .Take(5)
                .ToDictionary(a => a.Key, b => b.Value);


            Console.WriteLine("TOP 20");
            foreach (var item in mapList)
            {
                Console.WriteLine($"{item.Key}:{item.Value.count}");
            }

            Console.WriteLine();
            Console.WriteLine("NEXT 5");
            foreach (var item in nextList)
            {
                Console.WriteLine($"{item.Key}:{item.Value}");
            }
            Console.WriteLine($"Time : {DateTime.Now.Subtract(start).TotalSeconds}");
            Console.WriteLine();
        }

        static async Task Main(string[] args)
        {
            ConcurrentDictionary<string, Word> map = new ConcurrentDictionary<string, Word>();

            const int bufferSize = 65566;
            string downloadUrl = "https://www.gutenberg.org/files/4300/4300-0.txt";

            using (HttpClient client = new HttpClient())
            {
                using (var response = await client.GetAsync(downloadUrl))
                {
                    response.EnsureSuccessStatusCode();
                    var stream = await response.Content.ReadAsStreamAsync();

                    using (var sr = new StreamReader(stream))
                    {
                        var buffer = new Char[bufferSize];
                        var length = 0L;
                        var totalRead = 0L;
                        var count = bufferSize;

                        length = sr.BaseStream.Length;
                        string lastword = "";
                        while (count > 0)
                        {
                            count = sr.Read(buffer, 0, bufferSize);
                            var sb = new StringBuilder();
                            sb.Append(buffer, 0, count);
                            if (sb.Length > 0)
                            {
                                var jobResult = SeperateWords(sb, lastword);
                                lastword = jobResult.Item2;

                                if (map.Count > 0 && jobResult.Item1.Count > 0)
                                    map.LastOrDefault().Value.next.AddLast(jobResult.Item1.Keys.First());

                                foreach (var item in jobResult.Item1)
                                    map.AddOrUpdate(item.Key, item.Value, (key, v) =>
                                    new Word
                                    {
                                        title = v.title,
                                        count = v.count + item.Value.count,
                                        next = MergeLinkedList(v.next, item.Value.next)
                                    });
                            }
                            totalRead += count;
                        }

                        Next(map);
                    }

                }
            }
        }
    }
}
