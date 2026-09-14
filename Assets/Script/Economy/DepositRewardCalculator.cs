using UnityEngine;

public static class DepositRewardCalculator
{
    public static int Calculate(int depositedAmount, int bagCapacity, int fullBagBaseReward, float fullBagBonusMultiplier, float incomeMultiplier = 1f)
    {
        if (depositedAmount <= 0 || bagCapacity <= 0 || fullBagBaseReward <= 0)
        {
            return 0;
        }

        int cappedAmount = Mathf.Min(depositedAmount, bagCapacity);
        float progressReward = fullBagBaseReward * (cappedAmount / (float)bagCapacity) * Mathf.Max(0.01f, incomeMultiplier);
        int reward = Mathf.RoundToInt(progressReward);

        if (depositedAmount >= bagCapacity)
        {
            reward += Mathf.RoundToInt(fullBagBaseReward * Mathf.Max(0f, fullBagBonusMultiplier));
        }

        return Mathf.Max(0, reward);
    }
}
