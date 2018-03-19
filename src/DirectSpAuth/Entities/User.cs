using System;

namespace DirectSpAuth.Entities
{
    public class User
    {
        public string UserId { get; set; }
        public bool IsEnabled { get; set; }
        public string UserName { get; set; }
        public string UserDisplayName { get; set; }
        public string NationalNumber { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Gender { get; set; }
        public DateTime? Birthdate { get; set; }
        public string AddressProvinceName { get; set; }
        public string AddressCityName { get; set; }
        public string AddressStreet { get; set; }
        public string AddressPostalCode { get; set; }
        public bool IsPasswordMustBeChanged { get; set; }
    }
}
