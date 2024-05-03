using System;
using System.Collections.Generic;
using Collective.Components.Definitions;

namespace Collective.Components.Modals;

[Serializable]
public class ProductInfo
{
    public readonly int ID;
    public readonly string Name;
    public readonly string Brand;
    public readonly int TypicalFreshness = 0;
    public readonly int TypicalQuantity = 1;
    
    
    public float CurrentPrice { get; private set; }
    private readonly List<ProductType> ProductTypes;

    public void AddProductType(ProductType productType)
    {
        if (ProductTypes.Contains(productType)) return;
        ProductTypes.Add(productType);
    }
    public void UpdatePrice(float price) => CurrentPrice = price;
    public ProductInfo(int id, string productName, string brand, float price, int typicalQuantity)
    {
        ID = id;
        Name = productName;
        Brand = brand;
        CurrentPrice = price;
        TypicalQuantity = typicalQuantity;
        ProductTypes = DetermineProductTypes(productName, brand);
        TypicalFreshness = CalculateFreshness();
    }

    public bool HasProductType(params ProductType[] types)
    {
        foreach (var type in types)
            if (ProductTypes.Contains(type))
                return true;

        return false;
    }

    private int CalculateFreshness()
    {
        // Set default freshness in days
        var freshnessDays = 0;

        // Determine freshness based on product type
        if (HasProductType(ProductType.Meat, ProductType.Fish))
            freshnessDays = 3; // Meat and fish are typically fresh for 3 days
        else if (HasProductType(ProductType.Dairy))
            freshnessDays = 7; // Dairy products last about a week
        else if (HasProductType(ProductType.Fruit, ProductType.Vegetable))
            freshnessDays = 5; // Fruits and vegetables last about 5 days
        else if (HasProductType(ProductType.Bakery))
            freshnessDays = 2; // Bakery items are usually consumed within 2 days for optimal freshness
        else if (HasProductType(ProductType.Cereal, ProductType.PackagedFood))
            freshnessDays = 30; // Packaged and dry foods can last much longer
        else if (HasProductType(ProductType.Drink, ProductType.Alcohol))
            freshnessDays = 60; // Drinks and alcohols can last for about 2 months
        else if (HasProductType(ProductType.Spice, ProductType.Oil, ProductType.Condiment))
            freshnessDays = 180; // Spices, oils, and condiments can have a long shelf life
        else if (HasProductType(ProductType.Cleaning, ProductType.Book))
            freshnessDays = 365 * 2; // Non-perishable items like cleaning supplies and books have very long freshness periods
        else if (HasProductType(ProductType.PetFood))
            freshnessDays = 30; // Pet food typically lasts about a month

        // Return the calculated freshness in days
        return freshnessDays;
    }


    private List<ProductType> DetermineProductTypes(string productName, string brand)
    {
        var keywords = (productName + " " + brand).ToLower();
        var types = new List<ProductType>();
        
        if (keywords.Contains("meat") || keywords.Contains("chicken") || keywords.Contains("beef") ||
            keywords.Contains("pork") || keywords.Contains("lamb") || keywords.Contains("turkey") || keywords.Contains("duck"))
            types.Add(ProductType.Meat);
        
        if (keywords.Contains("fish"))
            types.Add(ProductType.Fish); 
        
        if (keywords.Contains("dairy") || keywords.Contains("milk") || keywords.Contains("cheese") ||
            keywords.Contains("yoghurt") || keywords.Contains("butter") || keywords.Contains("cream"))
            types.Add(ProductType.Dairy);
        
        if (keywords.Contains("fruit") || keywords.Contains("apple") || keywords.Contains("orange") ||
            keywords.Contains("banana") || keywords.Contains("grape") || keywords.Contains("berry"))
            types.Add(ProductType.Fruit);
        
        if (keywords.Contains("vegetable") || keywords.Contains("tomato") || keywords.Contains("carrot") ||
            keywords.Contains("potato") || keywords.Contains("lettuce") || keywords.Contains("onion"))
            types.Add(ProductType.Vegetable);
        
        if (keywords.Contains("spice") || keywords.Contains("salt") || keywords.Contains("pepper") ||
            keywords.Contains("cinnamon") || keywords.Contains("oregano"))
            types.Add(ProductType.Spice);
        
        if (keywords.Contains("pet food") || keywords.Contains("dog food") || keywords.Contains("cat food"))
            types.Add(ProductType.PetFood);
        
        if (keywords.Contains("drink") || keywords.Contains("water") || keywords.Contains("juice") ||
            keywords.Contains("soda") || keywords.Contains("beer") || keywords.Contains("wine") ||
            keywords.Contains("whiskey") || keywords.Contains("vodka"))
            types.Add(ProductType.Drink);
        
        if (keywords.Contains("alcohol") || keywords.Contains("beer") || keywords.Contains("wine") ||
            keywords.Contains("whiskey") || keywords.Contains("vodka"))
            types.Add(ProductType.Alcohol);
        
        if (keywords.Contains("cleaning") || keywords.Contains("bleach") || keywords.Contains("cleaner") ||
            keywords.Contains("soap") || keywords.Contains("detergent") || keywords.Contains("fabric softener") ||
            keywords.Contains("toiletpaper") || keywords.Contains("paper towel")) 
            types.Add(ProductType.Cleaning); 

        if (keywords.Contains("book") || keywords.Contains("novel") || keywords.Contains("literature") ||
            keywords.Contains("fiction"))
            types.Add(ProductType.Book);

     
        if (keywords.Contains("cereal"))
            types.Add(ProductType.Cereal);
        if (keywords.Contains("bread") || keywords.Contains("cake") || keywords.Contains("pastry") ||
            keywords.Contains("baguette") || keywords.Contains("biscuit"))
            types.Add(ProductType.Bakery);
        if (keywords.Contains("ketchup") || keywords.Contains("mayonnaise") || keywords.Contains("sauce") ||
            keywords.Contains("dressing") || keywords.Contains("condiment"))
            types.Add(ProductType.Condiment);
        if (keywords.Contains("oil") || keywords.Contains("olive oil") || keywords.Contains("cooking oil"))
            types.Add(ProductType.Oil);
        if (keywords.Contains("packaged") || keywords.Contains("processed") || keywords.Contains("instant") ||
            keywords.Contains("ready meal") || keywords.Contains("frozen"))
            types.Add(ProductType.PackagedFood);
        
        

        // If no specific type matches, classify as "Unknown"
        if (types.Count == 0)
            types.Add(ProductType.Unknown);

        return types;
    }
}