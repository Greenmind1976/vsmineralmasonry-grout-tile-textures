using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace VSMineralMasonry;

public static class DecorEditingHelper
{
    public sealed class DecorTarget
    {
        public required BlockPos Position { get; init; }
        public required int DecorIndex { get; init; }
        public required Block Block { get; init; }
    }

    public static DecorTarget? GetSelectedDecor(IWorldAccessor world, BlockSelection? blockSel)
    {
        if (blockSel == null || blockSel.Position == null)
        {
            return null;
        }

        try
        {
            foreach (BlockPos pos in CandidatePositions(blockSel))
            {
                DecorTarget? target = GetSelectedDecorAt(world, pos, blockSel);
                if (target != null)
                {
                    return target;
                }
            }
        }
        catch
        {
            // Tool-mode queries can run while the client is in a transient selection state.
            // Failing closed here avoids crashing the game when no stable decor target exists.
            return null;
        }

        return null;
    }

    public static bool IsEditableDecor(Block? block)
    {
        return block is BlockGroutCycle
            || block is BlockTriangleOverlayCycle;
    }

    private static DecorTarget? GetSelectedDecorAt(IWorldAccessor world, BlockPos pos, BlockSelection blockSel)
    {
        if (blockSel.Face == null)
        {
            return null;
        }

        int exactIndex = blockSel.ToDecorIndex();
        Block? decor = world.BlockAccessor.GetDecor(pos, exactIndex);
        if (IsEditableDecor(decor))
        {
            return new DecorTarget { Position = pos.Copy(), DecorIndex = exactIndex, Block = decor };
        }

        int faceIndex = (int)new DecorBits(blockSel.Face);
        decor = world.BlockAccessor.GetDecor(pos, faceIndex);
        if (IsEditableDecor(decor))
        {
            return new DecorTarget { Position = pos.Copy(), DecorIndex = faceIndex, Block = decor };
        }

        var subDecors = world.BlockAccessor.GetSubDecors(pos);
        if (subDecors != null)
        {
            foreach (var entry in subDecors)
            {
                if (IsEditableDecor(entry.Value) && entry.Key % 6 == blockSel.Face.Index)
                {
                    return new DecorTarget { Position = pos.Copy(), DecorIndex = entry.Key, Block = entry.Value };
                }
            }
        }

        return null;
    }

    private static BlockPos[] CandidatePositions(BlockSelection blockSel)
    {
        BlockPos? origin = blockSel.Position;
        if (origin == null)
        {
            return [];
        }

        if (blockSel.Face == null)
        {
            return
            [
                origin.Copy(),
                origin.AddCopy(BlockFacing.NORTH),
                origin.AddCopy(BlockFacing.EAST),
                origin.AddCopy(BlockFacing.SOUTH),
                origin.AddCopy(BlockFacing.WEST),
                origin.AddCopy(BlockFacing.UP),
                origin.AddCopy(BlockFacing.DOWN)
            ];
        }

        return
        [
            origin.Copy(),
            origin.AddCopy(blockSel.Face),
            origin.AddCopy(blockSel.Face.Opposite),
            origin.AddCopy(BlockFacing.NORTH),
            origin.AddCopy(BlockFacing.EAST),
            origin.AddCopy(BlockFacing.SOUTH),
            origin.AddCopy(BlockFacing.WEST),
            origin.AddCopy(BlockFacing.UP),
            origin.AddCopy(BlockFacing.DOWN)
        ];
    }
}
