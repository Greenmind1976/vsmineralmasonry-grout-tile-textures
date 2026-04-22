# VSMineralMasonry - Grout and Tile Textures

`VSMineralMasonry - Grout and Tile Textures` is the finish-work and surface-customization branch of Mineral Masonry.

It gives builders a way to change how stone faces read without replacing the whole block palette, using grout, colored variants, rock-matched grout, tile patterns, and editing tools that support detail-first building.

## What It Adds

- Standard grout and colored grout variants
- Rock grout and grout-related crafting
- Grout trowel and grout sponge
- Decorative grout tile textures
- Triangle overlay and grout editing helpers

## Best Use Cases

- Adding trim lines and contrast to stone builds
- Creating tiled floors, panels, and decorative wall insets
- Matching grout tones to specific rock families
- Covering mural slab blocks with stone texture grout using the first tileset
- Iterating on details with placement and removal tools instead of rebuilding sections

## Build

Set `VINTAGE_STORY` to your Vintage Story app folder, then build the project:

```bash
dotnet build VSMineralMasonry.GroutTileTextures.csproj -c Release -p:NuGetAudit=false
```

## Release Package

Create a distributable zip with:

```bash
./release.sh
```

## Local Install

Install the built mod into your local Vintage Story app with the script for the game version you are testing:

```bash
./build-122-install.sh
```

For 1.21.7 testing, use `./build-install.sh`. For final 1.22 testing, this branch targets `/Applications/Vintage Story 1.22.app` and requires Vintage Story `1.22.0`.
