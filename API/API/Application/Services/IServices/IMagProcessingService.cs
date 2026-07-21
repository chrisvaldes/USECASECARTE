using API.Application.DTO;
using API.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace API.Application.Services.IServices
{

    public interface IMagProcessingService
    {
        Task<ApiResponse<bool>> ProcessTxtExcelFiles([FromForm] InputModel inputModel); 
    }
}