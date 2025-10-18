using System.Collections.Generic;
using NamPhuThuy.Data;

public struct RewardBundle
{
    private int coinQuantity;
    private int[] boosterQuantities;
    private bool isRemoveAd;

    public int CoinQuantity
    {
        get => coinQuantity;
        set => coinQuantity = value;
    }

    public int[] BoosterQuantities
    {
        get => boosterQuantities;
        set => boosterQuantities = value;
    }

    public bool IsRemoveAd
    {
        get => isRemoveAd;
        set => isRemoveAd = value;
    }

    public RewardBundle(IAPRecord iapRecord)
    {
        coinQuantity = 0;
        boosterQuantities = new int[3];
        isRemoveAd = false;

        int boosterIndex = 0;

        foreach (var item in iapRecord.rewardList)
        {
            if (item.resourceType == ResourceType.COIN)
            {
                coinQuantity = item.amount;
            }
            else if (item.resourceType == ResourceType.BOOSTER)
            {
                boosterQuantities[boosterIndex] = item.amount;

                boosterIndex++;
            }
            else if (item.resourceType == ResourceType.NO_ADS)
            {
                isRemoveAd = true;
            }
        }
    }

    public RewardBundle(List<ResourceReward> rewards)
    {
        coinQuantity = 0;
        boosterQuantities = new int[3];
        isRemoveAd = false;

        int boosterIndex = 0;
        
        foreach (var item in rewards)
        {
            if (item.resourceType == ResourceType.COIN)
            {
                coinQuantity = item.amount;
            }
            else if (item.resourceType == ResourceType.BOOSTER)
            {
                boosterQuantities[boosterIndex] = item.amount;

                boosterIndex++;
            }
            else if (item.resourceType == ResourceType.NO_ADS)
            {
                isRemoveAd = true;
            }
        }
    }
}
