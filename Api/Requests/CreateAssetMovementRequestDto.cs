using System.ComponentModel.DataAnnotations;

namespace AssetManagement.Web.Api.Requests;

public class CreateAssetMovementRequestDto
{
    [Required]
    public int AssetId { get; set; }

    [Required]
    public required string MovementType { get; set; } // Transfer, Repair, Disposal

    public bool ApprovalRequired { get; set; } = true;

    [Required]
    public required string SourceLocation { get; set; }

    [Required]
    public required string DestinationLocation { get; set; }

    [Required]
    public required string CurrentCustodian { get; set; }
    public string? CurrentCustodianId { get; set; }
    public string? CurrentCustodianEmail { get; set; }

    public string? NewCustodian { get; set; }
    public string? NewCustodianId { get; set; }
    public string? NewCustodianEmail { get; set; }

    public DateTime? ExpectedReturnDate { get; set; }
    public string? Comments { get; set; }

    // Only relevant when MovementType == Repair
    public RepairRecordRequestDto? RepairRecord { get; set; }

    // Only relevant when MovementType == Disposal
    public DisposalRecordRequestDto? DisposalRecord { get; set; }
}

public class RepairRecordRequestDto
{
    public string? ServiceProvider { get; set; }
    public string? IssueDescription { get; set; }
    public decimal? RepairCost { get; set; }
}

public class DisposalRecordRequestDto
{
    public string? DisposalMethod { get; set; }
    public string? ReasonForDisposal { get; set; }
}
