[ Factory Modular Pack - README ]

Thank you for using Factory Modular Pack.

Factory Modular Pack is a modular industrial environment asset pack for creating factory interiors, exteriors, industrial yards, loading areas, and facility scenes in Unity URP.

This package includes modular factory building parts, industrial props, safety equipment, traffic control props, decals, and two demo scenes.

────────────────────────────
◎ Table of Contents
────────────────────────────

1. Package Overview
2. Quick Start
3. Demo Scenes
4. Main Asset Categories
5. Modular Building System
6. Materials and Textures
7. Technical Details
8. Lighting and Environment
9. Colliders, LODs, and Animations
10. Scripts and Code
11. Third-Party Notice
12. Notes

────────────────────────────
1. Package Overview
────────────────────────────

Factory Modular Pack is designed around reusable modular assets and prefabs.

You can use the included sample scene as a complete factory site, or assemble your own factory layouts using the provided building modules, props, and decals.

The package includes:
- Modular factory building parts
- Interior and exterior wall panels
- Roof, floor, ground, door, window, canopy, stair, and structure modules
- Industrial facility props
- Safety and emergency equipment
- Traffic control props
- Road, floor, safety, and zone ID decals
- Exterior sample scene
- Module preview scene

────────────────────────────
2. Quick Start
────────────────────────────

To get started:

1. Open the package in a Unity URP project.
2. Open Scene_Exterior to view the complete factory site sample scene.
3. Open Scene_Module to browse the included modular assets by category.
4. Drag prefabs from the Prefabs folder into your own scene.
5. Use grid snapping, vertex snapping, pivot-based alignment, or manual placement depending on the asset type and layout.
6. If you modify static objects, lighting, or reflection probes, rebake the lighting if needed.

Recommended first scene:
- Scene_Exterior

Recommended asset browsing scene:
- Scene_Module

────────────────────────────
3. Demo Scenes
────────────────────────────

1. Scene_Exterior

Scene_Exterior is the main sample scene.

It contains a complete factory site layout with three factory buildings:

- Factory_A: Large factory
- Factory_B: Medium factory
- Factory_C: Small factory

Each factory includes both exterior and interior areas.

Factory_A and Factory_B include completed interior layouts and prop placement.
Factory_C includes the interior space layout, but does not include detailed interior prop placement.

Scene_Exterior includes:
- Factory exterior and interior layouts
- Roads and parking areas
- Loading dock areas
- Fences and gates
- Containers and storage tanks
- Safety props and decals
- Basic lighting setup
- Reflection probes
- Global volume settings
- Baked lighting

2. Scene_Module

Scene_Module is a module preview scene.

This scene displays the included assets by category, so users can quickly browse the available modular parts, props, decals, and variations.

Scene_Module is intended for asset preview and does not include the same full environment setup as Scene_Exterior.

────────────────────────────
4. Main Asset Categories
────────────────────────────

Building Modules
- Wall Panel
- Roof
- Floor
- Ground
- Door
- Window
- Canopy
- Structure
- Stairs
- Mezzanine Floor
- Flower Bed

Industrial Props
- Storage Tank
- Overhead Crane
- Pipe
- Duct
- Cable Tray
- Vent
- Enclosure
- Solar Panel
- Container

Site & Safety Props
- Fence
- Traffic Control
- Safety Equipment
- Lighting
- Decals

Safety Equipment includes items such as:
- Fire Extinguisher
- Hydrant Box
- First Aid Kit Box
- Safety Mirror
- Emergency Exit Props

Traffic Control includes items such as:
- Bollards
- Parking Barriers
- Safety Cones
- Traffic-related site props

Decals include:
- Road Markings
- Parking Lines
- Hazard Areas
- Safety Signs
- Zone ID Markings
- Direction Arrows

────────────────────────────
5. Modular Building System
────────────────────────────

Most building assets are provided as modular prefabs.

Wall panels, roof panels, ground panels, structures, stairs, pipes, ducts, cable trays, fences, and decals are designed to be combined, repeated, or aligned depending on the required layout.

