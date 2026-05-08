using apbd_proj03.data;
using apbd_proj03.models;

namespace apbd_proj03.controllers;

using Microsoft.AspNetCore.Mvc;
[ApiController]
[Route("api/[controller]")]
public class ReservationsController : ControllerBase
{
    [HttpGet]
    public ActionResult<IEnumerable<Reservation>> GetAll(
        [FromQuery] DateOnly? date,
        [FromQuery] string? status,
        [FromQuery] int? roomId)
    {
        var reservations = AppData.Reservations.AsEnumerable();

        if (date.HasValue)
        {
            reservations = reservations.Where(reservation => reservation.Date == date.Value);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            reservations = reservations.Where(reservation =>
                reservation.Status.Equals(status, StringComparison.OrdinalIgnoreCase));
        }

        if (roomId.HasValue)
        {
            reservations = reservations.Where(reservation => reservation.RoomId == roomId.Value);
        }

        return Ok(reservations);
    }

    [HttpGet("{id:int}")]
    public ActionResult<Reservation> GetById([FromRoute] int id)
    {
        var reservation = AppData.Reservations.FirstOrDefault(reservation => reservation.Id == id);

        if (reservation is null)
        {
            return NotFound();
        }

        return Ok(reservation);
    }

    [HttpPost]
    public ActionResult<Reservation> Create([FromBody] Reservation reservation)
    {
        var room = AppData.Rooms.FirstOrDefault(room => room.Id == reservation.RoomId);

        if (room is null)
        {
            return BadRequest("Room does not exist.");
        }

        if (!room.IsActive)
        {
            return BadRequest("Room is not active.");
        }

        if (HasTimeConflict(reservation))
        {
            return Conflict("Reservation time conflicts with another reservation for the same room.");
        }

        reservation.Id = AppData.NextReservationId();

        AppData.Reservations.Add(reservation);

        return CreatedAtAction(nameof(GetById), new { id = reservation.Id }, reservation);
    }

    [HttpPut("{id:int}")]
    public ActionResult<Reservation> Update([FromRoute] int id, [FromBody] Reservation updatedReservation)
    {
        var reservation = AppData.Reservations.FirstOrDefault(reservation => reservation.Id == id);

        if (reservation is null)
        {
            return NotFound();
        }

        var room = AppData.Rooms.FirstOrDefault(room => room.Id == updatedReservation.RoomId);

        if (room is null)
        {
            return BadRequest("Room does not exist.");
        }

        if (!room.IsActive)
        {
            return BadRequest("Room is not active.");
        }

        if (HasTimeConflict(updatedReservation, id))
        {
            return Conflict("Reservation time conflicts with another reservation for the same room.");
        }

        reservation.RoomId = updatedReservation.RoomId;
        reservation.OrganizerName = updatedReservation.OrganizerName;
        reservation.Topic = updatedReservation.Topic;
        reservation.Date = updatedReservation.Date;
        reservation.StartTime = updatedReservation.StartTime;
        reservation.EndTime = updatedReservation.EndTime;
        reservation.Status = updatedReservation.Status;

        return Ok(reservation);
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete([FromRoute] int id)
    {
        var reservation = AppData.Reservations.FirstOrDefault(reservation => reservation.Id == id);

        if (reservation is null)
        {
            return NotFound();
        }

        AppData.Reservations.Remove(reservation);

        return NoContent();
    }

    private static bool HasTimeConflict(Reservation reservation, int? ignoredReservationId = null)
    {
        return AppData.Reservations.Any(existing =>
            existing.Id != ignoredReservationId &&
            existing.RoomId == reservation.RoomId &&
            existing.Date == reservation.Date &&
            reservation.StartTime < existing.EndTime &&
            reservation.EndTime > existing.StartTime &&
            !existing.Status.Equals("cancelled", StringComparison.OrdinalIgnoreCase));
    }
}