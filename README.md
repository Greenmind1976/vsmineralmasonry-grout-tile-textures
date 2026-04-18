# VSMineralMasonry - Grout and Tile Textures

Standalone Vintage Story mod for grout placement, grout tools, and decorative tile finish content from VSMineralMasonry.

## Included Content

- Standard grout and colored grout variants
- Rock grout and grout-related crafting
- Grout trowel and grout sponge
- Decorative grout tile textures
- Triangle overlay and grout editing helpers

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

Install the built mod into your local Vintage Story app with:

```bash
./build-install.sh
```
