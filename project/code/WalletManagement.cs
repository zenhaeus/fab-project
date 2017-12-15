using NBitcoin;
using System;
using System.IO;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;

namespace ProgAssignment
{
    class WalletManager
    {
        private Key privateKey;
        private PubKey publicKey;
        private BitcoinPubKeyAddress mainAddress;
        private BitcoinPubKeyAddress testAddress;

        private static StreamWriter writer;
        private static readonly HttpClient client = new HttpClient();
        private String walletFileName;
        public const String STD_WALLET_NAME = ".wallet";
        public const String WALLET_PATH = "../wallets/";

        /*
            Load existing wallet or automatically create new wallet
         */
        public WalletManager(String walletFileName = STD_WALLET_NAME)
        {
            this.walletFileName = walletFileName;

            // Try to load wallet
            if (!loadWallet())
            {
                // create new wallet if loading unsucessful
                createPrivateKey();
                // save new wallet to disk
                saveWallet();
            }
            Task<byte[]> qr = createQRCode(testAddress.ToString());
            saveQRCode(qr.Result);
        }

        /*
            Create Wallet from existing privateKey
         */
        public WalletManager(Key privateKey, String walletFileName = STD_WALLET_NAME) {
            this.walletFileName = walletFileName;
            this.privateKey = privateKey;
            generateWalletInfo();
            saveWallet();
            Task<byte[]> qr = createQRCode(testAddress.ToString());
            saveQRCode(qr.Result);
        }
        public WalletManager(String wif, String walletFileName = STD_WALLET_NAME) :
            this(Key.Parse(wif, Network.TestNet), walletFileName) {
        }

        public Boolean saveWallet()
        {
            // save wallet
            writer = new StreamWriter(getFullWalletPath());
            BitcoinSecret secret = privateKey.GetWif(Network.Main);
            try
            {
                writer.WriteLine(secret);
                writer.WriteLine(mainAddress);
                writer.WriteLine(testAddress);
            }
            catch (IOException ioe)
            {
                Console.WriteLine("Writing wallet to disk failed.");
                Console.WriteLine(ioe.StackTrace);
                return false;
            }
            finally
            {
                writer.Close();
            }
            return true;
        }

        public Boolean loadWallet()
        {
            if (File.Exists(getFullWalletPath()))
            {
                // load wallet
                Console.WriteLine("Found wallet, reading wallet info");
                String walletInfo = File.ReadAllText(getFullWalletPath());
                String[] walletArray = walletInfo.Split("\n");
                privateKey = Key.Parse(walletArray[0], Network.Main);
                generateWalletInfo();
                Console.WriteLine("Private key restored with following Adresses:");
                printAdresses();
                return true;
            }
            else
            {
                // wallet does not exist, load failed
                Console.WriteLine("Could not read wallet, file does not exist.");
                return false;
            }
        }

        private void createPrivateKey()
        {
            // Create new private key
            privateKey = new Key();
            generateWalletInfo();
            Console.WriteLine("Created New wallet with following Adresses:");
            printAdresses();
        }

        private void generateWalletInfo()
        {
            // Save public key, main and test address in class variables
            publicKey = privateKey.PubKey;
            mainAddress = publicKey.GetAddress(Network.Main);
            testAddress = publicKey.GetAddress(Network.TestNet);
        }

        private String getFullWalletPath()
        {
            return WALLET_PATH + walletFileName;
        }

        private void printAdresses()
        {
            Console.WriteLine("Main Network: " + mainAddress);
            Console.WriteLine("Test Network: " + testAddress);
        }

        private static async Task<byte[]> createQRCode(String address, UInt16 size = 150)
        {
            byte[] responseString = await client.GetByteArrayAsync(
                "https://api.qrserver.com/v1/create-qr-code/?" +
                "size=" + size + "x" + size +
                "&data=" + address
            );
            return responseString;
        }

        private void saveQRCode(byte[] qr)
        {
            String fullQRPath = WALLET_PATH + walletFileName + ".png"; 
            File.WriteAllBytes(fullQRPath, qr);
            Console.WriteLine("Saved qr code to: " + fullQRPath);
        }
    }
}