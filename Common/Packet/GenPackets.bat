START ../../PacketGenerator/bin/PacketGenerator.exe ../../PacketGenerator/PDL.xml
XCOPY /Y GenPackets.cs "../../DummyClient/Packet"
XCOPY /Y GenPackets.cs "C:\Users\JJONG\Documents\GitHub\2024_Capstone_FE\Assets\02.Scripts\NetWork\Packet"
XCOPY /Y GenPackets.cs "../../Server/Packet"

XCOPY /Y ClientPacketManager.cs "../../DummyClient/Packet"
XCOPY /Y ClientPacketManager.cs "C:\Users\JJONG\Documents\GitHub\2024_Capstone_FE\Assets\02.Scripts\NetWork\Packet"
XCOPY /Y ServerPacketManager.cs "../../Server/Packet"
