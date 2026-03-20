using System.ComponentModel.DataAnnotations;

namespace Homework.EfCoreInAction.Entities;

public class Book
{
    public int Id { get; set; }
    [MaxLength(50)] 
    public string Title { get; set; } = null!;
    [MaxLength(100)] 
    public string Description { get; set; } = null!;
}