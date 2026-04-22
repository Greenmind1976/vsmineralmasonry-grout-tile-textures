using Cairo;
using System.Collections.Generic;
using System.IO;
using IOPath = System.IO.Path;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;

namespace VSMineralMasonry;

public class ItemGroutTrowel : Item
{
    private const string ToolModeCodeAttribute = "vsmineralmasonry:trowelShapeCode";
    private static readonly string[] GroutShapeModes =
    [
        "solid",
        "tileset1",
        "tileset2",
        "tileset3",
        "tileset4",
        "tileset5",
        "tileset6",
        "tileset7",
        "tileset8",
        "tileset9",
        "tileset10",
        "tileset11",
        "tileset12",
        "tileset13",
        "tileset14",
        "tileset15",
        "tileset16",
        "border"
    ];
    private static readonly string[] GroutShapeLabels =
    [
        "Solid",
        "Tileset 1",
        "Tileset 2",
        "Tileset 3",
        "Tileset 4",
        "Tileset 5",
        "Tileset 6",
        "Tileset 7",
        "Tileset 8",
        "Tileset 9",
        "Tileset 10",
        "Tileset 11",
        "Tileset 12",
        "Tileset 13",
        "Tileset 14",
        "Tileset 15",
        "Tileset 16",
        "Border"
    ];
    private static readonly string[] BorderParts =
    [
        "frame",
        "top",
        "right",
        "bottom",
        "left",
        "topleft",
        "topright",
        "bottomright",
        "bottomleft"
    ];
    private static readonly string[] TilesetParts =
    [
        "top",
        "right",
        "bottom",
        "left"
    ];
    private static readonly Dictionary<string, string[]> ShapePartsByCode = new()
    {
        ["solid"] = ["frame"],
        ["tileset1"] = ["top", "right"],
        ["tileset2"] = TilesetParts,
        ["tileset3"] = ["top", "right"],
        ["tileset4"] = ["top"],
        ["tileset5"] = TilesetParts,
        ["tileset6"] = ["top"],
        ["tileset7"] = ["top"],
        ["tileset8"] = ["top", "right"],
        ["tileset9"] = ["top"],
        ["tileset10"] = BorderParts,
        ["tileset11"] = ["top"],
        ["tileset12"] = ["top"],
        ["tileset13"] = ["top"],
        ["tileset14"] = ["top"],
        ["tileset15"] = ["top"],
        ["tileset16"] = TilesetParts,
        ["border"] = BorderParts
    };
    private ICoreClientAPI? capi;
    private string? modRootPath;
    private SkillItem[]? defaultToolModes;
    private readonly Dictionary<string, SkillItem[]> toolModeCache = [];
    private readonly Dictionary<string, ImageSurface> iconSurfaceCache = [];

    public override void OnLoaded(ICoreAPI api)
    {
        base.OnLoaded(api);
        modRootPath = IOPath.GetDirectoryName(GetType().Assembly.Location);

        if (api is ICoreClientAPI)
        {
            capi = (ICoreClientAPI)api;
            defaultToolModes = BuildToolModes(GroutShapeModes, GroutShapeLabels);
        }
    }

    public override SkillItem[] GetToolModes(ItemSlot slot, IClientPlayer forPlayer, BlockSelection blockSel)
    {
        DecorEditingHelper.DecorTarget? target = blockSel == null ? null : DecorEditingHelper.GetSelectedDecor(forPlayer.Entity.World, blockSel);
        if (target == null)
        {
            return defaultToolModes ?? BuildToolModes(GroutShapeModes, GroutShapeLabels);
        }

        return BuildToolModes(GetShapeParts(target.Block), GetShapeLabels(target.Block));
    }

    public override void OnUnloaded(ICoreAPI api)
    {
        base.OnUnloaded(api);

        foreach (SkillItem[] modes in toolModeCache.Values)
        {
            foreach (SkillItem mode in modes)
            {
                mode.Dispose();
            }
        }

        foreach (ImageSurface surface in iconSurfaceCache.Values)
        {
            surface.Dispose();
        }

        toolModeCache.Clear();
        iconSurfaceCache.Clear();
        defaultToolModes = null;
        capi = null;
        modRootPath = null;
    }

