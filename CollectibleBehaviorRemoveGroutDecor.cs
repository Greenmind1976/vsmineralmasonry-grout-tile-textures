using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;

namespace VSMineralMasonry;

public class CollectibleBehaviorRemoveGroutDecor : CollectibleBehavior
{
    public CollectibleBehaviorRemoveGroutDecor(CollectibleObject collObj) : base(collObj)
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
        if (!firstEvent || blockSel == null || collObj.Tool != EnumTool.Wrench)
        {
            return;
        }

        DecorEditingHelper.DecorTarget? target = DecorEditingHelper.GetSelectedDecor(byEntity.World, blockSel);
        if (target?.Block is not BlockGroutCycle)
        {
            return;
        }

        if (byEntity.World.Side != EnumAppSide.Server)
        {
            handHandling = EnumHandHandling.Handled;
            handling = EnumHandling.PreventDefault;
            return;
        }

        Block air = byEntity.World.GetBlock(0);
        if (byEntity.World.BlockAccessor.SetDecor(air, target.Position, target.DecorIndex))
        {
            handHandling = EnumHandHandling.Handled;
            handling = EnumHandling.PreventDefault;
        }
    }
}
