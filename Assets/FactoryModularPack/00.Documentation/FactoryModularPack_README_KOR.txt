[ Factory Modular Pack - README ]

Factory Modular Pack을 사용해주셔서 감사합니다.

Factory Modular Pack은 Unity URP 환경에서 공장 내부, 외부, 산업 야드, 하역 구역, 설비 공간 등을 제작할 수 있도록 구성된 모듈형 산업 환경 에셋 팩입니다.

본 패키지에는 모듈형 공장 건물 부품, 산업용 프랍, 안전 설비, 교통 제어 프랍, 데칼, 그리고 2개의 데모 씬이 포함되어 있습니다.

────────────────────────────
◎ 목차
────────────────────────────

1. 패키지 개요
2. 빠른 시작 안내
3. 데모 씬
4. 주요 에셋 카테고리
5. 모듈형 건물 시스템
6. 머티리얼 및 텍스처
7. 기술 사양
8. 조명 및 환경 세팅
9. 콜라이더, LOD, 애니메이션
10. 스크립트 및 코드
11. 서드파티 고지
12. 참고 사항

────────────────────────────
1. 패키지 개요
────────────────────────────

Factory Modular Pack은 재사용 가능한 모듈형 에셋과 프리팹을 중심으로 구성되어 있습니다.

제공되는 샘플 씬을 완성형 공장 단지로 사용할 수 있으며, 포함된 건물 모듈, 프랍, 데칼을 조합하여 원하는 공장 레이아웃을 직접 구성할 수도 있습니다.

패키지 포함 항목:
- 모듈형 공장 건물 부품
- 내부 및 외부 벽 패널
- 지붕, 바닥, 그라운드, 문, 창문, 차양, 계단, 구조물 모듈
- 산업 설비용 프랍
- 안전 및 비상 설비
- 교통 제어 프랍
- 도로, 바닥, 안전 표지, 구역 ID 데칼
- 외부 샘플 씬
- 모듈 프리뷰 씬

────────────────────────────
2. 빠른 시작 안내
────────────────────────────

시작 방법:

1. Unity URP 프로젝트에서 패키지를 엽니다.
2. Scene_Exterior를 열어 완성형 공장 단지 샘플 씬을 확인합니다.
3. Scene_Module을 열어 포함된 모듈형 에셋을 카테고리별로 확인합니다.
4. Prefabs 폴더에서 필요한 프리팹을 원하는 씬에 배치합니다.
5. 에셋 종류와 레이아웃에 따라 그리드 스냅, 버텍스 스냅, 피벗 기준 정렬 또는 수동 배치를 사용합니다.
6. Static 오브젝트, 조명, 리플렉션 프로브 등을 수정한 경우 필요에 따라 라이팅을 다시 베이크합니다.

처음 확인하기 좋은 씬:
- Scene_Exterior

에셋 확인용 추천 씬:
- Scene_Module

────────────────────────────
3. 데모 씬
────────────────────────────

1. Scene_Exterior

Scene_Exterior는 메인 샘플 씬입니다.

해당 씬은 3개의 공장 건물로 구성된 완성형 공장 단지 레이아웃을 포함합니다.

- Factory_A: 대형 공장
- Factory_B: 중형 공장
- Factory_C: 소형 공장

각 공장은 외부와 내부 공간을 모두 포함합니다.

Factory_A와 Factory_B에는 내부 공간 구성과 프랍 배치가 완료되어 있습니다.
Factory_C에는 내부 공간 레이아웃이 구성되어 있으나, 세부 내부 프랍은 배치되어 있지 않습니다.

Scene_Exterior 포함 항목:
- 공장 외부 및 내부 레이아웃
- 도로 및 주차 구역
- 하역 구역
- 펜스 및 게이트
- 컨테이너 및 저장 탱크
- 안전 프랍 및 데칼
- 기본 조명 세팅
- 리플렉션 프로브
- 글로벌 볼륨 세팅
- 베이크 라이팅

2. Scene_Module

Scene_Module은 모듈 프리뷰 씬입니다.

해당 씬은 포함된 에셋을 카테고리별로 나열하여, 사용자가 모듈형 파츠, 프랍, 데칼, 바리에이션을 빠르게 확인할 수 있도록 구성되어 있습니다.

Scene_Module은 에셋 확인용 씬이며, Scene_Exterior와 같은 완성형 환경 세팅은 포함되어 있지 않습니다.

────────────────────────────
4. 주요 에셋 카테고리
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

Safety Equipment 포함 예시:
- Fire Extinguisher
- Hydrant Box
- First Aid Kit Box
- Safety Mirror
- Emergency Exit Props

Traffic Control 포함 예시:
- Bollards
- Parking Barriers
- Safety Cones
- Traffic-related site props

Decals 포함 예시:
- Road Markings
- Parking Lines
- Hazard Areas
- Safety Signs
- Zone ID Markings
- Direction Arrows

────────────────────────────
5. 모듈형 건물 시스템
────────────────────────────

대부분의 건물 에셋은 모듈형 프리팹으로 제공됩니다.

벽 패널, 지붕 패널, 그라운드 패널, 구조물, 계단, 파이프, 덕트, 케이블 트레이, 펜스, 데칼은 필요한 레이아웃에 따라 조합, 반복 배치 또는 정렬할 수 있도록 제작되었습니다.

에셋 종류에 따라 그리드 스냅, 버텍스 스냅, 피벗 기준 정렬 또는 수동 배치를 사용하여 배치할 수 있습니다.

