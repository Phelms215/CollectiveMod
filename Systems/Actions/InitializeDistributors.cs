using Collective.Components.DataSets;
using Collective.Components.Definitions;
using Collective.Components.Interfaces;
using Collective.Components.Modals;
using Collective.Systems.Managers;

namespace Collective.Systems.Actions;

public class InitializeDistributors : IGameAction
{
    public void Execute()
    { 
        var distributionManager = Collective.GetManager<DistributionManager>();
        distributionManager.AddDistributor(new Distributor()
        {
            Name = "WelMart",
            Description = "Because you aren't a real store yet anyway",
            MinLevel = 0,
            RespectLevel = 0,
            PaymentTerms = PaymentTerms.Immediate,
            IsMember = true,
            Slug = "wel-mart",
            Type = DistributorType.SmallBusiness,
            JoinCost = 0,
            Icon ="Distro-2",
            ShippingCost = new ShippingCost(65, 5, 2.50f)
        });     
        
        distributionManager.AddDistributor(new Distributor()
        {
            Name = "WholeSale.com",
            Description = "Want to become a millionaire fast? Join WholeSale.com! ",
            MinLevel = 0,
            RespectLevel = 0,
            PaymentTerms = PaymentTerms.Immediate,
            IsMember = false,
            JoinCost = 2500,
            Type = DistributorType.GlobalWarehouse,
            Slug = "whole-sale-com",
            Icon = "online-store",
            ShippingCost = new ShippingCost(9, 4, 0)
        });

        distributionManager.AddDistributor(new Distributor()
        {
            Name = "J&J Distribution",
            Slug = "jj-distribution",
            Description = "We got everything your customers want!",
            MinLevel = 50,
            RespectLevel = 0,
            PaymentTerms = PaymentTerms.Immediate,
            JoinCost = 3200,
            Icon = "Distro-1",
            Type = DistributorType.LargeWarehouse,
            ShippingCost = new ShippingCost(30, 10, 0)
        });
        distributionManager.AddDistributor(new Distributor()
        {
            Name = "Samantha's Club",
            Slug = "samanthas-club",
            Description = "Bulk warehouse deals on all of the most popular products!",
            MinLevel = 5,
            RespectLevel = 0,
            PaymentTerms = PaymentTerms.Immediate,
            JoinCost = 500,
            Icon ="Distro-3",
            Type = DistributorType.GlobalChain,
            ShippingCost = new ShippingCost(15, 10, 4)
        });

        distributionManager.AddDistributor(new Distributor()
        {
            Name = "Local Co-Op",
            Description = "Locally farmed products only!",
            Slug = "local-co-op",
            Type = DistributorType.Farm,
            MinLevel = 3,
            RespectLevel = 0,
            PaymentTerms = PaymentTerms.Immediate,
            JoinCost = 250,
            Icon = "localfarmer",
            ShippingCost = new ShippingCost(5,5, 5)
        });

        
        // Only One Furniture Store today 
        distributionManager.AddDistributor(new Distributor()
        {
            Name = "Furniture Market",
            Slug = "furniture-market",
            Type = DistributorType.Furniture,
            Description = "The only furniture store in town!",
            MinLevel = 0,
            RespectLevel = 0,
            PaymentTerms = PaymentTerms.Immediate,
            IsMember = true,
            JoinCost = 0,
            Icon = "furniture",
            ShippingCost = new ShippingCost(199, 0, -25)
        });
    }
}