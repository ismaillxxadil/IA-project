using System;

namespace SmartRentApi.Models
{
    public enum Role
    {
        Admin,
        Landlord,
        Tenant
    }

    public enum AccountStatus
    {
        Pending,
        Approved,
        Rejected
    }

    public enum PropertyType
    {
        Apartment,
        Villa,
        SharedRoom
    }

    public enum PropertyStatus
    {
        Pending,
        Available,
        Rented
    }

    public enum VisitRequestStatus
    {
        Pending,
        Accepted,
        Rejected
    }

    public enum ApplicationStatus
    {
        Pending,
        Accepted,
        Rejected
    }
}
