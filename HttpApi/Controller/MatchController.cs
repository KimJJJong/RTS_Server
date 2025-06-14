using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;

[ApiController]
[Route("match")]
public class MatchController : ControllerBase
{
    private readonly MatchQueue _queue;
    private readonly RoomMapping _mapping;

    public MatchController(MatchQueue queue, RoomMapping mapping)
    {
        _queue = queue;
        _mapping = mapping;
    }

    [HttpPost("request")]
    public IActionResult RequestMatch([FromBody] MatchPlayerInfo request)
    {
        if (_queue.Enqueue(request))
        {
           Console.WriteLine($"{request.UserId} Join Matching");
           return Conflict("Enqueued");
        }

        Console.WriteLine($"{request.UserId} already Join");
        return Ok("이미 돌리고 있다능");

    }

    [HttpPost("status")]
    public IActionResult MatchStatus([FromBody] MatchPlayerInfo request)
    {
        var room = _mapping.GetRoomInfo(request.UserId);
        if (room == null)
            return Ok("Waiting");

        return Ok(room);
    }

    [HttpPost("cancel")]
    public IActionResult CancelMatch([FromBody] MatchPlayerInfo request)
    {
        if (!_queue.Cancel(request.UserId))
        {
            return NotFound("대기열에 존재하지 않습니다.");
        }

        Console.WriteLine($"player : {request.UserId} is Cancel Matching");
        return Ok("매칭 요청 취소");
    }



}

public class MatchRequest
{
    public string UserId { get; set; }
    public List<CardInfo> Deck {  get; set; }
}
