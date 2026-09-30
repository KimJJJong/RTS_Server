[한국어](README.md) | [日本語](README.ja.md) | [English](README.en.md)

# WAR'LD Painting ゲームサーバー

WAR'LD Paintingの1対1リアルタイムPvPを支える、**.NET 8ベースのゲームサーバー**です。ASP.NET Core Match APIとC# TCP GameServerを分離し、`SocketAsyncEventArgs`ベースの非同期セッション、2-byte length-prefix framing、結果Tickパケット、JSONログパイプラインを実装しました。

## 技術スタック

- **言語 / ランタイム**: C#、.NET 8
- **API**: ASP.NET Core Match API
- **リアルタイム通信**: TCP/IP、`SocketAsyncEventArgs`
- **パケット**: 2-byte length-prefix framing、XML PDLベースのコード生成
- **テストクライアント**: .NET 8 Console DummyClient
- **ログ**: JSON log、Filebeat、Elasticsearch、Kibana
- **ゲームクライアント**: Unity（チームのリポジトリは非公開）

> このリポジトリの`Server`、`HttpApi`、`DummyClient`、`ServerCore`、`Shared`、`PacketGenerator`プロジェクトは、すべて`net8.0`を対象としています。

## 構成

```text
Client
  ├─ HTTP match request/status ──> ASP.NET Core Match API
  └─ TCP room/gameplay packets ──> C# TCP GameServer

PacketGenerator ── PDL.xml ──> packet classes / dispatchers
GameServer ── JSON log ──> Filebeat ──> Elasticsearch ──> Kibana
```

- `HttpApi`: マッチングのリクエスト、キャンセル、状態照会、およびGameServerへのRoom作成リクエスト
- `Server`: TCP session、Roomへの参加、戦闘リクエストの検証、および33msの論理Tickに基づく結果パケットのscheduling
- `ServerCore`: listener、session、receive/send buffer、send queue
- `PacketGenerator`: `PDL.xml`に基づくpacket classとmanagerの生成
- `DummyClient`: サーバー通信を検証するための.NET 8クライアント
- `Shared`: 共通契約とJSON logging

## 同期範囲

サーバーはOIDプール、座標変換、HP、占領度、勝敗など一部の結果状態を処理し、クライアントは受け取った基準値に基づいて演出を実行するように分離しました。ここでいう33msは、Room全体を30Hzでシミュレーションするという意味ではなく、結果パケットの適用タイミングを揃えるための論理Tick間隔です。

## ビルド

```bash
dotnet build Server.sln
```

Unity Clientのリポジトリはチーム資産のため非公開です。公開ポートフォリオでは、ネットワーク処理とResult Tick schedulingを説明するための最小限のコード抜粋のみを使用しています。
