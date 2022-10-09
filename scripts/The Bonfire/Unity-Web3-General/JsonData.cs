// Filename: JsonData.cs
// Author: 0xFirekeeper
// Description: Just a place for some JSON classes to test response deserializing with.

using System.Collections.Generic;

[System.Serializable]
public class NFTMetadata
{
    public NFTAttribute[] attributes;
    public string description;
    public string image;
    public string name;
    public string tokenType;
    public string tokenId;
    public int userBalance;
    public string price;
    public string itemId;

    public NFTMetadata()
    {
        attributes = new NFTAttribute[] {
            new NFTAttribute("Game", "Reverie"),
            new NFTAttribute("Creator", "0xFirekeeper"),
            new NFTAttribute("Character", "Dragon"),
            new NFTAttribute("Type", "Skin"),
            new NFTAttribute("Rarity", "Basekit"),
            new NFTAttribute("Identifier", "Default"),

        };
        description = "Default Skin for the Dragon character in Reverie.";
        image = "";
        name = "Reverie - Dragon Skin (Default)";
        tokenType = "";
        tokenId = "";
    }
}

[System.Serializable]
public class NFTAttribute
{
    public string trait_type;
    public string value;

    public NFTAttribute(string _trait_type, string _value)
    {
        trait_type = _trait_type;
        value = _value;
    }
}