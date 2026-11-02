# Using Construction Site Imports

Version **1.0.0**, prepared for Timberborn **1.1.2.8** (Update 1.1).

## What it does

Construction sites create demand for their missing materials. Materials arrive through district crossings without requiring a warehouse for each material.

Construction Site Hauler is optional. Construction Site Imports can be used without it or with both mods enabled.

## How to use it

1. Enable Construction Site Imports and its dependencies: Moddable Timberborn **11.1.0 or later**, TimberUI and Harmony.
2. In the receiving district's distribution settings, set the material to **Import If Needed**.
3. Keep a functioning district crossing staffed and connected to both districts. The supplying district must have materials available and permit their export under its configured threshold.
4. Provide builders with access to the construction site and crossing. Without Construction Site Hauler, builders handle the final delivery. If Construction Site Hauler is also enabled, local haulers can make that delivery according to that mod's rules.

## Example

A construction site needs 20 paper. The neighboring district has paper, but the receiving district has no paper warehouse. The site can request paper through the crossing, and builders carry it to the site. With Construction Site Hauler enabled, local haulers can also make this delivery.

## Behavior

- Accounts for local stock and deliveries already reserved or in transit.
- Requests only the missing amount when construction is the only demand for that material.
- Preserves normal imports when warehouses or workshops also need the material.
- Protects construction materials against immediate re-export.
- Respects disabled imports, forced imports and export thresholds.
- Stops new requests when a site is paused, removed, completed or becomes unreachable.
- Deliveries already dispatched may still arrive after a site is paused or removed.
- Assigns each site to one reachable importing district. Sites without an assigned district use the nearest reachable district center.
- Tracks construction sites independently of Construction Site Hauler.
- Does not create hauler destinations or replace Construction Site Hauler's transport system. The two mods can coexist without registering the same hauling destination twice.

## Manual installation

Copy the `ConstructionSiteImports` folder to `Documents\Timberborn\Mods`.

The resulting structure must include:

```text
Mods\ConstructionSiteImports\version-1.1\ConstructionSiteImports.dll
Mods\ConstructionSiteImports\version-1.1\manifest.json
```

The mod can be disabled in the mod manager. It has not been published on Steam. Construction Site Hauler is not a required dependency.

To build the mod from source, see [BUILDING.md](BUILDING.md).

## Validation

The Release build completed without warnings or errors. **48 checks passed without Construction Site Hauler, and 48 passed with it available in the same test environment.**

The automated checks verify demand calculations, reservations, limits and compatibility with the installed libraries without running the simulation. The full transport flow still needs to be validated in the game, both without Construction Site Hauler and with both mods enabled.

The recorded build details are in [BUILD-INFO.txt](BUILD-INFO.txt).

## References and license

- [Original Construction Site Hauler](https://steamcommunity.com/sharedfiles/filedetails/?id=3779332547).
- [Original repository](https://github.com/datvm/TimberbornMods).
- Upstream revision: [BUILD-INFO.txt](BUILD-INFO.txt).
- [MIT license](LICENSE), included in this repository.
