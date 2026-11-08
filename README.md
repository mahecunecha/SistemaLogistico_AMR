# Sistema Logístico AMR — Gemelo Digital de Despacho Táctico Nocturno

[![Unity Version](https://img.shields.io/badge/Unity-2022.3.62f3%20LTS-black?style=flat&logo=unity)](https://unity.com/)
[![Render Pipeline](https://img.shields.io/badge/Render%20Pipeline-URP%2014.0.12-blue?style=flat)](https://unity.com/)
[![License](https://img.shields.io/badge/Standard-Industria%205.0%20%2F%20ISO%203691--4-green?style=flat)]()
[![Build Target](https://img.shields.io/badge/Platform-StandaloneLinux64%20%7C%20Windows64-lightgrey?style=flat)]()
[![Documentation](https://img.shields.io/badge/Norma-APA%207ma%20Edici%C3%B3n-orange?style=flat)]()

Entorno interactivo industrial y Producto Mínimo Viable (MVP) de un **Gemelo Digital** para la optimización del turno nocturno de despacho logístico (18:00 - 06:00) en el sector de bebidas de consumo masivo. Desarrollado en **Unity 2022.3 LTS**, el sistema simula la orquestación autónoma de flotas robóticas (AMR/EVE) bajo estándares **DPO (Distribution Process Optimization)**, desacoplamiento estricto de datos JSON e interacción supervisada basada en **Industria 5.0**.

---

##  Arquitectura y Ecosistema de Planta (Lore 100% vs. MVP 20%)

El sistema se concibe conceptualmente como parte de un macroproceso automatizado de 5 microservicios independientes:
1. **Módulo 1: Recepción y Lavado de Envases** *(Restringido en MVP)*
2. **Módulo 2: Llenado y Sellado de Producto** *(Restringido en MVP)*
3. **Módulo 3: Paletizado Automatizado** *(Restringido en MVP)*
4. **Módulo 4: Bodegaje Inteligente** *(Implementado en MVP)*
5. **Módulo 5: Despacho Táctico Nocturno y TMS** *(Implementado en MVP)*

---

##  Características Principales de Ingeniería

* **Desacoplamiento Estricto (Data-Driven Design):** Cero dependencias rígidas en código fuente. Las misiones de carga y especificaciones de SKU se extraen en tiempo de ejecución desde `Assets/StreamingAssets/manifiesto.json`.
* **Física y Batería Predictiva Realista:** El drenaje de las celdas LiFePO4 cruza distancia ortogonal real con el peso del payload (`kg`) y un factor de dispersión térmica.
* **Seguridad Industrial ISO 3691-4 (Cat. 3):** Detección volumétrica por LiDAR frontal y parada de emergencia a $\le 0.85\text{ m}$ con protocolo *Docking Mute* en zonas de carga.
* **Trade-Off Gerencial Industria 5.0:** El operario humano resuelve el dilema energético ante sobrecargas críticas ($920\text{ kg}$):
  * **Forzar Override:** Continúa la entrega asumiendo degradación física.
  * **Activar Relevo 24/7:** Despliega a `AMR-02` (80% batería) y retira a `AMR-01` a recarga inductiva.
* **Cero Basura en Memoria (Zero-GC Optimization):** Uso de `StringBuilder` para el scroll de terminal, eventos en C# desacoplados y throttling de telemetría para garantizar estabilidad en rendimiento.

---

##  Stack Tecnológico y Dependencias

* **Motor:** Unity 2022.3.62f3 LTS (URP - Universal Render Pipeline 14.0.12)
* **Lenguaje:** C# (.NET Standard 2.1)
* **Paquetes Principales:**
  * `com.unity.ai.navigation` (1.1.7): Mallas dinámicas de navegación para AMRs terrestres.
  * `com.unity.cinemachine` (3.1.7): Arquitectura cinemática multicámara por estados robóticos.
  * `com.unity.textmeshpro` (3.0.9): Renderizado tipográfico SDF en HUDs y SCADA.
  * `com.unity.inputsystem` (1.14.2): Procesamiento de periféricos.

---

##  Estructura del Repositorio

```text
├── Assets/
│   ├── Art/                # Fuentes tipográficas SDF, texturas e iconos corporativos
│   ├── Prefabs/            # Fila_Dato_Prefab, módulos de fábrica y estanterías
│   ├── Scenes/
│   │   └── Bodega_TurnoNocturno.unity  # Escena operacional activa (Index 0)
│   ├── Scripts/            # Los 9 scripts de arquitectura C#
│   └── StreamingAssets/
│       └── manifiesto.json # Contrato de datos externo de pedidos
├── Manuales/               # Documentación oficial bajo norma APA 7ma Edición (PDFs)
└── ProjectSettings/        # Configuraciones de compilación y capas del motor
