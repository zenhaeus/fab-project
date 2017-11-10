using System;
using NBitcoin;

namespace MyProject
{
    class Program
    {
        public static void Main()
        {
            
            int N = 5;
            int maxN = 100000;
            string test = " ";
            string initial = " ";
            string text = "mummy";
            int i = 0;
            Key trial = new Key();
            while (i < maxN && initial.ToLower() != text.Substring(0,N))
            {
                trial = new Key();
                var address = trial.PubKey.GetAddress(Network.TestNet);
                test = address.ToString();
                initial = test.Substring(0,N);
                i++;
            }
            Console.WriteLine("Adress : " + test);
            Console.WriteLine("Sequence : " + initial);
            Console.WriteLine("Private key : " + trial.GetBitcoinSecret(Network.TestNet));
        }
    }
}