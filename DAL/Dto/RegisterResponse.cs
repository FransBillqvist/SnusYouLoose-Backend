using System;

namespace DAL.Dto;

public class RegisterResponse
{
    public string Message { get; set; } = string.Empty;
    public Boolean Success { get; set; }
    public string Id { get; set; } = string.Empty;
}