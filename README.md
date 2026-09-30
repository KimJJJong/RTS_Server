[한국어](README.md) | [日本語](README.ja.md) | [English](README.en.md)

# WAR'LD Painting 게임 서버 / WAR'LD Painting ゲームサーバー

WAR'LD Painting의 1:1 실시간 PvP를 지원하는 **.NET 8 기반 게임 서버**입니다. ASP.NET Core Match API와 C# TCP GameServer를 분리하고, `SocketAsyncEventArgs` 기반 비동기 세션·2-byte length-prefix framing·결과 Tick 패킷·JSON 로그 파이프라인을 구현했습니다.

## 기술 스택

- **언어 / 런타임**: C#, .NET 8
- **API**: ASP.NET Core Match API
- **실시간 통신**: TCP/IP, `SocketAsyncEventArgs`
- **패킷**: 2-byte length-prefix framing, XML PDL 기반 코드 생성
- **테스트 클라이언트**: .NET 8 Console DummyClient
- **로그**: JSON log, Filebeat, Elasticsearch, Kibana
- **게임 클라이언트**: Unity (팀 저장소 비공개)

> 이 저장소의 `Server`, `HttpApi`, `DummyClient`, `ServerCore`, `Shared`, `PacketGenerator` 프로젝트는 모두 `net8.0`을 대상으로 합니다.

## 구성

```text
Client
  ├─ HTTP match request/status ──> ASP.NET Core Match API
  └─ TCP room/gameplay packets ──> C# TCP GameServer

PacketGenerator ── PDL.xml ──> packet classes / dispatchers
GameServer ── JSON log ──> Filebeat ──> Elasticsearch ──> Kibana
```

- `HttpApi`: 매칭 요청·취소·상태 조회와 GameServer room 생성 요청
- `Server`: TCP session, room 입장, 전투 요청 검증과 33ms 논리 Tick 기반 결과 패킷 scheduling
- `ServerCore`: listener, session, receive/send buffer, send queue
- `PacketGenerator`: `PDL.xml` 기반 packet class·manager 생성
- `DummyClient`: 서버 통신 검증용 .NET 8 클라이언트
- `Shared`: 공통 계약과 JSON logging

## 동기화 범위

서버는 OID 풀, 좌표 변환, HP, 점령도와 승패 등 일부 결과 상태를 처리하고, 클라이언트는 전달받은 기준값으로 연출을 실행하도록 분리했습니다. 여기서 33ms는 Room 전체를 30Hz로 시뮬레이션한다는 뜻이 아니라, 결과 패킷의 적용 시점을 맞추기 위한 논리 Tick 간격입니다.

## 빌드

```bash
dotnet build Server.sln
```

Unity Client 저장소는 팀 자산으로 비공개입니다. 공개 포트폴리오에는 네트워크 처리와 Result Tick scheduling을 설명하는 최소 코드 발췌만 사용합니다.
