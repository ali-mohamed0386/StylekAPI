using Se7ety.Api.DTOs.Common;
using System.ComponentModel.DataAnnotations;

namespace Se7ety.Api.DTOs.Doctors;

public sealed class DoctorListQuery : PaginationQuery
{
    [MaxLength(100)]
    public string? Specialty { get; set; }
}
