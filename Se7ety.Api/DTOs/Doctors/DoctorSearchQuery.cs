using Se7ety.Api.DTOs.Common;
using System.ComponentModel.DataAnnotations;

namespace Se7ety.Api.DTOs.Doctors;

public sealed class DoctorSearchQuery : PaginationQuery
{
    [MaxLength(100)]
    public string? Query { get; set; }
}
