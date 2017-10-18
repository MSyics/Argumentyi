using MSyics.Argumentyi;
using System;

namespace MSyics.Argmentyi.Example
{
    class Program
    {
        static void Main(string[] args)
        {
            args = "foo 10 -a -B bar -C 2017/01/01".Split(" ");

            var parser = ArgumentParser<Hoge>.Create(setting =>
            {
                setting.Default(x => x.Path)
                       .Default(x => x.Count, x => int.Parse(x))
                       .Option("-A", x => x.OptionA, () => true)
                       .Option("-B", x => x.OptionB)
                       .Option("-C", x => x.OptionC, x => DateTime.Parse(x));
            });
            parser.IgnoreCase = true;
            if (!parser.TryParse(args, out var hoge))
            {
                Console.WriteLine("ArgumentException");
            }
            else
            {
                Console.WriteLine($"{hoge.Path},{hoge.Count},{hoge.OptionA},{hoge.OptionB},{hoge.OptionC}");
            }

        }

        public class Hoge
        {
            public string Path { get; set; }
            public int Count { get; set; }
            public bool OptionA { get; set; }
            public string OptionB { get; set; }
            public DateTime OptionC { get; set; }
        }
    }
}
