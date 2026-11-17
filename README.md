Copia y pega este contenido directamente en el archivo `README.md` de la raíz de tu repositorio (`[https://github.com/mahecunecha/SistemaLogistico_AMR.git](https://github.com/mahecunecha/SistemaLogistico_AMR.git)`), ya que actualiza las rutas reales (`Assets/_ProyectoBodega/`), los 12 scripts, la flota de 3 AMRs, el dron de mantenimiento, los lotes ERP y la autoría definitiva de la versión `1.0`:

```markdown
# SistemaLogistico_AMR — Gemelo Digital de Despacho Táctico Nocturno

[![Unity Version](https://img.shields.io/badge/Unity-2022.3.62f3%20LTS-black?style=flat&logo=unity)](https://unity.com/)
[![Render Pipeline](https://img.shields.io/badge/Render%20Pipeline-URP%2014.0.12-blue?style=flat)](https://unity.com/)
[![Standards](https://img.shields.io/badge/Standards-ISO%203691--4%20%7C%20VDA%205050%20%7C%20ISA--101-green?style=flat)]()
[![Paradigm](https://img.shields.io/badge/Paradigm-Industria%205.0%20(Human--in--the--Loop)-teal?style=flat)]()
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

**Gemelo Digital Interactivo 3D** y **Producto Mínimo Viable (MVP)** para la orquestación del despacho logístico nocturno (`23:00 – 06:00`) en una planta embotelladora y distribuidora de bebidas de consumo masivo. Desarrollado en **Unity 2022.3.62f3 LTS (URP)** bajo indicadores corporativos **DPO (*Distribution Process Optimization*)** y supervisión humana colaborativa de **Industria 5.0**.

---

##  Contexto Industrial: Ecosistema Macro (100%) vs. Núcleo MVP (20%)

La arquitectura conceptual modela una cadena productiva automatizada de cinco eslabones desacoplados, acotando la simulación física en tiempo real al **20% operativo crítico** donde convergen el almacenamiento inteligente y el despacho a camiones:

1. **Módulo 1 — Recepción y Lavado de Envases** *(Contexto visual en menú)*
2. **Módulo 2 — Llenado y Sellado Automatizado** *(Contexto visual en menú)*
3. **Módulo 3 — Paletizado y Enfardado** *(Contexto visual en menú)*
4. **Módulo 4 — Bodegaje Inteligente (WMS / Flota AMR)** *(Activo en simulación 3D)*
5. **Módulo 5 — Despacho Táctico Nocturno (TMS / Muelles Flatbed)** *(Activo en simulación 3D)*

---

##  Arquitectura Desacoplada en Tres Capas

```text
┌──────────────────────────────────────────────────────────────────────────────────┐
│ CAPA 1: INGESTA EXTERNA Y CONTRATOS DE DATOS (Assets/StreamingAssets/ & SO)      │
│  manifiesto.json | lote_30_pallets.json | lote_260_pallets.json | lote_450.json  │
│  Deserialización defensiva vía ModelosLogistica.cs (JsonUtility)                 │
└────────────────────────────────────────┬─────────────────────────────────────────┘
                                         │
                                         ▼
┌──────────────────────────────────────────────────────────────────────────────────┐
│ CAPA 2: NÚCLEO DE ORQUESTACIÓN Y SIMULACIÓN CINEMÁTICA DETERMINISTA              │
│  • RobotManagerEVE.cs   -> WMS Aéreo (PoE++ 48V DC), Caché O(1) y Admisión SLA   │
│  • RobotAMR.cs          -> FSM 8 Estados, Ruteo Manhattan VDA 5050 y LiFePO4     │
│  • ModuloDespachoTMS.cs -> 3 Muelles Flatbed, Estiba LIFO Invertida y Compuertas │
│  • DronMantenimiento.cs -> Despeje ISO 3691-4 (12s) y Rescate Vertical (<3% BMS) │
└────────────────────────────────────────┬─────────────────────────────────────────┘
                                         │ Eventos C# (Action) + Singleton Limpio
                                         ▼
