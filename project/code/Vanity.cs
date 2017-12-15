using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NBitcoin;

namespace ProgAssignment {
    class Vanity {
        public static HashSet<Key> run_vanity_address_code()
        {
            int N = 5;  // length of strings we want to find
            Console.WriteLine("length: " + N);

            int numCPUs = 4;  // Degree of Paralellism

            // The strings we search for.
            // Note that only the ones with length N are actually considered.
            List<string> names = new List<string>();
            names.Add("cyril");  // cyril
            names.Add("leona");  // leonardo
            names.Add("lukas");  // lukas
            names.Add("joey");   // joey

            HashSet<string> targets = new HashSet<string>();
            foreach (string name in names)
            {
                if (name.Length == N)
                {  // only take strings with length N
                    targets.Add(name.Substring(0, N).ToLower());
                }
            }

            // print targets
            Console.Write("targets: ");
            foreach (string target in targets)
            {
                Console.Write(target + " ");
            }
            Console.WriteLine();

            // Call the search method in numCPUs threads.
            // Note that 'targets' and 'found_keys' are shared variables. HashSet should be thread save.
            HashSet<Key> found_keys = new HashSet<Key>();
            Parallel.For(0, numCPUs, index => find_vanity_address(index, targets, found_keys, N));

            // Print results
            foreach (Key key in found_keys)
            {
                // Save wallets
                String testAddress = key.PubKey.GetAddress(Network.TestNet).ToString();
                WalletManager wm = new WalletManager(key, ".wallet-" + testAddress.Substring(34 - N, N));
                Console.WriteLine("Address : " + testAddress);
                Console.WriteLine("Private key : " + key.GetBitcoinSecret(Network.TestNet));
            }

            return found_keys;
        }
        private static void find_vanity_address(int index, HashSet<string> targets, HashSet<Key> found_keys, int N)
        {
            Console.WriteLine("[" + index + "]Starting search...");

            // Initialize variables
            int trialnbr = 0;
            Key trial = null;
            string test = null;
            string subs = null;

            // while not all targets are found.
            while (targets.Count > 0)
            {
                trialnbr++;

                trial = new Key();  // generate random key
                test = trial.PubKey.GetAddress(Network.TestNet).ToString();  // the corresponding address
                subs = test.Substring(34 - N, N).ToLower();  // the end of the address
                if (targets.Contains(subs))
                {  // if the address ends in a desired string ...
                    targets.Remove(subs);  // ... remove it from the target set ...
                    found_keys.Add(trial); /// ... and add it to the found set
                    Console.WriteLine("[" + index + "]Found " + subs + " PK: " + trial.GetBitcoinSecret(Network.TestNet) + " -> " + test);
                }

                // some periodic output
                if (trialnbr % 50000 == 0)
                {
                    Console.WriteLine("[" + index + "] " + trialnbr + " ... ");
                }
            }

            // write out how many trials have been made
            Console.WriteLine("[" + index + "]searched " + trialnbr + " keys");

        }
    }
}