    public override int GetToolMode(ItemSlot slot, IPlayer byPlayer, BlockSelection blockSelection)
    {
        ItemStack? stack = slot.Itemstack;
        if (stack == null)
        {
            return 0;
        }

        Block? block = blockSelection == null ? null : DecorEditingHelper.GetSelectedDecor(byPlayer.Entity.World, blockSelection)?.Block;
        string[] shapeParts = block == null ? GroutShapeModes : GetShapeParts(block);
        string selectedCode = stack.Attributes.GetString(ToolModeCodeAttribute, shapeParts[0]);

        for (int i = 0; i < shapeParts.Length; i++)
        {
            if (shapeParts[i] == selectedCode)
            {
                return i;
            }
        }

        return 0;
    }

    public override void SetToolMode(ItemSlot slot, IPlayer byPlayer, BlockSelection blockSelection, int toolMode)
    {
        ItemStack? stack = slot.Itemstack;
        if (stack == null)
        {
            return;
        }

        Block? block = blockSelection == null ? null : DecorEditingHelper.GetSelectedDecor(byPlayer.Entity.World, blockSelection)?.Block;
        string[] shapeParts = block == null ? GroutShapeModes : GetShapeParts(block);
        int clampedMode = GameMath.Clamp(toolMode, 0, shapeParts.Length - 1);
        stack.Attributes.SetString(ToolModeCodeAttribute, shapeParts[clampedMode]);
        slot.MarkDirty();
    }

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

