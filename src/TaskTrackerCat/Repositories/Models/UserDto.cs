﻿namespace TaskTrackerCat.Repositories.Models;

public class UserDto
{
    public int Id { get; set; }
    public string Email { get; set; }
    public string Name { get; set; }
    public string Password { get; set; }
    public int NativeGroupId { get; set; }
    public int CurrentGroupId { get; set; }
}