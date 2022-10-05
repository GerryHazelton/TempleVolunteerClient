namespace TempleVolunteerClient
{
    public class StaffRequest : Audit
    {
        public int StaffId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public int RoleId { get; set; }
        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }
        public string Gender { get; set; }
        public bool FirstAid { get; set; }
        public bool CPR { get; set; }
        public bool Kriyaban { get; set; }
        public bool LessonStudent { get; set; }
        public bool AcceptTerms { get; set; }
        public string Notes { get; set; }
        public bool CanSchedule { get; set; }
        public bool CanOrderSupplyItems { get; set; }
        public bool CanViewReports { get; set; }
        public bool CanSendMessages { get; set; }
        public string? StaffImageFileName { get; set; }
        public string? StaffImage { get; set; }
        public bool IsVerified { get; set; }
        public DateTime? VerifiedDate { get; set; }
        public bool RememberMe { get; set; }
        public bool IsLockedOut { get; set; }
        public int LoginAttempts { get; set; }
        public bool UnlockUser { get; set; }
        public string Password { get; set; }
        public string PasswordSalt { get; set; }
        public ICollection<RoleRequest> Roles { get; set; }

        public StaffRequest()
        {
            this.Roles = new HashSet<RoleRequest>();
        }
    }
}
