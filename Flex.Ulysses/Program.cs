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
        public static ConcurrentDictionary<string, int> MergeDictionary(ConcurrentDictionary<string, int> a, ConcurrentDictionary<string, int> b)
        {
            foreach (var item in b)
                a.AddOrUpdate(item.Key, item.Value, (key, v) => item.Value + v);

            return a;
        }

        public static (Dictionary<string, Word>, string last) SeperateWords(StringBuilder sb, string priorLastWord)
        {
            DateTime start = DateTime.Now;
            Dictionary<string, Word> map = new Dictionary<string, Word>();
            var str = sb.ToString().Replace("’", "||");


            Regex r = new Regex("(?:[^a-z| ]|(?<=['\"])s)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
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
                    if (i < items.Length - 1)
                        map[items[i]].Next.AddOrUpdate(items[i + 1], 1, (key, value) => value + 1);
                }
                else
                {
                    map.Add(items[i], new Word { count = 1, Next = new ConcurrentDictionary<string, int>() });
                    if (i < items.Length - 1)
                        map[items[i]].Next.AddOrUpdate(items[i + 1], 1, (key, value) => 1);
                }
            }
            //return list and last word
            return (map.OrderByDescending(x => x.Value.count).ToDictionary(a => a.Key, b => b.Value), isWaste ? items[items.Length - 1] : "");
        }

        public static void Next(ConcurrentDictionary<string, Word> map)
        {
            var mapList = map.OrderByDescending(x => x.Value.count).Take(20).ToDictionary(a => a.Key, b => b.Value);

            foreach (var item in mapList)
            {
                Console.Write($"{item.Key}({item.Value.count}) : ");

                foreach (var next in item.Value.Next.OrderByDescending(x => x.Value).Take(5).ToDictionary(a => a.Key, b => b.Value))
                {
                    Console.Write($" {next.Key}({next.Value})");
                }
                Console.WriteLine();
            }

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
                                    map.LastOrDefault().Value.Next.AddOrUpdate(jobResult.Item1.Keys.First(), 1, (key, value) => value + 1);

                                foreach (var item in jobResult.Item1)
                                    map.AddOrUpdate(item.Key, item.Value, (key, v) =>
                                    new Word
                                    {
                                        count = v.count + item.Value.count,
                                        Next = MergeDictionary(v.Next,item.Value.Next)
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
