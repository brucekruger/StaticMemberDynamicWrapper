using System;

namespace StaticMemberDynamicWrapper
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            dynamic stringType = new StaticMemberDynamicWrapper(typeof(string));
            //Dynamic invoke of the static method Concat (class String)
            var r = stringType.Concat("A", "B");

            Console.WriteLine(r);
        }
    }
}
