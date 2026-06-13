namespace CarRental.Services
{
    public interface ISystemClock
    {
        DateTime Now { get; }
    }

    // Zwykły zegar używany na produkcji
    public class SystemClock : ISystemClock
    {
        public DateTime Now => DateTime.Now;
    }

    // "Zgwałcony" zegar do testów, w którym możesz ręcznie przestawić wskazówki!
    public class TestableClock : ISystemClock
    {
        private DateTime? _customTime;

        public DateTime Now => _customTime ?? DateTime.Now;

        public void SetTime(DateTime fakeTime)
        {
            _customTime = fakeTime;
        }

        public void Reset()
        {
            _customTime = null;
        }
    }
}
