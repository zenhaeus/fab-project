using System;
using NBitcoin;

namespace MyProject
{
    class Program
    {
        public static void Main()
        {
            
            int N = 4;
            string test = " ";
            string initial = " ";
            string text = "nice";
            Key trial = new Key();
            while (initial.ToLower() != text.Substring(0,N))
            {
                trial = new Key();
                var address = trial.PubKey.GetAddress(Network.TestNet);
                test = address.ToString();
                initial = test.Substring(34-N,N);
            }
            Console.WriteLine("Adress : " + test);
            Console.WriteLine("Sequence : " + initial);
            Console.WriteLine("Private key : " + trial.GetBitcoinSecret(Network.TestNet));
        }
    }
}