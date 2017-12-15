using System;


namespace ProgAssignment { 
    class MainClass {
        public static void Main(String[] args)
        {
            string userInput;
            do
            {
                printInstructions();
                userInput = Console.ReadLine();
                switch (userInput)
                {
                    case "0":
                        Console.WriteLine("Goodbye!");
                        break;
                    case "1":
                        WalletManager walletManager = new WalletManager();
                        Console.WriteLine();
                        break;
                    case "2":
                        Vanity.run_vanity_address_code();
                        Console.WriteLine();
                        break;
                    case "3":
                        TransactionTask.run_send_transaction_code();
                        Console.WriteLine();
                        break;
                    case "4":
                        NinjaInteractor ninjaInteractor = new NinjaInteractor();
                        Console.WriteLine();
                        break;
                    default:
                        Console.WriteLine("===================================");
                        Console.WriteLine(" Invalid option, please try again. ");
                        Console.WriteLine("===================================");
                        break;
                }
            } while(userInput != "0");
        }

        private static void printInstructions() {
            Console.WriteLine("Enter the number corresponding to the program you would like to execute:");
            Console.WriteLine("1: Wallet manager");
            Console.WriteLine("2: Vanity address generator");
            Console.WriteLine("3: Build transaction");
            Console.WriteLine("4: QBitNinja blockchain indexer");
            Console.WriteLine("0: Exit");
        }
    }
}