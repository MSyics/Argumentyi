using MSyics.Argumentyi;
using System;
using System.Linq;

namespace MSyics.Argmentyi.Example
{
    class Program
    {
        static void Main(string[] args)
        {
            args = "foo 10 o4 -items item1 item2 item3 -a -B bar -C 2017/01/01 /AAA -nums 1 2 3 /BBB o1 o2 o3".Split(" ");

            var parser = ArgumentParser<Hoge>.Create(setting =>
            {
                setting.
                Default(x => x.Path).
                Default(x => x.Count, x => int.Parse(x)).
                Option("-A", x => x.OptionA, () => true).
                Option("-B", x => x.OptionB).
                Option("-C", x => x.OptionC, x => DateTime.Parse(x)).
                Option("/AAA", x => x.Actions |= Actions.AAA).
                Option("/BBB", x => x.Actions |= Actions.BBB).
                Options("-items", x => x.Items).
                Options("-nums", x => x.Numbers, x => x.Select(y => int.Parse(y)).ToArray()).
                Others((x, y) => x.Others = y);
            });
            parser.IgnoreCase = true;
            if (!parser.TryParse(args, out var hoge))
            {
                Console.WriteLine("ArgumentException");
            }
            else
            {
                Console.WriteLine($"{hoge.Path},{hoge.Count},{hoge.OptionA},{hoge.OptionB},{hoge.OptionC},[{hoge.Actions}]");
                foreach (var item in hoge.Items)
                {
                    Console.WriteLine($"{item}");
                }
                foreach (var item in hoge.Numbers)
                {
                    Console.WriteLine($"{item}");
                }
                foreach (var item in hoge.Others)
                {
                    Console.WriteLine($"{item}");
                }
            }
        }

        class Hoge
        {
            public string Path { get; set; }
            public int Count { get; set; }
            public bool OptionA { get; set; }
            public string OptionB { get; set; }
            public DateTime OptionC { get; set; }
            public Actions Actions { get; set; }
            public string[] Items { get; set; }
            public int[] Numbers { get; set; }
            public string[] Others { get; set; }
        }

        [Flags]
        enum Actions
        {
            AAA = 1,
            BBB = 2,
            CCC = 4,
        }
    }
}
