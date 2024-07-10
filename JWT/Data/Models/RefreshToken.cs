namespace JWT.Data.Models
{
	/*
	[Owned]
	// Complex Attribute [Multivalued + Composite]
	// Will be mapped into new table with generated col: Id
	// (pk => Id + ApplicationUserId)
	*/
	public class RefreshToken
	{
		public int Id { get; set; }
		public string Token { get; set; }
		public DateTime ExpiresOn { get; set; }
		public bool IsExpired => DateTime.UtcNow >= ExpiresOn;
		public DateTime CreatedOn { get; set; }
		public DateTime? RevokedOn { get; set; }
		public bool IsActive => RevokedOn is null && !IsExpired;
	}
}
