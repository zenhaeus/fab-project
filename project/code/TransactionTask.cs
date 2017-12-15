using System;
using System.Collections.Generic;
using System.Threading;
using NBitcoin;
using QBitNinja.Client;
using System.Threading.Tasks;
using QBitNinja.Client.Models;
// install QBitNinja with 'dotnet install QBitNinja.Client' (on linux)

namespace ProgAssignment
{
    class TransactionTask
    {
        public static void run_send_transaction_code()
        {
            BitcoinSecret pk = new BitcoinSecret("cQAAf3mbbPem99TJrLxWqhvQ61iZEgs5ieU2JMc4vEQEEre7fCUm");  // address: mksZSHNnEA81eh2EKwSQrYNEaDRe5LUKAs
            BitcoinPubKeyAddress to_addr = new BitcoinPubKeyAddress("mipVxuurz9XAapprSX5RquroiZfa4povRj ");  // to 'lukas' copay wallet
            string transaction_id = "ead78a2348b5b471e98931cdcc4f8f65e71e378e90652acbedbc14116fb857c6";  // my last transaction -> lukas address (1BTC)
            decimal amount = 0.001m;

            uint256 thash = generate_transaction(pk, transaction_id, to_addr, amount);
            // uint256 thash = uint256.Parse("4f48f94745dea2dab906be295a25be57b190b4a823e49409cdfbb01398cce98a");  // test transaction
            int time_to_sleep = 10 * 1000; // 10 seconds
            int confirmations_to_wait_for = 3;
            wait_for_transaction_confirmation(thash, confirmations_to_wait_for, time_to_sleep);
        }

        private static uint256 generate_transaction(BitcoinSecret from_key, string from_transaction, BitcoinPubKeyAddress to_address, decimal amount)
        {
            /*1. Explain the structure of the transaction to send bitcoins from your vanity address to your CoPay wallet
              2. Build the raw transaction
              3. Sign the inputs of the raw transaction
              4. Broadcast the signed transaction to the testnet network using QBitNinja
              5. Verify that the transaction gets confirmed by the network
              6. Implement a script writing in the console the number of confirmations of a transaction as soon as it changes
              */

            // Create a client
            QBitNinjaClient client = new QBitNinjaClient(Network.TestNet);

            // find what transaction to spend from
            var transactionId = uint256.Parse(from_transaction);
            var transaction_response = client.GetTransaction(transactionId).Result;

            // determine outPoint to spend later
            List<ICoin> received_coins = transaction_response.ReceivedCoins;
            OutPoint outPointToSpend = null;
            foreach (ICoin coin in received_coins)
            {
                if (coin.TxOut.ScriptPubKey == from_key.ScriptPubKey)
                {
                    outPointToSpend = coin.Outpoint;
                }
            }
            if (outPointToSpend == null)
            {
                throw new Exception("TxOut doesn't contain our ScriptPubKey");
            }
            Console.WriteLine("We want to spend outpoint {0}.", outPointToSpend.N);

            // transaction
            var transaction = new Transaction();
            transaction.Inputs.Add(new TxIn()
            {
                PrevOut = outPointToSpend
            });

            // amount
            Money money_amount = new Money(amount, MoneyUnit.BTC);
            Console.WriteLine("amount in btc: {0}", money_amount);

            // fee
            var miner_fee = new Money(0.001m, MoneyUnit.BTC);
            Console.WriteLine("minerfee in btc: {0}", miner_fee);

            // create the out points
            TxOut real_TxOut = new TxOut()
            {
                Value = money_amount,
                ScriptPubKey = to_address.ScriptPubKey
            };

            // determine change
            Money prev_received_amount = (Money)received_coins[(int)outPointToSpend.N].Amount;
            Money change_amount = prev_received_amount - money_amount - miner_fee;

            TxOut change_TxOut = new TxOut()
            {
                Value = change_amount,
                ScriptPubKey = from_key.ScriptPubKey
            };

            transaction.Outputs.Add(real_TxOut);
            transaction.Outputs.Add(change_TxOut);

            // add a message
            var message = "That was easy!";
            var bytes = System.Text.Encoding.UTF8.GetBytes(message);
            transaction.Outputs.Add(new TxOut()
            {
                Value = Money.Zero,
                ScriptPubKey = TxNullDataTemplate.Instance.GenerateScriptPubKey(bytes)
            });

            // sign the transaction
            transaction.Inputs[0].ScriptSig = from_key.ScriptPubKey;
            transaction.Sign(from_key, false);

            // print the transaction
            Console.WriteLine("transaction:");
            Console.WriteLine(transaction);

            // send it

            BroadcastResponse broadcastResponse = client.Broadcast(transaction).Result;

            if (!broadcastResponse.Success)
            {
                Console.Error.WriteLine("ErrorCode: " + broadcastResponse.Error.ErrorCode);
                Console.Error.WriteLine("Error message: " + broadcastResponse.Error.Reason);
                return (uint256)0;
            }
            else
            {
                Console.WriteLine("Success! You can check out the hash of the transaciton in any block explorer:");
                uint256 thash = transaction.GetHash();
                Console.WriteLine("Generated transaction {0}!", thash);
                return thash;
            }
        }

        private static void wait_for_transaction_confirmation(uint256 transaction_hash, int nbr_block_confirmations, int sleep_millis)
        {
            QBitNinjaClient client = new QBitNinjaClient(Network.TestNet);
            Console.WriteLine("Waiting for confinrmation ...");
            int confirmations = 0;
            GetTransactionResponse resp = client.GetTransaction(transaction_hash).Result;
            if (resp.Block == null)
            {
                Console.WriteLine("Transaction not confirmed yet..");
            }
            while (confirmations < nbr_block_confirmations)
            {
                resp = client.GetTransaction(transaction_hash).Result;
                if (resp.Block == null)
                {
                    Console.Write(".");
                }
                else if (confirmations != resp.Block.Confirmations)
                {
                    confirmations = resp.Block.Confirmations;

                    Console.WriteLine("[{0}] New block confirmation! Confirmed now: {1}", DateTime.Now.ToString("H:mm:ss"), confirmations);
                }
                Thread.Sleep(sleep_millis);
            }
            Console.WriteLine("Done. The transaction has {0} Blocks confirmation(s)!", resp.Block.Confirmations);
        }
    }
}