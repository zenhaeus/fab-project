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

            //1 + 2
            Transaction_info();

            QBitNinjaClient client = new QBitNinjaClient(Network.TestNet);
            //3
            WalletClient wallet = CreateWallet(client);
            //4
            Copay_info(client, wallet);
            //5
            PrintUTXO(client, wallet);
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
            Console.WriteLine("==============================");
            Console.WriteLine("Outputs");
            Console.WriteLine("------------------------------");
            var outputs = transaction.Outputs;
            foreach (TxOut output in outputs)
            {
                Money amount = output.Value;

                Console.Write("amout : ");
                Console.WriteLine(amount.ToDecimal(MoneyUnit.BTC));
                var paymentScript = output.ScriptPubKey;
                Console.Write("ScriptPubKey : ");
                Console.WriteLine(paymentScript);  // It's the ScriptPubKey
                var address = paymentScript.GetDestinationAddress(Network.Main);
                Console.Write("ScriptPubKey : ");
                Console.WriteLine(address);
                Console.WriteLine("------------------------------");
            }
            Console.WriteLine("==============================");

            Console.WriteLine("==============================");
            Console.WriteLine("Inputs");
            Console.WriteLine("------------------------------");
            var inputs = transaction.Inputs;
            foreach (TxIn input in inputs)
            {
                OutPoint previousOutpoint = input.PrevOut;
                Console.Write("Previous hash : ");
                Console.WriteLine(previousOutpoint.Hash); // hash of prev tx
                Console.Write("idx of output from previous tx : ");
                Console.WriteLine(previousOutpoint.N); // idx of out from prev tx, that has been spent in the current tx
                Console.WriteLine("------------------------------");
            }
            Console.WriteLine("==============================");
        }

        private static WalletClient CreateWallet(QBitNinjaClient client)
        {
            // create the wallet
            var wallet = client.GetWalletClient("Copay");
            wallet.CreateIfNotExists().Wait();
            // add all existing and used bitcoin adress'
            wallet.CreateAddressIfNotExists(BitcoinAddress.Create("mpfyMn8d3mwfvXPx7FDHMG6vWVnhqMJjdk")).Wait();
            wallet.CreateAddressIfNotExists(BitcoinAddress.Create("mprwDuzhzhLBvrNkrmra9nGtRZzrfCBssJ")).Wait();
            wallet.CreateAddressIfNotExists(BitcoinAddress.Create("mrhSA8tVWFhY3FXkAMuS6n19ZR4oFw5oDR")).Wait();

            return wallet;
        }
        private static void Copay_info(QBitNinjaClient client, WalletClient wallet)
        {
            List<BalanceOperation> operations = wallet.GetBalance().Result.Operations;
        
            Decimal total_balance = 0;

            Console.WriteLine("==============================");
            Console.WriteLine("All transactions");
            Console.WriteLine("------------------------------");
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

        }

        private static void PrintUTXO(QBitNinjaClient client, WalletClient wallet)
        {
            // list of all unspend transaction
            List<BalanceOperation> unspend_operations = new List<BalanceOperation>();

            // for each bitcoin address of the wallet we take the list of unspend transactions
            foreach (WalletAddress address in wallet.GetAddresses().Result)
            {
                BitcoinAddress me = BitcoinAddress.Create(address.Address.ToString());
                unspend_operations.AddRange(client.GetBalance(me, true).Result.Operations);
            }

            // print transactions id and amount
            Console.WriteLine("==============================");
            Console.WriteLine("Unspend transactions");
            Console.WriteLine("------------------------------");
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

    }

}