namespace Badminton_BE.DTOs
{
    public class MemberMatchStatsDto
    {
        public int Wins { get; init; }
        public int Losses { get; init; }
        public int Draws { get; init; }
        public decimal WinRate { get; init; }
    }
}
