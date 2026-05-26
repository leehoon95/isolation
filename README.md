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

### 핵심 구현 사항

|기술|목적|
|-|-|
|Unity|
|네트워크 동기화|Unity NGO|
|Relay|P2P 연결|
|Lobby|플레이|

### 참고 소스 코드
* AudioContainer: Addressasble 에셋 다운로드, 캐싱, 오디오 플레이 기능 제공
    [AudioContainer.cs](./Assets/2.%20Sources/Audio/AudioContainer.cs)

* 투사체 오브젝트를 