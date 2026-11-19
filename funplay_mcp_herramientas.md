# Herramientas del MCP Funplay (`funplay-sistemalogistico`)

Catálogo completo de las **185 herramientas** del servidor MCP de Funplay para Unity (`funplay-sistemalogistico`), verificadas en vivo con el perfil **`full`** activo (185 implementadas, 185 expuestas y 0 deshabilitadas).

---

## 1. Escenas y Jerarquía
- **`open_scene`**: Abre una escena existente a partir de su ruta.
- **`create_new_scene`**: Crea una nueva escena vacía.
- **`save_scene`**: Guarda la escena actual.
- **`save_all_scenes`**: Guarda todas las escenas abiertas con cambios sin guardar (`confirm_all=true` si hay más de una).
- **`load_scene_additive`**: Carga una escena de forma aditiva junto a las escenas ya abiertas.
- **`unload_scene`**: Descarga y cierra una escena cargada aditivamente por nombre o ruta.
- **`get_scene_info`**: Obtiene información de todas las escenas cargadas (ruta, estado *dirty* y objetos raíz).
- **`list_scenes`**: Lista todas las escenas existentes en el proyecto.
- **`list_dirty_scenes`**: Lista las escenas cargadas que tienen cambios sin guardar.
- **`get_hierarchy`**: Explora el árbol de jerarquía de la escena (completo o desde un `root_name`).
- **`find_game_objects`**: Busca GameObjects por ID, nombre, ruta, tag, layer o componente.
- **`get_selection`**: Obtiene los GameObjects seleccionados actualmente en la jerarquía.
- **`set_selection`**: Reemplaza la selección actual de la jerarquía por los objetos indicados.
- **`select_object`**: Selecciona un GameObject en la jerarquía y en el Inspector.
- **`analyze_scene_complexity`**: Analiza la escena actual para detectar cuellos de botella (triángulos, materiales y componentes).

## 2. GameObjects y Transformaciones
- **`create_game_object`**: Crea un GameObject vacío en la escena y devuelve su `instanceId`.
- **`create_primitive`**: Crea un GameObject primitivo (`Cube`, `Sphere`, `Capsule`, `Cylinder`, `Plane`, `Quad`).
- **`duplicate_game_object`**: Duplica un GameObject existente y opcionalmente renombra la copia.
- **`delete_game_object`**: Elimina un GameObject de la escena.
- **`rename_game_object`**: Cambia el nombre de un GameObject.
- **`set_parent`**: Cambia el padre de un GameObject (o lo desvincula si se pasa vacío).
- **`set_active`**: Activa o desactiva uno o varios GameObjects (hasta 100 en lote).
- **`set_transform`**: Establece posición, rotación y/o escala de uno o varios GameObjects.
- **`set_tag_and_layer`**: Asigna un *tag* y/o una *layer* a un GameObject.
- **`get_game_object_info`**: Obtiene información completa de un GameObject (Transform, componentes, estado activo, tag, layer).
- **`focus_on_object`**: Enfoca la cámara de la Scene View sobre un GameObject específico.

## 3. Componentes y Propiedades Serializadas
- **`add_component`**: Agrega un componente a un GameObject y devuelve su `instanceId`.
- **`add_component_to_many`**: Agrega un mismo tipo de componente a múltiples GameObjects (hasta 100) en una sola llamada con Undo.
- **`list_components`**: Lista los componentes de un GameObject junto con sus `instanceId`.
- **`get_component_properties`**: Lee todas las propiedades serializadas de un componente (incluidos campos privados con `[SerializeField]`).
- **`set_component_property`**: Modifica una propiedad o campo serializado de un componente (o de varios en lote).
- **`set_component_properties`**: Modifica múltiples propiedades serializadas de un componente en una sola llamada.
- **`copy_component`**: Copia un componente al portapapeles compartido de componentes del Editor de Unity.
- **`paste_component_values`**: Pega los valores del portapapeles de componentes sobre múltiples GameObjects.