        if (TryCycleTarget(slot, byEntity, blockSel))
        {
            handling = EnumHandHandling.Handled;
        }
    }

    private static bool TryCycleTarget(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel)
    {
        IWorldAccessor world = byEntity.World;
        DecorEditingHelper.DecorTarget? target = DecorEditingHelper.GetSelectedDecor(world, blockSel);

        if (target == null || !IsEditableByTrowel(target.Block))
        {
            return false;
        }

        if (world.Side != EnumAppSide.Server)
        {
            return true;
        }

        Block? nextBlock = GetNextCycleBlock(slot, world, target.Block);
        if (nextBlock == null || nextBlock.Id == 0)
        {
            return false;
        }

        bool changed = world.BlockAccessor.SetDecor(nextBlock, target.Position, target.DecorIndex);
        if (changed)
        {
            slot.Itemstack?.Collectible.DamageItem(world, byEntity, slot, 1, false);
        }

        return changed;
    }

    private static Block? GetNextCycleBlock(ItemSlot slot, IWorldAccessor world, Block block)
    {
        if (block is BlockGroutCycle grout)
        {
            return GetNextGroutBlock(slot, world, grout);
        }

        if (block is BlockTriangleOverlayCycle triangle)
        {
            return GetNextBlock(world, triangle, BlockTriangleOverlayCycle.Parts);
        }

        return null;
    }

    private static bool IsEditableByTrowel(Block? block)
    {
        return block is BlockGroutCycle || block is BlockTriangleOverlayCycle;
    }

    private static Block? GetNextGroutBlock(ItemSlot slot, IWorldAccessor world, Block block)
    {
        ItemStack? stack = slot.Itemstack;
        if (stack == null)
        {
            return null;
        }

        if (!IsStandardGroutBlock(block))
        {
            string[] shapeParts = GetShapeParts(block);
            string selectedPart = stack.Attributes.GetString(ToolModeCodeAttribute, shapeParts[0]);

            bool isValidPart = false;
            foreach (string shapePart in shapeParts)
            {
                if (shapePart == selectedPart)
                {
                    isValidPart = true;
                    break;
                }
            }

            if (!isValidPart)
            {
                selectedPart = shapeParts[0];
            }

            return world.GetBlock(block.CodeWithParts(selectedPart));
        }

        string selectedShape = GetSelectedShapeCode(slot, block);
        string color = block.Variant?["color"] ?? "black";
        string currentPart = block.LastCodePart(0) ?? "blob";
        string currentShape = GetCurrentGroutShapeCode(block, currentPart);

        string nextPart = currentShape == selectedShape
            ? GetNextPartForShape(selectedShape, currentPart)
            : GetDefaultPartForShape(selectedShape);

        AssetLocation code = GetCodeForShape(block, color, selectedShape, nextPart);
        return world.GetBlock(code);
    }

    private static Block? GetNextBlock(IWorldAccessor world, Block block, string[] parts)
    {
        string currentPart = block.LastCodePart(0) ?? parts[0];
        int currentIndex = 0;
        for (int i = 0; i < parts.Length; i++)
        {
            if (parts[i] == currentPart)
            {
                currentIndex = i;
                break;
            }
        }

        string nextPart = parts[(currentIndex + 1) % parts.Length];
        return world.GetBlock(block.CodeWithParts(nextPart));
    }

    private SkillItem[] BuildToolModes(string[] shapeParts, string[] shapeLabels)
    {
        string cacheKey = string.Join("|", shapeParts) + "||" + string.Join("|", shapeLabels);
        if (toolModeCache.TryGetValue(cacheKey, out SkillItem[]? cachedModes))
        {
            return cachedModes;
        }

        var modes = new SkillItem[shapeParts.Length];
        for (int i = 0; i < shapeParts.Length; i++)
        {
            string label = i < shapeLabels.Length && !string.IsNullOrWhiteSpace(shapeLabels[i])
                ? shapeLabels[i]
                : ShapeLabel(shapeParts[i]);
            modes[i] = new SkillItem { Code = new AssetLocation(shapeParts[i]), Name = label };
            if (capi != null)
            {
                string iconCode = shapeParts[i];
                modes[i].WithIcon(capi, (cr, x, y, w, h, rgba) => DrawShapeIcon(cr, x, y, w, h, rgba, iconCode));
            }
        }

        toolModeCache[cacheKey] = modes;
        return modes;
    }

    private static string[] GetShapeParts(Block? block)
    {
        if (IsStandardGroutBlock(block))
        {
            string? configuredGrout = block?.Attributes?["trowelShapeParts"]?.AsString();
            return ParseCsv(configuredGrout, GroutShapeModes);
        }

        string? configured = block?.Attributes?["trowelShapeParts"]?.AsString();
        return ParseCsv(configured, DefaultShapeParts());
    }

    private static string[] GetShapeLabels(Block? block)
    {
        if (IsStandardGroutBlock(block))
        {
            string? configuredGrout = block?.Attributes?["trowelShapeLabels"]?.AsString();
            return ParseCsv(configuredGrout, GroutShapeLabels);
        }

        string? configured = block?.Attributes?["trowelShapeLabels"]?.AsString();
        return ParseCsv(configured, DefaultShapeLabels());
    }

    private static string[] DefaultShapeParts()
    {
        return
        [
            "frame",
            "top",
            "right",
            "bottom",
            "left",
            "topleft",
            "topright",
            "bottomright",
            "bottomleft",
            "blob"
        ];
    }

    private static string[] DefaultShapeLabels()
    {
        return
        [
            "Full Border",
            "Top",
            "Right",
            "Bottom",
            "Left",
            "Top Left",
            "Top Right",
            "Bottom Right",
            "Bottom Left",
            "Blob"
        ];
    }

    private static string[] ParseCsv(string? configured, string[] fallback)
    {
        if (string.IsNullOrWhiteSpace(configured))
        {
            return fallback;
        }

        string[] parts = configured.Split(',');
        int count = 0;
        for (int i = 0; i < parts.Length; i++)
        {
            string trimmed = parts[i].Trim();
            if (trimmed.Length == 0)
            {
                continue;
            }

            parts[count++] = trimmed;
        }

        if (count == 0)
        {
            return fallback;
        }

        if (count == parts.Length)
        {
            return parts;
        }

        var compacted = new string[count];
        for (int i = 0; i < count; i++)
        {
            compacted[i] = parts[i];
        }

        return compacted;
    }

    private static string ShapeLabel(string code)
    {
        return code switch
        {
            "solid" => "Solid",
            "tileset1" => "Tileset 1",
            "tileset2" => "Tileset 2",
            "tileset3" => "Tileset 3",
            "tileset4" => "Tileset 4",
            "tileset5" => "Tileset 5",
            "tileset6" => "Tileset 6",
            "tileset7" => "Tileset 7",
            "tileset8" => "Tileset 8",
            "tileset9" => "Tileset 9",
            "tileset10" => "Tileset 10",
            "tileset11" => "Tileset 11",
            "tileset12" => "Tileset 12",
            "tileset13" => "Tileset 13",
            "tileset14" => "Tileset 14",
            "tileset15" => "Tileset 15",
            "tileset16" => "Tileset 16",
            "border" => "Border",
            "frame" => "Full Border",
            "topleft" => "Top Left",
            "topright" => "Top Right",
            "bottomleft" => "Bottom Left",
            "bottomright" => "Bottom Right",
            "blob" => "Blob",
            _ when code.Length > 0 => char.ToUpperInvariant(code[0]) + code[1..],
            _ => "Shape"
        };
    }

    private static string GetSelectedShapeCode(ItemSlot slot, Block block)
    {
        ItemStack? stack = slot.Itemstack;
        if (stack == null)
        {
            return GetShapeParts(block)[0];
        }

        string[] shapeParts = GetShapeParts(block);
        string selectedShape = stack.Attributes.GetString(ToolModeCodeAttribute, shapeParts[0]);
        foreach (string shapePart in shapeParts)
        {
            if (shapePart == selectedShape)
            {
                return selectedShape;
            }
        }

        return shapeParts[0];
    }

    private static string GetCurrentGroutShapeCode(Block block, string currentPart)
    {
        string? tileset = block.Variant?["tileset"];
        if (!string.IsNullOrWhiteSpace(tileset))
        {
            return tileset;
        }

        return currentPart == "blob" ? "blob" : "border";
    }

    private static string GetDefaultPartForShape(string shapeCode)
    {
        return GetPartsForShape(shapeCode)[0];
    }

    private static string GetNextPartForShape(string shapeCode, string currentPart)
    {
        string[] parts = GetPartsForShape(shapeCode);
        int currentIndex = -1;
        for (int i = 0; i < parts.Length; i++)
        {
            if (parts[i] == currentPart)
            {
                currentIndex = i;
                break;
            }
        }

        if (currentIndex < 0)
        {
            return parts[0];
        }

        return parts[(currentIndex + 1) % parts.Length];
    }

    private static string[] GetPartsForShape(string shapeCode)
    {
        return ShapePartsByCode.TryGetValue(shapeCode, out string[]? parts) ? parts : TilesetParts;
    }

    private static AssetLocation GetCodeForShape(Block block, string color, string shapeCode, string part)
    {
        string? rock = block.Variant?["rock"];
        string path;

        if (!string.IsNullOrWhiteSpace(rock))
        {
            path = shapeCode == "border"
                ? $"groutrockvsm-{rock}-{part}"
                : $"grouttilerockvsm-{rock}-{shapeCode}-{part}";
        }
        else
        {
            path = shapeCode == "border"
                ? $"groutvsm-{color}-{part}"
                : $"grouttilevsm-{color}-{shapeCode}-{part}";
        }

        return new AssetLocation(block.Code.Domain, path);
    }

    private static bool IsStandardGroutBlock(Block? block)
    {
        string? path = block?.Code?.Path;
        return path != null
            && (path.StartsWith("groutvsm-")
                || path.StartsWith("grouttestvsm-")
                || path.StartsWith("grouttilevsm-")
                || path.StartsWith("groutrockvsm-")
                || path.StartsWith("grouttilerockvsm-"));
    }

