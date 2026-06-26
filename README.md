# Isolation 
익스트랙션 슈팅게임  

### 프로젝트 요약
|||
|-|-|
|개발 시간|2025.10~2026.06|
|개발 인원|1|
|사용 엔진|Unity 6.0.68f1 LTS|
|플랫폼|Windows|
|사용 기술 및 패키지|Unity NGO, Relay, Lobby, Addressables, UI Soft Mask, NavMeshPlus|

### 참고 소스 코드와 기능
* Player 스포너: NetworkPrefabInstanceHandlerWithData를 이용한 Player 스폰  
    [PlayerSpawner.cs](./Assets/2.%20Sources/Spawner/PlayerSpawner.cs)  
    [PlayaerPrefabWithDataHandler.cs](./Assets/2.%20Sources/Spawner/PlayerPrefabWithDataHandler.cs)  

* 아이템 스포너: NGO 오브젝트를 스폰하는 스포너  
    [ItemSpawner.cs](./Assets/2.%20Sources/Spawner/ItemSpawner.cs)

* Illusion 스포너: 로컬에만 실체, 다른 클라이언트는 illusion을 스폰  
    [PooledDynamicSpawner.cs](./Assets/2.%20Sources/Spawner/PooledDynamicSpawner.cs)  

* Enemy와 기본 클래스: Collision Event를 네트워크 오브젝트와 로컬(실체) 오브젝트간 충돌 처리  
    [SuicideBomberEnemy.cs](./Assets/2.%20Sources/Enemy/SuicideBomberEnemy.cs)  
    [EnemyBase.cs](./Assets/2.%20Sources/Enemy/EnemyBase.cs)  

* 오디오 시스템: Addressasbles 에셋 다운로드, 캐싱, 오디오 플레이  
    [AudioContainer.cs](./Assets/2.%20Sources/Audio/AudioContainer.cs)  
    [AudioHolder.cs](./Assets/2.%20Sources/Audio/AudioHolder.cs)  

* 잡시스템: Enemy의 타겟 추적 코드에 잡시스템과 버스트 컴파일 사용  
    [TargetSearchJob.cs](./Assets/2.%20Sources/TargetSearchJob.cs)  

* Weapon 시스템: Projectile 발사와 레이저 동기화  
    [WeaponContainer.cs](./Assets/2.%20Sources/Player/Weapon/WeaponContainer.cs)  
    [WeaponLaser.cs](./Assets/2.%20Sources/Player/Weapon/WeaponLaser.cs)  

### 오디오 소스 출처
[pixabay](https://pixabay.com/ko/sound-effects/)

### 폰트 출처
[Neo둥근모](https://github.com/neodgm/neodgm)