## 4. Prefabs y Prefab Stage
- **`create_prefab`**: Crea un asset `.prefab` a partir de un GameObject de la escena.
- **`instantiate_prefab`**: Instancia un prefab en la escena actual.
- **`unpack_prefab`**: Desempaqueta una instancia de prefab en la escena.
- **`open_prefab_stage`**: Abre un prefab en *Prefab Mode* (escenario aislado) para editar su estructura directamente.
- **`get_prefab_stage`**: Consulta información del *Prefab Stage* abierto actualmente (ruta, raíz y estado *dirty*).
- **`save_prefab_stage`**: Guarda los cambios del *Prefab Stage* abierto en su archivo `.prefab`.
- **`close_prefab_stage`**: Cierra el *Prefab Stage* actual (guardando o descartando cambios según parámetro).
- **`set_prefab_property`**: Modifica un campo serializado en un prefab sin abrir *Prefab Mode* y verifica la lectura posterior.
- **`set_prefab_properties`**: Modifica múltiples campos serializados de un componente en un prefab sin abrir *Prefab Mode*.

## 5. UI (uGUI / TextMeshPro) y Auditoría Visual
- **`create_canvas`**: Crea un Canvas en Edit Mode usando la plantilla o valores por defecto del proyecto.
- **`create_button`**: Crea un botón de UI en Edit Mode usando las plantillas y tipografía del proyecto.
- **`create_text`**: Crea un elemento de texto en Edit Mode (prefiere `TextMeshProUGUI` si está disponible).
- **`create_image`**: Crea un elemento `Image` de UI.
- **`create_project_ui`**: Crea Canvas, Button o Text en Edit Mode respetando prefabs, tipografía y layout configurados.
- **`configure_ui_defaults`**: Reemplaza la configuración por defecto de UI del proyecto (TMP/Legacy, fuentes, módulo de entrada, plantillas).
- **`get_ui_defaults`**: Lee las plantillas y valores por defecto de UI del proyecto y fuentes disponibles.
- **`inspect_ui_sprites`**: Inspecciona en lote `Image -> Sprite -> TextureImporter` (bordes, modo Single/Multiple, atlas, GUID/ID).
- **`get_object_screen_bounds`**: Calcula los límites en pantalla de un `RectTransform` o `Renderer` en píxeles de render y de imagen.
- **`raycast_at_point`**: Diagnostica qué elementos UI o de física recibirían un clic en un punto de pantalla en Play Mode (detecta bloqueadores invisibles).
- **`audit_ui`**: Ejecuta una auditoría de UI de solo lectura sobre selección, raíces, prefabs o escenas (bordes, raycast targets, clipping, layout).
- **`get_ui_audit`**: Consulta el estado y los hallazgos paginados de una auditoría de UI.
- **`cancel_ui_audit`**: Cancela una auditoría de UI en curso y cierra su escena de vista previa.
- **`start_ui_preview_session`**: Inicia una sesión temporal de vista previa para un prefab o plantilla de escena UI.
- **`get_ui_preview_session`**: Consulta el estado actual o más reciente de la sesión de vista previa UI.
- **`end_ui_preview_session`**: Finaliza la sesión de vista previa UI y restaura escenas, selección y ajustes originales.

## 6. Assets, Materiales, Scripts, ScriptableObjects e Importación
- **`find_assets`**: Busca assets en el proyecto por tipo y nombre.
- **`find_references`**: Busca dependencias directas de un asset (`depends_on`) y qué assets lo referencian (`referenced_by`).
- **`find_broken_references`**: Escanea escenas o prefabs/assets en busca de scripts faltantes o referencias rotas.
- **`copy_asset`**: Copia un asset a una nueva ubicación.
- **`rename_asset`**: Renombra un asset.
- **`delete_asset`**: Elimina un asset del proyecto.
- **`ping_asset`**: Resalta y ubica un asset en la ventana Project de Unity.
- **`get_asset_import_settings`**: Lee los ajustes de importación de texturas, audio o modelos (`TextureImporter`, `AudioImporter`, `ModelImporter`).
- **`set_asset_import_settings`**: Modifica y reimporta los ajustes de importación de un asset con lectura de verificación.
- **`create_material`**: Crea un nuevo asset de Material con un color especificado.
- **`assign_material`**: Asigna un Material al `Renderer` de un GameObject.
- **`get_material_properties`**: Inspecciona un Material (shader, keywords, render queue y propiedades actuales).
- **`set_material_property`**: Modifica una propiedad de shader de un Material y guarda el asset.
- **`get_mesh_info`**: Inspecciona una malla (vértices, triángulos, submeshes, bounds, UVs, normales, blendShapes) desde asset o GameObject.
- **`create_script`**: Crea un nuevo script C# con el contenido especificado.
- **`edit_script`**: Reemplaza el contenido completo de un script existente.
- **`patch_script`**: Aplica un parche de búsqueda y reemplazo exacto sobre un script existente.
- **`create_scriptable_object`**: Crea un nuevo asset `ScriptableObject` del tipo indicado.
- **`get_scriptable_object`**: Lee todas las propiedades serializadas de un asset `ScriptableObject`.
- **`set_scriptable_object_properties`**: Modifica múltiples propiedades serializadas de un `ScriptableObject` y lo guarda.
- **`patch_serialized_reference`**: Actualiza de forma segura y verificada por hash una referencia `ObjectReference` en un `.asset` o `.prefab`.
- **`inspect_render_bindings`**: Lee enlaces reales de renderizado (materiales, shaders, texturas, partículas, cámaras, orden y máscaras) sin instanciar materiales.