#pragma warning disable IDE0060
    private void DrawShapeIcon(Context cr, int x, int y, float width, float height, double[] rgba, string shapeCode)
#pragma warning restore IDE0060
    {
        string? primaryPath = GetPrimaryIconPath(shapeCode);
        if (primaryPath != null)
        {
            DrawTextureIcon(cr, x, y, width, height, GetIconSurface(primaryPath), 0f, 0f, 1f);

            string? shadowPath = GetShadowIconPath(shapeCode);
            if (shadowPath != null)
            {
                DrawTextureIcon(cr, x, y, width, height, GetIconSurface(shadowPath), 1.25f, 1.25f, 0.9f);
                DrawTextureIcon(cr, x, y, width, height, GetIconSurface(primaryPath), 0f, 0f, 1f);
            }

            return;
        }

        DrawFallbackShapeIcon(cr, x, y, width, height, shapeCode);
    }

    private static void DrawFallbackShapeIcon(Context cr, int x, int y, float width, float height, string shapeCode)
    {
        cr.Save();
        cr.Translate(x, y);
        cr.Scale(width / 48.0, height / 48.0);
        cr.SetSourceRGBA(1, 1, 1, 1);

        switch (shapeCode)
        {
            case "tileset1":
                DrawEdge(cr, 12, 8, 24, 8);
                break;
            case "tileset2":
                DrawEdge(cr, 28, 12, 8, 24);
                break;
            case "tileset3":
                DrawEdge(cr, 12, 32, 24, 8);
                break;
            case "tileset4":
                DrawEdge(cr, 8, 12, 8, 24);
                break;
            case "tileset5":
                DrawEdge(cr, 12, 8, 24, 8);
                DrawEdge(cr, 28, 12, 8, 24);
                break;
            case "tileset6":
                DrawEdge(cr, 28, 12, 8, 24);
                DrawEdge(cr, 12, 32, 24, 8);
                break;
            case "tileset7":
                DrawEdge(cr, 12, 32, 24, 8);
                DrawEdge(cr, 8, 12, 8, 24);
                break;
            case "tileset8":
                DrawEdge(cr, 8, 12, 8, 24);
                DrawEdge(cr, 12, 8, 24, 8);
                break;
            case "tileset9":
                DrawEdge(cr, 8, 12, 8, 24);
                DrawEdge(cr, 28, 12, 8, 24);
                break;
            case "border":
            case "frame":
                cr.Rectangle(8, 8, 32, 32);
                cr.Rectangle(16, 16, 16, 16);
                cr.FillRule = FillRule.EvenOdd;
                cr.Fill();
                break;
            case "top":
                DrawEdge(cr, 10, 8, 28, 8);
                break;
            case "right":
                DrawEdge(cr, 30, 10, 8, 28);
                break;
            case "bottom":
                DrawEdge(cr, 10, 32, 28, 8);
                break;
            case "left":
                DrawEdge(cr, 8, 10, 8, 28);
                break;
            case "topleft":
                DrawCorner(cr, true, true);
                break;
            case "topright":
                DrawCorner(cr, false, true);
                break;
            case "bottomright":
                DrawCorner(cr, false, false);
                break;
            case "bottomleft":
                DrawCorner(cr, true, false);
                break;
            case "blob":
                cr.Arc(24, 24, 11, 0, 2 * System.Math.PI);
                cr.Fill();
                break;
            default:
                cr.Rectangle(12, 12, 24, 24);
                cr.Fill();
                break;
        }

        cr.Restore();
    }

    private static void DrawEdge(Context cr, double x, double y, double w, double h)
    {
        cr.Rectangle(x, y, w, h);
        cr.Fill();
    }

    private static void DrawCorner(Context cr, bool left, bool top)
    {
        double edge = 8;
        double longSpan = 24;
        double x = left ? 8 : 32;
        double y = top ? 8 : 32;
        double hx = left ? 8 : 16;
        double hy = top ? 8 : 16;
        double vx = left ? 8 : 32;
        double vy = top ? 8 : 16;

        cr.Rectangle(hx, y, longSpan, edge);
        cr.Rectangle(x, vy, edge, longSpan);
        cr.Fill();
    }

    private string? GetPrimaryIconPath(string shapeCode)
    {
        return GetIconPath(shapeCode, true);
    }

    private string? GetShadowIconPath(string shapeCode)
    {
        return GetIconPath(shapeCode, false);
    }

    private string? GetIconPath(string shapeCode, bool whiteVariant)
    {
        string color = whiteVariant ? "white" : "black";
        string representativePart = GetPartsForShape(shapeCode)[0];
        string? relativePath = shapeCode switch
        {
            var code when code.StartsWith("tileset", System.StringComparison.Ordinal)
                => IOPath.Combine("assets", "vsmineralmasonrygrouttiles", "textures", "block", "stone", "grouttilecolor", $"{shapeCode}-{color}-{representativePart}.png"),
            "border" or "frame"
                => IOPath.Combine("assets", "vsmineralmasonrygrouttiles", "textures", "block", "stone", "grout", $"{color}-frame.png"),
            "top" or "right" or "bottom" or "left" or "topleft" or "topright" or "bottomright" or "bottomleft"
                => IOPath.Combine("assets", "vsmineralmasonrygrouttiles", "textures", "block", "stone", "triangleoverlay", $"whitemarble-{shapeCode}.png"),
            "blob"
                => IOPath.Combine("assets", "vsmineralmasonrygrouttiles", "textures", "block", "stone", "grout", $"{color}-blob.png"),
            _ => null
        };

        if (relativePath == null || modRootPath == null)
        {
            return null;
        }

        string fullPath = IOPath.Combine(modRootPath, relativePath);
        return File.Exists(fullPath) ? fullPath : null;
    }

    private ImageSurface GetIconSurface(string path)
    {
        if (!iconSurfaceCache.TryGetValue(path, out ImageSurface? surface))
        {
            surface = new ImageSurface(path);
            iconSurfaceCache[path] = surface;
        }

        return surface;
    }

    private static void DrawTextureIcon(Context cr, int x, int y, float width, float height, ImageSurface surface, float offsetX, float offsetY, double alpha)
    {
        cr.Save();

        double usableWidth = System.Math.Max(1, width - 8);
        double usableHeight = System.Math.Max(1, height - 8);
        double scale = System.Math.Min(usableWidth / surface.Width, usableHeight / surface.Height);
        double drawWidth = surface.Width * scale;
        double drawHeight = surface.Height * scale;
        double drawX = x + ((width - drawWidth) / 2.0) + offsetX;
        double drawY = y + ((height - drawHeight) / 2.0) + offsetY;

        cr.Translate(drawX, drawY);
        cr.Scale(scale, scale);
        cr.SetSourceSurface(surface, 0, 0);
        cr.PaintWithAlpha(alpha);

        cr.Restore();
    }
}
