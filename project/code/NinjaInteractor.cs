using System;
using System.Collections.Generic;
using System.Linq;
using NBitcoin;
using QBitNinja.Client;
using QBitNinja.Client.Models;

namespace MyProject
{
    class NinjaInteractor
    {
        public static void Main()
        {
            /*
            1.Choose a transaction at random on https://blockchain.info
            2.Download all information available for this transaction from your code
            3.Download all information relative to your CoPay address from your code
            4.Print in the console the balance and the list of transactions related to this address
            5.Print the list of unspent transaction outputs, also known as UTXO or coins
            6.Explain the relationship between the balance and the UTXO set
            */
            Transaction_info();
            Copay_info();
        }

        private static void Transaction_info()
        {
            // Create a client in the main network
            QBitNinjaClient client = new QBitNinjaClient(Network.Main);

            // Parse transaction id to NBitcoin.uint256 so the client can eat it
            // https://blockchain.info/fr/tx/5e89acb5bb302098207ff1ac099e6ff00909fdc46f55ee65c5b3b0ae182d0fc3
            var transactionId = uint256.Parse("5e89acb5bb302098207ff1ac099e6ff00909fdc46f55ee65c5b3b0ae182d0fc3");

            // Query the transaction
            GetTransactionResponse transactionResponse = client.GetTransaction(transactionId).Result;
            Transaction transaction = transactionResponse.Transaction;

            // Download all information for this transaction

            var outputs = transaction.Outputs;
            foreach (TxOut output in outputs)
            {
                Money amount = output.Value;

                Console.WriteLine(amount.ToDecimal(MoneyUnit.BTC));
                var paymentScript = output.ScriptPubKey;
                Console.WriteLine(paymentScript);  // It's the ScriptPubKey
                var address = paymentScript.GetDestinationAddress(Network.Main);
                Console.WriteLine(address);
                Console.WriteLine();
            }

            var inputs = transaction.Inputs;
            foreach (TxIn input in inputs)
            {
                OutPoint previousOutpoint = input.PrevOut;
                Console.WriteLine(previousOutpoint.Hash); // hash of prev tx
                Console.WriteLine(previousOutpoint.N); // idx of out from prev tx, that has been spent in the current tx
                Console.WriteLine();
            }
        }
        private static void Copay_info()
        {

            // Create a client
            QBitNinjaClient client = new QBitNinjaClient(Network.TestNet);

            // get my wallet
            String LeoWalletAddress = "mpfyMn8d3mwfvXPx7FDHMG6vWVnhqMJjdk";
            //String LukasWalletAddress = "mipVxuurz9XAapprSX5RquroiZfa4povRj";

            // copay address
            var me = BitcoinAddress.Create(LeoWalletAddress);

            // get all transactions
            List<BalanceOperation> operations = client.GetBalance(me, false).Result.Operations;

            Decimal total_balance = 0;

            Console.WriteLine("All transactions");
            Console.WriteLine("==============================");
            // print all transactions with their amount, and add theme to get balance
            foreach (BalanceOperation balance in operations)
            {
                Decimal amout = balance.Amount.ToDecimal(MoneyUnit.BTC);
                total_balance += amout;
                Console.Write("Transaction ID : ");
                Console.WriteLine(balance.TransactionId);
                Console.Write("Amount : ");
                Console.WriteLine(amout);
            }

            Console.Write("Total balance : ");
            Console.WriteLine(total_balance);
            Console.WriteLine("==============================");


            // get only unspend transactions
            List<BalanceOperation> unspend_operations = client.GetBalance(me, true).Result.Operations;
            Console.WriteLine("Unspend transactions");
            Console.WriteLine("==============================");            // print only unspend transactions with their amount
            foreach (BalanceOperation balance in unspend_operations)
            {
                Decimal amout = balance.Amount.ToDecimal(MoneyUnit.BTC);
                Console.Write("Transaction ID : ");
                Console.WriteLine(balance.TransactionId);
                Console.Write("Amount : ");
                Console.WriteLine(amout);
            }
            Console.WriteLine("==============================");

        }

        private static void Test()
        {
            var client = new QBitNinjaClient(Network.TestNet);
            var wallet = client.GetWalletClient("Copay");
            wallet.CreateIfNotExists().Wait();
            wallet.CreateAddressIfNotExists(BitcoinAddress.Create("mpfyMn8d3mwfvXPx7FDHMG6vWVnhqMJjdk")).Wait();
            Console.WriteLine(wallet.Name.ToString());

            var balance = wallet.GetBalance().Result;

                foreach (var b in balance.Operations)
                {
                Console.WriteLine(b.Amount);
                }

        }

    }

}