일부 벽 및 구조물 모듈은 더 쉽게 배치할 수 있도록 사전 조립된 세트 프리팹으로도 제공됩니다.

일반적인 네이밍 안내:

- 2P
  2개 파츠로 구성된 벽 또는 구조물 세트입니다.

- 3P
  3개 파츠로 구성된 벽 또는 구조물 세트입니다.

- Standard
  오버헤드 크레인용 레일 지지 구조가 없는 기본 구조 타입입니다.

- CraneReady
  오버헤드 크레인 배치를 고려한 구조 타입입니다.

- Flat
  평지붕 타입입니다.

- Gable
  박공지붕 타입입니다.

예시:
PR06_Building_Structure_Wall_Flat_3P_CraneReady_Default

의미:
- 평지붕 타입의 벽 구조물
- 3개 파츠 세트
- 크레인 배치 대응 타입
- 기본 바리에이션

────────────────────────────
6. 머티리얼 및 텍스처
────────────────────────────

본 패키지는 URP용 머티리얼을 사용합니다.

텍스처 정보:
- Texture Resolution: 2048
- Texture Maps: Albedo / MAOG / NRM

텍스처 맵 안내:
- Albedo: 기본 컬러 텍스처
- MAOG: 머티리얼 채널 정보가 패킹된 텍스처
- NRM: 노멀맵

────────────────────────────
7. 기술 사양
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
8. 조명 및 환경 세팅
────────────────────────────

Scene_Exterior 포함 항목:
- 기본 조명 세팅
- 리플렉션 프로브
- 글로벌 볼륨 세팅
- 베이크 라이팅

Scene_Module은 에셋 프리뷰용 씬이며, 완성형 환경 세팅은 포함되어 있지 않습니다.

포함된 씬은 Unity 기본 스카이박스를 사용합니다.

Unity 버전, URP 설정, 그래픽스 설정, 라이팅 설정에 따라 조명 및 베이크 결과가 다르게 보일 수 있습니다.

다른 Unity 버전으로 패키지를 불러온 뒤 씬의 조명이 기존과 다르게 보이는 경우, 라이팅을 다시 베이크하는 것을 권장합니다.

Scene_Exterior에서 조명, 리플렉션 프로브, Static 오브젝트 등을 수정할 경우에도 라이팅을 다시 베이크해야 할 수 있습니다.

기본 라이팅 확인 순서:
1. Scene_Exterior를 엽니다.
2. Lighting 창을 확인합니다.
3. 씬 조명, 리플렉션 프로브, 글로벌 볼륨 세팅을 확인합니다.
4. 다른 Unity 버전에서 씬을 열었을 때 라이팅이 정상적으로 보이지 않는 경우, 필요에 따라 기존 라이팅 데이터를 삭제합니다.
5. Lighting 창에서 베이크 라이팅을 다시 빌드합니다.

────────────────────────────
9. 콜라이더, LOD, 애니메이션
────────────────────────────

콜라이더는 포함되어 있지 않습니다.
프로젝트 용도에 맞게 필요한 콜라이더를 추가해 사용해 주세요.

LOD는 포함되어 있지 않습니다.

애니메이션은 포함되어 있지 않습니다.
오버헤드 크레인, 문, 셔터, 주차 차단기와 같은 오브젝트는 별도 표기가 없는 한 정적 에셋으로 제공됩니다.

────────────────────────────
10. 스크립트 및 코드
────────────────────────────

본 패키지에 포함된 에셋을 사용하는 데 별도의 커스텀 스크립트는 필요하지 않습니다.

패키지는 주로 아래 항목으로 구성되어 있습니다.
- Static Mesh
- Prefab
- Material
- Texture
- Decal
- Demo Scene

포함된 데모 씬에는 별도의 커스텀 런타임 시스템이 필요하지 않습니다.

상호작용, 충돌 처리, 게임플레이 로직, 애니메이션, 커스텀 동작이 필요한 경우 프로젝트 요구사항에 맞게 별도의 스크립트를 추가해 사용해 주세요.

────────────────────────────
11. 서드파티 고지
────────────────────────────

홍보용 스크린샷은 Poly Haven HDRI를 사용하여 제작되었습니다.

Kloofendal 48d Partly Cloudy Pure Sky

홍보용 스크린샷에 사용된 파일:
kloofendal_48d_partly_cloudy_puresky_2k

Source:
https://polyhaven.com/a/kloofendal_48d_partly_cloudy_puresky

License:
CC0

해당 HDRI는 본 패키지에 포함되어 있지 않습니다.

포함된 데모 씬은 Unity 기본 스카이박스를 사용합니다.

────────────────────────────
12. 참고 사항
────────────────────────────

- 일부 에셋은 여러 바리에이션을 포함합니다.
- 일부 모듈형 파츠는 개별 프리팹과 사전 조립된 세트 프리팹으로 함께 제공됩니다.
- Scene_Exterior는 샘플 레이아웃으로 제공되며 자유롭게 수정할 수 있습니다.
- Scene_Module은 포함된 에셋을 확인하기 위한 프리뷰 씬입니다.
- 콜라이더, LOD, 애니메이션은 프로젝트 필요에 따라 별도로 추가해 사용해 주세요.
- 다른 Unity 버전에서 씬을 열거나 수정하는 경우, 라이팅을 다시 베이크해야 할 수 있습니다.
- 일부 모듈은 반복 배치를 고려해 제작되었으며, 레이아웃에 따라 버텍스 스냅, 피벗 기준 정렬 또는 수동 조정이 필요할 수 있습니다.