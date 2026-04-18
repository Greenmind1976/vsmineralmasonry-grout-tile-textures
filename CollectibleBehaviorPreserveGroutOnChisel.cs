using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;

namespace VSMineralMasonry;

public class CollectibleBehaviorPreserveGroutOnChisel : CollectibleBehavior
{
    public CollectibleBehaviorPreserveGroutOnChisel(CollectibleObject collObj) : base(collObj)
    {
    }

    public override void OnHeldInteractStart(
        ItemSlot slot,
        EntityAgent byEntity,
        BlockSelection blockSel,
        EntitySelection entitySel,
        bool firstEvent,
        ref EnumHandHandling handHandling,
        ref EnumHandling handling)
    {
        if (!firstEvent || blockSel == null || byEntity.World.Side != EnumAppSide.Server)
        {
            return;
        }

        DestroyNearbyGrout(byEntity.World, blockSel.Position);
    }

    private static void DestroyNearbyGrout(IWorldAccessor world, BlockPos origin)
    {
        Block air = world.GetBlock(0);

        foreach (BlockPos pos in CandidatePositions(origin))
        {
            var subDecors = world.BlockAccessor.GetSubDecors(pos);
            if (subDecors == null)
            {
                continue;
            }

            foreach (var entry in subDecors)
            {
                if (!IsEditableDecor(entry.Value))
                {
                    continue;
                }

                world.BlockAccessor.SetDecor(air, pos, entry.Key);
            }
        }
    }

    private static bool IsEditableDecor(Block? block)
    {
        return block is BlockGroutCycle || block is BlockTriangleOverlayCycle;
    }

    private static BlockPos[] CandidatePositions(BlockPos origin)
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
}
