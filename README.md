
# RTS_Server

**RTS_Server**는 실시간 전략 게임의 서버 아키텍처를 구현한 프로젝트로, .NET Core 3.1을 기반으로 TCP/IP 통신을 활용하여 클라이언트와의 실시간 동기화를 지원합니다. 이 프로젝트는 게임 서버의 핵심 기능인 명령 동기화, 인증 처리, 로그 모니터링 등을 포함하고 있습니다.

## 📌 프로젝트 개요

- **목표**: 실시간 전략 게임에서의 안정적인 서버-클라이언트 통신 및 동기화 구현
- **주요 기능**:
  - Tick 기반의 명령 동기화 시스템
  - 클라이언트 인증 처리
  - 실시간 로그 수집 및 모니터링
  - 멀티 클라이언트 지원

## 🛠️ 기술 스택

- **언어**: C#
- **프레임워크**: .NET Core 3.1
- **통신**: TCP/IP (비동기 소켓)
- **인증**: Firebase Auth, Node.js 기반 API 서버
- **로그 모니터링**: Filebeat, Elasticsearch, Kibana
- **배포 환경**: ( 예정 )AWS EC2
- **기타**: Unity (클라이언트), Windows Forms (테스트 클라이언트)

## 📂 디렉토리 구조

```
RTS_Server/
├── Common/Packet/           # 패킷 정의 및 관련 클래스
├── DummyClient/             # 테스트용 클라이언트 구현
├── PacketGenerator/         # 패킷 자동 생성 도구
├── Server/                  # 서버 실행 파일 및 설정
├── ServerCore/              # 서버의 핵심 로직 (TickManager 등)
├── Shared/                  # 공통 유틸리티 및 상수
├── Server.sln               # 솔루션 파일
└── .gitignore               # Git 무시 파일 설정
```

## 🚀 설치 및 실행 방법

1. **레포지토리 클론**:
   ```bash
   git clone https://github.com/KimJJJong/RTS_Server.git
   cd RTS_Server
   ```

2. **패킷 생성기 실행**:
   - `PacketGenerator` 프로젝트를 빌드 및 실행하여 필요한 패킷 코드를 생성합니다.

3. **서버 실행**:
   - `Server` 프로젝트를 빌드 및 실행하여 서버를 시작합니다.

4. **테스트 클라이언트 실행**:
   - `DummyClient` 프로젝트를 빌드 및 실행하여 서버와의 통신을 테스트합니다.

## ✅ 주요 기능

- **Tick 기반 명령 동기화**:
  - 서버는 일정한 Tick 간격으로 클라이언트의 명령을 수집하고, 이를 동기화하여 일관된 게임 상태를 유지합니다.

- **클라이언트 인증 처리**:
  - Firebase Auth와 Node.js 기반의 API 서버를 통해 클라이언트의 인증을 처리합니다.

- **실시간 로그 모니터링**:
  - Filebeat를 통해 서버 로그를 수집하고, Elasticsearch와 Kibana를 활용하여 실시간으로 모니터링합니다.

