This is an ASP.NET Core 8 Web API solution representing a fake cinema application.
It is designed to manage the showtimes of the cinema, getting some data from an external API.

The following features are implemented:

- Create showtimes.
- Reserve seats.
- Buy seats.

These features are covered by unit tests.


## Commands:

- **Create showtime**
    
    Should create showtime and should grab the movie data from an external API.
    
- **Reserve seats**
    - Reserving the seat response will contain a GUID of the reservation, also the number of seats, the auditorium used and the movie that will be played.
    - After 10 minutes after the reservation is created, the reservation is considered expired by the system.
    - It should not be possible to reserve the same seats two times in 10 minutes.
    - It shouldn't be possible to reserve an already sold seat.
    - All the seats, when doing a reservation, need to be contiguous.
- **Buy seats**
    - We will need the GUID of the reservation, it is only possible to do it while the seats are reserved.
    - It is not possible to buy the same seat two times.
    - Expired reservations (older than 10 minutes) cannot be confirmed.

### Caching and resilience pipelines

Caching and resilience are implemented as decorators on top of a faulty movies provider
