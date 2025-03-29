//using System.Collections.Generic;
//using System;

//public enum CommandType
//{
//    Summon = 1,
//    // Future commands...
//}

//public interface ICommand
//{
//    int SessionId { get; }
//    CommandType Type { get; }

//}


//public class CommandScheduler
//{
//    private Dictionary<int, List<ICommand>> _scheduledCommands = new Dictionary<int, List<ICommand>>();
//    private List<(int tick, ICommand command)> _executedHistory = new List<(int, ICommand)>();
//    //P
//    private Dictionary<int, List<IPacket>> _scheduledCommandsP = new Dictionary<int, List<IPacket>>();
//    private List<(int tick, IPacket command)> _executedHistoryP = new List<(int, IPacket)>();

//    public void Schedule(int tick, ICommand command)
//    {
//        if (_scheduledCommands.ContainsKey(tick) == false)
//        {
//            _scheduledCommands[tick] = new List<ICommand>();
//            Console.WriteLine($"Tick :{tick} Add Command : {command}" );
//        }

//        _scheduledCommands[tick].Add(command);
//    }
//    //P
//    public void Schedule(int tick, C_ReqSummon packet)
//    {
//        if (_scheduledCommandsP.ContainsKey(tick) == false)
//        {
//            _scheduledCommandsP[tick] = new List<IPacket>();
//            Console.WriteLine($"Tick :{tick} Add Command : {packet}");
//        }

//        _scheduledCommandsP[tick].Add(packet);
//    }

//    //public List<ICommand> GetCommandsForTick(int tick)
//    //{
//    //    if (_scheduledCommands.TryGetValue(tick, out var cmds))
//    //    {
//    //        _scheduledCommands.Remove(tick);
//    //        return cmds;
//    //    }

//    //    return new List<ICommand>();
//    //}
//    //P
//    public List<IPacket> GetCommandsForTick(int tick)
//    {
//        if (_scheduledCommandsP.TryGetValue(tick, out var packets))
//        {
//            _scheduledCommandsP.Remove(tick);
//            return packets;
//        }

//        return new List<IPacket>();
//    }

//    //public void RecordExecuted(int tick, ICommand command)
//    //{
//    //    _executedHistory.Add((tick, command));
//    //    // 필요 시 로그 출력
//    //    Console.WriteLine($"[Tick {tick}] Executed {command.Type} by {command.SessionId}");
//    //    Console.WriteLine("===================================================");

//    //}

//    //P
//    public void RecordExecuted(int tick, C_ReqSummon packet)
//    {
//        _executedHistoryP.Add((tick, packet));
//        // 필요 시 로그 출력
//        Console.WriteLine($"[Tick {tick}] Executed [C_ReqSummon] by {packet.reqSessionID}");
//        Console.WriteLine("===================================================");

//    }

//    public IReadOnlyList<(int tick, ICommand command)> ExecutedCommands => _executedHistory;

//    //P
//    public IReadOnlyList<(int tick, IPacket command)> ExecutedCommandsP => _executedHistoryP;

//}


//public static class CommandFactory
//{
//    public static ICommand FromPacket(IPacket packet)
//    {
//        switch (packet)
//        {
//            case C_ReqSummon summon:
//                return new SummonCommand(summon);
//            // case C_ReqMove move: return new MoveCommand(move);
//            default:
//                throw new InvalidOperationException($"Unknown packet type: {packet.GetType()}");
//        }
//    }
//}


///// <summary>
///// Command Class
///// </summary>




//public class SummonCommand : ICommand
//{
//    public int SessionId { get; private set; }
//    public float x, y;
//    public int oid;
//    public int needMana;

//    public CommandType Type => CommandType.Summon;


//    public SummonCommand(C_ReqSummon c_AnsSummon)
//    {
//        SessionId = c_AnsSummon.reqSessionID;
//        this.x = c_AnsSummon.x;
//        this.y = c_AnsSummon.y;
//        this.oid = c_AnsSummon.oid;
//        this.needMana = c_AnsSummon.reqSessionID;
//    }
//}