## 7. Animación, Timeline y Partículas
- **`create_animation_clip`**: Crea un asset `AnimationClip`.
- **`create_animator_controller`**: Crea un asset `AnimatorController`.
- **`assign_animator`**: Asigna un `AnimatorController` a un GameObject.
- **`get_animator_state`**: Consulta el estado actual de un `Animator` (estado activo, tiempo normalizado, transiciones y parámetros).
- **`play_animator_state`**: Reproduce un estado específico del `Animator` por nombre (fuerza evaluación en Edit Mode).
- **`set_animator_parameter`**: Modifica un parámetro del `Animator` (`Float`, `Int`, `Bool` o `Trigger`).
- **`director_evaluate`**: Evalúa un `PlayableDirector` (Timeline) en un tiempo específico dentro del editor.
- **`particle_control`**: Controla un `ParticleSystem` (`simulate`, `play`, `pause`, `stop`, `restart`) para inspección o captura determinista.

## 8. Cámaras e Iluminación
- **`get_camera_properties`**: Lee todas las propiedades de una cámara (tipo, FOV, planos de recorte, fondo y culling mask).
- **`set_camera_projection`**: Configura la proyección de una cámara (ortográfica o perspectiva, tamaño/FOV).
- **`set_camera_settings`**: Configura planos de recorte, color de fondo y *clear flags* de una cámara.
- **`set_camera_culling_mask`**: Configura la *culling mask* de una cámara para mostrar u ocultar capas.
- **`get_lighting_settings`**: Lee la configuración de iluminación ambiental de la escena (ambient, skybox, niebla, estado de bake).
- **`set_lighting_settings`**: Modifica la configuración de iluminación ambiental de la escena.
- **`bake_lightmaps`**: Inicia un *bake* asíncrono de *lightmaps* para la escena actual.

## 9. Física (Raycast y Overlap)
- **`physics_raycast`**: Lanza un rayo 3D (`Physics.RaycastAll`) contra colliders estáticos en Edit Mode o Play Mode.
- **`physics_overlap`**: Consulta solapamientos 3D (`Physics.OverlapSphere` o `Physics.OverlapBox`).
- **`physics2d_overlap_point`**: Consulta colliders 2D que solapan un punto (`Physics2D.OverlapPointAll`).

## 10. Captura Visual, Video y Simulador de Dispositivos
- **`capture_game_view`**: Captura una imagen PNG de la Game View (en base64 o archivo).
- **`capture_scene_view`**: Captura una imagen PNG de la Scene View (con opción de incluir UI Overlay).
- **`capture_editor_window`**: Captura una imagen PNG de cualquier ventana abierta del Editor (Inspector, Console, Project, etc.).
- **`capture_multiview`**: Captura múltiples vistas alrededor de un objetivo en modo `surround` (6 caras) u `orbit`.
- **`capture_simulator_view`**: Captura la pantalla del *Unity Device Simulator*, opcionalmente con el contorno de *Safe Area*.
- **`get_visual_coordinates`**: Consulta el contrato actual de coordenadas de la Game View o un recibo de captura reciente.
- **`record_game_view`**: Graba un video MP4 silencioso de la Game View en Play Mode (`start` / `stop`).
- **`mark_recording`**: Agrega un marcador con nombre en la línea de tiempo de una grabación activa.
- **`extract_recording_frames`**: Extrae de 1 a 16 fotogramas PNG de un video grabado en los timestamps indicados.
- **`get_recording_frame`**: Devuelve un fotograma extraído de video como imagen MCP junto con sus metadatos.

