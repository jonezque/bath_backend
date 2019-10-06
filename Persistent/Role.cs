using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace api.Persistent
{
	public class Role : IdentityRole<int>
	{
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		[Key]
		public override int Id { get; set; }
	}
}