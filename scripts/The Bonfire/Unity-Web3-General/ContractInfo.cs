// Filename: ContractInfo.cs
// Author: 0xFirekeeper
// Description: One place to set up all verified contracts to be used in your app.

// Verified contracts, used to fetch on-chain data
public enum Contract
{
    ChainSafeMarketplace,
    ReverieCollection,
    NFTCollection
}

// Set the general on-chain info for each case for better organization
[System.Serializable]
public class ContractInfo
{
    public Contract identifier; // identifier for the constructor
    public string chain; // set chain: ethereum, moonbeam, polygon etc]
    public string network; // set network mainnet, testnet
    public string chainId; // chain ID needed for write transactions
    public string rpc; // optional rpc
    public string contract; // address of contract
    public string abi; // abi in json format
    public string cidv0; // usually starts with Q and has lower and upper case
    public string cidv1; // usuallly starts with b and is only lower case

    public ContractInfo(Contract _identifier)
    {
        identifier = _identifier;

        switch (identifier)
        {
            case (Contract.ChainSafeMarketplace):
                chain = "ethereum";
                network = "goerli";
                break;
            case (Contract.ReverieCollection):
                chain = "ethereum";
                network = "goerli";
                chainId = "80001";
                rpc = "";
                contract = "0x2c1867bc3026178a47a677513746dcc6822a137a";
                break;
            case (Contract.NFTCollection):
                chain = "ethereum";
                network = "mainnet";
                chainId = "1";
                rpc = "";
                contract = "0x373ffdf2a50003fb7e10282cf50e03921828e1a7";
                abi = "";
                cidv0 = "QmbHMjJXVYicKcqshhjpH8xGZbmbsZg5JA6YxSkEqx5DAG";
                cidv1 = "bafybeigaj76s3ezefx3ryeropcoczwpud4auaxh673c5s7pdrfa77lu4te";
                break;
        }
    }

}