## 11. Simulación de Entrada (Input) y Play Mode
- **`enter_play_mode`**: Entra en Play Mode en el Editor.
- **`exit_play_mode`**: Sale de Play Mode en el Editor.
- **`get_time_scale`**: Consulta la escala de tiempo actual (`Time.timeScale`) e información temporal.
- **`set_time_scale`**: Establece la escala de tiempo (0 para pausar, 1 normal, etc.).
- **`simulate_key_press`**: Simula la pulsación de una tecla en Play Mode para el Input System.
- **`simulate_key_combo`**: Simula múltiples teclas presionadas simultáneamente en Play Mode.
- **`simulate_mouse_click`**: Simula un clic en coordenadas de pantalla en Play Mode (despacha eventos UI y de física).
- **`simulate_mouse_drag`**: Simula un arrastre (*drag*) con ratón o toque único a través de múltiples fotogramas en Play Mode.
- **`simulate_ui_scroll`**: Despacha un evento de scroll UI (`IScrollHandler`) sobre un punto de pantalla en Play Mode.

## 12. Compilación, Estado del Editor, Tareas y Ejecución de Código
- **`get_editor_state`**: Consulta el estado de ejecución del Editor (Play Mode, pausa, compilando, actualizando).
- **`get_compilation_errors`**: Obtiene los errores de compilación del ciclo más reciente.
- **`get_console_logs`**: Lee, filtra o limpia los mensajes de la consola de Unity (`Log`, `Warning`, `Error`).
- **`log_message`**: Escribe un mensaje en la consola de Unity.
- **`request_recompile`**: Fuerza a Unity a importar cambios externos y esperar compilación / *domain reload*.
- **`wait_for_compilation`**: Refresca y espera de forma no bloqueante a que termine la compilación de scripts.
- **`prepare_editor`**: Prepara el Editor en modo Edit o Play con una tarea durable que sobrevive a *domain reloads*.
- **`get_editor_operation`**: Consulta el estado de una preparación del Editor por `operation_id` o `request_key`.
- **`list_editor_operations`**: Lista los 32 recibos más recientes de operaciones de preparación del Editor.
- **`cancel_editor_operation`**: Cancela pasos futuros de preparación pendientes del Editor.
- **`get_reload_recovery_status`**: Consulta el último evento de recuperación tras un *domain reload*.
- **`get_task`**: Consulta de forma unificada el estado de cualquier tarea asíncrona (`editor`, `preview`, `execution`, `tests`, `recording`, etc.).
- **`get_tool_capabilities`**: Lista las herramientas implementadas frente a las expuestas actualmente según el perfil (`Core` / `Full`).
- **`find_project_types`**: Busca nombres completos de tipos y ensamblados del proyecto y de Unity sin instanciarlos.
- **`execute_code`**: Compila y ejecuta un fragmento C# en el hilo del Editor (`IFunplayCommand` o `Run()`) como respaldo para operaciones específicas no cubiertas por herramientas estructuradas.
- **`get_execute_code_history`**: Lista el historial (hasta 50 entradas) de ejecuciones recientes de `execute_code`.
- **`replay_execute_code`**: Vuelve a ejecutar una entrada previa del historial de `execute_code` por su índice.
- **`clear_execute_code_history`**: Borra el historial de `execute_code` de la sesión actual.
- **`execute_menu_item`**: Ejecuta un elemento del menú del Editor por su ruta completa (ej. `Edit/Undo`).
- **`validate_menu_item`**: Verifica si una ruta de menú existe sin ejecutarla.
- **`get_windows`**: Lista todas las ventanas abiertas del Editor (`EditorWindow`) con título, tipo y foco.
- **`show_dialog`**: Muestra un cuadro de diálogo al usuario en el Editor.

## 13. Pruebas (Test Runner) y Verificación
- **`run_tests`**: Ejecuta pruebas de Unity Test Runner (`EditMode` o `PlayMode`) de forma asíncrona con filtros opcionales.
- **`get_test_job`**: Consulta el progreso y resultados (pasadas, fallidas, omitidas) de una ejecución de pruebas.
- **`cancel_test_run`**: Cancela una ejecución de pruebas en curso.
- **`verification_scope`**: Inicia o finaliza un ámbito acotado de verificación en la escena Play real (asociando inputs y grabaciones).
- **`save_verification_evidence`**: Guarda un recibo de evidencia estática con hashes de archivos, `.meta`, dependencias y notas bajo `Library`.
- **`read_verification_evidence`**: Lee un recibo de evidencia guardado y revalida sus hashes actuales contra el disco.

