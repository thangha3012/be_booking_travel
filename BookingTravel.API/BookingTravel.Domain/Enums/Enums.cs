using System;

namespace BookingTravel.Domain.Enums
{
    public enum Role
    {
        Admin = 1,
        Staff = 2,
        Customer = 3
    }

    public enum TourStatus
    {
        Draft = 1,
        Published = 2,
        Archived = 3
    }

    public enum BookingStatus
    {
        Pending = 1,
        AwaitingPayment = 2,
        Confirmed = 3,
        Cancelled = 4,
        Completed = 5
    }

    public enum PaymentMethod
    {
        VNPay = 1,
        BankTransfer = 2,
        COD = 3
    }

    public enum PaymentStatus
    {
        Pending = 1,
        Success = 2,
        Failed = 3
    }

    public enum PassengerType
    {
        Adult = 1,
        Child = 2,
        Infant = 3
    }

    public enum ReviewStatus
    {
        Pending = 1,
        Approved = 2,
        Rejected = 3
    }
}