┌──────────────────────────────────────────────────────────────────────────────────┐
│ CAPA 3: OBSERVABILIDAD SCADA (ANSI/ISA-101) Y DIRECCIÓN MULTICÁMARA              │
│  • DashboardUI.cs & GeneradorTablaManifiesto.cs -> Paneles colapsables y Modales │
│  • TelemetriaAMR.cs     -> HUD World-Space dinámico (6.6 Hz, Zero-GC)            │
│  • DirectorCinematico.cs-> Orquestación automática/manual de 5 Cámaras Virtuales │
└──────────────────────────────────────────────────────────────────────────────────┘

```

---

##  Características Técnicas Destacadas

* **Ingesta Dinámica de Lotes ERP y Dimensionamiento SLA:** Importación en caliente de archivos `.json` desde el explorador nativo del sistema operativo. El analizador de horizonte $O(1)$ evalúa la capacidad de flota (`40 pallets/h` por AMR; `80 pallets/h` en operación dual) para aprobar lotes nominales (`lote_30_pallets.json`), escalar automáticamente a flota dual con reserva (`lote_260_pallets.json`) o rechazar sobrecargas calculando el déficit exacto de unidades requeridas (`lote_450_pallets.json`).
* **Flota Multi-Agente y Máquina de Estados Finitos (FSM) de 8 Nodos:** Coordinación simultánea de `Robot_AMR` (AMR-01), `Robot_AMR_02` (AMR-02) y `Robot_AMR_03` (AMR-03 en reserva) bajo estados estrictos (`Inactivo`, `En_Transito`, `Extrayendo`, `Transportando`, `Interrumpido`, `Entregando`, `Recargando` y `Apagado`).
* **Navegación Ortogonal VDA 5050 y Cinemática VLM / LIFO:** Tránsito segregado por carriles (`Z = -32.0 m` Oeste / `Z = -34.0 m` Este) con desfase lateral de `1.75 m` en estanterías, extracción por rodillos motorizados VLM/ASRS (`Y = 0.263 m`) y descarga LIFO invertida sobre plataformas *flatbed* (`Y = 1.813 m`, espaciado `1.60 m` y retroceso recto *Drop & Reverse* de `1.20 m`).
* **Electroquímica $\text{LiFePO}_4$ y Supervisión Humana (Industria 5.0):** Descarga calibrada por distancia, masa de estiba (`850 kg – 920 kg`) y ruido coherente de Perlin dentro de la ventana segura `80.0% – 20.0% SoC`, acoplada a compresión temporal de turno (`7 horas` en `3.5 s`, factor `160x`). Ante alerta preventiva ($\le 20\%$), el supervisor decide entre **Activar Relevo 24/7** o **Forzar Override** (`-50%` velocidad/aceleración, `+30%` consumo térmico).
* **Seguridad Activa LiDAR ISO 3691-4 y Rescate Aéreo:** Arreglo frontal de tres haces (`0°`, `±15°`) con alerta a `1.65 m`, parada Categoría 3 a `1.15 m` y *Docking Mute* en acoples. `Dron_Mantenimiento` remueve obstrucciones en pasillo en `12 s` y extrae verticalmente (`Y = 8.5 m`) cualquier unidad apagada por corte crítico de BMS (`< 3.0% SoC`).
* **Ergonomía HMI (ANSI/ISA-101) y Optimización Zero-GC:** Paneles colapsables (`[-]` / `[+]`) que preservan el contexto horario del turno, búferes preasignados `StringBuilder` y telemetría flotante a `6.6 Hz` (`0` referencias rotas, `0` errores de compilación).

---

## 🛠️ Stack Tecnológico y Dependencias

| Componente / Paquete | Versión Exacta | Función en el Proyecto |
| --- | --- | --- |
| **Unity Engine** | `2022.3.62f3 LTS` (`96770f904ca7`) | Motor de simulación 3D (Standalone `x86_64`) |
| `com.unity.render-pipelines.universal` | `14.0.12` | Renderizado PBR e iluminación industrial nocturna URP |
| `com.unity.cinemachine` | `3.1.7` | Dirección multicámara (`VCam0_Intro` a `VCam4_Emergencia`) |
| `com.unity.ai.navigation` | `1.1.7` | Mallas de navegación (`NavMeshSurface` / `NavMeshAgent`) |
| `com.unity.textmeshpro` | `3.0.9` | Tipografía SDF de alta legibilidad para paneles SCADA y HUDs |
| `com.unity.inputsystem` | `1.14.2` | Gestión de eventos de entrada y compatibilidad multiplataforma |
| `com.gamebooom.unity.mcp` | `0.7.6` | Auditoría forense de escena y validación automatizada |

---

## 📂 Estructura del Repositorio

```text
SistemaLogistico_AMR/
├── Assets/
│   ├── _ProyectoBodega/
│   │   ├── Art/                    # Tipografías SDF (Roboto Slab, Instrument Serif) e iconos
│   │   ├── Escenas/
│   │   │   └── Bodega_TurnoNocturno.unity  # Escena principal configurada en Build Index 0
│   │   ├── Prefabs/                # PalletCervezas.prefab, Fila_Dato_Prefab.prefab y fábrica
│   │   └── Scripts/                # 10 scripts Runtime + 2 herramientas Editor
│   │       ├── ModelosLogistica.cs
│   │       ├── RobotManagerEVE.cs
│   │       ├── RobotAMR.cs
│   │       ├── ModuloDespachoTMS.cs
│   │       ├── DronMantenimiento.cs
│   │       ├── DronePropellers.cs
│   │       ├── DirectorCinematico.cs
│   │       ├── DashboardUI.cs
│   │       ├── GeneradorTablaManifiesto.cs
│   │       ├── TelemetriaAMR.cs
│   │       └── Editor/
│   │           ├── OrphanAssetFinder.cs
│   │           └── RestructureProject.cs
│   ├── Plugins_Terceros/           # Activos 3D industriales externos aislados
│   └── StreamingAssets/            # Contratos de datos externos editables sin recompilar
│       ├── manifiesto.json         # Lote base del turno nocturno (3 pallets)
│       ├── lote_30_pallets.json    # Prueba SLA nominal (1 AMR)
│       ├── lote_260_pallets.json   # Prueba SLA escalamiento dual (2 AMRs + reserva)
│       └── lote_450_pallets.json   # Prueba SLA rechazo por capacidad máxima
├── Packages/
│   └── manifest.json               # Manifiesto oficial de dependencias UPM
├── ProjectSettings/                # Configuración de capas, física y Build Settings
├── Manuales/                       # Documentación oficial APA 7.ª Edición y Artículo Científico
├── LEEME.txt                       # Guía rápida multiplataforma
└── LICENSE                         # Licencia MIT (código fuente propio)

