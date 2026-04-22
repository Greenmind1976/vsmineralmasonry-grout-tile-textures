using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;

namespace VSMineralMasonry;

public class ItemPlaceGrout : Item
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

        if (TryPlaceGrout(slot, byEntity.World, blockSel))
        {
            handling = EnumHandHandling.Handled;
        }
    }

    private bool TryPlaceGrout(ItemSlot slot, IWorldAccessor world, BlockSelection blockSel)
    {
        string? rock = Variant["rock"];
        string? color = Variant["color"];
        string blockCode = rock != null
            ? $"groutrockvsm-{rock}-blob"
            : $"groutvsm-{color ?? "white"}-blob";

        Block? groutBlock = world.GetBlock(CodeWithPath(blockCode));
        if (groutBlock == null || groutBlock.Id == 0)
        {
            return false;
        }

        BlockPos pos = blockSel.Position;
        if (blockSel.Face == null)
        {
            return false;
        }

        int decorIndex = (int)new DecorBits(blockSel.Face);

        if (HasSameGroutMaterialOnFace(world, pos, blockSel.Face, groutBlock))
        {
            return true;
        }

        if (world.Side != EnumAppSide.Server)
        {
            return true;
        }

        bool placed = world.BlockAccessor.SetDecor(groutBlock, pos, decorIndex);
        if (!placed)
        {
            return false;
        }

        slot.TakeOut(1);
        slot.MarkDirty();
        return true;
    }

    private static bool HasSameGroutMaterialOnFace(IWorldAccessor world, BlockPos pos, BlockFacing face, Block groutBlock)
    {
        int faceIndex = (int)new DecorBits(face);
        if (IsSameGroutMaterial(world.BlockAccessor.GetDecor(pos, faceIndex), groutBlock))
        {
            return true;
        }

        var subDecors = world.BlockAccessor.GetSubDecors(pos);
        if (subDecors == null)
        {
            return false;
        }

        foreach (var entry in subDecors)
        {
            if (entry.Key % 6 == face.Index && IsSameGroutMaterial(entry.Value, groutBlock))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsSameGroutMaterial(Block? existingBlock, Block groutBlock)
    {
        string? existingMaterial = GetGroutMaterialKey(existingBlock);
        string? newMaterial = GetGroutMaterialKey(groutBlock);
        return existingMaterial != null && existingMaterial == newMaterial;
    }

    private static string? GetGroutMaterialKey(Block? block)
    {
        string? path = block?.Code?.Path;
        if (string.IsNullOrEmpty(path))
        {
            return null;
        }

        string[] parts = path.Split('-');
        if (parts.Length < 3)
        {
            return null;
        }

        return parts[0] switch
        {
            "groutvsm" or "grouttestvsm" or "grouttilevsm" => $"color:{parts[1]}",
            "groutrockvsm" or "grouttilerockvsm" => $"rock:{parts[1]}",
            _ => null
        };
    }
}