## 14. Profiling, Memoria y Frame Debugger
- **`profiler_start`**: Inicia una sesión del Profiler y habilita contadores persistentes.
- **`profiler_status`**: Informa si el Profiler está activo y cuántos contadores hay activos.
- **`profiler_stop`**: Detiene la sesión actual del Profiler y libera los contadores.
- **`get_counters`**: Lee contadores de renderizado y memoria (Draw Calls, Batches, Triángulos, SetPass, memoria GC/Gfx).
- **`get_frame_timing`**: Obtiene los tiempos de CPU/GPU de los fotogramas más recientes mediante `FrameTimingManager`.
- **`get_performance_snapshot`**: Genera una instantánea ligera de rendimiento (tiempos de frame, memoria, escala de escena y conteos básicos).
- **`get_object_memory`**: Obtiene el consumo de memoria en tiempo de ejecución de un asset o GameObject específico.
- **`get_top_memory_objects`**: Lista los N objetos cargados que más memoria consumen de un tipo dado (o resumen por tipo con `All`).
- **`memory_take_snapshot`**: Toma una instantánea ligera de memoria (contadores agregados en JSON).
- **`memory_list_snapshots`**: Lista las instantáneas ligeras de memoria guardadas.
- **`memory_compare_snapshots`**: Compara dos instantáneas ligeras de memoria y reporta las diferencias por categoría.
- **`memory_take_full_snapshot`**: Captura una instantánea completa real `.snap` compatible con el paquete Memory Profiler.
- **`memory_list_full_snapshots`**: Lista las instantáneas `.snap` reales disponibles en la carpeta de capturas.
- **`memory_open_snapshot_in_profiler`**: Abre un archivo `.snap` en la ventana del paquete Memory Profiler.
- **`memory_query_top_objects`**: Consulta en un archivo `.snap` los objetos nativos más grandes en formato JSON estructurado.
- **`memory_query_references`**: Consulta en un archivo `.snap` qué referencias mantienen vivo a un objeto nativo (o qué referencia él).
- **`frame_debugger_enable`**: Abre y activa el Frame Debugger para capturar eventos de renderizado.
- **`frame_debugger_get_events`**: Lista los eventos capturados del frame actual en el Frame Debugger.
- **`frame_debugger_disable`**: Desactiva el Frame Debugger y cierra su ventana.

## 15. Configuración del Proyecto, Paquetes, Tags, Layers, Undo/Redo y Archivos
- **`get_project_settings`**: Lee un resumen de `QualitySettings`, `PlayerSettings` y el `BuildTarget` activo.
- **`get_build_settings`**: Lee las escenas habilitadas en Build Settings, plataforma activa y backend de scripting.
- **`list_packages`**: Lista todos los paquetes instalados en el proyecto.
- **`install_package`**: Instala un paquete de Unity por nombre.
- **`remove_package`**: Desinstala un paquete de Unity.
- **`get_tags`**: Lista todos los *tags* definidos en `TagManager`.
- **`add_tag`**: Agrega un nuevo *tag* a `TagManager`.
- **`remove_tag`**: Elimina un *tag* de `TagManager` si no está en uso.
- **`get_layers`**: Lista los 32 slots de *layers* con sus nombres.
- **`add_layer`**: Agrega una nueva *layer* en el primer slot de usuario libre (8–31).
- **`get_active_tool`**: Consulta la herramienta de manipulación activa en el Editor (Move, Rotate, Scale, Rect, etc.).
- **`set_active_tool`**: Cambia la herramienta de manipulación activa en el Editor.
- **`get_undo_state`**: Lee el estado actual de la pila global de Undo del Editor.
- **`undo`**: Ejecuta un paso de Undo en la pila global del Editor.
- **`redo`**: Ejecuta un paso de Redo en la pila global del Editor.
- **`read_file`**: Lee el contenido de un archivo.
- **`write_file`**: Crea o sobrescribe el contenido de un archivo.
- **`exists`**: Verifica si un archivo o directorio existe.
- **`list_directory`**: Lista archivos y subdirectorios dentro de una ruta.
- **`search_files`**: Busca archivos en el proyecto por patrón de nombre.
