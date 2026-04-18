using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace VSMineralMasonry;

public class BlockGroutCycle : Block
{
    public static readonly string[] Parts =
    {
        "frame",
        "top",
        "left",
        "right",
        "bottom",
        "topleft",
        "topright",
        "bottomleft",
        "bottomright",
        "blob"
    };

    public override ItemStack OnPickBlock(IWorldAccessor world, BlockPos pos)
    {
        return new ItemStack(GetBaseVariant(world));
    }

    public override ItemStack[] GetDrops(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1)
    {
        return [];
    }

    private Block GetBaseVariant(IWorldAccessor world)
    {
        Block? block = world.GetBlock(CodeWithParts(Parts[0]));
        return block ?? this;
    }

    public bool TryCycleAt(IWorldAccessor world, BlockPos pos)
    {
        string currentPart = LastCodePart(0) ?? Parts[0];
        int currentIndex = 0;
        for (int i = 0; i < Parts.Length; i++)
        {
            if (Parts[i] == currentPart)
            {
                currentIndex = i;
                break;
            }
        }

        string nextPart = Parts[(currentIndex + 1) % Parts.Length];
        Block? nextBlock = world.GetBlock(CodeWithParts(nextPart));
        if (nextBlock == null || nextBlock.Id == 0 || nextBlock.Id == Id)
        {
            return false;
        }

        world.BlockAccessor.ExchangeBlock(nextBlock.Id, pos);
        return true;
    }
}
