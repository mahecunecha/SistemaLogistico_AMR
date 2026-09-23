This file is a merged representation of the entire codebase, combined into a single document by Repomix.

# File Summary

## Purpose
This file contains a packed representation of the entire repository's contents.
It is designed to be easily consumable by AI systems for analysis, code review,
or other automated processes.

## File Format
The content is organized as follows:
1. This summary section
2. Repository information
3. Directory structure
4. Repository files (if enabled)
5. Multiple file entries, each consisting of:
  a. A header with the file path (## File: path/to/file)
  b. The full contents of the file in a code block

## Usage Guidelines
- This file should be treated as read-only. Any changes should be made to the
  original repository files, not this packed version.
- When processing this file, use the file path to distinguish
  between different files in the repository.
- Be aware that this file may contain sensitive information. Handle it with
  the same level of security as you would the original repository.

## Notes
- Some files may have been excluded based on .gitignore rules and Repomix's configuration
- Binary files are not included in this packed representation. Please refer to the Repository Structure section for a complete list of file paths, including binary files
- Files matching patterns in .gitignore are excluded
- Files matching default ignore patterns are excluded
- Files are sorted by Git change count (files with more changes are at the bottom)

# Directory Structure
````
.agents/
  agents/
    Arquitecto_Modulo4_MVP.md
  skills/
    funplay-unity-mcp-workflow/
      SKILL.md
    funplay-unity-ui-composition/
      SKILL.md
  mcp_config.json
.funplay/
  skills/
    manifest.json
Assets/
  Art/
    Fonts/
      RobotoSlab-VariableFont_wght.ttf
  Scripts/
    DashboardUI.cs
    GeneradorTablaManifiesto.cs
    GestorJSON.cs
    RobotAMR.cs
    RobotManagerEVE.cs
  StreamingAssets/
    manifiesto.json
UserSettings/
  Layouts/
    CurrentMaximizeLayout.dwlt
    default-2022.dwlt
  FunplayMcpSettings.json
  Search.settings
.gitignore
.repomixignore
AGENTS.md
README.md
repomix-output.xml
````

# Files

## File: .repomixignore
````
# Archivos de metadatos de Unity (vitales para Git, inútiles para la IA)
*.meta

# Archivos YAML gigantes generados por Unity
*.unity
*.prefab
*.asset
*.mat
*.anim
*.controller
*.overrideController
*.physicMaterial
*.physicsMaterial2D
*.guiskin
*.flare
*.renderTexture
*.mixer

# Paquetes de Unity y configuraciones (opcional, pero recomendado)
Packages/
ProjectSettings/
````

## File: repomix-output.xml
````xml
This file is a merged representation of the entire codebase, combined into a single document by Repomix.

<file_summary>
This section contains a summary of this file.

<purpose>
This file contains a packed representation of the entire repository's contents.
It is designed to be easily consumable by AI systems for analysis, code review,
or other automated processes.
</purpose>

<file_format>
The content is organized as follows:
1. This summary section
2. Repository information
3. Directory structure
4. Repository files (if enabled)
5. Multiple file entries, each consisting of:
  - File path as an attribute
  - Full contents of the file
</file_format>

<usage_guidelines>
- This file should be treated as read-only. Any changes should be made to the
  original repository files, not this packed version.
- When processing this file, use the file path to distinguish
  between different files in the repository.
- Be aware that this file may contain sensitive information. Handle it with
  the same level of security as you would the original repository.
</usage_guidelines>

<notes>
- Some files may have been excluded based on .gitignore rules and Repomix's configuration
- Binary files are not included in this packed representation. Please refer to the Repository Structure section for a complete list of file paths, including binary files
- Files matching patterns in .gitignore are excluded
- Files matching default ignore patterns are excluded
- Files are sorted by Git change count (files with more changes are at the bottom)
</notes>

</file_summary>

<directory_structure>
.agents/
  agents/
    Arquitecto_Modulo4_MVP.md
  skills/
    funplay-unity-mcp-workflow/
      SKILL.md
    funplay-unity-ui-composition/
      SKILL.md
  mcp_config.json
.funplay/
  skills/
    manifest.json
Assets/
  Art/
    Fonts/
      RobotoSlab-VariableFont_wght.ttf
  Scripts/
    DashboardUI.cs
    GeneradorTablaManifiesto.cs
    GestorJSON.cs
    RobotAMR.cs
    RobotManagerEVE.cs
  StreamingAssets/
    manifiesto.json
UserSettings/
  Layouts/
    CurrentMaximizeLayout.dwlt
    default-2022.dwlt
  FunplayMcpSettings.json
  Search.settings
.gitignore
.repomixignore
AGENTS.md
README.md
</directory_structure>

<files>
This section contains the contents of the repository's files.

<file path=".repomixignore">
# Archivos de metadatos de Unity (vitales para Git, inútiles para la IA)
*.meta

# Archivos YAML gigantes generados por Unity
*.unity
*.prefab
*.asset
*.mat
*.anim
*.controller
*.overrideController
*.physicMaterial
*.physicsMaterial2D
*.guiskin
*.flare
*.renderTexture
*.mixer

# Paquetes de Unity y configuraciones (opcional, pero recomendado)
Packages/
ProjectSettings/
</file>

<file path=".agents/agents/Arquitecto_Modulo4_MVP.md">
---
name: Arquitecto_Modulo4_MVP
description: Tech Lead y Arquitecto de Software para el Módulo 4 (Bodega Inteligente - MVP 20%). Especialista en FSM, NavMesh, y seguridad MCP.
mainAgent: true
subagent: true
permissionMode: acceptEdits
commandExecutionPolicy: auto
tools:
  - execute_code
  - request_recompile
  - wait_for_compilation
  - get_compilation_errors
  - get_scene_info
  - get_hierarchy
  - get_console_logs
  - read_file
  - write_file
  - patch_script
  - find_assets
skills:
  - unity-mcp-workflow
  - unity-ui-composition
---

# Rol y Propósito Estratégico
Eres el Senior Unity Tech Lead del proyecto "Entorno Interactivo de Automatización" (Unity 2022.3 LTS). Tu jurisdicción es el Módulo 4 (Bodegaje Inteligente) y su integración con el Módulo 5 (Despacho Táctico). Tu objetivo es gobernar el 20% funcional del MVP, garantizando una arquitectura industrial altamente optimizada, modular y resistente a fallos.

# Pilar 1: Guardián del Git Flow
Eres el supervisor del control de versiones. Antes de inyectar código masivo o alterar la arquitectura:
- Debes auditar verbalmente que el usuario está trabajando en una rama de desarrollo segura (`feat/` o `fix/`).
- Si el usuario te pide programar en `main`, debes detenerte, advertirle del riesgo y ordenarle crear una rama antes de proceder.

# Pilar 2: Arquitectura Modular (Los 3 Actores)
El sistema opera bajo un ecosistema de roles estrictos. Prohibido mezclar responsabilidades:
1. **Robot Manager (EVE):** Es el orquestador absoluto (WMS). Se mueve de forma rígida mediante un arreglo matemático de `Waypoints` aéreos (usando `Vector3.MoveTowards`). NUNCA calculará colisiones ni usará NavMesh. Lee la base de datos y despacha misiones secuencialmente.
2. **Robot Transportador (AMR):** Es el músculo terrestre. Opera bajo una Máquina de Estados Finitos (FSM) estricta: `Inactivo`, `En_Transito`, `Extrayendo`, `Transportando`, `Interrumpido`, `Entregando`. 
3. **Robot Cargador:** Elemento de "Lore". Representa la matriz LIFO (Last In, First Out) en el camión.

# Pilar 3: Movimiento Rígido (Cero Libre Albedrío)
La eficiencia algorítmica es tu prioridad. 
- **NavMesh Obligatorio:** El AMR terrestre utiliza exclusivamente `NavMeshAgent` para navegar por la "carretera de luz". 
- **Navegación Dinámica sin Triggers:** Prohibido sugerir colisionadores invisibles (`OnTriggerEnter`) para que el AMR detecte su llegada. El AMR debe calcular su proximidad usando `agente.remainingDistance <= 1.0f`.
- **Zonas de Exclusión:** No uses IA de evasión ni físicas de empuje (`Rigidbody` forces).

# Pilar 4: Desacoplamiento de Datos (Data-Driven)
Toda misión táctica proviene del exterior. 
- Prohibido "quemar" (hardcodear) identificadores, coordenadas (ej. "P2-C4-F1") o pesos de pallets dentro de los scripts de C#. 
- Toda lectura debe hacerse consumiendo `manifiesto.json` en la ruta protegida `Application.streamingAssetsPath` mediante `JsonUtility`.

# Pilar 5: El Contrato al 110% (Incertidumbre y UI)
El sistema debe predecir la falla logística. 
- Si EVE asigna un pallet superior a 900 kg (anomalía de batería), el AMR debe cambiar su estado a `Interrumpido` y frenar el NavMeshAgent.
- Debes asistir en la conexión de esta interrupción con el `DashboardUI` (Singleton), mostrando un panel de advertencia crítica y habilitando un "Override" manual para que el usuario fuerce la batería al 0% y salve el contrato.

# Protocolo de Inyección Segura (MCP)
- Utiliza **siempre** la herramienta `execute_code` implementando la plantilla `IFunplayCommand`.
- Toda creación, modificación o destrucción de GameObjects debe registrarse con `ctx.RegisterObjectCreation`, `ctx.RegisterObjectModification` y `ctx.DestroyObject` para mantener intacto el sistema de Undo (Ctrl+Z) del usuario.
- Tras alterar scripts, es OBLIGATORIO ejecutar `request_recompile` seguido de `wait_for_compilation`. Verifica los resultados con `get_compilation_errors` antes de dar una tarea por terminada.
</file>

<file path="Assets/Scripts/GeneradorTablaManifiesto.cs">
using UnityEngine;
using TMPro; // Necesario para TextMeshPro
using System.Collections.Generic;

public class GeneradorTablaManifiesto : MonoBehaviour
{
    [Header("Configuración de UI")]
    [SerializeField] private Transform contenedorFilas; // Aquí arrastrarás tu panel 'Manifiesto'
    [SerializeField] private GameObject filaPrefab;     // Aquí arrastrarás tu Prefab azul

    // Función pública que llamaremos cuando el JSON termine de leerse
    public void PoblarTabla(List<Pedido> pedidosJSON)
    {
        // 1. Limpieza de seguridad: Borrar filas viejas si el JSON se recarga
        // (Empezamos desde el índice 1 para no borrar la Fila_Titulos que está en el índice 0)
        for (int i = 1; i < contenedorFilas.childCount; i++)
        {
            Destroy(contenedorFilas.GetChild(i).gameObject);
        }

        // 2. Crear una fila nueva por cada pedido en el JSON
        foreach (Pedido pedido in pedidosJSON)
        {
            // Clonamos el prefab dentro del contenedor
            GameObject nuevaFila = Instantiate(filaPrefab, contenedorFilas);

            // Obtenemos los 5 textos de esa nueva fila
            TextMeshProUGUI[] columnas = nuevaFila.GetComponentsInChildren<TextMeshProUGUI>();

            // Asignamos la información del JSON a cada columna respectiva
            columnas[0].text = pedido.id_pallet;
            columnas[1].text = pedido.nombre_producto; // Asegúrate de que esta variable exista en tu clase Pedido
            columnas[2].text = pedido.codigo_sku;
            columnas[3].text = pedido.peso_kg.ToString() + " kg";
            
            // Lógica visual para el estado
            columnas[4].text = "PENDIENTE";
            columnas[4].color = Color.red; // Resalta en rojo para indicar que el robot aún no lo mueve
        }
    }
}
</file>

<file path="UserSettings/Search.settings">
{}
</file>

<file path=".gitignore">
# This .gitignore file should be placed at the root of your Unity project directory
#
# Get latest from https://github.com/github/gitignore/blob/master/Unity.gitignore
#
/[Ll]ibrary/
/[Tt]emp/
/[Oo]bj/
/[Bb]uild/
/[Bb]uilds/
/[Ll]ogs/
/[Mm]emoryCaptures/

# Never ignore Asset meta data
!/[Aa]ssets/**/*.meta

# Uncomment this line if you wish to ignore the asset store tools plugin
# /[Aa]ssets/AssetStoreTools*

# TextMesh Pro files
[Aa]ssets/TextMesh*Pro/

# Autogenerated Jetbrains Rider plugin
[Aa]ssets/Plugins/Editor/JetBrains*

# Visual Studio cache directory
.vs/

# Gradle cache directory
.gradle/

# Autogenerated VS/MD/Consulo solution and project files
ExportedObj/
.consulo/
*.csproj
*.unityproj
*.sln
*.suo
*.tmp
*.user
*.userprefs
*.pidb
*.booproj
*.svd
*.pdb
*.mdb
*.opendb
*.VC.db

# Unity3D generated meta files
*.pidb.meta
*.pdb.meta
*.mdb.meta

# Unity3D generated file on crash reports
sysinfo.txt

# Builds
*.apk
*.unitypackage

# Crashlytics generated file
crashlytics-build.properties
</file>

<file path="README.md">
# SistemaLogistico_AMR
</file>

<file path=".agents/skills/funplay-unity-mcp-workflow/SKILL.md">
---
name: funplay-unity-mcp-workflow
description: Efficient workflow for using Unity MCP to edit, import, compile, inspect, and test Unity projects, including screenshot and Game View recording verification.
---
<!-- Funplay Unity MCP managed project skills -->
<!-- Funplay Unity MCP skill version: unity-mcp-workflow@1.0.5 -->

# Unity MCP Workflow

Use this skill when Codex or another AI agent is working in a Unity project and needs to verify code, prefabs, UI, Play Mode behavior, screenshots, Game View recordings, scene hierarchy, console logs, domain reloads, or MCP connection issues.

## MCP-First Unity UI Operations

- Do not use computer use (desktop mouse/keyboard automation) to operate Unity unless necessary. When assembling, modifying, inspecting or validating UI, prefer Unity MCP whenever it can complete the step, including hierarchy/component/prefab reads and edits, compilation/Play state, clicks/scrolling, screenshots and recordings.
- Check the connected project's tools/list and, when available, `get_tool_capabilities`. A tool missing from exposure, compilation/domain reload or a temporary disconnection is not evidence of a missing capability: check exposure/readiness and recover status first. Respect custom allowlists; do not widen exposure or use another interaction method to bypass restrictions.
- Prefer specialized MCP tools; for project-specific gaps they do not cover, use a permitted, guarded `execute_code` call through Unity Editor APIs when it can perform the step reliably. Computer use is a fallback only for a confirmed MCP capability gap, or an explicit user request: explain the uncovered step before using it, limit it to that step, and return to MCP readback/validation when available. If recovery fails, report the connection blocker rather than silently switching methods or repeating uncertain mutations.
- This routing applies to operating Unity, not ordinary source-file editing or viewing supplied design references and already-captured images/videos with appropriate file or media tools.


## Operating Loop

1. Establish context.
   - Confirm the Unity project root and active scene.
   - Check that Unity MCP is reachable before assuming Editor state.
   - Inspect hierarchy, prefab paths, selected objects, and relevant component references through MCP.
   - If the user names an object, treat the name as a hint and verify the real Unity object path before editing.
2. Choose the edit surface.
   - Edit source files with normal repo tools, then trigger Unity recompilation.
   - Edit scene objects through Unity APIs, mark the scene dirty, and save the scene.
   - Edit prefab fields with `set_prefab_property(ies)` when available. Use `PrefabUtility.LoadPrefabContents`, `SaveAsPrefabAsset`, and `UnloadPrefabContents` for structural changes.
   - Unless the user explicitly requests a full rebuild, preserve the existing hierarchy when editing UI or GameObject prefabs and modify only the required objects, components, and serialized fields; do not recreate the entire prefab.
   - Edit ScriptableObject assets through `SerializedObject`, `EditorUtility.SetDirty`, and `AssetDatabase.SaveAssetIfDirty` / `SaveAssets`.
   - Never patch `.unity`, `.prefab`, or `.asset` YAML with shell text tools.
   - If the user is looking at an open scene instance, update the visible scene instance as well as the prefab asset when appropriate.
3. Execute changes.
   - Prefer structured query/edit/audit tools for supported work. Use a guarded `execute_code` batch only for project-specific gaps, after resolving full type names and assemblies when needed.
   - Use null guards for every object, component, asset, and path lookup.
   - Return explicit missing-path/object/component messages that include the expected path and the scene or prefab searched.
   - Return concise before/after values from snippets.
   - Save only the assets or scenes intentionally modified.
   - Do not run self-healing fallback loops; if a reference, path, package, or tool is missing, report it once and stop or skip that item.
4. Validate.
   - Read back the changed objects through MCP.
   - For code or resource edits, use `prepare_editor` and poll its durable operation ID to verified readiness in the intended mode, then inspect compilation errors and console errors.
   - For runtime behavior, enter Play Mode or inspect live objects when needed.
   - If MCP is unreachable, do not claim scene, prefab, asset, or runtime verification.
   - Report exactly what was verified and what still requires device, store, network, or manual validation.

## Unity Serialized Asset Safety

- Do not use shell text tools, scripts, or patches to modify `.unity`, `.prefab`, or `.asset` files. These are Unity-owned serialized assets; changing them outside Unity can corrupt file IDs, prefab overrides, references, import state, or scene dirtiness.
- Shell tools may inspect or locate serialized Unity assets, but scene, prefab, and ScriptableObject modifications must go through Unity MCP tools or Editor APIs.
- For scenes, modify live objects through Unity APIs, mark only the touched scene dirty, and save that scene.
- For prefabs, use Prefab Mode tools or `PrefabUtility.LoadPrefabContents` / `SaveAsPrefabAsset` / `UnloadPrefabContents`.
- For ScriptableObjects or other `.asset` files, load the asset with `AssetDatabase`, modify serialized properties through `SerializedObject` when possible, mark that asset dirty, and save only that asset.
- If Unity readback and raw file text disagree, trust Unity readback and investigate the asset path instead of hand-editing YAML.

## Tool Exposure

- With default `core` exposure, prefer structured inspection/editing, audits, durable preparation, unified task reads, project-aware UI creation and visual evidence. `execute_code` remains a fallback for project-specific gaps.
- `full` retains legacy status and compile/Play tools, history, project-default configuration, preview management, explicit recording markers and specialized diagnostics. Check exposure before choosing those workflows; do not silently widen a custom list.
- Use `get_tool_capabilities` to distinguish implemented/enabled/exposed tools. Respect customized allowlists; report missing exposure rather than claiming an implementation does not exist.

## MCP Call Pattern

If native MCP tools are not directly available, probe the local HTTP endpoint. The port is
per project, so read it from the Funplay MCP Server window (it is also the port in the
configured client entry) instead of assuming a fixed one:

```bash
PORT=24312 # replace with the port shown in the Funplay MCP Server window
curl -sS -m 1 -X POST http://127.0.0.1:$PORT/mcp \
  -H 'Content-Type: application/json' \
  -d '{"jsonrpc":"2.0","id":1,"method":"tools/list"}'
```

For multi-line `execute_code` calls over curl, generate JSON with a real encoder instead of hand-escaping C#:

```bash
node - <<'NODE'
const code = String.raw`
using UnityEngine;

public class InspectSomething
{
    public static string Run()
    {
        var obj = GameObject.Find("PracticeInGameUiRoot");
        return obj != null ? obj.name : "not found";
    }
}
`;
const payload = {
  jsonrpc: "2.0",
  id: 1,
  method: "tools/call",
  params: { name: "execute_code", arguments: { code } }
};
process.stdout.write(JSON.stringify(payload));
NODE
```

## Recommended `execute_code` Template

For non-trivial snippets, prefer `IFunplayCommand` over the legacy `public static string Run()` template. `execute_code` auto-adds `using Funplay.Editor.Tools.Scripting;` when `IFunplayCommand` is used, but include it explicitly in generated snippets for readability:

```csharp
using Funplay.Editor.Tools.Scripting;
using UnityEngine;

public class CommandScript : IFunplayCommand
{
    public void Execute(ExecutionContext ctx)
    {
        var root = GameObject.Find("PracticeInGameUiRoot");
        if (root == null)
        {
            ctx.LogWarning("PracticeInGameUiRoot not found");
            ctx.ReturnValue = "missing root";
            return;
        }

        ctx.RegisterObjectModification(root);
        ctx.Log("Found {0}, active={1}", root.name, root.activeInHierarchy);
        ctx.ReturnValue = new
        {
            name = root.name,
            active = root.activeInHierarchy
        };
    }
}
```

Use `ctx.RegisterObjectCreation(obj)`, `ctx.RegisterObjectModification(obj)`, and `ctx.DestroyObject(obj)` instead of direct Undo calls when possible. Use `ctx.Log`, `ctx.LogWarning`, and `ctx.LogError` for output returned in the MCP response without polluting the Unity Console.

## Unity C# Patterns

Add explicit `using` directives or use fully qualified types for project code. `execute_code` does not auto-inject project namespaces by default:

```csharp
var root = UnityEngine.GameObject.Find("PracticeInGameUiRoot");
var rect = root.GetComponent<UnityEngine.RectTransform>();
```

Use Unity null semantics for `UnityEngine.Object` references:

```csharp
if (image == null)
{
    return "Image missing";
}
```

Do not use `??=` to lazily resolve or rebind `UnityEngine.Object` references. Unity's destroyed or unbound serialized references can be fake-null: `field == null` returns true through Unity's overloaded operator, while C# `??=` can still treat the managed wrapper as non-null and skip the fallback assignment. Use an explicit Unity-null check instead:

```csharp
if (_hud == null)
{
    _hud = GetComponentInChildren<MyHud>(true);
}
```

For prefab edits:

```csharp
var path = "Assets/MyGame/UI/Prefabs/PF_PracticeInGameUiRoot.prefab";
var prefab = UnityEditor.PrefabUtility.LoadPrefabContents(path);
try
{
    var target = prefab.transform.Find("SafeArea/SwingCancelZone");
    if (target == null)
    {
        return "SwingCancelZone not found in prefab";
    }

    var rect = target.GetComponent<UnityEngine.RectTransform>();
    var before = rect.anchoredPosition;
    rect.anchoredPosition = new UnityEngine.Vector2(-76f, 448f);

    UnityEditor.EditorUtility.SetDirty(rect);
    UnityEditor.PrefabUtility.SaveAsPrefabAsset(prefab, path);
    UnityEditor.AssetDatabase.SaveAssets();
    return "Prefab saved: pos " + before + " -> " + rect.anchoredPosition;
}
finally
{
    UnityEditor.PrefabUtility.UnloadPrefabContents(prefab);
}
```

For scene edits:

```csharp
var obj = UnityEngine.GameObject.Find("PracticeInGameUiRoot/SafeArea/SwingCancelZone");
if (obj == null)
{
    return "Scene object not found";
}

var rect = obj.GetComponent<UnityEngine.RectTransform>();
var before = rect.sizeDelta;
UnityEditor.Undo.RecordObject(rect, "Update cancel zone");
rect.sizeDelta = new UnityEngine.Vector2(220f, 116f);
UnityEditor.EditorUtility.SetDirty(rect);
UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(obj.scene);
UnityEditor.SceneManagement.EditorSceneManager.SaveScene(obj.scene);
return "Scene saved: size " + before + " -> " + rect.sizeDelta;
```

## Recompile And Reload

After external C# or asset file edits:

1. Call `prepare_editor` with target=edit or play, refresh_assets=true and a unique request_key.
2. Retain data.task.task_id and use `get_task` with bounded waiting and the last revision. If HTTP drops during reload, reconnect and repeat the status read with that handle or kind=editor and the original request_key.
3. Continue only when operation.status=ready, current_editor.ready=true and current_editor.is_playing matches the intended mode.
4. Stop on failed/cancelled/interrupted status; inspect compiler errors, phase history, deadline and current state before deciding the next action.
5. Check console/runtime initialization separately. Preparation does not prove business logic, visual fidelity or event routing.

Do not replay an interrupted arbitrary mutation. Its outcome is unknown until exact object/asset readback establishes what executed.

If durable preparation is unavailable in an older/customized configuration, use the legacy exit/request_recompile/wait/error-check/enter sequence and explicitly read back state after reconnection. `get_reload_recovery_status` is historical information, not a readiness flag.

## Verification Checklist

Use structured component/property readback that returns exact values and persistence state. Resolve all candidates with `find_game_objects`, choose the verified ID, and query `get_component_properties`; never select the first duplicate name. Only use a guarded readback snippet when the structured API cannot express the project-specific check.

For UI work, verify prefab or scene hierarchy, sprite references, anchors, sorting order, active state, text fit, and button listeners. A populated `Content` hierarchy does not prove the user can see the UI.

For gameplay or network work, verify object identity, ownership, live instance existence, transform values, animation state, visibility, and whether client-side filters are discarding valid data.

## Failure Handling

- If MCP is unreachable, say so and fall back only to safe filesystem inspection or code edits. Do not claim scene, prefab, or runtime verification without Unity readback.
- If an object lookup fails, inspect hierarchy and prefab contents instead of inventing a path.
- If multiple matching objects exist, print their paths and choose the one matching the user-visible UI or current scene.
- If a reference, package, tool, or path is missing, return one clear error and stop or skip that item. Do not loop through guessed fallback paths, create replacement objects silently, or report success after a best-effort fallback.
- If compile errors appear after a change, fix them before Play Mode validation.
- When Unity and text files disagree for serialized scene or prefab state, trust Unity readback and inspect the asset path.

## Structured UI Workflows

- Short MCP tasks briefly wait for completion (wait_seconds defaults to 2; zero returns immediately). For longer tasks use `get_task(data.task.task_id, wait_seconds=20, after_revision=<last revision>)`; it waits for completion or a meaningful state change. Honor poll_after_ms on unchanged responses instead of making the model poll every second. wait_complete is not proof of success: inspect native status, errors and ready/complete/restoration fields. A read_timeout carries only the snapshot_at observation. Cancellation of the HTTP wait does not cancel the task. On lost preparation/preview responses recover through kind + the original request_key; do not replay mutations. Recording and Test Runner starts return immediately, as do preparation/preview starts without a recovery key.
- Inspect before modifying: use `find_game_objects` with component/property filters and projections, `inspect_ui_sprites` for Image/effective Sprite/importer/border/local-ID associations, and `find_project_types` for exact type and assembly names. Check ambiguity, partial errors, scan completeness and pagination; an incomplete scan is not proof of absence. Component setters distinguish live in-memory readback from saved/reimported prefab values.
- Run `audit_ui` on relevant live roots or saved prefabs/scenes; small scans can finish in one call, otherwise read status and finding pages through `get_task`. It checks missing borders, missing/required references, transparent raycast blockers, text/clipping and layout conflicts without fixing or saving assets. Review measured evidence and contextual warnings; suppress intentional exceptions only with an explicit project reason. Do not invent border values or infer design fidelity from a clean audit.
- Before creating new UI, read `get_ui_defaults`. `create_project_ui` can reuse templates and retain their prefab connection, label bindings, font/material and authored geometry. Explicit overrides take precedence; existing template component types are not converted. `configure_ui_defaults` changes project-scoped authoring preferences, so use it only when that shared convention is intended. Tied/incomplete convention scans or missing TMP resources require a deliberate choice/action, never a silent legacy fallback. This is Edit Mode authoring: save the intended scene/prefab explicitly and preserve existing UI when revising it.
- When preview management is exposed (Full by default), use `start_ui_preview_session` with verified prefab_paths and/or a project scene_template, optionally enter_play_mode and target width/height. It needs saved clean original scenes and no open Prefab Stage; do not save/discard unrelated user work merely to satisfy this precondition. Retain session_id and data.task.task_id; use `get_task` until ready. Business data and initialization remain project-specific; entering the scene may run lifecycle code.
- End the matching session with `end_ui_preview_session`, then inspect scenes_restored, view_restored, selection_restored, assets_cleaned and warnings. Do not claim full restoration from a success envelope. Changed scene setup, dirty preview or modified temporary scene requires inspection; discard_preview_changes applies only to the owned preview scene and must reflect an intended discard. Network/save-game effects and source asset edits are not rolled back. Preserve user-created files and changed window choices; report recovery still needed.
- Use screenshot `geometry`, not an unrelated `Screen` size: render size and returned image size can differ. Pass coordinate_space=image_pixels, origin=top_left and a fresh capture_id to click/drag/scroll or `raycast_at_point` when measuring a screenshot. `get_object_screen_bounds` and `get_visual_coordinates` share the mapping. Expired IDs or changed mode/view/scene/camera viewport/render dimensions require a fresh capture, not clamping or guessing. Geometry validity does not prove animated content stayed unchanged.


## Game View Recording

Use `capture_game_view` for static layout or a single visual state. Use `record_game_view` when the task needs evidence over time, such as animation, transitions, or a reproducible interaction sequence; do not record every routine UI edit.

1. Prepare. Use `prepare_editor` targeting play (or a ready preview session) and verify current readiness. Recording requires a graphics-enabled macOS or Windows Unity Editor with a visible, rendering Game tab. Keep that tab visible and its resolution unchanged throughout capture; hiding it or resizing the source can fail the recording. The MP4 includes overlay UI but no audio.
2. Start a short, bounded clip before performing the relevant actions. For example, call `record_game_view` with:

   ```json
   {"action":"start","duration_seconds":10,"fps":15,"max_dimension":1280}
   ```

   Save `data.recording_id` from the response, then perform the interaction. Start returns immediately; recording stops automatically at the duration limit. These are the default settings; accepted ranges are 1-120 seconds, 1-60 fps, and a 128-1920 pixel maximum edge. Aspect ratio is preserved without upscaling. Prefer a shorter clip or lower sampling rate/resolution if capture overhead is disruptive.
3. After performing the interactions, use `get_task` with the returned data.task.task_id for bounded status waits. To finish early, use `record_game_view` with `{"action":"stop","recording_id":"<returned id>"}`, then query the matching task if finalization is still pending. Legacy action=status remains compatible. Always pass the saved ID so a stale request cannot inspect or stop a newer recording. If another recording is already active, report it rather than stopping someone else's capture.
4. Check the receipt, not just `success`. While `data.status` is `recording` or `stopping`, the file is not ready. Read the MP4 only when `data.ready=true`; a `success=true` status query can still describe a failed recording. Stop polling on terminal `completed`, `interrupted`, or `failed` status and inspect `error`, `stop_reason`, and the actual captured extent (`frame_count`, `elapsed_seconds`, `last_frame_seconds`). Leaving Play Mode or reloading scripts finalizes early; recover the receipt after reload and treat any usable partial clip as partial evidence, not a complete test.
5. Review the actual file at `data.path`, under `<UnityProject>/Library/FunplayMcp/Recordings/`. MCP returns a local-file receipt, not video bytes or base64; the client must have access to that filesystem and a video viewer. A remote MCP connection alone does not provide file access. If video viewing is unavailable, inspect extracted frames when supported and state their limits, or report that the clip was saved but not reviewed. Do not claim to have watched an inaccessible clip or upload project footage without authorization.

- Report reproduction steps, clip path, observed result, and interruptions or unverified portions. Combine visual evidence with Unity state readback and console checks.
- Use `mark_recording` when exposed (Full by default) for named before/after project actions. Click, drag and `simulate_ui_scroll` tools record automatic markers in Core; a marker identifies dispatch, not proof that the intended behavior succeeded.
- Once ready, use `extract_recording_frames` with recording_id and 1..16 timestamps; if still pending use `get_task`, then inspect images with `get_recording_frame`. Check requested_seconds, actual_seconds and delta_seconds: it selects the first decoded frame at or after the request, not an exact-time guarantee. Markers after last_frame_seconds have no captured frame. Historical frame geometry is not valid for live input.
- Frame extraction can be cancelled; cleanup deletes only that job's generated PNGs, never the video. Preserve needed evidence before cleanup. Native decoder support depends on Unity version; interrupted/failed extraction and partial clips are not complete verification. Sparse frames cannot establish motion or timing between samples.
- Capture is best-effort with real elapsed timestamps, not guaranteed target-fps sampling. Use it for visual behavior, not frame-accurate performance measurement; use Profiler and device tests for performance.
- If the tool, platform, or rendering prerequisites are unavailable, report the limitation and use screenshots or state checks only for what they can establish. Do not loop on terminal failures or install recording dependencies merely to bypass the limitation.


## Metadata

- Original skill id: `unity-mcp-workflow`
- Skill version: `1.0.5`
- Platform: `antigravity`
- Source repository: `https://github.com/FunplayAI/funplay-unity-mcp`
</file>

<file path=".agents/mcp_config.json">
{"mcpServers":{"funplay-sistemalogistico":{"serverUrl":"http://127.0.0.1:21442/"}}}
</file>

<file path="Assets/Scripts/DashboardUI.cs">
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class DashboardUI : MonoBehaviour
{
    /*Al usar static, le estamos diciendo a Unity: "Esta variable no le pertenece a un objeto individual,
    le pertenece a la clase globalmente"
    Por convención mundial en C#, cuando se hace un Singleton, a la variable que guarda la copia única se le llama Instance.*/
    public static DashboardUI Instance { get; private set; }

    [Header("UI Referencias")]
    [Tooltip("Asigna aquí el TextMeshProUGUI. Si usas Text heredado, asígnalo abajo.")]
    [SerializeField] private Component textoLogTMP;
    [Tooltip("Asigna aquí el Text heredado si no usas TMPro.")]
    [SerializeField] private Text textoLogLegacy;

    private Queue<string> logQueue = new Queue<string>();
    private const int MaxLineas = 5;

    void Awake()
    {
        /*Aquí le estamos diciendo: "Si ya existe una instancia de esta clase en la escena,
         y no soy yo mismo, destrúyeme". Esto evita que Unity cree copias o duplicados.*/
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void RegistrarLog(string mensaje)
    {
        string timestamp = $"[{System.DateTime.Now:HH:mm:ss}]";
        string logFinal = $"{timestamp} {mensaje}";

        logQueue.Enqueue(logFinal);

        if (logQueue.Count > MaxLineas)
        {
            logQueue.Dequeue();
        }

        ActualizarVisual();
    }

    private void ActualizarVisual()
    {
        string contenido = string.Join("\n", logQueue);
        
        if (textoLogTMP != null)
        {
            var prop = textoLogTMP.GetType().GetProperty("text");
            if (prop != null)
            {
                prop.SetValue(textoLogTMP, contenido);
            }
        }
        else if (textoLogLegacy != null)
        {
            textoLogLegacy.text = contenido;
        }
    }
}
</file>

<file path="UserSettings/FunplayMcpSettings.json">
{
    "settingsVersion": 1,
    "enabled": true,
    "port": 8765,
    "portConfigured": false,
    "toolExportProfile": "core",
    "coreToolsCustom": true,
    "coreTools": [
        "add_component",
        "add_component_to_many",
        "add_layer",
        "add_tag",
        "analyze_scene_complexity",
        "assign_animator",
        "assign_material",
        "bake_lightmaps",
        "cancel_test_run",
        "capture_editor_window",
        "capture_game_view",
        "capture_multiview",
        "capture_scene_view",
        "capture_simulator_view",
        "clear_execute_code_history",
        "close_prefab_stage",
        "copy_asset",
        "copy_component",
        "create_animation_clip",
        "create_animator_controller",
        "create_button",
        "create_canvas",
        "create_game_object",
        "create_image",
        "create_material",
        "create_new_scene",
        "create_prefab",
        "create_primitive",
        "create_script",
        "create_scriptable_object",
        "create_text",
        "delete_asset",
        "delete_game_object",
        "director_evaluate",
        "duplicate_game_object",
        "edit_script",
        "enter_play_mode",
        "execute_code",
        "execute_menu_item",
        "exists",
        "exit_play_mode",
        "find_assets",
        "find_broken_references",
        "find_game_objects",
        "find_references",
        "focus_on_object",
        "frame_debugger_disable",
        "frame_debugger_enable",
        "frame_debugger_get_events",
        "get_active_tool",
        "get_animator_state",
        "get_asset_import_settings",
        "get_build_settings",
        "get_camera_properties",
        "get_compilation_errors",
        "get_component_properties",
        "get_console_logs",
        "get_counters",
        "get_editor_state",
        "get_execute_code_history",
        "get_frame_timing",
        "get_game_object_info",
        "get_hierarchy",
        "get_layers",
        "get_lighting_settings",
        "get_material_properties",
        "get_mesh_info",
        "get_object_memory",
        "get_performance_snapshot",
        "get_prefab_stage",
        "get_project_settings",
        "get_reload_recovery_status",
        "get_scene_info",
        "get_scriptable_object",
        "get_selection",
        "get_tags",
        "get_test_job",
        "get_time_scale",
        "get_top_memory_objects",
        "get_undo_state",
        "get_windows",
        "install_package",
        "instantiate_prefab",
        "list_components",
        "list_directory",
        "list_dirty_scenes",
        "list_packages",
        "list_scenes",
        "load_scene_additive",
        "log_message",
        "memory_compare_snapshots",
        "memory_list_full_snapshots",
        "memory_list_snapshots",
        "memory_open_snapshot_in_profiler",
        "memory_query_references",
        "memory_query_top_objects",
        "memory_take_full_snapshot",
        "memory_take_snapshot",
        "open_prefab_stage",
        "open_scene",
        "particle_control",
        "paste_component_values",
        "patch_script",
        "physics2d_overlap_point",
        "physics_overlap",
        "physics_raycast",
        "ping_asset",
        "play_animator_state",
        "profiler_start",
        "profiler_status",
        "profiler_stop",
        "raycast_at_point",
        "read_file",
        "record_game_view",
        "redo",
        "remove_package",
        "remove_tag",
        "rename_asset",
        "rename_game_object",
        "replay_execute_code",
        "request_recompile",
        "run_tests",
        "save_all_scenes",
        "save_prefab_stage",
        "save_scene",
        "search_files",
        "select_object",
        "set_active",
        "set_active_tool",
        "set_animator_parameter",
        "set_asset_import_settings",
        "set_camera_culling_mask",
        "set_camera_projection",
        "set_camera_settings",
        "set_component_properties",
        "set_component_property",
        "set_lighting_settings",
        "set_material_property",
        "set_parent",
        "set_prefab_properties",
        "set_prefab_property",
        "set_scriptable_object_properties",
        "set_selection",
        "set_tag_and_layer",
        "set_time_scale",
        "set_transform",
        "show_dialog",
        "simulate_key_combo",
        "simulate_key_press",
        "simulate_mouse_click",
        "simulate_mouse_drag",
        "undo",
        "unload_scene",
        "unpack_prefab",
        "validate_menu_item",
        "wait_for_compilation",
        "write_file"
    ],
    "fullToolsCustom": false,
    "fullTools": [],
    "selectedConfigTarget": "Antigravity",
    "executeCodeSafetyChecksEnabled": true,
    "executeCodeSafetyChecksConfigured": true,
    "executeCodeStrictFilesystemSafetyEnabled": true,
    "executeCodeStrictFilesystemSafetyConfigured": true,
    "executeCodeProjectNamespaceInjectionEnabled": true,
    "executeCodeProjectNamespaceInjectionConfigured": true,
    "pluginDebugLoggingEnabled": false,
    "pluginDebugLoggingConfigured": true,
    "mcpRecentActivityExpandedByDefault": true,
    "mcpRecentActivityExpansionConfigured": true,
    "mcpBrokerModeEnabled": false,
    "mcpBrokerMonoPath": "",
    "mcpLastClientConfigKeys": [
        "Antigravity|funplay-sistemalogistico"
    ]
}
</file>

<file path=".agents/skills/funplay-unity-ui-composition/SKILL.md">
---
name: funplay-unity-ui-composition
description: Build and revise responsive Unity uGUI mobile interfaces, including portrait and landscape layouts, safe areas, prefabs, auto layout, scrolling, text, input, animation, and performance validation.
---
<!-- Funplay Unity MCP managed project skills -->
<!-- Funplay Unity MCP skill version: unity-ui-composition@1.0.6 -->

# Unity UI Composition

Use this built-in skill when creating, assembling, adapting, reviewing, or fixing Canvas-based Unity UI, especially mobile screen or popup prefabs that must work across aspect ratios, notches, tablets, localization, and runtime state changes.

## MCP-First Unity UI Operations

- Do not use computer use (desktop mouse/keyboard automation) to operate Unity unless necessary. When assembling, modifying, inspecting or validating UI, prefer Unity MCP whenever it can complete the step, including hierarchy/component/prefab reads and edits, compilation/Play state, clicks/scrolling, screenshots and recordings.
- Check the connected project's tools/list and, when available, `get_tool_capabilities`. A tool missing from exposure, compilation/domain reload or a temporary disconnection is not evidence of a missing capability: check exposure/readiness and recover status first. Respect custom allowlists; do not widen exposure or use another interaction method to bypass restrictions.
- Prefer specialized MCP tools; for project-specific gaps they do not cover, use a permitted, guarded `execute_code` call through Unity Editor APIs when it can perform the step reliably. Computer use is a fallback only for a confirmed MCP capability gap, or an explicit user request: explain the uncovered step before using it, limit it to that step, and return to MCP readback/validation when available. If recovery fails, report the connection blocker rather than silently switching methods or repeating uncertain mutations.
- This routing applies to operating Unity, not ordinary source-file editing or viewing supplied design references and already-captured images/videos with appropriate file or media tools.

## Operating Loop

1. Inspect before editing.
   - Confirm the active scene, Canvas render mode, Canvas Scaler settings, EventSystem and input module, target orientations, design resolution, safe-area policy, and relevant prefab asset paths.
   - Inspect the existing hierarchy, anchors, pivots, offsets, layout controllers, sibling order, Canvas sorting, serialized references, animation targets, and Prefab overrides.
   - Inspect representative screens and prefabs to determine whether `UnityEngine.UI.Text` or `TextMeshProUGUI` is the project's prevailing text component, and inspect existing visual-effect components and material presets before introducing a new UI effect.
   - Treat screenshots and design coordinates as visual intent, not as permission to replace a working hierarchy.
   - When design images are supplied, follow Design References And Clarification below to map screens and states, resolve material uncertainty, and validate the actual result against each requested reference.
2. Classify each region.
   - Mark art as full-bleed or safe-area content.
   - Mark placement as fixed to an edge or corner, stretched between regions, content-sized, repeated-layout content, scrollable content, modal, or world-space UI.
   - Decide which component owns each axis. One axis must not be driven concurrently by a Layout Group, ContentSizeFitter, AspectRatioFitter, animation, and manual code.
3. Make the smallest coherent change.
   - Preserve the prefab root, existing children, components, names, serialized references, animation bindings, and Prefab overrides unless a specific replacement is required.
   - Modify only the necessary RectTransforms, components, fields, and children. Do not recreate an entire UI or GameObject prefab unless the user explicitly requests a rebuild.
   - Author reusable user-facing screens, panels, and controls as prefabs with their hierarchy and component references wired in the Editor, then instantiate and bind data at runtime. Do not move a stable UI hierarchy into procedural runtime construction merely for implementation convenience.
   - Use Unity MCP or Unity Editor APIs for `.prefab`, `.unity`, and `.asset` changes; never patch Unity YAML as text.
4. Read back and validate.
   - Read exact hierarchy, anchors, offsets, sizes, sprites (including borders for Sliced Images), text settings, raycast state, sorting, and references back from Unity.
   - Test layout, input, safe area, localization, animation interruption, close and reopen state, and runtime data changes.
   - Capture screenshots at representative aspect ratios for static layout; use a short `record_game_view` clip when correctness depends on an animation or interaction sequence. Use a real device build for performance and platform behavior before claiming device validation.

## Design References And Clarification

- For one or more design images, map each reference to its intended screen, popup, state, or responsive variant before editing. Do not merge incompatible variants or assume that upload order establishes version priority. Inspect every requested reference at a readable scale and look for matching project sprites, fonts, materials, and existing controls before assuming a detail is unavailable.
- Keep a compact per-screen checklist of visible details that matter: composition, alignment, spacing, sizes, text and line breaks, typography and effects, colors, icons, borders and corners, layering, and shown control states. Distinguish what the image actually shows from inferred behavior. Do not omit decorative details, reuse an approximate asset, or simplify a control merely because the rough layout already looks similar.
- When revising existing UI, preserve prefab structure, serialized references, and working behavior, not known visual mismatches. A clearly requested design change calls for scoped visual edits; the old UI looking different is not itself a reason to ask. If matching the design would conflict with an explicit preservation requirement or require changing existing behavior, explain the conflict and ask before that affected change.
- If reference and asset inspection still leave a material ambiguity, proactively ask a focused question before committing to that interpretation: identify the image, screen and region, state what cannot be determined, and offer plausible alternatives and their impact when useful. Examples include unreadable copy, conflicting versions, unclear page or state mapping, or missing exact artwork; request a clearer crop or source asset when that would resolve it. Do not silently invent text, omit an element, or substitute an approximation. Continue independent, clearly specified work while awaiting an answer; use project conventions for low-risk reversible details and disclose assumptions that affect the visible result rather than asking about every pixel.
- Validate every requested screen and state with actual Unity captures at the reference aspect ratio and comparable content, then check responsive variants separately. Inspect the captures, compare each region against its reference (side-by-side or with aligned overlays when useful), correct the differences, and recapture. Compilation success, component readback, or one correct page does not establish visual fidelity for the other pages. Do not stretch or crop the comparison to hide a mismatch.
- Before reporting completion, distinguish verified matches, user-approved deviations, unresolved differences, and unverified screens or states. Include any missing assets, unanswered visual decisions, or capture limitations. Do not claim full fidelity while known unapproved mismatches remain; ask the user to resolve remaining design choices instead of declaring an approximation complete.

## Component Selection

| Component | Use it for | Configure deliberately | Avoid |
| --- | --- | --- | --- |
| `Canvas` | Root render and sorting space | Use Screen Space - Overlay for ordinary HUD and menus, Screen Space - Camera when camera composition or scene interleaving is required, and World Space only for UI that belongs in the 3D world | Adding independent canvases only to organize folders; leaving Event Camera unset in Camera or World Space modes |
| `CanvasScaler` | Converting a design resolution to screen-space scale | For mobile screen UI, normally use Scale With Screen Size and a documented portrait or landscape reference resolution; choose Match from actual width and height constraints | Assuming Match `0.5` solves every aspect ratio or relying on Constant Physical Size without validating device DPI |
| `RectTransform` | Parent-relative position and size | Set anchors first, then pivot and offsets; keep local scale at one; use stretch anchors for flexible regions | Using anchoredPosition from one screenshot as universal placement or using Transform scale as a layout tool |
| `HorizontalLayoutGroup` / `VerticalLayoutGroup` | Dynamic rows, columns, button rails, and variable-length lists | Set padding, spacing, child size control, expansion, and child `LayoutElement` intent | Applying a Layout Group to hand-composed full-screen art or manually positioning driven children |
| `GridLayoutGroup` | Uniform inventory, level, or card cells | Choose cell size, spacing, start axis, and a row or column constraint | Expecting child preferred sizes to change grid cells; GridLayoutGroup assigns fixed cells |
| `LayoutElement` | Declaring minimum, preferred, flexible, or ignored layout behavior | Use it to override an Image, text, or nested group's layout contribution and to make selected siblings flexible | Adding it without selecting the properties that should override layout input |
| `ContentSizeFitter` | Making the current RectTransform follow its content on one or two axes | Prefer a single required axis; set the pivot to control growth direction; allow deferred layout unless immediate measurement is truly required | Putting it on every child controlled by a parent Layout Group or writing the same driven size manually |
| `AspectRatioFitter` | Preserving aspect for an isolated preview, card art, or media surface | Use Fit In Parent for letterboxing or Envelope Parent for cover behavior | Treating it as general safe-area or screen-aspect adaptation, or combining it with another controller on the same axis |
| `Image` | Sprite UI, icons, frames, progress fills, and nine-sliced controls | Use Simple for fixed art, Sliced for resizable panels and buttons only after verifying suitable Sprite borders, Tiled for repeatable patterns, and Filled for progress or radial values | Sliced with all-zero borders, stretching bordered art as Simple, leaving decorative graphics as Raycast Target, or using a unique material without need |
| `RawImage` | Arbitrary Texture, RenderTexture, camera, video, downloaded, or generated texture content | Preserve the source aspect and manage texture lifetime explicitly | Using RawImage for ordinary Sprite UI that should atlas and batch with other Images |
| `UnityEngine.UI.Text` | Text in an established legacy uGUI project or screen family | Use it only after inspection shows it is the prevailing project convention; match the existing Font, material, alignment, line spacing, overflow, and localization behavior | Introducing it into a new project, mixing it casually into a TMP-based screen, or converting existing labels without checking layout and serialized references |
| `TextMeshProUGUI` | Text in an established TMP project and the default for a new project with no existing text convention | Match the project's font assets and material presets; set wrapping, alignment, overflow, fallback fonts, and localization limits; constrain Auto Size to a narrow range; when the design explicitly shows a text effect, use TMP's own component and shader-material controls | Replacing an established `Text` component merely to modernize, continuous Auto Size on rapidly changing text, or shipping without required CJK and symbol glyphs |
| TMP font material / material preset | An outline, underlay or shadow, glow, face dilation, softness, or other text treatment explicitly visible in the design for a `TextMeshProUGUI` project | For a simple outline set the TMP component's `outlineColor` and `outlineWidth`; for underlay, glow, or other shader effects reuse an approved project preset or create a dedicated preset or material instance and verify it with fallback fonts | Adding a uGUI `Outline` or `Shadow` to TMP text by habit, changing a shared `fontSharedMaterial` and unintentionally restyling other labels, or enabling effects absent from the design |
| `Outline` / `Shadow` / similar `BaseMeshEffect` | A specifically designed effect on legacy `UnityEngine.UI.Text` or another Graphic, when required by the design or established project style | Reuse the project's approved style and keep effect distance, color, alpha, and stacking minimal; verify legibility and cost on target hardware; prefer TMP-native effects when the text component is `TextMeshProUGUI` | Adding generic polish by default, stacking effects, applying them broadly, or using them to compensate for weak contrast or incorrect layout; these effects duplicate UI geometry and increase overdraw |
| `TMP_InputField` | Editable TMP text | Author and validate a prefab with `textComponent`, `textViewport`, `placeholder` when used, target Graphic, navigation, and input settings already serialized; instantiate the prefab and bind data or listeners at runtime | Rebuilding a stable input hierarchy in code; if dynamic construction is genuinely required, never add it to an active GameObject and bind `textComponent` afterward because affected TMP versions create the caret renderer in `OnEnable` only when that reference is already present |
| `ScrollRect` | Drag or wheel scrolling through content larger than a viewport | Use `ScrollRect -> Viewport + RectMask2D -> Content`, reference both Viewport and Content, enable only required axes, and choose Clamped or Elastic intentionally | Unrestricted movement without recovery, deeply nested competing scroll axes, or instantiating thousands of live rows without virtualization |
| `RectMask2D` | Rectangular clipping in 2D Canvas UI | Prefer it for scroll viewports and rectangular reveal areas | Using stencil `Mask` for a simple rectangle |
| `Mask` | Clipping to a non-rectangular Graphic shape | Use only when the shape matters and account for stencil and material cost | Deeply nested masks or using it where RectMask2D is sufficient |
| `CanvasGroup` | Fading and enabling or disabling a whole panel | Change alpha, interactable, and blocksRaycasts together according to visible state; decide whether parent groups apply | Setting alpha to zero while leaving an invisible panel interactive or raycast-blocking |
| `Button` and other `Selectable` controls | Click, toggle, slider, dropdown, and navigation behavior | Put the main Raycast Target on the interactive root, set Target Graphic and navigation, and add and remove runtime listeners symmetrically | Multiple child Raycast Targets for one control, duplicate listeners, or visual-only disabled states |
| `EventSystem` and `GraphicRaycaster` | Routing pointer, touch, submit, cancel, and navigation events | Keep one EventSystem and one active matching input module; use `InputSystemUIInputModule` with the Input System; enable raycast only where required | A second EventSystem in additive scenes or physics blocking checks when they are unnecessary |

## Sliced Images And Sprite Borders

- Before setting `Image.type = Image.Type.Sliced`, inspect the Sprite actually displayed, including `overrideSprite` when present, and read its `Sprite.border`. An all-zero border does not produce nine-slicing; selecting Sliced alone is insufficient. Do not leave `This Image doesn't have a border.` unresolved and claim that the frame is correctly sliced.
- Choose border insets from the source artwork in pixels, preserving its rounded corners, outlines, and other non-stretchable edge details while leaving a usable stretchable center. `Vector4` order is **left, bottom, right, top** (`x, y, z, w`). Do not invent arbitrary nonzero values just to suppress the warning. Zero on some sides can be intentional for one-axis stretching; not every design needs four positive insets.
- Configure Border in Sprite Editor and Apply, or use the matching Sprite import API: for Single mode, set `TextureImporter.spriteBorder` and call `SaveAndReimport`; for Multiple mode, edit only the intended sub-sprite's border metadata, preserving its rect, name, IDs, and other slices. For atlased sprites, edit the source Sprite metadata, not the packed atlas texture or atlas padding.
- Check other consumers before changing a shared Sprite's border. Reuse a suitable existing bordered Sprite when possible; if the art is unsuitable or cannot safely be changed, report the limitation and choose an appropriate Image type or asset within the task scope. Do not replace the whole prefab or silently alter unrelated UI to resolve a border warning.
- After Apply or reimport, reacquire the displayed Sprite and read back `Sprite.border` and `Image.type`; an importer assignment alone is not verification. Resize at the intended minimum and representative target sizes and aspect ratios, then inspect the result: corners retain their shape, edge thickness is consistent, and the center has no seams or clipping. `pixelsPerUnitMultiplier` changes border sizing, not the slicing insets, and cannot replace missing borders.

## Canvas And Layering

- Use a small number of semantic layers such as Background, Screen, HUD, Overlay, Modal, Loading, and Debug. Make each layer a full-stretch RectTransform and define sibling or sorting order once.
- Let opaque or decorative backgrounds bleed to the physical screen edges. Put critical labels and all interactive controls under a separate SafeAreaRoot.
- When Modal or Loading UI is visible, block gameplay input explicitly; a visible scrim alone does not prove input is blocked.
- Keep one stable scrim per popup layer when a popup stack owns it. Restore the previous popup and its input state when the top popup closes.
- Distinguish Hide from Close. Hide can retain a cached instance; Close must release instantiated assets, handles, listeners, and transient state.
- Split static and frequently changing UI only when profiling shows rebuild cost. Nested canvases isolate rebuilds but prevent batching across canvas boundaries, so do not create one Canvas per widget.

## Canvas Scaler And RectTransform Rules

- Treat the reference resolution as design coordinates, not a list of supported physical resolutions. A proven portrait baseline is `720 x 1559`; a proven landscape baseline is `1559 x 720`.
- Start with Scale With Screen Size and Match `0.5` when width and height are equally important, then verify. Move Match toward width when horizontal design width must remain stable, or toward height when vertical design height must remain stable.
- Use anchors to express attachment: top bars to top stretch, bottom actions to bottom or bottom stretch, edge buttons to their corner, and center gameplay viewports to stretch between reserved regions.
- Set anchors before recording offsets. With separated anchors, `sizeDelta` is the delta relative to the anchor rectangle, not the final absolute size.
- Resize UI through RectTransform width, height, anchors, and offsets; leave localScale at one. Animate a child named Visual or Container when the root is layout-driven.
- Respond to `OnRectTransformDimensionsChange` or an equivalent resolution and orientation signal when layout contains calculated page widths, aspect branches, or safe-area anchors. Do not poll and rewrite every RectTransform every frame.
- Prefer `LayoutRebuilder.MarkLayoutForRebuild` for deferred updates. Use `Canvas.ForceUpdateCanvases` or `LayoutRebuilder.ForceRebuildLayoutImmediate` only when code must measure the final layout in the same operation, never as a routine per-frame fix.

## Safe Area

- Read `Screen.safeArea` in screen pixels and convert both minimum and maximum corners to normalized anchors. Reapply when screen dimensions, orientation, or safe area changes; do not cache only a top inset.
- A minimal uGUI conversion is:

```csharp
Rect safe = Screen.safeArea;
safeAreaRoot.anchorMin = new Vector2(
    safe.xMin / Screen.width,
    safe.yMin / Screen.height);
safeAreaRoot.anchorMax = new Vector2(
    safe.xMax / Screen.width,
    safe.yMax / Screen.height);
safeAreaRoot.offsetMin = Vector2.zero;
safeAreaRoot.offsetMax = Vector2.zero;
```

- Guard zero screen dimensions and avoid duplicate application when nothing changed.
- Check `PlayerSettings.Android.renderOutsideSafeArea`. If rendering outside is disabled, the Player window can already be fitted to the safe area and `Screen.safeArea` can equal the full Player window; do not apply a second inset blindly.
- In portrait, verify top cutout and bottom home-indicator or navigation areas. In landscape, verify both left and right cutouts in Landscape Left and Landscape Right.
- For edge art assembled from nested images, particles, labels, or Spine content, wait until layout and final offsets are applied, then use `RectTransformUtility.CalculateRelativeRectTransformBounds(parent, visualRoot)` to clamp the complete visual bounds inside the allowed safe rectangle.

## Portrait Mobile Pattern

- Organize the screen as Top, Center, and Bottom regions. Anchor persistent status and currency UI to Top; navigation, primary actions, and skill buttons to Bottom; stretch the game or page viewport through Center.
- Reserve top and bottom space with offsets on the stretched center viewport instead of giving the viewport a fixed height.
- Let additional height on tall phones expand the center region. Do not multiply every vertical coordinate by the screen aspect ratio.
- For horizontally paged home screens, compute each page from the current viewport width and recompute content width and selected-page position when dimensions change.
- Keep centered popup content within a safe maximum height. Use scrolling for localized or data-driven content that can exceed that height.

## Landscape Mobile Pattern

- Organize the screen as Left, Center, Right, plus stable corners. Put high-frequency gameplay content in Center and distribute controls so neither side becomes a single crowded column.
- Use 16:9 as a common gameplay baseline, but branch deliberately for ultrawide, 16:10, and 4:3 tablet layouts. Anchors handle attachment; a small aspect-aware layout policy handles genuine composition changes.
- Keep camera framing separate from Canvas scaling. A narrow landscape viewport may require a larger orthographic size or alternate camera composition to preserve world-space gameplay even when the Canvas itself is correct.
- Use background cover scaling or phone and tablet background variants when one crop cannot preserve the art direction across 16:9 and 4:3.
- Reposition only the controls whose composition genuinely changes at tablet aspect ratios. Do not fork the entire screen prefab when a few region offsets or constraints are sufficient.

## Auto Layout And Dynamic Content

- Remember the allocation order: minimum size, then preferred size, then flexible size. Use `LayoutElement` to state which sibling can consume extra space.
- A Layout Group drives its children. Do not manually edit a driven child position or size and expect it to persist after the next rebuild.
- A ContentSizeFitter drives its own RectTransform and expands around its pivot. Use a top pivot for content that must grow downward and a left pivot for content that must grow rightward.
- Do not put ContentSizeFitter on children whose RectTransforms are already controlled by the parent Layout Group. Disable Child Force Expand and use child layout input instead.
- GridLayoutGroup ignores child minimum, preferred, and flexible dimensions and assigns the configured fixed cell size. Use a different layout or custom controller for variable-sized grid cells.
- Keep layout nesting shallow. Repeated layout invalidation walks the hierarchy; batch model changes, update content, then request one rebuild.

## Images, Text, Scrolling, And Input

- For scalable button and panel frames, follow Sliced Images And Sprite Borders above before using Image Type Sliced. Keep ornamental children non-raycastable.
- Use Sprite Atlas for compatible UI sprites, platform-specific texture overrides, sensible maximum sizes, and no mipmaps for ordinary screen-space UI unless a measured use case needs them.
- Treat large full-screen images separately from small control atlases. Verify memory, compression artifacts, overdraw, and crop behavior on target hardware.
- Before adding a label, inspect representative UI prefabs and scenes rather than inferring the text system from package availability. Preserve the component type on existing labels and use the text component that is most common in the relevant project or screen family. If the project is new and has no established convention, default to `TextMeshProUGUI`.
- Do not opportunistically migrate `UnityEngine.UI.Text` to `TextMeshProUGUI`, or the reverse, while composing unrelated UI. Such a migration can change preferred sizes, wrapping, materials, fallback behavior, animation bindings, and serialized component references and requires separate validation.
- In TMP projects, use font fallback chains for CJK, symbols, and localized glyphs. Keep common glyphs in the primary asset and verify fallback material appearance and draw-call impact.
- Prefer wrapping, truncation, or a known layout expansion policy over broad Auto Size ranges. TMP Auto Size performs repeated layout passes and is unsuitable for frequently changing counters or timers.
- Do not add `Outline`, `Shadow`, or another `BaseMeshEffect` merely because a control looks unfinished. Require an explicit design need or a verified existing project style, prefer the existing shared prefab or TMP material preset when applicable, avoid stacked effects, and verify the extra geometry and overdraw.
- When the reference image or design explicitly shows an outline or another font effect and the project uses `TextMeshProUGUI`, reproduce the visible treatment through the TMP component rather than omitting it or attaching a uGUI mesh effect by habit. Use `outlineColor` and `outlineWidth` for a simple outline; use an existing approved TMP material preset, or a dedicated preset or material instance, for underlay or shadow, glow, face dilation, softness, and other shader effects.
- Treat TMP material scope as part of the edit. Do not modify a shared `fontSharedMaterial` when the effect is local to one label or prefab because every user of that material may change. Reuse a matching project preset when one exists; otherwise create a deliberately scoped preset or instance, preserve the font atlas and fallback chain, and verify the result against the reference at target resolution.
- Prefer a prefab for `TMP_InputField` and other stable controls so hierarchy, references, navigation, styling, localization, and focus behavior are inspectable before Play Mode. Runtime code should instantiate the prefab and supply data and listeners, not recreate its child objects and component wiring.
- Only when procedural construction is explicitly required, treat the first enable of a runtime-created `TMP_InputField` as an initialization boundary. Create and wire its text hierarchy while the root is inactive (or the component is disabled), assign at least `textComponent` and `textViewport` plus `placeholder` when used, then enable it. In affected TMP versions, `OnEnable` creates the cached `Caret` renderer only when `textComponent` is already bound; assigning the property after that first enable does not retroactively create it.
- If a dynamically constructed `TMP_InputField` accepts text but shows no insertion caret, inspect whether a `Caret` / `TMP_SelectionCaret` object was created and whether `textComponent` was assigned before first enable. After wiring the missing references, disable and re-enable the field to run initialization again; then verify focus, blinking caret, selection highlight, placeholder state, and editing in Play Mode.
- For a vertical ScrollRect, top-anchor the Content and set its pivot to the top so growth is predictable. Preserve the normalized position intentionally when refreshing content.
- Use RectMask2D for rectangular viewports. Use Mask only when the clipping shape must follow a Graphic.
- Give touch controls a project-defined minimum hit area even when the visible art is smaller. Use one transparent or visible root Graphic or raycast padding rather than making every child Image a target.
- Add listeners once and remove them in the matching lifecycle. Disable interaction while entrance or exit animation makes a control visually unavailable.

## Animation And Prefab Safety

- Animate a popup Container or Visual child while leaving the full-screen scrim stable. This avoids scaling the raycast blocker and avoids fighting layout-driven roots.
- Before replaying an animation, kill or cancel the prior sequence and restore a deterministic base position, scale, alpha, interactable, and blocksRaycasts state.
- Use unscaled time for menu, pause, modal, and loading animations that must continue while gameplay time is zero.
- Preserve existing prefab objects by default. Replacing an asset at the same path can preserve the asset GUID while still changing child or component file IDs, breaking animation bindings, serialized references, Prefab Variants, and Scene overrides.
- Prefer serialized references or stable binding components. Use `Transform.Find` only for a verified stable hierarchy and fail clearly if it is missing; never silently create an alternate hierarchy.
- Use semantic names such as SafeAreaRoot, TopBar, ContentViewport, BottomActions, Visual, and Label. Replace ambiguous default names only when doing so will not break bindings, and update references atomically.

## Performance And Validation

- Profile before restructuring. Common uGUI bottlenecks are overdraw, Canvas batch rebuilds, repeated layout rebuilds, raycast candidates, text mesh generation, and excess materials or textures.
- Separate mostly static UI from high-frequency counters, timers, scrolling content, or animations when profiling justifies the extra Canvas. Co-locate elements that change together.
- Disable Raycast Target on decorative Images and TMP text. An active Graphic Raycaster tests eligible Graphics and raycast filters along their hierarchy.
- Avoid hiding large inactive screens only with alpha zero; they can still render or receive input depending on CanvasGroup state. Use the project's hide or pooling policy and measure reopen cost.
- Validate portrait at 16:9, 19.5:9 or 20:9, a cutout phone, and a portrait tablet. Validate landscape at 16:9, ultrawide, 16:10, 4:3, and both cutout sides.
- In every profile, verify full-bleed art, safe interactive content, text overflow and fallback glyphs, scroll bounds, modal input blocking, touch hit areas, selection navigation, animation interruption, and close and reopen state.
- Use `capture_game_view` for static composition, text fit, and before/after comparisons. Use a short `record_game_view` clip for behavior a still image cannot establish, such as popup transitions, scroll inertia, caret blinking, interrupted animations, or repeated close and reopen actions; record only the sequence relevant to the change.
- For a clip, finish compilation, enter Play Mode and wait for MCP recovery, keep the Game tab visible at a fixed resolution, then start recording before performing the interaction. Save `data.recording_id`, pass it to `action=status` or `action=stop`, and wait for `data.ready=true` before reviewing the local MP4. The Unity MCP Workflow skill describes supported Editors and failure handling. If recording or video viewing is unavailable, report that limitation; screenshots and hierarchy readback alone do not prove timing or transition correctness.
- Review intermediate frames as well as the final state: look for clipping or layout jumps, stuck raycast blocking, input leaking through a modal, and interruption or reopen state. Pair the clip with component-state readback and actual input checks; a visual result alone cannot prove event routing. Recording is silent and adds overhead, so it cannot validate audio or replace Profiler and real-device performance checks.
- Use Device Simulator for layout, safe-area, orientation, and basic single-touch checks. It does not simulate target CPU, GPU, memory, rendering backend, native plugins, or multitouch; use representative device builds for performance and final interaction validation.

## Official Unity References

- [Canvas render modes and nesting](https://docs.unity.cn/Packages/com.unity.ugui%402.0/manual/class-Canvas.html)
- [CanvasScaler API and Match behavior](https://docs.unity.cn/Packages/com.unity.ugui%402.0/api/UnityEngine.UI.CanvasScaler.html)
- [RectTransform](https://docs.unity.cn/Packages/com.unity.ugui%402.0/manual/class-RectTransform.html) and [multi-resolution UI](https://docs.unity.cn/Packages/com.unity.ugui%402.0/manual/HOWTO-UIMultiResolution.html)
- [Auto Layout](https://docs.unity.cn/Packages/com.unity.ugui%402.0/manual/UIAutoLayout.html), [LayoutElement](https://docs.unity.cn/Packages/com.unity.ugui%402.0/manual/script-LayoutElement.html), and [ContentSizeFitter](https://docs.unity.cn/Packages/com.unity.ugui%402.0/manual/script-ContentSizeFitter.html)
- [ScrollRect](https://docs.unity.cn/Packages/com.unity.ugui%402.0/manual/script-ScrollRect.html), [RectMask2D](https://docs.unity.cn/Packages/com.unity.ugui%402.0/manual/script-RectMask2D.html), and [Mask](https://docs.unity.cn/Packages/com.unity.ugui%402.0/manual/script-Mask.html)
- [Image](https://docs.unity.cn/Packages/com.unity.ugui%402.0/manual/script-Image.html), [CanvasGroup](https://docs.unity.cn/Packages/com.unity.ugui%402.0/manual/class-CanvasGroup.html), and [Selectable navigation](https://docs.unity.cn/Packages/com.unity.ugui%402.0/manual/script-SelectableNavigation.html)
- [Image.Type.Sliced and its border prerequisite](https://docs.unity3d.com/2018.4/Documentation/ScriptReference/UI.Image.Type.Sliced.html), [Sprite.border component order](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/Sprite-border.html), and [TextureImporter.spriteBorder](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/TextureImporter-spriteBorder.html)
- [TextMeshPro UI text and Auto Size](https://docs.unity.cn/Packages/com.unity.textmeshpro%403.2/manual/TMPObjectUIText.html) and [fallback fonts](https://docs.unity.cn/Packages/com.unity.textmeshpro%404.0/manual/FontAssetsFallback.html)
- [Screen.safeArea](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Screen-safeArea.html), [relative RectTransform bounds](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/RectTransformUtility.CalculateRelativeRectTransformBounds.html), and [Device Simulator](https://docs.unity3d.com/6000.0/Documentation/Manual/device-simulator-introduction.html)
- [Sprite Atlas](https://docs.unity3d.com/6000.0/Documentation/Manual/sprite/atlas/create-sprite-atlas.html), [platform texture overrides](https://docs.unity3d.com/6000.0/Documentation/Manual/class-TextureImporter-type-specific.html), and [official uGUI optimization guide](https://learn.unity.com/course/introduction-to-ui-in-unity/tutorial/optimizing-unity-ui)


## Structured UI Workflows

- Short MCP tasks briefly wait for completion (wait_seconds defaults to 2; zero returns immediately). For longer tasks use `get_task(data.task.task_id, wait_seconds=20, after_revision=<last revision>)`; it waits for completion or a meaningful state change. Honor poll_after_ms on unchanged responses instead of making the model poll every second. wait_complete is not proof of success: inspect native status, errors and ready/complete/restoration fields. A read_timeout carries only the snapshot_at observation. Cancellation of the HTTP wait does not cancel the task. On lost preparation/preview responses recover through kind + the original request_key; do not replay mutations. Recording and Test Runner starts return immediately, as do preparation/preview starts without a recovery key.
- Inspect before modifying: use `find_game_objects` with component/property filters and projections, `inspect_ui_sprites` for Image/effective Sprite/importer/border/local-ID associations, and `find_project_types` for exact type and assembly names. Check ambiguity, partial errors, scan completeness and pagination; an incomplete scan is not proof of absence. Component setters distinguish live in-memory readback from saved/reimported prefab values.
- Run `audit_ui` on relevant live roots or saved prefabs/scenes; small scans can finish in one call, otherwise read status and finding pages through `get_task`. It checks missing borders, missing/required references, transparent raycast blockers, text/clipping and layout conflicts without fixing or saving assets. Review measured evidence and contextual warnings; suppress intentional exceptions only with an explicit project reason. Do not invent border values or infer design fidelity from a clean audit.
- Before creating new UI, read `get_ui_defaults`. `create_project_ui` can reuse templates and retain their prefab connection, label bindings, font/material and authored geometry. Explicit overrides take precedence; existing template component types are not converted. `configure_ui_defaults` changes project-scoped authoring preferences, so use it only when that shared convention is intended. Tied/incomplete convention scans or missing TMP resources require a deliberate choice/action, never a silent legacy fallback. This is Edit Mode authoring: save the intended scene/prefab explicitly and preserve existing UI when revising it.
- When preview management is exposed (Full by default), use `start_ui_preview_session` with verified prefab_paths and/or a project scene_template, optionally enter_play_mode and target width/height. It needs saved clean original scenes and no open Prefab Stage; do not save/discard unrelated user work merely to satisfy this precondition. Retain session_id and data.task.task_id; use `get_task` until ready. Business data and initialization remain project-specific; entering the scene may run lifecycle code.
- End the matching session with `end_ui_preview_session`, then inspect scenes_restored, view_restored, selection_restored, assets_cleaned and warnings. Do not claim full restoration from a success envelope. Changed scene setup, dirty preview or modified temporary scene requires inspection; discard_preview_changes applies only to the owned preview scene and must reflect an intended discard. Network/save-game effects and source asset edits are not rolled back. Preserve user-created files and changed window choices; report recovery still needed.
- Use screenshot `geometry`, not an unrelated `Screen` size: render size and returned image size can differ. Pass coordinate_space=image_pixels, origin=top_left and a fresh capture_id to click/drag/scroll or `raycast_at_point` when measuring a screenshot. `get_object_screen_bounds` and `get_visual_coordinates` share the mapping. Expired IDs or changed mode/view/scene/camera viewport/render dimensions require a fresh capture, not clamping or guessing. Geometry validity does not prove animated content stayed unchanged.


## Metadata

- Original skill id: `unity-ui-composition`
- Skill version: `1.0.6`
- Platform: `antigravity`
- Source repository: `https://github.com/FunplayAI/funplay-unity-mcp`
</file>

<file path=".funplay/skills/manifest.json">
{
    "platforms": [
        "antigravity"
    ],
    "optionalSkills": [],
    "skillVersions": [
        {
            "id": "unity-mcp-workflow",
            "version": "1.0.5"
        },
        {
            "id": "unity-ui-composition",
            "version": "1.0.6"
        }
    ]
}
</file>

<file path="Assets/Scripts/RobotManagerEVE.cs">
using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class RobotManagerEVE : MonoBehaviour
{
    [Header("Waypoints de Vuelo")]
    public Transform[] waypoints;
    public float velocidadVuelo = 5f;
    private int indiceWaypointActual = 0;

    [Header("Base de Datos Local")]
    public Manifiesto manifiestoActual;
    public RobotAMR robotEjecutor;
    public int indicePedidoActual = 0;

    void Awake()
    {
        CargarManifiesto();
    }

    void Start()
    {
        if (manifiestoActual != null && manifiestoActual.pedidos.Count > 0 && robotEjecutor != null)
        {
            robotEjecutor.AsignarMision(manifiestoActual.pedidos[indicePedidoActual]);
        }
    }

    void Update()
    {
        MoverPorWaypoints();
    }

    void CargarManifiesto()
    {
        string ruta = Path.Combine(Application.streamingAssetsPath, "manifiesto.json");
        if (File.Exists(ruta))
        {
            string contenidoJson = File.ReadAllText(ruta);
            manifiestoActual = JsonUtility.FromJson<Manifiesto>(contenidoJson);
            
            Debug.Log("<color=green>EVE:</color> Manifiesto cargado. Total de pallets: " + manifiestoActual.pedidos.Count);
            DashboardUI.Instance?.RegistrarLog("EVE: Manifiesto cargado. Total de pallets: " + manifiestoActual.pedidos.Count);
            FindObjectOfType<GeneradorTablaManifiesto>()?.PoblarTabla(manifiestoActual.pedidos);
        }
        else
        {
            Debug.LogError("EVE Error crítico: No se encontró el Manifiesto de Carga en la ruta: " + ruta);
            DashboardUI.Instance?.RegistrarLog("EVE Error: No se encontró el Manifiesto de Carga.");
        }
    }

    public void ConfirmarEntregaExitosa()
    {
        indicePedidoActual++;
        if (indicePedidoActual < manifiestoActual.pedidos.Count)
        {
            robotEjecutor.AsignarMision(manifiestoActual.pedidos[indicePedidoActual]);
        }
        else
        {
            Debug.Log("<color=green>Operación Finalizada:</color> Manifiesto Nocturno completado. No hay más pedidos.");
            DashboardUI.Instance?.RegistrarLog("Operación Finalizada: Manifiesto completado.");
        }
    }

    void MoverPorWaypoints()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        Transform destino = waypoints[indiceWaypointActual];
        if (destino == null) return;
        /*En Unity, la palabra transform (con "t" minúscula) es una variable nativa e invisible
        que el motor le otorga automáticamente a todos los scripts que heredan de MonoBehaviour.
        No necesitas declararla en tu código porque ya existe en el núcleo del sistema.*/
        transform.position = Vector3.MoveTowards(transform.position, destino.position, velocidadVuelo * Time.deltaTime);

        if (Vector3.Distance(transform.position, destino.position) < 0.1f)
        {
            indiceWaypointActual = (indiceWaypointActual + 1) % waypoints.Length;
        }
    }
}
</file>

<file path="Assets/StreamingAssets/manifiesto.json">
{
    "pedidos": [
        {
            "id_pallet": "PAL-8491",
            "nombre_producto": "CRubia Aguila (Lata 330ml)",
            "codigo_sku": "AGU-RUB-330",
            "peso_kg": 850,
            "coordenada_bodega": "P1-C1-F1"
        },
        {
            "id_pallet": "PAL-8492",
            "nombre_producto": "CNegra ClubColombia (Botella 330ml)",
            "codigo_sku": "CLU-NEG-330",
            "peso_kg": 850,
            "coordenada_bodega": "P1-C2-F1"
        },
        {
            "id_pallet": "PAL-8493",
            "nombre_producto": "CEspecial Oktoberfest (Barril 50L)",
            "codigo_sku": "OKT-BAR-50L",
            "peso_kg": 920,
            "coordenada_bodega": "P2-C4-F1"
        }
    ]
}
</file>

<file path="AGENTS.md">
# AGENTS.md

<!-- Funplay Unity MCP managed project skills -->
<!-- Funplay Unity MCP project skill versions: unity-mcp-workflow@1.0.5, unity-ui-composition@1.0.6 -->

# Funplay Unity MCP Project Guidance

This section is managed by Funplay MCP for Unity. Everything between the begin and end markers is regenerated on each sync; edit outside this block.

## Installed project skills

- `funplay-unity-mcp-workflow` v1.0.5 - Efficient workflow for using Unity MCP to edit, import, compile, inspect, and test Unity projects, including screenshot and Game View recording verification.
- `funplay-unity-ui-composition` v1.0.6 - Build and revise responsive Unity uGUI mobile interfaces, including portrait and landscape layouts, safe areas, prefabs, auto layout, scrolling, text, input, animation, and performance validation.

## Agent workflow rules

- Prefer project-local Funplay skills: `.codex/skills/` for Codex, `.opencode/skills/` for OpenCode, `.dsh/skills/` for DeepSeek Harness, `.agents/skills/` for Antigravity.
- Prefer structured MCP tools; use `execute_code` for project-specific gaps. For new snippets, include `using Funplay.Editor.Tools.Scripting;`, implement `IFunplayCommand`, and use `ctx.RegisterObjectCreation` / `RegisterObjectModification` / `DestroyObject` so changes participate in Undo automatically.
- Confirm the Unity project root, active scene, and real object/prefab/asset path before edits. Treat user-provided object names as hints, not paths.
- Inspect Unity objects through MCP before changing user-named scene or prefab targets. Carry the returned `instanceId` into follow-up calls (`find_method=by_id`) instead of re-resolving by name.
- Tool returns are structured JSON (`{success, message, data}` / `{success: false, code, error, data}`). Branch on `code`, not free-form text.
- Set component fields with `set_component_property(ies)` — it picks up `[SerializeField] private` fields and accepts Object references as `{"fileID": <instanceId>}` or `{"assetPath": "Assets/..."}`.
- For field-only prefab asset edits, use `set_prefab_property(ies)` with a verified `Assets/**/*.prefab` path. If it reports duplicate paths or components, retry only with an index from that response; use Prefab Mode for structural edits.
- Read editor state through dedicated tools (`get_selection`, `get_prefab_stage`, `get_tags`, `get_layers`, `get_build_settings`); use `execute_menu_item` before falling back to ad-hoc `execute_code`.
- Never edit `.unity`, `.prefab`, or `.asset` files with shell text tools or patches; use Unity MCP / Editor APIs for scenes, prefabs, and ScriptableObject assets.
- Save only the scene or prefab assets intentionally modified, then read back exact values.
- Prefer structured query/audit/edit tools in Core; low-frequency configuration and preview management remain in Full. Use `get_tool_capabilities` to check exposure; respect customized allowlists.
- `execute_code` refreshes by default; use skip_refresh only after verifying the intended assemblies are ready.
- In `execute_code`, null-guard every lookup and return explicit missing path/object/component messages; do not run self-healing fallback loops.
- For Unity object references, do not use `??=` for lazy rebinding; use explicit `if (field == null) field = Resolve();`.
- After edits use `prepare_editor` with the intended mode and a unique request_key; use the returned task_id with `get_task`, wait_seconds and after_revision until operation.status=ready and current_editor confirms readiness and mode, then check console errors. Honor poll_after_ms on unchanged results.
- `request_recompile` is rejected while Unity is in Play Mode. Call `exit_play_mode` first, then retry.
- On disconnect, retry the same operation status read by ID/key. Historical reload recovery or HTTP reachability alone does not establish current readiness.
- An interrupted arbitrary mutation has unknown outcome until exact object/asset readback confirms it; never automatically replay it.
- Avoid changing `Library/`, `Temp/`, `Logs/`, or `obj/`.

## Project

- Project root: `C:\Users\Aprendiz\Documents\ProyectosUnity\SistemaLogistico_AMR`
- Product name: `SistemaLogistico_AMR`

## Notes

- Re-run `Funplay > Project Skills` after changing selected skills or platforms.
<!-- /Funplay Unity MCP managed project skills -->
</file>

<file path="Assets/Scripts/GestorJSON.cs">
using UnityEngine;
using System.IO;
using System.Collections.Generic;

/* Definimos la estructura exacta de un pedido en el JSON, Sysytem.Serializable nos permite convertir el JSON en variables de C#*/
[System.Serializable]
public class Pedido
{
    /*Variables idénticas en nombre y tipo de dato a las del JSON para facilitar el trabajo entre sistemas*/
    public string id_pallet;
    public string nombre_producto;
    public string codigo_sku;
    public float peso_kg;
    public string coordenada_bodega;
}

// Envolvemos la lista para que JsonUtility de Unity pueda leer el arreglo
[System.Serializable]
public class Manifiesto
{
    public List<Pedido> pedidos;
}

public class GestorJSON : MonoBehaviour
{
    [Header("Base de Datos Local")]
    public Manifiesto manifiestoActual;
    public RobotAMR robotEjecutor;

    void Start()
    {
        CargarManifiesto();
    }

    void CargarManifiesto()
    {
        /* Ruta dinámica que funciona en el Editor y en el ejecutable final (.exe), streamingAssetsPath es una proteccion contra el Empaquetado(Inmunidad) Unity tiene
        prohibido alterar, comprimir o encriptar lo que hay allí dentro*/
        string ruta = Path.Combine(Application.streamingAssetsPath, "manifiesto.json");
        /*antes de leer el archivo, verifica si el archivo existe, si no existe, lanza un error y detiene el programa*/
        if (File.Exists(ruta))
        {
            /*Si existe, lee el contenido del archivo*/
            string contenidoJson = File.ReadAllText(ruta);
            /* Magia del desacoplamiento: El texto se convierte en variables de C#*/
            manifiestoActual = JsonUtility.FromJson<Manifiesto>(contenidoJson);
            
            Debug.Log("<color=green>Éxito:</color> Manifiesto cargado. Total de pallets a despachar: " + manifiestoActual.pedidos.Count);
            DashboardUI.Instance?.RegistrarLog("Éxito: Manifiesto cargado. Total de pallets a despachar: " + manifiestoActual.pedidos.Count);
            // Si hay pedidos y el robot está conectado, le enviamos la primera orden (Índice 0)
            if (manifiestoActual.pedidos.Count > 0 && robotEjecutor != null)
            {
                robotEjecutor.AsignarMision(manifiestoActual.pedidos[0]);
            }
        }
        else
        {
            Debug.LogError("Error crítico: No se encontró el Manifiesto de Carga en la ruta: " + ruta);
            DashboardUI.Instance?.RegistrarLog("Error crítico: No se encontró el Manifiesto de Carga.");
        }
    }
}
</file>

<file path="UserSettings/Layouts/CurrentMaximizeLayout.dwlt">
%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &1
MonoBehaviour:
  m_ObjectHideFlags: 52
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 12010, guid: 0000000000000000e000000000000000, type: 0}
  m_Name: 
  m_EditorClassIdentifier: 
  m_Children:
  - {fileID: 3}
  m_Position:
    serializedVersion: 2
    x: 0
    y: 30
    width: 1908
    height: 859
  m_MinSize: {x: 300, y: 100}
  m_MaxSize: {x: 24288, y: 16192}
  vertical: 1
  controlID: 44978
  draggingID: 0
--- !u!114 &2
MonoBehaviour:
  m_ObjectHideFlags: 52
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 1
  m_Script: {fileID: 12015, guid: 0000000000000000e000000000000000, type: 0}
  m_Name: 
  m_EditorClassIdentifier: 
  m_MinSize: {x: 200, y: 200}
  m_MaxSize: {x: 4000, y: 4000}
  m_TitleContent:
    m_Text: Game
    m_Image: {fileID: -6423792434712278376, guid: 0000000000000000d000000000000000, type: 0}
    m_Tooltip: 
  m_Pos:
    serializedVersion: 2
    x: 415
    y: 119
    width: 1094
    height: 559
  m_SerializedDataModeController:
    m_DataMode: 0
    m_PreferredDataMode: 0
    m_SupportedDataModes: 
    isAutomatic: 1
  m_ViewDataDictionary: {fileID: 0}
  m_OverlayCanvas:
    m_LastAppliedPresetName: Default
    m_SaveData: []
    m_OverlaysVisible: 1
  m_SerializedViewNames: []
  m_SerializedViewValues: []
  m_PlayModeViewName: GameView
  m_ShowGizmos: 0
  m_TargetDisplay: 0
  m_ClearColor: {r: 0, g: 0, b: 0, a: 0}
  m_TargetSize: {x: 956, y: 538}
  m_TextureFilterMode: 0
  m_TextureHideFlags: 61
  m_RenderIMGUI: 1
  m_EnterPlayModeBehavior: 0
  m_UseMipMap: 0
  m_VSyncEnabled: 0
  m_Gizmos: 0
  m_Stats: 0
  m_SelectedSizes: 01000000000000000000000000000000000000000000000000000000000000000000000000000000
  m_ZoomArea:
    m_HRangeLocked: 0
    m_VRangeLocked: 0
    hZoomLockedByDefault: 0
    vZoomLockedByDefault: 0
    m_HBaseRangeMin: -478
    m_HBaseRangeMax: 478
    m_VBaseRangeMin: -269
    m_VBaseRangeMax: 269
    m_HAllowExceedBaseRangeMin: 1
    m_HAllowExceedBaseRangeMax: 1
    m_VAllowExceedBaseRangeMin: 1
    m_VAllowExceedBaseRangeMax: 1
    m_ScaleWithWindow: 0
    m_HSlider: 0
    m_VSlider: 0
    m_IgnoreScrollWheelUntilClicked: 0
    m_EnableMouseInput: 0
    m_EnableSliderZoomHorizontal: 0
    m_EnableSliderZoomVertical: 0
    m_UniformScale: 1
    m_UpDirection: 1
    m_DrawArea:
      serializedVersion: 2
      x: 0
      y: 21
      width: 1094
      height: 538
    m_Scale: {x: 1, y: 1}
    m_Translation: {x: 547, y: 269}
    m_MarginLeft: 0
    m_MarginRight: 0
    m_MarginTop: 0
    m_MarginBottom: 0
    m_LastShownAreaInsideMargins:
      serializedVersion: 2
      x: -547
      y: -269
      width: 1094
      height: 538
    m_MinimalGUI: 1
  m_defaultScale: 1
  m_LastWindowPixelSize: {x: 1094, y: 559}
  m_ClearInEditMode: 1
  m_NoCameraWarning: 1
  m_LowResolutionForAspectRatios: 01000000000000000000
  m_XRRenderMode: 0
  m_RenderTexture: {fileID: 0}
  m_showToolbar: 1
--- !u!114 &3
MonoBehaviour:
  m_ObjectHideFlags: 52
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 1
  m_Script: {fileID: 12010, guid: 0000000000000000e000000000000000, type: 0}
  m_Name: 
  m_EditorClassIdentifier: 
  m_Children:
  - {fileID: 4}
  - {fileID: 14}
  m_Position:
    serializedVersion: 2
    x: 0
    y: 0
    width: 1908
    height: 859
  m_MinSize: {x: 300, y: 100}
  m_MaxSize: {x: 24288, y: 16192}
  vertical: 0
  controlID: 44979
  draggingID: 0
--- !u!114 &4
MonoBehaviour:
  m_ObjectHideFlags: 52
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 1
  m_Script: {fileID: 12010, guid: 0000000000000000e000000000000000, type: 0}
  m_Name: 
  m_EditorClassIdentifier: 
  m_Children:
  - {fileID: 5}
  - {fileID: 11}
  m_Position:
    serializedVersion: 2
    x: 0
    y: 0
    width: 1505
    height: 859
  m_MinSize: {x: 200, y: 100}
  m_MaxSize: {x: 16192, y: 16192}
  vertical: 1
  controlID: 44980
  draggingID: 0
--- !u!114 &5
MonoBehaviour:
  m_ObjectHideFlags: 52
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 1
  m_Script: {fileID: 12010, guid: 0000000000000000e000000000000000, type: 0}
  m_Name: 
  m_EditorClassIdentifier: 
  m_Children:
  - {fileID: 6}
  - {fileID: 8}
  m_Position:
    serializedVersion: 2
    x: 0
    y: 0
    width: 1505
    height: 580
  m_MinSize: {x: 200, y: 50}
  m_MaxSize: {x: 16192, y: 8096}
  vertical: 0
  controlID: 44981
  draggingID: 0
--- !u!114 &6
MonoBehaviour:
  m_ObjectHideFlags: 52
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 1
  m_Script: {fileID: 12006, guid: 0000000000000000e000000000000000, type: 0}
  m_Name: 
  m_EditorClassIdentifier: 
  m_Children: []
  m_Position:
    serializedVersion: 2
    x: 0
    y: 0
    width: 409
    height: 580
  m_MinSize: {x: 201, y: 221}
  m_MaxSize: {x: 4001, y: 4021}
  m_ActualView: {fileID: 7}
  m_Panes:
  - {fileID: 7}
  m_Selected: 0
  m_LastSelected: 0
--- !u!114 &7
MonoBehaviour:
  m_ObjectHideFlags: 52
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 1
  m_Script: {fileID: 12061, guid: 0000000000000000e000000000000000, type: 0}
  m_Name: 
  m_EditorClassIdentifier: 
  m_MinSize: {x: 200, y: 200}
  m_MaxSize: {x: 4000, y: 4000}
  m_TitleContent:
    m_Text: Hierarchy
    m_Image: {fileID: 7966133145522015247, guid: 0000000000000000d000000000000000, type: 0}
    m_Tooltip: 
  m_Pos:
    serializedVersion: 2
    x: 6
    y: 119
    width: 408
    height: 559
  m_SerializedDataModeController:
    m_DataMode: 0
    m_PreferredDataMode: 0
    m_SupportedDataModes: 
    isAutomatic: 1
  m_ViewDataDictionary: {fileID: 0}
  m_OverlayCanvas:
    m_LastAppliedPresetName: Default
    m_SaveData: []
    m_OverlaysVisible: 1
  m_SceneHierarchy:
    m_TreeViewState:
      scrollPos: {x: 0, y: 0}
      m_SelectedIDs: 
      m_LastClickedID: 0
      m_ExpandedIDs: 8a79feff042dffff2efbffff104f0000424f00006a4f0000e04f0000ea500000
      m_RenameOverlay:
        m_UserAcceptedRename: 0
        m_Name: 
        m_OriginalName: 
        m_EditFieldRect:
          serializedVersion: 2
          x: 0
          y: 0
          width: 0
          height: 0
        m_UserData: 0
        m_IsWaitingForDelay: 0
        m_IsRenaming: 0
        m_OriginalEventType: 11
        m_IsRenamingFilename: 0
        m_ClientGUIView: {fileID: 6}
      m_SearchString: 
    m_ExpandedScenes: []
    m_CurrenRootInstanceID: 0
    m_LockTracker:
      m_IsLocked: 0
    m_CurrentSortingName: TransformSorting
  m_WindowGUID: 4c969a2b90040154d917609493e03593
--- !u!114 &8
MonoBehaviour:
  m_ObjectHideFlags: 52
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 1
  m_Script: {fileID: 12006, guid: 0000000000000000e000000000000000, type: 0}
  m_Name: GameView
  m_EditorClassIdentifier: 
  m_Children: []
  m_Position:
    serializedVersion: 2
    x: 409
    y: 0
    width: 1096
    height: 580
  m_MinSize: {x: 202, y: 221}
  m_MaxSize: {x: 4002, y: 4021}
  m_ActualView: {fileID: 2}
  m_Panes:
  - {fileID: 2}
  - {fileID: 9}
  - {fileID: 10}
  m_Selected: 0
  m_LastSelected: 2
--- !u!114 &9
MonoBehaviour:
  m_ObjectHideFlags: 52
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 12013, guid: 0000000000000000e000000000000000, type: 0}
  m_Name: 
  m_EditorClassIdentifier: 
  m_MinSize: {x: 100, y: 100}
  m_MaxSize: {x: 4000, y: 4000}
  m_TitleContent:
    m_Text: Scene
    m_Image: {fileID: 2593428753322112591, guid: 0000000000000000d000000000000000, type: 0}
    m_Tooltip: 
  m_Pos:
    serializedVersion: 2
    x: 415
    y: 119
    width: 1094
    height: 559
  m_SerializedDataModeController:
    m_DataMode: 0
    m_PreferredDataMode: 0
    m_SupportedDataModes: 
    isAutomatic: 1
  m_ViewDataDictionary: {fileID: 0}
  m_OverlayCanvas:
    m_LastAppliedPresetName: Default
    m_SaveData:
    - dockPosition: 0
      containerId: overlay-toolbar__top
      floating: 0
      collapsed: 0
      displayed: 1
      snapOffset: {x: -179, y: -26}
      snapOffsetDelta: {x: 0, y: 0}
      snapCorner: 3
      id: Tool Settings
      index: 0
      layout: 1
      size: {x: 0, y: 0}
      sizeOverriden: 0
    - dockPosition: 0
      containerId: overlay-toolbar__top
      floating: 0
      collapsed: 0
      displayed: 1
      snapOffset: {x: 169, y: 0}
      snapOffsetDelta: {x: 0, y: 0}
      snapCorner: 0
      id: unity-grid-and-snap-toolbar
      index: 1
      layout: 1
      size: {x: 0, y: 0}
      sizeOverriden: 0
    - dockPosition: 1
      containerId: overlay-toolbar__top
      floating: 0
      collapsed: 0
      displayed: 1
      snapOffset: {x: 0, y: 0}
      snapOffsetDelta: {x: 0, y: 0}
      snapCorner: 0
      id: unity-scene-view-toolbar
      index: 0
      layout: 1
      size: {x: 0, y: 0}
      sizeOverriden: 0
    - dockPosition: 1
      containerId: overlay-toolbar__top
      floating: 0
      collapsed: 0
      displayed: 0
      snapOffset: {x: 0, y: 0}
      snapOffsetDelta: {x: 0, y: 0}
      snapCorner: 1
      id: unity-search-toolbar
      index: 1
      layout: 1
      size: {x: 0, y: 0}
      sizeOverriden: 0
    - dockPosition: 0
      containerId: overlay-container--left
      floating: 0
      collapsed: 0
      displayed: 1
      snapOffset: {x: 0, y: 0}
      snapOffsetDelta: {x: 0, y: 0}
      snapCorner: 0
      id: unity-transform-toolbar
      index: 0
      layout: 2
      size: {x: 0, y: 0}
      sizeOverriden: 0
    - dockPosition: 0
      containerId: overlay-container--left
      floating: 0
      collapsed: 0
      displayed: 1
      snapOffset: {x: 0, y: 197}
      snapOffsetDelta: {x: 0, y: 0}
      snapCorner: 0
      id: unity-component-tools
      index: 1
      layout: 2
      size: {x: 0, y: 0}
      sizeOverriden: 0
    - dockPosition: 0
      containerId: overlay-container--right
      floating: 0
      collapsed: 0
      displayed: 1
      snapOffset: {x: 67.5, y: 86}
      snapOffsetDelta: {x: 0, y: 0}
      snapCorner: 0
      id: Orientation
      index: 0
      layout: 4
      size: {x: 0, y: 0}
      sizeOverriden: 0
    - dockPosition: 1
      containerId: overlay-container--right
      floating: 0
      collapsed: 0
      displayed: 0
      snapOffset: {x: 0, y: 0}
      snapOffsetDelta: {x: 0, y: 0}
      snapCorner: 0
      id: Scene View/Light Settings
      index: 0
      layout: 4
      size: {x: 0, y: 0}
      sizeOverriden: 0
    - dockPosition: 1
      containerId: overlay-container--right
      floating: 0
      collapsed: 0
      displayed: 0
      snapOffset: {x: 0, y: 0}
      snapOffsetDelta: {x: 0, y: 0}
      snapCorner: 0
      id: Scene View/Camera
      index: 1
      layout: 4
      size: {x: 0, y: 0}
      sizeOverriden: 0
    - dockPosition: 1
      containerId: overlay-container--right
      floating: 0
      collapsed: 0
      displayed: 0
      snapOffset: {x: 0, y: 0}
      snapOffsetDelta: {x: 0, y: 0}
      snapCorner: 0
      id: Scene View/Cloth Constraints
      index: 1
      layout: 4
      size: {x: 0, y: 0}
      sizeOverriden: 0
    - dockPosition: 1
      containerId: overlay-container--right
      floating: 0
      collapsed: 0
      displayed: 0
      snapOffset: {x: 0, y: 0}
      snapOffsetDelta: {x: 0, y: 0}
      snapCorner: 0
      id: Scene View/Cloth Collisions
      index: 2
      layout: 4
      size: {x: 0, y: 0}
      sizeOverriden: 0
    - dockPosition: 1
      containerId: overlay-container--right
      floating: 0
      collapsed: 0
      displayed: 0
      snapOffset: {x: 0, y: 0}
      snapOffsetDelta: {x: 0, y: 0}
      snapCorner: 0
      id: Scene View/Navmesh Display
      index: 4
      layout: 4
      size: {x: 0, y: 0}
      sizeOverriden: 0
    - dockPosition: 1
      containerId: overlay-container--right
      floating: 0
      collapsed: 0
      displayed: 0
      snapOffset: {x: 0, y: 0}
      snapOffsetDelta: {x: 0, y: 0}
      snapCorner: 0
      id: Scene View/Agent Display
      index: 5
      layout: 4
      size: {x: 0, y: 0}
      sizeOverriden: 0
    - dockPosition: 1
      containerId: overlay-container--right
      floating: 0
      collapsed: 0
      displayed: 0
      snapOffset: {x: 0, y: 0}
      snapOffsetDelta: {x: 0, y: 0}
      snapCorner: 0
      id: Scene View/Obstacle Display
      index: 6
      layout: 4
      size: {x: 0, y: 0}
      sizeOverriden: 0
    - dockPosition: 1
      containerId: overlay-container--right
      floating: 0
      collapsed: 0
      displayed: 0
      snapOffset: {x: 0, y: 0}
      snapOffsetDelta: {x: 0, y: 0}
      snapCorner: 0
      id: Scene View/Occlusion Culling
      index: 3
      layout: 4
      size: {x: 0, y: 0}
      sizeOverriden: 0
    - dockPosition: 1
      containerId: overlay-container--right
      floating: 0
      collapsed: 0
      displayed: 0
      snapOffset: {x: 0, y: 0}
      snapOffsetDelta: {x: 0, y: 0}
      snapCorner: 0
      id: Scene View/Physics Debugger
      index: 4
      layout: 4
      size: {x: 0, y: 0}
      sizeOverriden: 0
    - dockPosition: 1
      containerId: overlay-container--right
      floating: 0
      collapsed: 0
      displayed: 0
      snapOffset: {x: 0, y: 0}
      snapOffsetDelta: {x: 0, y: 0}
      snapCorner: 0
      id: Scene View/Scene Visibility
      index: 5
      layout: 4
      size: {x: 0, y: 0}
      sizeOverriden: 0
    - dockPosition: 1
      containerId: overlay-container--right
      floating: 0
      collapsed: 0
      displayed: 0
      snapOffset: {x: 0, y: 0}
      snapOffsetDelta: {x: 0, y: 0}
      snapCorner: 0
      id: Scene View/Particles
      index: 6
      layout: 4
      size: {x: 0, y: 0}
      sizeOverriden: 0
    - dockPosition: 1
      containerId: overlay-container--right
      floating: 0
      collapsed: 0
      displayed: 0
      snapOffset: {x: 0, y: 0}
      snapOffsetDelta: {x: 0, y: 0}
      snapCorner: 0
      id: Scene View/Tilemap
      index: 11
      layout: 4
      size: {x: 0, y: 0}
      sizeOverriden: 0
    - dockPosition: 1
      containerId: overlay-container--right
      floating: 0
      collapsed: 0
      displayed: 0
      snapOffset: {x: 0, y: 0}
      snapOffsetDelta: {x: 0, y: 0}
      snapCorner: 0
      id: Scene View/Tilemap Palette Helper
      index: 12
      layout: 4
      size: {x: 0, y: 0}
      sizeOverriden: 0
    - dockPosition: 1
      containerId: overlay-container--right
      floating: 0
      collapsed: 0
      displayed: 1
      snapOffset: {x: 48, y: 10}
      snapOffsetDelta: {x: 0, y: 0}
      snapCorner: 0
      id: UnityEditor.SceneViewCameraOverlay
      index: 9
      layout: 4
      size: {x: 0, y: 0}
      sizeOverriden: 0
    - dockPosition: 1
      containerId: overlay-container--right
      floating: 0
      collapsed: 0
      displayed: 0
      snapOffset: {x: 48, y: 48}
      snapOffsetDelta: {x: 0, y: 0}
      snapCorner: 0
      id: APV Overlay
      index: 7
      layout: 4
      size: {x: 0, y: 0}
      sizeOverriden: 0
    - dockPosition: 1
      containerId: overlay-container--right
      floating: 0
      collapsed: 0
      displayed: 0
      snapOffset: {x: 48, y: 48}
      snapOffsetDelta: {x: 0, y: 0}
      snapCorner: 0
      id: Scene View/TrailRenderer
      index: 7
      layout: 4
      size: {x: 0, y: 0}
      sizeOverriden: 0
    - dockPosition: 1
      containerId: overlay-container--right
      floating: 0
      collapsed: 0
      displayed: 1
      snapOffset: {x: -220, y: -232}
      snapOffsetDelta: {x: 0, y: 0}
      snapCorner: 3
      id: AINavigationOverlay
      index: 8
      layout: 4
      size: {x: 0, y: 0}
      sizeOverriden: 0
    - dockPosition: 1
      containerId: overlay-container--right
      floating: 0
      collapsed: 0
      displayed: 0
      snapOffset: {x: 48, y: 48}
      snapOffsetDelta: {x: 0, y: 0}
      snapCorner: 0
      id: unity-spline-inspector
      index: 9
      layout: 4
      size: {x: 0, y: 0}
      sizeOverriden: 0
    m_OverlaysVisible: 1
  m_WindowGUID: f48192587447b7b4ca9b7a5935333dd6
  m_Gizmos: 1
  m_OverrideSceneCullingMask: 6917529027641081856
  m_SceneIsLit: 1
  m_SceneLighting: 1
  m_2DMode: 0
  m_isRotationLocked: 0
  m_PlayAudio: 0
  m_AudioPlay: 0
  m_Position:
    m_Target: {x: 0, y: 1.3333335, z: 0}
    speed: 2
    m_Value: {x: 0, y: 1.3333335, z: 0}
  m_RenderMode: 0
  m_CameraMode:
    drawMode: 0
    name: Shaded
    section: Shading Mode
  m_ValidateTrueMetals: 0
  m_DoValidateTrueMetals: 0
  m_SceneViewState:
    m_AlwaysRefresh: 0
    showFog: 1
    showSkybox: 1
    showFlares: 1
    showImageEffects: 1
    showParticleSystems: 1
    showVisualEffectGraphs: 1
    m_FxEnabled: 1
  m_Grid:
    xGrid:
      m_Fade:
        m_Target: 0
        speed: 2
        m_Value: 0
      m_Color: {r: 0.5, g: 0.5, b: 0.5, a: 0.4}
      m_Pivot: {x: 0, y: 0, z: 0}
      m_Size: {x: 1, y: 1}
    yGrid:
      m_Fade:
        m_Target: 0
        speed: 2
        m_Value: 0
      m_Color: {r: 0.5, g: 0.5, b: 0.5, a: 0.4}
      m_Pivot: {x: 0, y: 0, z: 0}
      m_Size: {x: 1, y: 1}
    zGrid:
      m_Fade:
        m_Target: 0
        speed: 2
        m_Value: 0
      m_Color: {r: 0.5, g: 0.5, b: 0.5, a: 0.4}
      m_Pivot: {x: 0, y: 0, z: 0}
      m_Size: {x: 1, y: 1}
    m_ShowGrid: 0
    m_GridAxis: 1
    m_gridOpacity: 0.5
  m_Rotation:
    m_Target: {x: 0.7071068, y: 0, z: 0, w: 0.7071068}
    speed: 2
    m_Value: {x: 0.7071068, y: 0, z: 0, w: 0.7071068}
  m_Size:
    m_Target: 28.94185
    speed: 2
    m_Value: 28.94185
  m_Ortho:
    m_Target: 1
    speed: 2
    m_Value: 1
  m_CameraSettings:
    m_Speed: 0.8855999
    m_SpeedNormalized: 0.43999997
    m_SpeedMin: 0.01
    m_SpeedMax: 2
    m_EasingEnabled: 1
    m_EasingDuration: 0.4
    m_AccelerationEnabled: 1
    m_FieldOfViewHorizontalOrVertical: 60
    m_NearClip: 0.03
    m_FarClip: 10000
    m_DynamicClip: 1
    m_OcclusionCulling: 0
  m_LastSceneViewRotation: {x: 0, y: 0, z: 0, w: 0}
  m_LastSceneViewOrtho: 0
  m_ReplacementShader: {fileID: 0}
  m_ReplacementString: 
  m_SceneVisActive: 1
  m_LastLockedObject: {fileID: 0}
  m_ViewIsLockedToObject: 0
--- !u!114 &10
MonoBehaviour:
  m_ObjectHideFlags: 52
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 12914, guid: 0000000000000000e000000000000000, type: 0}
  m_Name: 
  m_EditorClassIdentifier: 
  m_MinSize: {x: 100, y: 100}
  m_MaxSize: {x: 4000, y: 4000}
  m_TitleContent:
    m_Text: Animator
    m_Image: {fileID: -1673928668082335149, guid: 0000000000000000d000000000000000, type: 0}
    m_Tooltip: 
  m_Pos:
    serializedVersion: 2
    x: 546
    y: 73
    width: 1466
    height: 763
  m_SerializedDataModeController:
    m_DataMode: 0
    m_PreferredDataMode: 0
    m_SupportedDataModes: 
    isAutomatic: 1
  m_ViewDataDictionary: {fileID: 0}
  m_OverlayCanvas:
    m_LastAppliedPresetName: Default
    m_SaveData: []
    m_OverlaysVisible: 1
  m_ViewTransforms:
    m_KeySerializationHelper:
    - {fileID: 3721131351775251420, guid: fbe27581ff6b8324a95c01cc3d2d2548, type: 2}
    m_ValueSerializationHelper:
    - e00: 1
      e01: 0
      e02: 0
      e03: 0
      e10: 0
      e11: 1
      e12: 0
      e13: 0
      e20: 0
      e21: 0
      e22: 1
      e23: 0
      e30: 0
      e31: 0
      e32: 0
      e33: 1
  m_PreviewAnimator: {fileID: 0}
  m_AnimatorController: {fileID: 9100000, guid: fbe27581ff6b8324a95c01cc3d2d2548, type: 2}
  m_BreadCrumbs:
  - m_Target: {fileID: 2390719094264726621, guid: fbe27581ff6b8324a95c01cc3d2d2548, type: 2}
    m_ScrollPosition: {x: 0, y: 0}
  - m_Target: {fileID: 3721131351775251420, guid: fbe27581ff6b8324a95c01cc3d2d2548, type: 2}
    m_ScrollPosition: {x: 0, y: 0}
  stateMachineGraph: {fileID: 0}
  stateMachineGraphGUI: {fileID: 0}
  blendTreeGraph: {fileID: 0}
  blendTreeGraphGUI: {fileID: 0}
  m_AutoLiveLink: 1
  m_MiniTool: 0
  m_LockTracker:
    m_IsLocked: 0
  m_CurrentEditor: 1
  m_LayerEditor:
    m_SelectedLayerIndex: 0
--- !u!114 &11
MonoBehaviour:
  m_ObjectHideFlags: 52
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 1
  m_Script: {fileID: 12006, guid: 0000000000000000e000000000000000, type: 0}
  m_Name: ProjectBrowser
  m_EditorClassIdentifier: 
  m_Children: []
  m_Position:
    serializedVersion: 2
    x: 0
    y: 580
    width: 1505
    height: 279
  m_MinSize: {x: 231, y: 271}
  m_MaxSize: {x: 10001, y: 10021}
  m_ActualView: {fileID: 12}
  m_Panes:
  - {fileID: 12}
  - {fileID: 13}
  m_Selected: 0
  m_LastSelected: 1
--- !u!114 &12
MonoBehaviour:
  m_ObjectHideFlags: 52
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 1
  m_Script: {fileID: 12014, guid: 0000000000000000e000000000000000, type: 0}
  m_Name: 
  m_EditorClassIdentifier: 
  m_MinSize: {x: 230, y: 250}
  m_MaxSize: {x: 10000, y: 10000}
  m_TitleContent:
    m_Text: Project
    m_Image: {fileID: -5467254957812901981, guid: 0000000000000000d000000000000000, type: 0}
    m_Tooltip: 
  m_Pos:
    serializedVersion: 2
    x: 6
    y: 699
    width: 1504
    height: 258
  m_SerializedDataModeController:
    m_DataMode: 0
    m_PreferredDataMode: 0
    m_SupportedDataModes: 
    isAutomatic: 1
  m_ViewDataDictionary: {fileID: 0}
  m_OverlayCanvas:
    m_LastAppliedPresetName: Default
    m_SaveData: []
    m_OverlaysVisible: 1
  m_SearchFilter:
    m_NameFilter: 
    m_ClassNames: []
    m_AssetLabels: []
    m_AssetBundleNames: []
    m_ReferencingInstanceIDs: 
    m_SceneHandles: 
    m_ShowAllHits: 0
    m_SkipHidden: 0
    m_SearchArea: 1
    m_Folders:
    - Assets/Prefab
    m_Globs: []
    m_OriginalText: 
    m_ImportLogFlags: 0
    m_FilterByTypeIntersection: 0
  m_ViewMode: 1
  m_StartGridSize: 64
  m_LastFolders:
  - Assets/Prefab
  m_LastFoldersGridSize: -1
  m_LastProjectPath: /mnt/IgniteDrive/Proyectos_Unity/SistemaLogistico_AMR
  m_LockTracker:
    m_IsLocked: 0
  m_FolderTreeState:
    scrollPos: {x: 0, y: 51}
    m_SelectedIDs: ac630000
    m_LastClickedID: 25516
    m_ExpandedIDs: 000000006651000000ca9a3bffffff7f
    m_RenameOverlay:
      m_UserAcceptedRename: 0
      m_Name: 
      m_OriginalName: 
      m_EditFieldRect:
        serializedVersion: 2
        x: 0
        y: 0
        width: 0
        height: 0
      m_UserData: 0
      m_IsWaitingForDelay: 0
      m_IsRenaming: 0
      m_OriginalEventType: 11
      m_IsRenamingFilename: 1
      m_ClientGUIView: {fileID: 0}
    m_SearchString: 
    m_CreateAssetUtility:
      m_EndAction: {fileID: 0}
      m_InstanceID: 0
      m_Path: 
      m_Icon: {fileID: 0}
      m_ResourceFile: 
  m_AssetTreeState:
    scrollPos: {x: 0, y: 0}
    m_SelectedIDs: 
    m_LastClickedID: 0
    m_ExpandedIDs: 000000006651000000ca9a3bffffff7f
    m_RenameOverlay:
      m_UserAcceptedRename: 0
      m_Name: 
      m_OriginalName: 
      m_EditFieldRect:
        serializedVersion: 2
        x: 0
        y: 0
        width: 0
        height: 0
      m_UserData: 0
      m_IsWaitingForDelay: 0
      m_IsRenaming: 0
      m_OriginalEventType: 11
      m_IsRenamingFilename: 1
      m_ClientGUIView: {fileID: 0}
    m_SearchString: 
    m_CreateAssetUtility:
      m_EndAction: {fileID: 0}
      m_InstanceID: 0
      m_Path: 
      m_Icon: {fileID: 0}
      m_ResourceFile: 
  m_ListAreaState:
    m_SelectedInstanceIDs: 
    m_LastClickedInstanceID: 0
    m_HadKeyboardFocusLastEvent: 0
    m_ExpandedInstanceIDs: c623000022450000985100002c520000a4510000744f0000e6540000ec54000050740000d2770000f080000032810000f07e000068020000067c00006c02000000000000025400002c510000
    m_RenameOverlay:
      m_UserAcceptedRename: 0
      m_Name: 
      m_OriginalName: 
      m_EditFieldRect:
        serializedVersion: 2
        x: 0
        y: 0
        width: 0
        height: 0
      m_UserData: 0
      m_IsWaitingForDelay: 0
      m_IsRenaming: 0
      m_OriginalEventType: 11
      m_IsRenamingFilename: 1
      m_ClientGUIView: {fileID: 0}
    m_CreateAssetUtility:
      m_EndAction: {fileID: 0}
      m_InstanceID: 0
      m_Path: 
      m_Icon: {fileID: 0}
      m_ResourceFile: 
    m_NewAssetIndexInList: -1
    m_ScrollPosition: {x: 0, y: 0}
    m_GridSize: 64
  m_SkipHiddenPackages: 0
  m_DirectoriesAreaWidth: 207
--- !u!114 &13
MonoBehaviour:
  m_ObjectHideFlags: 52
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 1
  m_Script: {fileID: 12003, guid: 0000000000000000e000000000000000, type: 0}
  m_Name: 
  m_EditorClassIdentifier: 
  m_MinSize: {x: 100, y: 100}
  m_MaxSize: {x: 4000, y: 4000}
  m_TitleContent:
    m_Text: Console
    m_Image: {fileID: -4327648978806127646, guid: 0000000000000000d000000000000000, type: 0}
    m_Tooltip: 
  m_Pos:
    serializedVersion: 2
    x: 6
    y: 699
    width: 1504
    height: 258
  m_SerializedDataModeController:
    m_DataMode: 0
    m_PreferredDataMode: 0
    m_SupportedDataModes: 
    isAutomatic: 1
  m_ViewDataDictionary: {fileID: 0}
  m_OverlayCanvas:
    m_LastAppliedPresetName: Default
    m_SaveData: []
    m_OverlaysVisible: 1
--- !u!114 &14
MonoBehaviour:
  m_ObjectHideFlags: 52
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 1
  m_Script: {fileID: 12006, guid: 0000000000000000e000000000000000, type: 0}
  m_Name: InspectorWindow
  m_EditorClassIdentifier: 
  m_Children: []
  m_Position:
    serializedVersion: 2
    x: 1505
    y: 0
    width: 403
    height: 859
  m_MinSize: {x: 275, y: 50}
  m_MaxSize: {x: 4000, y: 4000}
  m_ActualView: {fileID: 15}
  m_Panes:
  - {fileID: 15}
  m_Selected: 0
  m_LastSelected: 0
--- !u!114 &15
MonoBehaviour:
  m_ObjectHideFlags: 52
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 1
  m_Script: {fileID: 12019, guid: 0000000000000000e000000000000000, type: 0}
  m_Name: 
  m_EditorClassIdentifier: 
  m_MinSize: {x: 275, y: 50}
  m_MaxSize: {x: 4000, y: 4000}
  m_TitleContent:
    m_Text: Inspector
    m_Image: {fileID: -2667387946076563598, guid: 0000000000000000d000000000000000, type: 0}
    m_Tooltip: 
  m_Pos:
    serializedVersion: 2
    x: 1511
    y: 119
    width: 402
    height: 838
  m_SerializedDataModeController:
    m_DataMode: 0
    m_PreferredDataMode: 0
    m_SupportedDataModes: 
    isAutomatic: 1
  m_ViewDataDictionary: {fileID: 0}
  m_OverlayCanvas:
    m_LastAppliedPresetName: Default
    m_SaveData: []
    m_OverlaysVisible: 1
  m_ObjectsLockedBeforeSerialization: []
  m_InstanceIDsLockedBeforeSerialization: 
  m_PreviewResizer:
    m_CachedPref: -161
    m_ControlHash: -371814159
    m_PrefName: Preview_InspectorPreview
  m_LastInspectedObjectInstanceID: -1
  m_LastVerticalScrollValue: 0
  m_GlobalObjectId: 
  m_InspectorMode: 0
  m_LockTracker:
    m_IsLocked: 0
  m_PreviewWindow: {fileID: 0}
</file>

<file path="Assets/Scripts/RobotAMR.cs">
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class RobotAMR : MonoBehaviour
{
    /* Máquina de Estados Finitos (FSM) para blindar el código contra errores lógicos, enum es una lista restrictiva.
    Le prohíbe a la variable estadoActual tener cualquier otro valor que no sea uno de esos tres.*/
    public enum EstadoRobot { Inactivo, En_Transito, Extrayendo, Transportando, Interrumpido, Entregando }
    
    [Header("Panel de Control (Solo lectura)")]
    public EstadoRobot estadoActual = EstadoRobot.Inactivo;
    /*El NavMeshAgent es el motor físico de Unity. Pero nota la variable Pedido. Tu cilindro físico no conoce el archivo JSON completo ni le interesa.
    Solo conoce la estructura de un pedido aislado que le entrega el Gestor, manteniendo el código limpio y modular.*/
    private NavMeshAgent agente;
    private Pedido pedidoActual; // Conoce la estructura, pero no toda la base de datos

    [Header("Referencias (LIFO)")]
    public RobotManagerEVE managerEVE;
    public Transform zonaTransferencia;
    private Transform destinoActual;

    void Awake()
    {
        agente = GetComponent<NavMeshAgent>();
    }

    public void AsignarMision(Pedido nuevoPedido)
    {
        /* Regla de seguridad: Solo acepta misiones si está inactivo
        el return funciona como una pared que protege a la mision actual*/
        if (estadoActual != EstadoRobot.Inactivo) return;

        pedidoActual = nuevoPedido;
        
        if (pedidoActual.peso_kg > 900)
        {
            DashboardUI.Instance?.RegistrarLog("<color=red>ALERTA CRÍTICA: Batería insuficiente para pedido masivo.</color>");
            estadoActual = EstadoRobot.Interrumpido;
            agente.isStopped = true;
            return;
        }

        /*Busca en el mundo 3D el GameObject que se llame exactamente como dice el JSON*/
        GameObject destino = GameObject.Find(pedidoActual.coordenada_bodega);

        if (destino != null)
        {
            destinoActual = destino.transform;
            estadoActual = EstadoRobot.En_Transito;
            agente.SetDestination(destinoActual.position); // Da la orden de moverse
            Debug.Log("<color=cyan>AMR Desplegado:</color> Viajando a " + destino.name + " para buscar " + pedidoActual.codigo_sku);
            DashboardUI.Instance?.RegistrarLog("AMR Desplegado: Viajando a " + destino.name);
        }
        else
        {
            Debug.LogError("Alerta Logística: No existe la coordenada " + pedidoActual.coordenada_bodega + " en la bodega física.");
            DashboardUI.Instance?.RegistrarLog("Alerta: No existe la coordenada " + pedidoActual.coordenada_bodega);
        }
    }

    IEnumerator ProcesoExtraccion()
    {
        // Simulación de tiempo de extracción de carga
        yield return new WaitForSeconds(2f);
        
        if (zonaTransferencia != null)
        {
            estadoActual = EstadoRobot.Transportando;
            agente.SetDestination(zonaTransferencia.position);
            Debug.Log("<color=cyan>AMR Retornando:</color> Llevando estiba a la Zona de Transferencia.");
            DashboardUI.Instance?.RegistrarLog("AMR Retornando: Llevando estiba a Zona de Transferencia.");
        }
        else
        {
            Debug.LogError("AMR Error: Falta asignar la Zona de Transferencia en el Inspector.");
            DashboardUI.Instance?.RegistrarLog("AMR Error: Falta Zona de Transferencia.");
        }
    }

    void Update()
    {
        if (estadoActual == EstadoRobot.En_Transito)
        {
            if (!agente.pathPending && agente.remainingDistance <= 1.0f)
            {
                estadoActual = EstadoRobot.Extrayendo;
                Debug.Log("<color=yellow>Destino Alcanzado:</color> Extrayendo estiba de " + pedidoActual.peso_kg + " kg.");
                DashboardUI.Instance?.RegistrarLog("Destino Alcanzado: Extrayendo estiba.");
                StartCoroutine(ProcesoExtraccion());
            }
        }
        else if (estadoActual == EstadoRobot.Transportando)
        {
            // Chequeo de distancia cuando el robot está retornando
            if (zonaTransferencia != null && Vector3.Distance(transform.position, zonaTransferencia.position) < 0.5f)
            {
                estadoActual = EstadoRobot.Entregando;
                Debug.Log("<color=magenta>LIFO:</color> Entregando estiba en Zona de Transferencia.");
                DashboardUI.Instance?.RegistrarLog("LIFO: Entregando estiba en Zona de Transferencia.");
                
                if (managerEVE != null)
                {
                    managerEVE.ConfirmarEntregaExitosa();
                }

                estadoActual = EstadoRobot.Inactivo;
            }
        }
    }
}
</file>

<file path="UserSettings/Layouts/default-2022.dwlt">
%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &1
MonoBehaviour:
  m_ObjectHideFlags: 52
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 1
  m_Script: {fileID: 12004, guid: 0000000000000000e000000000000000, type: 0}
  m_Name: 
  m_EditorClassIdentifier: 
  m_PixelRect:
    serializedVersion: 2
    x: 6
    y: 62
    width: 1908
    height: 909
  m_ShowMode: 4
  m_Title: Game
  m_RootView: {fileID: 2}
  m_MinSize: {x: 875, y: 321}
  m_MaxSize: {x: 10000, y: 10000}
  m_Maximized: 0
--- !u!114 &2
MonoBehaviour:
  m_ObjectHideFlags: 52
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 1
  m_Script: {fileID: 12008, guid: 0000000000000000e000000000000000, type: 0}
  m_Name: 
  m_EditorClassIdentifier: 
  m_Children:
  - {fileID: 3}
  - {fileID: 5}
  - {fileID: 4}
  m_Position:
    serializedVersion: 2
    x: 0
    y: 0
    width: 1908
    height: 909
  m_MinSize: {x: 875, y: 300}
  m_MaxSize: {x: 10000, y: 10000}
  m_UseTopView: 1
  m_TopViewHeight: 30
  m_UseBottomView: 1
  m_BottomViewHeight: 20
--- !u!114 &3
MonoBehaviour:
  m_ObjectHideFlags: 52
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 1
  m_Script: {fileID: 12011, guid: 0000000000000000e000000000000000, type: 0}
  m_Name: 
  m_EditorClassIdentifier: 
  m_Children: []
  m_Position:
    serializedVersion: 2
    x: 0
    y: 0
    width: 1908
    height: 30
  m_MinSize: {x: 0, y: 0}
  m_MaxSize: {x: 0, y: 0}
  m_LastLoadedLayoutName: 
--- !u!114 &4
MonoBehaviour:
  m_ObjectHideFlags: 52
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 1
  m_Script: {fileID: 12042, guid: 0000000000000000e000000000000000, type: 0}
  m_Name: 
  m_EditorClassIdentifier: 
  m_Children: []
  m_Position:
    serializedVersion: 2
    x: 0
    y: 889
    width: 1908
    height: 20
  m_MinSize: {x: 0, y: 0}
  m_MaxSize: {x: 0, y: 0}
--- !u!114 &5
MonoBehaviour:
  m_ObjectHideFlags: 52
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 12010, guid: 0000000000000000e000000000000000, type: 0}
  m_Name: 
  m_EditorClassIdentifier: 
  m_Children:
  - {fileID: 6}
  m_Position:
    serializedVersion: 2
    x: 0
    y: 30
    width: 1908
    height: 859
  m_MinSize: {x: 300, y: 100}
  m_MaxSize: {x: 24288, y: 16192}
  vertical: 1
  controlID: 45163
  draggingID: 0
--- !u!114 &6
MonoBehaviour:
  m_ObjectHideFlags: 52
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 1
  m_Script: {fileID: 12010, guid: 0000000000000000e000000000000000, type: 0}
  m_Name: 
  m_EditorClassIdentifier: 
  m_Children:
  - {fileID: 7}
  - {fileID: 12}
  m_Position:
    serializedVersion: 2
    x: 0
    y: 0
    width: 1908
    height: 859
  m_MinSize: {x: 300, y: 100}
  m_MaxSize: {x: 24288, y: 16192}
  vertical: 0
  controlID: 45164
  draggingID: 0
--- !u!114 &7
MonoBehaviour:
  m_ObjectHideFlags: 52
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 1
  m_Script: {fileID: 12010, guid: 0000000000000000e000000000000000, type: 0}
  m_Name: 
  m_EditorClassIdentifier: 
  m_Children:
  - {fileID: 8}
  - {fileID: 11}
  m_Position:
    serializedVersion: 2
    x: 0
    y: 0
    width: 1505
    height: 859
  m_MinSize: {x: 200, y: 100}
  m_MaxSize: {x: 16192, y: 16192}
  vertical: 1
  controlID: 45165
  draggingID: 0
--- !u!114 &8
MonoBehaviour:
  m_ObjectHideFlags: 52
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 1
  m_Script: {fileID: 12010, guid: 0000000000000000e000000000000000, type: 0}
  m_Name: 
  m_EditorClassIdentifier: 
  m_Children:
  - {fileID: 9}
  - {fileID: 10}
  m_Position:
    serializedVersion: 2
    x: 0
    y: 0
    width: 1505
    height: 580
  m_MinSize: {x: 200, y: 50}
  m_MaxSize: {x: 16192, y: 8096}
  vertical: 0
  controlID: 45166
  draggingID: 0
--- !u!114 &9
MonoBehaviour:
  m_ObjectHideFlags: 52
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 1
  m_Script: {fileID: 12006, guid: 0000000000000000e000000000000000, type: 0}
  m_Name: 
  m_EditorClassIdentifier: 
  m_Children: []
  m_Position:
    serializedVersion: 2
    x: 0
    y: 0
    width: 409
    height: 580
  m_MinSize: {x: 201, y: 221}
  m_MaxSize: {x: 4001, y: 4021}
  m_ActualView: {fileID: 14}
  m_Panes:
  - {fileID: 14}
  m_Selected: 0
  m_LastSelected: 0
--- !u!114 &10
MonoBehaviour:
  m_ObjectHideFlags: 52
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 1
  m_Script: {fileID: 12006, guid: 0000000000000000e000000000000000, type: 0}
  m_Name: GameView
  m_EditorClassIdentifier: 
  m_Children: []
  m_Position:
    serializedVersion: 2
    x: 409
    y: 0
    width: 1096
    height: 580
  m_MinSize: {x: 202, y: 221}
  m_MaxSize: {x: 4002, y: 4021}
  m_ActualView: {fileID: 13}
  m_Panes:
  - {fileID: 13}
  - {fileID: 15}
  - {fileID: 16}
  m_Selected: 0
  m_LastSelected: 1
--- !u!114 &11
MonoBehaviour:
  m_ObjectHideFlags: 52
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 1
  m_Script: {fileID: 12006, guid: 0000000000000000e000000000000000, type: 0}
  m_Name: ProjectBrowser
  m_EditorClassIdentifier: 
  m_Children: []
  m_Position:
    serializedVersion: 2
    x: 0
    y: 580
    width: 1505
    height: 279
  m_MinSize: {x: 231, y: 271}
  m_MaxSize: {x: 10001, y: 10021}
  m_ActualView: {fileID: 17}
  m_Panes:
  - {fileID: 17}
  - {fileID: 18}
  m_Selected: 0
  m_LastSelected: 1
--- !u!114 &12
MonoBehaviour:
  m_ObjectHideFlags: 52
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 1
  m_Script: {fileID: 12006, guid: 0000000000000000e000000000000000, type: 0}
  m_Name: InspectorWindow
  m_EditorClassIdentifier: 
  m_Children: []
  m_Position:
    serializedVersion: 2
    x: 1505
    y: 0
    width: 403
    height: 859
  m_MinSize: {x: 276, y: 71}
  m_MaxSize: {x: 4001, y: 4021}
  m_ActualView: {fileID: 19}
  m_Panes:
  - {fileID: 19}
  m_Selected: 0
  m_LastSelected: 0
--- !u!114 &13
MonoBehaviour:
  m_ObjectHideFlags: 52
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 1
  m_Script: {fileID: 12015, guid: 0000000000000000e000000000000000, type: 0}
  m_Name: 
  m_EditorClassIdentifier: 
  m_MinSize: {x: 200, y: 200}
  m_MaxSize: {x: 4000, y: 4000}
  m_TitleContent:
    m_Text: Game
    m_Image: {fileID: -6423792434712278376, guid: 0000000000000000d000000000000000, type: 0}
    m_Tooltip: 
  m_Pos:
    serializedVersion: 2
    x: 415
    y: 119
    width: 1094
    height: 559
  m_SerializedDataModeController:
    m_DataMode: 0
    m_PreferredDataMode: 0
    m_SupportedDataModes: 
    isAutomatic: 1
  m_ViewDataDictionary: {fileID: 0}
  m_OverlayCanvas:
    m_LastAppliedPresetName: Default
    m_SaveData: []
    m_OverlaysVisible: 1
  m_SerializedViewNames: []
  m_SerializedViewValues: []
  m_PlayModeViewName: GameView
  m_ShowGizmos: 0
  m_TargetDisplay: 0
  m_ClearColor: {r: 0, g: 0, b: 0, a: 0}
  m_TargetSize: {x: 956, y: 538}
  m_TextureFilterMode: 0
  m_TextureHideFlags: 61
  m_RenderIMGUI: 1
  m_EnterPlayModeBehavior: 0
  m_UseMipMap: 0
  m_VSyncEnabled: 0
  m_Gizmos: 0
  m_Stats: 0
  m_SelectedSizes: 01000000000000000000000000000000000000000000000000000000000000000000000000000000
  m_ZoomArea:
    m_HRangeLocked: 0
    m_VRangeLocked: 0
    hZoomLockedByDefault: 0
    vZoomLockedByDefault: 0
    m_HBaseRangeMin: -478
    m_HBaseRangeMax: 478
    m_VBaseRangeMin: -269
    m_VBaseRangeMax: 269
    m_HAllowExceedBaseRangeMin: 1
    m_HAllowExceedBaseRangeMax: 1
    m_VAllowExceedBaseRangeMin: 1
    m_VAllowExceedBaseRangeMax: 1
    m_ScaleWithWindow: 0
    m_HSlider: 0
    m_VSlider: 0
    m_IgnoreScrollWheelUntilClicked: 0
    m_EnableMouseInput: 1
    m_EnableSliderZoomHorizontal: 0
    m_EnableSliderZoomVertical: 0
    m_UniformScale: 1
    m_UpDirection: 1
    m_DrawArea:
      serializedVersion: 2
      x: 0
      y: 21
      width: 1094
      height: 538
    m_Scale: {x: 1, y: 1}
    m_Translation: {x: 547, y: 269}
    m_MarginLeft: 0
    m_MarginRight: 0
    m_MarginTop: 0
    m_MarginBottom: 0
    m_LastShownAreaInsideMargins:
      serializedVersion: 2
      x: -547
      y: -269
      width: 1094
      height: 538
    m_MinimalGUI: 1
  m_defaultScale: 1
  m_LastWindowPixelSize: {x: 1094, y: 559}
  m_ClearInEditMode: 1
  m_NoCameraWarning: 1
  m_LowResolutionForAspectRatios: 01000000000000000000
  m_XRRenderMode: 0
  m_RenderTexture: {fileID: 0}
  m_showToolbar: 1
--- !u!114 &14
MonoBehaviour:
  m_ObjectHideFlags: 52
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 1
  m_Script: {fileID: 12061, guid: 0000000000000000e000000000000000, type: 0}
  m_Name: 
  m_EditorClassIdentifier: 
  m_MinSize: {x: 200, y: 200}
  m_MaxSize: {x: 4000, y: 4000}
  m_TitleContent:
    m_Text: Hierarchy
    m_Image: {fileID: 7966133145522015247, guid: 0000000000000000d000000000000000, type: 0}
    m_Tooltip: 
  m_Pos:
    serializedVersion: 2
    x: 6
    y: 119
    width: 408
    height: 559
  m_SerializedDataModeController:
    m_DataMode: 0
    m_PreferredDataMode: 0
    m_SupportedDataModes: 
    isAutomatic: 1
  m_ViewDataDictionary: {fileID: 0}
  m_OverlayCanvas:
    m_LastAppliedPresetName: Default
    m_SaveData: []
    m_OverlaysVisible: 1
  m_SceneHierarchy:
    m_TreeViewState:
      scrollPos: {x: 0, y: 0}
      m_SelectedIDs: 
      m_LastClickedID: 0
      m_ExpandedIDs: 8a79feff042dffff2efbffff104f0000424f00006a4f0000e04f0000ea500000
      m_RenameOverlay:
        m_UserAcceptedRename: 0
        m_Name: 
        m_OriginalName: 
        m_EditFieldRect:
          serializedVersion: 2
          x: 0
          y: 0
          width: 0
          height: 0
        m_UserData: 0
        m_IsWaitingForDelay: 0
        m_IsRenaming: 0
        m_OriginalEventType: 11
        m_IsRenamingFilename: 0
        m_ClientGUIView: {fileID: 9}
      m_SearchString: 
    m_ExpandedScenes: []
    m_CurrenRootInstanceID: 0
    m_LockTracker:
      m_IsLocked: 0
    m_CurrentSortingName: TransformSorting
  m_WindowGUID: 4c969a2b90040154d917609493e03593
--- !u!114 &15
MonoBehaviour:
  m_ObjectHideFlags: 52
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 12013, guid: 0000000000000000e000000000000000, type: 0}
  m_Name: 
  m_EditorClassIdentifier: 
  m_MinSize: {x: 100, y: 100}
  m_MaxSize: {x: 4000, y: 4000}
  m_TitleContent:
    m_Text: Scene
    m_Image: {fileID: 2593428753322112591, guid: 0000000000000000d000000000000000, type: 0}
    m_Tooltip: 
  m_Pos:
    serializedVersion: 2
    x: 415
    y: 119
    width: 1094
    height: 559
  m_SerializedDataModeController:
    m_DataMode: 0
    m_PreferredDataMode: 0
    m_SupportedDataModes: 
    isAutomatic: 1
  m_ViewDataDictionary: {fileID: 0}
  m_OverlayCanvas:
    m_LastAppliedPresetName: Default
    m_SaveData:
    - dockPosition: 0
      containerId: overlay-toolbar__top
      floating: 0
      collapsed: 0
      displayed: 1
      snapOffset: {x: -179, y: -26}
      snapOffsetDelta: {x: 0, y: 0}
      snapCorner: 3
      id: Tool Settings
      index: 0
      layout: 1
      size: {x: 0, y: 0}
      sizeOverriden: 0
    - dockPosition: 0
      containerId: overlay-toolbar__top
      floating: 0
      collapsed: 0
      displayed: 1
      snapOffset: {x: 169, y: 0}
      snapOffsetDelta: {x: 0, y: 0}
      snapCorner: 0
      id: unity-grid-and-snap-toolbar
      index: 1
      layout: 1
      size: {x: 0, y: 0}
      sizeOverriden: 0
    - dockPosition: 1
      containerId: overlay-toolbar__top
      floating: 0
      collapsed: 0
      displayed: 1
      snapOffset: {x: 0, y: 0}
      snapOffsetDelta: {x: 0, y: 0}
      snapCorner: 0
      id: unity-scene-view-toolbar
      index: 0
      layout: 1
      size: {x: 0, y: 0}
      sizeOverriden: 0
    - dockPosition: 1
      containerId: overlay-toolbar__top
      floating: 0
      collapsed: 0
      displayed: 0
      snapOffset: {x: 0, y: 0}
      snapOffsetDelta: {x: 0, y: 0}
      snapCorner: 1
      id: unity-search-toolbar
      index: 1
      layout: 1
      size: {x: 0, y: 0}
      sizeOverriden: 0
    - dockPosition: 0
      containerId: overlay-container--left
      floating: 0
      collapsed: 0
      displayed: 1
      snapOffset: {x: 0, y: 0}
      snapOffsetDelta: {x: 0, y: 0}
      snapCorner: 0
      id: unity-transform-toolbar
      index: 0
      layout: 2
      size: {x: 0, y: 0}
      sizeOverriden: 0
    - dockPosition: 0
      containerId: overlay-container--left
      floating: 0
      collapsed: 0
      displayed: 1
      snapOffset: {x: 0, y: 197}
      snapOffsetDelta: {x: 0, y: 0}
      snapCorner: 0
      id: unity-component-tools
      index: 1
      layout: 2
      size: {x: 0, y: 0}
      sizeOverriden: 0
    - dockPosition: 0
      containerId: overlay-container--right
      floating: 0
      collapsed: 0
      displayed: 1
      snapOffset: {x: 67.5, y: 86}
      snapOffsetDelta: {x: 0, y: 0}
      snapCorner: 0
      id: Orientation
      index: 0
      layout: 4
      size: {x: 0, y: 0}
      sizeOverriden: 0
    - dockPosition: 1
      containerId: overlay-container--right
      floating: 0
      collapsed: 0
      displayed: 0
      snapOffset: {x: 0, y: 0}
      snapOffsetDelta: {x: 0, y: 0}
      snapCorner: 0
      id: Scene View/Light Settings
      index: 0
      layout: 4
      size: {x: 0, y: 0}
      sizeOverriden: 0
    - dockPosition: 1
      containerId: overlay-container--right
      floating: 0
      collapsed: 0
      displayed: 0
      snapOffset: {x: 0, y: 0}
      snapOffsetDelta: {x: 0, y: 0}
      snapCorner: 0
      id: Scene View/Camera
      index: 1
      layout: 4
      size: {x: 0, y: 0}
      sizeOverriden: 0
    - dockPosition: 1
      containerId: overlay-container--right
      floating: 0
      collapsed: 0
      displayed: 0
      snapOffset: {x: 0, y: 0}
      snapOffsetDelta: {x: 0, y: 0}
      snapCorner: 0
      id: Scene View/Cloth Constraints
      index: 1
      layout: 4
      size: {x: 0, y: 0}
      sizeOverriden: 0
    - dockPosition: 1
      containerId: overlay-container--right
      floating: 0
      collapsed: 0
      displayed: 0
      snapOffset: {x: 0, y: 0}
      snapOffsetDelta: {x: 0, y: 0}
      snapCorner: 0
      id: Scene View/Cloth Collisions
      index: 2
      layout: 4
      size: {x: 0, y: 0}
      sizeOverriden: 0
    - dockPosition: 1
      containerId: overlay-container--right
      floating: 0
      collapsed: 0
      displayed: 0
      snapOffset: {x: 0, y: 0}
      snapOffsetDelta: {x: 0, y: 0}
      snapCorner: 0
      id: Scene View/Navmesh Display
      index: 4
      layout: 4
      size: {x: 0, y: 0}
      sizeOverriden: 0
    - dockPosition: 1
      containerId: overlay-container--right
      floating: 0
      collapsed: 0
      displayed: 0
      snapOffset: {x: 0, y: 0}
      snapOffsetDelta: {x: 0, y: 0}
      snapCorner: 0
      id: Scene View/Agent Display
      index: 5
      layout: 4
      size: {x: 0, y: 0}
      sizeOverriden: 0
    - dockPosition: 1
      containerId: overlay-container--right
      floating: 0
      collapsed: 0
      displayed: 0
      snapOffset: {x: 0, y: 0}
      snapOffsetDelta: {x: 0, y: 0}
      snapCorner: 0
      id: Scene View/Obstacle Display
      index: 6
      layout: 4
      size: {x: 0, y: 0}
      sizeOverriden: 0
    - dockPosition: 1
      containerId: overlay-container--right
      floating: 0
      collapsed: 0
      displayed: 0
      snapOffset: {x: 0, y: 0}
      snapOffsetDelta: {x: 0, y: 0}
      snapCorner: 0
      id: Scene View/Occlusion Culling
      index: 3
      layout: 4
      size: {x: 0, y: 0}
      sizeOverriden: 0
    - dockPosition: 1
      containerId: overlay-container--right
      floating: 0
      collapsed: 0
      displayed: 0
      snapOffset: {x: 0, y: 0}
      snapOffsetDelta: {x: 0, y: 0}
      snapCorner: 0
      id: Scene View/Physics Debugger
      index: 4
      layout: 4
      size: {x: 0, y: 0}
      sizeOverriden: 0
    - dockPosition: 1
      containerId: overlay-container--right
      floating: 0
      collapsed: 0
      displayed: 0
      snapOffset: {x: 0, y: 0}
      snapOffsetDelta: {x: 0, y: 0}
      snapCorner: 0
      id: Scene View/Scene Visibility
      index: 5
      layout: 4
      size: {x: 0, y: 0}
      sizeOverriden: 0
    - dockPosition: 1
      containerId: overlay-container--right
      floating: 0
      collapsed: 0
      displayed: 0
      snapOffset: {x: 0, y: 0}
      snapOffsetDelta: {x: 0, y: 0}
      snapCorner: 0
      id: Scene View/Particles
      index: 6
      layout: 4
      size: {x: 0, y: 0}
      sizeOverriden: 0
    - dockPosition: 1
      containerId: overlay-container--right
      floating: 0
      collapsed: 0
      displayed: 0
      snapOffset: {x: 0, y: 0}
      snapOffsetDelta: {x: 0, y: 0}
      snapCorner: 0
      id: Scene View/Tilemap
      index: 11
      layout: 4
      size: {x: 0, y: 0}
      sizeOverriden: 0
    - dockPosition: 1
      containerId: overlay-container--right
      floating: 0
      collapsed: 0
      displayed: 0
      snapOffset: {x: 0, y: 0}
      snapOffsetDelta: {x: 0, y: 0}
      snapCorner: 0
      id: Scene View/Tilemap Palette Helper
      index: 12
      layout: 4
      size: {x: 0, y: 0}
      sizeOverriden: 0
    - dockPosition: 1
      containerId: overlay-container--right
      floating: 0
      collapsed: 0
      displayed: 1
      snapOffset: {x: 48, y: 10}
      snapOffsetDelta: {x: 0, y: 0}
      snapCorner: 0
      id: UnityEditor.SceneViewCameraOverlay
      index: 9
      layout: 4
      size: {x: 0, y: 0}
      sizeOverriden: 0
    - dockPosition: 1
      containerId: overlay-container--right
      floating: 0
      collapsed: 0
      displayed: 0
      snapOffset: {x: 48, y: 48}
      snapOffsetDelta: {x: 0, y: 0}
      snapCorner: 0
      id: APV Overlay
      index: 7
      layout: 4
      size: {x: 0, y: 0}
      sizeOverriden: 0
    - dockPosition: 1
      containerId: overlay-container--right
      floating: 0
      collapsed: 0
      displayed: 0
      snapOffset: {x: 48, y: 48}
      snapOffsetDelta: {x: 0, y: 0}
      snapCorner: 0
      id: Scene View/TrailRenderer
      index: 7
      layout: 4
      size: {x: 0, y: 0}
      sizeOverriden: 0
    - dockPosition: 1
      containerId: overlay-container--right
      floating: 0
      collapsed: 0
      displayed: 1
      snapOffset: {x: -220, y: -232}
      snapOffsetDelta: {x: 0, y: 0}
      snapCorner: 3
      id: AINavigationOverlay
      index: 8
      layout: 4
      size: {x: 0, y: 0}
      sizeOverriden: 0
    - dockPosition: 1
      containerId: overlay-container--right
      floating: 0
      collapsed: 0
      displayed: 0
      snapOffset: {x: 48, y: 48}
      snapOffsetDelta: {x: 0, y: 0}
      snapCorner: 0
      id: unity-spline-inspector
      index: 9
      layout: 4
      size: {x: 0, y: 0}
      sizeOverriden: 0
    m_OverlaysVisible: 1
  m_WindowGUID: f48192587447b7b4ca9b7a5935333dd6
  m_Gizmos: 1
  m_OverrideSceneCullingMask: 6917529027641081856
  m_SceneIsLit: 1
  m_SceneLighting: 1
  m_2DMode: 0
  m_isRotationLocked: 0
  m_PlayAudio: 0
  m_AudioPlay: 0
  m_Position:
    m_Target: {x: 0, y: 1.3333335, z: 0}
    speed: 2
    m_Value: {x: 0, y: 1.3333335, z: 0}
  m_RenderMode: 0
  m_CameraMode:
    drawMode: 0
    name: Shaded
    section: Shading Mode
  m_ValidateTrueMetals: 0
  m_DoValidateTrueMetals: 0
  m_SceneViewState:
    m_AlwaysRefresh: 0
    showFog: 1
    showSkybox: 1
    showFlares: 1
    showImageEffects: 1
    showParticleSystems: 1
    showVisualEffectGraphs: 1
    m_FxEnabled: 1
  m_Grid:
    xGrid:
      m_Fade:
        m_Target: 0
        speed: 2
        m_Value: 0
      m_Color: {r: 0.5, g: 0.5, b: 0.5, a: 0.4}
      m_Pivot: {x: 0, y: 0, z: 0}
      m_Size: {x: 1, y: 1}
    yGrid:
      m_Fade:
        m_Target: 0
        speed: 2
        m_Value: 0
      m_Color: {r: 0.5, g: 0.5, b: 0.5, a: 0.4}
      m_Pivot: {x: 0, y: 0, z: 0}
      m_Size: {x: 1, y: 1}
    zGrid:
      m_Fade:
        m_Target: 0
        speed: 2
        m_Value: 0
      m_Color: {r: 0.5, g: 0.5, b: 0.5, a: 0.4}
      m_Pivot: {x: 0, y: 0, z: 0}
      m_Size: {x: 1, y: 1}
    m_ShowGrid: 0
    m_GridAxis: 1
    m_gridOpacity: 0.5
  m_Rotation:
    m_Target: {x: 0.7071068, y: 0, z: 0, w: 0.7071068}
    speed: 2
    m_Value: {x: -0.7071068, y: 0, z: -0, w: -0.7071068}
  m_Size:
    m_Target: 28.94185
    speed: 2
    m_Value: 28.94185
  m_Ortho:
    m_Target: 1
    speed: 2
    m_Value: 1
  m_CameraSettings:
    m_Speed: 0.8855999
    m_SpeedNormalized: 0.43999997
    m_SpeedMin: 0.01
    m_SpeedMax: 2
    m_EasingEnabled: 1
    m_EasingDuration: 0.4
    m_AccelerationEnabled: 1
    m_FieldOfViewHorizontalOrVertical: 60
    m_NearClip: 0.03
    m_FarClip: 10000
    m_DynamicClip: 1
    m_OcclusionCulling: 0
  m_LastSceneViewRotation: {x: 0, y: 0, z: 0, w: 0}
  m_LastSceneViewOrtho: 0
  m_ReplacementShader: {fileID: 0}
  m_ReplacementString: 
  m_SceneVisActive: 1
  m_LastLockedObject: {fileID: 0}
  m_ViewIsLockedToObject: 0
--- !u!114 &16
MonoBehaviour:
  m_ObjectHideFlags: 52
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 12914, guid: 0000000000000000e000000000000000, type: 0}
  m_Name: 
  m_EditorClassIdentifier: 
  m_MinSize: {x: 100, y: 100}
  m_MaxSize: {x: 4000, y: 4000}
  m_TitleContent:
    m_Text: Animator
    m_Image: {fileID: -1673928668082335149, guid: 0000000000000000d000000000000000, type: 0}
    m_Tooltip: 
  m_Pos:
    serializedVersion: 2
    x: 546
    y: 73
    width: 1466
    height: 763
  m_SerializedDataModeController:
    m_DataMode: 0
    m_PreferredDataMode: 0
    m_SupportedDataModes: 
    isAutomatic: 1
  m_ViewDataDictionary: {fileID: 0}
  m_OverlayCanvas:
    m_LastAppliedPresetName: Default
    m_SaveData: []
    m_OverlaysVisible: 1
  m_ViewTransforms:
    m_KeySerializationHelper:
    - {fileID: 3721131351775251420, guid: fbe27581ff6b8324a95c01cc3d2d2548, type: 2}
    m_ValueSerializationHelper:
    - e00: 1
      e01: 0
      e02: 0
      e03: 0
      e10: 0
      e11: 1
      e12: 0
      e13: 0
      e20: 0
      e21: 0
      e22: 1
      e23: 0
      e30: 0
      e31: 0
      e32: 0
      e33: 1
  m_PreviewAnimator: {fileID: 0}
  m_AnimatorController: {fileID: 9100000, guid: fbe27581ff6b8324a95c01cc3d2d2548, type: 2}
  m_BreadCrumbs:
  - m_Target: {fileID: 2390719094264726621, guid: fbe27581ff6b8324a95c01cc3d2d2548, type: 2}
    m_ScrollPosition: {x: 0, y: 0}
  - m_Target: {fileID: 3721131351775251420, guid: fbe27581ff6b8324a95c01cc3d2d2548, type: 2}
    m_ScrollPosition: {x: 0, y: 0}
  stateMachineGraph: {fileID: 0}
  stateMachineGraphGUI: {fileID: 0}
  blendTreeGraph: {fileID: 0}
  blendTreeGraphGUI: {fileID: 0}
  m_AutoLiveLink: 1
  m_MiniTool: 0
  m_LockTracker:
    m_IsLocked: 0
  m_CurrentEditor: 1
  m_LayerEditor:
    m_SelectedLayerIndex: 0
--- !u!114 &17
MonoBehaviour:
  m_ObjectHideFlags: 52
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 1
  m_Script: {fileID: 12014, guid: 0000000000000000e000000000000000, type: 0}
  m_Name: 
  m_EditorClassIdentifier: 
  m_MinSize: {x: 230, y: 250}
  m_MaxSize: {x: 10000, y: 10000}
  m_TitleContent:
    m_Text: Project
    m_Image: {fileID: -5467254957812901981, guid: 0000000000000000d000000000000000, type: 0}
    m_Tooltip: 
  m_Pos:
    serializedVersion: 2
    x: 6
    y: 699
    width: 1504
    height: 258
  m_SerializedDataModeController:
    m_DataMode: 0
    m_PreferredDataMode: 0
    m_SupportedDataModes: 
    isAutomatic: 1
  m_ViewDataDictionary: {fileID: 0}
  m_OverlayCanvas:
    m_LastAppliedPresetName: Default
    m_SaveData: []
    m_OverlaysVisible: 1
  m_SearchFilter:
    m_NameFilter: 
    m_ClassNames: []
    m_AssetLabels: []
    m_AssetBundleNames: []
    m_ReferencingInstanceIDs: 
    m_SceneHandles: 
    m_ShowAllHits: 0
    m_SkipHidden: 0
    m_SearchArea: 1
    m_Folders:
    - Assets/Prefab
    m_Globs: []
    m_OriginalText: 
    m_ImportLogFlags: 0
    m_FilterByTypeIntersection: 0
  m_ViewMode: 1
  m_StartGridSize: 64
  m_LastFolders:
  - Assets/Prefab
  m_LastFoldersGridSize: -1
  m_LastProjectPath: /mnt/IgniteDrive/Proyectos_Unity/SistemaLogistico_AMR
  m_LockTracker:
    m_IsLocked: 0
  m_FolderTreeState:
    scrollPos: {x: 0, y: 51}
    m_SelectedIDs: ac630000
    m_LastClickedID: 25516
    m_ExpandedIDs: 000000006651000000ca9a3bffffff7f
    m_RenameOverlay:
      m_UserAcceptedRename: 0
      m_Name: 
      m_OriginalName: 
      m_EditFieldRect:
        serializedVersion: 2
        x: 0
        y: 0
        width: 0
        height: 0
      m_UserData: 0
      m_IsWaitingForDelay: 0
      m_IsRenaming: 0
      m_OriginalEventType: 11
      m_IsRenamingFilename: 1
      m_ClientGUIView: {fileID: 0}
    m_SearchString: 
    m_CreateAssetUtility:
      m_EndAction: {fileID: 0}
      m_InstanceID: 0
      m_Path: 
      m_Icon: {fileID: 0}
      m_ResourceFile: 
  m_AssetTreeState:
    scrollPos: {x: 0, y: 0}
    m_SelectedIDs: 
    m_LastClickedID: 0
    m_ExpandedIDs: 000000006651000000ca9a3bffffff7f
    m_RenameOverlay:
      m_UserAcceptedRename: 0
      m_Name: 
      m_OriginalName: 
      m_EditFieldRect:
        serializedVersion: 2
        x: 0
        y: 0
        width: 0
        height: 0
      m_UserData: 0
      m_IsWaitingForDelay: 0
      m_IsRenaming: 0
      m_OriginalEventType: 11
      m_IsRenamingFilename: 1
      m_ClientGUIView: {fileID: 0}
    m_SearchString: 
    m_CreateAssetUtility:
      m_EndAction: {fileID: 0}
      m_InstanceID: 0
      m_Path: 
      m_Icon: {fileID: 0}
      m_ResourceFile: 
  m_ListAreaState:
    m_SelectedInstanceIDs: 
    m_LastClickedInstanceID: 0
    m_HadKeyboardFocusLastEvent: 0
    m_ExpandedInstanceIDs: c623000022450000985100002c520000a4510000744f0000e6540000ec54000050740000d2770000f080000032810000f07e000068020000067c00006c02000000000000025400002c510000
    m_RenameOverlay:
      m_UserAcceptedRename: 0
      m_Name: 
      m_OriginalName: 
      m_EditFieldRect:
        serializedVersion: 2
        x: 0
        y: 0
        width: 0
        height: 0
      m_UserData: 0
      m_IsWaitingForDelay: 0
      m_IsRenaming: 0
      m_OriginalEventType: 11
      m_IsRenamingFilename: 1
      m_ClientGUIView: {fileID: 0}
    m_CreateAssetUtility:
      m_EndAction: {fileID: 0}
      m_InstanceID: 0
      m_Path: 
      m_Icon: {fileID: 0}
      m_ResourceFile: 
    m_NewAssetIndexInList: -1
    m_ScrollPosition: {x: 0, y: 0}
    m_GridSize: 64
  m_SkipHiddenPackages: 0
  m_DirectoriesAreaWidth: 207
--- !u!114 &18
MonoBehaviour:
  m_ObjectHideFlags: 52
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 1
  m_Script: {fileID: 12003, guid: 0000000000000000e000000000000000, type: 0}
  m_Name: 
  m_EditorClassIdentifier: 
  m_MinSize: {x: 100, y: 100}
  m_MaxSize: {x: 4000, y: 4000}
  m_TitleContent:
    m_Text: Console
    m_Image: {fileID: -4327648978806127646, guid: 0000000000000000d000000000000000, type: 0}
    m_Tooltip: 
  m_Pos:
    serializedVersion: 2
    x: 6
    y: 699
    width: 1504
    height: 258
  m_SerializedDataModeController:
    m_DataMode: 0
    m_PreferredDataMode: 0
    m_SupportedDataModes: 
    isAutomatic: 1
  m_ViewDataDictionary: {fileID: 0}
  m_OverlayCanvas:
    m_LastAppliedPresetName: Default
    m_SaveData: []
    m_OverlaysVisible: 1
--- !u!114 &19
MonoBehaviour:
  m_ObjectHideFlags: 52
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 1
  m_Script: {fileID: 12019, guid: 0000000000000000e000000000000000, type: 0}
  m_Name: 
  m_EditorClassIdentifier: 
  m_MinSize: {x: 275, y: 50}
  m_MaxSize: {x: 4000, y: 4000}
  m_TitleContent:
    m_Text: Inspector
    m_Image: {fileID: -2667387946076563598, guid: 0000000000000000d000000000000000, type: 0}
    m_Tooltip: 
  m_Pos:
    serializedVersion: 2
    x: 1511
    y: 119
    width: 402
    height: 838
  m_SerializedDataModeController:
    m_DataMode: 0
    m_PreferredDataMode: 0
    m_SupportedDataModes: 
    isAutomatic: 1
  m_ViewDataDictionary: {fileID: 0}
  m_OverlayCanvas:
    m_LastAppliedPresetName: Default
    m_SaveData: []
    m_OverlaysVisible: 1
  m_ObjectsLockedBeforeSerialization: []
  m_InstanceIDsLockedBeforeSerialization: 
  m_PreviewResizer:
    m_CachedPref: -161
    m_ControlHash: -371814159
    m_PrefName: Preview_InspectorPreview
  m_LastInspectedObjectInstanceID: -1
  m_LastVerticalScrollValue: 0
  m_GlobalObjectId: 
  m_InspectorMode: 0
  m_LockTracker:
    m_IsLocked: 0
  m_PreviewWindow: {fileID: 0}
</file>

</files>
````

## File: .agents/agents/Arquitecto_Modulo4_MVP.md
````markdown
---
name: Arquitecto_Modulo4_MVP
description: Tech Lead y Arquitecto de Software para el Módulo 4 (Bodega Inteligente - MVP 20%). Especialista en FSM, NavMesh, y seguridad MCP.
mainAgent: true
subagent: true
permissionMode: acceptEdits
commandExecutionPolicy: auto
tools:
  - execute_code
  - request_recompile
  - wait_for_compilation
  - get_compilation_errors
  - get_scene_info
  - get_hierarchy
  - get_console_logs
  - read_file
  - write_file
  - patch_script
  - find_assets
skills:
  - unity-mcp-workflow
  - unity-ui-composition
---

# Rol y Propósito Estratégico
Eres el Senior Unity Tech Lead del proyecto "Entorno Interactivo de Automatización" (Unity 2022.3 LTS). Tu jurisdicción es el Módulo 4 (Bodegaje Inteligente) y su integración con el Módulo 5 (Despacho Táctico). Tu objetivo es gobernar el 20% funcional del MVP, garantizando una arquitectura industrial altamente optimizada, modular y resistente a fallos.

# Pilar 1: Guardián del Git Flow
Eres el supervisor del control de versiones. Antes de inyectar código masivo o alterar la arquitectura:
- Debes auditar verbalmente que el usuario está trabajando en una rama de desarrollo segura (`feat/` o `fix/`).
- Si el usuario te pide programar en `main`, debes detenerte, advertirle del riesgo y ordenarle crear una rama antes de proceder.

# Pilar 2: Arquitectura Modular (Los 3 Actores)
El sistema opera bajo un ecosistema de roles estrictos. Prohibido mezclar responsabilidades:
1. **Robot Manager (EVE):** Es el orquestador absoluto (WMS). Se mueve de forma rígida mediante un arreglo matemático de `Waypoints` aéreos (usando `Vector3.MoveTowards`). NUNCA calculará colisiones ni usará NavMesh. Lee la base de datos y despacha misiones secuencialmente.
2. **Robot Transportador (AMR):** Es el músculo terrestre. Opera bajo una Máquina de Estados Finitos (FSM) estricta: `Inactivo`, `En_Transito`, `Extrayendo`, `Transportando`, `Interrumpido`, `Entregando`. 
3. **Robot Cargador:** Elemento de "Lore". Representa la matriz LIFO (Last In, First Out) en el camión.

# Pilar 3: Movimiento Rígido (Cero Libre Albedrío)
La eficiencia algorítmica es tu prioridad. 
- **NavMesh Obligatorio:** El AMR terrestre utiliza exclusivamente `NavMeshAgent` para navegar por la "carretera de luz". 
- **Navegación Dinámica sin Triggers:** Prohibido sugerir colisionadores invisibles (`OnTriggerEnter`) para que el AMR detecte su llegada. El AMR debe calcular su proximidad usando `agente.remainingDistance <= 1.0f`.
- **Zonas de Exclusión:** No uses IA de evasión ni físicas de empuje (`Rigidbody` forces).

# Pilar 4: Desacoplamiento de Datos (Data-Driven)
Toda misión táctica proviene del exterior. 
- Prohibido "quemar" (hardcodear) identificadores, coordenadas (ej. "P2-C4-F1") o pesos de pallets dentro de los scripts de C#. 
- Toda lectura debe hacerse consumiendo `manifiesto.json` en la ruta protegida `Application.streamingAssetsPath` mediante `JsonUtility`.

# Pilar 5: El Contrato al 110% (Incertidumbre y UI)
El sistema debe predecir la falla logística. 
- Si EVE asigna un pallet superior a 900 kg (anomalía de batería), el AMR debe cambiar su estado a `Interrumpido` y frenar el NavMeshAgent.
- Debes asistir en la conexión de esta interrupción con el `DashboardUI` (Singleton), mostrando un panel de advertencia crítica y habilitando un "Override" manual para que el usuario fuerce la batería al 0% y salve el contrato.

# Protocolo de Inyección Segura (MCP)
- Utiliza **siempre** la herramienta `execute_code` implementando la plantilla `IFunplayCommand`.
- Toda creación, modificación o destrucción de GameObjects debe registrarse con `ctx.RegisterObjectCreation`, `ctx.RegisterObjectModification` y `ctx.DestroyObject` para mantener intacto el sistema de Undo (Ctrl+Z) del usuario.
- Tras alterar scripts, es OBLIGATORIO ejecutar `request_recompile` seguido de `wait_for_compilation`. Verifica los resultados con `get_compilation_errors` antes de dar una tarea por terminada.
````

## File: Assets/Scripts/GeneradorTablaManifiesto.cs
````csharp
using UnityEngine;
using TMPro; // Necesario para TextMeshPro
using System.Collections.Generic;

public class GeneradorTablaManifiesto : MonoBehaviour
{
    [Header("Configuración de UI")]
    [SerializeField] private Transform contenedorFilas; // Aquí arrastrarás tu panel 'Manifiesto'
    [SerializeField] private GameObject filaPrefab;     // Aquí arrastrarás tu Prefab azul

    // Función pública que llamaremos cuando el JSON termine de leerse
    public void PoblarTabla(List<Pedido> pedidosJSON)
    {
        // 1. Limpieza de seguridad: Borrar filas viejas si el JSON se recarga
        // (Empezamos desde el índice 1 para no borrar la Fila_Titulos que está en el índice 0)
        for (int i = 1; i < contenedorFilas.childCount; i++)
        {
            Destroy(contenedorFilas.GetChild(i).gameObject);
        }

        // 2. Crear una fila nueva por cada pedido en el JSON
        foreach (Pedido pedido in pedidosJSON)
        {
            // Clonamos el prefab dentro del contenedor
            GameObject nuevaFila = Instantiate(filaPrefab, contenedorFilas);

            // Obtenemos los 5 textos de esa nueva fila
            TextMeshProUGUI[] columnas = nuevaFila.GetComponentsInChildren<TextMeshProUGUI>();

            // Asignamos la información del JSON a cada columna respectiva
            columnas[0].text = pedido.id_pallet;
            columnas[1].text = pedido.nombre_producto; // Asegúrate de que esta variable exista en tu clase Pedido
            columnas[2].text = pedido.codigo_sku;
            columnas[3].text = pedido.peso_kg.ToString() + " kg";
            
            // Lógica visual para el estado
            columnas[4].text = "PENDIENTE";
            columnas[4].color = Color.red; // Resalta en rojo para indicar que el robot aún no lo mueve
        }
    }
}
````

## File: UserSettings/Search.settings
````
{}
````

## File: .gitignore
````
# This .gitignore file should be placed at the root of your Unity project directory
#
# Get latest from https://github.com/github/gitignore/blob/master/Unity.gitignore
#
/[Ll]ibrary/
/[Tt]emp/
/[Oo]bj/
/[Bb]uild/
/[Bb]uilds/
/[Ll]ogs/
/[Mm]emoryCaptures/

# Never ignore Asset meta data
!/[Aa]ssets/**/*.meta

# Uncomment this line if you wish to ignore the asset store tools plugin
# /[Aa]ssets/AssetStoreTools*

# TextMesh Pro files
[Aa]ssets/TextMesh*Pro/

# Autogenerated Jetbrains Rider plugin
[Aa]ssets/Plugins/Editor/JetBrains*

# Visual Studio cache directory
.vs/

# Gradle cache directory
.gradle/

# Autogenerated VS/MD/Consulo solution and project files
ExportedObj/
.consulo/
*.csproj
*.unityproj
*.sln
*.suo
*.tmp
*.user
*.userprefs
*.pidb
*.booproj
*.svd
*.pdb
*.mdb
*.opendb
*.VC.db

# Unity3D generated meta files
*.pidb.meta
*.pdb.meta
*.mdb.meta

# Unity3D generated file on crash reports
sysinfo.txt

# Builds
*.apk
*.unitypackage

# Crashlytics generated file
crashlytics-build.properties
````

## File: README.md
````markdown
# SistemaLogistico_AMR
````

## File: .agents/skills/funplay-unity-mcp-workflow/SKILL.md
````markdown
---
name: funplay-unity-mcp-workflow
description: Efficient workflow for using Unity MCP to edit, import, compile, inspect, and test Unity projects, including screenshot and Game View recording verification.
---
<!-- Funplay Unity MCP managed project skills -->
<!-- Funplay Unity MCP skill version: unity-mcp-workflow@1.0.5 -->

# Unity MCP Workflow

Use this skill when Codex or another AI agent is working in a Unity project and needs to verify code, prefabs, UI, Play Mode behavior, screenshots, Game View recordings, scene hierarchy, console logs, domain reloads, or MCP connection issues.

## MCP-First Unity UI Operations

- Do not use computer use (desktop mouse/keyboard automation) to operate Unity unless necessary. When assembling, modifying, inspecting or validating UI, prefer Unity MCP whenever it can complete the step, including hierarchy/component/prefab reads and edits, compilation/Play state, clicks/scrolling, screenshots and recordings.
- Check the connected project's tools/list and, when available, `get_tool_capabilities`. A tool missing from exposure, compilation/domain reload or a temporary disconnection is not evidence of a missing capability: check exposure/readiness and recover status first. Respect custom allowlists; do not widen exposure or use another interaction method to bypass restrictions.
- Prefer specialized MCP tools; for project-specific gaps they do not cover, use a permitted, guarded `execute_code` call through Unity Editor APIs when it can perform the step reliably. Computer use is a fallback only for a confirmed MCP capability gap, or an explicit user request: explain the uncovered step before using it, limit it to that step, and return to MCP readback/validation when available. If recovery fails, report the connection blocker rather than silently switching methods or repeating uncertain mutations.
- This routing applies to operating Unity, not ordinary source-file editing or viewing supplied design references and already-captured images/videos with appropriate file or media tools.


## Operating Loop

1. Establish context.
   - Confirm the Unity project root and active scene.
   - Check that Unity MCP is reachable before assuming Editor state.
   - Inspect hierarchy, prefab paths, selected objects, and relevant component references through MCP.
   - If the user names an object, treat the name as a hint and verify the real Unity object path before editing.
2. Choose the edit surface.
   - Edit source files with normal repo tools, then trigger Unity recompilation.
   - Edit scene objects through Unity APIs, mark the scene dirty, and save the scene.
   - Edit prefab fields with `set_prefab_property(ies)` when available. Use `PrefabUtility.LoadPrefabContents`, `SaveAsPrefabAsset`, and `UnloadPrefabContents` for structural changes.
   - Unless the user explicitly requests a full rebuild, preserve the existing hierarchy when editing UI or GameObject prefabs and modify only the required objects, components, and serialized fields; do not recreate the entire prefab.
   - Edit ScriptableObject assets through `SerializedObject`, `EditorUtility.SetDirty`, and `AssetDatabase.SaveAssetIfDirty` / `SaveAssets`.
   - Never patch `.unity`, `.prefab`, or `.asset` YAML with shell text tools.
   - If the user is looking at an open scene instance, update the visible scene instance as well as the prefab asset when appropriate.
3. Execute changes.
   - Prefer structured query/edit/audit tools for supported work. Use a guarded `execute_code` batch only for project-specific gaps, after resolving full type names and assemblies when needed.
   - Use null guards for every object, component, asset, and path lookup.
   - Return explicit missing-path/object/component messages that include the expected path and the scene or prefab searched.
   - Return concise before/after values from snippets.
   - Save only the assets or scenes intentionally modified.
   - Do not run self-healing fallback loops; if a reference, path, package, or tool is missing, report it once and stop or skip that item.
4. Validate.
   - Read back the changed objects through MCP.
   - For code or resource edits, use `prepare_editor` and poll its durable operation ID to verified readiness in the intended mode, then inspect compilation errors and console errors.
   - For runtime behavior, enter Play Mode or inspect live objects when needed.
   - If MCP is unreachable, do not claim scene, prefab, asset, or runtime verification.
   - Report exactly what was verified and what still requires device, store, network, or manual validation.

## Unity Serialized Asset Safety

- Do not use shell text tools, scripts, or patches to modify `.unity`, `.prefab`, or `.asset` files. These are Unity-owned serialized assets; changing them outside Unity can corrupt file IDs, prefab overrides, references, import state, or scene dirtiness.
- Shell tools may inspect or locate serialized Unity assets, but scene, prefab, and ScriptableObject modifications must go through Unity MCP tools or Editor APIs.
- For scenes, modify live objects through Unity APIs, mark only the touched scene dirty, and save that scene.
- For prefabs, use Prefab Mode tools or `PrefabUtility.LoadPrefabContents` / `SaveAsPrefabAsset` / `UnloadPrefabContents`.
- For ScriptableObjects or other `.asset` files, load the asset with `AssetDatabase`, modify serialized properties through `SerializedObject` when possible, mark that asset dirty, and save only that asset.
- If Unity readback and raw file text disagree, trust Unity readback and investigate the asset path instead of hand-editing YAML.

## Tool Exposure

- With default `core` exposure, prefer structured inspection/editing, audits, durable preparation, unified task reads, project-aware UI creation and visual evidence. `execute_code` remains a fallback for project-specific gaps.
- `full` retains legacy status and compile/Play tools, history, project-default configuration, preview management, explicit recording markers and specialized diagnostics. Check exposure before choosing those workflows; do not silently widen a custom list.
- Use `get_tool_capabilities` to distinguish implemented/enabled/exposed tools. Respect customized allowlists; report missing exposure rather than claiming an implementation does not exist.

## MCP Call Pattern

If native MCP tools are not directly available, probe the local HTTP endpoint. The port is
per project, so read it from the Funplay MCP Server window (it is also the port in the
configured client entry) instead of assuming a fixed one:

```bash
PORT=24312 # replace with the port shown in the Funplay MCP Server window
curl -sS -m 1 -X POST http://127.0.0.1:$PORT/mcp \
  -H 'Content-Type: application/json' \
  -d '{"jsonrpc":"2.0","id":1,"method":"tools/list"}'
```

For multi-line `execute_code` calls over curl, generate JSON with a real encoder instead of hand-escaping C#:

```bash
node - <<'NODE'
const code = String.raw`
using UnityEngine;

public class InspectSomething
{
    public static string Run()
    {
        var obj = GameObject.Find("PracticeInGameUiRoot");
        return obj != null ? obj.name : "not found";
    }
}
`;
const payload = {
  jsonrpc: "2.0",
  id: 1,
  method: "tools/call",
  params: { name: "execute_code", arguments: { code } }
};
process.stdout.write(JSON.stringify(payload));
NODE
```

## Recommended `execute_code` Template

For non-trivial snippets, prefer `IFunplayCommand` over the legacy `public static string Run()` template. `execute_code` auto-adds `using Funplay.Editor.Tools.Scripting;` when `IFunplayCommand` is used, but include it explicitly in generated snippets for readability:

```csharp
using Funplay.Editor.Tools.Scripting;
using UnityEngine;

public class CommandScript : IFunplayCommand
{
    public void Execute(ExecutionContext ctx)
    {
        var root = GameObject.Find("PracticeInGameUiRoot");
        if (root == null)
        {
            ctx.LogWarning("PracticeInGameUiRoot not found");
            ctx.ReturnValue = "missing root";
            return;
        }

        ctx.RegisterObjectModification(root);
        ctx.Log("Found {0}, active={1}", root.name, root.activeInHierarchy);
        ctx.ReturnValue = new
        {
            name = root.name,
            active = root.activeInHierarchy
        };
    }
}
```

Use `ctx.RegisterObjectCreation(obj)`, `ctx.RegisterObjectModification(obj)`, and `ctx.DestroyObject(obj)` instead of direct Undo calls when possible. Use `ctx.Log`, `ctx.LogWarning`, and `ctx.LogError` for output returned in the MCP response without polluting the Unity Console.

## Unity C# Patterns

Add explicit `using` directives or use fully qualified types for project code. `execute_code` does not auto-inject project namespaces by default:

```csharp
var root = UnityEngine.GameObject.Find("PracticeInGameUiRoot");
var rect = root.GetComponent<UnityEngine.RectTransform>();
```

Use Unity null semantics for `UnityEngine.Object` references:

```csharp
if (image == null)
{
    return "Image missing";
}
```

Do not use `??=` to lazily resolve or rebind `UnityEngine.Object` references. Unity's destroyed or unbound serialized references can be fake-null: `field == null` returns true through Unity's overloaded operator, while C# `??=` can still treat the managed wrapper as non-null and skip the fallback assignment. Use an explicit Unity-null check instead:

```csharp
if (_hud == null)
{
    _hud = GetComponentInChildren<MyHud>(true);
}
```

For prefab edits:

```csharp
var path = "Assets/MyGame/UI/Prefabs/PF_PracticeInGameUiRoot.prefab";
var prefab = UnityEditor.PrefabUtility.LoadPrefabContents(path);
try
{
    var target = prefab.transform.Find("SafeArea/SwingCancelZone");
    if (target == null)
    {
        return "SwingCancelZone not found in prefab";
    }

    var rect = target.GetComponent<UnityEngine.RectTransform>();
    var before = rect.anchoredPosition;
    rect.anchoredPosition = new UnityEngine.Vector2(-76f, 448f);

    UnityEditor.EditorUtility.SetDirty(rect);
    UnityEditor.PrefabUtility.SaveAsPrefabAsset(prefab, path);
    UnityEditor.AssetDatabase.SaveAssets();
    return "Prefab saved: pos " + before + " -> " + rect.anchoredPosition;
}
finally
{
    UnityEditor.PrefabUtility.UnloadPrefabContents(prefab);
}
```

For scene edits:

```csharp
var obj = UnityEngine.GameObject.Find("PracticeInGameUiRoot/SafeArea/SwingCancelZone");
if (obj == null)
{
    return "Scene object not found";
}

var rect = obj.GetComponent<UnityEngine.RectTransform>();
var before = rect.sizeDelta;
UnityEditor.Undo.RecordObject(rect, "Update cancel zone");
rect.sizeDelta = new UnityEngine.Vector2(220f, 116f);
UnityEditor.EditorUtility.SetDirty(rect);
UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(obj.scene);
UnityEditor.SceneManagement.EditorSceneManager.SaveScene(obj.scene);
return "Scene saved: size " + before + " -> " + rect.sizeDelta;
```

## Recompile And Reload

After external C# or asset file edits:

1. Call `prepare_editor` with target=edit or play, refresh_assets=true and a unique request_key.
2. Retain data.task.task_id and use `get_task` with bounded waiting and the last revision. If HTTP drops during reload, reconnect and repeat the status read with that handle or kind=editor and the original request_key.
3. Continue only when operation.status=ready, current_editor.ready=true and current_editor.is_playing matches the intended mode.
4. Stop on failed/cancelled/interrupted status; inspect compiler errors, phase history, deadline and current state before deciding the next action.
5. Check console/runtime initialization separately. Preparation does not prove business logic, visual fidelity or event routing.

Do not replay an interrupted arbitrary mutation. Its outcome is unknown until exact object/asset readback establishes what executed.

If durable preparation is unavailable in an older/customized configuration, use the legacy exit/request_recompile/wait/error-check/enter sequence and explicitly read back state after reconnection. `get_reload_recovery_status` is historical information, not a readiness flag.

## Verification Checklist

Use structured component/property readback that returns exact values and persistence state. Resolve all candidates with `find_game_objects`, choose the verified ID, and query `get_component_properties`; never select the first duplicate name. Only use a guarded readback snippet when the structured API cannot express the project-specific check.

For UI work, verify prefab or scene hierarchy, sprite references, anchors, sorting order, active state, text fit, and button listeners. A populated `Content` hierarchy does not prove the user can see the UI.

For gameplay or network work, verify object identity, ownership, live instance existence, transform values, animation state, visibility, and whether client-side filters are discarding valid data.

## Failure Handling

- If MCP is unreachable, say so and fall back only to safe filesystem inspection or code edits. Do not claim scene, prefab, or runtime verification without Unity readback.
- If an object lookup fails, inspect hierarchy and prefab contents instead of inventing a path.
- If multiple matching objects exist, print their paths and choose the one matching the user-visible UI or current scene.
- If a reference, package, tool, or path is missing, return one clear error and stop or skip that item. Do not loop through guessed fallback paths, create replacement objects silently, or report success after a best-effort fallback.
- If compile errors appear after a change, fix them before Play Mode validation.
- When Unity and text files disagree for serialized scene or prefab state, trust Unity readback and inspect the asset path.

## Structured UI Workflows

- Short MCP tasks briefly wait for completion (wait_seconds defaults to 2; zero returns immediately). For longer tasks use `get_task(data.task.task_id, wait_seconds=20, after_revision=<last revision>)`; it waits for completion or a meaningful state change. Honor poll_after_ms on unchanged responses instead of making the model poll every second. wait_complete is not proof of success: inspect native status, errors and ready/complete/restoration fields. A read_timeout carries only the snapshot_at observation. Cancellation of the HTTP wait does not cancel the task. On lost preparation/preview responses recover through kind + the original request_key; do not replay mutations. Recording and Test Runner starts return immediately, as do preparation/preview starts without a recovery key.
- Inspect before modifying: use `find_game_objects` with component/property filters and projections, `inspect_ui_sprites` for Image/effective Sprite/importer/border/local-ID associations, and `find_project_types` for exact type and assembly names. Check ambiguity, partial errors, scan completeness and pagination; an incomplete scan is not proof of absence. Component setters distinguish live in-memory readback from saved/reimported prefab values.
- Run `audit_ui` on relevant live roots or saved prefabs/scenes; small scans can finish in one call, otherwise read status and finding pages through `get_task`. It checks missing borders, missing/required references, transparent raycast blockers, text/clipping and layout conflicts without fixing or saving assets. Review measured evidence and contextual warnings; suppress intentional exceptions only with an explicit project reason. Do not invent border values or infer design fidelity from a clean audit.
- Before creating new UI, read `get_ui_defaults`. `create_project_ui` can reuse templates and retain their prefab connection, label bindings, font/material and authored geometry. Explicit overrides take precedence; existing template component types are not converted. `configure_ui_defaults` changes project-scoped authoring preferences, so use it only when that shared convention is intended. Tied/incomplete convention scans or missing TMP resources require a deliberate choice/action, never a silent legacy fallback. This is Edit Mode authoring: save the intended scene/prefab explicitly and preserve existing UI when revising it.
- When preview management is exposed (Full by default), use `start_ui_preview_session` with verified prefab_paths and/or a project scene_template, optionally enter_play_mode and target width/height. It needs saved clean original scenes and no open Prefab Stage; do not save/discard unrelated user work merely to satisfy this precondition. Retain session_id and data.task.task_id; use `get_task` until ready. Business data and initialization remain project-specific; entering the scene may run lifecycle code.
- End the matching session with `end_ui_preview_session`, then inspect scenes_restored, view_restored, selection_restored, assets_cleaned and warnings. Do not claim full restoration from a success envelope. Changed scene setup, dirty preview or modified temporary scene requires inspection; discard_preview_changes applies only to the owned preview scene and must reflect an intended discard. Network/save-game effects and source asset edits are not rolled back. Preserve user-created files and changed window choices; report recovery still needed.
- Use screenshot `geometry`, not an unrelated `Screen` size: render size and returned image size can differ. Pass coordinate_space=image_pixels, origin=top_left and a fresh capture_id to click/drag/scroll or `raycast_at_point` when measuring a screenshot. `get_object_screen_bounds` and `get_visual_coordinates` share the mapping. Expired IDs or changed mode/view/scene/camera viewport/render dimensions require a fresh capture, not clamping or guessing. Geometry validity does not prove animated content stayed unchanged.


## Game View Recording

Use `capture_game_view` for static layout or a single visual state. Use `record_game_view` when the task needs evidence over time, such as animation, transitions, or a reproducible interaction sequence; do not record every routine UI edit.

1. Prepare. Use `prepare_editor` targeting play (or a ready preview session) and verify current readiness. Recording requires a graphics-enabled macOS or Windows Unity Editor with a visible, rendering Game tab. Keep that tab visible and its resolution unchanged throughout capture; hiding it or resizing the source can fail the recording. The MP4 includes overlay UI but no audio.
2. Start a short, bounded clip before performing the relevant actions. For example, call `record_game_view` with:

   ```json
   {"action":"start","duration_seconds":10,"fps":15,"max_dimension":1280}
   ```

   Save `data.recording_id` from the response, then perform the interaction. Start returns immediately; recording stops automatically at the duration limit. These are the default settings; accepted ranges are 1-120 seconds, 1-60 fps, and a 128-1920 pixel maximum edge. Aspect ratio is preserved without upscaling. Prefer a shorter clip or lower sampling rate/resolution if capture overhead is disruptive.
3. After performing the interactions, use `get_task` with the returned data.task.task_id for bounded status waits. To finish early, use `record_game_view` with `{"action":"stop","recording_id":"<returned id>"}`, then query the matching task if finalization is still pending. Legacy action=status remains compatible. Always pass the saved ID so a stale request cannot inspect or stop a newer recording. If another recording is already active, report it rather than stopping someone else's capture.
4. Check the receipt, not just `success`. While `data.status` is `recording` or `stopping`, the file is not ready. Read the MP4 only when `data.ready=true`; a `success=true` status query can still describe a failed recording. Stop polling on terminal `completed`, `interrupted`, or `failed` status and inspect `error`, `stop_reason`, and the actual captured extent (`frame_count`, `elapsed_seconds`, `last_frame_seconds`). Leaving Play Mode or reloading scripts finalizes early; recover the receipt after reload and treat any usable partial clip as partial evidence, not a complete test.
5. Review the actual file at `data.path`, under `<UnityProject>/Library/FunplayMcp/Recordings/`. MCP returns a local-file receipt, not video bytes or base64; the client must have access to that filesystem and a video viewer. A remote MCP connection alone does not provide file access. If video viewing is unavailable, inspect extracted frames when supported and state their limits, or report that the clip was saved but not reviewed. Do not claim to have watched an inaccessible clip or upload project footage without authorization.

- Report reproduction steps, clip path, observed result, and interruptions or unverified portions. Combine visual evidence with Unity state readback and console checks.
- Use `mark_recording` when exposed (Full by default) for named before/after project actions. Click, drag and `simulate_ui_scroll` tools record automatic markers in Core; a marker identifies dispatch, not proof that the intended behavior succeeded.
- Once ready, use `extract_recording_frames` with recording_id and 1..16 timestamps; if still pending use `get_task`, then inspect images with `get_recording_frame`. Check requested_seconds, actual_seconds and delta_seconds: it selects the first decoded frame at or after the request, not an exact-time guarantee. Markers after last_frame_seconds have no captured frame. Historical frame geometry is not valid for live input.
- Frame extraction can be cancelled; cleanup deletes only that job's generated PNGs, never the video. Preserve needed evidence before cleanup. Native decoder support depends on Unity version; interrupted/failed extraction and partial clips are not complete verification. Sparse frames cannot establish motion or timing between samples.
- Capture is best-effort with real elapsed timestamps, not guaranteed target-fps sampling. Use it for visual behavior, not frame-accurate performance measurement; use Profiler and device tests for performance.
- If the tool, platform, or rendering prerequisites are unavailable, report the limitation and use screenshots or state checks only for what they can establish. Do not loop on terminal failures or install recording dependencies merely to bypass the limitation.


## Metadata

- Original skill id: `unity-mcp-workflow`
- Skill version: `1.0.5`
- Platform: `antigravity`
- Source repository: `https://github.com/FunplayAI/funplay-unity-mcp`
````

## File: .agents/mcp_config.json
````json
{"mcpServers":{"funplay-sistemalogistico":{"serverUrl":"http://127.0.0.1:21442/"}}}
````

## File: Assets/Scripts/DashboardUI.cs
````csharp
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class DashboardUI : MonoBehaviour
{
    /*Al usar static, le estamos diciendo a Unity: "Esta variable no le pertenece a un objeto individual,
    le pertenece a la clase globalmente"
    Por convención mundial en C#, cuando se hace un Singleton, a la variable que guarda la copia única se le llama Instance.*/
    public static DashboardUI Instance { get; private set; }

    [Header("UI Referencias")]
    [Tooltip("Asigna aquí el TextMeshProUGUI. Si usas Text heredado, asígnalo abajo.")]
    [SerializeField] private Component textoLogTMP;
    [Tooltip("Asigna aquí el Text heredado si no usas TMPro.")]
    [SerializeField] private Text textoLogLegacy;

    private Queue<string> logQueue = new Queue<string>();
    private const int MaxLineas = 5;

    void Awake()
    {
        /*Aquí le estamos diciendo: "Si ya existe una instancia de esta clase en la escena,
         y no soy yo mismo, destrúyeme". Esto evita que Unity cree copias o duplicados.*/
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void RegistrarLog(string mensaje)
    {
        string timestamp = $"[{System.DateTime.Now:HH:mm:ss}]";
        string logFinal = $"{timestamp} {mensaje}";

        logQueue.Enqueue(logFinal);

        if (logQueue.Count > MaxLineas)
        {
            logQueue.Dequeue();
        }

        ActualizarVisual();
    }

    private void ActualizarVisual()
    {
        string contenido = string.Join("\n", logQueue);
        
        if (textoLogTMP != null)
        {
            var prop = textoLogTMP.GetType().GetProperty("text");
            if (prop != null)
            {
                prop.SetValue(textoLogTMP, contenido);
            }
        }
        else if (textoLogLegacy != null)
        {
            textoLogLegacy.text = contenido;
        }
    }
}
````

## File: UserSettings/FunplayMcpSettings.json
````json
{
    "settingsVersion": 1,
    "enabled": true,
    "port": 8765,
    "portConfigured": false,
    "toolExportProfile": "core",
    "coreToolsCustom": true,
    "coreTools": [
        "add_component",
        "add_component_to_many",
        "add_layer",
        "add_tag",
        "analyze_scene_complexity",
        "assign_animator",
        "assign_material",
        "bake_lightmaps",
        "cancel_test_run",
        "capture_editor_window",
        "capture_game_view",
        "capture_multiview",
        "capture_scene_view",
        "capture_simulator_view",
        "clear_execute_code_history",
        "close_prefab_stage",
        "copy_asset",
        "copy_component",
        "create_animation_clip",
        "create_animator_controller",
        "create_button",
        "create_canvas",
        "create_game_object",
        "create_image",
        "create_material",
        "create_new_scene",
        "create_prefab",
        "create_primitive",
        "create_script",
        "create_scriptable_object",
        "create_text",
        "delete_asset",
        "delete_game_object",
        "director_evaluate",
        "duplicate_game_object",
        "edit_script",
        "enter_play_mode",
        "execute_code",
        "execute_menu_item",
        "exists",
        "exit_play_mode",
        "find_assets",
        "find_broken_references",
        "find_game_objects",
        "find_references",
        "focus_on_object",
        "frame_debugger_disable",
        "frame_debugger_enable",
        "frame_debugger_get_events",
        "get_active_tool",
        "get_animator_state",
        "get_asset_import_settings",
        "get_build_settings",
        "get_camera_properties",
        "get_compilation_errors",
        "get_component_properties",
        "get_console_logs",
        "get_counters",
        "get_editor_state",
        "get_execute_code_history",
        "get_frame_timing",
        "get_game_object_info",
        "get_hierarchy",
        "get_layers",
        "get_lighting_settings",
        "get_material_properties",
        "get_mesh_info",
        "get_object_memory",
        "get_performance_snapshot",
        "get_prefab_stage",
        "get_project_settings",
        "get_reload_recovery_status",
        "get_scene_info",
        "get_scriptable_object",
        "get_selection",
        "get_tags",
        "get_test_job",
        "get_time_scale",
        "get_top_memory_objects",
        "get_undo_state",
        "get_windows",
        "install_package",
        "instantiate_prefab",
        "list_components",
        "list_directory",
        "list_dirty_scenes",
        "list_packages",
        "list_scenes",
        "load_scene_additive",
        "log_message",
        "memory_compare_snapshots",
        "memory_list_full_snapshots",
        "memory_list_snapshots",
        "memory_open_snapshot_in_profiler",
        "memory_query_references",
        "memory_query_top_objects",
        "memory_take_full_snapshot",
        "memory_take_snapshot",
        "open_prefab_stage",
        "open_scene",
        "particle_control",
        "paste_component_values",
        "patch_script",
        "physics2d_overlap_point",
        "physics_overlap",
        "physics_raycast",
        "ping_asset",
        "play_animator_state",
        "profiler_start",
        "profiler_status",
        "profiler_stop",
        "raycast_at_point",
        "read_file",
        "record_game_view",
        "redo",
        "remove_package",
        "remove_tag",
        "rename_asset",
        "rename_game_object",
        "replay_execute_code",
        "request_recompile",
        "run_tests",
        "save_all_scenes",
        "save_prefab_stage",
        "save_scene",
        "search_files",
        "select_object",
        "set_active",
        "set_active_tool",
        "set_animator_parameter",
        "set_asset_import_settings",
        "set_camera_culling_mask",
        "set_camera_projection",
        "set_camera_settings",
        "set_component_properties",
        "set_component_property",
        "set_lighting_settings",
        "set_material_property",
        "set_parent",
        "set_prefab_properties",
        "set_prefab_property",
        "set_scriptable_object_properties",
        "set_selection",
        "set_tag_and_layer",
        "set_time_scale",
        "set_transform",
        "show_dialog",
        "simulate_key_combo",
        "simulate_key_press",
        "simulate_mouse_click",
        "simulate_mouse_drag",
        "undo",
        "unload_scene",
        "unpack_prefab",
        "validate_menu_item",
        "wait_for_compilation",
        "write_file"
    ],
    "fullToolsCustom": false,
    "fullTools": [],
    "selectedConfigTarget": "Antigravity",
    "executeCodeSafetyChecksEnabled": true,
    "executeCodeSafetyChecksConfigured": true,
    "executeCodeStrictFilesystemSafetyEnabled": true,
    "executeCodeStrictFilesystemSafetyConfigured": true,
    "executeCodeProjectNamespaceInjectionEnabled": true,
    "executeCodeProjectNamespaceInjectionConfigured": true,
    "pluginDebugLoggingEnabled": false,
    "pluginDebugLoggingConfigured": true,
    "mcpRecentActivityExpandedByDefault": true,
    "mcpRecentActivityExpansionConfigured": true,
    "mcpBrokerModeEnabled": false,
    "mcpBrokerMonoPath": "",
    "mcpLastClientConfigKeys": [
        "Antigravity|funplay-sistemalogistico"
    ]
}
````

## File: .agents/skills/funplay-unity-ui-composition/SKILL.md
````markdown
---
name: funplay-unity-ui-composition
description: Build and revise responsive Unity uGUI mobile interfaces, including portrait and landscape layouts, safe areas, prefabs, auto layout, scrolling, text, input, animation, and performance validation.
---
<!-- Funplay Unity MCP managed project skills -->
<!-- Funplay Unity MCP skill version: unity-ui-composition@1.0.6 -->

# Unity UI Composition

Use this built-in skill when creating, assembling, adapting, reviewing, or fixing Canvas-based Unity UI, especially mobile screen or popup prefabs that must work across aspect ratios, notches, tablets, localization, and runtime state changes.

## MCP-First Unity UI Operations

- Do not use computer use (desktop mouse/keyboard automation) to operate Unity unless necessary. When assembling, modifying, inspecting or validating UI, prefer Unity MCP whenever it can complete the step, including hierarchy/component/prefab reads and edits, compilation/Play state, clicks/scrolling, screenshots and recordings.
- Check the connected project's tools/list and, when available, `get_tool_capabilities`. A tool missing from exposure, compilation/domain reload or a temporary disconnection is not evidence of a missing capability: check exposure/readiness and recover status first. Respect custom allowlists; do not widen exposure or use another interaction method to bypass restrictions.
- Prefer specialized MCP tools; for project-specific gaps they do not cover, use a permitted, guarded `execute_code` call through Unity Editor APIs when it can perform the step reliably. Computer use is a fallback only for a confirmed MCP capability gap, or an explicit user request: explain the uncovered step before using it, limit it to that step, and return to MCP readback/validation when available. If recovery fails, report the connection blocker rather than silently switching methods or repeating uncertain mutations.
- This routing applies to operating Unity, not ordinary source-file editing or viewing supplied design references and already-captured images/videos with appropriate file or media tools.

## Operating Loop

1. Inspect before editing.
   - Confirm the active scene, Canvas render mode, Canvas Scaler settings, EventSystem and input module, target orientations, design resolution, safe-area policy, and relevant prefab asset paths.
   - Inspect the existing hierarchy, anchors, pivots, offsets, layout controllers, sibling order, Canvas sorting, serialized references, animation targets, and Prefab overrides.
   - Inspect representative screens and prefabs to determine whether `UnityEngine.UI.Text` or `TextMeshProUGUI` is the project's prevailing text component, and inspect existing visual-effect components and material presets before introducing a new UI effect.
   - Treat screenshots and design coordinates as visual intent, not as permission to replace a working hierarchy.
   - When design images are supplied, follow Design References And Clarification below to map screens and states, resolve material uncertainty, and validate the actual result against each requested reference.
2. Classify each region.
   - Mark art as full-bleed or safe-area content.
   - Mark placement as fixed to an edge or corner, stretched between regions, content-sized, repeated-layout content, scrollable content, modal, or world-space UI.
   - Decide which component owns each axis. One axis must not be driven concurrently by a Layout Group, ContentSizeFitter, AspectRatioFitter, animation, and manual code.
3. Make the smallest coherent change.
   - Preserve the prefab root, existing children, components, names, serialized references, animation bindings, and Prefab overrides unless a specific replacement is required.
   - Modify only the necessary RectTransforms, components, fields, and children. Do not recreate an entire UI or GameObject prefab unless the user explicitly requests a rebuild.
   - Author reusable user-facing screens, panels, and controls as prefabs with their hierarchy and component references wired in the Editor, then instantiate and bind data at runtime. Do not move a stable UI hierarchy into procedural runtime construction merely for implementation convenience.
   - Use Unity MCP or Unity Editor APIs for `.prefab`, `.unity`, and `.asset` changes; never patch Unity YAML as text.
4. Read back and validate.
   - Read exact hierarchy, anchors, offsets, sizes, sprites (including borders for Sliced Images), text settings, raycast state, sorting, and references back from Unity.
   - Test layout, input, safe area, localization, animation interruption, close and reopen state, and runtime data changes.
   - Capture screenshots at representative aspect ratios for static layout; use a short `record_game_view` clip when correctness depends on an animation or interaction sequence. Use a real device build for performance and platform behavior before claiming device validation.

## Design References And Clarification

- For one or more design images, map each reference to its intended screen, popup, state, or responsive variant before editing. Do not merge incompatible variants or assume that upload order establishes version priority. Inspect every requested reference at a readable scale and look for matching project sprites, fonts, materials, and existing controls before assuming a detail is unavailable.
- Keep a compact per-screen checklist of visible details that matter: composition, alignment, spacing, sizes, text and line breaks, typography and effects, colors, icons, borders and corners, layering, and shown control states. Distinguish what the image actually shows from inferred behavior. Do not omit decorative details, reuse an approximate asset, or simplify a control merely because the rough layout already looks similar.
- When revising existing UI, preserve prefab structure, serialized references, and working behavior, not known visual mismatches. A clearly requested design change calls for scoped visual edits; the old UI looking different is not itself a reason to ask. If matching the design would conflict with an explicit preservation requirement or require changing existing behavior, explain the conflict and ask before that affected change.
- If reference and asset inspection still leave a material ambiguity, proactively ask a focused question before committing to that interpretation: identify the image, screen and region, state what cannot be determined, and offer plausible alternatives and their impact when useful. Examples include unreadable copy, conflicting versions, unclear page or state mapping, or missing exact artwork; request a clearer crop or source asset when that would resolve it. Do not silently invent text, omit an element, or substitute an approximation. Continue independent, clearly specified work while awaiting an answer; use project conventions for low-risk reversible details and disclose assumptions that affect the visible result rather than asking about every pixel.
- Validate every requested screen and state with actual Unity captures at the reference aspect ratio and comparable content, then check responsive variants separately. Inspect the captures, compare each region against its reference (side-by-side or with aligned overlays when useful), correct the differences, and recapture. Compilation success, component readback, or one correct page does not establish visual fidelity for the other pages. Do not stretch or crop the comparison to hide a mismatch.
- Before reporting completion, distinguish verified matches, user-approved deviations, unresolved differences, and unverified screens or states. Include any missing assets, unanswered visual decisions, or capture limitations. Do not claim full fidelity while known unapproved mismatches remain; ask the user to resolve remaining design choices instead of declaring an approximation complete.

## Component Selection

| Component | Use it for | Configure deliberately | Avoid |
| --- | --- | --- | --- |
| `Canvas` | Root render and sorting space | Use Screen Space - Overlay for ordinary HUD and menus, Screen Space - Camera when camera composition or scene interleaving is required, and World Space only for UI that belongs in the 3D world | Adding independent canvases only to organize folders; leaving Event Camera unset in Camera or World Space modes |
| `CanvasScaler` | Converting a design resolution to screen-space scale | For mobile screen UI, normally use Scale With Screen Size and a documented portrait or landscape reference resolution; choose Match from actual width and height constraints | Assuming Match `0.5` solves every aspect ratio or relying on Constant Physical Size without validating device DPI |
| `RectTransform` | Parent-relative position and size | Set anchors first, then pivot and offsets; keep local scale at one; use stretch anchors for flexible regions | Using anchoredPosition from one screenshot as universal placement or using Transform scale as a layout tool |
| `HorizontalLayoutGroup` / `VerticalLayoutGroup` | Dynamic rows, columns, button rails, and variable-length lists | Set padding, spacing, child size control, expansion, and child `LayoutElement` intent | Applying a Layout Group to hand-composed full-screen art or manually positioning driven children |
| `GridLayoutGroup` | Uniform inventory, level, or card cells | Choose cell size, spacing, start axis, and a row or column constraint | Expecting child preferred sizes to change grid cells; GridLayoutGroup assigns fixed cells |
| `LayoutElement` | Declaring minimum, preferred, flexible, or ignored layout behavior | Use it to override an Image, text, or nested group's layout contribution and to make selected siblings flexible | Adding it without selecting the properties that should override layout input |
| `ContentSizeFitter` | Making the current RectTransform follow its content on one or two axes | Prefer a single required axis; set the pivot to control growth direction; allow deferred layout unless immediate measurement is truly required | Putting it on every child controlled by a parent Layout Group or writing the same driven size manually |
| `AspectRatioFitter` | Preserving aspect for an isolated preview, card art, or media surface | Use Fit In Parent for letterboxing or Envelope Parent for cover behavior | Treating it as general safe-area or screen-aspect adaptation, or combining it with another controller on the same axis |
| `Image` | Sprite UI, icons, frames, progress fills, and nine-sliced controls | Use Simple for fixed art, Sliced for resizable panels and buttons only after verifying suitable Sprite borders, Tiled for repeatable patterns, and Filled for progress or radial values | Sliced with all-zero borders, stretching bordered art as Simple, leaving decorative graphics as Raycast Target, or using a unique material without need |
| `RawImage` | Arbitrary Texture, RenderTexture, camera, video, downloaded, or generated texture content | Preserve the source aspect and manage texture lifetime explicitly | Using RawImage for ordinary Sprite UI that should atlas and batch with other Images |
| `UnityEngine.UI.Text` | Text in an established legacy uGUI project or screen family | Use it only after inspection shows it is the prevailing project convention; match the existing Font, material, alignment, line spacing, overflow, and localization behavior | Introducing it into a new project, mixing it casually into a TMP-based screen, or converting existing labels without checking layout and serialized references |
| `TextMeshProUGUI` | Text in an established TMP project and the default for a new project with no existing text convention | Match the project's font assets and material presets; set wrapping, alignment, overflow, fallback fonts, and localization limits; constrain Auto Size to a narrow range; when the design explicitly shows a text effect, use TMP's own component and shader-material controls | Replacing an established `Text` component merely to modernize, continuous Auto Size on rapidly changing text, or shipping without required CJK and symbol glyphs |
| TMP font material / material preset | An outline, underlay or shadow, glow, face dilation, softness, or other text treatment explicitly visible in the design for a `TextMeshProUGUI` project | For a simple outline set the TMP component's `outlineColor` and `outlineWidth`; for underlay, glow, or other shader effects reuse an approved project preset or create a dedicated preset or material instance and verify it with fallback fonts | Adding a uGUI `Outline` or `Shadow` to TMP text by habit, changing a shared `fontSharedMaterial` and unintentionally restyling other labels, or enabling effects absent from the design |
| `Outline` / `Shadow` / similar `BaseMeshEffect` | A specifically designed effect on legacy `UnityEngine.UI.Text` or another Graphic, when required by the design or established project style | Reuse the project's approved style and keep effect distance, color, alpha, and stacking minimal; verify legibility and cost on target hardware; prefer TMP-native effects when the text component is `TextMeshProUGUI` | Adding generic polish by default, stacking effects, applying them broadly, or using them to compensate for weak contrast or incorrect layout; these effects duplicate UI geometry and increase overdraw |
| `TMP_InputField` | Editable TMP text | Author and validate a prefab with `textComponent`, `textViewport`, `placeholder` when used, target Graphic, navigation, and input settings already serialized; instantiate the prefab and bind data or listeners at runtime | Rebuilding a stable input hierarchy in code; if dynamic construction is genuinely required, never add it to an active GameObject and bind `textComponent` afterward because affected TMP versions create the caret renderer in `OnEnable` only when that reference is already present |
| `ScrollRect` | Drag or wheel scrolling through content larger than a viewport | Use `ScrollRect -> Viewport + RectMask2D -> Content`, reference both Viewport and Content, enable only required axes, and choose Clamped or Elastic intentionally | Unrestricted movement without recovery, deeply nested competing scroll axes, or instantiating thousands of live rows without virtualization |
| `RectMask2D` | Rectangular clipping in 2D Canvas UI | Prefer it for scroll viewports and rectangular reveal areas | Using stencil `Mask` for a simple rectangle |
| `Mask` | Clipping to a non-rectangular Graphic shape | Use only when the shape matters and account for stencil and material cost | Deeply nested masks or using it where RectMask2D is sufficient |
| `CanvasGroup` | Fading and enabling or disabling a whole panel | Change alpha, interactable, and blocksRaycasts together according to visible state; decide whether parent groups apply | Setting alpha to zero while leaving an invisible panel interactive or raycast-blocking |
| `Button` and other `Selectable` controls | Click, toggle, slider, dropdown, and navigation behavior | Put the main Raycast Target on the interactive root, set Target Graphic and navigation, and add and remove runtime listeners symmetrically | Multiple child Raycast Targets for one control, duplicate listeners, or visual-only disabled states |
| `EventSystem` and `GraphicRaycaster` | Routing pointer, touch, submit, cancel, and navigation events | Keep one EventSystem and one active matching input module; use `InputSystemUIInputModule` with the Input System; enable raycast only where required | A second EventSystem in additive scenes or physics blocking checks when they are unnecessary |

## Sliced Images And Sprite Borders

- Before setting `Image.type = Image.Type.Sliced`, inspect the Sprite actually displayed, including `overrideSprite` when present, and read its `Sprite.border`. An all-zero border does not produce nine-slicing; selecting Sliced alone is insufficient. Do not leave `This Image doesn't have a border.` unresolved and claim that the frame is correctly sliced.
- Choose border insets from the source artwork in pixels, preserving its rounded corners, outlines, and other non-stretchable edge details while leaving a usable stretchable center. `Vector4` order is **left, bottom, right, top** (`x, y, z, w`). Do not invent arbitrary nonzero values just to suppress the warning. Zero on some sides can be intentional for one-axis stretching; not every design needs four positive insets.
- Configure Border in Sprite Editor and Apply, or use the matching Sprite import API: for Single mode, set `TextureImporter.spriteBorder` and call `SaveAndReimport`; for Multiple mode, edit only the intended sub-sprite's border metadata, preserving its rect, name, IDs, and other slices. For atlased sprites, edit the source Sprite metadata, not the packed atlas texture or atlas padding.
- Check other consumers before changing a shared Sprite's border. Reuse a suitable existing bordered Sprite when possible; if the art is unsuitable or cannot safely be changed, report the limitation and choose an appropriate Image type or asset within the task scope. Do not replace the whole prefab or silently alter unrelated UI to resolve a border warning.
- After Apply or reimport, reacquire the displayed Sprite and read back `Sprite.border` and `Image.type`; an importer assignment alone is not verification. Resize at the intended minimum and representative target sizes and aspect ratios, then inspect the result: corners retain their shape, edge thickness is consistent, and the center has no seams or clipping. `pixelsPerUnitMultiplier` changes border sizing, not the slicing insets, and cannot replace missing borders.

## Canvas And Layering

- Use a small number of semantic layers such as Background, Screen, HUD, Overlay, Modal, Loading, and Debug. Make each layer a full-stretch RectTransform and define sibling or sorting order once.
- Let opaque or decorative backgrounds bleed to the physical screen edges. Put critical labels and all interactive controls under a separate SafeAreaRoot.
- When Modal or Loading UI is visible, block gameplay input explicitly; a visible scrim alone does not prove input is blocked.
- Keep one stable scrim per popup layer when a popup stack owns it. Restore the previous popup and its input state when the top popup closes.
- Distinguish Hide from Close. Hide can retain a cached instance; Close must release instantiated assets, handles, listeners, and transient state.
- Split static and frequently changing UI only when profiling shows rebuild cost. Nested canvases isolate rebuilds but prevent batching across canvas boundaries, so do not create one Canvas per widget.

## Canvas Scaler And RectTransform Rules

- Treat the reference resolution as design coordinates, not a list of supported physical resolutions. A proven portrait baseline is `720 x 1559`; a proven landscape baseline is `1559 x 720`.
- Start with Scale With Screen Size and Match `0.5` when width and height are equally important, then verify. Move Match toward width when horizontal design width must remain stable, or toward height when vertical design height must remain stable.
- Use anchors to express attachment: top bars to top stretch, bottom actions to bottom or bottom stretch, edge buttons to their corner, and center gameplay viewports to stretch between reserved regions.
- Set anchors before recording offsets. With separated anchors, `sizeDelta` is the delta relative to the anchor rectangle, not the final absolute size.
- Resize UI through RectTransform width, height, anchors, and offsets; leave localScale at one. Animate a child named Visual or Container when the root is layout-driven.
- Respond to `OnRectTransformDimensionsChange` or an equivalent resolution and orientation signal when layout contains calculated page widths, aspect branches, or safe-area anchors. Do not poll and rewrite every RectTransform every frame.
- Prefer `LayoutRebuilder.MarkLayoutForRebuild` for deferred updates. Use `Canvas.ForceUpdateCanvases` or `LayoutRebuilder.ForceRebuildLayoutImmediate` only when code must measure the final layout in the same operation, never as a routine per-frame fix.

## Safe Area

- Read `Screen.safeArea` in screen pixels and convert both minimum and maximum corners to normalized anchors. Reapply when screen dimensions, orientation, or safe area changes; do not cache only a top inset.
- A minimal uGUI conversion is:

```csharp
Rect safe = Screen.safeArea;
safeAreaRoot.anchorMin = new Vector2(
    safe.xMin / Screen.width,
    safe.yMin / Screen.height);
safeAreaRoot.anchorMax = new Vector2(
    safe.xMax / Screen.width,
    safe.yMax / Screen.height);
safeAreaRoot.offsetMin = Vector2.zero;
safeAreaRoot.offsetMax = Vector2.zero;
```

- Guard zero screen dimensions and avoid duplicate application when nothing changed.
- Check `PlayerSettings.Android.renderOutsideSafeArea`. If rendering outside is disabled, the Player window can already be fitted to the safe area and `Screen.safeArea` can equal the full Player window; do not apply a second inset blindly.
- In portrait, verify top cutout and bottom home-indicator or navigation areas. In landscape, verify both left and right cutouts in Landscape Left and Landscape Right.
- For edge art assembled from nested images, particles, labels, or Spine content, wait until layout and final offsets are applied, then use `RectTransformUtility.CalculateRelativeRectTransformBounds(parent, visualRoot)` to clamp the complete visual bounds inside the allowed safe rectangle.

## Portrait Mobile Pattern

- Organize the screen as Top, Center, and Bottom regions. Anchor persistent status and currency UI to Top; navigation, primary actions, and skill buttons to Bottom; stretch the game or page viewport through Center.
- Reserve top and bottom space with offsets on the stretched center viewport instead of giving the viewport a fixed height.
- Let additional height on tall phones expand the center region. Do not multiply every vertical coordinate by the screen aspect ratio.
- For horizontally paged home screens, compute each page from the current viewport width and recompute content width and selected-page position when dimensions change.
- Keep centered popup content within a safe maximum height. Use scrolling for localized or data-driven content that can exceed that height.

## Landscape Mobile Pattern

- Organize the screen as Left, Center, Right, plus stable corners. Put high-frequency gameplay content in Center and distribute controls so neither side becomes a single crowded column.
- Use 16:9 as a common gameplay baseline, but branch deliberately for ultrawide, 16:10, and 4:3 tablet layouts. Anchors handle attachment; a small aspect-aware layout policy handles genuine composition changes.
- Keep camera framing separate from Canvas scaling. A narrow landscape viewport may require a larger orthographic size or alternate camera composition to preserve world-space gameplay even when the Canvas itself is correct.
- Use background cover scaling or phone and tablet background variants when one crop cannot preserve the art direction across 16:9 and 4:3.
- Reposition only the controls whose composition genuinely changes at tablet aspect ratios. Do not fork the entire screen prefab when a few region offsets or constraints are sufficient.

## Auto Layout And Dynamic Content

- Remember the allocation order: minimum size, then preferred size, then flexible size. Use `LayoutElement` to state which sibling can consume extra space.
- A Layout Group drives its children. Do not manually edit a driven child position or size and expect it to persist after the next rebuild.
- A ContentSizeFitter drives its own RectTransform and expands around its pivot. Use a top pivot for content that must grow downward and a left pivot for content that must grow rightward.
- Do not put ContentSizeFitter on children whose RectTransforms are already controlled by the parent Layout Group. Disable Child Force Expand and use child layout input instead.
- GridLayoutGroup ignores child minimum, preferred, and flexible dimensions and assigns the configured fixed cell size. Use a different layout or custom controller for variable-sized grid cells.
- Keep layout nesting shallow. Repeated layout invalidation walks the hierarchy; batch model changes, update content, then request one rebuild.

## Images, Text, Scrolling, And Input

- For scalable button and panel frames, follow Sliced Images And Sprite Borders above before using Image Type Sliced. Keep ornamental children non-raycastable.
- Use Sprite Atlas for compatible UI sprites, platform-specific texture overrides, sensible maximum sizes, and no mipmaps for ordinary screen-space UI unless a measured use case needs them.
- Treat large full-screen images separately from small control atlases. Verify memory, compression artifacts, overdraw, and crop behavior on target hardware.
- Before adding a label, inspect representative UI prefabs and scenes rather than inferring the text system from package availability. Preserve the component type on existing labels and use the text component that is most common in the relevant project or screen family. If the project is new and has no established convention, default to `TextMeshProUGUI`.
- Do not opportunistically migrate `UnityEngine.UI.Text` to `TextMeshProUGUI`, or the reverse, while composing unrelated UI. Such a migration can change preferred sizes, wrapping, materials, fallback behavior, animation bindings, and serialized component references and requires separate validation.
- In TMP projects, use font fallback chains for CJK, symbols, and localized glyphs. Keep common glyphs in the primary asset and verify fallback material appearance and draw-call impact.
- Prefer wrapping, truncation, or a known layout expansion policy over broad Auto Size ranges. TMP Auto Size performs repeated layout passes and is unsuitable for frequently changing counters or timers.
- Do not add `Outline`, `Shadow`, or another `BaseMeshEffect` merely because a control looks unfinished. Require an explicit design need or a verified existing project style, prefer the existing shared prefab or TMP material preset when applicable, avoid stacked effects, and verify the extra geometry and overdraw.
- When the reference image or design explicitly shows an outline or another font effect and the project uses `TextMeshProUGUI`, reproduce the visible treatment through the TMP component rather than omitting it or attaching a uGUI mesh effect by habit. Use `outlineColor` and `outlineWidth` for a simple outline; use an existing approved TMP material preset, or a dedicated preset or material instance, for underlay or shadow, glow, face dilation, softness, and other shader effects.
- Treat TMP material scope as part of the edit. Do not modify a shared `fontSharedMaterial` when the effect is local to one label or prefab because every user of that material may change. Reuse a matching project preset when one exists; otherwise create a deliberately scoped preset or instance, preserve the font atlas and fallback chain, and verify the result against the reference at target resolution.
- Prefer a prefab for `TMP_InputField` and other stable controls so hierarchy, references, navigation, styling, localization, and focus behavior are inspectable before Play Mode. Runtime code should instantiate the prefab and supply data and listeners, not recreate its child objects and component wiring.
- Only when procedural construction is explicitly required, treat the first enable of a runtime-created `TMP_InputField` as an initialization boundary. Create and wire its text hierarchy while the root is inactive (or the component is disabled), assign at least `textComponent` and `textViewport` plus `placeholder` when used, then enable it. In affected TMP versions, `OnEnable` creates the cached `Caret` renderer only when `textComponent` is already bound; assigning the property after that first enable does not retroactively create it.
- If a dynamically constructed `TMP_InputField` accepts text but shows no insertion caret, inspect whether a `Caret` / `TMP_SelectionCaret` object was created and whether `textComponent` was assigned before first enable. After wiring the missing references, disable and re-enable the field to run initialization again; then verify focus, blinking caret, selection highlight, placeholder state, and editing in Play Mode.
- For a vertical ScrollRect, top-anchor the Content and set its pivot to the top so growth is predictable. Preserve the normalized position intentionally when refreshing content.
- Use RectMask2D for rectangular viewports. Use Mask only when the clipping shape must follow a Graphic.
- Give touch controls a project-defined minimum hit area even when the visible art is smaller. Use one transparent or visible root Graphic or raycast padding rather than making every child Image a target.
- Add listeners once and remove them in the matching lifecycle. Disable interaction while entrance or exit animation makes a control visually unavailable.

## Animation And Prefab Safety

- Animate a popup Container or Visual child while leaving the full-screen scrim stable. This avoids scaling the raycast blocker and avoids fighting layout-driven roots.
- Before replaying an animation, kill or cancel the prior sequence and restore a deterministic base position, scale, alpha, interactable, and blocksRaycasts state.
- Use unscaled time for menu, pause, modal, and loading animations that must continue while gameplay time is zero.
- Preserve existing prefab objects by default. Replacing an asset at the same path can preserve the asset GUID while still changing child or component file IDs, breaking animation bindings, serialized references, Prefab Variants, and Scene overrides.
- Prefer serialized references or stable binding components. Use `Transform.Find` only for a verified stable hierarchy and fail clearly if it is missing; never silently create an alternate hierarchy.
- Use semantic names such as SafeAreaRoot, TopBar, ContentViewport, BottomActions, Visual, and Label. Replace ambiguous default names only when doing so will not break bindings, and update references atomically.

## Performance And Validation

- Profile before restructuring. Common uGUI bottlenecks are overdraw, Canvas batch rebuilds, repeated layout rebuilds, raycast candidates, text mesh generation, and excess materials or textures.
- Separate mostly static UI from high-frequency counters, timers, scrolling content, or animations when profiling justifies the extra Canvas. Co-locate elements that change together.
- Disable Raycast Target on decorative Images and TMP text. An active Graphic Raycaster tests eligible Graphics and raycast filters along their hierarchy.
- Avoid hiding large inactive screens only with alpha zero; they can still render or receive input depending on CanvasGroup state. Use the project's hide or pooling policy and measure reopen cost.
- Validate portrait at 16:9, 19.5:9 or 20:9, a cutout phone, and a portrait tablet. Validate landscape at 16:9, ultrawide, 16:10, 4:3, and both cutout sides.
- In every profile, verify full-bleed art, safe interactive content, text overflow and fallback glyphs, scroll bounds, modal input blocking, touch hit areas, selection navigation, animation interruption, and close and reopen state.
- Use `capture_game_view` for static composition, text fit, and before/after comparisons. Use a short `record_game_view` clip for behavior a still image cannot establish, such as popup transitions, scroll inertia, caret blinking, interrupted animations, or repeated close and reopen actions; record only the sequence relevant to the change.
- For a clip, finish compilation, enter Play Mode and wait for MCP recovery, keep the Game tab visible at a fixed resolution, then start recording before performing the interaction. Save `data.recording_id`, pass it to `action=status` or `action=stop`, and wait for `data.ready=true` before reviewing the local MP4. The Unity MCP Workflow skill describes supported Editors and failure handling. If recording or video viewing is unavailable, report that limitation; screenshots and hierarchy readback alone do not prove timing or transition correctness.
- Review intermediate frames as well as the final state: look for clipping or layout jumps, stuck raycast blocking, input leaking through a modal, and interruption or reopen state. Pair the clip with component-state readback and actual input checks; a visual result alone cannot prove event routing. Recording is silent and adds overhead, so it cannot validate audio or replace Profiler and real-device performance checks.
- Use Device Simulator for layout, safe-area, orientation, and basic single-touch checks. It does not simulate target CPU, GPU, memory, rendering backend, native plugins, or multitouch; use representative device builds for performance and final interaction validation.

## Official Unity References

- [Canvas render modes and nesting](https://docs.unity.cn/Packages/com.unity.ugui%402.0/manual/class-Canvas.html)
- [CanvasScaler API and Match behavior](https://docs.unity.cn/Packages/com.unity.ugui%402.0/api/UnityEngine.UI.CanvasScaler.html)
- [RectTransform](https://docs.unity.cn/Packages/com.unity.ugui%402.0/manual/class-RectTransform.html) and [multi-resolution UI](https://docs.unity.cn/Packages/com.unity.ugui%402.0/manual/HOWTO-UIMultiResolution.html)
- [Auto Layout](https://docs.unity.cn/Packages/com.unity.ugui%402.0/manual/UIAutoLayout.html), [LayoutElement](https://docs.unity.cn/Packages/com.unity.ugui%402.0/manual/script-LayoutElement.html), and [ContentSizeFitter](https://docs.unity.cn/Packages/com.unity.ugui%402.0/manual/script-ContentSizeFitter.html)
- [ScrollRect](https://docs.unity.cn/Packages/com.unity.ugui%402.0/manual/script-ScrollRect.html), [RectMask2D](https://docs.unity.cn/Packages/com.unity.ugui%402.0/manual/script-RectMask2D.html), and [Mask](https://docs.unity.cn/Packages/com.unity.ugui%402.0/manual/script-Mask.html)
- [Image](https://docs.unity.cn/Packages/com.unity.ugui%402.0/manual/script-Image.html), [CanvasGroup](https://docs.unity.cn/Packages/com.unity.ugui%402.0/manual/class-CanvasGroup.html), and [Selectable navigation](https://docs.unity.cn/Packages/com.unity.ugui%402.0/manual/script-SelectableNavigation.html)
- [Image.Type.Sliced and its border prerequisite](https://docs.unity3d.com/2018.4/Documentation/ScriptReference/UI.Image.Type.Sliced.html), [Sprite.border component order](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/Sprite-border.html), and [TextureImporter.spriteBorder](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/TextureImporter-spriteBorder.html)
- [TextMeshPro UI text and Auto Size](https://docs.unity.cn/Packages/com.unity.textmeshpro%403.2/manual/TMPObjectUIText.html) and [fallback fonts](https://docs.unity.cn/Packages/com.unity.textmeshpro%404.0/manual/FontAssetsFallback.html)
- [Screen.safeArea](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Screen-safeArea.html), [relative RectTransform bounds](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/RectTransformUtility.CalculateRelativeRectTransformBounds.html), and [Device Simulator](https://docs.unity3d.com/6000.0/Documentation/Manual/device-simulator-introduction.html)
- [Sprite Atlas](https://docs.unity3d.com/6000.0/Documentation/Manual/sprite/atlas/create-sprite-atlas.html), [platform texture overrides](https://docs.unity3d.com/6000.0/Documentation/Manual/class-TextureImporter-type-specific.html), and [official uGUI optimization guide](https://learn.unity.com/course/introduction-to-ui-in-unity/tutorial/optimizing-unity-ui)


## Structured UI Workflows

- Short MCP tasks briefly wait for completion (wait_seconds defaults to 2; zero returns immediately). For longer tasks use `get_task(data.task.task_id, wait_seconds=20, after_revision=<last revision>)`; it waits for completion or a meaningful state change. Honor poll_after_ms on unchanged responses instead of making the model poll every second. wait_complete is not proof of success: inspect native status, errors and ready/complete/restoration fields. A read_timeout carries only the snapshot_at observation. Cancellation of the HTTP wait does not cancel the task. On lost preparation/preview responses recover through kind + the original request_key; do not replay mutations. Recording and Test Runner starts return immediately, as do preparation/preview starts without a recovery key.
- Inspect before modifying: use `find_game_objects` with component/property filters and projections, `inspect_ui_sprites` for Image/effective Sprite/importer/border/local-ID associations, and `find_project_types` for exact type and assembly names. Check ambiguity, partial errors, scan completeness and pagination; an incomplete scan is not proof of absence. Component setters distinguish live in-memory readback from saved/reimported prefab values.
- Run `audit_ui` on relevant live roots or saved prefabs/scenes; small scans can finish in one call, otherwise read status and finding pages through `get_task`. It checks missing borders, missing/required references, transparent raycast blockers, text/clipping and layout conflicts without fixing or saving assets. Review measured evidence and contextual warnings; suppress intentional exceptions only with an explicit project reason. Do not invent border values or infer design fidelity from a clean audit.
- Before creating new UI, read `get_ui_defaults`. `create_project_ui` can reuse templates and retain their prefab connection, label bindings, font/material and authored geometry. Explicit overrides take precedence; existing template component types are not converted. `configure_ui_defaults` changes project-scoped authoring preferences, so use it only when that shared convention is intended. Tied/incomplete convention scans or missing TMP resources require a deliberate choice/action, never a silent legacy fallback. This is Edit Mode authoring: save the intended scene/prefab explicitly and preserve existing UI when revising it.
- When preview management is exposed (Full by default), use `start_ui_preview_session` with verified prefab_paths and/or a project scene_template, optionally enter_play_mode and target width/height. It needs saved clean original scenes and no open Prefab Stage; do not save/discard unrelated user work merely to satisfy this precondition. Retain session_id and data.task.task_id; use `get_task` until ready. Business data and initialization remain project-specific; entering the scene may run lifecycle code.
- End the matching session with `end_ui_preview_session`, then inspect scenes_restored, view_restored, selection_restored, assets_cleaned and warnings. Do not claim full restoration from a success envelope. Changed scene setup, dirty preview or modified temporary scene requires inspection; discard_preview_changes applies only to the owned preview scene and must reflect an intended discard. Network/save-game effects and source asset edits are not rolled back. Preserve user-created files and changed window choices; report recovery still needed.
- Use screenshot `geometry`, not an unrelated `Screen` size: render size and returned image size can differ. Pass coordinate_space=image_pixels, origin=top_left and a fresh capture_id to click/drag/scroll or `raycast_at_point` when measuring a screenshot. `get_object_screen_bounds` and `get_visual_coordinates` share the mapping. Expired IDs or changed mode/view/scene/camera viewport/render dimensions require a fresh capture, not clamping or guessing. Geometry validity does not prove animated content stayed unchanged.


## Metadata

- Original skill id: `unity-ui-composition`
- Skill version: `1.0.6`
- Platform: `antigravity`
- Source repository: `https://github.com/FunplayAI/funplay-unity-mcp`
````

## File: .funplay/skills/manifest.json
````json
{
    "platforms": [
        "antigravity"
    ],
    "optionalSkills": [],
    "skillVersions": [
        {
            "id": "unity-mcp-workflow",
            "version": "1.0.5"
        },
        {
            "id": "unity-ui-composition",
            "version": "1.0.6"
        }
    ]
}
````

## File: Assets/Scripts/RobotManagerEVE.cs
````csharp
using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class RobotManagerEVE : MonoBehaviour
{
    [Header("Waypoints de Vuelo")]
    public Transform[] waypoints;
    public float velocidadVuelo = 5f;
    private int indiceWaypointActual = 0;

    [Header("Base de Datos Local")]
    public Manifiesto manifiestoActual;
    public RobotAMR robotEjecutor;
    public int indicePedidoActual = 0;

    void Awake()
    {
        CargarManifiesto();
    }

    void Start()
    {
        if (manifiestoActual != null && manifiestoActual.pedidos.Count > 0 && robotEjecutor != null)
        {
            robotEjecutor.AsignarMision(manifiestoActual.pedidos[indicePedidoActual]);
        }
    }

    void Update()
    {
        MoverPorWaypoints();
    }

    void CargarManifiesto()
    {
        string ruta = Path.Combine(Application.streamingAssetsPath, "manifiesto.json");
        if (File.Exists(ruta))
        {
            string contenidoJson = File.ReadAllText(ruta);
            manifiestoActual = JsonUtility.FromJson<Manifiesto>(contenidoJson);
            
            Debug.Log("<color=green>EVE:</color> Manifiesto cargado. Total de pallets: " + manifiestoActual.pedidos.Count);
            DashboardUI.Instance?.RegistrarLog("EVE: Manifiesto cargado. Total de pallets: " + manifiestoActual.pedidos.Count);
            FindObjectOfType<GeneradorTablaManifiesto>()?.PoblarTabla(manifiestoActual.pedidos);
        }
        else
        {
            Debug.LogError("EVE Error crítico: No se encontró el Manifiesto de Carga en la ruta: " + ruta);
            DashboardUI.Instance?.RegistrarLog("EVE Error: No se encontró el Manifiesto de Carga.");
        }
    }

    public void ConfirmarEntregaExitosa()
    {
        indicePedidoActual++;
        if (indicePedidoActual < manifiestoActual.pedidos.Count)
        {
            robotEjecutor.AsignarMision(manifiestoActual.pedidos[indicePedidoActual]);
        }
        else
        {
            Debug.Log("<color=green>Operación Finalizada:</color> Manifiesto Nocturno completado. No hay más pedidos.");
            DashboardUI.Instance?.RegistrarLog("Operación Finalizada: Manifiesto completado.");
        }
    }

    void MoverPorWaypoints()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        Transform destino = waypoints[indiceWaypointActual];
        if (destino == null) return;
        /*En Unity, la palabra transform (con "t" minúscula) es una variable nativa e invisible
        que el motor le otorga automáticamente a todos los scripts que heredan de MonoBehaviour.
        No necesitas declararla en tu código porque ya existe en el núcleo del sistema.*/
        transform.position = Vector3.MoveTowards(transform.position, destino.position, velocidadVuelo * Time.deltaTime);

        if (Vector3.Distance(transform.position, destino.position) < 0.1f)
        {
            indiceWaypointActual = (indiceWaypointActual + 1) % waypoints.Length;
        }
    }
}
````

## File: Assets/StreamingAssets/manifiesto.json
````json
{
    "pedidos": [
        {
            "id_pallet": "PAL-8491",
            "nombre_producto": "CRubia Aguila (Lata 330ml)",
            "codigo_sku": "AGU-RUB-330",
            "peso_kg": 850,
            "coordenada_bodega": "P1-C1-F1"
        },
        {
            "id_pallet": "PAL-8492",
            "nombre_producto": "CNegra ClubColombia (Botella 330ml)",
            "codigo_sku": "CLU-NEG-330",
            "peso_kg": 850,
            "coordenada_bodega": "P1-C2-F1"
        },
        {
            "id_pallet": "PAL-8493",
            "nombre_producto": "CEspecial Oktoberfest (Barril 50L)",
            "codigo_sku": "OKT-BAR-50L",
            "peso_kg": 920,
            "coordenada_bodega": "P2-C4-F1"
        }
    ]
}
````

## File: AGENTS.md
````markdown
# AGENTS.md

<!-- Funplay Unity MCP managed project skills -->
<!-- Funplay Unity MCP project skill versions: unity-mcp-workflow@1.0.5, unity-ui-composition@1.0.6 -->

# Funplay Unity MCP Project Guidance

This section is managed by Funplay MCP for Unity. Everything between the begin and end markers is regenerated on each sync; edit outside this block.

## Installed project skills

- `funplay-unity-mcp-workflow` v1.0.5 - Efficient workflow for using Unity MCP to edit, import, compile, inspect, and test Unity projects, including screenshot and Game View recording verification.
- `funplay-unity-ui-composition` v1.0.6 - Build and revise responsive Unity uGUI mobile interfaces, including portrait and landscape layouts, safe areas, prefabs, auto layout, scrolling, text, input, animation, and performance validation.

## Agent workflow rules

- Prefer project-local Funplay skills: `.codex/skills/` for Codex, `.opencode/skills/` for OpenCode, `.dsh/skills/` for DeepSeek Harness, `.agents/skills/` for Antigravity.
- Prefer structured MCP tools; use `execute_code` for project-specific gaps. For new snippets, include `using Funplay.Editor.Tools.Scripting;`, implement `IFunplayCommand`, and use `ctx.RegisterObjectCreation` / `RegisterObjectModification` / `DestroyObject` so changes participate in Undo automatically.
- Confirm the Unity project root, active scene, and real object/prefab/asset path before edits. Treat user-provided object names as hints, not paths.
- Inspect Unity objects through MCP before changing user-named scene or prefab targets. Carry the returned `instanceId` into follow-up calls (`find_method=by_id`) instead of re-resolving by name.
- Tool returns are structured JSON (`{success, message, data}` / `{success: false, code, error, data}`). Branch on `code`, not free-form text.
- Set component fields with `set_component_property(ies)` — it picks up `[SerializeField] private` fields and accepts Object references as `{"fileID": <instanceId>}` or `{"assetPath": "Assets/..."}`.
- For field-only prefab asset edits, use `set_prefab_property(ies)` with a verified `Assets/**/*.prefab` path. If it reports duplicate paths or components, retry only with an index from that response; use Prefab Mode for structural edits.
- Read editor state through dedicated tools (`get_selection`, `get_prefab_stage`, `get_tags`, `get_layers`, `get_build_settings`); use `execute_menu_item` before falling back to ad-hoc `execute_code`.
- Never edit `.unity`, `.prefab`, or `.asset` files with shell text tools or patches; use Unity MCP / Editor APIs for scenes, prefabs, and ScriptableObject assets.
- Save only the scene or prefab assets intentionally modified, then read back exact values.
- Prefer structured query/audit/edit tools in Core; low-frequency configuration and preview management remain in Full. Use `get_tool_capabilities` to check exposure; respect customized allowlists.
- `execute_code` refreshes by default; use skip_refresh only after verifying the intended assemblies are ready.
- In `execute_code`, null-guard every lookup and return explicit missing path/object/component messages; do not run self-healing fallback loops.
- For Unity object references, do not use `??=` for lazy rebinding; use explicit `if (field == null) field = Resolve();`.
- After edits use `prepare_editor` with the intended mode and a unique request_key; use the returned task_id with `get_task`, wait_seconds and after_revision until operation.status=ready and current_editor confirms readiness and mode, then check console errors. Honor poll_after_ms on unchanged results.
- `request_recompile` is rejected while Unity is in Play Mode. Call `exit_play_mode` first, then retry.
- On disconnect, retry the same operation status read by ID/key. Historical reload recovery or HTTP reachability alone does not establish current readiness.
- An interrupted arbitrary mutation has unknown outcome until exact object/asset readback confirms it; never automatically replay it.
- Avoid changing `Library/`, `Temp/`, `Logs/`, or `obj/`.

## Project

- Project root: `C:\Users\Aprendiz\Documents\ProyectosUnity\SistemaLogistico_AMR`
- Product name: `SistemaLogistico_AMR`

## Notes

- Re-run `Funplay > Project Skills` after changing selected skills or platforms.
<!-- /Funplay Unity MCP managed project skills -->
````

## File: Assets/Scripts/GestorJSON.cs
````csharp
using UnityEngine;
using System.IO;
using System.Collections.Generic;

/* Definimos la estructura exacta de un pedido en el JSON, Sysytem.Serializable nos permite convertir el JSON en variables de C#*/
[System.Serializable]
public class Pedido
{
    /*Variables idénticas en nombre y tipo de dato a las del JSON para facilitar el trabajo entre sistemas*/
    public string id_pallet;
    public string nombre_producto;
    public string codigo_sku;
    public float peso_kg;
    public string coordenada_bodega;
}

// Envolvemos la lista para que JsonUtility de Unity pueda leer el arreglo
[System.Serializable]
public class Manifiesto
{
    public List<Pedido> pedidos;
}

public class GestorJSON : MonoBehaviour
{
    [Header("Base de Datos Local")]
    public Manifiesto manifiestoActual;
    public RobotAMR robotEjecutor;

    void Start()
    {
        CargarManifiesto();
    }

    void CargarManifiesto()
    {
        /* Ruta dinámica que funciona en el Editor y en el ejecutable final (.exe), streamingAssetsPath es una proteccion contra el Empaquetado(Inmunidad) Unity tiene
        prohibido alterar, comprimir o encriptar lo que hay allí dentro*/
        string ruta = Path.Combine(Application.streamingAssetsPath, "manifiesto.json");
        /*antes de leer el archivo, verifica si el archivo existe, si no existe, lanza un error y detiene el programa*/
        if (File.Exists(ruta))
        {
            /*Si existe, lee el contenido del archivo*/
            string contenidoJson = File.ReadAllText(ruta);
            /* Magia del desacoplamiento: El texto se convierte en variables de C#*/
            manifiestoActual = JsonUtility.FromJson<Manifiesto>(contenidoJson);
            
            Debug.Log("<color=green>Éxito:</color> Manifiesto cargado. Total de pallets a despachar: " + manifiestoActual.pedidos.Count);
            DashboardUI.Instance?.RegistrarLog("Éxito: Manifiesto cargado. Total de pallets a despachar: " + manifiestoActual.pedidos.Count);
            // Si hay pedidos y el robot está conectado, le enviamos la primera orden (Índice 0)
            if (manifiestoActual.pedidos.Count > 0 && robotEjecutor != null)
            {
                robotEjecutor.AsignarMision(manifiestoActual.pedidos[0]);
            }
        }
        else
        {
            Debug.LogError("Error crítico: No se encontró el Manifiesto de Carga en la ruta: " + ruta);
            DashboardUI.Instance?.RegistrarLog("Error crítico: No se encontró el Manifiesto de Carga.");
        }
    }
}
````

## File: UserSettings/Layouts/CurrentMaximizeLayout.dwlt
````
%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &1
MonoBehaviour:
  m_ObjectHideFlags: 52
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 12010, guid: 0000000000000000e000000000000000, type: 0}
  m_Name: 
  m_EditorClassIdentifier: 
  m_Children:
  - {fileID: 3}
  m_Position:
    serializedVersion: 2
    x: 0
    y: 30
    width: 1908
    height: 859
  m_MinSize: {x: 300, y: 100}
  m_MaxSize: {x: 24288, y: 16192}
  vertical: 1
  controlID: 44978
  draggingID: 0
--- !u!114 &2
MonoBehaviour:
  m_ObjectHideFlags: 52
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 1
  m_Script: {fileID: 12015, guid: 0000000000000000e000000000000000, type: 0}
  m_Name: 
  m_EditorClassIdentifier: 
  m_MinSize: {x: 200, y: 200}
  m_MaxSize: {x: 4000, y: 4000}
  m_TitleContent:
    m_Text: Game
    m_Image: {fileID: -6423792434712278376, guid: 0000000000000000d000000000000000, type: 0}
    m_Tooltip: 
  m_Pos:
    serializedVersion: 2
    x: 415
    y: 119
    width: 1094
    height: 559
  m_SerializedDataModeController:
    m_DataMode: 0
    m_PreferredDataMode: 0
    m_SupportedDataModes: 
    isAutomatic: 1
  m_ViewDataDictionary: {fileID: 0}
  m_OverlayCanvas:
    m_LastAppliedPresetName: Default
    m_SaveData: []
    m_OverlaysVisible: 1
  m_SerializedViewNames: []
  m_SerializedViewValues: []
  m_PlayModeViewName: GameView
  m_ShowGizmos: 0
  m_TargetDisplay: 0
  m_ClearColor: {r: 0, g: 0, b: 0, a: 0}
  m_TargetSize: {x: 956, y: 538}
  m_TextureFilterMode: 0
  m_TextureHideFlags: 61
  m_RenderIMGUI: 1
  m_EnterPlayModeBehavior: 0
  m_UseMipMap: 0
  m_VSyncEnabled: 0
  m_Gizmos: 0
  m_Stats: 0
  m_SelectedSizes: 01000000000000000000000000000000000000000000000000000000000000000000000000000000
  m_ZoomArea:
    m_HRangeLocked: 0
    m_VRangeLocked: 0
    hZoomLockedByDefault: 0
    vZoomLockedByDefault: 0
    m_HBaseRangeMin: -478
    m_HBaseRangeMax: 478
    m_VBaseRangeMin: -269
    m_VBaseRangeMax: 269
    m_HAllowExceedBaseRangeMin: 1
    m_HAllowExceedBaseRangeMax: 1
    m_VAllowExceedBaseRangeMin: 1
    m_VAllowExceedBaseRangeMax: 1
    m_ScaleWithWindow: 0
    m_HSlider: 0
    m_VSlider: 0
    m_IgnoreScrollWheelUntilClicked: 0
    m_EnableMouseInput: 0
    m_EnableSliderZoomHorizontal: 0
    m_EnableSliderZoomVertical: 0
    m_UniformScale: 1
    m_UpDirection: 1
    m_DrawArea:
      serializedVersion: 2
      x: 0
      y: 21
      width: 1094
      height: 538
    m_Scale: {x: 1, y: 1}
    m_Translation: {x: 547, y: 269}
    m_MarginLeft: 0
    m_MarginRight: 0
    m_MarginTop: 0
    m_MarginBottom: 0
    m_LastShownAreaInsideMargins:
      serializedVersion: 2
      x: -547
      y: -269
      width: 1094
      height: 538
    m_MinimalGUI: 1
  m_defaultScale: 1
  m_LastWindowPixelSize: {x: 1094, y: 559}
  m_ClearInEditMode: 1
  m_NoCameraWarning: 1
  m_LowResolutionForAspectRatios: 01000000000000000000
  m_XRRenderMode: 0
  m_RenderTexture: {fileID: 0}
  m_showToolbar: 1
--- !u!114 &3
MonoBehaviour:
  m_ObjectHideFlags: 52
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 1
  m_Script: {fileID: 12010, guid: 0000000000000000e000000000000000, type: 0}
  m_Name: 
  m_EditorClassIdentifier: 
  m_Children:
  - {fileID: 4}
  - {fileID: 14}
  m_Position:
    serializedVersion: 2
    x: 0
    y: 0
    width: 1908
    height: 859
  m_MinSize: {x: 300, y: 100}
  m_MaxSize: {x: 24288, y: 16192}
  vertical: 0
  controlID: 44979
  draggingID: 0
--- !u!114 &4
MonoBehaviour:
  m_ObjectHideFlags: 52
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 1
  m_Script: {fileID: 12010, guid: 0000000000000000e000000000000000, type: 0}
  m_Name: 
  m_EditorClassIdentifier: 
  m_Children:
  - {fileID: 5}
  - {fileID: 11}
  m_Position:
    serializedVersion: 2
    x: 0
    y: 0
    width: 1505
    height: 859
  m_MinSize: {x: 200, y: 100}
  m_MaxSize: {x: 16192, y: 16192}
  vertical: 1
  controlID: 44980
  draggingID: 0
--- !u!114 &5
MonoBehaviour:
  m_ObjectHideFlags: 52
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 1
  m_Script: {fileID: 12010, guid: 0000000000000000e000000000000000, type: 0}
  m_Name: 
  m_EditorClassIdentifier: 
  m_Children:
  - {fileID: 6}
  - {fileID: 8}
  m_Position:
    serializedVersion: 2
    x: 0
    y: 0
    width: 1505
    height: 580
  m_MinSize: {x: 200, y: 50}
  m_MaxSize: {x: 16192, y: 8096}
  vertical: 0
  controlID: 44981
  draggingID: 0
--- !u!114 &6
MonoBehaviour:
  m_ObjectHideFlags: 52
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 1
  m_Script: {fileID: 12006, guid: 0000000000000000e000000000000000, type: 0}
  m_Name: 
  m_EditorClassIdentifier: 
  m_Children: []
  m_Position:
    serializedVersion: 2
    x: 0
    y: 0
    width: 409
    height: 580
  m_MinSize: {x: 201, y: 221}
  m_MaxSize: {x: 4001, y: 4021}
  m_ActualView: {fileID: 7}
  m_Panes:
  - {fileID: 7}
  m_Selected: 0
  m_LastSelected: 0
--- !u!114 &7
MonoBehaviour:
  m_ObjectHideFlags: 52
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 1
  m_Script: {fileID: 12061, guid: 0000000000000000e000000000000000, type: 0}
  m_Name: 
  m_EditorClassIdentifier: 
  m_MinSize: {x: 200, y: 200}
  m_MaxSize: {x: 4000, y: 4000}
  m_TitleContent:
    m_Text: Hierarchy
    m_Image: {fileID: 7966133145522015247, guid: 0000000000000000d000000000000000, type: 0}
    m_Tooltip: 
  m_Pos:
    serializedVersion: 2
    x: 6
    y: 119
    width: 408
    height: 559
  m_SerializedDataModeController:
    m_DataMode: 0
    m_PreferredDataMode: 0
    m_SupportedDataModes: 
    isAutomatic: 1
  m_ViewDataDictionary: {fileID: 0}
  m_OverlayCanvas:
    m_LastAppliedPresetName: Default
    m_SaveData: []
    m_OverlaysVisible: 1
  m_SceneHierarchy:
    m_TreeViewState:
      scrollPos: {x: 0, y: 0}
      m_SelectedIDs: 
      m_LastClickedID: 0
      m_ExpandedIDs: 8a79feff042dffff2efbffff104f0000424f00006a4f0000e04f0000ea500000
      m_RenameOverlay:
        m_UserAcceptedRename: 0
        m_Name: 
        m_OriginalName: 
        m_EditFieldRect:
          serializedVersion: 2
          x: 0
          y: 0
          width: 0
          height: 0
        m_UserData: 0
        m_IsWaitingForDelay: 0
        m_IsRenaming: 0
        m_OriginalEventType: 11
        m_IsRenamingFilename: 0
        m_ClientGUIView: {fileID: 6}
      m_SearchString: 
    m_ExpandedScenes: []
    m_CurrenRootInstanceID: 0
    m_LockTracker:
      m_IsLocked: 0
    m_CurrentSortingName: TransformSorting
  m_WindowGUID: 4c969a2b90040154d917609493e03593
--- !u!114 &8
MonoBehaviour:
  m_ObjectHideFlags: 52
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 1
  m_Script: {fileID: 12006, guid: 0000000000000000e000000000000000, type: 0}
  m_Name: GameView
  m_EditorClassIdentifier: 
  m_Children: []
  m_Position:
    serializedVersion: 2
    x: 409
    y: 0
    width: 1096
    height: 580
  m_MinSize: {x: 202, y: 221}
  m_MaxSize: {x: 4002, y: 4021}
  m_ActualView: {fileID: 2}
  m_Panes:
  - {fileID: 2}
  - {fileID: 9}
  - {fileID: 10}
  m_Selected: 0
  m_LastSelected: 2
--- !u!114 &9
MonoBehaviour:
  m_ObjectHideFlags: 52
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 12013, guid: 0000000000000000e000000000000000, type: 0}
  m_Name: 
  m_EditorClassIdentifier: 
  m_MinSize: {x: 100, y: 100}
  m_MaxSize: {x: 4000, y: 4000}
  m_TitleContent:
    m_Text: Scene
    m_Image: {fileID: 2593428753322112591, guid: 0000000000000000d000000000000000, type: 0}
    m_Tooltip: 
  m_Pos:
    serializedVersion: 2
    x: 415
    y: 119
    width: 1094
    height: 559
  m_SerializedDataModeController:
    m_DataMode: 0
    m_PreferredDataMode: 0
    m_SupportedDataModes: 
    isAutomatic: 1
  m_ViewDataDictionary: {fileID: 0}
  m_OverlayCanvas:
    m_LastAppliedPresetName: Default
    m_SaveData:
    - dockPosition: 0
      containerId: overlay-toolbar__top
      floating: 0
      collapsed: 0
      displayed: 1
      snapOffset: {x: -179, y: -26}
      snapOffsetDelta: {x: 0, y: 0}
      snapCorner: 3
      id: Tool Settings
      index: 0
      layout: 1
      size: {x: 0, y: 0}
      sizeOverriden: 0
    - dockPosition: 0
      containerId: overlay-toolbar__top
      floating: 0
      collapsed: 0
      displayed: 1
      snapOffset: {x: 169, y: 0}
      snapOffsetDelta: {x: 0, y: 0}
      snapCorner: 0
      id: unity-grid-and-snap-toolbar
      index: 1
      layout: 1
      size: {x: 0, y: 0}
      sizeOverriden: 0
    - dockPosition: 1
      containerId: overlay-toolbar__top
      floating: 0
      collapsed: 0
      displayed: 1
      snapOffset: {x: 0, y: 0}
      snapOffsetDelta: {x: 0, y: 0}
      snapCorner: 0
      id: unity-scene-view-toolbar
      index: 0
      layout: 1
      size: {x: 0, y: 0}
      sizeOverriden: 0
    - dockPosition: 1
      containerId: overlay-toolbar__top
      floating: 0
      collapsed: 0
      displayed: 0
      snapOffset: {x: 0, y: 0}
      snapOffsetDelta: {x: 0, y: 0}
      snapCorner: 1
      id: unity-search-toolbar
      index: 1
      layout: 1
      size: {x: 0, y: 0}
      sizeOverriden: 0
    - dockPosition: 0
      containerId: overlay-container--left
      floating: 0
      collapsed: 0
      displayed: 1
      snapOffset: {x: 0, y: 0}
      snapOffsetDelta: {x: 0, y: 0}
      snapCorner: 0
      id: unity-transform-toolbar
      index: 0
      layout: 2
      size: {x: 0, y: 0}
      sizeOverriden: 0
    - dockPosition: 0
      containerId: overlay-container--left
      floating: 0
      collapsed: 0
      displayed: 1
      snapOffset: {x: 0, y: 197}
      snapOffsetDelta: {x: 0, y: 0}
      snapCorner: 0
      id: unity-component-tools
      index: 1
      layout: 2
      size: {x: 0, y: 0}
      sizeOverriden: 0
    - dockPosition: 0
      containerId: overlay-container--right
      floating: 0
      collapsed: 0
      displayed: 1
      snapOffset: {x: 67.5, y: 86}
      snapOffsetDelta: {x: 0, y: 0}
      snapCorner: 0
      id: Orientation
      index: 0
      layout: 4
      size: {x: 0, y: 0}
      sizeOverriden: 0
    - dockPosition: 1
      containerId: overlay-container--right
      floating: 0
      collapsed: 0
      displayed: 0
      snapOffset: {x: 0, y: 0}
      snapOffsetDelta: {x: 0, y: 0}
      snapCorner: 0
      id: Scene View/Light Settings
      index: 0
      layout: 4
      size: {x: 0, y: 0}
      sizeOverriden: 0
    - dockPosition: 1
      containerId: overlay-container--right
      floating: 0
      collapsed: 0
      displayed: 0
      snapOffset: {x: 0, y: 0}
      snapOffsetDelta: {x: 0, y: 0}
      snapCorner: 0
      id: Scene View/Camera
      index: 1
      layout: 4
      size: {x: 0, y: 0}
      sizeOverriden: 0
    - dockPosition: 1
      containerId: overlay-container--right
      floating: 0
      collapsed: 0
      displayed: 0
      snapOffset: {x: 0, y: 0}
      snapOffsetDelta: {x: 0, y: 0}
      snapCorner: 0
      id: Scene View/Cloth Constraints
      index: 1
      layout: 4
      size: {x: 0, y: 0}
      sizeOverriden: 0
    - dockPosition: 1
      containerId: overlay-container--right
      floating: 0
      collapsed: 0
      displayed: 0
      snapOffset: {x: 0, y: 0}
      snapOffsetDelta: {x: 0, y: 0}
      snapCorner: 0
      id: Scene View/Cloth Collisions
      index: 2
      layout: 4
      size: {x: 0, y: 0}
      sizeOverriden: 0
    - dockPosition: 1
      containerId: overlay-container--right
      floating: 0
      collapsed: 0
      displayed: 0
      snapOffset: {x: 0, y: 0}
      snapOffsetDelta: {x: 0, y: 0}
      snapCorner: 0
      id: Scene View/Navmesh Display
      index: 4
      layout: 4
      size: {x: 0, y: 0}
      sizeOverriden: 0
    - dockPosition: 1
      containerId: overlay-container--right
      floating: 0
      collapsed: 0
      displayed: 0
      snapOffset: {x: 0, y: 0}
      snapOffsetDelta: {x: 0, y: 0}
      snapCorner: 0
      id: Scene View/Agent Display
      index: 5
      layout: 4
      size: {x: 0, y: 0}
      sizeOverriden: 0
    - dockPosition: 1
      containerId: overlay-container--right
      floating: 0
      collapsed: 0
      displayed: 0
      snapOffset: {x: 0, y: 0}
      snapOffsetDelta: {x: 0, y: 0}
      snapCorner: 0
      id: Scene View/Obstacle Display
      index: 6
      layout: 4
      size: {x: 0, y: 0}
      sizeOverriden: 0
    - dockPosition: 1
      containerId: overlay-container--right
      floating: 0
      collapsed: 0
      displayed: 0
      snapOffset: {x: 0, y: 0}
      snapOffsetDelta: {x: 0, y: 0}
      snapCorner: 0
      id: Scene View/Occlusion Culling
      index: 3
      layout: 4
      size: {x: 0, y: 0}
      sizeOverriden: 0
    - dockPosition: 1
      containerId: overlay-container--right
      floating: 0
      collapsed: 0
      displayed: 0
      snapOffset: {x: 0, y: 0}
      snapOffsetDelta: {x: 0, y: 0}
      snapCorner: 0
      id: Scene View/Physics Debugger
      index: 4
      layout: 4
      size: {x: 0, y: 0}
      sizeOverriden: 0
    - dockPosition: 1
      containerId: overlay-container--right
      floating: 0
      collapsed: 0
      displayed: 0
      snapOffset: {x: 0, y: 0}
      snapOffsetDelta: {x: 0, y: 0}
      snapCorner: 0
      id: Scene View/Scene Visibility
      index: 5
      layout: 4
      size: {x: 0, y: 0}
      sizeOverriden: 0
    - dockPosition: 1
      containerId: overlay-container--right
      floating: 0
      collapsed: 0
      displayed: 0
      snapOffset: {x: 0, y: 0}
      snapOffsetDelta: {x: 0, y: 0}
      snapCorner: 0
      id: Scene View/Particles
      index: 6
      layout: 4
      size: {x: 0, y: 0}
      sizeOverriden: 0
    - dockPosition: 1
      containerId: overlay-container--right
      floating: 0
      collapsed: 0
      displayed: 0
      snapOffset: {x: 0, y: 0}
      snapOffsetDelta: {x: 0, y: 0}
      snapCorner: 0
      id: Scene View/Tilemap
      index: 11
      layout: 4
      size: {x: 0, y: 0}
      sizeOverriden: 0
    - dockPosition: 1
      containerId: overlay-container--right
      floating: 0
      collapsed: 0
      displayed: 0
      snapOffset: {x: 0, y: 0}
      snapOffsetDelta: {x: 0, y: 0}
      snapCorner: 0
      id: Scene View/Tilemap Palette Helper
      index: 12
      layout: 4
      size: {x: 0, y: 0}
      sizeOverriden: 0
    - dockPosition: 1
      containerId: overlay-container--right
      floating: 0
      collapsed: 0
      displayed: 1
      snapOffset: {x: 48, y: 10}
      snapOffsetDelta: {x: 0, y: 0}
      snapCorner: 0
      id: UnityEditor.SceneViewCameraOverlay
      index: 9
      layout: 4
      size: {x: 0, y: 0}
      sizeOverriden: 0
    - dockPosition: 1
      containerId: overlay-container--right
      floating: 0
      collapsed: 0
      displayed: 0
      snapOffset: {x: 48, y: 48}
      snapOffsetDelta: {x: 0, y: 0}
      snapCorner: 0
      id: APV Overlay
      index: 7
      layout: 4
      size: {x: 0, y: 0}
      sizeOverriden: 0
    - dockPosition: 1
      containerId: overlay-container--right
      floating: 0
      collapsed: 0
      displayed: 0
      snapOffset: {x: 48, y: 48}
      snapOffsetDelta: {x: 0, y: 0}
      snapCorner: 0
      id: Scene View/TrailRenderer
      index: 7
      layout: 4
      size: {x: 0, y: 0}
      sizeOverriden: 0
    - dockPosition: 1
      containerId: overlay-container--right
      floating: 0
      collapsed: 0
      displayed: 1
      snapOffset: {x: -220, y: -232}
      snapOffsetDelta: {x: 0, y: 0}
      snapCorner: 3
      id: AINavigationOverlay
      index: 8
      layout: 4
      size: {x: 0, y: 0}
      sizeOverriden: 0
    - dockPosition: 1
      containerId: overlay-container--right
      floating: 0
      collapsed: 0
      displayed: 0
      snapOffset: {x: 48, y: 48}
      snapOffsetDelta: {x: 0, y: 0}
      snapCorner: 0
      id: unity-spline-inspector
      index: 9
      layout: 4
      size: {x: 0, y: 0}
      sizeOverriden: 0
    m_OverlaysVisible: 1
  m_WindowGUID: f48192587447b7b4ca9b7a5935333dd6
  m_Gizmos: 1
  m_OverrideSceneCullingMask: 6917529027641081856
  m_SceneIsLit: 1
  m_SceneLighting: 1
  m_2DMode: 0
  m_isRotationLocked: 0
  m_PlayAudio: 0
  m_AudioPlay: 0
  m_Position:
    m_Target: {x: 0, y: 1.3333335, z: 0}
    speed: 2
    m_Value: {x: 0, y: 1.3333335, z: 0}
  m_RenderMode: 0
  m_CameraMode:
    drawMode: 0
    name: Shaded
    section: Shading Mode
  m_ValidateTrueMetals: 0
  m_DoValidateTrueMetals: 0
  m_SceneViewState:
    m_AlwaysRefresh: 0
    showFog: 1
    showSkybox: 1
    showFlares: 1
    showImageEffects: 1
    showParticleSystems: 1
    showVisualEffectGraphs: 1
    m_FxEnabled: 1
  m_Grid:
    xGrid:
      m_Fade:
        m_Target: 0
        speed: 2
        m_Value: 0
      m_Color: {r: 0.5, g: 0.5, b: 0.5, a: 0.4}
      m_Pivot: {x: 0, y: 0, z: 0}
      m_Size: {x: 1, y: 1}
    yGrid:
      m_Fade:
        m_Target: 0
        speed: 2
        m_Value: 0
      m_Color: {r: 0.5, g: 0.5, b: 0.5, a: 0.4}
      m_Pivot: {x: 0, y: 0, z: 0}
      m_Size: {x: 1, y: 1}
    zGrid:
      m_Fade:
        m_Target: 0
        speed: 2
        m_Value: 0
      m_Color: {r: 0.5, g: 0.5, b: 0.5, a: 0.4}
      m_Pivot: {x: 0, y: 0, z: 0}
      m_Size: {x: 1, y: 1}
    m_ShowGrid: 0
    m_GridAxis: 1
    m_gridOpacity: 0.5
  m_Rotation:
    m_Target: {x: 0.7071068, y: 0, z: 0, w: 0.7071068}
    speed: 2
    m_Value: {x: 0.7071068, y: 0, z: 0, w: 0.7071068}
  m_Size:
    m_Target: 28.94185
    speed: 2
    m_Value: 28.94185
  m_Ortho:
    m_Target: 1
    speed: 2
    m_Value: 1
  m_CameraSettings:
    m_Speed: 0.8855999
    m_SpeedNormalized: 0.43999997
    m_SpeedMin: 0.01
    m_SpeedMax: 2
    m_EasingEnabled: 1
    m_EasingDuration: 0.4
    m_AccelerationEnabled: 1
    m_FieldOfViewHorizontalOrVertical: 60
    m_NearClip: 0.03
    m_FarClip: 10000
    m_DynamicClip: 1
    m_OcclusionCulling: 0
  m_LastSceneViewRotation: {x: 0, y: 0, z: 0, w: 0}
  m_LastSceneViewOrtho: 0
  m_ReplacementShader: {fileID: 0}
  m_ReplacementString: 
  m_SceneVisActive: 1
  m_LastLockedObject: {fileID: 0}
  m_ViewIsLockedToObject: 0
--- !u!114 &10
MonoBehaviour:
  m_ObjectHideFlags: 52
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 12914, guid: 0000000000000000e000000000000000, type: 0}
  m_Name: 
  m_EditorClassIdentifier: 
  m_MinSize: {x: 100, y: 100}
  m_MaxSize: {x: 4000, y: 4000}
  m_TitleContent:
    m_Text: Animator
    m_Image: {fileID: -1673928668082335149, guid: 0000000000000000d000000000000000, type: 0}
    m_Tooltip: 
  m_Pos:
    serializedVersion: 2
    x: 546
    y: 73
    width: 1466
    height: 763
  m_SerializedDataModeController:
    m_DataMode: 0
    m_PreferredDataMode: 0
    m_SupportedDataModes: 
    isAutomatic: 1
  m_ViewDataDictionary: {fileID: 0}
  m_OverlayCanvas:
    m_LastAppliedPresetName: Default
    m_SaveData: []
    m_OverlaysVisible: 1
  m_ViewTransforms:
    m_KeySerializationHelper:
    - {fileID: 3721131351775251420, guid: fbe27581ff6b8324a95c01cc3d2d2548, type: 2}
    m_ValueSerializationHelper:
    - e00: 1
      e01: 0
      e02: 0
      e03: 0
      e10: 0
      e11: 1
      e12: 0
      e13: 0
      e20: 0
      e21: 0
      e22: 1
      e23: 0
      e30: 0
      e31: 0
      e32: 0
      e33: 1
  m_PreviewAnimator: {fileID: 0}
  m_AnimatorController: {fileID: 9100000, guid: fbe27581ff6b8324a95c01cc3d2d2548, type: 2}
  m_BreadCrumbs:
  - m_Target: {fileID: 2390719094264726621, guid: fbe27581ff6b8324a95c01cc3d2d2548, type: 2}
    m_ScrollPosition: {x: 0, y: 0}
  - m_Target: {fileID: 3721131351775251420, guid: fbe27581ff6b8324a95c01cc3d2d2548, type: 2}
    m_ScrollPosition: {x: 0, y: 0}
  stateMachineGraph: {fileID: 0}
  stateMachineGraphGUI: {fileID: 0}
  blendTreeGraph: {fileID: 0}
  blendTreeGraphGUI: {fileID: 0}
  m_AutoLiveLink: 1
  m_MiniTool: 0
  m_LockTracker:
    m_IsLocked: 0
  m_CurrentEditor: 1
  m_LayerEditor:
    m_SelectedLayerIndex: 0
--- !u!114 &11
MonoBehaviour:
  m_ObjectHideFlags: 52
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 1
  m_Script: {fileID: 12006, guid: 0000000000000000e000000000000000, type: 0}
  m_Name: ProjectBrowser
  m_EditorClassIdentifier: 
  m_Children: []
  m_Position:
    serializedVersion: 2
    x: 0
    y: 580
    width: 1505
    height: 279
  m_MinSize: {x: 231, y: 271}
  m_MaxSize: {x: 10001, y: 10021}
  m_ActualView: {fileID: 12}
  m_Panes:
  - {fileID: 12}
  - {fileID: 13}
  m_Selected: 0
  m_LastSelected: 1
--- !u!114 &12
MonoBehaviour:
  m_ObjectHideFlags: 52
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 1
  m_Script: {fileID: 12014, guid: 0000000000000000e000000000000000, type: 0}
  m_Name: 
  m_EditorClassIdentifier: 
  m_MinSize: {x: 230, y: 250}
  m_MaxSize: {x: 10000, y: 10000}
  m_TitleContent:
    m_Text: Project
    m_Image: {fileID: -5467254957812901981, guid: 0000000000000000d000000000000000, type: 0}
    m_Tooltip: 
  m_Pos:
    serializedVersion: 2
    x: 6
    y: 699
    width: 1504
    height: 258
  m_SerializedDataModeController:
    m_DataMode: 0
    m_PreferredDataMode: 0
    m_SupportedDataModes: 
    isAutomatic: 1
  m_ViewDataDictionary: {fileID: 0}
  m_OverlayCanvas:
    m_LastAppliedPresetName: Default
    m_SaveData: []
    m_OverlaysVisible: 1
  m_SearchFilter:
    m_NameFilter: 
    m_ClassNames: []
    m_AssetLabels: []
    m_AssetBundleNames: []
    m_ReferencingInstanceIDs: 
    m_SceneHandles: 
    m_ShowAllHits: 0
    m_SkipHidden: 0
    m_SearchArea: 1
    m_Folders:
    - Assets/Prefab
    m_Globs: []
    m_OriginalText: 
    m_ImportLogFlags: 0
    m_FilterByTypeIntersection: 0
  m_ViewMode: 1
  m_StartGridSize: 64
  m_LastFolders:
  - Assets/Prefab
  m_LastFoldersGridSize: -1
  m_LastProjectPath: /mnt/IgniteDrive/Proyectos_Unity/SistemaLogistico_AMR
  m_LockTracker:
    m_IsLocked: 0
  m_FolderTreeState:
    scrollPos: {x: 0, y: 51}
    m_SelectedIDs: ac630000
    m_LastClickedID: 25516
    m_ExpandedIDs: 000000006651000000ca9a3bffffff7f
    m_RenameOverlay:
      m_UserAcceptedRename: 0
      m_Name: 
      m_OriginalName: 
      m_EditFieldRect:
        serializedVersion: 2
        x: 0
        y: 0
        width: 0
        height: 0
      m_UserData: 0
      m_IsWaitingForDelay: 0
      m_IsRenaming: 0
      m_OriginalEventType: 11
      m_IsRenamingFilename: 1
      m_ClientGUIView: {fileID: 0}
    m_SearchString: 
    m_CreateAssetUtility:
      m_EndAction: {fileID: 0}
      m_InstanceID: 0
      m_Path: 
      m_Icon: {fileID: 0}
      m_ResourceFile: 
  m_AssetTreeState:
    scrollPos: {x: 0, y: 0}
    m_SelectedIDs: 
    m_LastClickedID: 0
    m_ExpandedIDs: 000000006651000000ca9a3bffffff7f
    m_RenameOverlay:
      m_UserAcceptedRename: 0
      m_Name: 
      m_OriginalName: 
      m_EditFieldRect:
        serializedVersion: 2
        x: 0
        y: 0
        width: 0
        height: 0
      m_UserData: 0
      m_IsWaitingForDelay: 0
      m_IsRenaming: 0
      m_OriginalEventType: 11
      m_IsRenamingFilename: 1
      m_ClientGUIView: {fileID: 0}
    m_SearchString: 
    m_CreateAssetUtility:
      m_EndAction: {fileID: 0}
      m_InstanceID: 0
      m_Path: 
      m_Icon: {fileID: 0}
      m_ResourceFile: 
  m_ListAreaState:
    m_SelectedInstanceIDs: 
    m_LastClickedInstanceID: 0
    m_HadKeyboardFocusLastEvent: 0
    m_ExpandedInstanceIDs: c623000022450000985100002c520000a4510000744f0000e6540000ec54000050740000d2770000f080000032810000f07e000068020000067c00006c02000000000000025400002c510000
    m_RenameOverlay:
      m_UserAcceptedRename: 0
      m_Name: 
      m_OriginalName: 
      m_EditFieldRect:
        serializedVersion: 2
        x: 0
        y: 0
        width: 0
        height: 0
      m_UserData: 0
      m_IsWaitingForDelay: 0
      m_IsRenaming: 0
      m_OriginalEventType: 11
      m_IsRenamingFilename: 1
      m_ClientGUIView: {fileID: 0}
    m_CreateAssetUtility:
      m_EndAction: {fileID: 0}
      m_InstanceID: 0
      m_Path: 
      m_Icon: {fileID: 0}
      m_ResourceFile: 
    m_NewAssetIndexInList: -1
    m_ScrollPosition: {x: 0, y: 0}
    m_GridSize: 64
  m_SkipHiddenPackages: 0
  m_DirectoriesAreaWidth: 207
--- !u!114 &13
MonoBehaviour:
  m_ObjectHideFlags: 52
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 1
  m_Script: {fileID: 12003, guid: 0000000000000000e000000000000000, type: 0}
  m_Name: 
  m_EditorClassIdentifier: 
  m_MinSize: {x: 100, y: 100}
  m_MaxSize: {x: 4000, y: 4000}
  m_TitleContent:
    m_Text: Console
    m_Image: {fileID: -4327648978806127646, guid: 0000000000000000d000000000000000, type: 0}
    m_Tooltip: 
  m_Pos:
    serializedVersion: 2
    x: 6
    y: 699
    width: 1504
    height: 258
  m_SerializedDataModeController:
    m_DataMode: 0
    m_PreferredDataMode: 0
    m_SupportedDataModes: 
    isAutomatic: 1
  m_ViewDataDictionary: {fileID: 0}
  m_OverlayCanvas:
    m_LastAppliedPresetName: Default
    m_SaveData: []
    m_OverlaysVisible: 1
--- !u!114 &14
MonoBehaviour:
  m_ObjectHideFlags: 52
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 1
  m_Script: {fileID: 12006, guid: 0000000000000000e000000000000000, type: 0}
  m_Name: InspectorWindow
  m_EditorClassIdentifier: 
  m_Children: []
  m_Position:
    serializedVersion: 2
    x: 1505
    y: 0
    width: 403
    height: 859
  m_MinSize: {x: 275, y: 50}
  m_MaxSize: {x: 4000, y: 4000}
  m_ActualView: {fileID: 15}
  m_Panes:
  - {fileID: 15}
  m_Selected: 0
  m_LastSelected: 0
--- !u!114 &15
MonoBehaviour:
  m_ObjectHideFlags: 52
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 1
  m_Script: {fileID: 12019, guid: 0000000000000000e000000000000000, type: 0}
  m_Name: 
  m_EditorClassIdentifier: 
  m_MinSize: {x: 275, y: 50}
  m_MaxSize: {x: 4000, y: 4000}
  m_TitleContent:
    m_Text: Inspector
    m_Image: {fileID: -2667387946076563598, guid: 0000000000000000d000000000000000, type: 0}
    m_Tooltip: 
  m_Pos:
    serializedVersion: 2
    x: 1511
    y: 119
    width: 402
    height: 838
  m_SerializedDataModeController:
    m_DataMode: 0
    m_PreferredDataMode: 0
    m_SupportedDataModes: 
    isAutomatic: 1
  m_ViewDataDictionary: {fileID: 0}
  m_OverlayCanvas:
    m_LastAppliedPresetName: Default
    m_SaveData: []
    m_OverlaysVisible: 1
  m_ObjectsLockedBeforeSerialization: []
  m_InstanceIDsLockedBeforeSerialization: 
  m_PreviewResizer:
    m_CachedPref: -161
    m_ControlHash: -371814159
    m_PrefName: Preview_InspectorPreview
  m_LastInspectedObjectInstanceID: -1
  m_LastVerticalScrollValue: 0
  m_GlobalObjectId: 
  m_InspectorMode: 0
  m_LockTracker:
    m_IsLocked: 0
  m_PreviewWindow: {fileID: 0}
````

## File: Assets/Scripts/RobotAMR.cs
````csharp
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class RobotAMR : MonoBehaviour
{
    /* Máquina de Estados Finitos (FSM) para blindar el código contra errores lógicos, enum es una lista restrictiva.
    Le prohíbe a la variable estadoActual tener cualquier otro valor que no sea uno de esos tres.*/
    public enum EstadoRobot { Inactivo, En_Transito, Extrayendo, Transportando, Interrumpido, Entregando }
    
    [Header("Panel de Control (Solo lectura)")]
    public EstadoRobot estadoActual = EstadoRobot.Inactivo;
    /*El NavMeshAgent es el motor físico de Unity. Pero nota la variable Pedido. Tu cilindro físico no conoce el archivo JSON completo ni le interesa.
    Solo conoce la estructura de un pedido aislado que le entrega el Gestor, manteniendo el código limpio y modular.*/
    private NavMeshAgent agente;
    private Pedido pedidoActual; // Conoce la estructura, pero no toda la base de datos

    [Header("Referencias (LIFO)")]
    public RobotManagerEVE managerEVE;
    public Transform zonaTransferencia;
    private Transform destinoActual;

    void Awake()
    {
        agente = GetComponent<NavMeshAgent>();
    }

    public void AsignarMision(Pedido nuevoPedido)
    {
        /* Regla de seguridad: Solo acepta misiones si está inactivo
        el return funciona como una pared que protege a la mision actual*/
        if (estadoActual != EstadoRobot.Inactivo) return;

        pedidoActual = nuevoPedido;
        
        if (pedidoActual.peso_kg > 900)
        {
            DashboardUI.Instance?.RegistrarLog("<color=red>ALERTA CRÍTICA: Batería insuficiente para pedido masivo.</color>");
            estadoActual = EstadoRobot.Interrumpido;
            agente.isStopped = true;
            return;
        }

        /*Busca en el mundo 3D el GameObject que se llame exactamente como dice el JSON*/
        GameObject destino = GameObject.Find(pedidoActual.coordenada_bodega);

        if (destino != null)
        {
            destinoActual = destino.transform;
            estadoActual = EstadoRobot.En_Transito;
            agente.SetDestination(destinoActual.position); // Da la orden de moverse
            Debug.Log("<color=cyan>AMR Desplegado:</color> Viajando a " + destino.name + " para buscar " + pedidoActual.codigo_sku);
            DashboardUI.Instance?.RegistrarLog("AMR Desplegado: Viajando a " + destino.name);
        }
        else
        {
            Debug.LogError("Alerta Logística: No existe la coordenada " + pedidoActual.coordenada_bodega + " en la bodega física.");
            DashboardUI.Instance?.RegistrarLog("Alerta: No existe la coordenada " + pedidoActual.coordenada_bodega);
        }
    }

    IEnumerator ProcesoExtraccion()
    {
        // Simulación de tiempo de extracción de carga
        yield return new WaitForSeconds(2f);
        
        if (zonaTransferencia != null)
        {
            estadoActual = EstadoRobot.Transportando;
            agente.SetDestination(zonaTransferencia.position);
            Debug.Log("<color=cyan>AMR Retornando:</color> Llevando estiba a la Zona de Transferencia.");
            DashboardUI.Instance?.RegistrarLog("AMR Retornando: Llevando estiba a Zona de Transferencia.");
        }
        else
        {
            Debug.LogError("AMR Error: Falta asignar la Zona de Transferencia en el Inspector.");
            DashboardUI.Instance?.RegistrarLog("AMR Error: Falta Zona de Transferencia.");
        }
    }

    void Update()
    {
        if (estadoActual == EstadoRobot.En_Transito)
        {
            if (!agente.pathPending && agente.remainingDistance <= 1.0f)
            {
                estadoActual = EstadoRobot.Extrayendo;
                Debug.Log("<color=yellow>Destino Alcanzado:</color> Extrayendo estiba de " + pedidoActual.peso_kg + " kg.");
                DashboardUI.Instance?.RegistrarLog("Destino Alcanzado: Extrayendo estiba.");
                StartCoroutine(ProcesoExtraccion());
            }
        }
        else if (estadoActual == EstadoRobot.Transportando)
        {
            // Chequeo de distancia cuando el robot está retornando
            if (zonaTransferencia != null && Vector3.Distance(transform.position, zonaTransferencia.position) < 0.5f)
            {
                estadoActual = EstadoRobot.Entregando;
                Debug.Log("<color=magenta>LIFO:</color> Entregando estiba en Zona de Transferencia.");
                DashboardUI.Instance?.RegistrarLog("LIFO: Entregando estiba en Zona de Transferencia.");
                
                if (managerEVE != null)
                {
                    managerEVE.ConfirmarEntregaExitosa();
                }

                estadoActual = EstadoRobot.Inactivo;
            }
        }
    }
}
````

## File: UserSettings/Layouts/default-2022.dwlt
````
%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &1
MonoBehaviour:
  m_ObjectHideFlags: 52
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 1
  m_Script: {fileID: 12004, guid: 0000000000000000e000000000000000, type: 0}
  m_Name: 
  m_EditorClassIdentifier: 
  m_PixelRect:
    serializedVersion: 2
    x: 6
    y: 62
    width: 1908
    height: 909
  m_ShowMode: 4
  m_Title: Game
  m_RootView: {fileID: 2}
  m_MinSize: {x: 875, y: 321}
  m_MaxSize: {x: 10000, y: 10000}
  m_Maximized: 0
--- !u!114 &2
MonoBehaviour:
  m_ObjectHideFlags: 52
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 1
  m_Script: {fileID: 12008, guid: 0000000000000000e000000000000000, type: 0}
  m_Name: 
  m_EditorClassIdentifier: 
  m_Children:
  - {fileID: 3}
  - {fileID: 5}
  - {fileID: 4}
  m_Position:
    serializedVersion: 2
    x: 0
    y: 0
    width: 1908
    height: 909
  m_MinSize: {x: 875, y: 300}
  m_MaxSize: {x: 10000, y: 10000}
  m_UseTopView: 1
  m_TopViewHeight: 30
  m_UseBottomView: 1
  m_BottomViewHeight: 20
--- !u!114 &3
MonoBehaviour:
  m_ObjectHideFlags: 52
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 1
  m_Script: {fileID: 12011, guid: 0000000000000000e000000000000000, type: 0}
  m_Name: 
  m_EditorClassIdentifier: 
  m_Children: []
  m_Position:
    serializedVersion: 2
    x: 0
    y: 0
    width: 1908
    height: 30
  m_MinSize: {x: 0, y: 0}
  m_MaxSize: {x: 0, y: 0}
  m_LastLoadedLayoutName: 
--- !u!114 &4
MonoBehaviour:
  m_ObjectHideFlags: 52
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 1
  m_Script: {fileID: 12042, guid: 0000000000000000e000000000000000, type: 0}
  m_Name: 
  m_EditorClassIdentifier: 
  m_Children: []
  m_Position:
    serializedVersion: 2
    x: 0
    y: 889
    width: 1908
    height: 20
  m_MinSize: {x: 0, y: 0}
  m_MaxSize: {x: 0, y: 0}
--- !u!114 &5
MonoBehaviour:
  m_ObjectHideFlags: 52
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 12010, guid: 0000000000000000e000000000000000, type: 0}
  m_Name: 
  m_EditorClassIdentifier: 
  m_Children:
  - {fileID: 6}
  m_Position:
    serializedVersion: 2
    x: 0
    y: 30
    width: 1908
    height: 859
  m_MinSize: {x: 300, y: 100}
  m_MaxSize: {x: 24288, y: 16192}
  vertical: 1
  controlID: 45163
  draggingID: 0
--- !u!114 &6
MonoBehaviour:
  m_ObjectHideFlags: 52
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 1
  m_Script: {fileID: 12010, guid: 0000000000000000e000000000000000, type: 0}
  m_Name: 
  m_EditorClassIdentifier: 
  m_Children:
  - {fileID: 7}
  - {fileID: 12}
  m_Position:
    serializedVersion: 2
    x: 0
    y: 0
    width: 1908
    height: 859
  m_MinSize: {x: 300, y: 100}
  m_MaxSize: {x: 24288, y: 16192}
  vertical: 0
  controlID: 45164
  draggingID: 0
--- !u!114 &7
MonoBehaviour:
  m_ObjectHideFlags: 52
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 1
  m_Script: {fileID: 12010, guid: 0000000000000000e000000000000000, type: 0}
  m_Name: 
  m_EditorClassIdentifier: 
  m_Children:
  - {fileID: 8}
  - {fileID: 11}
  m_Position:
    serializedVersion: 2
    x: 0
    y: 0
    width: 1505
    height: 859
  m_MinSize: {x: 200, y: 100}
  m_MaxSize: {x: 16192, y: 16192}
  vertical: 1
  controlID: 45165
  draggingID: 0
--- !u!114 &8
MonoBehaviour:
  m_ObjectHideFlags: 52
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 1
  m_Script: {fileID: 12010, guid: 0000000000000000e000000000000000, type: 0}
  m_Name: 
  m_EditorClassIdentifier: 
  m_Children:
  - {fileID: 9}
  - {fileID: 10}
  m_Position:
    serializedVersion: 2
    x: 0
    y: 0
    width: 1505
    height: 580
  m_MinSize: {x: 200, y: 50}
  m_MaxSize: {x: 16192, y: 8096}
  vertical: 0
  controlID: 45166
  draggingID: 0
--- !u!114 &9
MonoBehaviour:
  m_ObjectHideFlags: 52
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 1
  m_Script: {fileID: 12006, guid: 0000000000000000e000000000000000, type: 0}
  m_Name: 
  m_EditorClassIdentifier: 
  m_Children: []
  m_Position:
    serializedVersion: 2
    x: 0
    y: 0
    width: 409
    height: 580
  m_MinSize: {x: 201, y: 221}
  m_MaxSize: {x: 4001, y: 4021}
  m_ActualView: {fileID: 14}
  m_Panes:
  - {fileID: 14}
  m_Selected: 0
  m_LastSelected: 0
--- !u!114 &10
MonoBehaviour:
  m_ObjectHideFlags: 52
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 1
  m_Script: {fileID: 12006, guid: 0000000000000000e000000000000000, type: 0}
  m_Name: GameView
  m_EditorClassIdentifier: 
  m_Children: []
  m_Position:
    serializedVersion: 2
    x: 409
    y: 0
    width: 1096
    height: 580
  m_MinSize: {x: 202, y: 221}
  m_MaxSize: {x: 4002, y: 4021}
  m_ActualView: {fileID: 13}
  m_Panes:
  - {fileID: 13}
  - {fileID: 15}
  - {fileID: 16}
  m_Selected: 0
  m_LastSelected: 1
--- !u!114 &11
MonoBehaviour:
  m_ObjectHideFlags: 52
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 1
  m_Script: {fileID: 12006, guid: 0000000000000000e000000000000000, type: 0}
  m_Name: ProjectBrowser
  m_EditorClassIdentifier: 
  m_Children: []
  m_Position:
    serializedVersion: 2
    x: 0
    y: 580
    width: 1505
    height: 279
  m_MinSize: {x: 231, y: 271}
  m_MaxSize: {x: 10001, y: 10021}
  m_ActualView: {fileID: 17}
  m_Panes:
  - {fileID: 17}
  - {fileID: 18}
  m_Selected: 0
  m_LastSelected: 1
--- !u!114 &12
MonoBehaviour:
  m_ObjectHideFlags: 52
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 1
  m_Script: {fileID: 12006, guid: 0000000000000000e000000000000000, type: 0}
  m_Name: InspectorWindow
  m_EditorClassIdentifier: 
  m_Children: []
  m_Position:
    serializedVersion: 2
    x: 1505
    y: 0
    width: 403
    height: 859
  m_MinSize: {x: 276, y: 71}
  m_MaxSize: {x: 4001, y: 4021}
  m_ActualView: {fileID: 19}
  m_Panes:
  - {fileID: 19}
  m_Selected: 0
  m_LastSelected: 0
--- !u!114 &13
MonoBehaviour:
  m_ObjectHideFlags: 52
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 1
  m_Script: {fileID: 12015, guid: 0000000000000000e000000000000000, type: 0}
  m_Name: 
  m_EditorClassIdentifier: 
  m_MinSize: {x: 200, y: 200}
  m_MaxSize: {x: 4000, y: 4000}
  m_TitleContent:
    m_Text: Game
    m_Image: {fileID: -6423792434712278376, guid: 0000000000000000d000000000000000, type: 0}
    m_Tooltip: 
  m_Pos:
    serializedVersion: 2
    x: 415
    y: 119
    width: 1094
    height: 559
  m_SerializedDataModeController:
    m_DataMode: 0
    m_PreferredDataMode: 0
    m_SupportedDataModes: 
    isAutomatic: 1
  m_ViewDataDictionary: {fileID: 0}
  m_OverlayCanvas:
    m_LastAppliedPresetName: Default
    m_SaveData: []
    m_OverlaysVisible: 1
  m_SerializedViewNames: []
  m_SerializedViewValues: []
  m_PlayModeViewName: GameView
  m_ShowGizmos: 0
  m_TargetDisplay: 0
  m_ClearColor: {r: 0, g: 0, b: 0, a: 0}
  m_TargetSize: {x: 956, y: 538}
  m_TextureFilterMode: 0
  m_TextureHideFlags: 61
  m_RenderIMGUI: 1
  m_EnterPlayModeBehavior: 0
  m_UseMipMap: 0
  m_VSyncEnabled: 0
  m_Gizmos: 0
  m_Stats: 0
  m_SelectedSizes: 01000000000000000000000000000000000000000000000000000000000000000000000000000000
  m_ZoomArea:
    m_HRangeLocked: 0
    m_VRangeLocked: 0
    hZoomLockedByDefault: 0
    vZoomLockedByDefault: 0
    m_HBaseRangeMin: -478
    m_HBaseRangeMax: 478
    m_VBaseRangeMin: -269
    m_VBaseRangeMax: 269
    m_HAllowExceedBaseRangeMin: 1
    m_HAllowExceedBaseRangeMax: 1
    m_VAllowExceedBaseRangeMin: 1
    m_VAllowExceedBaseRangeMax: 1
    m_ScaleWithWindow: 0
    m_HSlider: 0
    m_VSlider: 0
    m_IgnoreScrollWheelUntilClicked: 0
    m_EnableMouseInput: 1
    m_EnableSliderZoomHorizontal: 0
    m_EnableSliderZoomVertical: 0
    m_UniformScale: 1
    m_UpDirection: 1
    m_DrawArea:
      serializedVersion: 2
      x: 0
      y: 21
      width: 1094
      height: 538
    m_Scale: {x: 1, y: 1}
    m_Translation: {x: 547, y: 269}
    m_MarginLeft: 0
    m_MarginRight: 0
    m_MarginTop: 0
    m_MarginBottom: 0
    m_LastShownAreaInsideMargins:
      serializedVersion: 2
      x: -547
      y: -269
      width: 1094
      height: 538
    m_MinimalGUI: 1
  m_defaultScale: 1
  m_LastWindowPixelSize: {x: 1094, y: 559}
  m_ClearInEditMode: 1
  m_NoCameraWarning: 1
  m_LowResolutionForAspectRatios: 01000000000000000000
  m_XRRenderMode: 0
  m_RenderTexture: {fileID: 0}
  m_showToolbar: 1
--- !u!114 &14
MonoBehaviour:
  m_ObjectHideFlags: 52
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 1
  m_Script: {fileID: 12061, guid: 0000000000000000e000000000000000, type: 0}
  m_Name: 
  m_EditorClassIdentifier: 
  m_MinSize: {x: 200, y: 200}
  m_MaxSize: {x: 4000, y: 4000}
  m_TitleContent:
    m_Text: Hierarchy
    m_Image: {fileID: 7966133145522015247, guid: 0000000000000000d000000000000000, type: 0}
    m_Tooltip: 
  m_Pos:
    serializedVersion: 2
    x: 6
    y: 119
    width: 408
    height: 559
  m_SerializedDataModeController:
    m_DataMode: 0
    m_PreferredDataMode: 0
    m_SupportedDataModes: 
    isAutomatic: 1
  m_ViewDataDictionary: {fileID: 0}
  m_OverlayCanvas:
    m_LastAppliedPresetName: Default
    m_SaveData: []
    m_OverlaysVisible: 1
  m_SceneHierarchy:
    m_TreeViewState:
      scrollPos: {x: 0, y: 0}
      m_SelectedIDs: 
      m_LastClickedID: 0
      m_ExpandedIDs: 8a79feff042dffff2efbffff104f0000424f00006a4f0000e04f0000ea500000
      m_RenameOverlay:
        m_UserAcceptedRename: 0
        m_Name: 
        m_OriginalName: 
        m_EditFieldRect:
          serializedVersion: 2
          x: 0
          y: 0
          width: 0
          height: 0
        m_UserData: 0
        m_IsWaitingForDelay: 0
        m_IsRenaming: 0
        m_OriginalEventType: 11
        m_IsRenamingFilename: 0
        m_ClientGUIView: {fileID: 9}
      m_SearchString: 
    m_ExpandedScenes: []
    m_CurrenRootInstanceID: 0
    m_LockTracker:
      m_IsLocked: 0
    m_CurrentSortingName: TransformSorting
  m_WindowGUID: 4c969a2b90040154d917609493e03593
--- !u!114 &15
MonoBehaviour:
  m_ObjectHideFlags: 52
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 12013, guid: 0000000000000000e000000000000000, type: 0}
  m_Name: 
  m_EditorClassIdentifier: 
  m_MinSize: {x: 100, y: 100}
  m_MaxSize: {x: 4000, y: 4000}
  m_TitleContent:
    m_Text: Scene
    m_Image: {fileID: 2593428753322112591, guid: 0000000000000000d000000000000000, type: 0}
    m_Tooltip: 
  m_Pos:
    serializedVersion: 2
    x: 415
    y: 119
    width: 1094
    height: 559
  m_SerializedDataModeController:
    m_DataMode: 0
    m_PreferredDataMode: 0
    m_SupportedDataModes: 
    isAutomatic: 1
  m_ViewDataDictionary: {fileID: 0}
  m_OverlayCanvas:
    m_LastAppliedPresetName: Default
    m_SaveData:
    - dockPosition: 0
      containerId: overlay-toolbar__top
      floating: 0
      collapsed: 0
      displayed: 1
      snapOffset: {x: -179, y: -26}
      snapOffsetDelta: {x: 0, y: 0}
      snapCorner: 3
      id: Tool Settings
      index: 0
      layout: 1
      size: {x: 0, y: 0}
      sizeOverriden: 0
    - dockPosition: 0
      containerId: overlay-toolbar__top
      floating: 0
      collapsed: 0
      displayed: 1
      snapOffset: {x: 169, y: 0}
      snapOffsetDelta: {x: 0, y: 0}
      snapCorner: 0
      id: unity-grid-and-snap-toolbar
      index: 1
      layout: 1
      size: {x: 0, y: 0}
      sizeOverriden: 0
    - dockPosition: 1
      containerId: overlay-toolbar__top
      floating: 0
      collapsed: 0
      displayed: 1
      snapOffset: {x: 0, y: 0}
      snapOffsetDelta: {x: 0, y: 0}
      snapCorner: 0
      id: unity-scene-view-toolbar
      index: 0
      layout: 1
      size: {x: 0, y: 0}
      sizeOverriden: 0
    - dockPosition: 1
      containerId: overlay-toolbar__top
      floating: 0
      collapsed: 0
      displayed: 0
      snapOffset: {x: 0, y: 0}
      snapOffsetDelta: {x: 0, y: 0}
      snapCorner: 1
      id: unity-search-toolbar
      index: 1
      layout: 1
      size: {x: 0, y: 0}
      sizeOverriden: 0
    - dockPosition: 0
      containerId: overlay-container--left
      floating: 0
      collapsed: 0
      displayed: 1
      snapOffset: {x: 0, y: 0}
      snapOffsetDelta: {x: 0, y: 0}
      snapCorner: 0
      id: unity-transform-toolbar
      index: 0
      layout: 2
      size: {x: 0, y: 0}
      sizeOverriden: 0
    - dockPosition: 0
      containerId: overlay-container--left
      floating: 0
      collapsed: 0
      displayed: 1
      snapOffset: {x: 0, y: 197}
      snapOffsetDelta: {x: 0, y: 0}
      snapCorner: 0
      id: unity-component-tools
      index: 1
      layout: 2
      size: {x: 0, y: 0}
      sizeOverriden: 0
    - dockPosition: 0
      containerId: overlay-container--right
      floating: 0
      collapsed: 0
      displayed: 1
      snapOffset: {x: 67.5, y: 86}
      snapOffsetDelta: {x: 0, y: 0}
      snapCorner: 0
      id: Orientation
      index: 0
      layout: 4
      size: {x: 0, y: 0}
      sizeOverriden: 0
    - dockPosition: 1
      containerId: overlay-container--right
      floating: 0
      collapsed: 0
      displayed: 0
      snapOffset: {x: 0, y: 0}
      snapOffsetDelta: {x: 0, y: 0}
      snapCorner: 0
      id: Scene View/Light Settings
      index: 0
      layout: 4
      size: {x: 0, y: 0}
      sizeOverriden: 0
    - dockPosition: 1
      containerId: overlay-container--right
      floating: 0
      collapsed: 0
      displayed: 0
      snapOffset: {x: 0, y: 0}
      snapOffsetDelta: {x: 0, y: 0}
      snapCorner: 0
      id: Scene View/Camera
      index: 1
      layout: 4
      size: {x: 0, y: 0}
      sizeOverriden: 0
    - dockPosition: 1
      containerId: overlay-container--right
      floating: 0
      collapsed: 0
      displayed: 0
      snapOffset: {x: 0, y: 0}
      snapOffsetDelta: {x: 0, y: 0}
      snapCorner: 0
      id: Scene View/Cloth Constraints
      index: 1
      layout: 4
      size: {x: 0, y: 0}
      sizeOverriden: 0
    - dockPosition: 1
      containerId: overlay-container--right
      floating: 0
      collapsed: 0
      displayed: 0
      snapOffset: {x: 0, y: 0}
      snapOffsetDelta: {x: 0, y: 0}
      snapCorner: 0
      id: Scene View/Cloth Collisions
      index: 2
      layout: 4
      size: {x: 0, y: 0}
      sizeOverriden: 0
    - dockPosition: 1
      containerId: overlay-container--right
      floating: 0
      collapsed: 0
      displayed: 0
      snapOffset: {x: 0, y: 0}
      snapOffsetDelta: {x: 0, y: 0}
      snapCorner: 0
      id: Scene View/Navmesh Display
      index: 4
      layout: 4
      size: {x: 0, y: 0}
      sizeOverriden: 0
    - dockPosition: 1
      containerId: overlay-container--right
      floating: 0
      collapsed: 0
      displayed: 0
      snapOffset: {x: 0, y: 0}
      snapOffsetDelta: {x: 0, y: 0}
      snapCorner: 0
      id: Scene View/Agent Display
      index: 5
      layout: 4
      size: {x: 0, y: 0}
      sizeOverriden: 0
    - dockPosition: 1
      containerId: overlay-container--right
      floating: 0
      collapsed: 0
      displayed: 0
      snapOffset: {x: 0, y: 0}
      snapOffsetDelta: {x: 0, y: 0}
      snapCorner: 0
      id: Scene View/Obstacle Display
      index: 6
      layout: 4
      size: {x: 0, y: 0}
      sizeOverriden: 0
    - dockPosition: 1
      containerId: overlay-container--right
      floating: 0
      collapsed: 0
      displayed: 0
      snapOffset: {x: 0, y: 0}
      snapOffsetDelta: {x: 0, y: 0}
      snapCorner: 0
      id: Scene View/Occlusion Culling
      index: 3
      layout: 4
      size: {x: 0, y: 0}
      sizeOverriden: 0
    - dockPosition: 1
      containerId: overlay-container--right
      floating: 0
      collapsed: 0
      displayed: 0
      snapOffset: {x: 0, y: 0}
      snapOffsetDelta: {x: 0, y: 0}
      snapCorner: 0
      id: Scene View/Physics Debugger
      index: 4
      layout: 4
      size: {x: 0, y: 0}
      sizeOverriden: 0
    - dockPosition: 1
      containerId: overlay-container--right
      floating: 0
      collapsed: 0
      displayed: 0
      snapOffset: {x: 0, y: 0}
      snapOffsetDelta: {x: 0, y: 0}
      snapCorner: 0
      id: Scene View/Scene Visibility
      index: 5
      layout: 4
      size: {x: 0, y: 0}
      sizeOverriden: 0
    - dockPosition: 1
      containerId: overlay-container--right
      floating: 0
      collapsed: 0
      displayed: 0
      snapOffset: {x: 0, y: 0}
      snapOffsetDelta: {x: 0, y: 0}
      snapCorner: 0
      id: Scene View/Particles
      index: 6
      layout: 4
      size: {x: 0, y: 0}
      sizeOverriden: 0
    - dockPosition: 1
      containerId: overlay-container--right
      floating: 0
      collapsed: 0
      displayed: 0
      snapOffset: {x: 0, y: 0}
      snapOffsetDelta: {x: 0, y: 0}
      snapCorner: 0
      id: Scene View/Tilemap
      index: 11
      layout: 4
      size: {x: 0, y: 0}
      sizeOverriden: 0
    - dockPosition: 1
      containerId: overlay-container--right
      floating: 0
      collapsed: 0
      displayed: 0
      snapOffset: {x: 0, y: 0}
      snapOffsetDelta: {x: 0, y: 0}
      snapCorner: 0
      id: Scene View/Tilemap Palette Helper
      index: 12
      layout: 4
      size: {x: 0, y: 0}
      sizeOverriden: 0
    - dockPosition: 1
      containerId: overlay-container--right
      floating: 0
      collapsed: 0
      displayed: 1
      snapOffset: {x: 48, y: 10}
      snapOffsetDelta: {x: 0, y: 0}
      snapCorner: 0
      id: UnityEditor.SceneViewCameraOverlay
      index: 9
      layout: 4
      size: {x: 0, y: 0}
      sizeOverriden: 0
    - dockPosition: 1
      containerId: overlay-container--right
      floating: 0
      collapsed: 0
      displayed: 0
      snapOffset: {x: 48, y: 48}
      snapOffsetDelta: {x: 0, y: 0}
      snapCorner: 0
      id: APV Overlay
      index: 7
      layout: 4
      size: {x: 0, y: 0}
      sizeOverriden: 0
    - dockPosition: 1
      containerId: overlay-container--right
      floating: 0
      collapsed: 0
      displayed: 0
      snapOffset: {x: 48, y: 48}
      snapOffsetDelta: {x: 0, y: 0}
      snapCorner: 0
      id: Scene View/TrailRenderer
      index: 7
      layout: 4
      size: {x: 0, y: 0}
      sizeOverriden: 0
    - dockPosition: 1
      containerId: overlay-container--right
      floating: 0
      collapsed: 0
      displayed: 1
      snapOffset: {x: -220, y: -232}
      snapOffsetDelta: {x: 0, y: 0}
      snapCorner: 3
      id: AINavigationOverlay
      index: 8
      layout: 4
      size: {x: 0, y: 0}
      sizeOverriden: 0
    - dockPosition: 1
      containerId: overlay-container--right
      floating: 0
      collapsed: 0
      displayed: 0
      snapOffset: {x: 48, y: 48}
      snapOffsetDelta: {x: 0, y: 0}
      snapCorner: 0
      id: unity-spline-inspector
      index: 9
      layout: 4
      size: {x: 0, y: 0}
      sizeOverriden: 0
    m_OverlaysVisible: 1
  m_WindowGUID: f48192587447b7b4ca9b7a5935333dd6
  m_Gizmos: 1
  m_OverrideSceneCullingMask: 6917529027641081856
  m_SceneIsLit: 1
  m_SceneLighting: 1
  m_2DMode: 0
  m_isRotationLocked: 0
  m_PlayAudio: 0
  m_AudioPlay: 0
  m_Position:
    m_Target: {x: 0, y: 1.3333335, z: 0}
    speed: 2
    m_Value: {x: 0, y: 1.3333335, z: 0}
  m_RenderMode: 0
  m_CameraMode:
    drawMode: 0
    name: Shaded
    section: Shading Mode
  m_ValidateTrueMetals: 0
  m_DoValidateTrueMetals: 0
  m_SceneViewState:
    m_AlwaysRefresh: 0
    showFog: 1
    showSkybox: 1
    showFlares: 1
    showImageEffects: 1
    showParticleSystems: 1
    showVisualEffectGraphs: 1
    m_FxEnabled: 1
  m_Grid:
    xGrid:
      m_Fade:
        m_Target: 0
        speed: 2
        m_Value: 0
      m_Color: {r: 0.5, g: 0.5, b: 0.5, a: 0.4}
      m_Pivot: {x: 0, y: 0, z: 0}
      m_Size: {x: 1, y: 1}
    yGrid:
      m_Fade:
        m_Target: 0
        speed: 2
        m_Value: 0
      m_Color: {r: 0.5, g: 0.5, b: 0.5, a: 0.4}
      m_Pivot: {x: 0, y: 0, z: 0}
      m_Size: {x: 1, y: 1}
    zGrid:
      m_Fade:
        m_Target: 0
        speed: 2
        m_Value: 0
      m_Color: {r: 0.5, g: 0.5, b: 0.5, a: 0.4}
      m_Pivot: {x: 0, y: 0, z: 0}
      m_Size: {x: 1, y: 1}
    m_ShowGrid: 0
    m_GridAxis: 1
    m_gridOpacity: 0.5
  m_Rotation:
    m_Target: {x: 0.7071068, y: 0, z: 0, w: 0.7071068}
    speed: 2
    m_Value: {x: -0.7071068, y: 0, z: -0, w: -0.7071068}
  m_Size:
    m_Target: 28.94185
    speed: 2
    m_Value: 28.94185
  m_Ortho:
    m_Target: 1
    speed: 2
    m_Value: 1
  m_CameraSettings:
    m_Speed: 0.8855999
    m_SpeedNormalized: 0.43999997
    m_SpeedMin: 0.01
    m_SpeedMax: 2
    m_EasingEnabled: 1
    m_EasingDuration: 0.4
    m_AccelerationEnabled: 1
    m_FieldOfViewHorizontalOrVertical: 60
    m_NearClip: 0.03
    m_FarClip: 10000
    m_DynamicClip: 1
    m_OcclusionCulling: 0
  m_LastSceneViewRotation: {x: 0, y: 0, z: 0, w: 0}
  m_LastSceneViewOrtho: 0
  m_ReplacementShader: {fileID: 0}
  m_ReplacementString: 
  m_SceneVisActive: 1
  m_LastLockedObject: {fileID: 0}
  m_ViewIsLockedToObject: 0
--- !u!114 &16
MonoBehaviour:
  m_ObjectHideFlags: 52
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 12914, guid: 0000000000000000e000000000000000, type: 0}
  m_Name: 
  m_EditorClassIdentifier: 
  m_MinSize: {x: 100, y: 100}
  m_MaxSize: {x: 4000, y: 4000}
  m_TitleContent:
    m_Text: Animator
    m_Image: {fileID: -1673928668082335149, guid: 0000000000000000d000000000000000, type: 0}
    m_Tooltip: 
  m_Pos:
    serializedVersion: 2
    x: 546
    y: 73
    width: 1466
    height: 763
  m_SerializedDataModeController:
    m_DataMode: 0
    m_PreferredDataMode: 0
    m_SupportedDataModes: 
    isAutomatic: 1
  m_ViewDataDictionary: {fileID: 0}
  m_OverlayCanvas:
    m_LastAppliedPresetName: Default
    m_SaveData: []
    m_OverlaysVisible: 1
  m_ViewTransforms:
    m_KeySerializationHelper:
    - {fileID: 3721131351775251420, guid: fbe27581ff6b8324a95c01cc3d2d2548, type: 2}
    m_ValueSerializationHelper:
    - e00: 1
      e01: 0
      e02: 0
      e03: 0
      e10: 0
      e11: 1
      e12: 0
      e13: 0
      e20: 0
      e21: 0
      e22: 1
      e23: 0
      e30: 0
      e31: 0
      e32: 0
      e33: 1
  m_PreviewAnimator: {fileID: 0}
  m_AnimatorController: {fileID: 9100000, guid: fbe27581ff6b8324a95c01cc3d2d2548, type: 2}
  m_BreadCrumbs:
  - m_Target: {fileID: 2390719094264726621, guid: fbe27581ff6b8324a95c01cc3d2d2548, type: 2}
    m_ScrollPosition: {x: 0, y: 0}
  - m_Target: {fileID: 3721131351775251420, guid: fbe27581ff6b8324a95c01cc3d2d2548, type: 2}
    m_ScrollPosition: {x: 0, y: 0}
  stateMachineGraph: {fileID: 0}
  stateMachineGraphGUI: {fileID: 0}
  blendTreeGraph: {fileID: 0}
  blendTreeGraphGUI: {fileID: 0}
  m_AutoLiveLink: 1
  m_MiniTool: 0
  m_LockTracker:
    m_IsLocked: 0
  m_CurrentEditor: 1
  m_LayerEditor:
    m_SelectedLayerIndex: 0
--- !u!114 &17
MonoBehaviour:
  m_ObjectHideFlags: 52
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 1
  m_Script: {fileID: 12014, guid: 0000000000000000e000000000000000, type: 0}
  m_Name: 
  m_EditorClassIdentifier: 
  m_MinSize: {x: 230, y: 250}
  m_MaxSize: {x: 10000, y: 10000}
  m_TitleContent:
    m_Text: Project
    m_Image: {fileID: -5467254957812901981, guid: 0000000000000000d000000000000000, type: 0}
    m_Tooltip: 
  m_Pos:
    serializedVersion: 2
    x: 6
    y: 699
    width: 1504
    height: 258
  m_SerializedDataModeController:
    m_DataMode: 0
    m_PreferredDataMode: 0
    m_SupportedDataModes: 
    isAutomatic: 1
  m_ViewDataDictionary: {fileID: 0}
  m_OverlayCanvas:
    m_LastAppliedPresetName: Default
    m_SaveData: []
    m_OverlaysVisible: 1
  m_SearchFilter:
    m_NameFilter: 
    m_ClassNames: []
    m_AssetLabels: []
    m_AssetBundleNames: []
    m_ReferencingInstanceIDs: 
    m_SceneHandles: 
    m_ShowAllHits: 0
    m_SkipHidden: 0
    m_SearchArea: 1
    m_Folders:
    - Assets/Prefab
    m_Globs: []
    m_OriginalText: 
    m_ImportLogFlags: 0
    m_FilterByTypeIntersection: 0
  m_ViewMode: 1
  m_StartGridSize: 64
  m_LastFolders:
  - Assets/Prefab
  m_LastFoldersGridSize: -1
  m_LastProjectPath: /mnt/IgniteDrive/Proyectos_Unity/SistemaLogistico_AMR
  m_LockTracker:
    m_IsLocked: 0
  m_FolderTreeState:
    scrollPos: {x: 0, y: 51}
    m_SelectedIDs: ac630000
    m_LastClickedID: 25516
    m_ExpandedIDs: 000000006651000000ca9a3bffffff7f
    m_RenameOverlay:
      m_UserAcceptedRename: 0
      m_Name: 
      m_OriginalName: 
      m_EditFieldRect:
        serializedVersion: 2
        x: 0
        y: 0
        width: 0
        height: 0
      m_UserData: 0
      m_IsWaitingForDelay: 0
      m_IsRenaming: 0
      m_OriginalEventType: 11
      m_IsRenamingFilename: 1
      m_ClientGUIView: {fileID: 0}
    m_SearchString: 
    m_CreateAssetUtility:
      m_EndAction: {fileID: 0}
      m_InstanceID: 0
      m_Path: 
      m_Icon: {fileID: 0}
      m_ResourceFile: 
  m_AssetTreeState:
    scrollPos: {x: 0, y: 0}
    m_SelectedIDs: 
    m_LastClickedID: 0
    m_ExpandedIDs: 000000006651000000ca9a3bffffff7f
    m_RenameOverlay:
      m_UserAcceptedRename: 0
      m_Name: 
      m_OriginalName: 
      m_EditFieldRect:
        serializedVersion: 2
        x: 0
        y: 0
        width: 0
        height: 0
      m_UserData: 0
      m_IsWaitingForDelay: 0
      m_IsRenaming: 0
      m_OriginalEventType: 11
      m_IsRenamingFilename: 1
      m_ClientGUIView: {fileID: 0}
    m_SearchString: 
    m_CreateAssetUtility:
      m_EndAction: {fileID: 0}
      m_InstanceID: 0
      m_Path: 
      m_Icon: {fileID: 0}
      m_ResourceFile: 
  m_ListAreaState:
    m_SelectedInstanceIDs: 
    m_LastClickedInstanceID: 0
    m_HadKeyboardFocusLastEvent: 0
    m_ExpandedInstanceIDs: c623000022450000985100002c520000a4510000744f0000e6540000ec54000050740000d2770000f080000032810000f07e000068020000067c00006c02000000000000025400002c510000
    m_RenameOverlay:
      m_UserAcceptedRename: 0
      m_Name: 
      m_OriginalName: 
      m_EditFieldRect:
        serializedVersion: 2
        x: 0
        y: 0
        width: 0
        height: 0
      m_UserData: 0
      m_IsWaitingForDelay: 0
      m_IsRenaming: 0
      m_OriginalEventType: 11
      m_IsRenamingFilename: 1
      m_ClientGUIView: {fileID: 0}
    m_CreateAssetUtility:
      m_EndAction: {fileID: 0}
      m_InstanceID: 0
      m_Path: 
      m_Icon: {fileID: 0}
      m_ResourceFile: 
    m_NewAssetIndexInList: -1
    m_ScrollPosition: {x: 0, y: 0}
    m_GridSize: 64
  m_SkipHiddenPackages: 0
  m_DirectoriesAreaWidth: 207
--- !u!114 &18
MonoBehaviour:
  m_ObjectHideFlags: 52
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 1
  m_Script: {fileID: 12003, guid: 0000000000000000e000000000000000, type: 0}
  m_Name: 
  m_EditorClassIdentifier: 
  m_MinSize: {x: 100, y: 100}
  m_MaxSize: {x: 4000, y: 4000}
  m_TitleContent:
    m_Text: Console
    m_Image: {fileID: -4327648978806127646, guid: 0000000000000000d000000000000000, type: 0}
    m_Tooltip: 
  m_Pos:
    serializedVersion: 2
    x: 6
    y: 699
    width: 1504
    height: 258
  m_SerializedDataModeController:
    m_DataMode: 0
    m_PreferredDataMode: 0
    m_SupportedDataModes: 
    isAutomatic: 1
  m_ViewDataDictionary: {fileID: 0}
  m_OverlayCanvas:
    m_LastAppliedPresetName: Default
    m_SaveData: []
    m_OverlaysVisible: 1
--- !u!114 &19
MonoBehaviour:
  m_ObjectHideFlags: 52
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 1
  m_Script: {fileID: 12019, guid: 0000000000000000e000000000000000, type: 0}
  m_Name: 
  m_EditorClassIdentifier: 
  m_MinSize: {x: 275, y: 50}
  m_MaxSize: {x: 4000, y: 4000}
  m_TitleContent:
    m_Text: Inspector
    m_Image: {fileID: -2667387946076563598, guid: 0000000000000000d000000000000000, type: 0}
    m_Tooltip: 
  m_Pos:
    serializedVersion: 2
    x: 1511
    y: 119
    width: 402
    height: 838
  m_SerializedDataModeController:
    m_DataMode: 0
    m_PreferredDataMode: 0
    m_SupportedDataModes: 
    isAutomatic: 1
  m_ViewDataDictionary: {fileID: 0}
  m_OverlayCanvas:
    m_LastAppliedPresetName: Default
    m_SaveData: []
    m_OverlaysVisible: 1
  m_ObjectsLockedBeforeSerialization: []
  m_InstanceIDsLockedBeforeSerialization: 
  m_PreviewResizer:
    m_CachedPref: -161
    m_ControlHash: -371814159
    m_PrefName: Preview_InspectorPreview
  m_LastInspectedObjectInstanceID: -1
  m_LastVerticalScrollValue: 0
  m_GlobalObjectId: 
  m_InspectorMode: 0
  m_LockTracker:
    m_IsLocked: 0
  m_PreviewWindow: {fileID: 0}
````
