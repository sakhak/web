using System.ComponentModel.DataAnnotations;

namespace AssetManagement.Web.Models;

public class ApprovalModel
{
    public class DisposalApproval
    {
        [Required]
        public int Id { get; set; }
        [Required, StringLength(1)]
        public string Approved { get; set; } = "Approved";
        [Required, StringLength(500)]
        public string? Reason { get; set; }
    }

    public class SendToApprove
    {
        [Required]
        public int Id { get; set; }
        [Required, StringLength(1)]
        public string? Authorizer { get; set; }
        [Required, StringLength(500)]
        public string? Reason { get; set; }
    }
}