```

---

##  Instalación y Ejecución

### Escenario A: Ejecución Directa del Simulador Compilado (Usuario / Evaluador)

1. Descargue el paquete comprimido `SistemaLogistico_AMR_v1.0.zip` desde la sección **[suspicious link removed]** del repositorio.
2. Descomprima el archivo en una ruta local limpia (por ejemplo, `C:\Simuladores\SistemaLogistico_AMR\`).
3. Ingrese a la carpeta `Build/` (manteniendo siempre el ejecutable junto a la carpeta `SistemaLogistico_AMR_Data/`) y ejecute:
* **Windows (64 bits):** Doble clic en `SistemaLogistico_AMR.exe` *(si aparece Windows Defender SmartScreen, haga clic en **Más información > Ejecutar de todas formas**)*.
* **Linux (64 bits):** Ejecute en terminal `chmod +x SistemaLogistico_AMR.x86_64 && ./SistemaLogistico_AMR.x86_64`.


4. En el menú corporativo inicial, haga clic en **INICIAR TURNO** dentro de la tarjeta **Módulo 4: Despacho Nocturno**.

### Escenario B: Apertura en Unity Editor y Compilación (Desarrollador)

1. Clone este repositorio:
```bash
git clone [https://github.com/mahecunecha/SistemaLogistico_AMR.git](https://github.com/mahecunecha/SistemaLogistico_AMR.git)

```


2. Abra **Unity Hub**, haga clic en **Add > Add project from disk** y seleccione la carpeta raíz clonada usando exactamente **Unity 2022.3.62f3 LTS**.
3. Abra la escena `Assets/_ProyectoBodega/Escenas/Bodega_TurnoNocturno.unity` y presione **Play (▶)**, o compile desde **File > Build Settings** verificando la escena en el **Índice 0**.

---

## 🎮 Controles y Atajos de Supervisión

| Entrada / Atajo | Contexto | Acción en el Simulador |
| --- | --- | --- |
| **Clic Izquierdo** | Interfaz SCADA / Menú | Operar tarjetas, botones laterales, colapso de paneles `[-]` / `[+]` y modales de decisión. |
| **Rueda del Ratón** | Tablas y Consola | Desplazar verticalmente el **Manifiesto de Carga** y la **Terminal de EVE**. |
| **`Espacio` / `Esc**` | Introducción | Omitir la secuencia cinematográfica inicial e ir al Menú Principal. |
| **`Tecla O`** | Operación en Pasillo | Inyectar obstáculo normativo **ISO 3691-4** frente al AMR activo para validar frenado y despeje aéreo. |
| **`Tecla I`** | Análisis de Capacidad | Ejecutar proyección instantánea $O(1)$ de estrés de flota ante `50` nuevos pedidos. |
| **`Shift + I`** | Reloj de Turno | Alternar entre tres hitos horarios (`1.5 h` Inicio, `5.0 h` Madrugada, `7.5 h` Cierre) para evaluar balance energético. |

---

## Documentación Oficial del Proyecto (Norma APA 7.ª Edición)

La suite documental completa se encuentra disponible en la carpeta `Manuales/`:

* **Especificación de Requerimientos de Software (SRS):** `SistemaLogistico_AMR_Documento_Requerimientos_SRS_ES.pdf`
* **Propuesta de Diseño del Entorno Interactivo (GDD):** `SistemaLogistico_AMR_Propuesta_Diseno_GDD_ES.pdf`
* **Manual Técnico (Español / Inglés):** `SistemaLogistico_AMR_Manual_Tecnico_ES.pdf` | `SistemaLogistico_AMR_Technical_Manual_EN.pdf`
* **Manual de Usuario (Español / Inglés):** `SistemaLogistico_AMR_Manual_Usuario_ES.pdf` | `SistemaLogistico_AMR_User_Manual_EN.pdf`
* **Manual de Instalación (Español / Inglés):** `SistemaLogistico_AMR_Manual_Instalacion_ES.pdf` | `SistemaLogistico_AMR_Installation_Manual_EN.pdf`
* **Artículo de Resultados Viables:** `SistemaLogistico_AMR_Articulo_Cientifico_APA7.pdf`

---

## Equipo de Desarrollo, Créditos y Licencia

Proyecto formativo desarrollado en el **Centro de la Industria, la Empresa y los Servicios (CIES) – Servicio Nacional de Aprendizaje (SENA), Regional Huila, Sede Neiva**:

* **Sergio Mahecha** — *Arquitectura de Software, Lógica C#, Desacoplamiento de Datos e Integración de Normas*
* **Nicolás Joussef Hoyos Cuéllar** — *Diseño Mecánico y Entorno 3D*
* **Juan José Solano Cuenca** — *Cinemática, Cámaras e Integración URP*
* **Caleb Santiago Cárdenas Espinosa** — *Aseguramiento de Calidad (QA), Pruebas Funcionales y Control de Datos*
* **Instructor Técnico Asesor:** Ing. Eddier Yair Liberatto
* **Programa de Formación:** Tecnología en Desarrollo de Videojuegos y Entornos Interactivos (Código `228108` — Ficha `3171050`)
* **Versión y Fecha:** Versión `1.0` — `23 de noviembre de 2026`
* **Soporte Técnico:** `soporte.sistemalogisticoamr@gmail.com`

### Licencia

El código fuente C# desarrollado por el equipo (`Assets/_ProyectoBodega/Scripts/`) se distribuye bajo la **[Licencia MIT](https://www.google.com/search?q=LICENSE)**. Los modelos tridimensionales y tipografías de terceros alojados en `Assets/Plugins_Terceros/` y `Assets/_ProyectoBodega/Art/Fonts/` conservan sus licencias originales (*Unity Asset Store Standard EULA* y *SIL Open Font License 1.1*).

```

### Mensaje de Commit Sugerido para GitKraken

* **Summary:** `docs(readme): actualizar README corporativo v1.0 con arquitectura multi-agente, SLA y suite documental`
* **Description:** `Sincronizar estructura real de Assets/_ProyectoBodega, catálogo de 12 scripts, flota de 3 AMRs, dron de mantenimiento, contratos JSON ERP, roles del equipo y enlaces a los manuales oficiales APA 7.`

```
