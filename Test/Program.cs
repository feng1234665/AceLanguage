using System;
using System.IO;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var s=File.ReadAllText("t.txt");
            var ts=AceLanguage.Lexical.Process(s);
            foreach (var item in ts)
            {
                Console.WriteLine
            (item.type.ToString()+"  "+item.value+" "+item.line);

            }

           var ast= AceLanguage.AST.Process(ts.ToArray());

            Print(ast, 0);
            Console.ReadLine();
        }

        static void Print(AceLanguage.ASTNode node,int depth)
        {

        }
    }
}
