using System;
using BookingTravel.Domain.Enums;

namespace BookingTravel.Domain.Entities
{
    public class Payment : BaseEntity
    {
        public int BookingId { get; set; }
        public Booking Booking { get; set; } = null!;

        public PaymentMethod Method { get; set; } = PaymentMethod.VNPay;
        public string? TransactionId { get; set; }
        public decimal Amount { get; set; }
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
        public DateTime? PaymentDate { get; set; }
    }
}
