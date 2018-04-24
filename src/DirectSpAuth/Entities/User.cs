using Newtonsoft.Json;
using System;

namespace DirectSp.AuthServer.Entities
{
    public enum Gender {
        Male = 1,
        Female = 2,
    }

    public class User
    {
        public string UserId { get; set; }
        public bool IsEnabled { get; set; }
        public string UserName { get; set; }
        public string UserDisplayName { get; set; }
        public string NationalNumber { get; set; }
        public string Email { get; set; }
        public string MobileNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Gender? GenderId { get; set; }
        public DateTime? Birthdate { get; set; }
        public string AddressProvinceName { get; set; }
        public string AddressCityName { get; set; }
        public string AddressStreet { get; set; }
        public string AddressPostalCode { get; set; }
        public bool IsPasswordMustBeChanged { get; set; }
    }
}