Depending on the asset type, modules can be placed using grid snapping, vertex snapping, pivot-based alignment, or manual placement.

Some wall and structure modules are also provided as pre-assembled set prefabs for easier placement.

Common naming notes:

- 2P
  Two-piece wall or structure set.

- 3P
  Three-piece wall or structure set.

- Standard
  Standard structure type without overhead crane-ready rail support.

- CraneReady
  Structure type prepared for overhead crane placement.

- Flat
  Flat roof type.

- Gable
  Gable roof type.

Example:
PR06_Building_Structure_Wall_Flat_3P_CraneReady_Default

This means:
- Flat roof wall structure
- Three-piece set
- Crane-ready type
- Default variation

────────────────────────────
6. Materials and Textures
────────────────────────────

This package uses URP-ready materials.

Texture information:
- Texture Resolution: 2048
- Texture Maps: Albedo / MAOG / NRM

Texture map notes:
- Albedo: Base color texture
- MAOG: Packed material texture
- NRM: Normal map

────────────────────────────
7. Technical Details
────────────────────────────

- Unity Version: 2021.3.45f2
- Tested Versions: 2022.3.62f2 / Unity 6.3 LTS
- Render Pipeline: Universal Render Pipeline, URP
- Target Platform: PC
- Poly Level: Mid Poly
- Texture Resolution: 2048
- Texture Maps: Albedo / MAOG / NRM
- Demo Scenes: 2
- Colliders: Not included
- LODs: Not included
- Animations: Not included

────────────────────────────
8. Lighting and Environment
────────────────────────────

Scene_Exterior includes:
- Basic lighting setup
- Reflection probes
- Global volume settings
- Baked lighting

Scene_Module is intended as a preview scene and does not include a full environment setup.

The included scenes use Unity’s default skybox.

Lighting and baked results may look slightly different depending on the Unity version, URP settings, graphics settings, or lighting settings used in your project.

If the scene lighting appears different after importing the package into another Unity version, rebaking the lighting is recommended.

If you modify lighting, reflection probes, or static objects in Scene_Exterior, rebaking the lighting may also be required.

Basic lighting workflow:
1. Open Scene_Exterior.
2. Check the Lighting window.
3. Check the scene lighting, reflection probes, and global volume settings.
4. If lighting appears incorrect after opening the scene in another Unity version, clear the existing lighting data if needed.
5. Rebuild baked lighting from the Lighting window.

────────────────────────────
9. Colliders, LODs, and Animations
────────────────────────────

Colliders are not included.
Please add colliders depending on the needs of your project.

LODs are not included.

Animations are not included.
Objects such as overhead cranes, doors, shutters, or parking barriers are provided as static assets unless otherwise noted.

────────────────────────────
10. Scripts and Code
────────────────────────────

This package does not require custom scripts to use the included assets.

The package is primarily composed of:
- Static meshes
- Prefabs
- Materials
- Textures
- Decals
- Demo scenes

No custom runtime system is required for the included demo scenes.

If your project requires interaction, collision, gameplay logic, animations, or custom behavior, please add your own scripts depending on your project requirements.

────────────────────────────
11. Third-Party Notice
────────────────────────────

Promotional screenshots were created using the Poly Haven HDRI:

Kloofendal 48d Partly Cloudy Pure Sky

File used for promotional screenshots:
kloofendal_48d_partly_cloudy_puresky_2k

Source:
https://polyhaven.com/a/kloofendal_48d_partly_cloudy_puresky

License:
CC0

This HDRI is not included in the package.

The included demo scenes use Unity’s default skybox.

────────────────────────────
12. Notes
────────────────────────────

- Some assets include multiple variations.
- Some modular parts are provided individually and also as pre-assembled set prefabs.
- Scene_Exterior is provided as a sample layout and can be modified freely.
- Scene_Module is provided for browsing and checking included assets.
- Add colliders, LODs, and animations as needed for your project.
- Lighting may need to be rebaked when opening or modifying the scene in a different Unity version.
- Some modules are intended for repeated placement, while others may require vertex snapping, pivot-based alignment, or manual adjustment depending on the layout.