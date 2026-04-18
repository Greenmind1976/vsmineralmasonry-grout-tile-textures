using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace VSMineralMasonry;

public class BlockTriangleOverlayCycle : Block
{
    public static readonly string[] Parts =
    {
        "topleft",
        "topright",
        "bottomright",
        "bottomleft"
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
}
