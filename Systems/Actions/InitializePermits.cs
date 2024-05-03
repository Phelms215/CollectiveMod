using Collective.Components.Interfaces;
using Collective.Components.Modals;
using Collective.Definitions;
using Collective.Systems.Managers;

namespace Collective.Systems.Actions;

public class InitializePermits : IGameAction
{
    public void Execute()
    { 
        var permitManager = Collective.GetManager<PermitManager>(); 
        permitManager.RegisterPermit(new Permit("247Hours")
        {
            Title = "24/7",
            Description = "You've bribed enough of the city counsel. Your store can now stay open 24/7!",
            Level = 50,
            Cost = 25000,
            PreRequirements = new[] { "EarlyBird", "ExtendHours", "LateNightStore" },
            Type = PermitType.StoreHours
        });
        permitManager.RegisterPermit(new Permit("LateNightStore")
        {
            Title = "Late Night Store",
            Description = "Can Stay Open Until 11PM!",
            Level = 40,
            Cost = 2500,
            PreRequirements = new[] { "ExtendHours" },
            Type = PermitType.StoreHours
        });
        permitManager.RegisterPermit(new Permit("ExtendHours")
        {
            Title = "Extended Hours",
            Description = "Can Stay Open Until 9pm",
            Level = 10,
            Cost = 500,
            Type = PermitType.StoreHours
        });
        permitManager.RegisterPermit(new Permit("EarlyBird")
        {
            Title = "Early Bird",
            Description = "Can open up as early as 4am",
            Level = 10,
            Cost = 500,
            Type = PermitType.StoreHours
        });

        permitManager.RegisterPermit(new Permit("BeersLiquor")
        {
            Title = "Beers & Liquor",
            Description = "You can serve your beers & liquor!",
            Level = 40,
            Cost = 10000,
            Type = PermitType.Products
        });
        permitManager.RegisterPermit(new Permit("MeatsSeafood")
        {
            Title = "Meats & Seafood",
            Description = "The health & safety office approved your selling of meat & seafood products!",
            Level = 15,
            Cost = 2500,
            Type = PermitType.Products
        });
        permitManager.RegisterPermit(new Permit("DairyProducts")
        {
            Title = "Dairy Products",
            Description = "Cheeses, Yogurt, Milk. Sell em all you'd like!",
            Level = 5,
            Cost = 500,
            Type = PermitType.Products
        });
    }
}