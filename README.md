# Isolation 
익스트랙션 슈팅게임  

### 프로젝트 요약
|||
|-|-|
|개발 시간|2025.10~2026.05|
|개발 인원|1|
|사용 엔진|Unity 6.0.68f1 LTS|
|플랫폼|Windows|
|사용 기술 및 패키지|Unity NGO, Relay, Lobby, Addressables, UI Soft Mask, NavMeshPlus|

### 참고 소스 코드와 기능
* 아이템 스포너: 기본적으로 네트워크 동기화 오브젝트를 스폰하는 스포너  
    [ItemSpawner.cs](./Assets/2.%20Sources/Spawner/ItemSpawner.cs)
* 투사체 스포너: 실제 네트워크 스폰이 아니고 로컬에만 실체, 다른 클라이언트는 허상을 스폰  
    [PooledDynamicSpawner.cs](./Assets/2.%20Sources/Spawner/PooledDynamicSpawner.cs)  
* Enemy와 기본 클래스: Enemy의 기본 동작과 Collision Event를 네트워크 오브젝트와 로컬(실체) 오브젝트간 충돌 처리  
    [SuicideBomberEnemy.cs](./Assets/2.%20Sources/Enemy/SuicideBomberEnemy.cs)  
    [EnemyBase.cs](./Assets/2.%20Sources/Enemy/EnemyBase.cs)  
* AudioContainer: Addressasble 에셋 다운로드, 캐싱, 오디오 플레이 기능  
    [AudioContainer.cs](./Assets/2.%20Sources/Audio/AudioContainer.cs)  

