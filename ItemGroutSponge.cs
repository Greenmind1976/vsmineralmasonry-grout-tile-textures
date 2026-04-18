using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;

namespace VSMineralMasonry;

public class ItemGroutSponge : Item
{
    public override void OnHeldInteractStart(
        ItemSlot slot,
        EntityAgent byEntity,
        BlockSelection blockSel,
        EntitySelection entitySel,
        bool firstEvent,
        ref EnumHandHandling handling)
    {
        if (!firstEvent || blockSel == null)
        {
            return;
        }

        if (TryRemoveDecor(byEntity.World, blockSel))
        {
            handling = EnumHandHandling.Handled;
        }
    }

    private static bool TryRemoveDecor(IWorldAccessor world, BlockSelection blockSel)
    {
        DecorEditingHelper.DecorTarget? target = DecorEditingHelper.GetSelectedDecor(world, blockSel);
        if (target == null || !IsRemovableBySponge(target.Block))
        {
            return false;
        }

        if (world.Side != EnumAppSide.Server)
        {
            return true;
        }

        Block air = world.GetBlock(0);
        return world.BlockAccessor.SetDecor(air, target.Position, target.DecorIndex);
    }

    private static bool IsRemovableBySponge(Block? block)
    {
        return block is BlockGroutCycle || block is BlockTriangleOverlayCycle;
    }
}
